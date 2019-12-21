using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalFlank : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Strafe = (base.CreateAction(typeof(Strafe)) as Strafe);
		}

		public override bool ShouldPerform()
		{
			if (base.GetPlanCount() > 0)
			{
				return true;
			}
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && (this.m_LastFlankTime <= 0f || Time.time - this.m_LastFlankTime >= 15f) && enemy.transform.position.Distance(this.m_AI.transform.position) >= 7f;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Strafe.SetupParams(UnityEngine.Random.Range(4f, 6f), (UnityEngine.Random.Range(0, 2) == 0) ? Direction.Left : Direction.Right);
			base.AddToPlan(this.m_Strafe);
			this.m_AI.m_PathModule.CalcPath(PathModule.PathType.Flank);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * 5f);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this.m_LastFlankTime = Time.time;
		}

		private Strafe m_Strafe;

		private float m_LastFlankTime;
	}
}
