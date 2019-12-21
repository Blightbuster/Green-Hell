using System;
using System.Collections.Generic;
using AIs;
using UnityEngine;
using UnityEngine.AI;

public class Spikes : Construction, ITrapTriggerOwner, IItemSlotParent
{
	protected override void Awake()
	{
		base.Awake();
		base.StaticPhxRequestAdd();
		this.m_DamageInfo.m_Damager = base.gameObject;
		this.m_DamageInfo.m_Normal = -Vector3.up;
		this.m_DamageInfo.m_DamageItem = this;
		this.m_DamageInfo.m_HitDir = Vector3.up + ((UnityEngine.Random.Range(0f, 1f) > 0.5f) ? Vector3.right : Vector3.left) * 0.1f;
		this.m_MaskSlot = base.gameObject.GetComponentInChildren<ItemSlot>();
		this.m_MaskSlot.m_ActivityUpdate = false;
		this.m_Obstacle = base.gameObject.GetComponent<NavMeshObstacle>();
		this.m_Obstacle.enabled = true;
		this.m_DamageCollider.isTrigger = true;
		foreach (GameObject gameObject in this.m_MaskObjects)
		{
			gameObject.SetActive(true);
		}
		this.m_Mask = true;
		this.m_ArmSound = (AudioClip)Resources.Load("Sounds/Traps/snare_trap_arm_03");
		this.m_UnarmSound = (AudioClip)Resources.Load("Sounds/Traps/spike_trap_triggered_01");
		this.Arm(false);
	}

	private void Arm(bool play_sound = true)
	{
		this.m_ArmedObj.SetActive(true);
		this.m_UnarmedObj.SetActive(false);
		if (play_sound)
		{
			if (this.m_AudioSource == null)
			{
				this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
			}
			this.m_AudioSource.PlayOneShot(this.m_ArmSound);
		}
		this.m_Armed = true;
	}

	private void Unarm(bool play_sound = true)
	{
		this.m_ArmedObj.SetActive(false);
		this.m_UnarmedObj.SetActive(true);
		if (this.m_MaskSlot.m_Item)
		{
			UnityEngine.Object.Destroy(this.m_MaskSlot.m_Item.gameObject);
		}
		this.m_MaskSlot.Activate(true);
		foreach (GameObject gameObject in this.m_MaskObjects)
		{
			gameObject.SetActive(false);
		}
		this.m_Mask = false;
		if (play_sound)
		{
			if (this.m_AudioSource == null)
			{
				this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
			}
			this.m_AudioSource.PlayOneShot(this.m_UnarmSound);
		}
		this.m_Armed = false;
	}

	public override bool IsSpikes()
	{
		return true;
	}

	public void OnEnterTrigger(GameObject obj)
	{
		if (!this.m_Armed)
		{
			return;
		}
		Being being = obj.IsPlayer() ? Player.Get() : obj.gameObject.GetComponent<Being>();
		if (being)
		{
			AI component = obj.gameObject.GetComponent<AI>();
			if (component && !component.m_Params.m_BigAnimal && !being.IsHumanAI())
			{
				return;
			}
			if (being.IsPlayer())
			{
				this.m_DamageInfo.m_Damage = this.m_PlayerDamage;
			}
			else if (being.IsHumanAI())
			{
				this.m_DamageInfo.m_Damage = this.m_HumanDamage;
			}
			else
			{
				this.m_DamageInfo.m_Damage = this.m_AnimalDamage;
			}
			being.TakeDamage(this.m_DamageInfo);
			this.Unarm(true);
		}
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !this.m_Armed && this.m_Mask;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
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

	protected override void Update()
	{
		base.Update();
		bool flag = this.m_Armed && !this.m_Mask;
		if (this.m_Obstacle.enabled != flag)
		{
			this.m_Obstacle.enabled = flag;
		}
		if (this.m_Mask == this.m_MaskSlot.m_Active)
		{
			this.m_MaskSlot.Activate(!this.m_Mask);
		}
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		slot.m_Item.enabled = false;
		foreach (GameObject gameObject in this.m_MaskObjects)
		{
			gameObject.SetActive(true);
		}
		this.m_Mask = true;
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public override string GetIconName()
	{
		return "HUD_arming_trap";
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("SpikesArmed" + index, this.m_Armed);
		SaveGame.SaveVal("SpikesMask" + index, this.m_Mask);
	}

	public override void Load(int index)
	{
		base.Load(index);
		if (SaveGame.LoadBVal("SpikesArmed" + index))
		{
			this.Arm(false);
		}
		else
		{
			this.Unarm(false);
		}
		bool flag = SaveGame.LoadBVal("SpikesMask" + index);
		foreach (GameObject gameObject in this.m_MaskObjects)
		{
			gameObject.SetActive(flag);
		}
		this.m_Mask = flag;
		this.m_MaskSlot.Activate(!this.m_Mask);
	}

	public GameObject m_ArmedObj;

	public GameObject m_UnarmedObj;

	private bool m_Armed;

	public BoxCollider m_DamageCollider;

	public float m_PlayerDamage;

	public float m_HumanDamage;

	public float m_AnimalDamage;

	private DamageInfo m_DamageInfo = new DamageInfo();

	private ItemSlot m_MaskSlot;

	public List<GameObject> m_MaskObjects;

	private bool m_Mask;

	private NavMeshObstacle m_Obstacle;

	private AudioSource m_AudioSource;

	private AudioClip m_ArmSound;

	private AudioClip m_UnarmSound;
}
