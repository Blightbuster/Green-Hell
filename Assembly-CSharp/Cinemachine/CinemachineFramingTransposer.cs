using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[RequireComponent(typeof(CinemachinePipeline))]
	[ExecuteInEditMode]
	[DocumentationSorting(5.5f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("")]
	[SaveDuringPlay]
	public class CinemachineFramingTransposer : CinemachineComponentBase
	{
		public Rect SoftGuideRect
		{
			get
			{
				return new Rect(this.m_ScreenX - this.m_DeadZoneWidth / 2f, this.m_ScreenY - this.m_DeadZoneHeight / 2f, this.m_DeadZoneWidth, this.m_DeadZoneHeight);
			}
			set
			{
				this.m_DeadZoneWidth = Mathf.Clamp01(value.width);
				this.m_DeadZoneHeight = Mathf.Clamp01(value.height);
				this.m_ScreenX = Mathf.Clamp01(value.x + this.m_DeadZoneWidth / 2f);
				this.m_ScreenY = Mathf.Clamp01(value.y + this.m_DeadZoneHeight / 2f);
				this.m_SoftZoneWidth = Mathf.Max(this.m_SoftZoneWidth, this.m_DeadZoneWidth);
				this.m_SoftZoneHeight = Mathf.Max(this.m_SoftZoneHeight, this.m_DeadZoneHeight);
			}
		}

		public Rect HardGuideRect
		{
			get
			{
				Rect result = new Rect(this.m_ScreenX - this.m_SoftZoneWidth / 2f, this.m_ScreenY - this.m_SoftZoneHeight / 2f, this.m_SoftZoneWidth, this.m_SoftZoneHeight);
				result.position += new Vector2(this.m_BiasX * (this.m_SoftZoneWidth - this.m_DeadZoneWidth), this.m_BiasY * (this.m_SoftZoneHeight - this.m_DeadZoneHeight));
				return result;
			}
			set
			{
				this.m_SoftZoneWidth = Mathf.Clamp(value.width, 0f, 2f);
				this.m_SoftZoneHeight = Mathf.Clamp(value.height, 0f, 2f);
				this.m_DeadZoneWidth = Mathf.Min(this.m_DeadZoneWidth, this.m_SoftZoneWidth);
				this.m_DeadZoneHeight = Mathf.Min(this.m_DeadZoneHeight, this.m_SoftZoneHeight);
				Vector2 center = value.center;
				Vector2 vector = center - new Vector2(this.m_ScreenX, this.m_ScreenY);
				float num = Mathf.Max(0f, this.m_SoftZoneWidth - this.m_DeadZoneWidth);
				float num2 = Mathf.Max(0f, this.m_SoftZoneHeight - this.m_DeadZoneHeight);
				this.m_BiasX = ((num >= 0.0001f) ? Mathf.Clamp(vector.x / num, -0.5f, 0.5f) : 0f);
				this.m_BiasY = ((num2 >= 0.0001f) ? Mathf.Clamp(vector.y / num2, -0.5f, 0.5f) : 0f);
			}
		}

		private void OnValidate()
		{
			this.m_CameraDistance = Mathf.Max(this.m_CameraDistance, 0.01f);
			this.m_DeadZoneDepth = Mathf.Max(this.m_DeadZoneDepth, 0f);
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

		public override bool IsValid
		{
			get
			{
				return base.enabled && base.FollowTarget != null && base.LookAtTarget == null;
			}
		}

		public override CinemachineCore.Stage Stage
		{
			get
			{
				return CinemachineCore.Stage.Body;
			}
		}

		public Vector3 TrackedPoint { get; private set; }

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			if (deltaTime < 0f)
			{
				this.m_Predictor.Reset();
				this.m_PreviousCameraPosition = curState.RawPosition + curState.RawOrientation * Vector3.back * this.m_CameraDistance;
			}
			if (!this.IsValid)
			{
				return;
			}
			Vector3 previousCameraPosition = this.m_PreviousCameraPosition;
			curState.ReferenceLookAt = base.FollowTarget.position;
			this.m_Predictor.Smoothing = this.m_LookaheadSmoothing;
			this.m_Predictor.AddPosition(curState.ReferenceLookAt);
			this.TrackedPoint = ((this.m_LookaheadTime <= 0f) ? curState.ReferenceLookAt : this.m_Predictor.PredictPosition(this.m_LookaheadTime));
			Quaternion rawOrientation = curState.RawOrientation;
			Quaternion rotation = Quaternion.Inverse(rawOrientation);
			Vector3 vector = rotation * previousCameraPosition;
			Vector3 targetPos2D = rotation * this.TrackedPoint - vector;
			Vector3 vector2 = Vector3.zero;
			float num = Mathf.Max(0.01f, this.m_CameraDistance - this.m_DeadZoneDepth / 2f);
			float num2 = Mathf.Max(num, this.m_CameraDistance + this.m_DeadZoneDepth / 2f);
			if (targetPos2D.z < num)
			{
				vector2.z = targetPos2D.z - num;
			}
			if (targetPos2D.z > num2)
			{
				vector2.z = targetPos2D.z - num2;
			}
			CinemachineTargetGroup targetGroup = this.TargetGroup;
			if (targetGroup != null && this.m_GroupFramingMode != CinemachineFramingTransposer.FramingMode.None)
			{
				vector2.z += this.AdjustCameraDepthAndLensForGroupFraming(targetGroup, targetPos2D.z - vector2.z, ref curState, deltaTime);
			}
			targetPos2D.z -= vector2.z;
			float orthoSize = (!curState.Lens.Orthographic) ? (Mathf.Tan(0.5f * curState.Lens.FieldOfView * 0.0174532924f) * targetPos2D.z) : curState.Lens.OrthographicSize;
			Rect screenRect = this.ScreenToOrtho(this.SoftGuideRect, orthoSize, curState.Lens.Aspect);
			if (deltaTime < 0f)
			{
				Rect screenRect2 = new Rect(screenRect.center, Vector2.zero);
				vector2 += this.OrthoOffsetToScreenBounds(targetPos2D, screenRect2);
			}
			else
			{
				vector2 += this.OrthoOffsetToScreenBounds(targetPos2D, screenRect);
				Vector3 vector3 = Vector3.zero;
				if (!this.m_UnlimitedSoftZone)
				{
					Rect screenRect3 = this.ScreenToOrtho(this.HardGuideRect, orthoSize, curState.Lens.Aspect);
					vector3 = this.OrthoOffsetToScreenBounds(targetPos2D, screenRect3);
					float d = Mathf.Max(vector3.x / (vector2.x + 0.0001f), vector3.y / (vector2.y + 0.0001f));
					vector3 = vector2 * d;
				}
				vector2 = vector3 + Damper.Damp(vector2 - vector3, new Vector3(this.m_XDamping, this.m_YDamping, this.m_ZDamping), deltaTime);
			}
			curState.RawPosition = (this.m_PreviousCameraPosition = rawOrientation * (vector + vector2));
		}

		private Rect ScreenToOrtho(Rect rScreen, float orthoSize, float aspect)
		{
			return new Rect
			{
				yMax = 2f * orthoSize * (1f - rScreen.yMin - 0.5f),
				yMin = 2f * orthoSize * (1f - rScreen.yMax - 0.5f),
				xMin = 2f * orthoSize * aspect * (rScreen.xMin - 0.5f),
				xMax = 2f * orthoSize * aspect * (rScreen.xMax - 0.5f)
			};
		}

		private Vector3 OrthoOffsetToScreenBounds(Vector3 targetPos2D, Rect screenRect)
		{
			Vector3 zero = Vector3.zero;
			if (targetPos2D.x < screenRect.xMin)
			{
				zero.x += targetPos2D.x - screenRect.xMin;
			}
			if (targetPos2D.x > screenRect.xMax)
			{
				zero.x += targetPos2D.x - screenRect.xMax;
			}
			if (targetPos2D.y < screenRect.yMin)
			{
				zero.y += targetPos2D.y - screenRect.yMin;
			}
			if (targetPos2D.y > screenRect.yMax)
			{
				zero.y += targetPos2D.y - screenRect.yMax;
			}
			return zero;
		}

		public Bounds m_LastBounds { get; private set; }

		public Matrix4x4 m_lastBoundsMatrix { get; private set; }

		public CinemachineTargetGroup TargetGroup
		{
			get
			{
				Transform followTarget = base.FollowTarget;
				if (followTarget != null)
				{
					return followTarget.GetComponent<CinemachineTargetGroup>();
				}
				return null;
			}
		}

		private float AdjustCameraDepthAndLensForGroupFraming(CinemachineTargetGroup group, float targetZ, ref CameraState curState, float deltaTime)
		{
			float num = 0f;
			Bounds boundingBox = group.BoundingBox;
			Vector3 a = curState.RawOrientation * Vector3.forward;
			this.m_lastBoundsMatrix = Matrix4x4.TRS(boundingBox.center - a * boundingBox.extents.magnitude, curState.RawOrientation, Vector3.one);
			this.m_LastBounds = group.GetViewSpaceBoundingBox(this.m_lastBoundsMatrix);
			float num2 = this.GetTargetHeight(this.m_LastBounds);
			if (deltaTime >= 0f)
			{
				float num3 = num2 - this.m_prevTargetHeight;
				num3 = Damper.Damp(num3, this.m_ZDamping, deltaTime);
				num2 = this.m_prevTargetHeight + num3;
			}
			this.m_prevTargetHeight = num2;
			if (!curState.Lens.Orthographic && this.m_AdjustmentMode != CinemachineFramingTransposer.AdjustmentMode.ZoomOnly)
			{
				float num4 = num2 / (2f * Mathf.Tan(curState.Lens.FieldOfView * 0.0174532924f / 2f));
				num4 += this.m_LastBounds.extents.z;
				num4 = Mathf.Clamp(num4, targetZ - this.m_MaxDollyIn, targetZ + this.m_MaxDollyOut);
				num4 = Mathf.Clamp(num4, this.m_MinimumDistance, this.m_MaximumDistance);
				num += num4 - targetZ;
			}
			if (curState.Lens.Orthographic || this.m_AdjustmentMode != CinemachineFramingTransposer.AdjustmentMode.DollyOnly)
			{
				float num5 = targetZ + num - this.m_LastBounds.extents.z;
				float value = 179f;
				if (num5 > 0.0001f)
				{
					value = 2f * Mathf.Atan(num2 / (2f * num5)) * 57.29578f;
				}
				LensSettings lens = curState.Lens;
				lens.FieldOfView = Mathf.Clamp(value, this.m_MinimumFOV, this.m_MaximumFOV);
				lens.OrthographicSize = Mathf.Clamp(num2 / 2f, this.m_MinimumOrthoSize, this.m_MaximumOrthoSize);
				curState.Lens = lens;
			}
			return -num;
		}

		private float GetTargetHeight(Bounds b)
		{
			float num = Mathf.Max(0.0001f, this.m_GroupFramingSize);
			switch (this.m_GroupFramingMode)
			{
			case CinemachineFramingTransposer.FramingMode.Horizontal:
				return b.size.x / (num * base.VcamState.Lens.Aspect);
			case CinemachineFramingTransposer.FramingMode.Vertical:
				return b.size.y / num;
			}
			return Mathf.Max(b.size.x / (num * base.VcamState.Lens.Aspect), b.size.y / num);
		}

		[NoSaveDuringPlay]
		[HideInInspector]
		public Action OnGUICallback;

		[Tooltip("This setting will instruct the composer to adjust its target offset based on the motion of the target.  The composer will look at a point where it estimates the target will be this many seconds into the future.  Note that this setting is sensitive to noisy animation, and can amplify the noise, resulting in undesirable camera jitter.  If the camera jitters unacceptably when the target is in motion, turn down this setting, or animate the target more smoothly.")]
		[Range(0f, 1f)]
		public float m_LookaheadTime;

		[Range(3f, 30f)]
		[Tooltip("Controls the smoothness of the lookahead algorithm.  Larger values smooth out jittery predictions and also increase prediction lag")]
		public float m_LookaheadSmoothing = 10f;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain the offset in the X-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's x-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public float m_XDamping = 1f;

		[Tooltip("How aggressively the camera tries to maintain the offset in the Y-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's y-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		[Range(0f, 20f)]
		public float m_YDamping = 1f;

		[Tooltip("How aggressively the camera tries to maintain the offset in the Z-axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's z-axis offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		[Range(0f, 20f)]
		public float m_ZDamping = 1f;

		[Range(0f, 1f)]
		[Space]
		[Tooltip("Horizontal screen position for target. The camera will move to position the tracked object here.")]
		public float m_ScreenX = 0.5f;

		[Tooltip("Vertical screen position for target, The camera will move to position the tracked object here.")]
		[Range(0f, 1f)]
		public float m_ScreenY = 0.5f;

		[Tooltip("The distance along the camera axis that will be maintained from the Follow target")]
		public float m_CameraDistance = 10f;

		[Space]
		[Range(0f, 1f)]
		[Tooltip("Camera will not move horizontally if the target is within this range of the position.")]
		public float m_DeadZoneWidth = 0.1f;

		[Range(0f, 1f)]
		[Tooltip("Camera will not move vertically if the target is within this range of the position.")]
		public float m_DeadZoneHeight = 0.1f;

		[Tooltip("The camera will not move along its z-axis if the Follow target is within this distance of the specified camera distance")]
		[FormerlySerializedAs("m_DistanceDeadZoneSize")]
		public float m_DeadZoneDepth;

		[Tooltip("If checked, then then soft zone will be unlimited in size.")]
		[Space]
		public bool m_UnlimitedSoftZone;

		[Tooltip("When target is within this region, camera will gradually move horizontally to re-align towards the desired position, depending on the damping speed.")]
		[Range(0f, 2f)]
		public float m_SoftZoneWidth = 0.8f;

		[Tooltip("When target is within this region, camera will gradually move vertically to re-align towards the desired position, depending on the damping speed.")]
		[Range(0f, 2f)]
		public float m_SoftZoneHeight = 0.8f;

		[Range(-0.5f, 0.5f)]
		[Tooltip("A non-zero bias will move the target position horizontally away from the center of the soft zone.")]
		public float m_BiasX;

		[Range(-0.5f, 0.5f)]
		[Tooltip("A non-zero bias will move the target position vertically away from the center of the soft zone.")]
		public float m_BiasY;

		[FormerlySerializedAs("m_FramingMode")]
		[Space]
		[Tooltip("What screen dimensions to consider when framing.  Can be Horizontal, Vertical, or both")]
		public CinemachineFramingTransposer.FramingMode m_GroupFramingMode = CinemachineFramingTransposer.FramingMode.HorizontalAndVertical;

		[Tooltip("How to adjust the camera to get the desired framing.  You can zoom, dolly in/out, or do both.")]
		public CinemachineFramingTransposer.AdjustmentMode m_AdjustmentMode = CinemachineFramingTransposer.AdjustmentMode.DollyThenZoom;

		[Tooltip("The bounding box of the targets should occupy this amount of the screen space.  1 means fill the whole screen.  0.5 means fill half the screen, etc.")]
		public float m_GroupFramingSize = 0.8f;

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

		[Tooltip("If adjusting FOV, will not set the FOV higher than this.")]
		[Range(1f, 179f)]
		public float m_MaximumFOV = 60f;

		[Tooltip("If adjusting Orthographic Size, will not set it lower than this.")]
		public float m_MinimumOrthoSize = 1f;

		[Tooltip("If adjusting Orthographic Size, will not set it higher than this.")]
		public float m_MaximumOrthoSize = 100f;

		private const float kMinimumCameraDistance = 0.01f;

		private Vector3 m_PreviousCameraPosition = Vector3.zero;

		private PositionPredictor m_Predictor = new PositionPredictor();

		private float m_prevTargetHeight;

		[DocumentationSorting(4.01f, DocumentationSortingAttribute.Level.UserRef)]
		public enum FramingMode
		{
			Horizontal,
			Vertical,
			HorizontalAndVertical,
			None
		}

		public enum AdjustmentMode
		{
			ZoomOnly,
			DollyOnly,
			DollyThenZoom
		}
	}
}
