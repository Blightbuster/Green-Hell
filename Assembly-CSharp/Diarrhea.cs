using System;
using Enums;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Diarrhea : global::DiseaseSymptom
{
	public override void Initialize()
	{
		base.Initialize();
		this.m_Type = Enums.DiseaseSymptom.Diarrhea;
		DiarrheaController.Get().SetDiarrhea(this);
		this.m_NoiseEffect = Camera.main.gameObject.GetComponent<NoiseAndGrain>();
		this.m_BlurEffect = Camera.main.gameObject.GetComponent<BlurOptimized>();
		this.m_Duration = 300f;
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (this.m_NoiseEffect)
		{
			this.m_NoiseEffect.enabled = true;
			this.m_NoiseEffect.intensityMultiplier = 0f;
		}
		if (this.m_BlurEffect)
		{
			this.m_BlurEffect.enabled = true;
			this.m_BlurEffect.downsample = 0;
			this.m_BlurEffect.blurSize = 0f;
			this.m_BlurEffect.blurIterations = 1;
		}
		this.m_EffectPower = 0f;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		if (this.m_NoiseEffect)
		{
			this.m_NoiseEffect.enabled = false;
		}
		if (this.m_BlurEffect)
		{
			this.m_BlurEffect.enabled = false;
		}
	}

	public override void Update()
	{
		base.Update();
		if (!this.m_Effect)
		{
			if (Time.time < this.m_NextEffectTime)
			{
				return;
			}
			this.StartEffect();
		}
		this.m_EffectPower = 0f;
		float num = Time.time - this.m_StartEffectTime;
		if (num < this.m_EffectDuration && this.m_EffectDuration > 0f)
		{
			this.m_EffectPower = Mathf.Sin(num / this.m_EffectDuration * 3.14159274f);
			this.m_EffectPower = Mathf.Clamp(this.m_EffectPower, 0f, 1f);
		}
		else
		{
			this.m_Effect = false;
			this.m_NextEffectTime = Time.time + this.m_EffectInterval;
		}
		if (this.m_NoiseEffect)
		{
			this.m_NoiseEffect.intensityMultiplier = this.m_EffectPower * this.m_MaxNoiseIntensity;
		}
		if (this.m_BlurEffect)
		{
			this.m_BlurEffect.blurSize = this.m_EffectPower * this.m_MaxBlurSize;
		}
	}

	private void StartEffect()
	{
		this.m_Effect = true;
		this.m_StartEffectTime = Time.time;
	}

	public bool IsEffect()
	{
		return this.m_EffectPower > 0f;
	}

	private NoiseAndGrain m_NoiseEffect;

	private BlurOptimized m_BlurEffect;

	private float m_MaxNoiseIntensity = 10f;

	private float m_MaxBlurSize = 3.5f;

	private float m_EffectPower;

	private float m_EffectDuration = 5f;

	private bool m_Effect;

	private float m_EffectInterval = 60f;

	private float m_StartEffectTime;

	private float m_NextEffectTime;
}
