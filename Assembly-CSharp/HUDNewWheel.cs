using System;
using UnityEngine;

public class HUDNewWheel : HUDBase
{
	public static HUDNewWheel Get()
	{
		return HUDNewWheel.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDNewWheel.s_Instance = this;
		for (int i = 0; i < 6; i++)
		{
			int ii = i;
			NewWheelButton[] buttons = this.m_Buttons;
			int num = i;
			Transform transform = base.transform;
			string str = "Icons/";
			HUDNewWheel.HUDWheelSlot hudwheelSlot = (HUDNewWheel.HUDWheelSlot)i;
			buttons[num] = transform.Find(str + hudwheelSlot.ToString()).GetComponent<NewWheelButton>();
			this.m_Buttons[i].onClick.AddListener(delegate
			{
				this.Execute((HUDNewWheel.HUDWheelSlot)ii);
			});
		}
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		AudioClip audioClip = Resources.Load<AudioClip>("Sounds/wheel_menu");
		if (!audioClip)
		{
			audioClip = Resources.Load<AudioClip>("Sounds/TempSounds/wheel_menu");
		}
		this.m_AudioSource.clip = audioClip;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	public bool IsSelected()
	{
		if (!base.enabled)
		{
			return false;
		}
		for (int i = 0; i < 6; i++)
		{
			if (this.m_Buttons[i].m_Selected)
			{
				return true;
			}
		}
		return false;
	}

	public bool ScenarioIsActive()
	{
		return base.enabled;
	}

	protected override bool ShouldShow()
	{
		return !Player.Get().IsDead() && !Player.Get().m_DreamActive && !CutscenesManager.Get().IsCutscenePlaying() && !SleepController.Get().IsActive() && !HUDWheel.Get().enabled && (Inventory3DManager.Get().gameObject.activeSelf || NotepadController.Get().IsActive() || MapController.Get().IsActive() || BodyInspectionController.Get().IsActive());
	}

	protected override void OnShow()
	{
		base.OnShow();
		for (int i = 0; i < 6; i++)
		{
			this.m_Buttons[i].ResetAll();
		}
	}

	private bool IsSlotActive(HUDNewWheel.HUDWheelSlot slot)
	{
		return (slot != HUDNewWheel.HUDWheelSlot.Craft || Player.Get().CanStartCrafting()) && (slot != HUDNewWheel.HUDWheelSlot.Notebook || NotepadController.Get().IsActive() || Player.Get().CanShowNotepad()) && (slot != HUDNewWheel.HUDWheelSlot.Map || MapController.Get().IsActive() || Player.Get().CanShowMap()) && (slot != HUDNewWheel.HUDWheelSlot.Sleep || Player.Get().CanSleep()) && (slot != HUDNewWheel.HUDWheelSlot.Inspect || BodyInspectionController.Get().IsActive() || Player.Get().CanStartBodyInspection());
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	protected override void Update()
	{
		base.Update();
		this.SetupSelections();
		this.UpdateSelection();
	}

	private void SetupSelections()
	{
		Color color = this.m_Buttons[0].m_Icon.color;
		color.a = (Player.Get().CanStartCrafting() ? ((!CraftingManager.Get().gameObject.activeSelf) ? this.m_NormalAlpha : this.m_SelectedAlpha) : this.m_InactiveAlpha);
		this.m_Buttons[0].m_Icon.color = color;
		color = this.m_Buttons[3].m_Icon.color;
		color.a = (Player.Get().CanShowNotepad() ? ((!NotepadController.Get().IsActive()) ? this.m_NormalAlpha : this.m_SelectedAlpha) : this.m_InactiveAlpha);
		this.m_Buttons[3].m_Icon.color = color;
		color = this.m_Buttons[4].m_Icon.color;
		color.a = ((!Inventory3DManager.Get().gameObject.activeSelf) ? this.m_NormalAlpha : this.m_SelectedAlpha);
		this.m_Buttons[4].m_Icon.color = color;
		color = this.m_Buttons[5].m_Icon.color;
		color.a = (Player.Get().CanStartBodyInspection() ? ((!BodyInspectionController.Get().IsActive()) ? this.m_NormalAlpha : this.m_SelectedAlpha) : this.m_InactiveAlpha);
		this.m_Buttons[5].m_Icon.color = color;
		color = this.m_Buttons[1].m_Icon.color;
		color.a = (Player.Get().CanStartCrafting() ? ((!CraftingManager.Get().IsActive()) ? this.m_NormalAlpha : this.m_SelectedAlpha) : this.m_InactiveAlpha);
		this.m_Buttons[1].m_Icon.color = color;
		color = this.m_Buttons[0].m_Icon.color;
		color.a = (Player.Get().CanShowMap() ? ((!MapController.Get().IsActive()) ? this.m_NormalAlpha : this.m_SelectedAlpha) : this.m_InactiveAlpha);
		this.m_Buttons[0].m_Icon.color = color;
		color = this.m_Buttons[2].m_Icon.color;
		color.a = (Player.Get().CanSleep() ? ((!SleepController.Get().IsActive()) ? this.m_NormalAlpha : this.m_SelectedAlpha) : this.m_InactiveAlpha);
		this.m_Buttons[2].m_Icon.color = color;
	}

	private void UpdateSelection()
	{
		for (int i = 0; i < 6; i++)
		{
			this.m_Buttons[i].interactable = this.IsSlotActive((HUDNewWheel.HUDWheelSlot)i);
		}
	}

	private void Execute(HUDNewWheel.HUDWheelSlot slot)
	{
		switch (slot + 1)
		{
		case HUDNewWheel.HUDWheelSlot.Craft:
			this.OnMap();
			break;
		case HUDNewWheel.HUDWheelSlot.Sleep:
			this.OnCraft();
			break;
		case HUDNewWheel.HUDWheelSlot.Notebook:
			this.OnSleep();
			break;
		case HUDNewWheel.HUDWheelSlot.Backpack:
			this.OnNotebook();
			break;
		case HUDNewWheel.HUDWheelSlot.Inspect:
			this.OnBackpack();
			break;
		case HUDNewWheel.HUDWheelSlot.Count:
			this.OnInspect();
			break;
		}
	}

	private void OnMap()
	{
		if (MapController.Get().IsActive())
		{
			MapController.Get().Hide();
		}
		else
		{
			if (!Player.Get().CanShowMap())
			{
				return;
			}
			Player.Get().StartController(PlayerControllerType.Map);
		}
	}

	private void OnCraft()
	{
		if (CraftingManager.Get().IsActive())
		{
			CraftingManager.Get().Deactivate();
		}
		else
		{
			Inventory3DManager.Get().Activate();
			CraftingManager.Get().Activate();
		}
	}

	private void OnSleep()
	{
		SleepController.Get().StartSleeping(null, true);
	}

	private void OnNotebook()
	{
		if (NotepadController.Get().IsActive())
		{
			NotepadController.Get().Hide();
		}
		else
		{
			if (MenuNotepad.Get().m_ActiveTab == MenuNotepad.MenuNotepadTab.PlannerTab)
			{
				HUDPlanner.Get().m_PlannerMode = PlannerMode.ReadOnly;
			}
			if (Inventory3DManager.Get().IsActive())
			{
				Inventory3DManager.Get().Deactivate();
			}
			Player.Get().StartController(PlayerControllerType.Notepad);
		}
	}

	private void OnBackpack()
	{
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		else
		{
			Inventory3DManager.Get().Activate();
		}
	}

	private void OnInspect()
	{
		if (BodyInspectionController.Get().IsActive())
		{
			BodyInspectionController.Get().Stop();
		}
		else
		{
			Player.Get().StartController(PlayerControllerType.BodyInspection);
			if (CraftingManager.Get().IsActive())
			{
				CraftingManager.Get().Deactivate();
			}
		}
	}

	private NewWheelButton[] m_Buttons = new NewWheelButton[6];

	private static HUDNewWheel s_Instance;

	private AudioSource m_AudioSource;

	public float m_NormalAlpha = 0.3f;

	public float m_SelectedAlpha = 0.8f;

	public float m_InactiveAlpha = 0.1f;

	private enum HUDWheelSlot
	{
		None = -1,
		Map,
		Craft,
		Sleep,
		Notebook,
		Backpack,
		Inspect,
		Count
	}
}
