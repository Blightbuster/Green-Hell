using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public static class WaterLogger
	{
		public static void Info(string script, string method, string text)
		{
			Debug.Log("[Ultimate Water System] : " + text);
		}

		public static void Warning(string script, string method, string text)
		{
			Debug.LogWarning("[Ultimate Water System] : " + text);
		}

		public static void Error(string script, string method, string text)
		{
			Debug.LogError("[Ultimate Water System] : " + text);
		}

		private const string _Prefix = "[Ultimate Water System] : ";
	}
}
