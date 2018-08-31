using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class BowController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		this.m_ControllerType = PlayerControllerType.Bow;
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
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.SetState(BowController.State.NoArrowIdle);
		this.m_Animator.SetInteger(this.m_IBowState, (int)this.m_State);
		this.m_RHandHolder = this.m_Player.GetRHand();
		this.m_WasActivated = false;
		this.UpdateArrowFromInventory();
		this.m_CurrentItemInHand = this.m_Player.GetCurrentItem(Hand.Left);
		this.m_ItemRenderer = this.m_CurrentItemInHand.gameObject.GetComponentInChildren<Renderer>();
		DebugUtils.Assert(this.m_CurrentItemInHand && this.m_CurrentItemInHand.m_Info.IsWeapon(), true);
		this.m_Animator.SetInteger(this.m_IWeaponType, (int)((Weapon)this.m_CurrentItemInHand).GetWeaponType());
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Arrow != null && Inventory3DManager.Get())
		{
			this.m_Arrow.gameObject.transform.parent = null;
			this.m_Arrow.m_Loaded = false;
			InventoryBackpack.Get().InsertItem(this.m_Arrow, null, null, true, true, true, true, true);
			this.m_Arrow = null;
		}
		this.SetState(BowController.State.None);
		this.m_Animator.SetInteger(this.m_IBowState, 0);
		this.m_Animator.SetInteger(this.m_IWeaponType, 0);
		this.m_Player.StopAim();
	}

	private void SetState(BowController.State state)
	{
		this.m_State = state;
		this.m_EnterStateTime = Time.time;
	}

	public override void ControllerUpdate()
	{
		this.UpdateState();
		this.CheckForWatchShow();
		this.CheckForWatchHide();
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
	}

	private void UpdateArrowFromInventory()
	{
		if (this.m_Arrow == null)
		{
			Item item = InventoryBackpack.Get().FindItem(ItemType.Arrow);
			if (item != null)
			{
				this.m_Arrow = (Arrow)item;
				this.m_Arrow.gameObject.SetActive(true);
				this.m_Arrow.m_Loaded = true;
				this.m_ArrowHolder = this.m_Arrow.gameObject.transform.FindDeepChild("Holder");
				DebugUtils.Assert(this.m_ArrowHolder, true);
				InventoryBackpack.Get().RemoveItem(item, false);
			}
		}
	}

	private void UpdateArrow()
	{
		if (this.m_Arrow == null)
		{
			return;
		}
		Transform rhandHolder = this.m_RHandHolder;
		Rigidbody component = this.m_Arrow.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		Collider component2 = this.m_Arrow.gameObject.GetComponent<Collider>();
		if (component2 != null)
		{
			component2.isTrigger = true;
		}
		Quaternion rhs = Quaternion.Inverse(this.m_ArrowHolder.localRotation);
		Vector3 b = this.m_ArrowHolder.parent.position - this.m_ArrowHolder.position;
		this.m_Arrow.gameObject.transform.rotation = rhandHolder.rotation;
		this.m_Arrow.gameObject.transform.rotation *= rhs;
		this.m_Arrow.gameObject.transform.position = rhandHolder.position;
		this.m_Arrow.gameObject.transform.position += b;
		this.m_Arrow.gameObject.transform.parent = rhandHolder.transform;
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		base.OnInputAction(action);
		if (action == InputsManager.InputAction.BowAim)
		{
			if (!Inventory3DManager.Get().gameObject.activeSelf && this.m_State == BowController.State.Idle && this.m_Arrow != null && Time.time - this.m_EnterStateTime >= 0.5f && !PlayerConditionModule.Get().IsStaminaCriticalLevel())
			{
				this.SetState(BowController.State.AimLoop);
				this.m_Player.StartAim(Player.AimType.Bow);
				this.PlayDrawSound();
			}
		}
		else if (action == InputsManager.InputAction.BowShot)
		{
			if (this.m_State == BowController.State.AimLoop)
			{
				this.SetState(BowController.State.Shot);
				this.m_Player.StopAim();
				this.PlayShotSound();
			}
		}
		else if (action == InputsManager.InputAction.BowCancelAim && this.m_State == BowController.State.AimLoop)
		{
			this.SetState(BowController.State.Idle);
			this.m_Player.StopAim();
		}
	}

	private void UpdateState()
	{
		BowController.State state = this.m_State;
		if (state != BowController.State.NoArrowIdle)
		{
			if (state != BowController.State.Idle)
			{
				if (state == BowController.State.AimLoop)
				{
					if (TriggerController.Get().IsGrabInProgress())
					{
						this.SetState(BowController.State.Idle);
						this.m_Player.StopAim();
					}
					else
					{
						float num = PlayerConditionModule.Get().GetStaminaDecrease(StaminaDecreaseReason.Bow) * Skill.Get<ArcherySkill>().GetStaminaConsumptionMul();
						this.m_Player.DecreaseStamina(num * Time.deltaTime);
						if (PlayerConditionModule.Get().GetStamina() == 0f)
						{
							this.SetState(BowController.State.Idle);
							this.m_Player.StopAim();
						}
					}
				}
			}
			else if (!Inventory3DManager.Get().gameObject.activeSelf)
			{
				this.UpdateArrowFromInventory();
				if (this.m_Arrow == null)
				{
					this.SetState(BowController.State.NoArrowIdle);
				}
			}
		}
		else
		{
			this.UpdateArrowFromInventory();
			if (this.m_Arrow != null)
			{
				this.SetState(BowController.State.Idle);
			}
		}
		this.m_Animator.SetInteger(this.m_IBowState, (int)this.m_State);
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.BowShot)
		{
			this.Shot();
		}
		else if (id == AnimEventID.BowShotEnd)
		{
			this.m_Arrow = null;
			this.UpdateArrowFromInventory();
			if (this.m_Arrow != null)
			{
				this.SetState(BowController.State.Reload);
			}
			else
			{
				this.SetState(BowController.State.Idle);
			}
		}
		else if (id == AnimEventID.BowReloadEnd)
		{
			this.SetState(BowController.State.Idle);
		}
		else if (id == AnimEventID.GrabItemShowArrow)
		{
			this.ShowArrow(true);
		}
		else if (id == AnimEventID.GrabItemHideArrow)
		{
			this.ShowArrow(false);
		}
		else if (id == AnimEventID.WatchStart)
		{
			this.CheckForWatchHide();
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
		this.m_Arrow.m_Loaded = false;
		this.m_Player.ThrowItem(this.m_Arrow);
		this.m_Arrow = null;
		this.m_CurrentItemInHand.m_Info.m_Health -= this.m_CurrentItemInHand.m_Info.m_DamageSelf;
	}

	public override void GetInputActions(ref List<int> actions)
	{
		BowController.State state = this.m_State;
		if (state != BowController.State.Idle)
		{
			if (state == BowController.State.AimLoop)
			{
				actions.Add(16);
			}
		}
		else if (this.m_Arrow != null)
		{
			actions.Add(14);
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
		ItemInfo info = this.m_Player.GetCurrentItem(Hand.Left).m_Info;
		List<AudioClip> list = null;
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		if (this.m_DrawSoundClipsDict.TryGetValue((int)info.m_ID, out list))
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

	private BowController.State m_State;

	private int m_IBowState = Animator.StringToHash("Bow");

	private int m_IWeaponType = Animator.StringToHash("WeaponType");

	private int m_WatchHideClip = Animator.StringToHash("WatchHide");

	private int m_WatchShowClip = Animator.StringToHash("WatchShow");

	private int m_ArrowIdle = Animator.StringToHash("BowIdle");

	private int m_NoArrowIdle = Animator.StringToHash("NoArrowIdle");

	private Arrow m_Arrow;

	private Transform m_ArrowHolder;

	private Transform m_RHandHolder;

	public float m_AimShakePower = 5f;

	private AudioSource m_AudioSource;

	private Dictionary<int, List<AudioClip>> m_DrawSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private Dictionary<int, List<AudioClip>> m_ShotSoundClipsDict = new Dictionary<int, List<AudioClip>>();

	private float m_EnterStateTime;

	private PlayerConditionModule m_ConditionModule;

	private Item m_CurrentItemInHand;

	private Renderer m_ItemRenderer;

	private bool m_WasActivated;

	private enum State
	{
		None,
		Idle,
		AimLoop = 3,
		Shot,
		Reload,
		NoArrowIdle
	}
}
