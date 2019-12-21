﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class SwimAttack : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			string text = string.Empty;
			for (int i = 0; i < 999; i++)
			{
				text = "SwimAttack_" + i;
				if (!this.m_AI.m_AnimationModule.ContainsState(text))
				{
					break;
				}
				this.m_Anims.Add(text);
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add("SwimAttack");
			}
		}

		public override void Start()
		{
			base.Start();
			this.m_Animation = this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)];
			if (this.m_AI.m_VisModule)
			{
				this.m_AI.m_VisModule.OnStartAttack();
			}
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}

		protected override void Stop()
		{
			base.Stop();
			this.m_AI.m_LastAttackTime = Time.time;
		}

		private List<string> m_Anims = new List<string>();
	}
}
