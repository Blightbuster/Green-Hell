using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalMoveToAttractor : GoalMove
	{
		public override bool ShouldPerform()
		{
			return this.m_AI.m_Attractor && this.m_AI.m_PathModule.CalcPath(PathModule.PathType.MoveToAttractor);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			if (GreenHellGame.TWITCH_DEMO)
			{
				if (this.m_AI.m_Attractor.name == "Attr_0")
				{
					this.m_AI.m_Attractor = JaguarTwitchDemo.s_Object.transform.Find("Attr_1").GetComponent<AIAttractor>();
					return;
				}
				UnityEngine.Object.Destroy(this.m_AI.gameObject);
			}
		}

		public override AIMoveStyle GetWantedMoveStyle()
		{
			if (!this.m_AI.m_Attractor)
			{
				return AIMoveStyle.Run;
			}
			return this.m_AI.m_Attractor.m_MoveStyle;
		}
	}
}
