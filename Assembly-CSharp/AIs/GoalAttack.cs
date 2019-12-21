using System;
using UnityEngine;

namespace AIs
{
	public class GoalAttack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Attack = (base.CreateAction(typeof(Attack)) as Attack);
			this.m_RotateTo = (base.CreateAction(typeof(RotateTo)) as RotateTo);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && !enemy.IsDead() && (this.m_Active || enemy.transform.position.Distance(this.m_AI.transform.position) <= this.m_AI.m_Params.m_AttackRange);
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.IsCat())
			{
				Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D();
				Vector3 normalized2D2 = this.m_AI.transform.forward.GetNormalized2D();
				if (Vector3.Angle(normalized2D, normalized2D2) > 75f)
				{
					this.m_RotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.gameObject, true);
					base.StartAction(this.m_RotateTo);
				}
			}
			base.AddToPlan(this.m_Attack);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdatePosition();
			this.UpdateRotation();
		}

		private void UpdatePosition()
		{
			if (this.m_AI.IsCat())
			{
				Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
				Vector3 position = this.m_AI.transform.position + normalized * Time.deltaTime * 0.5f;
				this.m_AI.transform.position = position;
				return;
			}
			if (this.m_AI.m_Params.m_Human || this.m_AI.m_Params.m_BigAnimal)
			{
				return;
			}
			if (this.m_AI.m_EnemyModule.m_Enemy.transform.position.Distance(this.m_AI.transform.position) > this.m_AI.m_Params.m_AttackRange)
			{
				Vector3 normalized2 = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
				Vector3 position2 = this.m_AI.transform.position + normalized2 * Time.deltaTime * 5f;
				this.m_AI.transform.position = position2;
			}
		}

		private void UpdateRotation()
		{
			if (this.m_AI.m_Params.m_Human || this.m_AI.m_Params.m_BigAnimal)
			{
				return;
			}
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * 10f);
		}

		public override bool ShouldLookAtPlayer()
		{
			return true;
		}

		public override bool ShouldRotateToPlayer()
		{
			return false;
		}

		private Attack m_Attack;

		private RotateTo m_RotateTo;
	}
}
