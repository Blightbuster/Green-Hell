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
			this.m_ArrowHolder = this.m_AI.transform.FindDeepChild("RH_holder");
		}

		private bool ForceContinue()
		{
			return this.m_Active && Time.time - this.m_ActivationTime >= 1.2f;
		}

		public override bool ShouldPerform()
		{
			return this.ForceContinue() || (this.m_AI.m_EnemyModule.m_Enemy && Time.time - this.m_AI.m_LastAttackTime >= this.m_HunterAI.m_BowAttackInterval && this.m_HunterAI.IsProperPosToBowAttack());
		}

		protected override void Prepare()
		{
			base.Prepare();
			this.m_Arrow = null;
			this.SetupAction();
		}

		private void SetupAction()
		{
			if (Vector3.Angle((this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).GetNormalized2D(), this.m_AI.transform.forward.GetNormalized2D()) >= 5f)
			{
				this.m_HumanRotateTo.SetupParams(this.m_AI.m_EnemyModule.m_Enemy.transform.position, 5f);
				base.StartAction(this.m_HumanRotateTo);
				return;
			}
			if (this.m_HumanAI.m_WeaponType != HumanAI.WeaponType.Primary)
			{
				base.StartAction(this.m_SwitchWeapon);
				return;
			}
			base.StartAction(this.m_BowAttack);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (this.ForceContinue())
			{
				this.m_HunterAI.m_CheckDistanceToTarget = true;
			}
			if (this.m_Arrow)
			{
				this.m_Arrow.transform.localRotation = Quaternion.identity;
				this.m_Arrow.transform.localPosition = Vector3.zero;
			}
		}

		private void UpdateRotation()
		{
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * 10f);
		}

		public override void OnStopAction(AIAction action)
		{
			base.OnStopAction(action);
			if (action.GetType() == typeof(HumanRotateTo) && this.m_HumanAI.m_WeaponType != HumanAI.WeaponType.Primary)
			{
				base.StartAction(this.m_SwitchWeapon);
				return;
			}
			if (action.GetType() == typeof(SwitchWeapon) || action.GetType() == typeof(HumanRotateTo))
			{
				base.StartAction(this.m_BowAttack);
			}
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (id == AnimEventID.BowGetArrow)
			{
				this.m_Arrow = ItemsManager.Get().CreateItem(ItemID.Tribe_Arrow, true, this.m_AI.transform.position, Quaternion.identity);
				this.m_Arrow.transform.parent = this.m_ArrowHolder;
				this.m_Arrow.transform.localRotation = Quaternion.identity;
				this.m_Arrow.transform.localPosition = Vector3.zero;
				Vector3 localScale = this.m_Arrow.transform.localScale;
				this.m_OrigArrowScale = localScale;
				localScale.x = ((Arrow)this.m_Arrow).m_AimScale;
				this.m_Arrow.transform.localScale = localScale;
				this.m_Arrow.enabled = false;
				return;
			}
			if (id == AnimEventID.BowShot)
			{
				this.Shot();
			}
		}

		private void Shot()
		{
			this.m_Arrow.transform.localScale = this.m_OrigArrowScale;
			this.m_Arrow.enabled = true;
			this.m_Arrow.m_RequestThrow = true;
			this.m_Arrow.m_Thrower = this.m_AI.gameObject;
			this.m_Arrow = null;
			this.m_HunterAI.OnBowShot();
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (this.m_Arrow)
			{
				UnityEngine.Object.Destroy(this.m_Arrow.gameObject);
				this.m_Arrow = null;
			}
		}

		private const float MAX_ANGLE = 5f;

		private SwitchWeapon m_SwitchWeapon;

		private BowAttack m_BowAttack;

		private HumanRotateTo m_HumanRotateTo;

		private Item m_Arrow;

		private Transform m_ArrowHolder;

		private Vector3 m_OrigArrowScale = Vector3.zero;
	}
}
