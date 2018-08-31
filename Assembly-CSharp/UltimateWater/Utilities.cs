using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace UltimateWater
{
	public static class Utilities
	{
		public static int LayerMaskToInt(LayerMask mask)
		{
			for (int i = 0; i < 32; i++)
			{
				if ((mask.value & 1 << i) != 0)
				{
					return i;
				}
			}
			return -1;
		}

		public static Water GetWaterReference()
		{
			return Utilities.FindRefenrece<Water>();
		}

		public static bool IsNullReference<T>(this T obj, MonoBehaviour caller = null) where T : UnityEngine.Object
		{
			if (obj != null)
			{
				return false;
			}
			string text = string.Empty;
			MethodBase method = new StackTrace().GetFrame(1).GetMethod();
			if (method != null && method.ReflectedType != null)
			{
				text = "[" + method.ReflectedType.Name + "]: ";
			}
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"[Ultimate Water System]",
				text,
				"reference to the: ",
				typeof(T),
				" not set."
			}));
			if (caller != null)
			{
				caller.enabled = false;
			}
			return true;
		}

		public static void Destroy(this UnityEngine.Object obj)
		{
			UnityEngine.Object.Destroy(obj);
		}

		private static T FindRefenrece<T>() where T : MonoBehaviour
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(T));
			if (array.Length == 0 || array.Length > 1)
			{
				return (T)((object)null);
			}
			return array[0] as T;
		}
	}
}
