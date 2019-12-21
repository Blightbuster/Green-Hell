using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanAttack : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Attack = (base.CreateAction(typeof(Attack)) as Attack);
		}

		public override bool ShouldPerform()
		{
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && !enemy.IsDead() && (!this.m_HumanAI.m_CurrentWeapon || !this.m_HumanAI.m_CurrentWeapon.m_Info.IsBow()) && (this.m_Active || enemy.transform.position.Distance(this.m_AI.transform.position) <= this.m_AI.m_Params.m_AttackRange);
		}

		protected override void Prepare()
		{
			base.Prepare();
			Vector3 normalized2D = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_HumanAI.transform.position).GetNormalized2D();
			float num = this.m_HumanAI.transform.forward.GetNormalized2D().AngleSigned(normalized2D, Vector3.up);
			if (Mathf.Abs(num) >= 35f)
			{
				RotateAttack rotateAttack = base.CreateAction(typeof(RotateAttack)) as RotateAttack;
				rotateAttack.SetupParams(num);
				base.StartAction(rotateAttack);
				this.m_Rotate = true;
				return;
			}
			base.StartAction(this.m_Attack);
			this.m_Rotate = false;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.m_AI.m_ID == AI.AIID.Spearman && FPPController.Get().m_Dodge && this.m_CurrentAction == this.m_Attack)
			{
				this.m_AI.m_DamageModule.m_BlockDamage = true;
			}
			this.UpdateRotation();
		}

		private void UpdateRotation()
		{
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * (this.m_Rotate ? 6f : 2f));
		}

		private const float MAX_ANGLE = 35f;

		private bool m_Rotate;

		private Attack m_Attack;
	}
}
