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

	private void Update()
	{
		if (!this.m_FirstFadeIn && LoadingScreen.Get() != null && !LoadingScreen.Get().m_Active)
		{
			this.m_FirstFadeInDelay += Time.deltaTime;
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
			this.UpdateAlpha();
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

	public void FadeIn(FadeType type, VDelegate callback = null, float speed = 1.5f)
	{
		this.m_FadeSpeed = speed;
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
	}

	private void FadeIn()
	{
		if (this.ShouldFadeImage())
		{
			this.m_FadeLevel1[0] = Mathf.Lerp(this.m_FadeLevel1[0], 0f, this.m_FadeSpeed * Time.deltaTime);
			this.UpdateAlpha();
		}
		if (this.ShouldFadeSounds())
		{
			this.m_FadeLevel1[1] = Mathf.Lerp(this.m_FadeLevel1[1], 0f, this.m_FadeSpeed * Time.deltaTime);
			this.UpdateSounds();
		}
		this.m_VisFadeLevel = this.m_FadeLevel1[0];
		this.m_SoundFadeLevel = this.m_FadeLevel1[1];
	}

	public void FadeOut(FadeType type, VDelegate callback = null, float speed = 1.5f, GameObject screen_prefab = null)
	{
		this.m_FadeSpeed = speed;
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
	}

	private void FadeOut()
	{
		if (this.ShouldFadeImage())
		{
			this.m_FadeLevel1[0] = Mathf.Lerp(this.m_FadeLevel1[0], 1.1f, this.m_FadeSpeed * Time.deltaTime);
			this.UpdateAlpha();
		}
		if (this.ShouldFadeSounds())
		{
			this.m_FadeLevel1[1] = Mathf.Lerp(this.m_FadeLevel1[1], 1.1f, this.m_FadeSpeed * Time.deltaTime);
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
		if (this.m_ScreenFadingOut)
		{
			float b = (1f - this.m_StartScreenFadeAlpha) * 1.5f;
			this.m_ScreenAlphaController.alpha = CJTools.Math.GetProportionalClamp(this.m_StartScreenFadeAlpha, 1f, Time.time - this.m_StartScreenFadeTime, 0f, b);
			if (this.m_ScreenAlphaController.alpha >= 1f)
			{
				this.m_ScreenAlphaController.alpha = 1f;
				this.m_ScreenFadingOut = false;
			}
		}
		else if (this.m_ScreenFadingIn)
		{
			float b2 = this.m_StartScreenFadeAlpha * 1.5f;
			this.m_ScreenAlphaController.alpha = CJTools.Math.GetProportionalClamp(this.m_StartScreenFadeAlpha, 0f, Time.time - this.m_StartScreenFadeTime, 0f, b2);
			if (this.m_ScreenAlphaController.alpha == 0f)
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
		float num = 0.05f;
		bool flag = false;
		FadeType fadeType = this.m_FadeType;
		if (fadeType != FadeType.All)
		{
			if (fadeType != FadeType.Vis)
			{
				if (fadeType == FadeType.Sound)
				{
					flag = (this.m_FadeLevel1[1] <= num);
				}
			}
			else
			{
				flag = (this.m_FadeLevel1[0] <= num);
			}
		}
		else
		{
			flag = (this.m_FadeLevel1[0] <= num && this.m_FadeLevel1[1] <= num);
		}
		if (flag)
		{
			FadeType fadeType2 = this.m_FadeType;
			if (fadeType2 != FadeType.All)
			{
				if (fadeType2 != FadeType.Vis)
				{
					if (fadeType2 == FadeType.Sound)
					{
						this.m_FadeLevel1[1] = 0f;
					}
				}
				else
				{
					this.m_FadeLevel1[0] = 0f;
				}
			}
			else
			{
				this.m_FadeLevel1[0] = 0f;
				this.m_FadeLevel1[1] = 0f;
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
		FadeType fadeType = this.m_FadeType;
		if (fadeType != FadeType.All)
		{
			if (fadeType != FadeType.Vis)
			{
				if (fadeType == FadeType.Sound)
				{
					flag = (this.m_FadeLevel1[1] >= num);
				}
			}
			else
			{
				flag = (this.m_FadeLevel1[0] >= num);
			}
		}
		else
		{
			flag = (this.m_FadeLevel1[0] >= num && this.m_FadeLevel1[1] >= num);
		}
		if (flag)
		{
			FadeType fadeType2 = this.m_FadeType;
			if (fadeType2 != FadeType.All)
			{
				if (fadeType2 != FadeType.Vis)
				{
					if (fadeType2 == FadeType.Sound)
					{
						this.m_FadeLevel1[1] = 1f;
					}
				}
				else
				{
					this.m_FadeLevel1[0] = 1f;
				}
			}
			else
			{
				this.m_FadeLevel1[0] = 1f;
				this.m_FadeLevel1[1] = 1f;
			}
			this.m_VisFadeLevel = this.m_FadeLevel1[0];
			this.m_SoundFadeLevel = this.m_FadeLevel1[1];
			this.UpdateAlpha();
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
		FadeType fadeType = this.m_FadeType;
		if (fadeType != FadeType.All)
		{
			if (fadeType != FadeType.Vis)
			{
				if (fadeType == FadeType.Sound)
				{
					this.m_FadeLevel1[1] = val;
				}
			}
			else
			{
				this.m_FadeLevel1[0] = val;
			}
		}
		else
		{
			this.m_FadeLevel1[0] = val;
			this.m_FadeLevel1[1] = val;
		}
	}

	public List<RawImage> m_FadeImages;

	public float m_FadeSpeed = 1.5f;

	private const float m_FadeSpeedDefault = 1.5f;

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
}
