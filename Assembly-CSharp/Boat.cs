using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class Boat : Trigger
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Floaters = new Floater[4];
		for (int i = 0; i < 4; i++)
		{
			this.m_Floaters[i] = new Floater();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_RigidBody = General.GetComponentDeepChild<Rigidbody>(base.gameObject);
		this.m_Floaters[0].m_LocalPos.x = -2f;
		this.m_Floaters[0].m_LocalPos.y = this.m_FloatersYOffset;
		this.m_Floaters[0].m_LocalPos.z = 3f;
		this.m_Floaters[1].m_LocalPos.x = 2f;
		this.m_Floaters[1].m_LocalPos.y = this.m_FloatersYOffset;
		this.m_Floaters[1].m_LocalPos.z = 3f;
		this.m_Floaters[2].m_LocalPos.x = 2f;
		this.m_Floaters[2].m_LocalPos.y = this.m_FloatersYOffset;
		this.m_Floaters[2].m_LocalPos.z = -3f;
		this.m_Floaters[3].m_LocalPos.x = -2f;
		this.m_Floaters[3].m_LocalPos.y = this.m_FloatersYOffset;
		this.m_Floaters[3].m_LocalPos.z = -3f;
		Vector3 extents = new Vector3(0.5f, 0.5f, 0.5f);
		for (int i = 0; i < 4; i++)
		{
			this.m_Floaters[i].m_ObjectRigidBody = this.m_RigidBody;
			this.m_Floaters[i].m_Bounds.extents = extents;
			this.m_Floaters[i].m_Force = this.m_FloaterForce;
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		Player.Get().GetComponent<BoatController>().SetBoat(this);
		Player.Get().StartController(PlayerControllerType.Boat);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (this.m_Occupied)
		{
			actions.Add(TriggerAction.TYPE.Exit);
			return;
		}
		actions.Add(TriggerAction.TYPE.Use);
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < 4; i++)
		{
			this.m_Floaters[i].m_LocalPos.y = this.m_FloatersYOffset;
			this.m_Floaters[i].Update();
			Vector3 a = this.m_RigidBody.transform.TransformVector(this.m_WantedVel);
			if (this.m_Floaters[i].m_InWater)
			{
				this.m_Floaters[i].m_Force = this.m_FloaterForce;
				float drivingForce = this.m_DrivingForce;
				float turningTorque = this.m_TurningTorque;
				Vector3 a2 = a - this.m_RigidBody.velocity;
				a2.Normalize();
				a2.y = 0f;
				this.m_RigidBody.AddForce(a2 * drivingForce);
				Vector3 a3 = this.m_RigidBody.transform.InverseTransformVector(this.m_RigidBody.angularVelocity) - this.m_WantedAngVel;
				a3.x = (a3.z = 0f);
				a3 = this.m_RigidBody.transform.TransformVector(-a3);
				this.m_RigidBody.AddTorque(a3 * turningTorque);
			}
		}
	}

	public override void GetInfoText(ref string result)
	{
	}

	public override bool CanTrigger()
	{
		return !this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying();
	}

	private bool m_Occupied;

	[HideInInspector]
	public Vector3 m_WantedVel = Vector3.zero;

	[HideInInspector]
	public Vector3 m_WantedAngVel = Vector3.zero;

	private Rigidbody m_RigidBody;

	private Floater[] m_Floaters;

	public float m_MaxSpeed = 2f;

	public float m_MaxAngSpeed = 0.5f;

	public float m_FloaterForce = 110f;

	public float m_DrivingForce = 100f;

	public float m_TurningTorque = 20f;

	private float m_FloatersYOffset = 0.8f;
}
