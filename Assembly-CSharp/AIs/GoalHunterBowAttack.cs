using System;
using CJTools;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalHunterBowAttack : GoalHunter
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_SwitchWeapon = (base.CreateAction(typeof(SwitchWeapon)) as SwitchWeapon);
			this.m_BowAttack = (base.CreateAction(typeof(BowAttack)) as BowAttack);
			this.m_HumanRotateTo = (base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo);
		}

		public override bool ShouldPerform()
		{
			if (this.m_Active)
			{
				return true;
			}
			Being enemy = this.m_AI.m_EnemyModule.m_Enemy;
			return enemy && Time.time - this.m_AI.m_LastAttackTime >= this.m_HunterAI.m_BowAttackInterval && this.m_HunterAI.IsProperPosToBowAttack();
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.SetupAction();
		}

		private void SetupAction()
		{
			Vector3 normalized2D = (Player.Get().transform.position - this.m_AI.transform.position).GetNormalized2D();
			float num = Vector3.Angle(normalized2D, this.m_AI.transform.forward.GetNormalized2D());
			if (num >= 5f)
			{
				this.m_HumanRotateTo.SetupParams(Player.Get().transform.position, 5f);
				base.StartAction(this.m_HumanRotateTo);
			}
			else if (this.m_HumanAI.m_WeaponType != HumanAI.WeaponType.Primary)
			{
				base.StartAction(this.m_SwitchWeapon);
			}
			else
			{
				base.StartAction(this.m_BowAttack);
			}
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanRotateTo) && this.m_HumanAI.m_WeaponType != HumanAI.WeaponType.Primary)
			{
				base.StartAction(this.m_SwitchWeapon);
			}
			else if (action.GetType() == typeof(SwitchWeapon) || action.GetType() == typeof(HumanRotateTo))
			{
				base.StartAction(this.m_BowAttack);
			}
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (id == AnimEventID.BowShot)
			{
				this.Shot();
			}
		}

		private void Shot()
		{
			Item item = ItemsManager.Get().CreateItem(ItemID.Arrow, true, this.m_AI.transform.position, Quaternion.identity);
			Vector3 vector = Player.Get().transform.position - this.m_AI.transform.position;
			Vector3 upwards = Vector3.Cross(-this.m_AI.transform.right, vector.normalized);
			item.transform.rotation = Quaternion.LookRotation(-this.m_AI.transform.right, upwards);
			Transform transform = this.m_AI.transform.FindDeepChild("LH_holder");
			item.transform.position = transform.position + this.m_AI.transform.forward;
			item.m_RequestThrow = true;
			item.m_Thrower = this.m_AI;
			this.m_HunterAI.OnBowShot();
		}

		private const float MAX_ANGLE = 5f;

		private SwitchWeapon m_SwitchWeapon;

		private BowAttack m_BowAttack;

		private HumanRotateTo m_HumanRotateTo;
	}
}
