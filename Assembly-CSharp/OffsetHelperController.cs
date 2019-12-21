using System;
using CJTools;
using UnityEngine;

public class OffsetHelperController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.OffsetHelper;
		this.m_OffsetHelper = this.m_Animator.gameObject.transform.FindDeepChild("OffsetHelper");
	}

	public void MoveCharacterController(CharacterControllerProxy char_controller)
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(0);
		foreach (AnimatorClipInfo animatorClipInfo in this.m_Animator.GetCurrentAnimatorClipInfo(0))
		{
			if ((double)animatorClipInfo.weight == 1.0)
			{
				float time = currentAnimatorStateInfo.length * (currentAnimatorStateInfo.normalizedTime % 1f);
				float num = currentAnimatorStateInfo.length * (currentAnimatorStateInfo.normalizedTime % 1f) - Time.deltaTime;
				num = Mathf.Clamp(num, 0f, currentAnimatorStateInfo.length);
				animatorClipInfo.clip.SampleAnimation(this.m_Animator.gameObject, num);
				Vector3 position = this.m_OffsetHelper.position;
				Quaternion quaternion = Quaternion.Inverse(this.m_OffsetHelper.rotation);
				animatorClipInfo.clip.SampleAnimation(this.m_Animator.gameObject, time);
				Vector3 position2 = this.m_OffsetHelper.position;
				Quaternion rotation = this.m_OffsetHelper.rotation;
				quaternion *= rotation;
				Vector3 vector = position2 - position;
				char_controller.Move(vector, true);
				char_controller.transform.rotation *= quaternion;
				DebugRender.DrawLine(position2, position2 + vector, Color.blue, 0f);
			}
		}
	}

	private Transform m_OffsetHelper;
}
