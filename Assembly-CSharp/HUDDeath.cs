using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDDeath : HUDBase, IYesNoDialogOwner, IInputsReceiver
{
	public static HUDDeath Get()
	{
		return HUDDeath.s_Instance;
	}

	protected override void Awake()
	{
		HUDDeath.s_Instance = this;
		this.ParseHintsScript();
	}

	private void ParseHintsScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Hints/HUDDeath_Hints", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Hint")
			{
				if (!this.m_Hints.Keys.Contains(key.GetVariable(0).SValue))
				{
					this.m_Hints.Add(key.GetVariable(0).SValue, new List<string>());
					this.m_Hints[key.GetVariable(0).SValue].Add(key.GetVariable(1).SValue);
				}
				else
				{
					this.m_Hints[key.GetVariable(0).SValue].Add(key.GetVariable(1).SValue);
				}
			}
		}
	}

	public void ClearStateAll()
	{
		this.m_DeathState = 0;
	}

	public void ClearState(HUDDeath.DeathState state)
	{
		this.m_DeathState &= (int)state;
	}

	public void SetState(HUDDeath.DeathState state)
	{
		this.m_DeathState |= (int)state;
	}

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
		this.SetDeathReason();
		this.SetupIcons();
		this.SetupHint();
		GameObject loadButton = this.m_LoadButton;
		if (loadButton != null)
		{
			loadButton.SetActive(!DifficultySettings.ActivePreset.m_PermaDeath && !GreenHellGame.Instance.IsGamescom() && ReplTools.IsPlayingAlone());
		}
		this.m_CoopButtonsGrp.SetActive(!ReplTools.IsPlayingAlone());
		this.m_ButtonsGrp.SetActive(ReplTools.IsPlayingAlone());
	}

	private void SetupHint()
	{
		List<string> list = null;
		if ((this.m_DeathState & 256) != 0)
		{
			list = this.m_Hints["Sanity"];
		}
		else if ((this.m_DeathState & 512) != 0)
		{
			list = this.m_Hints["Tribe"];
		}
		else if ((this.m_DeathState & 2048) != 0)
		{
			list = this.m_Hints["Caiman"];
		}
		else if ((this.m_DeathState & 4096) != 0)
		{
			list = this.m_Hints["Predator"];
		}
		else if ((this.m_DeathState & 8192) != 0)
		{
			list = this.m_Hints["Infection"];
		}
		else if ((this.m_DeathState & 64) != 0)
		{
			list = this.m_Hints["Wounds"];
		}
		else if ((this.m_DeathState & 4) != 0)
		{
			list = this.m_Hints["Poison"];
		}
		else if ((this.m_DeathState & 128) != 0)
		{
			list = this.m_Hints["Underwater"];
		}
		else if ((this.m_DeathState & 1) != 0)
		{
			list = this.m_Hints["Macro"];
		}
		else if ((this.m_DeathState & 32) != 0)
		{
			list = this.m_Hints["Insomnia"];
		}
		else if ((this.m_DeathState & 16) != 0)
		{
			list = this.m_Hints["ParasiteSickness"];
		}
		else if ((this.m_DeathState & 2) != 0)
		{
			list = this.m_Hints["FoodPoison"];
		}
		else if ((this.m_DeathState & 1024) != 0)
		{
			list = this.m_Hints["Fall"];
		}
		if (list != null && list.Count != 0)
		{
			this.m_HintText.enabled = true;
			this.m_HintText.text = GreenHellGame.Instance.GetLocalization().Get("HUDDeathHint", true) + GreenHellGame.Instance.GetLocalization().Get(list[UnityEngine.Random.Range(0, list.Count)], true);
			return;
		}
		this.m_HintText.enabled = false;
		if (list == null)
		{
			Debug.Log("HUDDeath::SetupHint list == null: m_DeathState = " + this.m_DeathState.ToString());
			return;
		}
		Debug.Log("HUDDeath::SetupHint list.Count == 0: m_DeathState = " + this.m_DeathState.ToString());
	}

	private void SetupIcons()
	{
		if ((this.m_DeathState & 1) != 0)
		{
			this.m_MacronutritientsIcon.gameObject.SetActive(true);
		}
		else
		{
			this.m_MacronutritientsIcon.gameObject.SetActive(false);
		}
		if ((this.m_DeathState & 2) != 0)
		{
			this.m_FoodPoison.gameObject.SetActive(true);
		}
		else
		{
			this.m_FoodPoison.gameObject.SetActive(false);
		}
		if ((this.m_DeathState & 4) != 0)
		{
			this.m_Poison.gameObject.SetActive(true);
		}
		else
		{
			this.m_Poison.gameObject.SetActive(false);
		}
		if ((this.m_DeathState & 8) != 0)
		{
			this.m_Fever.gameObject.SetActive(true);
		}
		else
		{
			this.m_Fever.gameObject.SetActive(false);
		}
		if ((this.m_DeathState & 16) != 0)
		{
			this.m_ParasiteSickness.gameObject.SetActive(true);
		}
		else
		{
			this.m_ParasiteSickness.gameObject.SetActive(false);
		}
		if ((this.m_DeathState & 32) != 0)
		{
			this.m_Insomnia.gameObject.SetActive(true);
		}
		else
		{
			this.m_Insomnia.gameObject.SetActive(false);
		}
		if ((this.m_DeathState & 64) != 0)
		{
			this.m_Wounds.gameObject.SetActive(true);
			return;
		}
		this.m_Wounds.gameObject.SetActive(false);
	}

	private void SetDeathReason()
	{
		switch (DeathController.Get().m_DeathType)
		{
		case DeathController.DeathType.UnderWater:
		case DeathController.DeathType.OnWater:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_UnderWater", true) + "</color>";
			return;
		case DeathController.DeathType.Caiman:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Caiman", true) + "</color>";
			return;
		case DeathController.DeathType.Predator:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Predator", true) + "</color>";
			return;
		case DeathController.DeathType.Cut:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Cut", true) + "</color>";
			return;
		case DeathController.DeathType.Fall:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Fall", true) + "</color>";
			return;
		case DeathController.DeathType.Insects:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Insects", true) + "</color>";
			return;
		case DeathController.DeathType.Melee:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Melee", true) + "</color>";
			return;
		case DeathController.DeathType.Poison:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Poison", true) + "</color>";
			return;
		case DeathController.DeathType.Thrust:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Thrust", true) + "</color>";
			return;
		case DeathController.DeathType.Infection:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Infection", true) + "</color>";
			return;
		case DeathController.DeathType.Piranha:
			this.m_DeathReason.enabled = true;
			this.m_DeathReason.text = GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Reason", true) + "<color=#b61919> " + GreenHellGame.Instance.GetLocalization().Get("HUD_Death_Piranha", true) + "</color>";
			return;
		default:
			this.m_DeathReason.enabled = false;
			return;
		}
	}

	private void SetupStatistics()
	{
		int ivalue = StatsManager.Get().GetStatistic(Enums.Event.DaysSurvived).IValue;
		this.m_DaysVal.text = ivalue.ToString() + ((ivalue == 1) ? " day" : " days");
		this.m_DistVal.text = StatsManager.Get().GetStatistic(Enums.Event.TraveledDist).FValue.ToString("F1") + " km";
		int ivalue2 = StatsManager.Get().GetStatistic(Enums.Event.Vomit).IValue;
		this.m_VomitVal.text = ivalue2.ToString() + ((ivalue2 == 1) ? " time" : " times");
	}

	protected override void OnHide()
	{
		base.OnHide();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
		}
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
			return;
		}
		if (this.m_CanvasGroup.alpha < 1f)
		{
			this.m_CanvasGroup.alpha += Time.deltaTime;
			this.m_CanvasGroup.alpha = Mathf.Clamp01(this.m_CanvasGroup.alpha);
			if (this.m_CanvasGroup.alpha == 1f && GreenHellGame.IsPCControllerActive())
			{
				CursorManager.Get().ShowCursor(true, true);
				this.m_CursorVisible = true;
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
		MenuInGameManager.Get().ShowScreen(typeof(LoadGameMenu));
	}

	public void OnLoadFromLastSave()
	{
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Warning", true), GreenHellGame.Instance.GetLocalization().Get("MenuLoadGame_Confirm", true), true);
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.LoadLastSave;
	}

	public void OnQuit()
	{
		if (DifficultySettings.ActivePreset.m_PermaDeath)
		{
			LoadingScreen.Get().Show(LoadingScreenState.ReturnToMainMenu);
			GreenHellGame.Instance.ReturnToMainMenu();
			return;
		}
		GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit", true), GreenHellGame.Instance.GetLocalization().Get("MenuInGame_Quit_Question", true), true);
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.Quit;
	}

	public void OnRespawn()
	{
		this.m_CoopButtonsGrp.SetActive(false);
		DeathController.Get().StartRespawn();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
	}

	public void OnYesFromDialog()
	{
		switch (this.m_YesNoQuestion)
		{
		case HUDDeath.YesNoQuestion.LoadGame:
			MenuInGameManager.Get().ShowScreen(typeof(LoadGameMenu));
			break;
		case HUDDeath.YesNoQuestion.LoadLastSave:
			DeathController.Get().StartRespawn();
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
			break;
		case HUDDeath.YesNoQuestion.Quit:
			LoadingScreen.Get().Show(LoadingScreenState.ReturnToMainMenu);
			GreenHellGame.Instance.ReturnToMainMenu();
			break;
		}
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	public void OnCloseDialog()
	{
	}

	public void OnNoFromDialog()
	{
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	public void OnOkFromDialog()
	{
		this.m_YesNoQuestion = HUDDeath.YesNoQuestion.None;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (GreenHellGame.GetYesNoDialog().gameObject.activeSelf)
		{
			return;
		}
		if (this.m_ButtonsGrp.activeSelf)
		{
			if (action_data.m_Action == InputsManager.InputAction.Button_A)
			{
				this.OnLoadGame();
			}
			else if (action_data.m_Action == InputsManager.InputAction.Button_B)
			{
				this.OnQuit();
			}
		}
		if (this.m_CoopButtonsGrp.activeSelf && action_data.m_Action == InputsManager.InputAction.Button_A)
		{
			this.OnRespawn();
		}
	}

	public bool CanReceiveAction()
	{
		return base.enabled;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	private HUDDeath.YesNoQuestion m_YesNoQuestion;

	public RawImage m_BG;

	public Text m_DaysVal;

	public Text m_DistVal;

	public Text m_VomitVal;

	public Text m_DeathReason;

	public CanvasGroup m_CanvasGroup;

	public static HUDDeath s_Instance;

	private int m_DeathState;

	public RawImage m_MacronutritientsIcon;

	public RawImage m_FoodPoison;

	public RawImage m_Poison;

	public RawImage m_Fever;

	public RawImage m_ParasiteSickness;

	public RawImage m_Insomnia;

	public RawImage m_Wounds;

	private Dictionary<string, List<string>> m_Hints = new Dictionary<string, List<string>>();

	public Text m_HintText;

	public GameObject m_LoadButton;

	public GameObject m_ButtonsGrp;

	public GameObject m_CoopButtonsGrp;

	private bool m_CursorVisible;

	private enum YesNoQuestion
	{
		None,
		LoadGame,
		LoadLastSave,
		Quit
	}

	public enum DeathState
	{
		None,
		Macronutritients,
		FoodPoison,
		Poison = 4,
		Fever = 8,
		ParasiteSickness = 16,
		Insomnia = 32,
		Wounds = 64,
		Drowning = 128,
		Sanity = 256,
		Tribe = 512,
		Fall = 1024,
		Caiman = 2048,
		Predator = 4096,
		Infection = 8192
	}
}
