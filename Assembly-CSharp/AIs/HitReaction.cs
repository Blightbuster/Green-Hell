using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class HitReaction : AIAction
	{
		public override void Initialize(AI ai, AIGoal goal)
		{
			base.Initialize(ai, goal);
			string text = string.Empty;
			for (int i = 0; i < 999; i++)
			{
				text = "HitReaction_" + i.ToString();
				if (!this.m_AI.m_AnimationModule.ContainsState(text))
				{
					break;
				}
				this.m_Anims.Add(text);
			}
			if (this.m_Anims.Count == 0)
			{
				this.m_Anims.Add("HitReaction");
			}
		}

		public override void Start()
		{
			base.Start();
			string str = (this.m_Type != HitReaction.Type.None) ? this.m_Type.ToString() : string.Empty;
			this.m_Animation = str + this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)];
		}

		public void SetType(HitReaction.Type type)
		{
			this.m_Type = type;
			string str = (this.m_Type != HitReaction.Type.None) ? this.m_Type.ToString() : string.Empty;
			this.m_Animation = str + this.m_Anims[UnityEngine.Random.Range(0, this.m_Anims.Count)];
		}

		protected override bool ShouldFinish()
		{
			return base.IsAnimFinishing();
		}

		protected override void SetupTransitionDuration()
		{
			this.m_AI.m_AnimationModule.m_TransitionDuration = 0.1f;
		}

		private List<string> m_Anims = new List<string>();

		private HitReaction.Type m_Type = HitReaction.Type.None;

		public enum Type
		{
			None = -1,
			Right,
			Left,
			Middle,
			StepBack
		}
	}
}
