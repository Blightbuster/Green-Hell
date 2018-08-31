using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDEndDemo : HUDBase
{
	public static HUDEndDemo Get()
	{
		return HUDEndDemo.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.TwitchDemo);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDEndDemo.s_Instance = this;
		this.m_BG = base.transform.Find("BG").GetComponent<RawImage>();
		Color color = this.m_BG.color;
		color.a = 0f;
		this.m_BG.color = color;
		this.m_Logo = base.transform.Find("Logo").GetComponent<RawImage>();
		color = this.m_Logo.color;
		color.a = 0f;
		this.m_Logo.color = color;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_ShowTime = Time.time;
		Color color = this.m_BG.color;
		color.a = 0f;
		this.m_BG.color = color;
		color = this.m_Logo.color;
		color.a = 0f;
		this.m_Logo.color = color;
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
	}

	protected override bool ShouldShow()
	{
		return base.gameObject.activeSelf;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateBGAlpha();
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void UpdateBGAlpha()
	{
		Color color = this.m_BG.color;
		if (color.a < 1f)
		{
			color.a = Mathf.Clamp01(CJTools.Math.GetProportionalClamp(0f, 1f, Time.time - this.m_ShowTime, 0f, this.m_FadoutDuration));
			this.m_BG.color = color;
			this.m_LastBGFadeTime = Time.time;
		}
		else if (Time.time - this.m_LastBGFadeTime > 1.5f)
		{
			if (this.m_StartFadeLogoTime == 0f)
			{
				this.m_StartFadeLogoTime = Time.time;
			}
			Color color2 = this.m_Logo.color;
			if (color2.a < 1f)
			{
				color2.a = Mathf.Clamp01(CJTools.Math.GetProportionalClamp(0f, 1f, Time.time - this.m_StartFadeLogoTime, 0f, 2f));
				this.m_Logo.color = color2;
			}
		}
	}

	private RawImage m_BG;

	private RawImage m_Logo;

	private float m_ShowTime;

	private float m_LastBGFadeTime;

	private float m_StartFadeLogoTime;

	public float m_FadoutDuration = 2f;

	private static HUDEndDemo s_Instance;
}
