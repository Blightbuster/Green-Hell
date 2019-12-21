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
			hudwheelSlot = (HUDWheel.HUDWheelSlot)i;
			icons[num2] = transform2.Find(hudwheelSlot.ToString() + "Icon").GetComponent<RawImage>();
			RawImage[] iconsHL = this.m_IconsHL;
			int num3 = i;
			Transform transform3 = base.transform;
			hudwheelSlot = (HUDWheel.HUDWheelSlot)i;
			iconsHL[num3] = transform3.Find(hudwheelSlot.ToString() + "IconHL").GetComponent<RawImage>();
		}
		this.m_CenterText = GreenHellGame.Instance.GetLocalization().Get("HUD_Wheel_Center", true);
		this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
		this.m_AudioSource.playOnAwake = false;
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
		return this.m_Active && (!NotepadController.Get().IsActive() || NotepadController.Get().CanDisable()) && !ScenarioManager.Get().IsDreamOrPreDream();
	}

	protected override void OnShow()
	{
		base.OnShow();
		Player.Get().BlockRotation();
		if (GreenHellGame.IsPCControllerActive())
		{
			CursorManager.Get().ShowCursor(true, false);
			this.m_CursorVisible = true;
			Vector3 position = base.gameObject.transform.position;
			CursorManager.Get().SetCursorPos(position);
		}
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
		HUDItem.Get().Deactivate();
		this.m_SelectedSlot = HUDWheel.HUDWheelSlot.None;
		for (int j = 0; j < 6; j++)
		{
			this.m_Selections[j].enabled = false;
			this.m_Icons[j].enabled = true;
			this.m_IconsHL[j].enabled = false;
		}
		this.UpdateText();
	}

	private bool IsSlotActive(HUDWheel.HUDWheelSlot slot)
	{
		return (slot != HUDWheel.HUDWheelSlot.Craft || Player.Get().CanStartCrafting()) && (slot != HUDWheel.HUDWheelSlot.Notebook || Player.Get().CanShowNotepad()) && (slot != HUDWheel.HUDWheelSlot.Map || Player.Get().CanShowMap()) && (slot != HUDWheel.HUDWheelSlot.Sleep || Player.Get().CanSleep()) && (slot != HUDWheel.HUDWheelSlot.Inspect || Player.Get().CanStartBodyInspection()) && (slot != HUDWheel.HUDWheelSlot.Backpack || !Player.Get().m_Animator.GetBool(Player.Get().m_CleanUpHash));
	}

	protected override void OnHide()
	{
		base.OnHide();
		Player.Get().UnblockRotation();
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
		}
	}

	public bool CanReceiveAction()
	{
		return true;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.ShowWheel && !this.m_Active && Player.Get().CanInvokeWheelHUD())
		{
			this.Activate();
		}
		if (!base.enabled)
		{
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.HideWheel && this.m_Active)
		{
			this.Deactivate(true);
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.WheelSelect)
		{
			this.Execute();
			this.m_Active = false;
			base.Show(false);
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

	private void UpdateBatteryLevel()
	{
		this.m_WTBatteryLevel.text = (PlayerWalkieTalkieModule.Get().GetBatteryLevel() * 100f).ToString("F0");
		if (PlayerWalkieTalkieModule.Get().CanCall())
		{
			this.m_WTBatteryLevel.color = Color.white;
			return;
		}
		this.m_WTBatteryLevel.color = Color.red;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateSelection();
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
		if (!CursorManager.Get().IsCursorVisible() && GreenHellGame.IsPCControllerActive())
		{
			CursorManager.Get().ShowCursor(true, false);
			this.m_CursorVisible = true;
		}
	}

	private void UpdateSelection()
	{
		HUDWheel.HUDWheelSlot selectedSlot = this.m_SelectedSlot;
		if (GreenHellGame.IsPCControllerActive())
		{
			float num = (float)Screen.width;
			Vector3 from = Input.mousePosition - base.transform.position;
			if (from.magnitude / (num * 0.5f) < 0.08f)
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
			float num2 = Vector3.Angle(from, Vector3.up);
			if (Input.mousePosition.x < base.transform.position.x)
			{
				num2 = 360f - num2;
			}
			this.m_SelectedSlot = (HUDWheel.HUDWheelSlot)(num2 / 360f * 6f);
		}
		else
		{
			float axis = InputsManager.Get().GetAxis("RightStickX");
			float axis2 = InputsManager.Get().GetAxis("RightStickY");
			Vector2 zero = Vector2.zero;
			zero.x = axis;
			zero.y = axis2;
			if (zero.magnitude <= 0.1f)
			{
				this.m_DeadZoneDuration += Time.deltaTime;
				if (this.m_DeadZoneDuration >= 0.15f)
				{
					this.m_SelectedSlot = HUDWheel.HUDWheelSlot.None;
					for (int j = 0; j < 6; j++)
					{
						this.m_Selections[j].enabled = false;
						this.m_Icons[j].enabled = true;
						this.m_IconsHL[j].enabled = false;
					}
				}
				this.UpdateText();
				return;
			}
			this.m_DeadZoneDuration = 0f;
			float num3 = Vector3.Angle(zero, Vector3.up);
			if (axis > 0f)
			{
				num3 = 360f - num3;
			}
			this.m_SelectedSlot = (HUDWheel.HUDWheelSlot)(num3 / 360f * 6f);
		}
		for (int k = 0; k < 6; k++)
		{
			this.m_Selections[k].enabled = (k == (int)this.m_SelectedSlot && this.IsSlotActive((HUDWheel.HUDWheelSlot)k));
			this.m_Icons[k].enabled = (k != (int)this.m_SelectedSlot || !this.IsSlotActive((HUDWheel.HUDWheelSlot)k));
			this.m_IconsHL[k].enabled = (k == (int)this.m_SelectedSlot && this.IsSlotActive((HUDWheel.HUDWheelSlot)k));
		}
		if (HUDWheel.HUDWheelSlot.Inspect == this.m_SelectedSlot && !Player.Get().CanStartBodyInspection())
		{
			this.m_SelectedSlot = HUDWheel.HUDWheelSlot.None;
			this.m_Selections[5].enabled = false;
		}
		if (HUDWheel.HUDWheelSlot.Sleep == this.m_SelectedSlot && !Player.Get().CanSleep())
		{
			this.m_SelectedSlot = HUDWheel.HUDWheelSlot.None;
			this.m_Selections[2].enabled = false;
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
			return;
		}
		this.m_Text.text = GreenHellGame.Instance.GetLocalization().Get(this.m_SelectedSlot.ToString(), true);
	}

	private void Execute()
	{
		switch (this.m_SelectedSlot)
		{
		case HUDWheel.HUDWheelSlot.None:
			break;
		case HUDWheel.HUDWheelSlot.Map:
			this.OnMap();
			return;
		case HUDWheel.HUDWheelSlot.Craft:
			this.OnCraft();
			return;
		case HUDWheel.HUDWheelSlot.Sleep:
			this.OnSleep();
			return;
		case HUDWheel.HUDWheelSlot.Notebook:
			this.OnNotebook();
			return;
		case HUDWheel.HUDWheelSlot.Backpack:
			this.OnBackpack();
			return;
		case HUDWheel.HUDWheelSlot.Inspect:
			this.OnInspect();
			break;
		default:
			return;
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

	private void OnDialog()
	{
		if (!DialogsManager.Get().CanSelectDialog())
		{
			return;
		}
		HUDSelectDialog.Get().Activate();
		Player.Get().m_ShouldStartWalkieTalkieController = true;
		if (Player.Get().GetCurrentItem(Hand.Left) == null)
		{
			Player.Get().StartController(PlayerControllerType.WalkieTalkie);
		}
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
		if (!NotepadController.Get().IsActive())
		{
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

	[HideInInspector]
	public bool m_Active;

	private static HUDWheel s_Instance;

	private Color m_ActiveColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private Color m_InactiveColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 47);

	public Text m_Text;

	private AudioSource m_AudioSource;

	private string m_CenterText = string.Empty;

	public Text m_WTBatteryLevel;

	private bool m_CursorVisible;

	private float m_DeadZoneDuration;

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
