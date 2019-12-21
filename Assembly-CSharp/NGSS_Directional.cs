using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class NGSS_Directional : MonoBehaviour
{
	private void OnDisable()
	{
		this.isInitialized = false;
		if (this.KEEP_NGSS_ONDISABLE)
		{
			return;
		}
		if (this.isGraphicSet)
		{
			this.isGraphicSet = false;
			GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/Internal-ScreenSpaceShadows"));
			GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseBuiltin);
		}
	}

	private void OnEnable()
	{
		this.Init();
	}

	private void Init()
	{
		if (this.isInitialized)
		{
			return;
		}
		if (!this.isGraphicSet)
		{
			GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);
			GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/NGSS_Directional"));
			this.isGraphicSet = true;
		}
		this.isInitialized = true;
	}

	private void Update()
	{
		if (this.EARLY_BAILOUT_OPTIMIZATION)
		{
			Shader.EnableKeyword("NGSS_USE_EARLY_BAILOUT_OPTIMIZATION_DIR");
		}
		else
		{
			Shader.DisableKeyword("NGSS_USE_EARLY_BAILOUT_OPTIMIZATION_DIR");
		}
		if (this.BIAS_FADE)
		{
			Shader.EnableKeyword("NGSS_USE_BIAS_FADE_DIR");
			Shader.SetGlobalFloat("NGSS_BIAS_FADE_DIR", this.BIAS_FADE_VALUE * 0.001f);
		}
		else
		{
			Shader.DisableKeyword("NGSS_USE_BIAS_FADE_DIR");
		}
		if (this.PCSS_ENABLED)
		{
			Shader.EnableKeyword("NGSS_PCSS_FILTER_DIR");
		}
		else
		{
			Shader.DisableKeyword("NGSS_PCSS_FILTER_DIR");
		}
		Shader.SetGlobalFloat("NGSS_POISSON_SAMPLING_NOISE_DIR", this.BANDING_NOISE_VALUE * 10f);
		Shader.SetGlobalFloat("NGSS_PCSS_GLOBAL_SOFTNESS", this.GLOBAL_SOFTNESS * 0.01f);
		Shader.SetGlobalFloat("NGSS_PCSS_GLOBAL_SOFTNESS_MOBILE", 1f - this.GLOBAL_SOFTNESS * 75f / QualitySettings.shadowDistance);
		float num = this.PCSS_SOFTNESS_MIN * 0.05f;
		float num2 = this.PCSS_SOFTNESS_MAX * 0.25f;
		Shader.SetGlobalFloat("NGSS_PCSS_FILTER_DIR_MIN", (num > num2) ? num2 : num);
		Shader.SetGlobalFloat("NGSS_PCSS_FILTER_DIR_MAX", (num2 < num) ? num : num2);
		Shader.DisableKeyword("DIR_POISSON_64");
		Shader.DisableKeyword("DIR_POISSON_32");
		Shader.DisableKeyword("DIR_POISSON_25");
		Shader.DisableKeyword("DIR_POISSON_16");
		Shader.EnableKeyword((this.SAMPLERS_COUNT == NGSS_Directional.SAMPLER_COUNT.SAMPLERS_64) ? "DIR_POISSON_64" : ((this.SAMPLERS_COUNT == NGSS_Directional.SAMPLER_COUNT.SAMPLERS_32) ? "DIR_POISSON_32" : ((this.SAMPLERS_COUNT == NGSS_Directional.SAMPLER_COUNT.SAMPLERS_25) ? "DIR_POISSON_25" : "DIR_POISSON_16")));
	}

	[Header("MAIN SETTINGS")]
	[Tooltip("If false, NGSS Directional shadows replacement will be removed from Graphics settings when OnDisable is called in this component.")]
	public bool KEEP_NGSS_ONDISABLE = true;

	[Header("OPTIMIZATION")]
	[Tooltip("Optimize shadows performance by skipping fragments that are either 100% lit or 100% shadowed. Some macro noisy artefacts can be seen if shadows are too soft or sampling amount is below 64.")]
	public bool EARLY_BAILOUT_OPTIMIZATION = true;

	[Tooltip("Recommended values: Mobile = 16, Consoles = 25, Desktop VR = 32, Desktop High = 64")]
	public NGSS_Directional.SAMPLER_COUNT SAMPLERS_COUNT = NGSS_Directional.SAMPLER_COUNT.SAMPLERS_64;

	[Header("SOFTNESS")]
	[Tooltip("Overall softness for both PCF and PCSS shadows.")]
	[Range(0f, 2f)]
	public float GLOBAL_SOFTNESS = 1f;

	[Header("BANDING")]
	[Tooltip("Amount of banding or noise. Example: 0.0 gives 100 % Banding and 1.0 gives 100 % Noise.")]
	[Range(0f, 2f)]
	public float BANDING_NOISE_VALUE = 1f;

	[Header("BIAS")]
	[Tooltip("Fades out artifacts produced by shadow bias")]
	public bool BIAS_FADE = true;

	[Tooltip("Fades out artifacts produced by shadow bias")]
	[Range(0f, 2f)]
	public float BIAS_FADE_VALUE = 1f;

	[Header("PCSS")]
	[Tooltip("Provides Area Light like soft-shadows. With shadows being harder at close ranges and softer at long ranges.\nDisable it if you are looking for uniformly simple soft-shadows. Disabled by default on Mobile.")]
	public bool PCSS_ENABLED = true;

	[Tooltip("PCSS softness when shadows is close to caster.")]
	[Range(0f, 2f)]
	public float PCSS_SOFTNESS_MIN = 1f;

	[Tooltip("PCSS softness when shadows is far from caster.")]
	[Range(0f, 2f)]
	public float PCSS_SOFTNESS_MAX = 1f;

	private bool isInitialized;

	private bool isGraphicSet;

	public enum SAMPLER_COUNT
	{
		SAMPLERS_16,
		SAMPLERS_25,
		SAMPLERS_32,
		SAMPLERS_64
	}
}
