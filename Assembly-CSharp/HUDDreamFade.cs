using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDDreamFade : HUDBase
{
	public static HUDDreamFade Get()
	{
		return HUDDreamFade.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDDreamFade.s_Instance = this;
		this.m_FadeLevel1 = new float[2];
		this.m_Color.a = 0f;
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.color = this.m_Color;
			rawImage.enabled = false;
		}
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	protected override void Start()
	{
		base.Start();
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.enabled = true;
		}
		this.UpdateAlpha();
	}

	public void SetFadeOut()
	{
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.enabled = true;
		}
		this.m_FadeLevel1[0] = 1f;
		this.UpdateAlpha();
		this.m_FadeIn = false;
		this.m_FadingIn = false;
		this.m_FadeOut = false;
		this.m_FadingOut = false;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.UpdateAlpha();
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateInternal();
	}

	public float GetFadeLevel(FadeType type)
	{
		return this.m_FadeLevel1[(int)type];
	}

	public void UpdateInternal()
	{
		if (this.m_FadingIn)
		{
			this.FadeInCoroutine();
		}
		if (this.m_FadingOut)
		{
			this.FadeOutCoroutine();
		}
		if (this.m_FadeIn)
		{
			this.StartFadeIn();
		}
		if (this.m_FadeOut)
		{
			this.StartFadeOut();
		}
	}

	private bool ShouldFadeImage()
	{
		return this.m_FadeType == FadeType.All || this.m_FadeType == FadeType.Vis;
	}

	private bool ShouldFadeSounds()
	{
		return this.m_FadeType == FadeType.All || this.m_FadeType == FadeType.Sound;
	}

	public void FadeIn(FadeType type, float duration = 1.5f)
	{
		this.m_FadeDuration = duration;
		Debug.Log(string.Concat(new string[]
		{
			"FadeSystem FadIn m_FadeIn = ",
			this.m_FadeIn.ToString(),
			" m_FadeOut = ",
			this.m_FadeOut.ToString(),
			" m_FadingIn ",
			this.m_FadingIn.ToString(),
			" m_FadingOut ",
			this.m_FadingOut.ToString()
		}));
		this.m_FadeType = type;
		this.m_FadeIn = true;
		this.m_FadeOut = false;
		this.m_FadingOut = false;
		this.m_Active = true;
	}

	private void FadeIn()
	{
		if (this.ShouldFadeImage())
		{
			this.m_FadeLevel1[0] = Mathf.Lerp(this.m_FadeLevel1[0], 0f, (this.m_FadeDuration > 0f) ? (Time.deltaTime / this.m_FadeDuration) : 1f);
			this.UpdateAlpha();
		}
		if (this.ShouldFadeSounds())
		{
			this.m_FadeLevel1[1] = Mathf.Lerp(this.m_FadeLevel1[1], 0f, (this.m_FadeDuration > 0f) ? (Time.deltaTime / this.m_FadeDuration) : 1f);
			this.UpdateSounds();
		}
		this.m_VisFadeLevel = this.m_FadeLevel1[0];
		this.m_SoundFadeLevel = this.m_FadeLevel1[1];
	}

	public void FadeOut(FadeType type, float duration = 1.5f)
	{
		this.m_FadeDuration = duration;
		Debug.Log(string.Concat(new string[]
		{
			"FadeSystem FadOut m_FadeIn = ",
			this.m_FadeIn.ToString(),
			" m_FadeOut = ",
			this.m_FadeOut.ToString(),
			" m_FadingIn ",
			this.m_FadingIn.ToString(),
			" m_FadingOut ",
			this.m_FadingOut.ToString()
		}));
		this.m_FadeType = type;
		this.m_FadeOut = true;
		this.m_FadeIn = false;
		this.m_FadingIn = false;
		this.m_FadeLevel1[0] = 0f;
		this.m_FadeLevel1[1] = 0f;
		this.m_Active = true;
	}

	private void FadeOut()
	{
		if (this.ShouldFadeImage())
		{
			this.m_FadeLevel1[0] = Mathf.Lerp(this.m_FadeLevel1[0], 1.1f, (this.m_FadeDuration > 0f) ? (Time.deltaTime / this.m_FadeDuration) : 1f);
			this.UpdateAlpha();
		}
		if (this.ShouldFadeSounds())
		{
			this.m_FadeLevel1[1] = Mathf.Lerp(this.m_FadeLevel1[1], 1.1f, (this.m_FadeDuration > 0f) ? (Time.deltaTime / this.m_FadeDuration) : 1f);
			this.UpdateSounds();
		}
		this.m_VisFadeLevel = this.m_FadeLevel1[0];
		this.m_SoundFadeLevel = this.m_FadeLevel1[1];
	}

	private void UpdateAlpha()
	{
		this.m_Color.a = this.m_FadeLevel1[0];
		this.m_Color.a = Mathf.Clamp01(this.m_Color.a);
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.color = this.m_Color;
		}
	}

	private void UpdateSounds()
	{
	}

	private void StartFadeIn()
	{
		this.m_FadingIn = true;
		this.m_FadeIn = false;
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.enabled = true;
		}
	}

	public void FadeInCoroutine()
	{
		this.FadeIn();
		float num = 0.05f;
		bool flag = false;
		switch (this.m_FadeType)
		{
		case FadeType.Vis:
			flag = (this.m_FadeLevel1[0] <= num);
			break;
		case FadeType.Sound:
			flag = (this.m_FadeLevel1[1] <= num);
			break;
		case FadeType.All:
			flag = (this.m_FadeLevel1[0] <= num && this.m_FadeLevel1[1] <= num);
			break;
		}
		if (flag)
		{
			switch (this.m_FadeType)
			{
			case FadeType.Vis:
				this.m_FadeLevel1[0] = 0f;
				break;
			case FadeType.Sound:
				this.m_FadeLevel1[1] = 0f;
				break;
			case FadeType.All:
				this.m_FadeLevel1[0] = 0f;
				this.m_FadeLevel1[1] = 0f;
				break;
			}
			this.m_VisFadeLevel = this.m_FadeLevel1[0];
			this.m_SoundFadeLevel = this.m_FadeLevel1[1];
			this.UpdateAlpha();
			foreach (RawImage rawImage in this.m_FadeImages)
			{
				rawImage.enabled = false;
			}
			this.m_FadingIn = false;
			Debug.Log("FadeSystem FadeInCoroutine m_FadingIn = false");
			this.m_Active = false;
		}
	}

	public void StartFadeOut()
	{
		this.m_FadingOut = true;
		this.m_FadeOut = false;
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.enabled = true;
		}
	}

	public void FadeOutCoroutine()
	{
		float num = 1f;
		bool flag = false;
		switch (this.m_FadeType)
		{
		case FadeType.Vis:
			flag = (this.m_FadeLevel1[0] >= num);
			break;
		case FadeType.Sound:
			flag = (this.m_FadeLevel1[1] >= num);
			break;
		case FadeType.All:
			flag = (this.m_FadeLevel1[0] >= num && this.m_FadeLevel1[1] >= num);
			break;
		}
		if (flag)
		{
			switch (this.m_FadeType)
			{
			case FadeType.Vis:
				this.m_FadeLevel1[0] = 1f;
				break;
			case FadeType.Sound:
				this.m_FadeLevel1[1] = 1f;
				break;
			case FadeType.All:
				this.m_FadeLevel1[0] = 1f;
				this.m_FadeLevel1[1] = 1f;
				break;
			}
			this.m_VisFadeLevel = this.m_FadeLevel1[0];
			this.m_SoundFadeLevel = this.m_FadeLevel1[1];
			this.UpdateAlpha();
			this.m_FadingOut = false;
			Debug.Log("FadeSystem FadeOutCoroutine m_FadingOut = false");
		}
		this.FadeOut();
	}

	public bool CanStartFade()
	{
		return !this.m_FadeIn && !this.m_FadeOut && !this.m_FadingIn && !this.m_FadingOut;
	}

	public void SetFadeLevel(FadeType type, float val)
	{
		switch (this.m_FadeType)
		{
		case FadeType.Vis:
			this.m_FadeLevel1[0] = val;
			return;
		case FadeType.Sound:
			this.m_FadeLevel1[1] = val;
			return;
		case FadeType.All:
			this.m_FadeLevel1[0] = val;
			this.m_FadeLevel1[1] = val;
			return;
		default:
			return;
		}
	}

	public void SetImagesColor(Color color)
	{
		this.m_Color = color;
		for (int i = 0; i < this.m_FadeImages.Count; i++)
		{
			RawImage rawImage = this.m_FadeImages[i];
			if (rawImage)
			{
				rawImage.color = color;
			}
		}
	}

	public List<RawImage> m_FadeImages;

	public float m_FadeDuration = 1.5f;

	private const float m_FadeDurationDefault = 1.5f;

	[NonSerialized]
	public bool m_FadeIn;

	[NonSerialized]
	public bool m_FadingIn;

	[NonSerialized]
	public bool m_FadeOut;

	[NonSerialized]
	public bool m_FadingOut;

	public Color m_Color;

	private float[] m_FadeLevel1 = new float[2];

	[NonSerialized]
	public float m_SoundFadeLevel;

	[NonSerialized]
	public float m_VisFadeLevel;

	public FadeType m_FadeType = FadeType.All;

	private static HUDDreamFade s_Instance;

	private bool m_Active;
}
