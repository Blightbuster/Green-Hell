using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class Idle : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			string text = string.Empty;
			for (int i = 0; i < 999; i++)
			{
				text = "Idle_" + i.ToString();
				if (!this.m_AI.m_AnimationModule.ContainsState(text))
				{
					break;
				}
				this.m_Anims.Add(text);
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add("Idle");
			}
		}

		public override void Start()
		{
			base.Start();
			this.m_Animation = ((this.m_ForceVersion < 0) ? this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)] : this.m_Anims[this.m_ForceVersion]);
		}

		public void SetupParams(float length)
		{
			this.m_Length = length;
		}

		protected override bool ShouldFinish()
		{
			return base.GetDuration() >= this.m_Length;
		}

		private float m_Length;

		private List<string> m_Anims = new List<string>();

		public int m_ForceVersion = -1;
	}
}
