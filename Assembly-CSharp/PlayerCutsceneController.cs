using System;
using UnityEngine;

public class PlayerCutsceneController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		this.m_ControllerType = PlayerControllerType.PlayerCutscene;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Player.GetComponent<Rigidbody>().useGravity = false;
		this.m_Player.GetComponent<Rigidbody>().isKinematic = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Player.GetComponent<Rigidbody>().useGravity = true;
		this.m_Player.GetComponent<Rigidbody>().isKinematic = true;
	}
}
