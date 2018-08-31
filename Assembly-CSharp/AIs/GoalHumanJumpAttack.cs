using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanJumpAttack : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_JumpAttack = (base.CreateAction(typeof(JumpAttack)) as JumpAttack);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			if (!enemy)
			{
				return false;
			}
			if (enemy.IsDead())
			{
				return false;
			}
			if (this.m_HumanAI.m_CurrentWeapon && (this.m_HumanAI.m_CurrentWeapon.m_Info.IsBow() || this.m_HumanAI.m_CurrentWeapon.m_Info.IsSpear()))
			{
				return false;
			}
			if (this.m_Active)
			{
				return true;
			}
			Vector3 normalized2D = (Player.Get().transform.position - this.m_HumanAI.transform.position).GetNormalized2D();
			float num = enemy.transform.position.Distance(this.m_AI.transform.position);
			return num > this.m_AI.m_Params.m_AttackRange && num < this.m_AI.m_Params.m_JumpAttackRange;
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.StartAction(this.m_JumpAttack);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateRotation();
		}

		private void UpdateRotation()
		{
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * 2f);
		}

		private const float MAX_ANGLE = 15f;

		private JumpAttack m_JumpAttack;
	}
}
