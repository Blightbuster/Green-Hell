using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDWheel : HUDBase, IInputsReceiver
{
	public static HUDWheel Get()
	{
		return HUDWheel.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDWheel.s_Instance = this;
		for (int i = 0; i < 6; i++)
		{
			RawImage[] selections = this.m_Selections;
			int num = i;
			Transform transform = base.transform;
			HUDWheel.HUDWheelSlot hudwheelSlot = (HUDWheel.HUDWheelSlot)i;
			selections[num] = transform.Find(hudwheelSlot.ToString() + "Sel").GetComponent<RawImage>();
			RawImage[] icons = this.m_Icons;
			int num2 = i;
			Transform transform2 = base.transform;
			HUDWheel.HUDWheelSlot hudwheelSlot2 = (HUDWheel.HUDWheelSlot)i;
			icons[num2] = transform2.Find(hudwheelSlot2.ToString() + "Icon").GetComponent<RawImage>();
			RawImage[] iconsHL = this.m_IconsHL;
			int num3 = i;
			Transform transform3 = base.transform;
			HUDWheel.HUDWheelSlot hudwheelSlot3 = (HUDWheel.HUDWheelSlot)i;
			iconsHL[num3] = transform3.Find(hudwheelSlot3.ToString() + "IconHL").GetComponent<RawImage>();
		}
		this.m_CenterText = GreenHellGame.Instance.GetLocalization().Get("HUD_Wheel_Center");
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

	public bool ScenarioIsActive()
	{
		return this.m_Active;
	}

	protected override bool ShouldShow()
	{
		return this.m_Active && (!NotepadController.Get().IsActive() || NotepadController.Get().CanDisable());
	}

	protected override void OnShow()
	{
		base.OnShow();
		Player.Get().BlockRotation();
		CursorManager.Get().ShowCursor(true);
		Vector3 position = base.gameObject.transform.position;
		CursorManager.Get().SetCursorPos(position);
		for (int i = 0; i < 6; i++)
		{
			if (!this.IsSlotActive((HUDWheel.HUDWheelSlot)i))
			{
				this.m_Icons[i].color = this.m_InactiveColor;
			}
			else
			{
				this.m_Icons[i].color = this.m_ActiveColor;
			}
		}
		if (Inventory3DManager.Get().IsActive())
		{
			Inventory3DManager.Get().Deactivate();
		}
		if (BodyInspectionController.Get().IsActive())
		{
			BodyInspectionController.Get().Hide();
		}
		if (NotepadController.Get().IsActive())
		{
			NotepadController.Get().Hide();
		}
		if (MapController.Get().IsActive())
		{
			MapController.Get().Hide();
		}
	}

	private bool IsSlotActive(HUDWheel.HUDWheelSlot slot)
	{
		return (slot != HUDWheel.HUDWheelSlot.Craft || Player.Get().CanStartCrafting()) && (slot != HUDWheel.HUDWheelSlot.Notebook || Player.Get().CanShowNotepad()) && (slot != HUDWheel.HUDWheelSlot.Map || Player.Get().CanShowMap()) && (slot != HUDWheel.HUDWheelSlot.Sleep || Player.Get().CanSleep()) && (slot != HUDWheel.HUDWheelSlot.Inspect || Player.Get().CanStartBodyInspection());
	}

	protected override void OnHide()
	{
		base.OnHide();
		Player.Get().UnblockRotation();
		CursorManager.Get().ShowCursor(false);
	}

	public bool CanReceiveAction()
	{
		return true;
	}

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (action == InputsManager.InputAction.ShowWheel && !this.m_Active && Player.Get().CanInvokeWheelHUD())
		{
			this.Activate();
		}
		if (action == InputsManager.InputAction.HideWheel && this.m_Active)
		{
			this.Deactivate(true);
		}
	}

	private void Activate()
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		if (currentItem != null && currentItem.m_Info.IsHeavyObject())
		{
			Player.Get().DropItem(currentItem);
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		this.m_Active = true;
	}

	private void Deactivate(bool execute)
	{
		if (execute && this.IsSlotActive(this.m_SelectedSlot))
		{
			this.Execute();
		}
		this.m_Active = false;
		base.Show(false);
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		if (!this.m_Active)
		{
			return;
		}
		Player player = Player.Get();
		if (player.IsDead() || SwimController.Get().IsActive() || player.m_DreamActive)
		{
			this.Deactivate(false);
			return;
		}
		if (BodyInspectionMiniGameController.Get().IsActive())
		{
			this.Deactivate(false);
			return;
		}
		if (CutscenesManager.Get().IsCutscenePlaying())
		{
			this.Deactivate(false);
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			this.Execute();
			this.m_Active = false;
			base.Show(false);
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateSelection();
		if (!CursorManager.Get().IsCursorVisible())
		{
			CursorManager.Get().ShowCursor(true);
		}
	}

	private void UpdateSelection()
	{
		HUDWheel.HUDWheelSlot selectedSlot = this.m_SelectedSlot;
		float num = (float)Screen.width;
		Vector3 from = Input.mousePosition - base.transform.position;
		float num2 = from.magnitude / (num * 0.5f);
		if (num2 < 0.08f)
		{
			this.m_SelectedSlot = HUDWheel.HUDWheelSlot.None;
			for (int i = 0; i < 6; i++)
			{
				this.m_Selections[i].enabled = false;
				this.m_Icons[i].enabled = true;
				this.m_IconsHL[i].enabled = false;
			}
			this.UpdateText();
			return;
		}
		from.Normalize();
		float num3 = Vector3.Angle(from, Vector3.up);
		if (Input.mousePosition.x < base.transform.position.x)
		{
			num3 = 360f - num3;
		}
		this.m_SelectedSlot = (HUDWheel.HUDWheelSlot)(num3 / 360f * 6f);
		for (int j = 0; j < 6; j++)
		{
			this.m_Selections[j].enabled = (j == (int)this.m_SelectedSlot && this.IsSlotActive((HUDWheel.HUDWheelSlot)j));
			this.m_Icons[j].enabled = (j != (int)this.m_SelectedSlot || !this.IsSlotActive((HUDWheel.HUDWheelSlot)j));
			this.m_IconsHL[j].enabled = (j == (int)this.m_SelectedSlot && this.IsSlotActive((HUDWheel.HUDWheelSlot)j));
		}
		if (this.m_SelectedSlot == HUDWheel.HUDWheelSlot.Inspect && !Player.Get().CanStartBodyInspection())
		{
			this.m_SelectedSlot = HUDWheel.HUDWheelSlot.None;
			this.m_Selections[5].enabled = false;
		}
		this.UpdateText();
		if (this.m_SelectedSlot != HUDWheel.HUDWheelSlot.None && selectedSlot != this.m_SelectedSlot && this.m_Selections[(int)this.m_SelectedSlot].enabled)
		{
			if (this.m_AudioSource.isPlaying)
			{
				this.m_AudioSource.Stop();
			}
			this.m_AudioSource.Play();
		}
	}

	private void UpdateText()
	{
		if (this.m_SelectedSlot == HUDWheel.HUDWheelSlot.None)
		{
			this.m_Text.text = this.m_CenterText;
		}
		else
		{
			this.m_Text.text = GreenHellGame.Instance.GetLocalization().Get(this.m_SelectedSlot.ToString());
		}
	}

	private void Execute()
	{
		HUDWheel.HUDWheelSlot selectedSlot = this.m_SelectedSlot;
		switch (selectedSlot + 1)
		{
		case HUDWheel.HUDWheelSlot.Craft:
			this.OnMap();
			break;
		case HUDWheel.HUDWheelSlot.Sleep:
			this.OnCraft();
			break;
		case HUDWheel.HUDWheelSlot.Notebook:
			this.OnSleep();
			break;
		case HUDWheel.HUDWheelSlot.Backpack:
			this.OnNotebook();
			break;
		case HUDWheel.HUDWheelSlot.Inspect:
			this.OnBackpack();
			break;
		case HUDWheel.HUDWheelSlot.Count:
			this.OnInspect();
			break;
		}
	}

	private void OnMap()
	{
		if (!Player.Get().CanShowMap())
		{
			return;
		}
		Player.Get().StartController(PlayerControllerType.Map);
	}

	private void OnConstruct()
	{
		MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.ConstructionsTab, false);
		Player.Get().StartController(PlayerControllerType.Notepad);
	}

	private void OnCraft()
	{
		CraftingManager.Get().Activate();
	}

	private void OnSleep()
	{
		this.Deactivate(false);
		SleepController.Get().StartSleeping(null, true);
	}

	private void OnNotebook()
	{
		if (!Player.Get().CanShowNotepad())
		{
			return;
		}
		if (!Player.Get().GetComponent<NotepadController>().IsActive())
		{
			if (MenuNotepad.Get().m_ActiveTab == MenuNotepad.MenuNotepadTab.PlannerTab)
			{
				HUDPlanner.Get().m_PlannerMode = PlannerMode.ReadOnly;
			}
			Player.Get().StartController(PlayerControllerType.Notepad);
		}
	}

	private void OnBackpack()
	{
		Inventory3DManager.Get().Activate();
	}

	private void OnInspect()
	{
		if (!Player.Get().CanStartBodyInspection())
		{
			return;
		}
		if (!Player.Get().m_BodyInspectionController.IsActive())
		{
			Player.Get().StartController(PlayerControllerType.BodyInspection);
		}
	}

	private HUDWheel.HUDWheelSlot m_SelectedSlot = HUDWheel.HUDWheelSlot.None;

	private RawImage[] m_Selections = new RawImage[6];

	private RawImage[] m_Icons = new RawImage[6];

	private RawImage[] m_IconsHL = new RawImage[6];

	private bool m_Active;

	private static HUDWheel s_Instance;

	private Color m_ActiveColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private Color m_InactiveColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 47);

	public Text m_Text;

	private AudioSource m_AudioSource;

	private string m_CenterText = string.Empty;

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
