using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDWalkieTalkie : HUDBase, IInputsReceiver
{
	public static HUDWalkieTalkie Get()
	{
		return HUDWalkieTalkie.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDWalkieTalkie.s_Instance = this;
		this.m_KeyText = base.transform.FindDeepChild("Key").gameObject.GetComponent<Text>();
		this.m_TalkText = base.transform.FindDeepChild("TalkText").gameObject;
		this.m_KeyFrame = base.transform.FindDeepChild("KeyFrame").gameObject;
		this.m_PadIcon = base.transform.FindDeepChild("PadIcon").gameObject;
		this.m_TextGen = new TextGenerator();
	}

	protected override void Start()
	{
		base.Start();
		this.m_InputData = InputsManager.Get().GetActionDataByInputAction(InputsManager.InputAction.ShowSelectDialogNode, ControllerType._Count);
		this.m_LastKeyCode = this.m_InputData.m_KeyCode;
	}

	protected override bool ShouldShow()
	{
		if (GreenHellGame.Instance.m_GameMode != GameMode.Story)
		{
			return false;
		}
		if (ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding"))
		{
			return false;
		}
		if (ChallengesManager.Get() && ChallengesManager.Get().IsChallengeActive())
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
		if (HUDWheel.Get().enabled)
		{
			return false;
		}
		if (FPPController.Get().m_Dodge)
		{
			return false;
		}
		int shortNameHash = Player.Get().m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
		return shortNameHash != this.m_MapWatchHideHash && shortNameHash != this.m_MapWatchIdleHash && shortNameHash != this.m_MapWatchShowHash && shortNameHash != this.m_MapZoomHash && shortNameHash != this.m_MapHideHash && shortNameHash != this.m_MapIdleHash && shortNameHash != this.m_ShowMapHash && (!DeathController.Get().IsActive() && !InsectsController.Get().IsActive() && !ScenarioManager.Get().IsDreamOrPreDream() && !Inventory3DManager.Get().IsActive() && !CutscenesManager.Get().IsCutscenePlaying() && !SwimController.Get().IsActive() && !BodyInspectionController.Get().IsActive() && !HarvestingAnimalController.Get().IsActive() && !HarvestingSmallAnimalController.Get().IsActive() && !VomitingController.Get().IsActive() && !MapController.Get().IsActive() && !NotepadController.Get().IsActive() && !MudMixerController.Get().IsActive()) && !MakeFireController.Get().IsActive();
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_NewDialogsObject.SetActive(false);
		float x = ((RectTransform)HUDManager.Get().m_CanvasGameObject.transform).localScale.x;
		Vector3 position = this.m_KeyFrame.transform.position;
		position.x = this.m_TalkText.transform.position.x;
		Text component = this.m_TalkText.GetComponent<Text>();
		TextGenerationSettings generationSettings = component.GetGenerationSettings(component.rectTransform.rect.size);
		generationSettings.scaleFactor = 1f;
		float num = this.m_TextGen.GetPreferredWidth(component.text, generationSettings) * x;
		position.x -= num;
		position.x -= this.m_KeyFrame.GetComponent<RectTransform>().sizeDelta.x * 1.1f * x;
		this.m_KeyFrame.transform.position = position;
		this.m_PadIcon.transform.position = position;
	}

	public void UpdateNewDialogsCounter()
	{
		this.m_NewDialogsCount = 0;
		foreach (List<Dialog> list in DialogsManager.Get().m_ScenarioDialogs.Values)
		{
			using (List<Dialog>.Enumerator enumerator2 = list.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (!enumerator2.Current.m_ShownInSelectDialog)
					{
						this.m_NewDialogsCount++;
					}
				}
			}
		}
		this.m_CounterText.text = this.m_NewDialogsCount.ToString();
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_LastKeyCode != this.m_InputData.m_KeyCode)
		{
			this.m_KeyText.text = KeyCodeToString.GetString(this.m_InputData.m_KeyCode);
			this.m_LastKeyCode = this.m_InputData.m_KeyCode;
		}
		if ((this.m_LastSelectDialogDeactivationTime == 0f || Time.time - this.m_LastSelectDialogDeactivationTime > 0.5f) && GreenHellGame.IsPCControllerActive())
		{
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.ShowSelectDialogNode) && !DialogsManager.Get().IsAnyDialogPlaying())
			{
				if (!HUDSelectDialog.Get().enabled && Player.Get().m_Animator.GetCurrentAnimatorStateInfo(2).shortNameHash == this.m_IdleHash && !MakeFireController.Get().IsActive())
				{
					HUDSelectDialog.Get().Activate();
					this.m_LastSelectDialogDeactivationTime = Time.time;
				}
			}
			else if (HUDSelectDialog.Get().enabled && !DialogsManager.Get().IsAnyDialogPlaying())
			{
				HUDSelectDialog.Get().Deactivate();
				this.m_LastSelectDialogDeactivationTime = Time.time;
			}
		}
		if (this.m_NewDialogsCount == 0 || HUDSelectDialog.Get().enabled || DialogsManager.Get().IsAnyDialogPlaying())
		{
			if (this.m_NewDialogsObject.activeSelf)
			{
				this.m_NewDialogsObject.SetActive(false);
				return;
			}
		}
		else if (!this.m_NewDialogsObject.activeSelf)
		{
			this.m_NewDialogsObject.SetActive(true);
		}
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.ShowSelectDialogNode && GreenHellGame.IsPadControllerActive())
		{
			if (this.m_LastSelectDialogDeactivationTime == 0f || Time.time - this.m_LastSelectDialogDeactivationTime > 0.5f)
			{
				if (!DialogsManager.Get().IsAnyDialogPlaying())
				{
					if (!HUDSelectDialog.Get().enabled && Player.Get().m_Animator.GetCurrentAnimatorStateInfo(2).shortNameHash == this.m_IdleHash && HUDSelectDialog.Get().CanShow())
					{
						HUDSelectDialog.Get().Activate();
						this.m_LastSelectDialogDeactivationTime = Time.time;
						return;
					}
				}
				else if (HUDSelectDialog.Get().enabled && !DialogsManager.Get().IsAnyDialogPlaying())
				{
					HUDSelectDialog.Get().Deactivate();
					this.m_LastSelectDialogDeactivationTime = Time.time;
					return;
				}
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.Button_B && HUDSelectDialog.Get().enabled)
		{
			HUDSelectDialog.Get().Deactivate();
			this.m_LastSelectDialogDeactivationTime = Time.time;
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

	public GameObject m_NewDialogsObject;

	public GameObject m_CounterObject;

	public Text m_CounterText;

	private int m_NewDialogsCount;

	private float m_LastSelectDialogDeactivationTime;

	private Text m_KeyText;

	private InputActionData m_InputData;

	private KeyCode m_LastKeyCode;

	private static HUDWalkieTalkie s_Instance;

	private GameObject m_TalkText;

	private GameObject m_KeyFrame;

	private GameObject m_PadIcon;

	private TextGenerator m_TextGen;

	private int m_MapWatchHideHash = Animator.StringToHash("MapWatchHide");

	private int m_MapWatchIdleHash = Animator.StringToHash("MapWatchIdle");

	private int m_MapWatchShowHash = Animator.StringToHash("MapWatchShow");

	private int m_MapZoomHash = Animator.StringToHash("MapZoom");

	private int m_MapHideHash = Animator.StringToHash("MapHide");

	private int m_MapIdleHash = Animator.StringToHash("MapIdle");

	private int m_ShowMapHash = Animator.StringToHash("ShowMap");

	private int m_IdleHash = Animator.StringToHash("Idle");
}
