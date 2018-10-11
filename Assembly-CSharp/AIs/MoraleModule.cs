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
			this.UpdateFirecamp();
		}

		private void UpdateAutoRestore()
		{
			if (this.m_Morale < 1f && !this.m_AI.m_EnemyModule.m_Enemy)
			{
				this.m_Morale += MainLevel.s_GameTimeDelta * this.m_AI.m_Params.m_MoraleRegeneration;
				this.m_Morale = Mathf.Clamp01(this.m_Morale);
			}
		}

		private void UpdateFirecamp()
		{
			if (!this.m_AI.IsCat())
			{
				return;
			}
			if (this.m_Morale <= 0f)
			{
				return;
			}
			foreach (Firecamp firecamp in Firecamp.s_Firecamps)
			{
				if (firecamp.m_Burning)
				{
					if (base.transform.position.Distance(firecamp.transform.position) < 4f)
					{
						this.m_Morale = 0f;
						break;
					}
				}
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
