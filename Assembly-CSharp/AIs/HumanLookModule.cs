using System;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class HumanLookModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_HumanAI = (HumanAI)this.m_AI;
			DebugUtils.Assert(this.m_HumanAI, true);
			this.m_Head = base.transform.FindDeepChild("head");
			DebugUtils.Assert(this.m_Head, true);
		}

		public override void OnLateUpdate()
		{
			base.OnLateUpdate();
			if (this.ShouldLookAtPlayer())
			{
				this.UpdateLookAtPlayer();
			}
		}

		private bool ShouldLookAtPlayer()
		{
			return true;
		}

		private void UpdateLookAtPlayer()
		{
			Quaternion rotation = Quaternion.LookRotation((CameraManager.Get().m_MainCamera.transform.position - this.m_Head.position).normalized, Vector2.up);
			this.m_Head.rotation = rotation;
			this.m_Head.Rotate(this.m_Euler);
		}

		private HumanAI m_HumanAI;

		private Transform m_Head;

		public Vector3 m_Euler = Vector3.zero;
	}
}
