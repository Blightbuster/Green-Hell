using System;
using UnityEngine;

public class RainCone : MonoBehaviour
{
	private void LateUpdate()
	{
		if (this.m_Camera == null)
		{
			this.m_Camera = Camera.main;
		}
		if (!this.m_Camera)
		{
			return;
		}
		Vector3 position = this.m_Camera.transform.position;
		position.y = position.y;
		base.transform.position = position;
	}

	private Camera m_Camera;

	private const float m_YOffset = 0f;
}
