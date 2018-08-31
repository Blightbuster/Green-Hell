using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class AIAttractorsManager : MonoBehaviour
	{
		public static void RegisterAttractor(AIAttractor attr)
		{
			if (AIAttractorsManager.s_Attractors != null)
			{
				AIAttractorsManager.s_Attractors.Add(attr);
			}
		}

		public static void UnregisterAttractor(AIAttractor attr)
		{
			if (AIAttractorsManager.s_Attractors != null)
			{
				AIAttractorsManager.s_Attractors.Remove(attr);
			}
		}

		private void OnDestroy()
		{
			AIAttractorsManager.s_Attractors = null;
		}

		public static List<AIAttractor> s_Attractors = new List<AIAttractor>();
	}
}
