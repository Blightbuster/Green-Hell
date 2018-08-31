using System;
using UnityEngine;

namespace Cinemachine.Utility
{
	public static class Damper
	{
		private static float DecayConstant(float time, float residual)
		{
			return Mathf.Log(1f / residual) / time;
		}

		private static float Decay(float initial, float decayConstant, float deltaTime)
		{
			return initial / Mathf.Exp(decayConstant * deltaTime);
		}

		public static float Damp(float initial, float dampTime, float deltaTime)
		{
			if (dampTime < 0.0001f || Mathf.Abs(initial) < 0.0001f)
			{
				return initial;
			}
			if (deltaTime < 0.0001f)
			{
				return 0f;
			}
			return initial - Damper.Decay(initial, Damper.DecayConstant(dampTime, 0.01f), deltaTime);
		}

		public static Vector3 Damp(Vector3 initial, Vector3 dampTime, float deltaTime)
		{
			for (int i = 0; i < 3; i++)
			{
				initial[i] = Damper.Damp(initial[i], dampTime[i], deltaTime);
			}
			return initial;
		}

		public static Vector3 Damp(Vector3 initial, float dampTime, float deltaTime)
		{
			for (int i = 0; i < 3; i++)
			{
				initial[i] = Damper.Damp(initial[i], dampTime, deltaTime);
			}
			return initial;
		}

		private const float Epsilon = 0.0001f;

		public const float kNegligibleResidual = 0.01f;
	}
}
