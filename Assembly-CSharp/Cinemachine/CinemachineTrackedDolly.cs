using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[DocumentationSorting(7f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("")]
	[RequireComponent(typeof(CinemachinePipeline))]
	[SaveDuringPlay]
	public class CinemachineTrackedDolly : CinemachineComponentBase
	{
		public override bool IsValid
		{
			get
			{
				return base.enabled && this.m_Path != null;
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
			if (deltaTime < 0f)
			{
				this.m_PreviousPathPosition = this.m_PathPosition;
				this.m_PreviousCameraPosition = curState.RawPosition;
			}
			if (!this.IsValid)
			{
				return;
			}
			if (this.m_AutoDolly.m_Enabled && base.FollowTarget != null)
			{
				float num = this.m_PreviousPathPosition;
				if (this.m_PositionUnits == CinemachinePathBase.PositionUnits.Distance)
				{
					num = this.m_Path.GetPathPositionFromDistance(num);
				}
				this.m_PathPosition = this.m_Path.FindClosestPoint(base.FollowTarget.transform.position, Mathf.FloorToInt(num), (deltaTime < 0f || this.m_AutoDolly.m_SearchRadius <= 0) ? -1 : this.m_AutoDolly.m_SearchRadius, this.m_AutoDolly.m_SearchResolution);
				if (this.m_PositionUnits == CinemachinePathBase.PositionUnits.Distance)
				{
					this.m_PathPosition = this.m_Path.GetPathDistanceFromPosition(this.m_PathPosition);
				}
				this.m_PathPosition += this.m_AutoDolly.m_PositionOffset;
			}
			float num2 = this.m_PathPosition;
			if (deltaTime >= 0f)
			{
				float num3 = this.m_Path.MaxUnit(this.m_PositionUnits);
				if (num3 > 0f)
				{
					float num4 = this.m_Path.NormalizeUnit(this.m_PreviousPathPosition, this.m_PositionUnits);
					float num5 = this.m_Path.NormalizeUnit(num2, this.m_PositionUnits);
					if (this.m_Path.Looped && Mathf.Abs(num5 - num4) > num3 / 2f)
					{
						if (num5 > num4)
						{
							num4 += num3;
						}
						else
						{
							num4 -= num3;
						}
					}
					this.m_PreviousPathPosition = num4;
					num2 = num5;
				}
				float num6 = this.m_PreviousPathPosition - num2;
				num6 = Damper.Damp(num6, this.m_ZDamping, deltaTime);
				num2 = this.m_PreviousPathPosition - num6;
			}
			this.m_PreviousPathPosition = num2;
			Quaternion quaternion = this.m_Path.EvaluateOrientationAtUnit(num2, this.m_PositionUnits);
			Vector3 vector = this.m_Path.EvaluatePositionAtUnit(num2, this.m_PositionUnits);
			Vector3 a = quaternion * Vector3.right;
			Vector3 vector2 = quaternion * Vector3.up;
			Vector3 a2 = quaternion * Vector3.forward;
			vector += this.m_PathOffset.x * a;
			vector += this.m_PathOffset.y * vector2;
			vector += this.m_PathOffset.z * a2;
			if (deltaTime >= 0f)
			{
				Vector3 previousCameraPosition = this.m_PreviousCameraPosition;
				Vector3 vector3 = previousCameraPosition - vector;
				Vector3 vector4 = Vector3.Dot(vector3, vector2) * vector2;
				Vector3 vector5 = vector3 - vector4;
				vector5 = Damper.Damp(vector5, this.m_XDamping, deltaTime);
				vector4 = Damper.Damp(vector4, this.m_YDamping, deltaTime);
				vector = previousCameraPosition - (vector5 + vector4);
			}
			curState.RawPosition = (this.m_PreviousCameraPosition = vector);
			Quaternion quaternion2 = this.GetTargetOrientationAtPathPoint(quaternion, curState.ReferenceUp);
			if (deltaTime < 0f)
			{
				this.m_PreviousOrientation = quaternion2;
			}
			else
			{
				if (deltaTime >= 0f)
				{
					Vector3 vector6 = (Quaternion.Inverse(this.m_PreviousOrientation) * quaternion2).eulerAngles;
					for (int i = 0; i < 3; i++)
					{
						if (vector6[i] > 180f)
						{
							int index = i;
							vector6[index] -= 360f;
						}
					}
					vector6 = Damper.Damp(vector6, this.AngularDamping, deltaTime);
					quaternion2 = this.m_PreviousOrientation * Quaternion.Euler(vector6);
				}
				this.m_PreviousOrientation = quaternion2;
			}
			curState.RawOrientation = quaternion2;
			curState.ReferenceUp = curState.RawOrientation * Vector3.up;
		}

		public override void OnPositionDragged(Vector3 delta)
		{
			Vector3 b = Quaternion.Inverse(this.m_Path.EvaluateOrientationAtUnit(this.m_PathPosition, this.m_PositionUnits)) * delta;
			this.m_PathOffset += b;
		}

		private Quaternion GetTargetOrientationAtPathPoint(Quaternion pathOrientation, Vector3 up)
		{
			switch (this.m_CameraUp)
			{
			case CinemachineTrackedDolly.CameraUpMode.Path:
				return pathOrientation;
			case CinemachineTrackedDolly.CameraUpMode.PathNoRoll:
				return Quaternion.LookRotation(pathOrientation * Vector3.forward, up);
			case CinemachineTrackedDolly.CameraUpMode.FollowTarget:
				if (base.FollowTarget != null)
				{
					return base.FollowTarget.rotation;
				}
				break;
			case CinemachineTrackedDolly.CameraUpMode.FollowTargetNoRoll:
				if (base.FollowTarget != null)
				{
					return Quaternion.LookRotation(base.FollowTarget.rotation * Vector3.forward, up);
				}
				break;
			}
			return Quaternion.LookRotation(base.transform.rotation * Vector3.forward, up);
		}

		private Vector3 AngularDamping
		{
			get
			{
				switch (this.m_CameraUp)
				{
				case CinemachineTrackedDolly.CameraUpMode.Default:
					return Vector3.zero;
				case CinemachineTrackedDolly.CameraUpMode.PathNoRoll:
				case CinemachineTrackedDolly.CameraUpMode.FollowTargetNoRoll:
					return new Vector3(this.m_PitchDamping, this.m_YawDamping, 0f);
				}
				return new Vector3(this.m_PitchDamping, this.m_YawDamping, this.m_RollDamping);
			}
		}

		[Tooltip("The path to which the camera will be constrained.  This must be non-null.")]
		public CinemachinePathBase m_Path;

		[Tooltip("The position along the path at which the camera will be placed.  This can be animated directly, or set automatically by the Auto-Dolly feature to get as close as possible to the Follow target.  The value is interpreted according to the Position Units setting.")]
		public float m_PathPosition;

		[Tooltip("How to interpret Path Position.  If set to Path Units, values are as follows: 0 represents the first waypoint on the path, 1 is the second, and so on.  Values in-between are points on the path in between the waypoints.  If set to Distance, then Path Position represents distance along the path.")]
		public CinemachinePathBase.PositionUnits m_PositionUnits;

		[Tooltip("Where to put the camera relative to the path position.  X is perpendicular to the path, Y is up, and Z is parallel to the path.  This allows the camera to be offset from the path itself (as if on a tripod, for example).")]
		public Vector3 m_PathOffset = Vector3.zero;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain its position in a direction perpendicular to the path.  Small numbers are more responsive, rapidly translating the camera to keep the target's x-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public float m_XDamping;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain its position in the path-local up direction.  Small numbers are more responsive, rapidly translating the camera to keep the target's y-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public float m_YDamping;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain its position in a direction parallel to the path.  Small numbers are more responsive, rapidly translating the camera to keep the target's z-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public float m_ZDamping = 1f;

		[Tooltip("How to set the virtual camera's Up vector.  This will affect the screen composition, because the camera Aim behaviours will always try to respect the Up direction.")]
		public CinemachineTrackedDolly.CameraUpMode m_CameraUp;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to track the target rotation's X angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
		public float m_PitchDamping;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to track the target rotation's Y angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
		public float m_YawDamping;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to track the target rotation's Z angle.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
		public float m_RollDamping;

		[Tooltip("Controls how automatic dollying occurs.  A Follow target is necessary to use this feature.")]
		public CinemachineTrackedDolly.AutoDolly m_AutoDolly = new CinemachineTrackedDolly.AutoDolly(false, 0f, 2, 5);

		private float m_PreviousPathPosition;

		private Quaternion m_PreviousOrientation = Quaternion.identity;

		private Vector3 m_PreviousCameraPosition = Vector3.zero;

		[DocumentationSorting(7.1f, DocumentationSortingAttribute.Level.UserRef)]
		public enum CameraUpMode
		{
			Default,
			Path,
			PathNoRoll,
			FollowTarget,
			FollowTargetNoRoll
		}

		[DocumentationSorting(7.2f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct AutoDolly
		{
			public AutoDolly(bool enabled, float positionOffset, int searchRadius, int stepsPerSegment)
			{
				this.m_Enabled = enabled;
				this.m_PositionOffset = positionOffset;
				this.m_SearchRadius = searchRadius;
				this.m_SearchResolution = stepsPerSegment;
			}

			[Tooltip("If checked, will enable automatic dolly, which chooses a path position that is as close as possible to the Follow target.  Note: this can have significant performance impact")]
			public bool m_Enabled;

			[Tooltip("Offset, in current position units, from the closest point on the path to the follow target")]
			public float m_PositionOffset;

			[Tooltip("Search up to how many waypoints on either side of the current position.  Use 0 for Entire path.")]
			public int m_SearchRadius;

			[FormerlySerializedAs("m_StepsPerSegment")]
			[Tooltip("We search between waypoints by dividing the segment into this many straight pieces.  The higher the number, the more accurate the result, but performance is proportionally slower for higher numbers")]
			public int m_SearchResolution;
		}
	}
}
