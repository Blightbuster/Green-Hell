using System;
using UnityEngine;

namespace AIs
{
	public class SightModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
			this.m_Collider = this.m_AI.GetComponent<Collider>();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			bool enabled = this.m_Collider.enabled;
			this.m_Collider.enabled = false;
			this.UpdatePlayerVisible();
			this.m_Collider.enabled = enabled;
			this.m_LastCheckTime = Time.time;
		}

		private void UpdatePlayerVisible()
		{
			this.m_PlayerVisible = false;
			float maxValue = float.MaxValue;
			float num = Vector3.Distance(Player.Get().transform.position, this.m_AI.transform.position);
			if (num > this.m_AI.m_Params.m_SightRange)
			{
				return;
			}
			if (maxValue < num)
			{
				return;
			}
			if (!this.IsInSightCone(Player.Get().gameObject))
			{
				return;
			}
			RaycastHit raycastHit;
			if (Physics.Linecast(this.m_AI.GetHeadTransform().position, Player.Get().GetHeadTransform().position, out raycastHit) && raycastHit.transform == Player.Get().transform)
			{
				this.m_PlayerVisible = true;
			}
		}

		private bool IsInSightCone(GameObject obj)
		{
			Vector3 to = -this.m_AI.GetHeadTransform().up.GetNormalized2D();
			Vector3 normalized = (obj.transform.position - this.m_AI.GetHeadTransform().position).normalized;
			float num = Vector3.Angle(normalized, to);
			return num <= this.m_AI.m_Params.m_SightAngle;
		}

		public bool m_PlayerVisible;

		private float m_LastCheckTime;

		private Collider m_Collider;
	}
}
