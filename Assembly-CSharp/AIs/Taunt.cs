using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class Taunt : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			string str = (!this.m_AI.IsHunter()) ? "Taunt_" : "HunterTaunt_";
			for (int i = 0; i < 999; i++)
			{
				if (!this.m_AI.m_AnimationModule.ContainsState(str + i.ToString()))
				{
					break;
				}
				this.m_Anims.Add(str + i.ToString());
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add((!this.m_AI.IsHunter()) ? "Taunt" : "HunterTaunt");
			}
		}

		public override void Start()
		{
			base.Start();
			this.m_Animation = this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)];
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}

		private List<string> m_Anims = new List<string>();
	}
}
