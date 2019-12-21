using System;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class GoalSwim : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Swim = (base.CreateAction(typeof(SwimTo)) as SwimTo);
			this.waterMask = 1 << NavMesh.GetAreaFromName("Water");
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			if (!this.m_AI.m_Params.m_CanSwim)
			{
				return false;
			}
			bool flag = Time.time - this.m_AI.m_LastAttackTime > 2.5f;
			return this.m_AI.IsSwimming() && this.m_AI.m_PathModule.CalcPath((flag && this.m_AI.m_EnemyModule.m_Enemy) ? PathModule.PathType.AnimalMoveToEnemy : PathModule.PathType.Loiter);
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.AddToPlan(this.m_Swim);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
		}

		private SwimTo m_Swim;

		private int waterMask;
	}
}
