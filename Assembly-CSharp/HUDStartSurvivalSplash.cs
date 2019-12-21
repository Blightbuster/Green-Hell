using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDStartSurvivalSplash : HUDBase
{
	public static HUDStartSurvivalSplash Get()
	{
		return HUDStartSurvivalSplash.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_CanvasGroup.blocksRaycasts = false;
		this.m_CanvasGroup.gameObject.GetComponent<RawImage>().raycastTarget = false;
		this.m_Text.raycastTarget = false;
		HUDStartSurvivalSplash.s_Instance = this;
	}

	protected override void Start()
	{
		base.Start();
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	public void Activate()
	{
		this.m_Active = true;
		this.m_WasActive = true;
	}

	protected override void OnShow()
	{
		base.OnShow();
		Color color = this.m_Text.color;
		color.a = 0f;
		this.m_Text.color = color;
		this.m_State = HUDStartSurvivalSplash.State.Enter;
		this.m_CanvasGroup.alpha = 1f;
	}

	protected override void Update()
	{
		base.Update();
		switch (this.m_State)
		{
		case HUDStartSurvivalSplash.State.Enter:
		{
			if (this.m_CanvasGroup.alpha < 1f)
			{
				this.m_CanvasGroup.alpha += Time.deltaTime;
				this.m_CanvasGroup.alpha = Mathf.Clamp01(this.m_CanvasGroup.alpha);
				return;
			}
			Color color = this.m_Text.color;
			color.a += Time.deltaTime;
			color.a = Mathf.Clamp01(color.a);
			this.m_Text.color = color;
			if (color.a == 1f)
			{
				this.m_State = HUDStartSurvivalSplash.State.Normal;
				return;
			}
			break;
		}
		case HUDStartSurvivalSplash.State.Normal:
			if (Input.GetKeyDown(KeyCode.Space))
			{
				if (this.m_WasPause)
				{
					MainLevel.Instance.Pause(false);
				}
				this.m_State = HUDStartSurvivalSplash.State.Exit;
				return;
			}
			if (!MainLevel.Instance.IsPause() && FadeSystem.Get().CanStartFade())
			{
				this.m_WasPause = true;
				MainLevel.Instance.Pause(true);
				return;
			}
			break;
		case HUDStartSurvivalSplash.State.Exit:
			if (this.m_Text.color.a > 0f)
			{
				Color color2 = this.m_Text.color;
				color2.a -= Time.deltaTime;
				color2.a = Mathf.Clamp01(color2.a);
				this.m_Text.color = color2;
				return;
			}
			if (this.m_CanvasGroup.alpha > 0f)
			{
				this.m_CanvasGroup.alpha -= Time.deltaTime;
				this.m_CanvasGroup.alpha = Mathf.Clamp01(this.m_CanvasGroup.alpha);
				if (this.m_CanvasGroup.alpha == 0f)
				{
					this.m_Active = false;
				}
			}
			break;
		default:
			return;
		}
	}

	public bool m_Active;

	public bool m_WasActive;

	private HUDStartSurvivalSplash.State m_State;

	public Text m_Text;

	public CanvasGroup m_CanvasGroup;

	private bool m_WasPause;

	private static HUDStartSurvivalSplash s_Instance;

	private enum State
	{
		Enter,
		Normal,
		Exit
	}
}
