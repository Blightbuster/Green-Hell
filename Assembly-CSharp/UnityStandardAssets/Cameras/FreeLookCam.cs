using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Cameras
{
	public class FreeLookCam : PivotBasedCameraRig
	{
		protected override void Awake()
		{
			base.Awake();
			Cursor.lockState = ((!this.m_LockCursor) ? CursorLockMode.None : CursorLockMode.Locked);
			Cursor.visible = !this.m_LockCursor;
			this.m_PivotEulers = this.m_Pivot.rotation.eulerAngles;
			this.m_PivotTargetRot = this.m_Pivot.transform.localRotation;
			this.m_TransformTargetRot = base.transform.localRotation;
		}

		protected void Update()
		{
			this.HandleRotationMovement();
			if (this.m_LockCursor && Input.GetMouseButtonUp(0))
			{
				Cursor.lockState = ((!this.m_LockCursor) ? CursorLockMode.None : CursorLockMode.Locked);
				Cursor.visible = !this.m_LockCursor;
			}
		}

		private void OnDisable()
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		protected override void FollowTarget(float deltaTime)
		{
			if (this.m_Target == null)
			{
				return;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, this.m_Target.position, deltaTime * this.m_MoveSpeed);
		}

		private void HandleRotationMovement()
		{
			if (Time.timeScale < 1.401298E-45f)
			{
				return;
			}
			float axis = CrossPlatformInputManager.GetAxis("Mouse X");
			float axis2 = CrossPlatformInputManager.GetAxis("Mouse Y");
			this.m_LookAngle += axis * this.m_TurnSpeed;
			this.m_TransformTargetRot = Quaternion.Euler(0f, this.m_LookAngle, 0f);
			if (this.m_VerticalAutoReturn)
			{
				this.m_TiltAngle = ((axis2 <= 0f) ? Mathf.Lerp(0f, this.m_TiltMax, -axis2) : Mathf.Lerp(0f, -this.m_TiltMin, axis2));
			}
			else
			{
				this.m_TiltAngle -= axis2 * this.m_TurnSpeed;
				this.m_TiltAngle = Mathf.Clamp(this.m_TiltAngle, -this.m_TiltMin, this.m_TiltMax);
			}
			this.m_PivotTargetRot = Quaternion.Euler(this.m_TiltAngle, this.m_PivotEulers.y, this.m_PivotEulers.z);
			if (this.m_TurnSmoothing > 0f)
			{
				this.m_Pivot.localRotation = Quaternion.Slerp(this.m_Pivot.localRotation, this.m_PivotTargetRot, this.m_TurnSmoothing * Time.deltaTime);
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, this.m_TransformTargetRot, this.m_TurnSmoothing * Time.deltaTime);
			}
			else
			{
				this.m_Pivot.localRotation = this.m_PivotTargetRot;
				base.transform.localRotation = this.m_TransformTargetRot;
			}
		}

		[SerializeField]
		private float m_MoveSpeed = 1f;

		[SerializeField]
		[Range(0f, 10f)]
		private float m_TurnSpeed = 1.5f;

		[SerializeField]
		private float m_TurnSmoothing = 0.1f;

		[SerializeField]
		private float m_TiltMax = 75f;

		[SerializeField]
		private float m_TiltMin = 45f;

		[SerializeField]
		private bool m_LockCursor;

		[SerializeField]
		private bool m_VerticalAutoReturn;

		private float m_LookAngle;

		private float m_TiltAngle;

		private const float k_LookDistance = 100f;

		private Vector3 m_PivotEulers;

		private Quaternion m_PivotTargetRot;

		private Quaternion m_TransformTargetRot;
	}
}
