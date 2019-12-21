using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class PostAttack : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			string text = "PostAttack_";
			if (this.m_AI.IsHunter())
			{
				text = "Hunter" + text;
			}
			int num = 0;
			while (num < 999 && this.m_AI.m_AnimationModule.ContainsState(text + num.ToString()))
			{
				this.m_Anims.Add(text + num.ToString());
				num++;
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add(this.m_AI.IsHunter() ? "HunterPostAttack" : "PostAttack");
			}
			this.m_Animation = this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)];
		}

		private List<string> m_Anims = new List<string>();
	}
}
