using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(ThirdPersonCharacter))]
	public class ThirdPersonUserControl : MonoBehaviour
	{
		private void Start()
		{
			if (Camera.main != null)
			{
				this.m_Cam = Camera.main.transform;
			}
			else
			{
				Debug.LogWarning("Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
			}
			this.m_Character = base.GetComponent<ThirdPersonCharacter>();
		}

		private void Update()
		{
			if (!this.m_Jump)
			{
				this.m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
		}

		private void FixedUpdate()
		{
			float axis = CrossPlatformInputManager.GetAxis("Horizontal");
			float axis2 = CrossPlatformInputManager.GetAxis("Vertical");
			bool key = Input.GetKey(KeyCode.C);
			if (this.m_Cam != null)
			{
				this.m_CamForward = Vector3.Scale(this.m_Cam.forward, new Vector3(1f, 0f, 1f)).normalized;
				this.m_Move = axis2 * this.m_CamForward + axis * this.m_Cam.right;
			}
			else
			{
				this.m_Move = axis2 * Vector3.forward + axis * Vector3.right;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.m_Move *= 0.5f;
			}
			this.m_Character.Move(this.m_Move, key, this.m_Jump);
			this.m_Jump = false;
		}

		private ThirdPersonCharacter m_Character;

		private Transform m_Cam;

		private Vector3 m_CamForward;

		private Vector3 m_Move;

		private bool m_Jump;
	}
}
