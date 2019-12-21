using System;
using CJTools;
using Enums;
using UnityEngine;

public class MakeFireController : PlayerController
{
	public static MakeFireController Get()
	{
		return MakeFireController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		MakeFireController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.MakeFire;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_PlayerIdleAnimHash = Animator.StringToHash(Player.Get().GetCurrentItem(Hand.Right).m_InfoName + "_Idle");
		this.m_PlayerAnimHash = Animator.StringToHash(Player.Get().GetCurrentItem(Hand.Right).m_InfoName);
		this.SetState(MakeFireController.State.WaitingForKindling);
		HintsManager.Get().ShowHint("AddKindling", 0f);
		Vector2 lookDev = this.m_Player.m_FPPController.GetLookDev();
		lookDev.y = 0f;
		this.m_Player.m_FPPController.SetLookDev(lookDev);
		this.InitializeAudio();
		if (CraftingManager.Get().IsActive())
		{
			CraftingManager.Get().Deactivate();
		}
	}

	private void InitializeAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
			this.m_AudioSource.spatialBlend = 1f;
		}
		if (this.m_AudioClipBoard == null)
		{
			this.m_AudioClipBoard = (Resources.Load("Sounds/StartingFire/GH_Drill_Board_Success") as AudioClip);
		}
		if (this.m_AudioClipStick == null)
		{
			this.m_AudioClipStick = (Resources.Load("Sounds/StartingFire/GH_Drill_Stick_Success") as AudioClip);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		HintsManager.Get().HideHint("AddKindling");
		this.SetState(MakeFireController.State.None);
	}

	public bool IsStartingFire()
	{
		return this.IsActive() && this.m_State == MakeFireController.State.WaitingForKindling;
	}

	public bool IsMakeFireGame()
	{
		return this.IsActive() && this.m_State > MakeFireController.State.WaitingForKindling;
	}

	private void SetState(MakeFireController.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnSetState();
	}

	private void OnSetState()
	{
		this.m_StartStateTime = Time.time;
		switch (this.m_State)
		{
		case MakeFireController.State.None:
			if (this.m_Kindling)
			{
				UnityEngine.Object.Destroy(this.m_Kindling.gameObject);
			}
			if (this.m_FireTool)
			{
				if (this.m_FireTool.m_Animator.isInitialized)
				{
					this.m_FireTool.m_Animator.SetBool(this.m_ToolAnimHash, false);
				}
				this.m_FireTool.m_Light.enabled = false;
				this.m_FireTool.m_Emission.enabled = false;
				this.m_FireTool.m_KindlingSlot.Deactivate();
				this.m_FireTool.m_KindlingSlot.gameObject.SetActive(false);
				if (this.m_FireTool.m_AnimationVis)
				{
					this.m_FireTool.m_AnimationVis.SetActive(false);
				}
				if (this.m_FireTool.m_NormalVis)
				{
					this.m_FireTool.m_NormalVis.SetActive(true);
				}
				this.m_FireTool = null;
			}
			if (this.m_Tool)
			{
				this.m_Tool.StaticPhxRequestRemove();
				this.m_Tool.OnRemovedFromHand();
				this.m_Tool = null;
			}
			if (this.m_Animator.isInitialized)
			{
				this.m_Animator.SetBool(this.m_PlayerIdleAnimHash, false);
				this.m_Animator.SetBool(this.m_PlayerAnimHash, false);
				this.m_Animator.SetBool(this.m_SuccessHash, false);
				this.m_Animator.SetBool(this.m_FailHash, false);
			}
			if (this.m_ParticleObj)
			{
				ParticlesManager.Get().Remove(this.m_ParticleObj);
				this.m_ParticleObj = null;
			}
			this.m_FXSpawned = false;
			this.m_Player.UnblockMoves();
			this.m_Player.UnblockRotation();
			return;
		case MakeFireController.State.WaitingForKindling:
			this.m_FireLevel = 0f;
			this.m_Tool = (ItemTool)this.m_Player.GetCurrentItem(Hand.Right);
			DebugUtils.Assert(this.m_Tool != null, "[MakeFireController:OnEnable] ERROR - Currentitem is not a Fire Tool!", true, DebugUtils.AssertType.Info);
			this.m_Tool.ItemsManagerUnregister();
			this.m_FireTool = this.m_Tool.GetComponent<FireTool>();
			DebugUtils.Assert(this.m_FireTool != null, "[MakeFireController:OnEnable] ERROR - Missing FireTool component!", true, DebugUtils.AssertType.Info);
			this.m_FireTool.m_KindlingSlot.gameObject.SetActive(true);
			this.m_FireTool.m_KindlingSlot.Activate();
			this.m_Tool.StaticPhxRequestAdd();
			if (this.m_FireTool.m_AnimationVis)
			{
				this.m_FireTool.m_AnimationVis.SetActive(true);
			}
			if (this.m_FireTool.m_NormalVis)
			{
				this.m_FireTool.m_NormalVis.SetActive(false);
			}
			this.m_Player.BlockMoves();
			this.m_Player.BlockRotation();
			this.m_Animator.CrossFadeInFixedTime(this.m_PlayerIdleAnimHash, 0.25f, this.m_SpineLayerIndex);
			this.m_Animator.CrossFadeInFixedTime(this.m_PlayerIdleAnimHash, 0.25f, this.m_BaseLayerIndex);
			this.m_Animator.SetBool(this.m_PlayerIdleAnimHash, true);
			this.m_Animator.SetBool(this.m_PlayerAnimHash, false);
			this.m_Animator.SetBool(this.m_SuccessHash, false);
			this.m_Animator.SetBool(this.m_FailHash, false);
			this.m_Tool.OnWaitingForKindling();
			return;
		case MakeFireController.State.Game:
			HintsManager.Get().HideHint("AddKindling");
			PlayerSanityModule.Get().OnWhispersEvent(PlayerSanityModule.WhisperType.StartFire);
			this.m_Tool.OnStartMakeFireGame();
			this.m_Animator.CrossFadeInFixedTime(this.m_PlayerAnimHash, 0.25f, this.m_SpineLayerIndex);
			this.m_Animator.CrossFadeInFixedTime(this.m_PlayerAnimHash, 0.25f, this.m_BaseLayerIndex);
			this.m_Animator.SetBool(this.m_PlayerAnimHash, true);
			this.m_Animator.SetBool(this.m_PlayerIdleAnimHash, false);
			this.m_FireTool.m_Animator.SetBool(this.m_ToolAnimHash, true);
			this.m_FireTool.m_KindlingSlot.gameObject.SetActive(false);
			this.m_FireTool.m_KindlingSlot.Deactivate();
			this.m_Kindling.enabled = false;
			Inventory3DManager.Get().Deactivate();
			this.CalcStaminaConsumption();
			this.PlaySound();
			PlayerAudioModule.Get().PlayMakeFireSound();
			return;
		case MakeFireController.State.Fail:
		case MakeFireController.State.Quit:
			this.m_Animator.SetBool(this.m_PlayerAnimHash, false);
			this.m_Animator.SetBool(this.m_PlayerIdleAnimHash, false);
			this.m_Tool.OnFailMakeFireGame();
			this.m_Animator.SetBool(this.m_FailHash, true);
			ParticlesManager.Get().Remove(this.m_ParticleObj);
			this.m_ParticleObj = null;
			if (this.m_FireTool != null)
			{
				this.m_FireTool.m_Animator.SetBool(this.m_ToolAnimHash, false);
			}
			this.StopSound();
			HintsManager.Get().ShowHint("MakeFire_Fail", 10f);
			PlayerAudioModule.Get().StopMakeFireSound();
			if (this.m_State == MakeFireController.State.Fail)
			{
				PlayerAudioModule.Get().PlayMakeFireFailSound();
				return;
			}
			break;
		case MakeFireController.State.Success:
			this.m_Animator.SetBool(this.m_PlayerAnimHash, false);
			this.m_Animator.SetBool(this.m_PlayerIdleAnimHash, false);
			Skill.Get<MakeFireSkill>().OnSkillAction();
			ParticlesManager.Get().Remove(this.m_ParticleObj);
			this.m_ParticleObj = null;
			this.m_Tool.OnSuccessMakeFireGame();
			this.m_Animator.SetBool(this.m_SuccessHash, true);
			this.m_FireTool.m_Animator.SetBool(this.m_ToolAnimHash, false);
			this.StopSound();
			PlayerAudioModule.Get().StopMakeFireSound();
			break;
		default:
			return;
		}
	}

	private void CalcStaminaConsumption()
	{
		this.m_StaminaConsumption = this.m_FireTool.m_StaminaConsumption * this.m_Kindling.m_Info.m_MakeFireStaminaConsumptionMul;
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if ((action_data.m_Action == InputsManager.InputAction.Quit || action_data.m_Action == InputsManager.InputAction.AdditionalQuit) && (this.m_State == MakeFireController.State.WaitingForKindling || this.m_State == MakeFireController.State.Game) && (!GreenHellGame.IsPadControllerActive() || !HUDItem.Get().m_Active))
		{
			this.SetState(MakeFireController.State.Quit);
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateToolTransform();
		this.UpdateState();
		if (this.m_State != MakeFireController.State.None && this.m_FireTool)
		{
			this.m_FireTool.gameObject.layer = 0;
			for (int i = 0; i < this.m_FireTool.transform.childCount; i++)
			{
				this.m_FireTool.transform.GetChild(i).gameObject.layer = 0;
			}
		}
	}

	private void UpdateToolTransform()
	{
		if (this.m_State != MakeFireController.State.WaitingForKindling)
		{
			return;
		}
		if (this.m_Tool == null)
		{
			return;
		}
		Vector3 localPosition = this.m_Tool.m_Holder.transform.localPosition;
		Transform lhand = this.m_Player.GetLHand();
		Quaternion rhs = Quaternion.Inverse(this.m_Tool.m_Holder.localRotation);
		this.m_Tool.gameObject.transform.rotation = lhand.rotation;
		this.m_Tool.gameObject.transform.rotation *= rhs;
		Vector3 b = this.m_Tool.m_Holder.parent.position - this.m_Tool.m_Holder.position;
		this.m_Tool.gameObject.transform.position = lhand.position;
		this.m_Tool.gameObject.transform.position += b;
	}

	private void UpdateState()
	{
		switch (this.m_State)
		{
		case MakeFireController.State.WaitingForKindling:
			if (this.m_Tool == null)
			{
				this.SetState(MakeFireController.State.Quit);
				return;
			}
			this.UpdatePose();
			this.m_Tool.transform.parent = null;
			return;
		case MakeFireController.State.Game:
			this.UpdateStamina();
			this.UpdateToolDurability();
			this.UpdateFireLevel();
			this.UpdateEffects();
			if (this.m_Player.GetStamina() <= 0f)
			{
				this.SetState(MakeFireController.State.Fail);
				return;
			}
			if (Time.time - this.m_StartStateTime >= this.m_Tool.m_Info.m_MakeFireDuration)
			{
				this.SetState(MakeFireController.State.Success);
			}
			break;
		case MakeFireController.State.Fail:
		case MakeFireController.State.Success:
		case MakeFireController.State.Quit:
			break;
		default:
			return;
		}
	}

	private void UpdatePose()
	{
	}

	private void UpdateStamina()
	{
		this.m_Player.DecreaseStamina(Time.deltaTime * this.m_StaminaConsumption * Skill.Get<MakeFireSkill>().GetStaminaConsumptionMul());
	}

	private void UpdateToolDurability()
	{
		this.m_Tool.m_Info.m_Health -= Time.deltaTime * this.m_Tool.m_Info.m_DamageSelf * Skill.Get<MakeFireSkill>().GetToolDurabilityConsumptionMul();
	}

	private void UpdateFireLevel()
	{
		this.m_FireLevel += Time.deltaTime * 0.2f;
		this.m_FireLevel = Mathf.Clamp01(this.m_FireLevel);
	}

	private void UpdateEffects()
	{
		if (this.m_FireLevel < this.m_MinFireLevelToEnableEffects)
		{
			this.m_FireTool.m_Light.enabled = false;
			this.m_FireTool.m_Emission.enabled = false;
			return;
		}
		this.m_FireTool.m_Light.enabled = true;
		this.m_FireTool.m_Emission.enabled = true;
		this.m_FireTool.m_Light.intensity = CJTools.Math.GetProportionalClamp(0f, this.m_MaxLightIntensity, this.m_FireLevel, this.m_MinFireLevelToEnableEffects, 1f);
		if (!this.m_FXSpawned && this.m_FireLevel >= this.m_MinFireLevelToEnableEffects)
		{
			this.m_ParticleObj = ParticlesManager.Get().Spawn("Small Smoke - Ember", this.m_FireTool.m_KindlingSlot.transform.position, Quaternion.identity, Vector3.zero, null, -1f, false);
			this.m_FXSpawned = true;
		}
	}

	public void OnAddKindling(Item kindling)
	{
		this.m_Kindling = kindling;
		this.SetState(MakeFireController.State.Game);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.MakeFireSuccessEnd)
		{
			if (this.m_State == MakeFireController.State.Success && this.m_Fire != null)
			{
				this.m_FireTool.m_KindlingSlot.RemoveItem();
				Item tool = this.m_Tool;
				this.m_Player.DropItem(tool);
				InventoryBackpack.Get().InsertItem(tool, null, null, true, true, true, true, true);
				this.m_Player.ResetControllerToStart();
				this.m_Player.SetWantedItem(Hand.Right, this.m_Fire, true);
				this.m_Fire.m_ConnectedParticleObj = ParticlesManager.Get().Spawn("Small Smoke - Ember", this.m_Fire.transform.position, Quaternion.identity, Vector3.zero, this.m_Fire.transform, -1f, false);
				this.m_Fire = null;
				return;
			}
		}
		else if (id == AnimEventID.MakeFireFailEnd)
		{
			if (this.m_State == MakeFireController.State.Fail || this.m_State == MakeFireController.State.Quit)
			{
				if (this.m_FireTool != null)
				{
					this.m_FireTool.m_KindlingSlot.RemoveItem();
					Item tool2 = this.m_Tool;
					this.m_Player.DropItem(tool2);
					InventoryBackpack.Get().InsertItem(tool2, null, null, true, true, true, true, true);
				}
				this.m_Player.SetWantedItem(Hand.Right, null, true);
				if (this.m_Kindling != null)
				{
					this.m_Kindling.enabled = true;
					InventoryBackpack.Get().InsertItem(this.m_Kindling, null, null, true, true, true, true, true);
					this.m_Kindling = null;
					return;
				}
			}
		}
		else if (id == AnimEventID.MakeFireTakeFire && this.m_State == MakeFireController.State.Success && !this.m_Fire)
		{
			this.m_Fire = ItemsManager.Get().CreateItem(ItemID.Fire, false, Vector3.zero, Quaternion.identity);
			this.m_Player.AttachItemToHand(Hand.Right, this.m_Fire);
			UnityEngine.Object.Destroy(this.m_Kindling.gameObject);
			this.m_Kindling = null;
		}
	}

	public bool ShouldBlockTriggers()
	{
		return this.m_State == MakeFireController.State.Game || this.m_State == MakeFireController.State.Fail || this.m_State == MakeFireController.State.Quit || this.m_State == MakeFireController.State.Success;
	}

	private void PlaySound()
	{
		if (this.m_AudioSource == null || this.m_AudioClipBoard == null || this.m_AudioClipStick == null)
		{
			return;
		}
		if (this.m_Tool.m_Info.m_ID == ItemID.Hand_Drill_Board)
		{
			this.m_AudioSource.clip = this.m_AudioClipBoard;
		}
		else
		{
			this.m_AudioSource.clip = this.m_AudioClipStick;
		}
		this.m_AudioSource.Play();
	}

	private void StopSound()
	{
		if (this.m_AudioSource == null)
		{
			return;
		}
		this.m_AudioSource.Stop();
		this.m_AudioSource.clip = null;
	}

	public bool CanUseItemAsKindling(Item item)
	{
		return this.m_FireTool && this.m_FireTool.m_KindlingSlot.CanInsertItem(item);
	}

	public void InsertItemToKindlingSlot(Item item)
	{
		if (!this.m_FireTool)
		{
			return;
		}
		this.m_FireTool.m_KindlingSlot.InsertItem(item);
	}

	private int m_ToolAnimHash = Animator.StringToHash("MakeFire");

	private int m_SuccessHash = Animator.StringToHash("MakeFireSuccess");

	private int m_FailHash = Animator.StringToHash("MakeFireFail");

	private int m_PlayerIdleAnimHash;

	private int m_PlayerAnimHash;

	private ItemTool m_Tool;

	private FireTool m_FireTool;

	private Item m_Kindling;

	private float m_MaxLightIntensity = 4f;

	private MakeFireController.State m_State;

	private float m_StartStateTime;

	private float m_StaminaConsumption;

	private float m_FireLevel;

	private float m_MinFireLevelToEnableEffects = 0.05f;

	public float m_GameLength = 5f;

	private static MakeFireController s_Instance;

	private bool m_FXSpawned;

	private GameObject m_ParticleObj;

	private Item m_Fire;

	private AudioSource m_AudioSource;

	private AudioClip m_AudioClipBoard;

	private AudioClip m_AudioClipStick;

	private enum State
	{
		None,
		WaitingForKindling,
		Game,
		Fail,
		Success,
		Quit
	}
}
