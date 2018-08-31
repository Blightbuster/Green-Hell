using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDDeath : HUDBase, IYesNoDialogOwner
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return DeathController.Get().IsState(DeathController.DeathState.Death) && !ChallengesManager.Get().IsChallengeActive();
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.SetupStatistics();
		Color color = this.m_BG.color;
		color.a = 0f;
		this.m_BG.color = color;
		this.m_CanvasGroup.alpha = 0f;
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	private void SetupStatistics()
	{
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		this.m_DaysVal.text = ivalue.ToString() + ((ivalue != 1) ? " days" : " day");
		this.m_DistVal.text = StatsManager.Get().GetStatistic(Enums.Event.TraveledDist).FValue.ToString("F1") + " km";
		int ivalue2 = StatsManager.Get().GetStatistic(Enums.Event.Vomit).IValue;
		this.m_VomitVal.text = ivalue2.ToString() + ((ivalue2 != 1) ? " times" : " time");
	}

	protected override void OnHide()
	{
		base.OnHide();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		CursorManager.Get().ShowCursor(false);
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_BG.color.a < 1f)
		{
			Color color = this.m_BG.color;
			color.a += Time.deltaTime;
			color.a = Mathf.Clamp01(color.a);
			this.m_BG.color = color;
		}
		else if (this.m_CanvasGroup.alpha < 1f)
		{
			this.m_CanvasGroup.alpha += Time.deltaTime;
			this.m_CanvasGroup.alpha = Mathf.Clamp01(this.m_CanvasGroup.alpha);
			if (this.m_CanvasGroup.alpha == 1f)
			{
				CursorManager.Get().ShowCursor(true);
			}
		}
	}

	public void OnButtonEnter()
	{
		CursorManager.Get().SetCursor(CursorManager.TYPE.MouseOver);
	}

	public void OnButtonExit()
	{
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
	}

	public void OnLoadGame()
	{
		MenuInGameManager.Get().ShowScreen(typeof(MenuLoad));
	}

	public void OnLoadFromLastSave()
	{
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning"), GreenHellGame.Instance.GetLocalization().Get("MenuLoadGame_Confirm"), true);
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.LoadLastSave;
	}

	public void OnQuit()
	{
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit"), GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit_Question"), true);
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.Quit;
	}

	public void OnYesFromDialog()
	{
		HUDDeath.YesNoQuestion yesNoQuestion = this.m_YesNoQuestion;
		if (yesNoQuestion != HUDDeath.YesNoQuestion.Quit)
		{
			if (yesNoQuestion != HUDDeath.YesNoQuestion.LoadGame)
			{
				if (yesNoQuestion == HUDDeath.YesNoQuestion.LoadLastSave)
				{
					DeathController.Get().StartRespawn();
					CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
				}
			}
			else
			{
				MenuInGameManager.Get().ShowScreen(typeof(MenuLoad));
			}
		}
		else
		{
			LoadingScreen.Get().Show(LoadingScreenState.ReturnToMainMenu);
			GreenHellGame.Instance.ReturnToMainMenu();
		}
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	public void OnNoFromDialog()
	{
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	public void OnOkFromDialog()
	{
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	private HUDDeath.YesNoQuestion m_YesNoQuestion;

	public RawImage m_BG;

	public Text m_DaysVal;

	public Text m_DistVal;

	public Text m_VomitVal;

	public CanvasGroup m_CanvasGroup;

	private enum YesNoQuestion
	{
		None,
		LoadGame,
		LoadLastSave,
		Quit
	}
}
