using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDSleeping : HUDBase
{
	public static HUDSleeping Get()
	{
		return HUDSleeping.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Awake()
	{
		HUDSleeping.s_Instance = this;
		base.Awake();
		this.m_Time = base.transform.FindDeepChild("Time").GetComponent<Text>();
		this.m_StartTime = base.transform.FindDeepChild("StartTime").GetComponent<Text>();
		this.m_EndTime = base.transform.FindDeepChild("EndTime").GetComponent<Text>();
		this.m_CanvasGroup = base.GetComponentInChildren<CanvasGroup>();
		this.m_CanvasGroup.alpha = 0f;
		this.m_Progress = base.transform.FindDeepChild("ProgressFill").GetComponent<Image>();
	}

	protected override bool ShouldShow()
	{
		return SleepController.Get().IsSleeping() && !Player.Get().m_DreamActive;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_CanvasGroup.alpha = 0f;
		int num = (int)MainLevel.Instance.m_TODSky.Cycle.Hour;
		int num2 = (int)((MainLevel.Instance.m_TODSky.Cycle.Hour - (float)num) * 60f);
		string text = string.Format("{0}:{1:00}", num, num2);
		this.m_StartTime.text = text;
		num += SleepController.Get().m_SleepDuration;
		if (num >= 24)
		{
			num -= 24;
		}
		text = string.Format("{0}:{1:00}", num, num2);
		this.m_EndTime.text = text;
		this.SetBGAlpha(0f);
		CursorManager.Get().ShowCursor(true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		CursorManager.Get().ShowCursor(false);
	}

	private void SetBGAlpha(float alpha)
	{
		if (alpha == this.m_BG.color.a)
		{
			return;
		}
		alpha = Mathf.Clamp01(alpha);
		Color color = this.m_BG.color;
		color.a = alpha;
		this.m_BG.color = color;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateAlpha();
		this.UpdateProgress();
		this.UpdateTime();
	}

	private void UpdateAlpha()
	{
		if (this.m_BG.color.a < 1f)
		{
			this.SetBGAlpha(this.m_BG.color.a + Time.deltaTime);
		}
		else if (this.m_CanvasGroup.alpha < 1f)
		{
			this.m_CanvasGroup.alpha += Time.deltaTime;
			this.m_CanvasGroup.alpha = Mathf.Clamp01(this.m_CanvasGroup.alpha);
		}
	}

	protected override void OnEnable()
	{
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
	}

	public void OnWakeUpButton()
	{
		SleepController.Get().WakeUp(false, true);
	}

	private void UpdateProgress()
	{
		this.m_Progress.fillAmount = SleepController.Get().m_Progress;
	}

	private void UpdateTime()
	{
		int num = (int)MainLevel.Instance.m_TODSky.Cycle.Hour;
		int num2 = (int)((MainLevel.Instance.m_TODSky.Cycle.Hour - (float)num) * 60f);
		string text = string.Format("{0}:{1:00}", num, num2);
		this.m_Time.text = text;
	}

	public void OnButtonEnter()
	{
		CursorManager.Get().SetCursor(CursorManager.TYPE.MouseOver);
	}

	public void OnButtonExit()
	{
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
	}

	private Text m_Time;

	private Text m_StartTime;

	private Text m_EndTime;

	private Image m_Progress;

	public RawImage m_BG;

	private CanvasGroup m_CanvasGroup;

	private static HUDSleeping s_Instance;
}
