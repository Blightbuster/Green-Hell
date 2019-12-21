using System;
using UnityEngine;

public class AnimatorSpeedCurve : MonoBehaviour
{
	private void Update()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this.m_Animator.GetCurrentAnimatorStateInfo(this.m_Layer);
		float time = currentAnimatorStateInfo.normalizedTime - Mathf.Floor(currentAnimatorStateInfo.normalizedTime);
		float speed = this.m_Curve.Evaluate(time);
		this.m_Animator.speed = speed;
		this.m_Speed = speed;
	}

	public Animator m_Animator;

	public AnimationCurve m_Curve = new AnimationCurve();

	public int m_Layer;

	public float m_Speed = 1f;
}
