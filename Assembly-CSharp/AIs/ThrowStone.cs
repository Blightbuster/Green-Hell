using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class ThrowStone : AIAction
	{
		public override void Start()
		{
			base.Start();
			this.m_Animation = "ThrowStone";
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (id == AnimEventID.ThrowStone)
			{
				this.Throw();
			}
		}

		private void Throw()
		{
			Item item = ItemsManager.Get().CreateItem(ItemID.Stone, true, this.m_AI.transform.position, Quaternion.identity);
			Vector3 vector = this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position;
			Vector3 upwards = Vector3.Cross(-this.m_AI.transform.right, vector.normalized);
			item.transform.rotation = Quaternion.LookRotation(-this.m_AI.transform.right, upwards);
			item.transform.position = this.m_AI.GetHeadTransform().position + this.m_AI.transform.forward;
			item.m_RequestThrow = true;
			item.m_Thrower = this.m_AI.gameObject;
		}
	}
}
