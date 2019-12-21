using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ReplicatedTransform : ReplicatedBehaviour, IAddComponentEventListener
{
	private Renderer RendererComponent
	{
		get
		{
			return this.m_Renderer.Get(this);
		}
	}

	public ReplicatedTransform.TransformSyncMode transformSyncMode
	{
		get
		{
			if (this.m_TransformSyncMode != ReplicatedTransform.TransformSyncMode.Auto)
			{
				return this.m_TransformSyncMode;
			}
			if (this.m_CharacterController != null)
			{
				return ReplicatedTransform.TransformSyncMode.SyncCharacterController;
			}
			if (this.m_RigidBody3D != null)
			{
				return ReplicatedTransform.TransformSyncMode.SyncRigidbody3D;
			}
			if (this.m_RigidBody2D != null)
			{
				return ReplicatedTransform.TransformSyncMode.SyncRigidbody2D;
			}
			return ReplicatedTransform.TransformSyncMode.SyncTransform;
		}
		set
		{
			if (this.m_TransformSyncMode != value)
			{
				this.m_TransformSyncMode = value;
				if (Application.isPlaying)
				{
					this.CacheComponents();
				}
			}
		}
	}

	public ReplicatedTransform.AxisSyncMode syncRotationAxis
	{
		get
		{
			return this.m_SyncRotationAxis;
		}
		set
		{
			this.m_SyncRotationAxis = value;
		}
	}

	public ReplicatedTransform.CompressionSyncMode rotationSyncCompression
	{
		get
		{
			return this.m_RotationSyncCompression;
		}
		set
		{
			this.m_RotationSyncCompression = value;
		}
	}

	public bool syncSpin
	{
		get
		{
			return this.m_SyncSpin;
		}
		set
		{
			this.m_SyncSpin = value;
		}
	}

	public float movementTheshold
	{
		get
		{
			return this.m_MovementTheshold;
		}
		set
		{
			this.m_MovementTheshold = value;
		}
	}

	public float rotationTheshold
	{
		get
		{
			return this.m_RotationTheshold;
		}
		set
		{
			this.m_RotationTheshold = value;
		}
	}

	public float snapThreshold
	{
		get
		{
			return this.m_SnapThreshold;
		}
		set
		{
			this.m_SnapThreshold = value;
		}
	}

	public float interpolateRotation
	{
		get
		{
			return this.m_InterpolateRotation;
		}
		set
		{
			this.m_InterpolateRotation = value;
		}
	}

	public float interpolateMovement
	{
		get
		{
			return this.m_InterpolateMovement;
		}
		set
		{
			this.m_InterpolateMovement = value;
		}
	}

	public ReplicatedTransform.ClientMoveCallback3D clientMoveCallback3D
	{
		get
		{
			return this.m_MoveCallback3D;
		}
		set
		{
			this.m_MoveCallback3D = value;
		}
	}

	public ReplicatedTransform.ClientMoveCallback2D clientMoveCallback2D
	{
		get
		{
			return this.m_ClientMoveCallback2D;
		}
		set
		{
			this.m_ClientMoveCallback2D = value;
		}
	}

	public CharacterController characterContoller
	{
		get
		{
			return this.m_CharacterController;
		}
	}

	public Rigidbody rigidbody3D
	{
		get
		{
			return this.m_RigidBody3D;
		}
	}

	public Rigidbody2D rigidbody_2D
	{
		get
		{
			return this.m_RigidBody2D;
		}
	}

	public float lastSyncTime
	{
		get
		{
			return this.m_LastSyncTime;
		}
	}

	public Vector3 targetSyncPosition
	{
		get
		{
			return this.m_TargetSyncPosition;
		}
	}

	public Vector3 targetSyncVelocity
	{
		get
		{
			return this.m_TargetSyncVelocity;
		}
	}

	public Quaternion targetSyncRotation3D
	{
		get
		{
			return this.m_TargetSyncRotation3D;
		}
	}

	public float targetSyncRotation2D
	{
		get
		{
			return this.m_TargetSyncRotation2D;
		}
	}

	public bool grounded
	{
		get
		{
			return this.m_Grounded;
		}
		set
		{
			this.m_Grounded = value;
		}
	}

	public bool preferSimulatedVerticalVelocity
	{
		get
		{
			if (!this.m_PreferSimulatedVerticalVelocity)
			{
				return false;
			}
			if (this.rigidbody3D && !this.rigidbody3D.useGravity)
			{
				return false;
			}
			if (this.m_ParamComponents != null)
			{
				IReplicatedTransformParams[] paramComponents = this.m_ParamComponents;
				for (int i = 0; i < paramComponents.Length; i++)
				{
					if (!paramComponents[i].CanUseSimulatedVerticalVelocity())
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	private void Awake()
	{
		this.CacheComponents();
	}

	private void OnDisable()
	{
		if (this.ReplIsDirty())
		{
			return;
		}
		if (!base.ReplIsOwner())
		{
			return;
		}
		this.ReplSetDirty();
	}

	private void CacheComponents()
	{
		this.m_RigidBody3D = base.GetComponent<Rigidbody>();
		this.m_RigidBody2D = base.GetComponent<Rigidbody2D>();
		this.m_CharacterController = base.GetComponent<CharacterController>();
		this.m_PrevPosition = base.transform.position;
		this.m_PrevRotation = base.transform.rotation;
		this.m_PrevVelocity = 0f;
		this.m_AttachSynchronizer = base.GetComponent<AttachmentSynchronizer>();
		this.m_ParamComponents = base.GetComponents<IReplicatedTransformParams>();
	}

	public override void OnReplicationPrepare()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		this.UpdateIsDirty();
	}

	private void UpdateIsDirty()
	{
		if (this.ReplIsDirty())
		{
			return;
		}
		if (this.IsAttached())
		{
			return;
		}
		Transform transform = base.transform;
		if ((transform.position - this.m_PrevPosition).magnitude < this.movementTheshold)
		{
			if (this.syncRotationAxis == ReplicatedTransform.AxisSyncMode.None)
			{
				return;
			}
			if (Quaternion.Angle(this.m_PrevRotation, transform.rotation) < this.rotationTheshold)
			{
				return;
			}
		}
		this.ReplSetDirty();
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initialState)
	{
		if (this.IsAttached())
		{
			writer.Write('\0');
			return;
		}
		writer.Write((char)this.transformSyncMode);
		switch (this.transformSyncMode)
		{
		case ReplicatedTransform.TransformSyncMode.SyncNone:
			return;
		case ReplicatedTransform.TransformSyncMode.SyncTransform:
			this.SerializeModeTransform(writer);
			return;
		case ReplicatedTransform.TransformSyncMode.SyncRigidbody2D:
			this.SerializeMode2D(writer);
			return;
		case ReplicatedTransform.TransformSyncMode.SyncRigidbody3D:
			this.SerializeMode3D(writer);
			return;
		case ReplicatedTransform.TransformSyncMode.SyncCharacterController:
			this.SerializeModeCharacterController(writer);
			return;
		default:
			return;
		}
	}

	private void SerializeModeTransform(P2PNetworkWriter writer)
	{
		writer.Write(base.transform.position);
		if (this.m_SyncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
		{
			ReplicatedTransform.SerializeRotation3D(writer, base.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
		}
		this.m_PrevPosition = base.transform.position;
		this.m_PrevRotation = base.transform.rotation;
		this.m_PrevVelocity = 0f;
	}

	private void SerializeMode3D(P2PNetworkWriter writer)
	{
		if (base.gameObject.activeSelf)
		{
			writer.Write(this.m_RigidBody3D.worldCenterOfMass);
		}
		else
		{
			writer.Write(base.transform.position);
		}
		ReplicatedTransform.SerializeVelocity3D(writer, this.m_RigidBody3D.velocity, ReplicatedTransform.CompressionSyncMode.None);
		if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
		{
			if (base.gameObject.activeSelf)
			{
				ReplicatedTransform.SerializeRotation3D(writer, this.m_RigidBody3D.rotation, this.syncRotationAxis, this.rotationSyncCompression);
			}
			else
			{
				ReplicatedTransform.SerializeRotation3D(writer, base.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
			}
		}
		if (this.m_SyncSpin)
		{
			ReplicatedTransform.SerializeSpin3D(writer, this.m_RigidBody3D.angularVelocity, this.syncRotationAxis, this.rotationSyncCompression);
		}
		this.m_PrevPosition = (base.gameObject.activeSelf ? this.m_RigidBody3D.position : base.transform.position);
		this.m_PrevRotation = (base.gameObject.activeSelf ? this.m_RigidBody3D.rotation : base.transform.rotation);
		this.m_PrevVelocity = this.m_RigidBody3D.velocity.sqrMagnitude;
	}

	private void SerializeModeCharacterController(P2PNetworkWriter writer)
	{
		writer.Write(base.transform.position);
		if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
		{
			ReplicatedTransform.SerializeRotation3D(writer, base.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
		}
		this.m_PrevPosition = base.transform.position;
		this.m_PrevRotation = base.transform.rotation;
		this.m_PrevVelocity = 0f;
	}

	private void SerializeMode2D(P2PNetworkWriter writer)
	{
		writer.Write(this.m_RigidBody2D.position);
		ReplicatedTransform.SerializeVelocity2D(writer, this.m_RigidBody2D.velocity, ReplicatedTransform.CompressionSyncMode.None);
		if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
		{
			float num = this.m_RigidBody2D.rotation % 360f;
			if (num < 0f)
			{
				num += 360f;
			}
			ReplicatedTransform.SerializeRotation2D(writer, num, this.rotationSyncCompression);
		}
		if (this.m_SyncSpin)
		{
			ReplicatedTransform.SerializeSpin2D(writer, this.m_RigidBody2D.angularVelocity, this.rotationSyncCompression);
		}
		this.m_PrevPosition = this.m_RigidBody2D.position;
		this.m_PrevRotation = base.transform.rotation;
		this.m_PrevVelocity = this.m_RigidBody2D.velocity.sqrMagnitude;
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		switch (reader.ReadChar())
		{
		case '\0':
			return;
		case '\u0001':
			this.UnserializeModeTransform(reader, initial_state);
			break;
		case '\u0002':
			this.UnserializeMode2D(reader, initial_state);
			break;
		case '\u0003':
			this.UnserializeMode3D(reader, initial_state);
			break;
		case '\u0004':
			this.UnserializeModeCharacterController(reader, initial_state);
			break;
		}
		this.m_LastSyncTime = Time.time;
	}

	private void UnserializeModeTransform(P2PNetworkReader reader, bool initial_state)
	{
		if (initial_state)
		{
			this.m_TargetSyncPosition = (base.transform.position = reader.ReadVector3());
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation3D = (base.transform.rotation = ReplicatedTransform.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression));
			}
			this.m_FixedPosDiff = Vector3.zero;
			return;
		}
		this.m_TargetSyncPosition = reader.ReadVector3();
		if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
		{
			this.m_TargetSyncRotation3D = ReplicatedTransform.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
		}
		Vector3 a = (this.m_TargetSyncPosition - base.transform.position) / this.ReplGetReplicationInterval();
		this.m_FixedPosDiff = a * Time.fixedDeltaTime;
	}

	private void UnserializeMode3D(P2PNetworkReader reader, bool initial_state)
	{
		if (this.m_MoveCallback3D != null)
		{
			Vector3 targetSyncPosition = reader.ReadVector3();
			Vector3 targetSyncVelocity = reader.ReadVector3();
			Quaternion targetSyncRotation3D = Quaternion.identity;
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				targetSyncRotation3D = ReplicatedTransform.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
			}
			if (!this.m_MoveCallback3D(ref targetSyncPosition, ref targetSyncVelocity, ref targetSyncRotation3D))
			{
				return;
			}
			this.m_TargetSyncPosition = targetSyncPosition;
			this.m_TargetSyncVelocity = targetSyncVelocity;
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation3D = targetSyncRotation3D;
			}
		}
		else
		{
			this.m_TargetSyncPosition = reader.ReadVector3();
			if (this.m_RigidBody3D != null)
			{
				this.m_TargetSyncTransformPosition = this.m_RigidBody3D.position - this.m_RigidBody3D.worldCenterOfMass + this.m_TargetSyncPosition;
			}
			else
			{
				Renderer rendererComponent = this.RendererComponent;
				if (rendererComponent != null)
				{
					this.m_TargetSyncTransformPosition = base.transform.position - rendererComponent.bounds.center + this.m_TargetSyncPosition;
				}
			}
			this.m_TargetSyncVelocity = reader.ReadVector3();
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation3D = ReplicatedTransform.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
			}
		}
		if (this.syncSpin)
		{
			this.m_TargetSyncAngularVelocity3D = ReplicatedTransform.UnserializeSpin3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
		}
		if (!base.gameObject.activeSelf || initial_state)
		{
			base.transform.position = this.m_TargetSyncTransformPosition;
			base.transform.rotation = this.m_TargetSyncRotation3D;
		}
		if (this.m_RigidBody3D != null)
		{
			if (this.ReplGetReplicationInterval() <= 0f)
			{
				this.m_RigidBody3D.MovePosition(this.m_TargetSyncTransformPosition);
				this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
				if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
				{
					this.m_RigidBody3D.MoveRotation(this.m_TargetSyncRotation3D);
				}
				if (this.syncSpin)
				{
					this.m_RigidBody3D.angularVelocity = this.m_TargetSyncAngularVelocity3D;
				}
				return;
			}
			if ((this.m_RigidBody3D.worldCenterOfMass - this.m_TargetSyncPosition).magnitude > this.snapThreshold)
			{
				this.m_RigidBody3D.MovePosition(this.m_TargetSyncTransformPosition);
				this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
			}
			if (this.interpolateRotation == 0f && this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_RigidBody3D.rotation = this.m_TargetSyncRotation3D;
				if (this.syncSpin)
				{
					this.m_RigidBody3D.angularVelocity = this.m_TargetSyncAngularVelocity3D;
				}
			}
			if (this.m_InterpolateMovement == 0f)
			{
				this.m_RigidBody3D.position = this.m_TargetSyncTransformPosition;
			}
			if (initial_state && this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_RigidBody3D.rotation = this.m_TargetSyncRotation3D;
			}
		}
	}

	private void UnserializeMode2D(P2PNetworkReader reader, bool initialState)
	{
		if (this.m_ClientMoveCallback2D != null)
		{
			Vector2 v = reader.ReadVector2();
			Vector2 v2 = reader.ReadVector2();
			float targetSyncRotation2D = 0f;
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				targetSyncRotation2D = ReplicatedTransform.UnserializeRotation2D(reader, this.rotationSyncCompression);
			}
			if (!this.m_ClientMoveCallback2D(ref v, ref v2, ref targetSyncRotation2D))
			{
				return;
			}
			this.m_TargetSyncPosition = v;
			this.m_TargetSyncVelocity = v2;
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation2D = targetSyncRotation2D;
			}
		}
		else
		{
			this.m_TargetSyncPosition = reader.ReadVector2();
			this.m_TargetSyncVelocity = reader.ReadVector2();
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation2D = ReplicatedTransform.UnserializeRotation2D(reader, this.rotationSyncCompression);
			}
		}
		if (this.syncSpin)
		{
			this.m_TargetSyncAngularVelocity2D = ReplicatedTransform.UnserializeSpin2D(reader, this.rotationSyncCompression);
		}
		if (this.m_RigidBody2D != null)
		{
			if (this.ReplGetReplicationInterval() <= 0f)
			{
				base.transform.position = this.m_TargetSyncPosition;
				this.m_RigidBody2D.velocity = this.m_TargetSyncVelocity;
				if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
				{
					this.m_RigidBody2D.MoveRotation(this.m_TargetSyncRotation2D);
				}
				if (this.syncSpin)
				{
					this.m_RigidBody2D.angularVelocity = this.m_TargetSyncAngularVelocity2D;
				}
				return;
			}
			if ((this.m_RigidBody2D.position - this.m_TargetSyncPosition).magnitude > this.snapThreshold)
			{
				this.m_RigidBody2D.position = this.m_TargetSyncPosition;
				this.m_RigidBody2D.velocity = this.m_TargetSyncVelocity;
			}
			if (this.interpolateRotation == 0f && this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_RigidBody2D.rotation = this.m_TargetSyncRotation2D;
				if (this.syncSpin)
				{
					this.m_RigidBody2D.angularVelocity = this.m_TargetSyncAngularVelocity2D;
				}
			}
			if (this.m_InterpolateMovement == 0f)
			{
				this.m_RigidBody2D.position = this.m_TargetSyncPosition;
			}
			if (initialState)
			{
				this.m_RigidBody2D.rotation = this.m_TargetSyncRotation2D;
			}
		}
	}

	private void UnserializeModeCharacterController(P2PNetworkReader reader, bool initial_state)
	{
		if (this.m_MoveCallback3D != null)
		{
			Vector3 targetSyncPosition = reader.ReadVector3();
			Quaternion targetSyncRotation3D = Quaternion.identity;
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				targetSyncRotation3D = ReplicatedTransform.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
			}
			Vector3 targetSyncVelocity = (this.m_CharacterController != null) ? this.m_CharacterController.velocity : Vector3.zero;
			if (!this.m_MoveCallback3D(ref targetSyncPosition, ref targetSyncVelocity, ref targetSyncRotation3D))
			{
				return;
			}
			this.m_TargetSyncPosition = targetSyncPosition;
			this.m_TargetSyncVelocity = targetSyncVelocity;
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation3D = targetSyncRotation3D;
			}
		}
		else
		{
			this.m_TargetSyncPosition = reader.ReadVector3();
			if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				this.m_TargetSyncRotation3D = ReplicatedTransform.UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
			}
		}
		if (this.m_CharacterController != null)
		{
			if (initial_state || this.ReplGetReplicationInterval() <= 0f)
			{
				base.transform.position = this.m_TargetSyncPosition;
				if (this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
				{
					base.transform.rotation = this.m_TargetSyncRotation3D;
				}
				this.m_FixedPosDiff = Vector3.zero;
				return;
			}
			Vector3 a = (this.m_TargetSyncPosition - base.transform.position) / this.ReplGetReplicationInterval();
			this.m_FixedPosDiff = a * Time.fixedDeltaTime;
			if ((base.transform.position - this.m_TargetSyncPosition).magnitude > this.snapThreshold)
			{
				base.transform.position = this.m_TargetSyncPosition;
				this.m_FixedPosDiff = Vector3.zero;
			}
			if (this.interpolateRotation == 0f && this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
			{
				base.transform.rotation = this.m_TargetSyncRotation3D;
			}
			if (this.m_InterpolateMovement == 0f)
			{
				base.transform.position = this.m_TargetSyncPosition;
			}
		}
	}

	private void FixedUpdateNonOwner()
	{
		if (this.m_LastSyncTime == 0f)
		{
			return;
		}
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (this.IsAttached())
		{
			return;
		}
		if (this.m_InterpolateMaxDuration > 0f && this.ReplGetLastReplTime() < Time.time - this.m_InterpolateMaxDuration)
		{
			return;
		}
		switch (this.transformSyncMode)
		{
		case ReplicatedTransform.TransformSyncMode.SyncNone:
			return;
		case ReplicatedTransform.TransformSyncMode.SyncTransform:
			this.InterpolateTransformMode();
			return;
		case ReplicatedTransform.TransformSyncMode.SyncRigidbody2D:
			this.InterpolateTransformMode2D();
			return;
		case ReplicatedTransform.TransformSyncMode.SyncRigidbody3D:
			this.InterpolateTransformMode3D();
			return;
		case ReplicatedTransform.TransformSyncMode.SyncCharacterController:
			this.InterpolateTransformModeCharacterController();
			return;
		default:
			return;
		}
	}

	private void InterpolateTransformMode()
	{
		if (this.m_InterpolateMovement != 0f)
		{
			if (Time.time - this.m_LastSyncTime > this.ReplGetReplicationInterval() * 2f)
			{
				Vector3 a = this.m_TargetSyncPosition - base.transform.position;
				if (a.magnitude > this.movementTheshold && a.magnitude < this.snapThreshold && Time.time - this.m_LastSyncTime < this.m_InterpolateMaxDuration)
				{
					this.m_FixedPosDiff = a * Time.fixedDeltaTime;
					base.transform.position += this.m_FixedPosDiff * this.m_InterpolateMovement;
				}
				else
				{
					this.m_FixedPosDiff = Vector3.zero;
					base.transform.position = this.m_TargetSyncPosition;
				}
			}
			else
			{
				base.transform.position += this.m_FixedPosDiff * this.m_InterpolateMovement;
			}
		}
		else
		{
			base.transform.position = this.m_TargetSyncPosition;
		}
		if (this.interpolateRotation != 0f)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, this.m_TargetSyncRotation3D, Time.fixedDeltaTime * this.interpolateRotation * 10f);
			return;
		}
		base.transform.rotation = this.m_TargetSyncRotation3D;
	}

	private void InterpolateTransformMode3D()
	{
		if (this.m_RigidBody3D == null)
		{
			return;
		}
		if (this.m_InterpolateMovement != 0f)
		{
			if (this.m_RigidBody3D.isKinematic)
			{
				this.m_RigidBody3D.MovePosition(this.m_TargetSyncTransformPosition);
			}
			else if ((this.m_TargetSyncPosition - this.m_RigidBody3D.worldCenterOfMass).magnitude > this.movementTheshold)
			{
				Vector3 vector = (this.m_TargetSyncPosition - this.m_RigidBody3D.worldCenterOfMass) * this.m_InterpolateMovement / this.ReplGetReplicationInterval();
				if (this.preferSimulatedVerticalVelocity && Math.Abs(this.m_RigidBody3D.velocity.y) > Math.Abs(vector.y))
				{
					vector.y = this.m_RigidBody3D.velocity.y;
				}
				this.m_RigidBody3D.velocity = vector;
			}
		}
		if (this.interpolateRotation != 0f && this.syncRotationAxis != ReplicatedTransform.AxisSyncMode.None)
		{
			if (this.m_RigidBody3D.isKinematic)
			{
				this.m_RigidBody3D.MoveRotation(this.m_TargetSyncRotation3D);
			}
			else if (Quaternion.Angle(this.m_RigidBody3D.rotation, this.m_TargetSyncRotation3D) > this.rotationTheshold)
			{
				this.m_RigidBody3D.MoveRotation(Quaternion.Slerp(this.m_RigidBody3D.rotation, this.m_TargetSyncRotation3D, Time.fixedDeltaTime * this.interpolateRotation));
			}
		}
		this.m_TargetSyncPosition += this.m_TargetSyncVelocity * Time.fixedDeltaTime * 1f;
	}

	private void InterpolateTransformModeCharacterController()
	{
		if (this.m_CharacterController == null)
		{
			return;
		}
		if (this.m_FixedPosDiff != Vector3.zero && this.m_InterpolateMovement != 0f)
		{
			if (Time.time - this.m_LastSyncTime > this.ReplGetReplicationInterval() * 2f)
			{
				Vector3 vector = this.m_TargetSyncPosition - base.transform.position;
				if (vector.magnitude > this.movementTheshold && vector.magnitude < this.snapThreshold && Time.time - this.m_LastSyncTime < this.m_InterpolateMaxDuration)
				{
					this.m_FixedPosDiff = vector * Time.fixedDeltaTime;
					this.m_CharacterController.Move(this.m_FixedPosDiff * this.m_InterpolateMovement);
				}
				else
				{
					this.m_FixedPosDiff = Vector3.zero;
					this.m_CharacterController.Move(vector);
				}
			}
			else
			{
				this.m_CharacterController.Move(this.m_FixedPosDiff * this.m_InterpolateMovement);
			}
		}
		if (Quaternion.Angle(this.m_TargetSyncRotation3D, base.transform.rotation) > 1f && this.interpolateRotation != 0f)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, this.m_TargetSyncRotation3D, Time.fixedDeltaTime * this.interpolateRotation * 10f);
		}
	}

	private void InterpolateTransformMode2D()
	{
		if (this.m_RigidBody2D == null)
		{
			return;
		}
		if (this.m_InterpolateMovement != 0f)
		{
			Vector2 velocity = this.m_RigidBody2D.velocity;
			Vector2 vector = (this.m_TargetSyncPosition - this.m_RigidBody2D.position) * this.m_InterpolateMovement / this.ReplGetReplicationInterval();
			if (!this.m_Grounded && vector.y < 0f)
			{
				vector.y = velocity.y;
			}
			this.m_RigidBody2D.velocity = vector;
		}
		if (this.interpolateRotation != 0f)
		{
			float num = this.m_RigidBody2D.rotation % 360f;
			if (num < 0f)
			{
				num += 360f;
			}
			Quaternion quaternion = Quaternion.Slerp(base.transform.rotation, Quaternion.Euler(0f, 0f, this.m_TargetSyncRotation2D), Time.fixedDeltaTime * this.interpolateRotation / this.ReplGetReplicationInterval());
			this.m_RigidBody2D.MoveRotation(quaternion.eulerAngles.z);
			this.m_TargetSyncRotation2D += this.m_TargetSyncAngularVelocity2D * Time.fixedDeltaTime * 1f;
		}
		this.m_TargetSyncPosition += this.m_TargetSyncVelocity * Time.fixedDeltaTime * 1f;
	}

	private bool HasMoved()
	{
		float num;
		if (this.m_RigidBody3D != null)
		{
			num = (this.m_RigidBody3D.position - this.m_PrevPosition).magnitude;
		}
		else if (this.m_RigidBody2D != null)
		{
			num = (this.m_RigidBody2D.position - this.m_PrevPosition).magnitude;
		}
		else
		{
			num = (base.transform.position - this.m_PrevPosition).magnitude;
		}
		if (num > 1E-05f)
		{
			return true;
		}
		if (this.m_RigidBody3D != null)
		{
			num = Quaternion.Angle(this.m_RigidBody3D.rotation, this.m_PrevRotation);
		}
		else if (this.m_RigidBody2D != null)
		{
			num = Math.Abs(this.m_RigidBody2D.rotation - this.m_PrevRotation2D);
		}
		else
		{
			num = Quaternion.Angle(base.transform.rotation, this.m_PrevRotation);
		}
		if (num > 1E-05f)
		{
			return true;
		}
		if (this.m_RigidBody3D != null)
		{
			num = Mathf.Abs(this.m_RigidBody3D.velocity.sqrMagnitude - this.m_PrevVelocity);
		}
		else if (this.m_RigidBody2D != null)
		{
			num = Mathf.Abs(this.m_RigidBody2D.velocity.sqrMagnitude - this.m_PrevVelocity);
		}
		return num > 1E-05f;
	}

	private bool IsAttached()
	{
		return this.m_AttachSynchronizer != null && base.transform.parent != null;
	}

	private static void WriteAngle(P2PNetworkWriter writer, float angle, ReplicatedTransform.CompressionSyncMode compression)
	{
		switch (compression)
		{
		case ReplicatedTransform.CompressionSyncMode.None:
			writer.Write(angle);
			return;
		case ReplicatedTransform.CompressionSyncMode.Low:
			writer.Write((short)angle);
			return;
		case ReplicatedTransform.CompressionSyncMode.High:
			writer.Write((short)angle);
			return;
		default:
			return;
		}
	}

	private static float ReadAngle(P2PNetworkReader reader, ReplicatedTransform.CompressionSyncMode compression)
	{
		switch (compression)
		{
		case ReplicatedTransform.CompressionSyncMode.None:
			return reader.ReadFloat();
		case ReplicatedTransform.CompressionSyncMode.Low:
			return (float)reader.ReadInt16();
		case ReplicatedTransform.CompressionSyncMode.High:
			return (float)reader.ReadInt16();
		default:
			return 0f;
		}
	}

	public static void SerializeVelocity3D(P2PNetworkWriter writer, Vector3 velocity, ReplicatedTransform.CompressionSyncMode compression)
	{
		writer.Write(velocity);
	}

	public static void SerializeVelocity2D(P2PNetworkWriter writer, Vector2 velocity, ReplicatedTransform.CompressionSyncMode compression)
	{
		writer.Write(velocity);
	}

	public static void SerializeRotation3D(P2PNetworkWriter writer, Quaternion rot, ReplicatedTransform.AxisSyncMode mode, ReplicatedTransform.CompressionSyncMode compression)
	{
		switch (mode)
		{
		case ReplicatedTransform.AxisSyncMode.None:
			break;
		case ReplicatedTransform.AxisSyncMode.AxisX:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.x, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisY:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.y, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisZ:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.z, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisXY:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.x, compression);
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.y, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisXZ:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.x, compression);
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.z, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisYZ:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.y, compression);
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.z, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisXYZ:
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.x, compression);
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.y, compression);
			ReplicatedTransform.WriteAngle(writer, rot.eulerAngles.z, compression);
			break;
		default:
			return;
		}
	}

	public static void SerializeRotation2D(P2PNetworkWriter writer, float rot, ReplicatedTransform.CompressionSyncMode compression)
	{
		ReplicatedTransform.WriteAngle(writer, rot, compression);
	}

	public static void SerializeSpin3D(P2PNetworkWriter writer, Vector3 angularVelocity, ReplicatedTransform.AxisSyncMode mode, ReplicatedTransform.CompressionSyncMode compression)
	{
		switch (mode)
		{
		case ReplicatedTransform.AxisSyncMode.None:
			break;
		case ReplicatedTransform.AxisSyncMode.AxisX:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.x, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisY:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.y, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisZ:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.z, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisXY:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.x, compression);
			ReplicatedTransform.WriteAngle(writer, angularVelocity.y, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisXZ:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.x, compression);
			ReplicatedTransform.WriteAngle(writer, angularVelocity.z, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisYZ:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.y, compression);
			ReplicatedTransform.WriteAngle(writer, angularVelocity.z, compression);
			return;
		case ReplicatedTransform.AxisSyncMode.AxisXYZ:
			ReplicatedTransform.WriteAngle(writer, angularVelocity.x, compression);
			ReplicatedTransform.WriteAngle(writer, angularVelocity.y, compression);
			ReplicatedTransform.WriteAngle(writer, angularVelocity.z, compression);
			break;
		default:
			return;
		}
	}

	public static void SerializeSpin2D(P2PNetworkWriter writer, float angularVelocity, ReplicatedTransform.CompressionSyncMode compression)
	{
		ReplicatedTransform.WriteAngle(writer, angularVelocity, compression);
	}

	public static Vector3 UnserializeVelocity3D(P2PNetworkReader reader, ReplicatedTransform.CompressionSyncMode compression)
	{
		return reader.ReadVector3();
	}

	public static Vector3 UnserializeVelocity2D(P2PNetworkReader reader, ReplicatedTransform.CompressionSyncMode compression)
	{
		return reader.ReadVector2();
	}

	public static Quaternion UnserializeRotation3D(P2PNetworkReader reader, ReplicatedTransform.AxisSyncMode mode, ReplicatedTransform.CompressionSyncMode compression)
	{
		Quaternion identity = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		switch (mode)
		{
		case ReplicatedTransform.AxisSyncMode.AxisX:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), 0f, 0f);
			identity.eulerAngles = zero;
			break;
		case ReplicatedTransform.AxisSyncMode.AxisY:
			zero.Set(0f, ReplicatedTransform.ReadAngle(reader, compression), 0f);
			identity.eulerAngles = zero;
			break;
		case ReplicatedTransform.AxisSyncMode.AxisZ:
			zero.Set(0f, 0f, ReplicatedTransform.ReadAngle(reader, compression));
			identity.eulerAngles = zero;
			break;
		case ReplicatedTransform.AxisSyncMode.AxisXY:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression), 0f);
			identity.eulerAngles = zero;
			break;
		case ReplicatedTransform.AxisSyncMode.AxisXZ:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), 0f, ReplicatedTransform.ReadAngle(reader, compression));
			identity.eulerAngles = zero;
			break;
		case ReplicatedTransform.AxisSyncMode.AxisYZ:
			zero.Set(0f, ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression));
			identity.eulerAngles = zero;
			break;
		case ReplicatedTransform.AxisSyncMode.AxisXYZ:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression));
			identity.eulerAngles = zero;
			break;
		}
		return identity;
	}

	public static float UnserializeRotation2D(P2PNetworkReader reader, ReplicatedTransform.CompressionSyncMode compression)
	{
		return ReplicatedTransform.ReadAngle(reader, compression);
	}

	public static Vector3 UnserializeSpin3D(P2PNetworkReader reader, ReplicatedTransform.AxisSyncMode mode, ReplicatedTransform.CompressionSyncMode compression)
	{
		Vector3 zero = Vector3.zero;
		switch (mode)
		{
		case ReplicatedTransform.AxisSyncMode.AxisX:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), 0f, 0f);
			break;
		case ReplicatedTransform.AxisSyncMode.AxisY:
			zero.Set(0f, ReplicatedTransform.ReadAngle(reader, compression), 0f);
			break;
		case ReplicatedTransform.AxisSyncMode.AxisZ:
			zero.Set(0f, 0f, ReplicatedTransform.ReadAngle(reader, compression));
			break;
		case ReplicatedTransform.AxisSyncMode.AxisXY:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression), 0f);
			break;
		case ReplicatedTransform.AxisSyncMode.AxisXZ:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), 0f, ReplicatedTransform.ReadAngle(reader, compression));
			break;
		case ReplicatedTransform.AxisSyncMode.AxisYZ:
			zero.Set(0f, ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression));
			break;
		case ReplicatedTransform.AxisSyncMode.AxisXYZ:
			zero.Set(ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression), ReplicatedTransform.ReadAngle(reader, compression));
			break;
		}
		return zero;
	}

	public static float UnserializeSpin2D(P2PNetworkReader reader, ReplicatedTransform.CompressionSyncMode compression)
	{
		return ReplicatedTransform.ReadAngle(reader, compression);
	}

	public override void ReplOnChangedOwner(bool was_owner)
	{
		if (base.ReplIsOwner())
		{
			this.m_LastSyncTime = 0f;
			this.m_TargetSyncVelocity = Vector3.zero;
			this.m_FixedPosDiff = Vector3.zero;
			this.m_TargetSyncAngularVelocity3D = Vector3.zero;
			this.m_PrevPosition = base.transform.position;
			this.m_PrevRotation = base.transform.rotation;
			return;
		}
		if (was_owner)
		{
			this.m_TargetSyncTransformPosition = (this.m_TargetSyncPosition = base.transform.position);
		}
	}

	public void OnComponentAdded(Component component)
	{
		if (component.GetType() == typeof(Rigidbody) || component.GetType() == typeof(Rigidbody2D) || component.GetType() == typeof(CharacterController) || component.GetType() == typeof(CharacterControllerProxy) || component.GetType() == typeof(AttachmentSynchronizer))
		{
			this.CacheComponents();
		}
	}

	[SerializeField]
	private ReplicatedTransform.TransformSyncMode m_TransformSyncMode = ReplicatedTransform.TransformSyncMode.SyncRigidbody3D;

	[SerializeField]
	private ReplicatedTransform.AxisSyncMode m_SyncRotationAxis = ReplicatedTransform.AxisSyncMode.AxisXYZ;

	[SerializeField]
	private ReplicatedTransform.CompressionSyncMode m_RotationSyncCompression;

	[SerializeField]
	private bool m_SyncSpin;

	[SerializeField]
	private float m_MovementTheshold = 0.1f;

	[SerializeField]
	private float m_RotationTheshold = 5f;

	[SerializeField]
	private bool m_PreferSimulatedVerticalVelocity = true;

	[SerializeField]
	private float m_SnapThreshold = 5f;

	[SerializeField]
	private float m_InterpolateRotation = 1f;

	[SerializeField]
	private float m_InterpolateMovement = 1f;

	[SerializeField]
	private float m_InterpolateMaxDuration = 2f;

	[SerializeField]
	private ReplicatedTransform.ClientMoveCallback3D m_MoveCallback3D;

	[SerializeField]
	private ReplicatedTransform.ClientMoveCallback2D m_ClientMoveCallback2D;

	private Rigidbody m_RigidBody3D;

	private Rigidbody2D m_RigidBody2D;

	private CharacterController m_CharacterController;

	private bool m_Grounded = true;

	private AttachmentSynchronizer m_AttachSynchronizer;

	private CachedComponent<Renderer> m_Renderer;

	private Vector3 m_TargetSyncPosition;

	private Vector3 m_TargetSyncTransformPosition;

	private Vector3 m_TargetSyncVelocity;

	private Vector3 m_FixedPosDiff;

	private Quaternion m_TargetSyncRotation3D;

	private Vector3 m_TargetSyncAngularVelocity3D;

	private float m_TargetSyncRotation2D;

	private float m_TargetSyncAngularVelocity2D;

	private float m_LastSyncTime;

	private Vector3 m_PrevPosition;

	private Quaternion m_PrevRotation;

	private float m_PrevRotation2D;

	private float m_PrevVelocity;

	private const float k_LocalMovementThreshold = 1E-05f;

	private const float k_LocalRotationThreshold = 1E-05f;

	private const float k_LocalVelocityThreshold = 1E-05f;

	private const float k_MoveAheadRatio = 1f;

	private const float k_DesyncIntervalMul = 2f;

	private IReplicatedTransformParams[] m_ParamComponents;

	public enum TransformSyncMode
	{
		SyncNone,
		SyncTransform,
		SyncRigidbody2D,
		SyncRigidbody3D,
		SyncCharacterController,
		Auto
	}

	public enum AxisSyncMode
	{
		None,
		AxisX,
		AxisY,
		AxisZ,
		AxisXY,
		AxisXZ,
		AxisYZ,
		AxisXYZ
	}

	public enum CompressionSyncMode
	{
		None,
		Low,
		High
	}

	public delegate bool ClientMoveCallback3D(ref Vector3 position, ref Vector3 velocity, ref Quaternion rotation);

	public delegate bool ClientMoveCallback2D(ref Vector2 position, ref Vector2 velocity, ref float rotation);
}
