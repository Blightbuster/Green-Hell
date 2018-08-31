using System;
using System.Collections.Generic;
using UnityEngine;

public class CutscenePlayerController : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.m_Animator = base.gameObject.GetComponent<Animator>();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return null;
	}

	public virtual bool IsActive()
	{
		return base.enabled;
	}

	protected PlayerControllerType m_ControllerType = PlayerControllerType.Unknown;

	[HideInInspector]
	public Animator m_Animator;
}
