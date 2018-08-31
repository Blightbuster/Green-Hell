using System;
using UnityEngine;

namespace AIs
{
	public class MoraleModule : AIModule
	{
		public override void Initialize()
		{
			base.Initialize();
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateAutoRestore();
		}

		private void UpdateAutoRestore()
		{
			if (this.m_Morale < 1f && !this.m_AI.m_EnemyModule.m_Enemy)
			{
				this.m_Morale += MainLevel.s_GameTimeDelta * this.m_AI.m_Params.m_MoraleRegeneration;
				this.m_Morale = Mathf.Clamp01(this.m_Morale);
			}
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			this.m_Morale -= this.m_AI.m_Params.m_MoraleDamageDecrease;
			this.m_Morale = Mathf.Clamp01(this.m_Morale);
		}

		public float m_Morale = 1f;
	}
}
