using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace AIs
{
	public class Die : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			string text = string.Empty;
			for (int i = 0; i < 999; i++)
			{
				text = "Die_" + i.ToString();
				if (!this.m_AI.m_AnimationModule.ContainsState(text))
				{
					break;
				}
				this.m_Anims.Add(text);
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add("Die");
			}
		}

		public override void Start()
		{
			base.Start();
			if (this.m_AI.m_Trap && this.m_AI.m_Trap.m_Info.m_ID == ItemID.Snare_Trap)
			{
				this.m_Animation = "SnareTrap";
				return;
			}
			this.m_Animation = this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)];
		}

		protected override bool ShouldFinish()
		{
			return false;
		}

		private List<string> m_Anims = new List<string>();
	}
}
