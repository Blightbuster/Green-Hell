using System;
using UnityEngine;

namespace UltimateWater
{
	public class ScriptableObjectSingleton : ScriptableObject
	{
		protected static T LoadSingleton<T>() where T : ScriptableObject
		{
			return Resources.Load<T>(typeof(T).Name);
		}
	}
}
