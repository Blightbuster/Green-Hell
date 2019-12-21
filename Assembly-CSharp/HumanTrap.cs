using System;
using System.Collections.Generic;
using AIs;
using Enums;
using UnityEngine;

public class HumanTrap : Construction
{
	protected override void Start()
	{
		base.Start();
		this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/killer_trap_arm_01"));
		this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/killer_trap_arm_02"));
		this.m_ArmSoundClips.Add((AudioClip)Resources.Load("Sounds/Traps/killer_trap_arm_04"));
		this.m_Animation = base.gameObject.GetComponentInChildren<Animation>();
		this.m_Animation.Play(this.m_IdleAnimName);
		this.m_Joint = this.m_JointObject.GetComponent<Joint>();
		this.SetArmed(this.m_ArmOnStart);
	}

	public override bool CheckDot()
	{
		return false;
	}

	public override string GetIconName()
	{
		return "HUD_arming_trap";
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !this.m_Armed;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		actions.Add(TriggerAction.TYPE.Arm);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		this.PlayArmSound();
		this.SetArmed(true);
	}

	private void SetArmed(bool armed)
	{
		this.m_Armed = armed;
		this.m_Animation.CrossFade(armed ? this.m_IdleAnimName : this.m_FireAnimName, armed ? 1f : 0f);
		this.m_Area.enabled = armed;
		if (armed)
		{
			this.m_Joint.connectedBody = null;
			if (this.m_Body)
			{
				this.m_Body.AddForceToRagdoll(Vector3.up * 0.5f);
				this.m_Body = null;
			}
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("TrapArmed" + index, this.m_Armed);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.SetArmed(SaveGame.LoadBVal("TrapArmed" + index));
	}

	private void PlayArmSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		this.m_AudioSource.PlayOneShot(this.m_ArmSoundClips[UnityEngine.Random.Range(0, this.m_ArmSoundClips.Count - 1)]);
	}

	public void OnEnter(GameObject obj)
	{
		if (obj.IsPlayer())
		{
			this.Fire(null);
			Player.Get().GiveDamage(base.gameObject, this, 10f, -base.transform.forward, DamageType.None, 0, false);
			return;
		}
		HumanAI component = obj.GetComponent<HumanAI>();
		if (component)
		{
			this.Fire(component);
		}
	}

	private void Fire(HumanAI ai)
	{
		this.m_Animation.CrossFade(this.m_FireAnimName);
		this.SetArmed(false);
		if (ai)
		{
			ai.GiveDamage(base.gameObject, this, 9999f, -base.transform.forward, DamageType.None, 0, false);
			this.m_Body = ai.gameObject.GetComponent<DeadBody>();
			this.m_Joint.connectedBody = ai.m_KillerTrapJoint;
		}
	}

	public bool m_ArmOnStart = true;

	private bool m_Armed;

	private DeadBody m_Body;

	private Animation m_Animation;

	private string m_FireAnimName = "Fire";

	private string m_IdleAnimName = "Idle";

	private AudioSource m_AudioSource;

	private List<AudioClip> m_ArmSoundClips = new List<AudioClip>();

	public BoxCollider m_Area;

	public GameObject m_JointObject;

	private Joint m_Joint;
}
