using System;
using UnityEngine;

namespace Pathfinding.Util
{
	public static class MovementUtilities
	{
		public static Vector2 ClampVelocity(Vector2 velocity, float maxSpeed, float slowdownFactor, bool slowWhenNotFacingTarget, Vector2 forward)
		{
			float num = maxSpeed * Mathf.Sqrt(Mathf.Min(1f, slowdownFactor));
			if (slowWhenNotFacingTarget && (forward.x != 0f || forward.y != 0f))
			{
				float num2;
				Vector2 vector = VectorMath.Normalize(velocity, out num2);
				float num3 = Vector2.Dot(vector, forward);
				float num4 = Mathf.Clamp(num3 + 0.707f, 0.2f, 1f);
				num *= num4;
				num2 = Mathf.Min(num2, num);
				float f = Mathf.Min(Mathf.Acos(Mathf.Clamp(num3, -1f, 1f)), (20f + 180f * Mathf.Clamp01(1f - slowdownFactor)) * 0.0174532924f);
				float num5 = Mathf.Sin(f);
				float num6 = Mathf.Cos(f);
				num5 *= Mathf.Sign(vector.x * forward.y - vector.y * forward.x);
				return new Vector2(forward.x * num6 + forward.y * num5, forward.y * num6 - forward.x * num5) * num2;
			}
			return Vector2.ClampMagnitude(velocity, num);
		}

		public static Vector2 CalculateAccelerationToReachPoint(Vector2 deltaPosition, Vector2 targetVelocity, Vector2 currentVelocity, float acceleration, float maxSpeed)
		{
			if (targetVelocity == Vector2.zero)
			{
				float num = 0.01f;
				float num2 = 10f;
				float num3 = acceleration * acceleration;
				while (num2 - num > 0.01f)
				{
					float num4 = (num2 + num) * 0.5f;
					Vector2 a = (6f * deltaPosition - 4f * num4 * currentVelocity) / (num4 * num4);
					Vector2 a2 = 6f * (num4 * currentVelocity - 2f * deltaPosition) / (num4 * num4 * num4);
					if (a.sqrMagnitude > num3 || (a + a2 * num4).sqrMagnitude > num3)
					{
						num = num4;
					}
					else
					{
						num2 = num4;
					}
				}
				Vector2 vector = (6f * deltaPosition - 4f * num2 * currentVelocity) / (num2 * num2);
				Vector2 vector2 = new Vector2(currentVelocity.y, -currentVelocity.x);
				return Vector3.ClampMagnitude(vector + 1f * Vector3.Dot(vector, vector2) / Mathf.Max(0.0001f, vector2.sqrMagnitude) * vector2, acceleration);
			}
			float magnitude = currentVelocity.magnitude;
			float num5;
			Vector2 a3 = VectorMath.Normalize(targetVelocity, out num5);
			float magnitude2 = deltaPosition.magnitude;
			return Vector2.ClampMagnitude(((deltaPosition - a3 * Math.Min(0.5f * magnitude2 * num5 / (magnitude + num5), maxSpeed * 1.5f)).normalized * maxSpeed - currentVelocity) * 10f, acceleration);
		}
	}
}
