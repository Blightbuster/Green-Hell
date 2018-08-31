using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeluxeTonemapper : MonoBehaviour
{
	public FTDLut LutTool
	{
		get
		{
			return this.m_LutTool;
		}
	}

	public Shader TonemappingShader
	{
		get
		{
			return this.m_Shader;
		}
	}

	private void DestroyMaterial(Material mat)
	{
		if (mat)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	private void CreateMaterials()
	{
		if (this.m_Shader == null)
		{
			if (this.m_Mode == DeluxeTonemapper.Mode.Color)
			{
				this.m_Shader = Shader.Find("Hidden/Deluxe/TonemapperColor");
			}
			if (this.m_Mode == DeluxeTonemapper.Mode.Luminance)
			{
				this.m_Shader = Shader.Find("Hidden/Deluxe/TonemapperLuminosity");
			}
			if (this.m_Mode == DeluxeTonemapper.Mode.ExtendedLuminance)
			{
				this.m_Shader = Shader.Find("Hidden/Deluxe/TonemapperLuminosityExtended");
			}
			if (this.m_Mode == DeluxeTonemapper.Mode.ColorHD)
			{
				this.m_Shader = Shader.Find("Hidden/Deluxe/TonemapperColorHD");
			}
		}
		if (this.m_Material == null && this.m_Shader != null && this.m_Shader.isSupported)
		{
			this.m_Material = this.CreateMaterial(this.m_Shader);
		}
	}

	private Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
	}

	private void OnDisable()
	{
		this.DestroyMaterial(this.m_Material);
		this.m_Material = null;
		this.m_Shader = null;
	}

	public void ClearLutTextures()
	{
		if (this.m_3DLut0 != null)
		{
			UnityEngine.Object.DestroyImmediate(this.m_3DLut0);
			this.m_3DLut0 = null;
		}
		this.m_2DLut0 = null;
	}

	public void ConvertAndApply2dLut()
	{
		if (this.m_2DLut0 != null)
		{
			this.m_3DLut0 = this.LutTool.Allocate3DLut(this.m_2DLut0);
			this.LutTool.ConvertLUT(this.m_2DLut0, this.m_3DLut0);
		}
	}

	public void SetLut0(Texture2D lut0)
	{
		this.ClearLutTextures();
		this.m_2DLut0 = lut0;
		this.ConvertAndApply2dLut();
	}

	public void VerifyAndUpdateShaderVariations(bool forceUpdate)
	{
		if (this.m_Material == null)
		{
			return;
		}
		if (!SystemInfo.supports3DTextures)
		{
			this.m_Use2DLut = true;
		}
		if (this.m_DisableColorGrading != this.m_LastDisableColorGrading || forceUpdate)
		{
			this.m_Material.DisableKeyword("FTD_DISABLE_CGRADING");
			if (this.m_DisableColorGrading)
			{
				this.m_Material.EnableKeyword("FTD_DISABLE_CGRADING");
			}
		}
		if (this.m_Use2DLut != this.m_LastUse2DLut || forceUpdate)
		{
			this.m_Material.DisableKeyword("FTD_2D_LUT");
			if (this.m_Use2DLut)
			{
				this.m_Material.EnableKeyword("FTD_2D_LUT");
			}
		}
		this.m_LastUse2DLut = this.m_Use2DLut;
		this.m_LastDisableColorGrading = this.m_DisableColorGrading;
	}

	public void UpdateCoefficients()
	{
		if (this.m_Mode == DeluxeTonemapper.Mode.ColorHD)
		{
			this.m_MainCurve.m_UseLegacyCurve = false;
		}
		else
		{
			this.m_MainCurve.m_UseLegacyCurve = true;
		}
		this.m_MainCurve.UpdateCoefficients();
		if (this.m_Material == null)
		{
			return;
		}
		if (this.m_2DLut0 == null)
		{
			this.ClearLutTextures();
		}
		this.m_Material.SetFloat("_K", this.m_MainCurve.m_k);
		this.m_Material.SetFloat("_Crossover", this.m_MainCurve.m_CrossOverPoint);
		this.m_Material.SetVector("_Toe", this.m_MainCurve.m_ToeCoef);
		this.m_Material.SetVector("_Shoulder", this.m_MainCurve.m_ShoulderCoef);
		this.m_Material.SetFloat("_LuminosityWhite", this.m_MainCurve.m_LuminositySaturationPoint * this.m_MainCurve.m_LuminositySaturationPoint);
		this.m_Material.SetFloat("_Sharpness", this.m_Sharpness);
		this.m_Material.SetFloat("_Saturation", this.m_Saturation);
		if (this.m_2DLut0 != null && this.m_3DLut0 == null && this.LutTool.Validate2DLut(this.m_2DLut0))
		{
			this.ConvertAndApply2dLut();
		}
		float value = (QualitySettings.activeColorSpace != ColorSpace.Linear) ? 1f : 0.5f;
		if (this.m_Use2DLut && this.m_2DLut0 != null)
		{
			this.m_2DLut0.filterMode = FilterMode.Bilinear;
			float num = Mathf.Sqrt((float)this.m_2DLut0.width);
			this.m_Material.SetTexture("_2dLut0", this.m_2DLut0);
			this.m_Material.SetVector("_Lut0Params", new Vector4(this.m_Lut0Amount, 1f / (float)this.m_2DLut0.width, 1f / (float)this.m_2DLut0.height, num - 1f));
			this.m_Material.SetFloat("_IsLinear", value);
		}
		else if (this.m_3DLut0 != null)
		{
			int width = this.m_3DLut0.width;
			float y = (float)(width - 1) / (1f * (float)width);
			float z = 1f / (2f * (float)width);
			this.m_3DLut0.filterMode = FilterMode.Trilinear;
			this.m_Material.SetVector("_Lut0Params", new Vector4(this.m_Lut0Amount, y, z, 0f));
			this.m_Material.SetTexture("_Lut0", this.m_3DLut0);
			this.m_Material.SetFloat("_IsLinear", value);
		}
		else
		{
			this.m_Material.SetVector("_Lut0Params", Vector4.zero);
		}
	}

	public void ReloadShaders()
	{
		this.OnDisable();
	}

	public void SetCurveParameters(float range, float crossover, float toe, float shoulder)
	{
		this.SetCurveParameters(0f, range, crossover, toe, shoulder);
	}

	public void SetCurveParameters(float crossover, float toe, float shoulder)
	{
		this.SetCurveParameters(0f, 1f, crossover, toe, shoulder);
	}

	public void SetCurveParameters(float blackPoint, float whitePoint, float crossover, float toe, float shoulder)
	{
		this.m_MainCurve.m_BlackPoint = blackPoint;
		this.m_MainCurve.m_WhitePoint = whitePoint;
		this.m_MainCurve.m_ToeStrength = -1f * toe;
		this.m_MainCurve.m_ShoulderStrength = shoulder;
		this.m_MainCurve.m_CrossOverPoint = crossover * (whitePoint - blackPoint) + blackPoint;
		this.UpdateCoefficients();
	}

	private void OnEnable()
	{
		if (this.m_MainCurve == null)
		{
			this.m_MainCurve = new FilmicCurve();
		}
		this.CreateMaterials();
		this.UpdateCoefficients();
		this.VerifyAndUpdateShaderVariations(true);
	}

	private void Initialize()
	{
	}

	[ImageEffectTransformsToLDR]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.Initialize();
		if (this.m_Mode != this.m_LastMode)
		{
			this.ReloadShaders();
			this.CreateMaterials();
			this.VerifyAndUpdateShaderVariations(true);
		}
		this.m_LastMode = this.m_Mode;
		this.UpdateCoefficients();
		this.VerifyAndUpdateShaderVariations(false);
		this.m_Material.SetVector("_Tint", this.m_Tint);
		this.m_Material.SetVector("_Offsets", new Vector4(this.m_SharpnessKernel / (float)source.width, this.m_SharpnessKernel / (float)source.height));
		if (this.m_EnableLut0)
		{
			Graphics.Blit(source, destination, this.m_Material, 1);
		}
		else
		{
			Graphics.Blit(source, destination, this.m_Material, 0);
		}
	}

	[SerializeField]
	public FilmicCurve m_MainCurve = new FilmicCurve();

	[SerializeField]
	public Color m_Tint = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	public float m_MoodAmount;

	[SerializeField]
	public float m_Sharpness;

	[SerializeField]
	public float m_SharpnessKernel = 0.5f;

	[SerializeField]
	public DeluxeTonemapper.Mode m_Mode = DeluxeTonemapper.Mode.ColorHD;

	public bool m_EnableLut0;

	public float m_Saturation = 1f;

	public bool m_DisableColorGrading;

	private bool m_LastDisableColorGrading;

	public float m_Lut0Amount = 1f;

	public Texture2D m_2DLut0;

	public Texture3D m_3DLut0;

	private FTDLut m_LutTool = new FTDLut();

	public bool m_FilmicCurveOpened = true;

	public bool m_ColorGradingOpened;

	public bool m_HDREffectsgOpened;

	public bool m_BannerIsOpened = true;

	public bool m_Use2DLut;

	private bool m_LastUse2DLut;

	[SerializeField]
	private bool m_IsInitialized;

	private DeluxeTonemapper.Mode m_LastMode;

	private Material m_Material;

	private Shader m_Shader;

	public enum Mode
	{
		Color,
		Luminance,
		ExtendedLuminance,
		ColorHD
	}
}
