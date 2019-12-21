using System;
using UnityEngine;

public class BloodPP : MonoBehaviour
{
	public static BloodPP Get()
	{
		return BloodPP.s_Instance;
	}

	public Material m_Material
	{
		get
		{
			if (this.mat == null)
			{
				this.mat = new Material(this.m_BloodShader);
				this.mat.hideFlags = HideFlags.HideAndDontSave;
			}
			return this.mat;
		}
	}

	private void Awake()
	{
		BloodPP.s_Instance = this;
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (!this.m_BloodShader && !this.m_BloodShader.isSupported)
		{
			base.enabled = false;
			return;
		}
		Texture2D value = Resources.Load("Textures/PP/Wounds/PP_Blood") as Texture2D;
		int nameID = Shader.PropertyToID("_MainDrops");
		this.m_Material.SetTexture(nameID, value);
		value = (Resources.Load("Textures/PP/Wounds/PP_Blood_Mask") as Texture2D);
		nameID = Shader.PropertyToID("_DropsMask00");
		this.m_Material.SetTexture(nameID, value);
		for (int i = 0; i < 4; i++)
		{
			this.m_MaskFactors[i] = -1f;
			this.m_MaskFactorProperty[i] = Shader.PropertyToID("_MaskFactor" + i.ToString());
			this.m_MaskIntensityProperty[i] = Shader.PropertyToID("_MaskIntensity" + i.ToString());
		}
	}

	private void Update()
	{
		if (Debug.isDebugBuild)
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha1))
			{
				this.m_MaskFactors[0] = Time.time;
			}
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha2))
			{
				this.m_MaskFactors[1] = Time.time;
			}
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha3))
			{
				this.m_MaskFactors[2] = Time.time;
			}
			else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Alpha4))
			{
				this.m_MaskFactors[3] = Time.time;
			}
		}
		for (int i = 0; i < 4; i++)
		{
			this.m_Material.SetFloat(this.m_MaskFactorProperty[i], this.m_MaskFactors[i]);
			float value = 1f - (Time.time - this.m_MaskFactors[i] - 1f);
			value = Mathf.Clamp01(value);
			this.m_Material.SetFloat(this.m_MaskIntensityProperty[i], value);
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (this.m_BloodShader != null)
		{
			Graphics.Blit(src, dst, this.m_Material);
			return;
		}
		Graphics.Blit(src, dst);
	}

	public void ShowMask(int i)
	{
		this.m_MaskFactors[i] = Time.time;
	}

	public Shader m_BloodShader;

	public const int m_NumMasks = 4;

	private float[] m_MaskFactors = new float[4];

	private int[] m_MaskFactorProperty = new int[4];

	private int[] m_MaskIntensityProperty = new int[4];

	private static BloodPP s_Instance;

	private Material mat;
}
