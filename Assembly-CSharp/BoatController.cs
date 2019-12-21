using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class BoatController : PlayerController
{
	public static BoatController Get()
	{
		return BoatController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		BoatController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.Boat;
		this.m_CharacterController = base.GetComponent<CharacterControllerProxy>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_BodyRotationBonesParams.Clear();
		this.m_BodyRotationBonesParams[base.gameObject.transform.FindDeepChild("mixamorig:Spine")] = 0f;
		this.m_BodyRotationBonesParams[base.gameObject.transform.FindDeepChild("mixamorig:Spine1")] = 0f;
		this.m_BodyRotationBonesParams[base.gameObject.transform.FindDeepChild("mixamorig:Spine2")] = 0f;
		this.m_State = BoatControllerState.Entering;
		this.m_BoatRigidBody = General.GetComponentDeepChild<Rigidbody>(this.m_Boat.gameObject);
		Collider componentDeepChild = General.GetComponentDeepChild<Collider>(this.m_Boat.gameObject);
		Collider collider = this.m_Player.m_Collider;
		Physics.IgnoreCollision(componentDeepChild, collider);
		this.m_AnimState = BoatControllerAnimationState.Idle;
		this.m_BoatStick = ItemsManager.Get().CreateItem("Boat_Stick", false);
		this.m_Player.SetWantedItem(Hand.Right, this.m_BoatStick, true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Player.gameObject.transform.parent = null;
		this.m_Animator.SetInteger(this.m_IBoat, 0);
		Collider componentDeepChild = General.GetComponentDeepChild<Collider>(this.m_Boat.gameObject);
		Collider collider = this.m_Player.m_Collider;
		Physics.IgnoreCollision(componentDeepChild, collider, false);
		if (this.m_BoatStick != null)
		{
			UnityEngine.Object.Destroy(this.m_BoatStick.gameObject);
			this.m_BoatStick = null;
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		this.UpdateState();
		this.UpdateAnimator();
	}

	private void UpdateState()
	{
		BoatControllerState state = this.m_State;
		if (state != BoatControllerState.Entering)
		{
			if (state != BoatControllerState.Steering)
			{
				return;
			}
			this.UpdateSteering();
		}
		else
		{
			Vector3 forward = this.m_Boat.gameObject.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 forward2 = this.m_CharacterController.transform.forward;
			Vector3 position = this.m_Boat.gameObject.transform.position;
			position.y = 0f;
			Vector3 position2 = this.m_Player.gameObject.transform.position;
			position2.y = 0f;
			if (Vector3.Dot(forward, forward2) > 0.95f && position.Distance(position2) < 0.1f)
			{
				this.m_State = BoatControllerState.Steering;
				this.m_BoatRigidBody.isKinematic = false;
				this.m_BoatRigidBody.useGravity = true;
				return;
			}
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		if (this.m_State == BoatControllerState.Entering)
		{
			this.UpdatePosition();
			this.UpdateRotation();
			return;
		}
		if (this.m_State == BoatControllerState.Steering)
		{
			this.UpdatePlayerTransform();
		}
	}

	private void UpdatePosition()
	{
		Vector3 a = this.m_Boat.gameObject.transform.position - this.m_CharacterController.transform.position;
		a.y = 0f;
		this.m_CharacterController.Move(a * Time.deltaTime * 3f, true);
	}

	private void UpdateRotation()
	{
		Vector3 forward = this.m_Boat.gameObject.transform.forward;
		forward.Normalize();
		Quaternion b = Quaternion.LookRotation(forward, Vector3.up);
		this.m_CharacterController.transform.rotation = Quaternion.Slerp(this.m_CharacterController.transform.rotation, b, 0.9f);
	}

	private void UpdateSteering()
	{
		this.UpdateInputs();
		if (this.m_State != BoatControllerState.Steering)
		{
			return;
		}
		float maxSpeed = this.m_Boat.m_MaxSpeed;
		float maxAngSpeed = this.m_Boat.m_MaxAngSpeed;
		Vector3 vector = this.m_Boat.m_WantedVel;
		Vector3 vector2 = this.m_Boat.m_WantedAngVel;
		if (this.m_Inputs.m_Vertical > 0.5f)
		{
			vector += Vector3.forward * 3f * Time.deltaTime;
			this.m_AnimState = BoatControllerAnimationState.RowForward;
		}
		else if (this.m_Inputs.m_Vertical < -0.5f)
		{
			vector += Vector3.back * 3f * Time.deltaTime;
			this.m_AnimState = BoatControllerAnimationState.RowBackward;
		}
		else
		{
			vector *= Time.deltaTime;
			this.m_AnimState = BoatControllerAnimationState.Idle;
		}
		if (this.m_Inputs.m_Horizontal > 0.5f)
		{
			vector2 += Vector3.up * 3f * Time.deltaTime;
		}
		else if (this.m_Inputs.m_Horizontal < -0.5f)
		{
			vector2 -= Vector3.up * 3f * Time.deltaTime;
		}
		else
		{
			vector2 *= Time.deltaTime;
		}
		if (vector.magnitude > maxSpeed)
		{
			vector *= maxSpeed / vector.magnitude;
		}
		if (vector2.magnitude > maxAngSpeed)
		{
			vector2 *= maxAngSpeed / vector2.magnitude;
		}
		this.m_Boat.m_WantedVel = vector;
		this.m_Boat.m_WantedAngVel = vector2;
	}

	private void UpdateInputs()
	{
		this.m_Inputs.m_Horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
		this.m_Inputs.m_Vertical = CrossPlatformInputManager.GetAxis("Vertical");
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Quit))
		{
			this.m_AnimState = BoatControllerAnimationState.None;
			this.m_State = BoatControllerState.Leaving;
			this.Stop();
		}
	}

	public void SetBoat(Boat boat)
	{
		this.m_Boat = boat;
	}

	public override Dictionary<Transform, float> GetBodyRotationBonesParams()
	{
		return this.m_BodyRotationBonesParams;
	}

	private void UpdateAnimator()
	{
		this.m_Animator.SetInteger(this.m_IBoat, (int)this.m_AnimState);
	}

	private void UpdatePlayerTransform()
	{
		this.m_Player.gameObject.transform.position = this.m_Boat.gameObject.transform.position;
		this.m_Player.gameObject.transform.rotation = this.m_Boat.gameObject.transform.rotation;
	}

	private Boat m_Boat;

	private Dictionary<Transform, float> m_BodyRotationBonesParams = new Dictionary<Transform, float>();

	private BoatControllerState m_State;

	private CharacterControllerProxy m_CharacterController;

	private BoatControllerInputs m_Inputs = new BoatControllerInputs();

	private Rigidbody m_BoatRigidBody;

	private int m_IBoat = Animator.StringToHash("Boat");

	private BoatControllerAnimationState m_AnimState;

	private Item m_BoatStick;

	private static BoatController s_Instance;
}
