using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalEat : GoalMove
	{
		public override bool ShouldPerform()
		{
			return false;
		}

		protected override void Prepare()
		{
			base.Prepare();
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			return AIMoveStyle.Walk;
		}

		protected override void OnDeactivate()
		{
			if (this.m_SelectedFood)
			{
				if (this.m_Eat != null && this.m_Eat.IsFinished())
				{
					UnityEngine.Object.Destroy(this.m_SelectedFood.gameObject);
				}
				else
				{
					Physics.IgnoreCollision(this.m_SelectedFood.m_Collider, this.m_AI.GetComponent<Collider>(), false);
				}
			}
			base.OnDeactivate();
		}

		private Eat m_Eat;

		private Food m_SelectedFood;
	}
}
