using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(4f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[RequireComponent(typeof(CinemachinePipeline))]
	[SaveDuringPlay]
	public class CinemachineGroupComposer : CinemachineComposer
	{
		private void OnValidate()
		{
			this.m_GroupFramingSize = Mathf.Max(0.0001f, this.m_GroupFramingSize);
			this.m_MaxDollyIn = Mathf.Max(0f, this.m_MaxDollyIn);
			this.m_MaxDollyOut = Mathf.Max(0f, this.m_MaxDollyOut);
			this.m_MinimumDistance = Mathf.Max(0f, this.m_MinimumDistance);
			this.m_MaximumDistance = Mathf.Max(this.m_MinimumDistance, this.m_MaximumDistance);
			this.m_MinimumFOV = Mathf.Max(1f, this.m_MinimumFOV);
			this.m_MaximumFOV = Mathf.Clamp(this.m_MaximumFOV, this.m_MinimumFOV, 179f);
			this.m_MinimumOrthoSize = Mathf.Max(0.01f, this.m_MinimumOrthoSize);
			this.m_MaximumOrthoSize = Mathf.Max(this.m_MinimumOrthoSize, this.m_MaximumOrthoSize);
		}

		public CinemachineTargetGroup TargetGroup
		{
			get
			{
				Transform lookAtTarget = base.LookAtTarget;
				if (lookAtTarget != null)
				{
					return lookAtTarget.GetComponent<CinemachineTargetGroup>();
				}
				return null;
			}
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			CinemachineTargetGroup targetGroup = this.TargetGroup;
			if (targetGroup == null)
			{
				base.MutateCameraState(ref curState, deltaTime);
				return;
			}
			if (!this.IsValid || !curState.HasLookAt)
			{
				this.m_prevTargetHeight = 0f;
				return;
			}
			curState.ReferenceLookAt = this.GetLookAtPointAndSetTrackedPoint(targetGroup.transform.position);
			Vector3 v = base.TrackedPoint - curState.RawPosition;
			float magnitude = v.magnitude;
			if (magnitude < 0.0001f)
			{
				return;
			}
			Vector3 vector = v.AlmostZero() ? Vector3.forward : v.normalized;
			Bounds boundingBox = targetGroup.BoundingBox;
			this.m_lastBoundsMatrix = Matrix4x4.TRS(boundingBox.center - vector * boundingBox.extents.magnitude, Quaternion.LookRotation(vector, curState.ReferenceUp), Vector3.one);
			this.m_LastBounds = targetGroup.GetViewSpaceBoundingBox(this.m_lastBoundsMatrix);
			float num = this.GetTargetHeight(this.m_LastBounds);
			Vector3 a = this.m_lastBoundsMatrix.MultiplyPoint3x4(this.m_LastBounds.center);
			if (deltaTime >= 0f)
			{
				float num2 = num - this.m_prevTargetHeight;
				num2 = Damper.Damp(num2, this.m_FrameDamping, deltaTime);
				num = this.m_prevTargetHeight + num2;
			}
			this.m_prevTargetHeight = num;
			if (!curState.Lens.Orthographic && this.m_AdjustmentMode != CinemachineGroupComposer.AdjustmentMode.ZoomOnly)
			{
				float fieldOfView = curState.Lens.FieldOfView;
				float num3 = num / (2f * Mathf.Tan(fieldOfView * 0.0174532924f / 2f)) + this.m_LastBounds.extents.z;
				num3 = Mathf.Clamp(num3, magnitude - this.m_MaxDollyIn, magnitude + this.m_MaxDollyOut);
				num3 = Mathf.Clamp(num3, this.m_MinimumDistance, this.m_MaximumDistance);
				curState.PositionCorrection += a - vector * num3 - curState.RawPosition;
			}
			if (curState.Lens.Orthographic || this.m_AdjustmentMode != CinemachineGroupComposer.AdjustmentMode.DollyOnly)
			{
				float num4 = (base.TrackedPoint - curState.CorrectedPosition).magnitude - this.m_LastBounds.extents.z;
				float value = 179f;
				if (num4 > 0.0001f)
				{
					value = 2f * Mathf.Atan(num / (2f * num4)) * 57.29578f;
				}
				LensSettings lens = curState.Lens;
				lens.FieldOfView = Mathf.Clamp(value, this.m_MinimumFOV, this.m_MaximumFOV);
				lens.OrthographicSize = Mathf.Clamp(num / 2f, this.m_MinimumOrthoSize, this.m_MaximumOrthoSize);
				curState.Lens = lens;
			}
			base.MutateCameraState(ref curState, deltaTime);
		}

		public Bounds m_LastBounds { get; private set; }

		public Matrix4x4 m_lastBoundsMatrix { get; private set; }

		private float GetTargetHeight(Bounds b)
		{
			float num = Mathf.Max(0.0001f, this.m_GroupFramingSize);
			switch (this.m_FramingMode)
			{
			case CinemachineGroupComposer.FramingMode.Horizontal:
				return Mathf.Max(0.0001f, b.size.x) / (num * base.VcamState.Lens.Aspect);
			case CinemachineGroupComposer.FramingMode.Vertical:
				return Mathf.Max(0.0001f, b.size.y) / num;
			}
			return Mathf.Max(Mathf.Max(0.0001f, b.size.x) / (num * base.VcamState.Lens.Aspect), Mathf.Max(0.0001f, b.size.y) / num);
		}

		[Space]
		[Tooltip("The bounding box of the targets should occupy this amount of the screen space.  1 means fill the whole screen.  0.5 means fill half the screen, etc.")]
		public float m_GroupFramingSize = 0.8f;

		[Tooltip("What screen dimensions to consider when framing.  Can be Horizontal, Vertical, or both")]
		public CinemachineGroupComposer.FramingMode m_FramingMode = CinemachineGroupComposer.FramingMode.HorizontalAndVertical;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to frame the group. Small numbers are more responsive, rapidly adjusting the camera to keep the group in the frame.  Larger numbers give a more heavy slowly responding camera.")]
		public float m_FrameDamping = 2f;

		[Tooltip("How to adjust the camera to get the desired framing.  You can zoom, dolly in/out, or do both.")]
		public CinemachineGroupComposer.AdjustmentMode m_AdjustmentMode = CinemachineGroupComposer.AdjustmentMode.DollyThenZoom;

		[Tooltip("The maximum distance toward the target that this behaviour is allowed to move the camera.")]
		public float m_MaxDollyIn = 5000f;

		[Tooltip("The maximum distance away the target that this behaviour is allowed to move the camera.")]
		public float m_MaxDollyOut = 5000f;

		[Tooltip("Set this to limit how close to the target the camera can get.")]
		public float m_MinimumDistance = 1f;

		[Tooltip("Set this to limit how far from the target the camera can get.")]
		public float m_MaximumDistance = 5000f;

		[Range(1f, 179f)]
		[Tooltip("If adjusting FOV, will not set the FOV lower than this.")]
		public float m_MinimumFOV = 3f;

		[Range(1f, 179f)]
		[Tooltip("If adjusting FOV, will not set the FOV higher than this.")]
		public float m_MaximumFOV = 60f;

		[Tooltip("If adjusting Orthographic Size, will not set it lower than this.")]
		public float m_MinimumOrthoSize = 1f;

		[Tooltip("If adjusting Orthographic Size, will not set it higher than this.")]
		public float m_MaximumOrthoSize = 100f;

		private float m_prevTargetHeight;

		[DocumentationSorting(4.01f, DocumentationSortingAttribute.Level.UserRef)]
		public enum FramingMode
		{
			Horizontal,
			Vertical,
			HorizontalAndVertical
		}

		public enum AdjustmentMode
		{
			ZoomOnly,
			DollyOnly,
			DollyThenZoom
		}
	}
}
