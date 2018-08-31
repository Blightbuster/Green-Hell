using System;
using AIs;
using UnityEngine;

public class VisibilityReceiver : MonoBehaviour
{
	private void OnBecameInvisible()
	{
		if (this.m_AI)
		{
			this.m_AI.m_Visible = false;
		}
	}

	private void OnBecameVisible()
	{
		if (this.m_AI)
		{
			this.m_AI.m_Visible = true;
		}
	}

	public AI m_AI;
}
