using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[RequireComponent(typeof(CinemachinePipeline))]
	[SaveDuringPlay]
	[AddComponentMenu("")]
	[ExecuteInEditMode]
	[DocumentationSorting(3f, DocumentationSortingAttribute.Level.UserRef)]
	public class CinemachineComposer : CinemachineComponentBase
	{
		public override bool IsValid
		{
			get
			{
				return base.enabled && base.LookAtTarget != null;
			}
		}

		public override CinemachineCore.Stage Stage
		{
			get
			{
				return CinemachineCore.Stage.Aim;
			}
		}

		public Vector3 TrackedPoint { get; private set; }

		protected virtual Vector3 GetLookAtPointAndSetTrackedPoint(Vector3 lookAt)
		{
			Vector3 vector = lookAt;
			if (base.LookAtTarget != null)
			{
				vector += base.LookAtTarget.transform.rotation * this.m_TrackedObjectOffset;
			}
			this.m_Predictor.Smoothing = this.m_LookaheadSmoothing;
			this.m_Predictor.AddPosition(vector);
			this.TrackedPoint = ((this.m_LookaheadTime <= 0f) ? vector : this.m_Predictor.PredictPosition(this.m_LookaheadTime));
			return vector;
		}

		public override void PrePipelineMutateCameraState(ref CameraState curState)
		{
			if (this.IsValid && curState.HasLookAt)
			{
				curState.ReferenceLookAt = this.GetLookAtPointAndSetTrackedPoint(curState.ReferenceLookAt);
			}
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			if (deltaTime < 0f)
			{
				this.m_Predictor.Reset();
			}
			if (!this.IsValid || !curState.HasLookAt)
			{
				return;
			}
			float magnitude = (this.TrackedPoint - curState.CorrectedPosition).magnitude;
			if (magnitude < 0.0001f)
			{
				if (deltaTime >= 0f)
				{
					curState.RawOrientation = this.m_CameraOrientationPrevFrame;
				}
				return;
			}
			float num;
			float fovH;
			if (curState.Lens.Orthographic)
			{
				num = 114.59156f * Mathf.Atan(curState.Lens.OrthographicSize / magnitude);
				fovH = 114.59156f * Mathf.Atan(curState.Lens.Aspect * curState.Lens.OrthographicSize / magnitude);
			}
			else
			{
				num = curState.Lens.FieldOfView;
				double num2 = 2.0 * Math.Atan(Math.Tan((double)(num * 0.0174532924f / 2f)) * (double)curState.Lens.Aspect);
				fovH = (float)(57.295780181884766 * num2);
			}
			Quaternion quaternion = curState.RawOrientation;
			Rect screenRect = this.ScreenToFOV(this.SoftGuideRect, num, fovH, curState.Lens.Aspect);
			if (deltaTime < 0f)
			{
				Rect screenRect2 = new Rect(screenRect.center, Vector2.zero);
				this.RotateToScreenBounds(ref curState, screenRect2, ref quaternion, num, fovH, -1f);
			}
			else
			{
				Vector3 vector = this.m_LookAtPrevFrame - (this.m_CameraPosPrevFrame + curState.PositionDampingBypass);
				if (vector.AlmostZero())
				{
					quaternion = Quaternion.LookRotation(this.m_CameraOrientationPrevFrame * Vector3.forward, curState.ReferenceUp);
				}
				else
				{
					quaternion = Quaternion.LookRotation(vector, curState.ReferenceUp);
					quaternion = quaternion.ApplyCameraRotation(-this.m_ScreenOffsetPrevFrame, curState.ReferenceUp);
				}
				Rect screenRect3 = this.ScreenToFOV(this.HardGuideRect, num, fovH, curState.Lens.Aspect);
				if (!this.RotateToScreenBounds(ref curState, screenRect3, ref quaternion, num, fovH, -1f))
				{
					this.RotateToScreenBounds(ref curState, screenRect, ref quaternion, num, fovH, deltaTime);
				}
			}
			this.m_CameraPosPrevFrame = curState.CorrectedPosition;
			this.m_LookAtPrevFrame = this.TrackedPoint;
			this.m_CameraOrientationPrevFrame = quaternion.Normalized();
			this.m_ScreenOffsetPrevFrame = this.m_CameraOrientationPrevFrame.GetCameraRotationToTarget(this.m_LookAtPrevFrame - curState.CorrectedPosition, curState.ReferenceUp);
			curState.RawOrientation = this.m_CameraOrientationPrevFrame;
		}

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

		private Rect ScreenToFOV(Rect rScreen, float fov, float fovH, float aspect)
		{
			Rect result = new Rect(rScreen);
			Matrix4x4 inverse = Matrix4x4.Perspective(fov, aspect, 0.01f, 10000f).inverse;
			Vector3 to = inverse.MultiplyPoint(new Vector3(0f, result.yMin * 2f - 1f, 0.1f));
			to.z = -to.z;
			float num = UnityVectorExtensions.SignedAngle(Vector3.forward, to, Vector3.left);
			result.yMin = (fov / 2f + num) / fov;
			to = inverse.MultiplyPoint(new Vector3(0f, result.yMax * 2f - 1f, 0.1f));
			to.z = -to.z;
			num = UnityVectorExtensions.SignedAngle(Vector3.forward, to, Vector3.left);
			result.yMax = (fov / 2f + num) / fov;
			to = inverse.MultiplyPoint(new Vector3(result.xMin * 2f - 1f, 0f, 0.1f));
			to.z = -to.z;
			num = UnityVectorExtensions.SignedAngle(Vector3.forward, to, Vector3.up);
			result.xMin = (fovH / 2f + num) / fovH;
			to = inverse.MultiplyPoint(new Vector3(result.xMax * 2f - 1f, 0f, 0.1f));
			to.z = -to.z;
			num = UnityVectorExtensions.SignedAngle(Vector3.forward, to, Vector3.up);
			result.xMax = (fovH / 2f + num) / fovH;
			return result;
		}

		private bool RotateToScreenBounds(ref CameraState state, Rect screenRect, ref Quaternion rigOrientation, float fov, float fovH, float deltaTime)
		{
			Vector3 vector = this.TrackedPoint - state.CorrectedPosition;
			Vector2 cameraRotationToTarget = rigOrientation.GetCameraRotationToTarget(vector, state.ReferenceUp);
			this.ClampVerticalBounds(ref screenRect, vector, state.ReferenceUp, fov);
			float num = (screenRect.yMin - 0.5f) * fov;
			float num2 = (screenRect.yMax - 0.5f) * fov;
			if (cameraRotationToTarget.x < num)
			{
				cameraRotationToTarget.x -= num;
			}
			else if (cameraRotationToTarget.x > num2)
			{
				cameraRotationToTarget.x -= num2;
			}
			else
			{
				cameraRotationToTarget.x = 0f;
			}
			num = (screenRect.xMin - 0.5f) * fovH;
			num2 = (screenRect.xMax - 0.5f) * fovH;
			if (cameraRotationToTarget.y < num)
			{
				cameraRotationToTarget.y -= num;
			}
			else if (cameraRotationToTarget.y > num2)
			{
				cameraRotationToTarget.y -= num2;
			}
			else
			{
				cameraRotationToTarget.y = 0f;
			}
			if (deltaTime >= 0f)
			{
				cameraRotationToTarget.x = Damper.Damp(cameraRotationToTarget.x, this.m_VerticalDamping, deltaTime);
				cameraRotationToTarget.y = Damper.Damp(cameraRotationToTarget.y, this.m_HorizontalDamping, deltaTime);
			}
			rigOrientation = rigOrientation.ApplyCameraRotation(cameraRotationToTarget, state.ReferenceUp);
			return false;
		}

		private bool ClampVerticalBounds(ref Rect r, Vector3 dir, Vector3 up, float fov)
		{
			float num = Vector3.Angle(dir, up);
			float num2 = fov / 2f + 1f;
			if (num < num2)
			{
				float num3 = 1f - (num2 - num) / fov;
				if (r.yMax > num3)
				{
					r.yMin = Mathf.Min(r.yMin, num3);
					r.yMax = Mathf.Min(r.yMax, num3);
					return true;
				}
			}
			if (num > 180f - num2)
			{
				float num4 = (num - (180f - num2)) / fov;
				if (num4 > r.yMin)
				{
					r.yMin = Mathf.Max(r.yMin, num4);
					r.yMax = Mathf.Max(r.yMax, num4);
					return true;
				}
			}
			return false;
		}

		[NoSaveDuringPlay]
		[HideInInspector]
		public Action OnGUICallback;

		[Tooltip("Target offset from the target object's center in target-local space. Use this to fine-tune the tracking target position when the desired area is not the tracked object's center.")]
		public Vector3 m_TrackedObjectOffset = Vector3.zero;

		[Tooltip("This setting will instruct the composer to adjust its target offset based on the motion of the target.  The composer will look at a point where it estimates the target will be this many seconds into the future.  Note that this setting is sensitive to noisy animation, and can amplify the noise, resulting in undesirable camera jitter.  If the camera jitters unacceptably when the target is in motion, turn down this setting, or animate the target more smoothly.")]
		[Range(0f, 1f)]
		public float m_LookaheadTime;

		[Tooltip("Controls the smoothness of the lookahead algorithm.  Larger values smooth out jittery predictions and also increase prediction lag")]
		[Range(3f, 30f)]
		public float m_LookaheadSmoothing = 10f;

		[Tooltip("How aggressively the camera tries to follow the target in the screen-horizontal direction. Small numbers are more responsive, rapidly orienting the camera to keep the target in the dead zone. Larger numbers give a more heavy slowly responding camera. Using different vertical and horizontal settings can yield a wide range of camera behaviors.")]
		[Range(0f, 20f)]
		[Space]
		public float m_HorizontalDamping = 0.5f;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to follow the target in the screen-vertical direction. Small numbers are more responsive, rapidly orienting the camera to keep the target in the dead zone. Larger numbers give a more heavy slowly responding camera. Using different vertical and horizontal settings can yield a wide range of camera behaviors.")]
		public float m_VerticalDamping = 0.5f;

		[Range(0f, 1f)]
		[Space]
		[Tooltip("Horizontal screen position for target. The camera will rotate to position the tracked object here.")]
		public float m_ScreenX = 0.5f;

		[Range(0f, 1f)]
		[Tooltip("Vertical screen position for target, The camera will rotate to position the tracked object here.")]
		public float m_ScreenY = 0.5f;

		[Range(0f, 1f)]
		[Tooltip("Camera will not rotate horizontally if the target is within this range of the position.")]
		public float m_DeadZoneWidth = 0.1f;

		[Tooltip("Camera will not rotate vertically if the target is within this range of the position.")]
		[Range(0f, 1f)]
		public float m_DeadZoneHeight = 0.1f;

		[Tooltip("When target is within this region, camera will gradually rotate horizontally to re-align towards the desired position, depending on the damping speed.")]
		[Range(0f, 2f)]
		public float m_SoftZoneWidth = 0.8f;

		[Range(0f, 2f)]
		[Tooltip("When target is within this region, camera will gradually rotate vertically to re-align towards the desired position, depending on the damping speed.")]
		public float m_SoftZoneHeight = 0.8f;

		[Tooltip("A non-zero bias will move the target position horizontally away from the center of the soft zone.")]
		[Range(-0.5f, 0.5f)]
		public float m_BiasX;

		[Tooltip("A non-zero bias will move the target position vertically away from the center of the soft zone.")]
		[Range(-0.5f, 0.5f)]
		public float m_BiasY;

		private Vector3 m_CameraPosPrevFrame = Vector3.zero;

		private Vector3 m_LookAtPrevFrame = Vector3.zero;

		private Vector2 m_ScreenOffsetPrevFrame = Vector2.zero;

		private Quaternion m_CameraOrientationPrevFrame = Quaternion.identity;

		private PositionPredictor m_Predictor = new PositionPredictor();
	}
}
