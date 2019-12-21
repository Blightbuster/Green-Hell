using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class FadeSystem : MonoBehaviour
{
	public static FadeSystem Get()
	{
		return FadeSystem.s_Instance;
	}

	private void Awake()
	{
		FadeSystem.s_Instance = this;
		this.m_FadeLevel1 = new float[2];
		this.m_Color.a = 0f;
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.color = this.m_Color;
			rawImage.enabled = false;
		}
	}

	private void Start()
	{
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.enabled = true;
		}
		this.m_FadeLevel1[0] = 1f;
		this.UpdateAlpha(false);
	}

	public void SetFadeOut()
	{
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.enabled = true;
		}
		this.m_FadeLevel1[0] = 1f;
		this.UpdateAlpha(false);
		this.m_FadeIn = false;
		this.m_FadingIn = false;
		this.m_FadeOut = false;
		this.m_FadingOut = false;
	}

	private void Update()
	{
		if (!this.m_FirstFadeIn && LoadingScreen.Get() != null && !LoadingScreen.Get().m_Active)
		{
			float num = (HUDMovie.Get() != null && HUDMovie.Get().m_Active) ? Time.unscaledDeltaTime : Time.deltaTime;
			this.m_FirstFadeInDelay += num;
			if (this.m_FirstFadeInDelay >= 1f)
			{
				this.FadeIn(FadeType.Vis, null, 1.5f);
				this.m_FirstFadeIn = true;
			}
		}
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
		if (this.m_ScreenFadingIn || this.m_ScreenFadingOut)
		{
			this.UpdateAlpha(true);
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

	public void FadeIn(FadeType type, VDelegate callback = null, float duration = 1.5f)
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
		this.m_Callback = callback;
		if (this.m_Screen)
		{
			this.m_ScreenFadingOut = false;
			this.m_ScreenFadingIn = true;
			this.m_StartScreenFadeAlpha = this.m_ScreenAlphaController.alpha;
			this.m_StartScreenFadeTime = Time.time;
		}
		this.m_FadeInTime = 0f;
	}

	private void FadeIn()
	{
		float num = (HUDMovie.Get() != null && HUDMovie.Get().m_Active) ? Time.unscaledDeltaTime : Time.deltaTime;
		this.m_FadeInTime += num;
		if (this.ShouldFadeImage())
		{
			this.m_FadeLevel1[0] = CJTools.Math.GetProportionalClamp(1f, 0f, this.m_FadeInTime, 0f, this.m_FadeDuration);
			this.UpdateAlpha(false);
		}
		if (this.ShouldFadeSounds())
		{
			this.m_FadeLevel1[1] = CJTools.Math.GetProportionalClamp(1f, 0f, this.m_FadeInTime, 0f, this.m_FadeDuration);
			this.UpdateSounds();
		}
		this.m_VisFadeLevel = this.m_FadeLevel1[0];
		this.m_SoundFadeLevel = this.m_FadeLevel1[1];
	}

	public void FadeOut(FadeType type, VDelegate callback = null, float duration = 1.5f, GameObject screen_prefab = null)
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
		this.m_Callback = callback;
		if (screen_prefab)
		{
			if (this.m_Screen)
			{
				UnityEngine.Object.Destroy(this.m_Screen);
				this.m_Screen = null;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(screen_prefab, Vector3.zero, Quaternion.identity);
			gameObject.name = "FadeScreen";
			gameObject.transform.parent = base.transform;
			((RectTransform)gameObject.transform).anchoredPosition = Vector3.zero;
			this.m_Screen = gameObject;
			this.m_ScreenAlphaController = gameObject.GetComponent<CanvasGroup>();
		}
		this.m_FadeOutTime = 0f;
	}

	private void FadeOut()
	{
		float num = (HUDMovie.Get() != null && HUDMovie.Get().m_Active) ? Time.unscaledDeltaTime : Time.deltaTime;
		this.m_FadeOutTime += num;
		if (this.ShouldFadeImage())
		{
			this.m_FadeLevel1[0] = CJTools.Math.GetProportionalClamp(0f, 1f, this.m_FadeOutTime, 0f, this.m_FadeDuration);
			this.UpdateAlpha(false);
		}
		if (this.ShouldFadeSounds())
		{
			this.m_FadeLevel1[1] = CJTools.Math.GetProportionalClamp(0f, 1f, this.m_FadeOutTime, 0f, this.m_FadeDuration);
			this.UpdateSounds();
		}
		this.m_VisFadeLevel = this.m_FadeLevel1[0];
		this.m_SoundFadeLevel = this.m_FadeLevel1[1];
	}

	private void UpdateAlpha(bool update_time = false)
	{
		this.m_Color.a = this.m_FadeLevel1[0];
		this.m_Color.a = Mathf.Clamp01(this.m_Color.a);
		foreach (RawImage rawImage in this.m_FadeImages)
		{
			rawImage.color = this.m_Color;
		}
		if (this.m_ScreenFadingOut)
		{
			if (update_time)
			{
				float num = (HUDMovie.Get() != null && HUDMovie.Get().m_Active) ? Time.unscaledDeltaTime : Time.deltaTime;
				this.m_FadeOutTime += num;
			}
			this.m_ScreenAlphaController.alpha = CJTools.Math.GetProportionalClamp(this.m_StartScreenFadeAlpha, 1f, this.m_FadeOutTime, 0f, this.m_FadeDuration);
			if (this.m_ScreenAlphaController.alpha >= 1f)
			{
				this.m_ScreenAlphaController.alpha = 1f;
				this.m_ScreenFadingOut = false;
				return;
			}
		}
		else if (this.m_ScreenFadingIn)
		{
			if (update_time)
			{
				float num2 = (HUDMovie.Get() != null && HUDMovie.Get().m_Active) ? Time.unscaledDeltaTime : Time.deltaTime;
				this.m_FadeInTime += num2;
			}
			this.m_ScreenAlphaController.alpha = CJTools.Math.GetProportionalClamp(this.m_StartScreenFadeAlpha, 0f, this.m_FadeInTime, 0f, this.m_FadeDuration);
			if (this.m_ScreenAlphaController.alpha <= 0f)
			{
				this.m_ScreenFadingIn = false;
			}
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
		if (this.m_ScreenFadingIn)
		{
			return;
		}
		this.FadeIn();
		float num = 0f;
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
			this.UpdateAlpha(false);
			foreach (RawImage rawImage in this.m_FadeImages)
			{
				rawImage.enabled = false;
			}
			this.m_FadingIn = false;
			Debug.Log("FadeSystem FadeInCoroutine m_FadingIn = false");
			if (this.m_Callback != null)
			{
				this.m_Callback();
			}
			if (this.m_Screen)
			{
				UnityEngine.Object.Destroy(this.m_Screen);
				this.m_Screen = null;
			}
			this.m_Callback = null;
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
			this.UpdateAlpha(false);
			this.m_FadingOut = false;
			Debug.Log("FadeSystem FadeOutCoroutine m_FadingOut = false");
			if (this.m_Callback != null)
			{
				this.m_Callback();
			}
			this.m_Callback = null;
			if (this.m_Screen)
			{
				this.m_ScreenFadingOut = true;
				this.m_StartScreenFadeAlpha = this.m_ScreenAlphaController.alpha;
				this.m_StartScreenFadeTime = Time.time;
			}
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

	public bool GetFirstFadeIn()
	{
		return this.m_FirstFadeIn;
	}

	public bool IsFadeVisible()
	{
		return this.m_Color.a > 0.05f;
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

	private VDelegate m_Callback;

	private GameObject m_Screen;

	private CanvasGroup m_ScreenAlphaController;

	private bool m_ScreenFadingIn;

	private float m_StartScreenFadeTime;

	private float m_StartScreenFadeAlpha;

	private bool m_ScreenFadingOut;

	private bool m_FirstFadeIn;

	private float m_FirstFadeInDelay;

	private static FadeSystem s_Instance;

	private float m_FadeInTime;

	private float m_FadeOutTime;
}
