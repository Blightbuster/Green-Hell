using System;
using UnityEngine;

namespace AIs
{
	public class HunterAI : HumanAI
	{
		protected override void Awake()
		{
			base.Awake();
			this.SetupBowAttackInterval();
		}

		public override bool IsHunter()
		{
			return true;
		}

		public bool IsProperPosToBowAttack()
		{
			float num = base.transform.position.Distance(Player.Get().transform.position);
			return num >= this.m_MinBowDistance && num <= this.m_MaxBowDistance;
		}

		public void OnBowShot()
		{
			this.SetupBowAttackInterval();
		}

		private void SetupBowAttackInterval()
		{
			this.m_BowAttackInterval = UnityEngine.Random.Range(this.m_MinBowAttackInterval, this.m_MaxBowAttackInterval);
		}

		public void OnHitPlayerByArrow()
		{
			float num = UnityEngine.Random.Range(0f, 1f);
			if (num < this.m_HitArrowTauntProbability)
			{
				this.m_GoalsModule.ActivateGoal(AIGoalType.HumanTaunt);
			}
		}

		[HideInInspector]
		public float m_MinBowDistance = 10f;

		[HideInInspector]
		public float m_MaxBowDistance = 15f;

		[HideInInspector]
		public float m_SecondaryWeaponDist = 5f;

		[HideInInspector]
		public float m_BowAttackInterval;

		private float m_MinBowAttackInterval = 1.5f;

		private float m_MaxBowAttackInterval = 3f;

		private float m_HitArrowTauntProbability = 0.2f;
	}
}
