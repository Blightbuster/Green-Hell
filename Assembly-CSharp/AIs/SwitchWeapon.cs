using System;
using Enums;

namespace AIs
{
	public class SwitchWeapon : AIAction
	{
		public override void Start()
		{
			base.Start();
			HumanAI humanAI = (HumanAI)this.m_AI;
			this.m_Animation = "Switch";
			if (humanAI.m_CurrentWeapon.m_Info.IsBow())
			{
				this.m_Animation += "Bow";
			}
			else if (humanAI.m_CurrentWeapon.m_Info.IsKnife())
			{
				this.m_Animation += "Knife";
			}
			else
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
			this.m_Animation += "To";
			Item item = (!(humanAI.m_CurrentWeapon == humanAI.m_PrimaryWeapon)) ? humanAI.m_PrimaryWeapon : humanAI.m_SecondaryWeapon;
			if (item.m_Info.IsBow())
			{
				this.m_Animation += "Bow";
			}
			else if (item.m_Info.IsKnife())
			{
				this.m_Animation += "Knife";
			}
			else
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
			}
		}

		public override void OnAnimEvent(AnimEventID id)
		{
			base.OnAnimEvent(id);
			if (id == AnimEventID.SwitchWeapon)
			{
				this.m_AI.SwitchWeapon();
			}
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}
	}
}
