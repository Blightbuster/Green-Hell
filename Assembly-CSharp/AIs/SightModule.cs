using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class SightModule : AIModule
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_Collider = this.m_AI.GetComponent<Collider>();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdatePlayerVisible();
		}

		private void UpdatePlayerVisible()
		{
			this.m_VisiblePlayers.Clear();
			for (int i = 0; i < ReplicatedLogicalPlayer.s_AllLogicalPlayers.Count; i++)
			{
				Being component = ReplicatedLogicalPlayer.s_AllLogicalPlayers[i].GetComponent<Being>();
				if (this.IsBeingVisible(component))
				{
					this.m_VisiblePlayers.Add(component);
				}
			}
		}

		private bool IsBeingVisible(Being being)
		{
			RaycastHit raycastHit;
			return Vector3.Distance(being.transform.position, this.m_AI.transform.position) <= this.m_AI.m_Params.m_SightRange && this.IsInSightCone(being.gameObject) && ((Physics.Linecast(this.m_AI.GetHeadTransform().position, being.GetHeadTransform().position, out raycastHit) && raycastHit.transform == being.transform) || (Camera.main && raycastHit.transform == Camera.main.transform));
		}

		private bool IsInSightCone(GameObject obj)
		{
			Vector3 to = -this.m_AI.GetHeadTransform().up.GetNormalized2D();
			return Vector3.Angle((obj.transform.position - this.m_AI.GetHeadTransform().position).normalized, to) <= this.m_AI.m_Params.m_SightAngle;
		}

		public List<Being> m_VisiblePlayers = new List<Being>();

		private Collider m_Collider;
	}
}
