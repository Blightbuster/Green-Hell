using System;
using UnityEngine;

[ExecuteInEditMode]
public class DeluxeEyeAdaptation : MonoBehaviour
{
	public bool RenderDebugInfos
	{
		get
		{
			return this.m_ShowExposure || this.m_ShowHistogram;
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

	private void DestroyMaterial(Material mat)
	{
		if (mat)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	public void OnDisable()
	{
		if (this.m_Logic != null)
		{
			this.m_Logic.ClearMeshes();
		}
		this.DestroyMaterial(this.m_HistogramMaterial);
		this.m_HistogramMaterial = null;
		this.m_HistogramShader = null;
		this.DestroyMaterial(this.m_BrightnessMaterial);
		this.m_BrightnessMaterial = null;
		this.m_BrightnessShader = null;
	}

	private void CreateMaterials()
	{
		if (this.m_LastUseGLSLFix != this.m_UseGLSLFix)
		{
			this.m_HistogramShader = null;
			this.m_HistogramMaterial = null;
		}
		this.m_LastUseGLSLFix = this.m_UseGLSLFix;
		if (this.m_HistogramShader == null)
		{
			if (this.m_UseGLSLFix)
			{
				this.m_HistogramShader = Shader.Find("Hidden/Deluxe/EyeAdaptationGLSLFix");
			}
			else
			{
				this.m_HistogramShader = Shader.Find("Hidden/Deluxe/EyeAdaptation");
			}
			this.m_LastShowHistogram = !this.m_ShowHistogram;
		}
		if (this.m_HistogramMaterial == null && this.m_HistogramShader != null && this.m_HistogramShader.isSupported)
		{
			this.m_HistogramMaterial = this.CreateMaterial(this.m_HistogramShader);
		}
		if (this.m_BrightnessShader == null)
		{
			this.m_BrightnessShader = Shader.Find("Hidden/Deluxe/EyeAdaptationBright");
		}
		if (this.m_BrightnessMaterial == null && this.m_BrightnessShader != null && this.m_BrightnessShader.isSupported)
		{
			this.m_BrightnessMaterial = this.CreateMaterial(this.m_BrightnessShader);
			return;
		}
		if (this.m_BrightnessShader == null)
		{
			Debug.LogError("Cant find brightness shader");
			return;
		}
		if (!this.m_BrightnessShader.isSupported)
		{
			Debug.LogError("Brightness shader unsupported");
		}
	}

	public void OnEnable()
	{
		this.CreateMaterials();
		this.VerifyAndUpdateShaderVariations(true);
	}

	public void VerifyAndUpdateShaderVariations(bool forceUpdate)
	{
		if (this.m_HistogramMaterial == null)
		{
			return;
		}
		if (this.m_ShowHistogram != this.m_LastShowHistogram || forceUpdate)
		{
			this.m_HistogramMaterial.DisableKeyword("DLX_DEBUG_HISTOGRAM");
			if (this.m_ShowHistogram)
			{
				this.m_HistogramMaterial.EnableKeyword("DLX_DEBUG_HISTOGRAM");
			}
		}
		this.m_LastShowHistogram = this.m_ShowHistogram;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.CreateMaterials();
		this.VerifyAndUpdateShaderVariations(false);
		if (this.m_Logic == null)
		{
			this.m_Logic = new DeluxeEyeAdaptationLogic();
		}
		base.GetComponent<DeluxeTonemapper>();
		RenderTexture temporary = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, source.format);
		RenderTexture temporary2 = RenderTexture.GetTemporary(temporary.width / 2, temporary.height / 2, 0, source.format);
		if (this.m_BrightnessMaterial == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		this.m_BrightnessMaterial.SetTexture("_UpTex", source);
		source.filterMode = FilterMode.Bilinear;
		this.m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1f / (float)source.width * 0.5f, 1f / (float)source.height * 0.5f));
		Graphics.Blit(source, temporary, this.m_BrightnessMaterial, 1);
		this.m_BrightnessMaterial.SetTexture("_UpTex", temporary);
		temporary.filterMode = FilterMode.Bilinear;
		this.m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1f / (float)temporary.width * 0.5f, 1f / (float)temporary.height * 0.5f));
		Graphics.Blit(temporary, temporary2, this.m_BrightnessMaterial, 1);
		source.filterMode = FilterMode.Point;
		if (this.m_LowResolution)
		{
			RenderTexture temporary3 = RenderTexture.GetTemporary(temporary2.width / 2, temporary2.height / 2, 0, source.format);
			this.m_BrightnessMaterial.SetTexture("_UpTex", temporary2);
			temporary2.filterMode = FilterMode.Bilinear;
			this.m_BrightnessMaterial.SetVector("_PixelSize", new Vector4(1f / (float)temporary2.width * 0.5f, 1f / (float)temporary2.height * 0.5f));
			Graphics.Blit(temporary2, temporary3, this.m_BrightnessMaterial, 1);
			this.m_HistogramMaterial.SetTexture("_FrameTex", temporary3);
			this.m_Logic.ComputeExposure(temporary3.width, temporary3.height, this.m_HistogramMaterial);
			RenderTexture.ReleaseTemporary(temporary3);
		}
		else
		{
			this.m_HistogramMaterial.SetTexture("_FrameTex", temporary2);
			this.m_Logic.ComputeExposure(temporary2.width, temporary2.height, this.m_HistogramMaterial);
		}
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		if (!Application.isPlaying)
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = this.m_Logic.m_BrightnessRT;
			GL.Clear(false, true, Color.white);
			RenderTexture.active = active;
		}
		this.m_BrightnessMaterial.SetFloat("_ExposureOffset", this.m_Logic.m_ExposureOffset);
		this.m_BrightnessMaterial.SetFloat("_BrightnessMultiplier", this.m_Logic.m_BrightnessMultiplier);
		this.m_BrightnessMaterial.SetTexture("_BrightnessTex", this.m_Logic.m_BrightnessRT);
		this.m_BrightnessMaterial.SetTexture("_ColorTex", source);
		if (this.RenderDebugInfos)
		{
			this.m_BrightnessMaterial.SetFloat("_VisualizeExposure", this.m_ShowExposure ? 1f : 0f);
			this.m_BrightnessMaterial.SetFloat("_VisualizeHistogram", this.m_ShowHistogram ? 1f : 0f);
			this.m_BrightnessMaterial.SetVector("_MinMaxSpeedDt", this.m_Logic.MinMaxSpeedDT);
			this.m_BrightnessMaterial.SetTexture("_Histogram", this.m_Logic.m_HistogramList[0]);
			this.m_BrightnessMaterial.SetTexture("_ColorTex", source);
			this.m_BrightnessMaterial.SetFloat("_LuminanceRange", this.m_Logic.m_Range);
			this.m_BrightnessMaterial.SetVector("_HistogramCoefs", this.m_Logic.m_HistCoefs);
			this.m_BrightnessMaterial.SetVector("_MinMax", new Vector4(0.01f, this.m_HistogramSize * 0.75f, 0.01f, this.m_HistogramSize * 0.5f));
			this.m_BrightnessMaterial.SetFloat("_TotalPixelNumber", (float)(this.m_Logic.m_CurrentWidth * this.m_Logic.m_CurrentHeight));
			Graphics.Blit(source, destination, this.m_BrightnessMaterial, 2);
			return;
		}
		Graphics.Blit(source, destination, this.m_BrightnessMaterial, 0);
	}

	[SerializeField]
	public DeluxeEyeAdaptationLogic m_Logic;

	[SerializeField]
	public bool m_LowResolution;

	[SerializeField]
	public bool m_ShowHistogram;

	private bool m_LastShowHistogram;

	[SerializeField]
	public bool m_ShowExposure;

	[SerializeField]
	public float m_HistogramSize = 0.5f;

	private Material m_HistogramMaterial;

	private Shader m_HistogramShader;

	private Material m_BrightnessMaterial;

	private Shader m_BrightnessShader;

	public bool m_LastUseGLSLFix;

	public bool m_UseGLSLFix;

	public bool m_ExposureOpened = true;

	public bool m_SpeedOpened = true;

	public bool m_HistogramOpened = true;

	public bool m_OptimizationOpened = true;

	public bool m_DebugOpened = true;

	public bool m_BannerIsOpened = true;
}
