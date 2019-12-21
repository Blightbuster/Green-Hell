using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class BowController : PlayerController
{
	public bool m_MaxAim { get; private set; }

	public static BowController Get()
	{
		return BowController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		BowController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.Bow;
		this.SetupAudio();
	}

	protected override void Start()
	{
		base.Start();
		this.m_ConditionModule = Player.Get().GetComponent<PlayerConditionModule>();
	}

	private void SetupAudio()
	{
		this.m_DrawSoundClipsDict[311] = new List<AudioClip>();
		AudioClip item = (AudioClip)Resources.Load("Sounds/Weapon/bow_bamboo_draw_01");
		this.m_DrawSoundClipsDict[311].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/bamboo_bow_draw_02");
		this.m_DrawSoundClipsDict[311].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/bamboo_bow_draw_03");
		this.m_DrawSoundClipsDict[311].Add(item);
		this.m_DrawSoundClipsDict[310] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/bow_wooden_draw_01");
		this.m_DrawSoundClipsDict[310].Add(item);
		this.m_ShotSoundClipsDict[311] = new List<AudioClip>();
		AudioClip item2 = (AudioClip)Resources.Load("Sounds/Weapon/bow_bamboo_shot_01");
		this.m_ShotSoundClipsDict[311].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/bamboo_bow_shot_02");
		this.m_ShotSoundClipsDict[311].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/bamboo_bow_shot_03");
		this.m_ShotSoundClipsDict[311].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/bamboo_bow_shot_04");
		this.m_ShotSoundClipsDict[311].Add(item2);
		this.m_DrawSoundClipsDict[310] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_draw_02");
		this.m_DrawSoundClipsDict[310].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_draw_03");
		this.m_DrawSoundClipsDict[310].Add(item);
		this.m_ShotSoundClipsDict[310] = new List<AudioClip>();
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/bow_wooden_shot_01");
		this.m_ShotSoundClipsDict[310].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_shot_02");
		this.m_ShotSoundClipsDict[310].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_shot_03");
		this.m_ShotSoundClipsDict[310].Add(item2);
		this.m_DrawSoundClipsDict[329] = new List<AudioClip>();
		item = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_draw_02");
		this.m_DrawSoundClipsDict[329].Add(item);
		item = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_draw_03");
		this.m_DrawSoundClipsDict[329].Add(item);
		this.m_ShotSoundClipsDict[329] = new List<AudioClip>();
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/bow_wooden_shot_01");
		this.m_ShotSoundClipsDict[329].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_shot_02");
		this.m_ShotSoundClipsDict[329].Add(item2);
		item2 = (AudioClip)Resources.Load("Sounds/Weapon/wooden_bow_shot_03");
		this.m_ShotSoundClipsDict[329].Add(item2);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_MaxAim = false;
		this.SetState(BowController.State.NoArrowIdle);
		this.m_Animator.SetInteger(this.m_IBowState, (int)this.m_State);
		this.m_RHandHolder = this.m_Player.GetRHand();
		this.m_LHandHolder = this.m_Player.GetLHand();
		this.m_WasActivated = false;
		this.UpdateArrowFromInventory();
		this.SetupBow();
	}

	private void SetupBow()
	{
		this.m_CurrentItemInHand = this.m_Player.GetCurrentItem(Hand.Left);
		if (this.m_CurrentItemInHand)
		{
			DebugUtils.Assert(this.m_CurrentItemInHand.m_Info.IsWeapon(), true);
			this.m_ItemRenderer = this.m_CurrentItemInHand.gameObject.GetComponentInChildren<Renderer>();
			this.m_Animator.SetInteger(this.m_IWeaponType, (int)((Weapon)this.m_CurrentItemInHand).GetWeaponType());
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Arrow != null && Inventory3DManager.Get())
		{
			this.m_Arrow.gameObject.transform.parent = null;
			this.m_Arrow.m_Loaded = false;
			if (InventoryBackpack.Get() && InventoryBackpack.Get().InsertItem(this.m_Arrow, this.m_LastSlot, this.m_LastGroup, true, false, false, true, true) != InsertResult.Ok)
			{
				InventoryBackpack.Get().InsertItem(this.m_Arrow, null, null, true, true, true, true, true);
			}
			this.m_Arrow = null;
		}
		this.SetState(BowController.State.None);
		this.m_Animator.SetInteger(this.m_IBowState, 0);
		this.m_Animator.SetBool(this.m_LowStaminaHash, false);
		this.m_Animator.SetBool(this.m_NoArrowLowStaminaHash, false);
		this.m_Animator.SetInteger(this.m_IWeaponType, 0);
	}

	private void SetState(BowController.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.OnExitState();
		this.m_State = state;
		this.m_EnterStateTime = Time.time;
		this.OnEnterState();
	}

	private void OnExitState()
	{
		BowController.State state = this.m_State;
		if (state == BowController.State.AimLoop)
		{
			Player.Get().StopShake(0.5f);
			Player.Get().StopAim();
			this.m_MaxAim = false;
		}
	}

	private void OnEnterState()
	{
		BowController.State state = this.m_State;
		if (state != BowController.State.AimLoop)
		{
			if (state == BowController.State.Shot)
			{
				this.m_ShotSetStateTime = Time.time;
				this.Shot();
				return;
			}
		}
		else
		{
			this.StartShake();
			Player.Get().StartAim(Player.AimType.Bow, 9f);
		}
	}

	public override void ControllerUpdate()
	{
		if (this.m_State == BowController.State.Shot)
		{
			this.UpdateShotState();
		}
		this.UpdateState();
		this.CheckForWatchShow();
		this.CheckForWatchHide();
	}

	private void UpdateShotState()
	{
		if (Time.time - this.m_ShotSetStateTime > 0.429f)
		{
			this.ShotEnd();
		}
	}

	private void ShotEnd()
	{
		this.m_Arrow = null;
		this.UpdateArrowFromInventory();
		if (this.m_Arrow != null)
		{
			this.SetState(BowController.State.Reload);
			return;
		}
		this.SetState(BowController.State.Idle);
	}

	public override void ControllerLateUpdate()
	{
		Item currentItem = Player.Get().GetCurrentItem(Hand.Left);
		if (currentItem && !this.m_WasActivated && !currentItem.gameObject.activeSelf)
		{
			int shortNameHash = this.m_Animator.GetCurrentAnimatorStateInfo(this.m_SpineLayerIndex).shortNameHash;
			if (shortNameHash == this.m_ArrowIdle || shortNameHash == this.m_NoArrowIdle)
			{
				currentItem.gameObject.SetActive(true);
				this.m_WasActivated = true;
			}
		}
		this.UpdateArrow();
		if (this.m_Arrow)
		{
			this.m_LastArrowRotation = this.m_Arrow.transform.rotation;
			return;
		}
		this.m_LastArrowRotation = Quaternion.identity;
	}

	public void SetArrow(Arrow arrow)
	{
		if (!arrow)
		{
			return;
		}
		this.m_LastGroup = arrow.m_Info.m_InventoryCellsGroup;
		this.m_LastSlot = arrow.m_CurrentSlot;
		InventoryBackpack.Get().RemoveItem(arrow, false);
		this.m_Arrow = arrow;
		this.m_Arrow.gameObject.SetActive(true);
		this.m_Arrow.m_Loaded = true;
		this.m_Arrow.StaticPhxRequestReset();
		this.m_ArrowHolder = this.m_Arrow.gameObject.transform.FindDeepChild("Holder");
		Item currentItem = Player.Get().GetCurrentItem(Hand.Left);
		if (currentItem)
		{
			Physics.IgnoreCollision(arrow.m_Collider, currentItem.m_Collider);
		}
		Physics.IgnoreCollision(Player.Get().m_Collider, arrow.m_Collider);
		DebugUtils.Assert(this.m_ArrowHolder, true);
		this.UpdateArrow();
	}

	private void UpdateArrowFromInventory()
	{
		if (SaveGame.m_State != SaveGame.State.None)
		{
			return;
		}
		if (this.m_Arrow == null)
		{
			Item item = InventoryBackpack.Get().FindItem(ItemType.Arrow, true);
			if (!item)
			{
				item = InventoryBackpack.Get().FindItem(ItemType.Arrow, false);
			}
			this.SetArrow((Arrow)item);
		}
	}

	private void UpdateArrow()
	{
		if (this.m_Arrow == null)
		{
			return;
		}
		int shortNameHash = this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash;
		int shortNameHash2 = this.m_Animator.GetCurrentAnimatorStateInfo(2).shortNameHash;
		if (TriggerController.Get().IsGrabInProgress() || ItemController.Get().m_State != ItemController.State.None || shortNameHash2 != this.m_WatchIdleClip || shortNameHash == this.m_ObjectAimClip || shortNameHash == this.m_ObjectThrowClip)
		{
			this.m_Arrow.gameObject.SetActive(false);
			return;
		}
		this.m_Arrow.gameObject.SetActive(true);
		if (this.m_Arrow.m_Rigidbody != null)
		{
			this.m_Arrow.m_Rigidbody.isKinematic = true;
		}
		if (this.m_Arrow.m_Collider != null)
		{
			this.m_Arrow.m_Collider.isTrigger = true;
			Physics.IgnoreCollision(this.m_Arrow.m_Collider, this.m_Player.m_Collider);
		}
		Quaternion rhs = Quaternion.Inverse(this.m_ArrowHolder.localRotation);
		Vector3 b = this.m_ArrowHolder.parent.position - this.m_ArrowHolder.position;
		this.m_Arrow.gameObject.transform.rotation = this.m_RHandHolder.rotation;
		this.m_Arrow.gameObject.transform.rotation *= rhs;
		this.m_Arrow.gameObject.transform.position = this.m_RHandHolder.position;
		this.m_Arrow.gameObject.transform.position += b;
		this.m_Arrow.gameObject.transform.parent = this.m_RHandHolder.transform;
	}

	public override void OnInputAction(InputActionData action_data)
	{
		base.OnInputAction(action_data);
		if (action_data.m_Action == InputsManager.InputAction.BowAim)
		{
			if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_State == BowController.State.Idle && this.m_Arrow != null && Time.time - this.m_EnterStateTime >= 0.5f && !PlayerConditionModule.Get().IsLowStamina())
			{
				this.SetState(BowController.State.AimLoop);
				this.PlayDrawSound();
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.BowShot)
		{
			if (this.m_State == BowController.State.AimLoop && this.m_Animator.GetCurrentAnimatorStateInfo(1).shortNameHash == this.m_BowAimLoop)
			{
				this.SetState(BowController.State.Shot);
				this.PlayShotSound();
				return;
			}
			this.SetState(BowController.State.Idle);
			return;
		}
		else if (action_data.m_Action == InputsManager.InputAction.BowCancelAim)
		{
			if (this.m_State == BowController.State.AimLoop)
			{
				this.SetState(BowController.State.Idle);
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.BowMaxAim)
		{
			if (this.m_State == BowController.State.AimLoop && !this.m_MaxAim && !PlayerConditionModule.Get().IsStaminaCriticalLevel())
			{
				Player.Get().StopShake(0.5f);
				this.m_MaxAim = true;
				return;
			}
		}
		else if (action_data.m_Action == InputsManager.InputAction.BowStopMaxAim && this.m_State == BowController.State.AimLoop && this.m_MaxAim)
		{
			this.StartShake();
			this.m_MaxAim = false;
		}
	}

	private void StartShake()
	{
		Player.Get().StartShake(10f, 1f, 1f);
	}

	private void UpdateState()
	{
		if (PlayerConditionModule.Get().GetStamina() == 0f)
		{
			this.m_Animator.SetBool(this.m_Arrow ? this.m_LowStaminaHash : this.m_NoArrowLowStaminaHash, true);
			this.SetState(this.m_Arrow ? BowController.State.LowStamina : BowController.State.NoArrowLowStamina);
		}
		switch (this.m_State)
		{
		case BowController.State.Idle:
			if (!Inventory3DManager.Get().gameObject.activeSelf)
			{
				this.UpdateArrowFromInventory();
				if (this.m_Arrow == null)
				{
					this.SetState(BowController.State.NoArrowIdle);
				}
			}
			break;
		case BowController.State.AimLoop:
			if (this.m_MaxAim && PlayerConditionModule.Get().IsStaminaCriticalLevel())
			{
				this.StartShake();
				this.m_MaxAim = false;
			}
			if (PlayerConditionModule.Get().IsStaminaCriticalLevel())
			{
				Player.Get().m_AdditionalShakePower += (1f - Player.Get().m_AdditionalShakePower) * Time.deltaTime * 0.5f;
			}
			if (TriggerController.Get().IsGrabInProgress() || this.m_Player.IsRunning())
			{
				this.SetState(BowController.State.Idle);
			}
			else if (Time.time - this.m_EnterStateTime > 2f)
			{
				float num = PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.Bow) * Skill.Get<ArcherySkill>().GetStaminaConsumptionMul();
				this.m_Player.DecreaseStamina(num * Time.deltaTime * (this.m_MaxAim ? 2f : 1f));
				if (PlayerConditionModule.Get().GetStamina() == 0f)
				{
					this.SetState(BowController.State.Idle);
				}
			}
			break;
		case BowController.State.NoArrowIdle:
			this.UpdateArrowFromInventory();
			if (this.m_Arrow != null)
			{
				this.SetState(BowController.State.Idle);
			}
			break;
		case BowController.State.LowStamina:
		case BowController.State.NoArrowLowStamina:
			if (!PlayerConditionModule.Get().IsLowStamina())
			{
				this.SetState((this.m_Arrow != null) ? BowController.State.Idle : BowController.State.NoArrowIdle);
				this.m_Animator.SetBool(this.m_LowStaminaHash, false);
			}
			break;
		}
		this.m_Animator.SetInteger(this.m_IBowState, (int)this.m_State);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.BowShotEnd)
		{
			if (this.m_State == BowController.State.Shot)
			{
				this.ShotEnd();
				return;
			}
		}
		else
		{
			if (id == AnimEventID.BowReloadEnd)
			{
				this.SetState(BowController.State.Idle);
				return;
			}
			if (id == AnimEventID.GrabItemShowArrow)
			{
				this.ShowArrow(true);
				return;
			}
			if (id == AnimEventID.GrabItemHideArrow)
			{
				this.ShowArrow(false);
				return;
			}
			if (id == AnimEventID.WatchStart)
			{
				this.CheckForWatchHide();
			}
		}
	}

	private void ShowArrow(bool show)
	{
		if (this.m_Arrow != null)
		{
			this.m_Arrow.gameObject.SetActive(show);
		}
	}

	private void Shot()
	{
		if (this.m_Arrow)
		{
			this.m_Arrow.m_Loaded = false;
			this.m_Player.ThrowItem(this.m_Arrow);
			this.m_Arrow.transform.rotation = this.m_LastArrowRotation;
			this.m_Arrow = null;
		}
		if (this.m_CurrentItemInHand)
		{
			this.m_CurrentItemInHand.m_Info.m_Health -= this.m_CurrentItemInHand.m_Info.m_DamageSelf;
		}
	}

	public bool IsShot()
	{
		return this.m_State == BowController.State.Shot;
	}

	public override void GetInputActions(ref List<int> actions)
	{
		BowController.State state = this.m_State;
		if (state != BowController.State.Idle)
		{
			if (state != BowController.State.AimLoop)
			{
				return;
			}
			actions.Add(18);
			actions.Add(4);
			if (!this.m_MaxAim)
			{
				actions.Add(16);
			}
		}
		else if (this.m_Arrow != null)
		{
			actions.Add(14);
			return;
		}
	}

	public override string ReplaceClipsGetItemName()
	{
		if (this.m_Player.GetCurrentItem(Hand.Left) == null)
		{
			return string.Empty;
		}
		if (this.m_Player.GetCurrentItem(Hand.Left).m_Info == null)
		{
			return string.Empty;
		}
		return this.m_Player.GetCurrentItem(Hand.Left).m_Info.m_ID.ToString();
	}

	private void PlayDrawSound()
	{
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Left);
		if (!currentItem)
		{
			return;
		}
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		List<AudioClip> list = null;
		if (this.m_DrawSoundClipsDict.TryGetValue((int)currentItem.m_Info.m_ID, out list))
		{
			this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
		}
	}

	private void PlayShotSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		ItemInfo info = this.m_Player.GetCurrentItem(Hand.Left).m_Info;
		List<AudioClip> list = null;
		if (this.m_ShotSoundClipsDict.TryGetValue((int)info.m_ID, out list))
		{
			this.m_AudioSource.PlayOneShot(list[UnityEngine.Random.Range(0, list.Count)]);
		}
	}

	private void CheckForWatchShow()
	{
		if (this.m_Animator.GetCurrentAnimatorStateInfo(2).shortNameHash == this.m_WatchShowClip)
		{
			this.m_ItemRenderer.enabled = false;
		}
	}

	private void CheckForWatchHide()
	{
		if (this.m_Animator.GetCurrentAnimatorStateInfo(2).shortNameHash == this.m_WatchHideClip)
		{
			this.m_ItemRenderer.enabled = true;
		}
	}

	public override void OnItemChanged(Item item, Hand hand)
	{
		base.OnItemChanged(item, hand);
		this.SetupBow();
	}

	public override bool PlayUnequipAnimation()
	{
		return true;
	}

	private BowController.State m_State;

	private int m_IBowState = Animator.StringToHash("Bow");

	private int m_IWeaponType = Animator.StringToHash("WeaponType");

	private int m_WatchIdleClip = Animator.StringToHash("Idle");

	private int m_WatchHideClip = Animator.StringToHash("WatchHide");

	private int m_WatchShowClip = Animator.StringToHash("WatchShow");

	private int m_ObjectAimClip = Animator.StringToHash("ObjectAim");

	private int m_ObjectThrowClip = Animator.StringToHash("ObjectThrow");

	private int m_ArrowIdle = Animator.StringToHash("BowIdle");

	private int m_NoArrowIdle = Animator.StringToHash("NoArrowIdle");

	private int m_LowStaminaHash = Animator.StringToHash("LowStamina");

	private int m_NoArrowLowStaminaHash = Animator.StringToHash("LowStaminaNoArrow");

	private int m_BowAimLoop = Animator.StringToHash("BowAimLoop");

	private ItemSlot m_LastSlot;

	private InventoryCellsGroup m_LastGroup;

	private Arrow m_Arrow;

	private Transform m_ArrowHolder;

	private Transform m_RHandHolder;

	private Transform m_LHandHolder;

	public float m_AimShakePower = 5f;

	private AudioSource m_AudioSource;

	private Dictionary<int, List<AudioClip>> m_DrawSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private Dictionary<int, List<AudioClip>> m_ShotSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private float m_EnterStateTime;

	private PlayerConditionModule m_ConditionModule;

	private Item m_CurrentItemInHand;

	private Renderer m_ItemRenderer;

	private bool m_WasActivated;

	private Quaternion m_LastArrowRotation = Quaternion.identity;

	private static BowController s_Instance;

	private const float m_ShotStateDuration = 0.33f;

	private float m_ShotSetStateTime = float.MaxValue;

	private enum State
	{
		None,
		Idle,
		AimLoop = 3,
		Shot,
		Reload,
		NoArrowIdle,
		LowStamina,
		NoArrowLowStamina
	}
}
