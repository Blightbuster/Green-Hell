using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDSelectDialog : HUDBase, IInputsReceiver
{
	public static HUDSelectDialog Get()
	{
		return HUDSelectDialog.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDSelectDialog.s_Instance = this;
		this.m_HeaderSelectDialog = GreenHellGame.Instance.GetLocalization().Get("HUD_SelectDialog_Select", true);
		this.m_HeaderCharging = GreenHellGame.Instance.GetLocalization().Get("HUD_SelectDialog_Charging", true);
		this.m_NewText = GreenHellGame.Instance.GetLocalization().Get("HUD_SelectDialog_New", true);
		this.m_Scrollbar = base.transform.GetComponentInChildren<Scrollbar>();
	}

	public bool CanShow()
	{
		if (GreenHellGame.Instance.m_GameMode != GameMode.Story)
		{
			return false;
		}
		if (ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding"))
		{
			return false;
		}
		if (ConsciousnessController.Get().IsActive())
		{
			return false;
		}
		if (SleepController.Get().IsActive())
		{
			return false;
		}
		if (InsectsController.Get().IsActive())
		{
			return false;
		}
		if (InsectsController.Get().IsActive())
		{
			return false;
		}
		if (SwimController.Get().IsActive())
		{
			return false;
		}
		if (Player.Get().m_IsInAir)
		{
			return false;
		}
		if (ChallengesManager.Get() && ChallengesManager.Get().IsChallengeActive())
		{
			return false;
		}
		if (HUDReadableItem.Get().enabled)
		{
			return false;
		}
		if (Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash))
		{
			return false;
		}
		if (Time.time - SwimController.Get().m_LastDisableTime < 0.5f)
		{
			return false;
		}
		if (Player.Get().m_IsInAir)
		{
			return false;
		}
		if (DeathController.Get().IsActive())
		{
			return false;
		}
		if (ScenarioManager.Get().IsDreamOrPreDream())
		{
			return false;
		}
		if (Inventory3DManager.Get().IsActive())
		{
			return false;
		}
		if (CutscenesManager.Get().IsCutscenePlaying())
		{
			return false;
		}
		if (SwimController.Get().IsActive())
		{
			return false;
		}
		if (BodyInspectionController.Get().IsActive())
		{
			return false;
		}
		if (HarvestingAnimalController.Get().IsActive())
		{
			return false;
		}
		if (HarvestingSmallAnimalController.Get().IsActive())
		{
			return false;
		}
		if (VomitingController.Get().IsActive())
		{
			return false;
		}
		if (MapController.Get().IsActive())
		{
			return false;
		}
		if (NotepadController.Get().IsActive())
		{
			return false;
		}
		if (MudMixerController.Get().IsActive())
		{
			return false;
		}
		if (MakeFireController.Get().IsActive())
		{
			return false;
		}
		if (HUDWheel.Get().enabled)
		{
			return false;
		}
		if (FPPController.Get().m_Dodge)
		{
			return false;
		}
		if (GreenHellGame.IsPadControllerActive() && HUDItem.Get().m_Active)
		{
			return false;
		}
		int shortNameHash = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
		return shortNameHash != this.m_MapWatchHideHash && shortNameHash != this.m_MapWatchIdleHash && shortNameHash != this.m_MapWatchShowHash && shortNameHash != this.m_MapZoomHash && shortNameHash != this.m_MapHideHash && shortNameHash != this.m_MapIdleHash && shortNameHash != this.m_ShowMapHash;
	}

	protected override bool ShouldShow()
	{
		return this.m_Active && this.CanShow();
	}

	public void Activate()
	{
		this.m_Active = true;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.SetupDialogs();
		this.SetupScroll();
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
		if (GreenHellGame.IsPCControllerActive())
		{
			CursorManager.Get().ShowCursor(CursorManager.TYPE.Normal);
			this.m_CursorVisible = true;
		}
		HUDItem.Get().Deactivate();
		this.m_InGroup = false;
		this.m_MarkedData = null;
		Player.Get().m_ShouldStartWalkieTalkieController = true;
		if (Player.Get().GetCurrentItem(Hand.Left) == null)
		{
			Player.Get().StartController(PlayerControllerType.WalkieTalkie);
		}
		this.m_TempSanityTexts.Clear();
		this.m_TempSanityTexts.AddRange(this.m_SanityTexts);
		this.m_BackButtonObject.SetActive(false);
		this.SetupScroll();
		this.m_PadSelectionIndex = -1;
	}

	public void Deactivate()
	{
		if (!this.m_Active)
		{
			return;
		}
		this.m_Active = false;
		this.ClearDatas();
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
			this.m_CursorVisible = false;
		}
		if (this.m_MarkedData != null)
		{
			if (!this.m_MarkedData.m_IsGroup && GreenHellGame.IsPCControllerActive())
			{
				this.OnSelectDialog(this.m_MarkedData);
			}
			this.m_MarkedData = null;
		}
		if (!DialogsManager.Get().IsAnyDialogPlaying())
		{
			WalkieTalkieController.Get().OnStopDialog(null);
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.m_BackButtonObject.SetActive(false);
		this.Deactivate();
	}

	private void ClearDatas()
	{
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			UnityEngine.Object.Destroy(this.m_Datas[i].m_Object);
		}
		this.m_Datas.Clear();
	}

	private void SetupDialogs()
	{
		this.ClearDatas();
		foreach (string text in DialogsManager.Get().m_ScenarioDialogs.Keys)
		{
			List<Dialog> list = DialogsManager.Get().m_ScenarioDialogs[text];
			if (list.Count != 0)
			{
				HUDSelectDialogElemData hudselectDialogElemData;
				if (text == "ALL")
				{
					using (List<Dialog>.Enumerator enumerator2 = list.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Dialog dialog = enumerator2.Current;
							hudselectDialogElemData = this.CreateElement(dialog.m_Name, !dialog.m_ShownInSelectDialog);
							if (!PlayerSanityModule.Get().IsWhispersLevel())
							{
								dialog.m_ShownInSelectDialog = true;
							}
							this.m_Datas.Add(hudselectDialogElemData);
						}
						continue;
					}
				}
				bool show_new = false;
				using (List<Dialog>.Enumerator enumerator2 = list.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (!enumerator2.Current.m_ShownInSelectDialog)
						{
							show_new = true;
							break;
						}
					}
				}
				hudselectDialogElemData = this.CreateElement(text, show_new);
				hudselectDialogElemData.m_IsGroup = true;
				hudselectDialogElemData.m_Dialogs = list;
				this.m_Datas.Add(hudselectDialogElemData);
			}
		}
		this.m_Datas.Sort(HUDSelectDialog.s_NewComparer);
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			this.m_Datas[i].m_Object.transform.localPosition = Vector3.zero + Vector3.up * 40f - Vector3.up * (this.m_Datas[i].m_BG.rectTransform.sizeDelta.y + 2f) * (float)i;
		}
		HUDWalkieTalkie.Get().UpdateNewDialogsCounter();
		this.UpdateVis();
	}

	private void UpdateBatteryLevel()
	{
		this.m_BatteryLevel.text = (PlayerWalkieTalkieModule.Get().GetBatteryLevel() * 100f).ToString("F0") + "%";
		if (PlayerWalkieTalkieModule.Get().CanCall())
		{
			this.m_BatteryLevel.color = Color.white;
			return;
		}
		this.m_BatteryLevel.color = Color.red;
	}

	private void UpdateHeaderText()
	{
		if (!PlayerWalkieTalkieModule.Get().CanCall())
		{
			this.m_HeaderText.text = this.m_HeaderCharging;
			return;
		}
		if (this.m_InGroup)
		{
			Localization localization = GreenHellGame.Instance.GetLocalization();
			this.m_HeaderText.text = localization.Get("HUD_" + this.m_LastGroupName, true);
			return;
		}
		this.m_HeaderText.text = this.m_HeaderSelectDialog;
	}

	private void SetupScroll()
	{
		if (this.m_Datas.Count <= this.m_MaxElements)
		{
			this.m_Scrollbar.gameObject.SetActive(false);
			return;
		}
		this.m_Scrollbar.gameObject.SetActive(true);
		this.m_Scrollbar.numberOfSteps = this.m_Datas.Count - this.m_MaxElements + 1;
		this.m_Scrollbar.value = 0f;
		this.m_ScrollStep = 0;
		this.m_ScrollValue = 0f;
		this.ApplyScroll();
	}

	private void UpdateScroll()
	{
		if (!this.m_Scrollbar.gameObject.activeSelf)
		{
			return;
		}
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			if (this.m_ScrollStep < this.m_Datas.Count - this.m_MaxElements)
			{
				this.m_ScrollStep++;
				this.ApplyScroll();
				return;
			}
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			if (this.m_ScrollStep > 0)
			{
				this.m_ScrollStep--;
				this.ApplyScroll();
				return;
			}
		}
		else if (this.m_Scrollbar.value != this.m_ScrollValue)
		{
			float num = 1f / (float)(this.m_Datas.Count - this.m_MaxElements);
			if (this.m_Scrollbar.value > this.m_ScrollValue + num * (float)this.m_ScrollStep * 0.5f)
			{
				this.m_ScrollStep++;
			}
			else if (this.m_Scrollbar.value < this.m_ScrollValue - num * (float)this.m_ScrollStep * 0.5f)
			{
				this.m_ScrollStep--;
			}
			this.ApplyScroll();
		}
	}

	private void ApplyScroll()
	{
		float num = 1f / (float)(this.m_Datas.Count - this.m_MaxElements);
		this.m_ScrollValue = num * (float)this.m_ScrollStep;
		this.m_Scrollbar.value = this.m_ScrollValue;
		int num2 = 0;
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			this.m_Datas[i].m_Object.SetActive(i >= this.m_ScrollStep && num2 < this.m_MaxElements);
			this.m_Datas[i].m_Object.transform.localPosition = Vector3.zero + Vector3.up * 40f - Vector3.up * (this.m_Datas[i].m_BG.rectTransform.sizeDelta.y + 2f) * (float)num2;
			if (this.m_Datas[i].m_Object.activeSelf)
			{
				num2++;
			}
		}
	}

	private void UpdateVis()
	{
		Color color = Color.white;
		bool flag = PlayerWalkieTalkieModule.Get().CanCall();
		if (GreenHellGame.IsPCControllerActive())
		{
			using (List<HUDSelectDialogElemData>.Enumerator enumerator = this.m_Datas.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					HUDSelectDialogElemData hudselectDialogElemData = enumerator.Current;
					if (flag && RectTransformUtility.RectangleContainsScreenPoint(hudselectDialogElemData.m_BG.rectTransform, Input.mousePosition))
					{
						this.m_MarkedData = hudselectDialogElemData;
						hudselectDialogElemData.m_BG.gameObject.SetActive(true);
					}
					else
					{
						hudselectDialogElemData.m_BG.gameObject.SetActive(false);
					}
					color = hudselectDialogElemData.m_Text.color;
					color.a = (flag ? 1f : this.m_InactiveAlpha);
					hudselectDialogElemData.m_Text.color = color;
				}
				goto IL_194;
			}
		}
		if (this.m_PadSelectionIndex < 0)
		{
			this.m_PadSelectionIndex = 0;
		}
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			HUDSelectDialogElemData hudselectDialogElemData2 = this.m_Datas[i];
			if (flag && i == this.m_PadSelectionIndex)
			{
				this.m_MarkedData = hudselectDialogElemData2;
				hudselectDialogElemData2.m_BG.gameObject.SetActive(true);
			}
			else
			{
				hudselectDialogElemData2.m_BG.gameObject.SetActive(false);
			}
			hudselectDialogElemData2.m_PadIconSelect.SetActive(hudselectDialogElemData2.m_BG.gameObject.activeSelf);
			color = hudselectDialogElemData2.m_Text.color;
			color.a = (flag ? 1f : this.m_InactiveAlpha);
			hudselectDialogElemData2.m_Text.color = color;
		}
		IL_194:
		this.UpdateHeaderText();
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateScroll();
		this.m_MarkedData = null;
		this.UpdateVis();
		bool flag = PlayerWalkieTalkieModule.Get().CanCall();
		this.m_BackBG.gameObject.SetActive(this.m_MarkedData == null && RectTransformUtility.RectangleContainsScreenPoint(this.m_BackBG.rectTransform, Input.mousePosition));
		CursorManager.Get().SetCursor((this.m_MarkedData != null || this.m_BackBG.gameObject.activeSelf) ? CursorManager.TYPE.MouseOver : CursorManager.TYPE.Normal);
		if (flag && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(InputHelpers.PadButton.Button_X.KeyFromPad())))
		{
			if (this.m_MarkedData != null)
			{
				this.OnSelectDialog(this.m_MarkedData);
			}
			else if (this.m_BackBG.gameObject.activeSelf)
			{
				if (this.m_InGroup)
				{
					this.SetupDialogs();
					this.m_InGroup = false;
				}
				else if (WalkieTalkieController.Get().IsActive())
				{
					WalkieTalkieController.Get().Stop();
				}
			}
		}
		this.UpdateBatteryLevel();
		this.UpdateHeaderText();
		Color color = this.m_WTIcon.color;
		color.a = (flag ? 1f : this.m_InactiveAlpha);
		this.m_WTIcon.color = color;
		this.UpdateBackButton();
	}

	private void UpdateBackButton()
	{
		if (this.m_InGroup && !this.m_BackButtonObject.activeSelf)
		{
			this.m_BackButtonObject.SetActive(true);
			return;
		}
		if (!this.m_InGroup && this.m_BackButtonObject.activeSelf)
		{
			this.m_BackButtonObject.SetActive(false);
		}
	}

	private void OnSelectDialog(HUDSelectDialogElemData data)
	{
		if (data.m_Dialogs != null)
		{
			this.m_LastGroupName = data.m_DialogName;
			this.SetupGroup(data.m_Dialogs);
			return;
		}
		DialogsManager.Get().StartDialog(data.m_DialogName);
		PlayerWalkieTalkieModule.Get().OnCall();
		this.m_MarkedData = null;
		this.m_LastSelectDialogTime = Time.time;
		this.Deactivate();
	}

	private void SetupGroup(List<Dialog> dialogs)
	{
		this.ClearDatas();
		foreach (Dialog dialog in dialogs)
		{
			this.m_Datas.Add(this.CreateElement(dialog.m_Name, !dialog.m_ShownInSelectDialog));
			if (!PlayerSanityModule.Get().IsWhispersLevel())
			{
				dialog.m_ShownInSelectDialog = true;
			}
		}
		this.m_Datas.Sort(HUDSelectDialog.s_NewComparer);
		for (int i = 0; i < this.m_Datas.Count; i++)
		{
			this.m_Datas[i].m_Object.transform.localPosition = Vector3.zero + Vector3.up * 40f - Vector3.up * (this.m_Datas[i].m_BG.rectTransform.sizeDelta.y + 2f) * (float)i;
		}
		this.m_InGroup = true;
		HUDWalkieTalkie.Get().UpdateNewDialogsCounter();
		this.UpdateVis();
	}

	private HUDSelectDialogElemData CreateElement(string name, bool show_new)
	{
		HUDSelectDialogElemData hudselectDialogElemData = new HUDSelectDialogElemData();
		hudselectDialogElemData.m_Object = UnityEngine.Object.Instantiate<GameObject>(this.m_Prefab, base.transform);
		hudselectDialogElemData.m_DialogName = name;
		Localization localization = GreenHellGame.Instance.GetLocalization();
		hudselectDialogElemData.m_Text = hudselectDialogElemData.m_Object.GetComponentInChildren<Text>();
		if (PlayerSanityModule.Get().IsWhispersLevel())
		{
			if (this.m_TempSanityTexts.Count == 0)
			{
				this.m_TempSanityTexts.AddRange(this.m_SanityTexts);
			}
			int index = UnityEngine.Random.Range(0, this.m_TempSanityTexts.Count);
			string key = this.m_TempSanityTexts[index];
			hudselectDialogElemData.m_Text.text = localization.Get(key, true);
			this.m_TempSanityTexts.RemoveAt(index);
		}
		else
		{
			hudselectDialogElemData.m_Text.text = localization.Get("HUD_" + hudselectDialogElemData.m_DialogName, true);
		}
		hudselectDialogElemData.m_BG = hudselectDialogElemData.m_Object.FindChild("BG").GetComponent<RawImage>();
		hudselectDialogElemData.m_BG.gameObject.SetActive(false);
		hudselectDialogElemData.m_PadIconSelect = hudselectDialogElemData.m_Object.FindChild("PadIconSelect").gameObject;
		hudselectDialogElemData.m_PadIconSelect.SetActive(false);
		hudselectDialogElemData.m_CounterObject = hudselectDialogElemData.m_Object.FindChild("Counter").gameObject;
		if (show_new)
		{
			hudselectDialogElemData.m_CounterObject.SetActive(true);
			hudselectDialogElemData.m_CounterObject.GetComponentInChildren<Text>().text = this.m_NewText;
		}
		else
		{
			hudselectDialogElemData.m_CounterObject.SetActive(false);
		}
		return hudselectDialogElemData;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if ((action_data.m_Action == InputsManager.InputAction.LSBackward || action_data.m_Action == InputsManager.InputAction.DPadDown) && this.m_PadSelectionIndex < this.m_Datas.Count - 1)
		{
			this.m_PadSelectionIndex++;
			if (this.m_PadSelectionIndex >= this.m_MaxElements)
			{
				this.m_ScrollStep++;
				this.ApplyScroll();
			}
		}
		if ((action_data.m_Action == InputsManager.InputAction.LSForward || action_data.m_Action == InputsManager.InputAction.DPadUp) && this.m_PadSelectionIndex > 0)
		{
			this.m_PadSelectionIndex--;
			if (this.m_PadSelectionIndex < this.m_ScrollStep)
			{
				this.m_ScrollStep--;
				this.ApplyScroll();
			}
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

	public GameObject m_Prefab;

	public RawImage m_BG;

	public RawImage m_BackBG;

	private bool m_Active;

	private bool m_InGroup;

	private string m_LastGroupName = string.Empty;

	private List<HUDSelectDialogElemData> m_Datas = new List<HUDSelectDialogElemData>();

	public Text m_BatteryLevel;

	public Text m_HeaderText;

	private string m_HeaderSelectDialog = string.Empty;

	private string m_HeaderCharging = string.Empty;

	private string m_NewText = string.Empty;

	public RawImage m_WTIcon;

	public float m_InactiveAlpha = 0.75f;

	private HUDSelectDialogElemData m_MarkedData;

	public GameObject m_BackButtonObject;

	private Scrollbar m_Scrollbar;

	private int m_ScrollStep;

	private float m_ScrollValue;

	public int m_MaxElements = 12;

	private static HUDSelectDialog s_Instance = null;

	private static CompareListByNew s_NewComparer = new CompareListByNew();

	public List<string> m_SanityTexts = new List<string>();

	public List<string> m_TempSanityTexts = new List<string>();

	[HideInInspector]
	public float m_LastSelectDialogTime;

	private bool m_CursorVisible;

	private int m_PadSelectionIndex = -1;

	private int m_MapWatchHideHash = Animator.StringToHash("MapWatchHide");

	private int m_MapWatchIdleHash = Animator.StringToHash("MapWatchIdle");

	private int m_MapWatchShowHash = Animator.StringToHash("MapWatchShow");

	private int m_MapZoomHash = Animator.StringToHash("MapZoom");

	private int m_MapHideHash = Animator.StringToHash("MapHide");

	private int m_MapIdleHash = Animator.StringToHash("MapIdle");

	private int m_ShowMapHash = Animator.StringToHash("ShowMap");
}
