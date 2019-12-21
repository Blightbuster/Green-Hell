using System;
using UnityEngine;

namespace LuxWater.Demo
{
	public class LuxWater_ExtendedFlycam : MonoBehaviour
	{
		private void Start()
		{
			this.rotationX = base.transform.eulerAngles.y;
		}

		private void Update()
		{
			float deltaTime = Time.deltaTime;
			this.rotationX += Input.GetAxis("Mouse X") * this.cameraSensitivity * deltaTime;
			this.rotationY += Input.GetAxis("Mouse Y") * this.cameraSensitivity * deltaTime;
			this.rotationY = Mathf.Clamp(this.rotationY, -90f, 90f);
			Quaternion quaternion = Quaternion.AngleAxis(this.rotationX, Vector3.up);
			quaternion *= Quaternion.AngleAxis(this.rotationY, Vector3.left);
			base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, quaternion, deltaTime * 6f);
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				base.transform.position += base.transform.forward * (this.normalMoveSpeed * this.fastMoveFactor) * Input.GetAxis("Vertical") * deltaTime;
				base.transform.position += base.transform.right * (this.normalMoveSpeed * this.fastMoveFactor) * Input.GetAxis("Horizontal") * deltaTime;
			}
			else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				base.transform.position += base.transform.forward * (this.normalMoveSpeed * this.slowMoveFactor) * Input.GetAxis("Vertical") * deltaTime;
				base.transform.position += base.transform.right * (this.normalMoveSpeed * this.slowMoveFactor) * Input.GetAxis("Horizontal") * deltaTime;
			}
			else
			{
				base.transform.position += base.transform.forward * this.normalMoveSpeed * Input.GetAxis("Vertical") * deltaTime;
				base.transform.position += base.transform.right * this.normalMoveSpeed * Input.GetAxis("Horizontal") * deltaTime;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				base.transform.position -= base.transform.up * this.climbSpeed * deltaTime;
			}
			if (Input.GetKey(KeyCode.E))
			{
				base.transform.position += base.transform.up * this.climbSpeed * deltaTime;
			}
		}

		public float cameraSensitivity = 90f;

		public float climbSpeed = 4f;

		public float normalMoveSpeed = 10f;

		public float slowMoveFactor = 0.25f;

		public float fastMoveFactor = 3f;

		private float rotationX;

		private float rotationY;
	}
}
