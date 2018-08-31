using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalStrafe : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Strafe = (base.CreateAction(typeof(Strafe)) as Strafe);
		}

		public override bool ShouldPerform()
		{
			float num = this.m_AI.transform.position.Distance(Player.Get().transform.position);
			if (num > this.m_AI.m_Params.m_AttackRange * 2f)
			{
				return false;
			}
			if (this.m_Active)
			{
				return base.GetDuration() < this.m_Length;
			}
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && this.m_AI.m_GoalsModule.m_PrevGoal != null && this.m_AI.m_GoalsModule.m_PrevGoal.m_Type == AIGoalType.JumpBack;
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Strafe.SetupParams(float.MaxValue, (UnityEngine.Random.Range(0, 2) != 0) ? Direction.Right : Direction.Left);
			base.AddToPlan(this.m_Strafe);
			this.m_Length = UnityEngine.Random.Range(this.m_MinDuration, this.m_MaxDuration);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * 5f);
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return true;
		}

		private Strafe m_Strafe;

		private float m_MinDuration = 2f;

		private float m_MaxDuration = 4f;

		private float m_Length;
	}
}
