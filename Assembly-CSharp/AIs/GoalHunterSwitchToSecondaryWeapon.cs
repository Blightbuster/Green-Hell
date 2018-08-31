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
			return this.m_Active || (Time.time - this.m_LastSwitchWeaponTime >= this.m_MinSwitchWeaponsInterval && (this.m_AI.m_GoalsModule.m_ActiveGoal == null || this.m_AI.m_GoalsModule.m_ActiveGoal.m_Type != AIGoalType.HunterBowAttack) && this.m_HunterAI.m_WeaponType == HumanAI.WeaponType.Primary && this.m_HunterAI.transform.position.Distance(Player.Get().transform.position) <= this.m_HunterAI.m_SecondaryWeaponDist);
		}

		protected override void Prepare()
		{
			base.Prepare();
			Vector3 normalized2D = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
			float num = Vector3.Angle(normalized2D, this.m_AI.transform.forward.GetNormalized2D());
			if (num >= 15f)
			{
				this.m_HumanRotateTo.SetupParams(Player.Get().transform.position, 15f);
				base.StartAction(this.m_HumanRotateTo);
			}
			else
			{
				base.StartAction(this.m_SwitchWeapon);
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanRotateTo))
			{
				base.StartAction(this.m_SwitchWeapon);
			}
			else if (action.GetType() == typeof(SwitchWeapon))
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
