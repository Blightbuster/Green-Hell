using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class BowTrap : Construction, IAnimationEventsReceiver, IItemSlotParent, ITrapTriggerOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Animator = base.gameObject.GetComponent<Animator>();
		this.m_AudioSource = base.gameObject.GetComponent<AudioSource>();
		this.m_ArrowSlot = base.gameObject.GetComponentInChildren<ItemSlot>();
		this.m_Colliders = base.gameObject.GetComponents<Collider>();
	}

	protected override void Start()
	{
		base.Start();
		this.m_ArrowSlot.m_ActivityUpdate = false;
		if (this.m_Armed)
		{
			this.m_ArrowSlot.Activate(true);
		}
		this.m_ArrowHolder.gameObject.SetActive(false);
		base.gameObject.GetComponent<AnimationEventsReceiver>().Initialize(null);
		HintsManager.Get().ShowHint("BowTrap_InsertArrow", 10f);
	}

	public override string GetIconName()
	{
		return "HUD_arming_trap";
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !this.m_Armed && !this.m_Arrow;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (!this.m_Armed)
		{
			actions.Add(TriggerAction.TYPE.Arm);
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Arm)
		{
			this.Arm(true);
		}
	}

	private void SetArrow(Item item)
	{
		this.m_Arrow = item;
		if (this.m_Arrow)
		{
			foreach (Collider collider in this.m_Colliders)
			{
				Physics.IgnoreCollision(this.m_Arrow.m_BoxCollider, collider);
			}
		}
	}

	protected void Arm(bool play_sound = true)
	{
		this.m_Animator.SetTrigger(this.m_ArmHash);
		this.m_ArrowSlot.Activate(true);
		if (this.m_ArmSoundClips != null && this.m_ArmSoundClips.Count > 0)
		{
			this.m_AudioSource.PlayOneShot(this.m_ArmSoundClips[UnityEngine.Random.Range(0, this.m_ArmSoundClips.Count - 1)]);
		}
		this.m_Armed = true;
	}

	public void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.BowTrapShootArrow)
		{
			this.ShootArrow();
		}
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public bool ForceReceiveAnimEvent()
	{
		return false;
	}

	public void OnEnterTrigger(GameObject obj)
	{
		Being being = obj.IsPlayer() ? Player.Get() : obj.GetComponent<Being>();
		if (being)
		{
			this.Shot(being, true);
		}
	}

	private void Shot(Being being, bool play_sound = true)
	{
		if (!this.m_Armed)
		{
			return;
		}
		this.m_Target = being;
		this.m_Animator.SetTrigger(this.m_ShotHash);
		this.m_ArrowSlot.Activate(false);
		if (play_sound && this.m_ShotSoundClips != null && this.m_ShotSoundClips.Count > 0)
		{
			this.m_AudioSource.PlayOneShot(this.m_ShotSoundClips[UnityEngine.Random.Range(0, this.m_ShotSoundClips.Count - 1)]);
		}
		this.m_Armed = false;
	}

	private void ShootArrow()
	{
		if (!this.m_Arrow)
		{
			return;
		}
		Item arrow = this.m_Arrow;
		this.m_ArrowSlot.RemoveItem();
		arrow.StaticPhxRequestReset();
		arrow.UpdatePhx();
		arrow.gameObject.SetActive(true);
		if (this.m_Target && (this.m_Target.IsPlayer() || this.m_Target.IsHumanAI()))
		{
			arrow.transform.rotation = Quaternion.LookRotation((this.m_Target.GetHeadTransform().position - arrow.transform.position).normalized);
		}
		else
		{
			arrow.transform.rotation = Quaternion.LookRotation(-this.m_ArrowHolder.transform.right);
		}
		arrow.m_RequestThrow = true;
		arrow.m_Thrower = base.gameObject;
	}

	public bool CanInsertItem(Item item)
	{
		return !this.m_Arrow;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		this.SetArrow(slot.m_Item);
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (slot.m_Item == this.m_Arrow)
		{
			this.SetArrow(null);
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("BowTrapArmed" + index, this.m_Armed);
	}

	public override void Load(int index)
	{
		base.Load(index);
		if (SaveGame.LoadBVal("BowTrapArmed" + index))
		{
			this.Arm(false);
		}
		else
		{
			this.m_ArrowSlot.Activate(false);
			this.m_Animator.SetTrigger(this.m_UnarmHash);
			this.m_Armed = false;
		}
		this.m_Animator.ResetTrigger(this.m_ArmHash);
		this.m_Animator.ResetTrigger(this.m_ShotHash);
	}

	public override bool TriggerThrough()
	{
		return this.m_Armed;
	}

	private int m_ArmHash = Animator.StringToHash("Arm");

	private int m_UnarmHash = Animator.StringToHash("Unarm");

	private int m_ShotHash = Animator.StringToHash("Shot");

	private bool m_Armed = true;

	private Item m_Arrow;

	public GameObject m_ArrowHolder;

	[HideInInspector]
	public Being m_Target;

	private Animator m_Animator;

	private AudioSource m_AudioSource;

	public List<AudioClip> m_ArmSoundClips;

	public List<AudioClip> m_ShotSoundClips;

	private ItemSlot m_ArrowSlot;

	private Collider[] m_Colliders;
}
