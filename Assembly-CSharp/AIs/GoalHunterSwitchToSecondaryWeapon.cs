using System;
using UnityEngine;

namespace AIs
{
	public class GoalHunterSwitchToSecondaryWeapon : GoalHunter
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_SwitchWeapon = (base.CreateAction(typeof(SwitchWeapon)) as SwitchWeapon);
			this.m_HumanRotateTo = (base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo);
		}

		public override bool ShouldPerform()
		{
			return this.m_Active || (Time.time - this.m_LastSwitchWeaponTime >= this.m_MinSwitchWeaponsInterval && (this.m_AI.m_GoalsModule.m_ActiveGoal == null || this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type != AIGoalType.HunterBowAttack) && this.m_HunterAI.m_WeaponType == HumanAI.WeaponType.Primary && !(this.m_AI.m_EnemyModule.m_Enemy == null) && this.m_HunterAI.transform.position.Distance(this.m_AI.m_EnemyModule.m_Enemy.transform.position) <= this.m_HunterAI.m_SecondaryWeaponDist);
		}

		protected override void Prepare()
		{
			base.Prepare();
			if (Vector3.Angle((this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 15f)
			{
				this.m_HumanRotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.transform.position, 15f);
				base.StartAction(this.m_HumanRotateTo);
				return;
			}
			base.StartAction(this.m_SwitchWeapon);
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanRotateTo))
			{
				base.StartAction(this.m_SwitchWeapon);
				return;
			}
			if (action.GetType() == typeof(SwitchWeapon))
			{
				this.m_AI.m_GoalsModule.ActivateGoal(AIGoalType.HumanMoveToEnemy);
			}
		}

		private float m_LastSwitchWeaponTime;

		private float m_MinSwitchWeaponsInterval = 2f;

		private const float MAX_ANGLE = 15f;

		private SwitchWeapon m_SwitchWeapon;

		private HumanRotateTo m_HumanRotateTo;
	}
}
