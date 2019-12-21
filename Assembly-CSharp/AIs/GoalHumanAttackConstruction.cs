using System;
using UnityEngine;

namespace AIs
{
	public class GoalHumanAttackConstruction : GoalHuman
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_Attack = (base.CreateAction(typeof(Attack)) as Attack);
			this.m_RotateTo = (base.CreateAction(typeof(HumanRotateTo)) as HumanRotateTo);
		}

		public override bool ShouldPerform()
		{
			return this.m_HumanAI.m_SelectedConstruction && this.m_HumanAI.m_SelectedConstruction.transform.position.Distance(this.m_AI.transform.position) <= this.m_AI.m_Params.m_AttackRange + 0.5f;
		}

		protected override void Prepare()
		{
			base.Prepare();
			Vector3 normalized2D = (this.m_HumanAI.m_SelectedConstruction.transform.position - this.m_HumanAI.transform.position).GetNormalized2D();
			if (Mathf.Abs(this.m_HumanAI.transform.forward.GetNormalized2D().AngleSigned(normalized2D, Vector3.up)) >= 35f)
			{
				this.m_RotateTo.SetupParams(this.m_HumanAI.m_SelectedConstruction.gameObject.transform.position, 10f);
				base.StartAction(this.m_RotateTo);
			}
			base.AddToPlan(this.m_Attack);
		}

		public override bool ShouldLookAtPlayer()
		{
			return false;
		}

		public override bool ShouldRotateToPlayer()
		{
			return false;
		}

		private Attack m_Attack;

		private HumanRotateTo m_RotateTo;

		private const float MAX_ANGLE = 35f;
	}
}
