using System;
using Enums;
using UnityEngine;

namespace AIs
{
	public class GoalPunchBack : AIGoal
	{
		public override void Initialize(AI ai)
		{
			base.Initialize(ai);
			this.m_PunchBack = (base.CreateAction(typeof(PunchBack)) as PunchBack);
		}

		protected override void Prepare()
		{
			base.Prepare();
			base.StartAction(this.m_PunchBack);
			Direction direction;
			if (FistFightController.Get().IsActive())
			{
				direction = (FistFightController.Get().IsLeftPunch() ? Direction.Right : Direction.Left);
			}
			else if (WeaponMeleeController.Get().IsActive())
			{
				if (WeaponMeleeController.Get().IsRightAttack())
				{
					direction = Direction.Left;
				}
				else if (WeaponMeleeController.Get().IsLeftAttack())
				{
					direction = Direction.Right;
				}
				else
				{
					direction = ((UnityEngine.Random.Range(0f, 1f) < 0.5f) ? Direction.Right : Direction.Left);
				}
			}
			else
			{
				direction = ((UnityEngine.Random.Range(0f, 1f) < 0.5f) ? Direction.Right : Direction.Left);
			}
			this.m_PunchBack.SetDirection(direction);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			this.UpdateRotation();
		}

		private void UpdateRotation()
		{
			Vector3 normalized = (this.m_AI.m_EnemyModule.m_Enemy.transform.position - this.m_AI.transform.position).normalized;
			this.m_AI.transform.rotation = Quaternion.Slerp(this.m_AI.transform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * 2f);
		}

		public const float MAX_ANGLE = 5f;

		private PunchBack m_PunchBack;
	}
}
