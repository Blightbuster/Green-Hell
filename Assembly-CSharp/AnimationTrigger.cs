using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour, ITriggerOwner
{
	private void Awake()
	{
		this.m_Animator = base.gameObject.GetComponent<Animator>();
		this.m_PlayerAnimationHash = Animator.StringToHash(this.m_PlayerAnimationName);
		this.m_AnimationHash = Animator.StringToHash(this.m_AnimationName);
		this.m_AudioSource = base.gameObject.GetComponent<AudioSource>();
	}

	private void Start()
	{
		this.m_Parent = base.gameObject.GetComponent<Trigger>();
		this.m_InfoLocalized = this.m_Parent.GetTriggerInfoLocalized();
		this.m_IconName = this.m_Parent.GetIconName();
		this.m_Parent.SetOwner(this);
		AnimationEventsReceiver component = base.gameObject.GetComponent<AnimationEventsReceiver>();
		if (component != null)
		{
			component.Initialize(null);
		}
	}

	public void PlayAnim()
	{
		this.m_Animator.CrossFade(this.m_AnimationHash, 0f);
	}

	public bool CanTrigger(Trigger trigger)
	{
		return base.enabled;
	}

	public void OnExecute(Trigger trigger, TriggerAction.TYPE action)
	{
		AnimationTriggerController.Get().SetTrigger(this);
		Player.Get().StartController(PlayerControllerType.AnimationTrigger);
		if (this.m_Sound && this.m_AudioSource)
		{
			this.m_AudioSource.clip = this.m_Sound;
			this.m_AudioSource.Play();
		}
	}

	public void GetActions(Trigger trigger, List<TriggerAction.TYPE> actions)
	{
		actions.Add(this.m_Action);
	}

	public string GetTriggerInfoLocalized(Trigger trigger)
	{
		return this.m_InfoLocalized;
	}

	public string GetIconName(Trigger trigger)
	{
		return this.m_IconName;
	}

	public bool WasTriggered()
	{
		return this.m_Parent.WasTriggered();
	}

	public void ResetTrigger()
	{
		this.m_Parent.ResetTrigger();
	}

	private Trigger m_Parent;

	[HideInInspector]
	public Animator m_Animator;

	public TriggerAction.TYPE m_Action = TriggerAction.TYPE.None;

	public string m_AnimationName = string.Empty;

	private int m_AnimationHash = -1;

	public string m_PlayerAnimationName = string.Empty;

	[HideInInspector]
	public int m_PlayerAnimationHash = -1;

	public Transform m_TransformObject;

	private string m_InfoLocalized = string.Empty;

	private string m_IconName = string.Empty;

	public string m_ScenarioBoolVariable = string.Empty;

	public AudioClip m_Sound;

	private AudioSource m_AudioSource;
}
