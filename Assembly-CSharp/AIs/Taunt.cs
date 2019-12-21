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
			string str = this.m_AI.IsHunter() ? "HunterTaunt_" : "Taunt_";
			int num = 0;
			while (num < 999 && this.m_AI.m_AnimationModule.ContainsState(str + num.ToString()))
			{
				this.m_Anims.Add(str + num.ToString());
				num++;
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add(this.m_AI.IsHunter() ? "HunterTaunt" : "Taunt");
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
