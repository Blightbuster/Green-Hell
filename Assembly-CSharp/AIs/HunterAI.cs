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
			if (this.m_CheckDistanceToTarget)
			{
				float num = base.transform.position.Distance(Player.Get().transform.position);
				if (num < this.m_MinBowDistance || num > this.m_MaxBowDistance)
				{
					return false;
				}
			}
			Vector3 normalized = (Player.Get().GetHeadTransform().position - this.GetHeadTransform().position).normalized;
			Debug.DrawLine(this.GetHeadTransform().position + normalized, Player.Get().GetHeadTransform().position - normalized, Color.blue);
			RaycastHit raycastHit;
			return Physics.Raycast(this.GetHeadTransform().position + normalized, normalized, out raycastHit) && (raycastHit.collider.gameObject.IsPlayer() || raycastHit.collider.gameObject == Camera.main.gameObject);
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
			if (UnityEngine.Random.Range(0f, 1f) < this.m_HitArrowTauntProbability)
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

		[HideInInspector]
		public bool m_CheckDistanceToTarget = true;
	}
}
