using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalAttackFoot : GoalMove
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_MoveTo = (base.CreateAction(typeof(MoveTo)) as MoveTo);
			this.m_Attack = (base.CreateAction(typeof(Attack)) as Attack);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			if (Time.time - this.m_LastAttackTime < this.m_AttackInterval)
			{
				return false;
			}
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			if (!enemy)
			{
				return false;
			}
			float num = enemy.m_LFoot.position.Distance(this.m_AI.transform.position);
			float num2 = enemy.m_RFoot.position.Distance(this.m_AI.transform.position);
			float attackRange = this.m_AI.m_Params.m_AttackRange;
			return num <= attackRange || num2 <= attackRange;
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (this.m_AI.m_ID == AI.AIID.GoliathBirdEater)
			{
				this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveToEnemy);
				base.AddToPlan(this.m_Attack);
			}
			else
			{
				base.StartAction(this.m_Attack);
			}
			float num = this.m_AI.m_EnemyModule.m_Enemy.m_LFoot.position.Distance(this.m_AI.transform.position);
			float num2 = this.m_AI.m_EnemyModule.m_Enemy.m_RFoot.position.Distance(this.m_AI.transform.position);
			if (num < num2)
			{
				this.m_Target = this.m_AI.m_EnemyModule.m_Enemy.m_LFoot.gameObject;
			}
			else
			{
				this.m_Target = this.m_AI.m_EnemyModule.m_Enemy.m_RFoot.gameObject;
			}
			if (this.m_AI.m_ID == AI.AIID.GoliathBirdEater)
			{
				this.m_AI.m_AnimationModule.SetForcedSpeed(10f);
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateRotation();
		}

		private void UpdatePosition()
		{
			if (AI.IsSnake(this.m_AI.m_ID))
			{
				return;
			}
			if (this.m_Target.transform.position.Distance(this.m_AI.transform.position) > this.m_AI.m_Radius)
			{
				Vector3 normalized = (this.m_Target.transform.position - this.m_AI.transform.position).normalized;
				Vector3 position = this.m_AI.transform.position + normalized * Time.deltaTime;
				this.m_AI.transform.position = position;
			}
		}

		private void UpdateRotation()
		{
			if (!AI.IsSnake(this.m_AI.m_ID))
			{
				return;
			}
			Vector3 normalized = (this.m_Target.transform.position - this.m_AI.transform.position).normalized;
			Vector3 upwards = this.m_AI.m_EnemyModule.m_Enemy ? this.m_AI.m_EnemyModule.m_Enemy.transform.up : Vector3.up;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized, upwards), Time.deltaTime / 0.1f);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this.m_LastAttackTime = Time.time;
			this.m_AttackInterval = UnityEngine.Random.Range(1.5f, 3f);
			this.m_AI.m_AnimationModule.ResetForcedSpeed();
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			if (AI.IsSnake(this.m_AI.m_ID))
			{
				return AIMoveStyle.None;
			}
			return base.GetWantedMoveStyle();
		}

		private float m_LastAttackTime;

		private float m_AttackInterval = 2f;

		private GameObject m_Target;

		private Attack m_Attack;

		private MoveTo m_MoveTo;
	}
}
