using System;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[DocumentationSorting(15f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[SaveDuringPlay]
	public class CinemachineCollider : CinemachineExtension
	{
		public bool IsTargetObscured(ICinemachineCamera vcam)
		{
			return base.GetExtraState<CinemachineCollider.VcamExtraState>(vcam).targetObscured;
		}

		public bool CameraWasDisplaced(CinemachineVirtualCameraBase vcam)
		{
			return base.GetExtraState<CinemachineCollider.VcamExtraState>(vcam).colliderDisplacement > 0f;
		}

		private void OnValidate()
		{
			this.m_DistanceLimit = Mathf.Max(0f, this.m_DistanceLimit);
			this.m_CameraRadius = Mathf.Max(0f, this.m_CameraRadius);
			this.m_MinimumDistanceFromTarget = Mathf.Max(0.01f, this.m_MinimumDistanceFromTarget);
			this.m_OptimalTargetDistance = Mathf.Max(0f, this.m_OptimalTargetDistance);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.CleanupCameraCollider();
		}

		public List<List<Vector3>> DebugPaths
		{
			get
			{
				List<List<Vector3>> list = new List<List<Vector3>>();
				List<CinemachineCollider.VcamExtraState> allExtraStates = base.GetAllExtraStates<CinemachineCollider.VcamExtraState>();
				foreach (CinemachineCollider.VcamExtraState vcamExtraState in allExtraStates)
				{
					if (vcamExtraState.debugResolutionPath != null)
					{
						list.Add(vcamExtraState.debugResolutionPath);
					}
				}
				return list;
			}
		}

		protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			CinemachineCollider.VcamExtraState vcamExtraState = null;
			if (stage == CinemachineCore.Stage.Body)
			{
				vcamExtraState = base.GetExtraState<CinemachineCollider.VcamExtraState>(vcam);
				vcamExtraState.targetObscured = false;
				vcamExtraState.colliderDisplacement = 0f;
				vcamExtraState.debugResolutionPath = null;
			}
			if (stage == CinemachineCore.Stage.Body && this.m_AvoidObstacles)
			{
				Vector3 vector = this.PreserveLignOfSight(ref state, ref vcamExtraState);
				if (this.m_Damping > 0f && deltaTime >= 0f)
				{
					Vector3 vector2 = vector - vcamExtraState.m_previousDisplacement;
					vector2 = Damper.Damp(vector2, this.m_Damping, deltaTime);
					vector = vcamExtraState.m_previousDisplacement + vector2;
				}
				vcamExtraState.m_previousDisplacement = vector;
				state.PositionCorrection += vector;
				vcamExtraState.colliderDisplacement += vector.magnitude;
			}
			if (stage == CinemachineCore.Stage.Aim)
			{
				vcamExtraState = base.GetExtraState<CinemachineCollider.VcamExtraState>(vcam);
				vcamExtraState.targetObscured = this.CheckForTargetObstructions(state);
				if (vcamExtraState.targetObscured)
				{
					state.ShotQuality *= 0.2f;
				}
				if (vcamExtraState.colliderDisplacement > 0f)
				{
					state.ShotQuality *= 0.8f;
				}
				float num = 0f;
				if (this.m_OptimalTargetDistance > 0f && state.HasLookAt)
				{
					float num2 = Vector3.Magnitude(state.ReferenceLookAt - state.FinalPosition);
					if (num2 <= this.m_OptimalTargetDistance)
					{
						float num3 = this.m_OptimalTargetDistance / 2f;
						if (num2 >= num3)
						{
							num = 0.2f * (num2 - num3) / (this.m_OptimalTargetDistance - num3);
						}
					}
					else
					{
						num2 -= this.m_OptimalTargetDistance;
						float num4 = this.m_OptimalTargetDistance * 3f;
						if (num2 < num4)
						{
							num = 0.2f * (1f - num2 / num4);
						}
					}
					state.ShotQuality *= 1f + num;
				}
			}
		}

		private Vector3 PreserveLignOfSight(ref CameraState state, ref CinemachineCollider.VcamExtraState extra)
		{
			Vector3 result = Vector3.zero;
			if (state.HasLookAt)
			{
				Vector3 correctedPosition = state.CorrectedPosition;
				Vector3 referenceLookAt = state.ReferenceLookAt;
				Vector3 vector = correctedPosition;
				Vector3 vector2 = vector - referenceLookAt;
				float magnitude = vector2.magnitude;
				float num = Mathf.Max(this.m_MinimumDistanceFromTarget, 0.0001f);
				if (magnitude > num)
				{
					vector2.Normalize();
					float num2 = magnitude - num;
					if (this.m_DistanceLimit > 0.0001f)
					{
						num2 = Mathf.Min(this.m_DistanceLimit, num2);
					}
					Ray ray = new Ray(vector - num2 * vector2, vector2);
					num2 += 0.001f;
					RaycastHit obstacle;
					if (num2 > 0.0001f && this.RaycastIgnoreTag(ray, out obstacle, num2))
					{
						float distance = Mathf.Max(0f, obstacle.distance - 0.001f);
						vector = ray.GetPoint(distance);
						extra.AddPointToDebugPath(vector);
						if (this.m_Strategy != CinemachineCollider.ResolutionStrategy.PullCameraForward)
						{
							vector = this.PushCameraBack(vector, vector2, obstacle, referenceLookAt, new Plane(state.ReferenceUp, correctedPosition), magnitude, this.m_MaximumEffort, ref extra);
						}
					}
				}
				if (this.m_CameraRadius > 0.0001f)
				{
					vector += this.RespectCameraRadius(vector, state.ReferenceLookAt);
				}
				else if (this.mCameraColliderGameObject != null)
				{
					this.CleanupCameraCollider();
				}
				result = vector - correctedPosition;
			}
			return result;
		}

		private bool RaycastIgnoreTag(Ray ray, out RaycastHit hitInfo, float rayLength)
		{
			while (Physics.Raycast(ray, out hitInfo, rayLength, this.m_CollideAgainst.value, QueryTriggerInteraction.Ignore))
			{
				if (this.m_IgnoreTag.Length == 0 || !hitInfo.collider.CompareTag(this.m_IgnoreTag))
				{
					return true;
				}
				Ray ray2 = new Ray(ray.GetPoint(rayLength), -ray.direction);
				if (!hitInfo.collider.Raycast(ray2, out hitInfo, rayLength))
				{
					break;
				}
				rayLength = hitInfo.distance - 0.001f;
				if (rayLength < 0.0001f)
				{
					break;
				}
				ray.origin = ray2.GetPoint(rayLength);
			}
			return false;
		}

		private Vector3 PushCameraBack(Vector3 currentPos, Vector3 pushDir, RaycastHit obstacle, Vector3 lookAtPos, Plane startPlane, float targetDistance, int iterations, ref CinemachineCollider.VcamExtraState extra)
		{
			Vector3 vector = Vector3.zero;
			if (!this.GetWalkingDirection(currentPos, pushDir, obstacle, ref vector))
			{
				return currentPos;
			}
			Ray ray = new Ray(currentPos, vector);
			float num = this.GetPushBackDistance(ray, startPlane, targetDistance, lookAtPos);
			if (num <= 0.0001f)
			{
				return currentPos;
			}
			float num2 = this.ClampRayToBounds(ray, num, obstacle.collider.bounds);
			num = Mathf.Min(num, num2 + 0.001f);
			RaycastHit obstacle2;
			Vector3 vector2;
			if (this.RaycastIgnoreTag(ray, out obstacle2, num))
			{
				float distance = obstacle2.distance - 0.001f;
				vector2 = ray.GetPoint(distance);
				extra.AddPointToDebugPath(vector2);
				if (iterations > 1)
				{
					vector2 = this.PushCameraBack(vector2, vector, obstacle2, lookAtPos, startPlane, targetDistance, iterations - 1, ref extra);
				}
				return vector2;
			}
			vector2 = ray.GetPoint(num);
			vector = vector2 - lookAtPos;
			float magnitude = vector.magnitude;
			RaycastHit raycastHit;
			if (magnitude < 0.0001f || this.RaycastIgnoreTag(new Ray(lookAtPos, vector), out raycastHit, magnitude - 0.001f))
			{
				return currentPos;
			}
			ray = new Ray(vector2, vector);
			extra.AddPointToDebugPath(vector2);
			num = this.GetPushBackDistance(ray, startPlane, targetDistance, lookAtPos);
			if (num > 0.0001f)
			{
				if (!this.RaycastIgnoreTag(ray, out obstacle2, num))
				{
					vector2 = ray.GetPoint(num);
					extra.AddPointToDebugPath(vector2);
				}
				else
				{
					float distance2 = obstacle2.distance - 0.001f;
					vector2 = ray.GetPoint(distance2);
					extra.AddPointToDebugPath(vector2);
					if (iterations > 1)
					{
						vector2 = this.PushCameraBack(vector2, vector, obstacle2, lookAtPos, startPlane, targetDistance, iterations - 1, ref extra);
					}
				}
			}
			return vector2;
		}

		private bool GetWalkingDirection(Vector3 pos, Vector3 pushDir, RaycastHit obstacle, ref Vector3 outDir)
		{
			Vector3 normal = obstacle.normal;
			float num = 0.00500000035f;
			int num2 = Physics.SphereCastNonAlloc(pos, num, pushDir.normalized, this.m_CornerBuffer, 0f, this.m_CollideAgainst.value, QueryTriggerInteraction.Ignore);
			if (num2 > 1)
			{
				for (int i = 0; i < num2; i++)
				{
					if (this.m_IgnoreTag.Length <= 0 || !this.m_CornerBuffer[i].collider.CompareTag(this.m_IgnoreTag))
					{
						Type type = this.m_CornerBuffer[i].collider.GetType();
						if (type == typeof(BoxCollider) || type == typeof(SphereCollider) || type == typeof(CapsuleCollider))
						{
							Vector3 a = this.m_CornerBuffer[i].collider.ClosestPoint(pos);
							Vector3 direction = a - pos;
							if (direction.magnitude > 1E-05f && this.m_CornerBuffer[i].collider.Raycast(new Ray(pos, direction), out this.m_CornerBuffer[i], num))
							{
								if (!(this.m_CornerBuffer[i].normal - obstacle.normal).AlmostZero())
								{
									normal = this.m_CornerBuffer[i].normal;
								}
								break;
							}
						}
					}
				}
			}
			Vector3 vector = Vector3.Cross(obstacle.normal, normal);
			if (vector.AlmostZero())
			{
				vector = Vector3.ProjectOnPlane(pushDir, obstacle.normal);
			}
			else
			{
				float num3 = Vector3.Dot(vector, pushDir);
				if (Mathf.Abs(num3) < 0.0001f)
				{
					return false;
				}
				if (num3 < 0f)
				{
					vector = -vector;
				}
			}
			if (vector.AlmostZero())
			{
				return false;
			}
			outDir = vector.normalized;
			return true;
		}

		private float GetPushBackDistance(Ray ray, Plane startPlane, float targetDistance, Vector3 lookAtPos)
		{
			float num = targetDistance - (ray.origin - lookAtPos).magnitude;
			if (num < 0.0001f)
			{
				return 0f;
			}
			if (this.m_Strategy == CinemachineCollider.ResolutionStrategy.PreserveCameraDistance)
			{
				return num;
			}
			float num2;
			if (!startPlane.Raycast(ray, out num2))
			{
				num2 = 0f;
			}
			num2 = Mathf.Min(num, num2);
			if (num2 < 0.0001f)
			{
				return 0f;
			}
			float num3 = Mathf.Abs(Vector3.Angle(startPlane.normal, ray.direction) - 90f);
			if (num3 < 0.1f)
			{
				num2 = Mathf.Lerp(0f, num2, num3 / 0.1f);
			}
			return num2;
		}

		private float ClampRayToBounds(Ray ray, float distance, Bounds bounds)
		{
			if (Vector3.Dot(ray.direction, Vector3.up) > 0f)
			{
				Plane plane = new Plane(Vector3.down, bounds.max);
				float num;
				if (plane.Raycast(ray, out num) && num > 0.0001f)
				{
					distance = Mathf.Min(distance, num);
				}
			}
			else if (Vector3.Dot(ray.direction, Vector3.down) > 0f)
			{
				Plane plane2 = new Plane(Vector3.up, bounds.min);
				float num;
				if (plane2.Raycast(ray, out num) && num > 0.0001f)
				{
					distance = Mathf.Min(distance, num);
				}
			}
			if (Vector3.Dot(ray.direction, Vector3.right) > 0f)
			{
				Plane plane3 = new Plane(Vector3.left, bounds.max);
				float num;
				if (plane3.Raycast(ray, out num) && num > 0.0001f)
				{
					distance = Mathf.Min(distance, num);
				}
			}
			else if (Vector3.Dot(ray.direction, Vector3.left) > 0f)
			{
				Plane plane4 = new Plane(Vector3.right, bounds.min);
				float num;
				if (plane4.Raycast(ray, out num) && num > 0.0001f)
				{
					distance = Mathf.Min(distance, num);
				}
			}
			if (Vector3.Dot(ray.direction, Vector3.forward) > 0f)
			{
				Plane plane5 = new Plane(Vector3.back, bounds.max);
				float num;
				if (plane5.Raycast(ray, out num) && num > 0.0001f)
				{
					distance = Mathf.Min(distance, num);
				}
			}
			else if (Vector3.Dot(ray.direction, Vector3.back) > 0f)
			{
				Plane plane6 = new Plane(Vector3.forward, bounds.min);
				float num;
				if (plane6.Raycast(ray, out num) && num > 0.0001f)
				{
					distance = Mathf.Min(distance, num);
				}
			}
			return distance;
		}

		private Vector3 RespectCameraRadius(Vector3 cameraPos, Vector3 lookAtPos)
		{
			Vector3 vector = Vector3.zero;
			int num = Physics.OverlapSphereNonAlloc(cameraPos, this.m_CameraRadius, this.mColliderBuffer, this.m_CollideAgainst, QueryTriggerInteraction.Ignore);
			if (num > 0)
			{
				if (this.mCameraColliderGameObject == null)
				{
					this.mCameraColliderGameObject = new GameObject("Cinemachine Collider Collider");
					this.mCameraColliderGameObject.hideFlags = HideFlags.HideAndDontSave;
					this.mCameraColliderGameObject.transform.position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
					this.mCameraColliderGameObject.SetActive(true);
					this.mCameraCollider = this.mCameraColliderGameObject.AddComponent<SphereCollider>();
				}
				this.mCameraCollider.radius = this.m_CameraRadius;
				for (int i = 0; i < num; i++)
				{
					Collider collider = this.mColliderBuffer[i];
					if (this.m_IgnoreTag.Length <= 0 || !collider.CompareTag(this.m_IgnoreTag))
					{
						Vector3 a;
						float d;
						if (Physics.ComputePenetration(this.mCameraCollider, cameraPos, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out a, out d))
						{
							vector += a * d;
						}
					}
				}
			}
			return vector;
		}

		private void CleanupCameraCollider()
		{
			if (this.mCameraColliderGameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(this.mCameraColliderGameObject);
			}
			this.mCameraColliderGameObject = null;
			this.mCameraCollider = null;
		}

		private bool CheckForTargetObstructions(CameraState state)
		{
			if (state.HasLookAt)
			{
				Vector3 referenceLookAt = state.ReferenceLookAt;
				Vector3 correctedPosition = state.CorrectedPosition;
				Vector3 vector = referenceLookAt - correctedPosition;
				float magnitude = vector.magnitude;
				if (magnitude < Mathf.Max(this.m_MinimumDistanceFromTarget, 0.0001f))
				{
					return true;
				}
				Ray ray = new Ray(correctedPosition, vector.normalized);
				RaycastHit raycastHit;
				if (this.RaycastIgnoreTag(ray, out raycastHit, magnitude - this.m_MinimumDistanceFromTarget))
				{
					return true;
				}
			}
			return false;
		}

		[Tooltip("The Unity layer mask against which the collider will raycast")]
		[Header("Obstacle Detection")]
		public LayerMask m_CollideAgainst = 1;

		[TagField]
		[Tooltip("Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
		public string m_IgnoreTag = string.Empty;

		[Tooltip("Obstacles closer to the target than this will be ignored")]
		public float m_MinimumDistanceFromTarget = 0.1f;

		[Tooltip("When enabled, will attempt to resolve situations where the line of sight to the target is blocked by an obstacle")]
		[Space]
		[FormerlySerializedAs("m_PreserveLineOfSight")]
		public bool m_AvoidObstacles = true;

		[FormerlySerializedAs("m_LineOfSightFeelerDistance")]
		[Tooltip("The maximum raycast distance when checking if the line of sight to this camera's target is clear.  If the setting is 0 or less, the current actual distance to target will be used.")]
		public float m_DistanceLimit;

		[Tooltip("Camera will try to maintain this distance from any obstacle.  Try to keep this value small.  Increase it if you are seeing inside obstacles due to a large FOV on the camera.")]
		public float m_CameraRadius = 0.1f;

		[Tooltip("The way in which the Collider will attempt to preserve sight of the target.")]
		public CinemachineCollider.ResolutionStrategy m_Strategy = CinemachineCollider.ResolutionStrategy.PreserveCameraHeight;

		[Range(1f, 10f)]
		[Tooltip("Upper limit on how many obstacle hits to process.  Higher numbers may impact performance.  In most environments, 4 is enough.")]
		public int m_MaximumEffort = 4;

		[Tooltip("The gradualness of collision resolution.  Higher numbers will move the camera more gradually away from obstructions.")]
		[Range(0f, 10f)]
		[FormerlySerializedAs("m_Smoothing")]
		public float m_Damping;

		[Tooltip("If greater than zero, a higher score will be given to shots when the target is closer to this distance.  Set this to zero to disable this feature.")]
		[Header("Shot Evaluation")]
		public float m_OptimalTargetDistance;

		private const float PrecisionSlush = 0.001f;

		private RaycastHit[] m_CornerBuffer = new RaycastHit[4];

		private const float AngleThreshold = 0.1f;

		private Collider[] mColliderBuffer = new Collider[5];

		private SphereCollider mCameraCollider;

		private GameObject mCameraColliderGameObject;

		public enum ResolutionStrategy
		{
			PullCameraForward,
			PreserveCameraHeight,
			PreserveCameraDistance
		}

		private class VcamExtraState
		{
			public void AddPointToDebugPath(Vector3 p)
			{
			}

			public Vector3 m_previousDisplacement;

			public float colliderDisplacement;

			public bool targetObscured;

			public List<Vector3> debugResolutionPath;
		}
	}
}
