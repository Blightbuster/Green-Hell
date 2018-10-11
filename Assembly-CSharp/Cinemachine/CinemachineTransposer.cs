using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[AddComponentMenu("")]
	[SaveDuringPlay]
	[RequireComponent(typeof(CinemachinePipeline))]
	[DocumentationSorting(5f, DocumentationSortingAttribute.Level.UserRef)]
	public class CinemachineTransposer : CinemachineComponentBase
	{
		protected virtual void OnValidate()
		{
			this.m_FollowOffset = this.EffectiveOffset;
		}

		protected Vector3 EffectiveOffset
		{
			get
			{
				Vector3 followOffset = this.m_FollowOffset;
				if (this.m_BindingMode == CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
				{
					followOffset.x = 0f;
					followOffset.z = -Mathf.Abs(followOffset.z);
				}
				return followOffset;
			}
		}

		public override bool IsValid
		{
			get
			{
				return base.enabled && base.FollowTarget != null;
			}
		}

		public override CinemachineCore.Stage Stage
		{
			get
			{
				return CinemachineCore.Stage.Body;
			}
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			this.InitPrevFrameStateInfo(ref curState, deltaTime);
			if (this.IsValid)
			{
				Vector3 effectiveOffset = this.EffectiveOffset;
				Vector3 a;
				Quaternion rotation;
				this.TrackTarget(deltaTime, curState.ReferenceUp, effectiveOffset, out a, out rotation);
				curState.RawPosition = a + rotation * effectiveOffset;
				curState.ReferenceUp = rotation * Vector3.up;
			}
		}

		public override void OnPositionDragged(Vector3 delta)
		{
			Quaternion referenceOrientation = this.GetReferenceOrientation(base.VcamState.ReferenceUp);
			Vector3 b = Quaternion.Inverse(referenceOrientation) * delta;
			this.m_FollowOffset += b;
			this.m_FollowOffset = this.EffectiveOffset;
		}

		protected void InitPrevFrameStateInfo(ref CameraState curState, float deltaTime)
		{
			if (this.m_previousTarget != base.FollowTarget || deltaTime < 0f)
			{
				this.m_previousTarget = base.FollowTarget;
				this.m_targetOrientationOnAssign = ((!(this.m_previousTarget == null)) ? base.FollowTarget.rotation : Quaternion.identity);
			}
			if (deltaTime < 0f)
			{
				this.m_PreviousTargetPosition = curState.RawPosition;
				this.m_PreviousReferenceOrientation = this.GetReferenceOrientation(curState.ReferenceUp);
			}
		}

		protected void TrackTarget(float deltaTime, Vector3 up, Vector3 desiredCameraOffset, out Vector3 outTargetPosition, out Quaternion outTargetOrient)
		{
			Quaternion referenceOrientation = this.GetReferenceOrientation(up);
			Quaternion quaternion = referenceOrientation;
			if (deltaTime >= 0f)
			{
				Vector3 vector = (Quaternion.Inverse(this.m_PreviousReferenceOrientation) * referenceOrientation).eulerAngles;
				for (int i = 0; i < 3; i++)
				{
					if (vector[i] > 180f)
					{
						int index;
						vector[index = i] = vector[index] - 360f;
					}
				}
				vector = Damper.Damp(vector, this.AngularDamping, deltaTime);
				quaternion = this.m_PreviousReferenceOrientation * Quaternion.Euler(vector);
			}
			this.m_PreviousReferenceOrientation = quaternion;
			Vector3 position = base.FollowTarget.position;
			Vector3 previousTargetPosition = this.m_PreviousTargetPosition;
			Vector3 vector2 = position - previousTargetPosition;
			if (deltaTime >= 0f)
			{
				Quaternion rotation;
				if (desiredCameraOffset.AlmostZero())
				{
					rotation = base.VcamState.RawOrientation;
				}
				else
				{
					rotation = Quaternion.LookRotation(quaternion * desiredCameraOffset.normalized, up);
				}
				Vector3 vector3 = Quaternion.Inverse(rotation) * vector2;
				vector3 = Damper.Damp(vector3, this.Damping, deltaTime);
				vector2 = rotation * vector3;
			}
			outTargetPosition = (this.m_PreviousTargetPosition = previousTargetPosition + vector2);
			outTargetOrient = quaternion;
		}

		protected Vector3 Damping
		{
			get
			{
				CinemachineTransposer.BindingMode bindingMode = this.m_BindingMode;
				if (bindingMode != CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
				{
					return new Vector3(this.m_XDamping, this.m_YDamping, this.m_ZDamping);
				}
				return new Vector3(0f, this.m_YDamping, this.m_ZDamping);
			}
		}

		protected Vector3 AngularDamping
		{
			get
			{
				switch (this.m_BindingMode)
				{
				case CinemachineTransposer.BindingMode.LockToTargetOnAssign:
				case CinemachineTransposer.BindingMode.WorldSpace:
				case CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp:
					return Vector3.zero;
				case CinemachineTransposer.BindingMode.LockToTargetWithWorldUp:
					return new Vector3(0f, this.m_YawDamping, 0f);
				case CinemachineTransposer.BindingMode.LockToTargetNoRoll:
					return new Vector3(this.m_PitchDamping, this.m_YawDamping, 0f);
				}
				return new Vector3(this.m_PitchDamping, this.m_YawDamping, this.m_RollDamping);
			}
		}

		public Vector3 GeTargetCameraPosition(Vector3 worldUp)
		{
			if (!this.IsValid)
			{
				return Vector3.zero;
			}
			return base.FollowTarget.position + this.GetReferenceOrientation(worldUp) * this.EffectiveOffset;
		}

		public Quaternion GetReferenceOrientation(Vector3 worldUp)
		{
			if (base.FollowTarget != null)
			{
				Quaternion rotation = base.FollowTarget.rotation;
				switch (this.m_BindingMode)
				{
				case CinemachineTransposer.BindingMode.LockToTargetOnAssign:
					return this.m_targetOrientationOnAssign;
				case CinemachineTransposer.BindingMode.LockToTargetWithWorldUp:
					return CinemachineTransposer.Uppify(rotation, worldUp);
				case CinemachineTransposer.BindingMode.LockToTargetNoRoll:
					return Quaternion.LookRotation(rotation * Vector3.forward, worldUp);
				case CinemachineTransposer.BindingMode.LockToTarget:
					return rotation;
				case CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp:
				{
					Vector3 vector = base.FollowTarget.position - base.VcamState.RawPosition;
					if (!vector.AlmostZero())
					{
						return CinemachineTransposer.Uppify(Quaternion.LookRotation(vector, worldUp), worldUp);
					}
					break;
				}
				}
			}
			return Quaternion.identity;
		}

		private static Quaternion Uppify(Quaternion q, Vector3 up)
		{
			Quaternion lhs = Quaternion.FromToRotation(q * Vector3.up, up);
			return lhs * q;
		}

		[Tooltip("The coordinate space to use when interpreting the offset from the target.  This is also used to set the camera's Up vector, which will be maintained when aiming the camera.")]
		public CinemachineTransposer.BindingMode m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;

		[Tooltip("The distance vector that the transposer will attempt to maintain from the Follow target")]
		public Vector3 m_FollowOffset = Vector3.back * 10f;

		[Tooltip("How aggressively the camera tries to maintain the offset in the X-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's x-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		[Range(0f, 20f)]
		public float m_XDamping = 1f;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain the offset in the Y-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's y-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public float m_YDamping = 1f;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain the offset in the Z-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's z-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public float m_ZDamping = 1f;

		[Tooltip("How aggressively the camera tries to track the target rotation's X angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
		[Range(0f, 20f)]
		public float m_PitchDamping;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to track the target rotation's Y angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
		public float m_YawDamping;

		[Tooltip("How aggressively the camera tries to track the target rotation's Z angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
		[Range(0f, 20f)]
		public float m_RollDamping;

		private Vector3 m_PreviousTargetPosition = Vector3.zero;

		private Quaternion m_PreviousReferenceOrientation = Quaternion.identity;

		private Quaternion m_targetOrientationOnAssign = Quaternion.identity;

		private Transform m_previousTarget;

		[DocumentationSorting(5.01f, DocumentationSortingAttribute.Level.UserRef)]
		public enum BindingMode
		{
			LockToTargetOnAssign,
			LockToTargetWithWorldUp,
			LockToTargetNoRoll,
			LockToTarget,
			WorldSpace,
			SimpleFollowWithWorldUp
		}
	}
}
