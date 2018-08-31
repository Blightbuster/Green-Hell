using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance
		{
			get
			{
				if (SceneSingleton<T>._Instance != null)
				{
					return SceneSingleton<T>._Instance;
				}
				SceneSingleton<T>._Instance = (T)((object)UnityEngine.Object.FindObjectOfType(typeof(T)));
				if (SceneSingleton<T>._Instance != null)
				{
					return SceneSingleton<T>._Instance;
				}
				if (SceneSingleton<T>._Quiting)
				{
					return (T)((object)null);
				}
				GameObject gameObject = new GameObject("[" + typeof(T).Name + "] - instance")
				{
					hideFlags = HideFlags.HideInHierarchy
				};
				SceneSingleton<T>._Instance = gameObject.AddComponent<T>();
				return SceneSingleton<T>._Instance;
			}
		}

		private void OnApplicationQuit()
		{
			SceneSingleton<T>._Quiting = true;
		}

		private void OnDestroy()
		{
			SceneSingleton<T>._Quiting = true;
		}

		private static T _Instance;

		private static bool _Quiting;
	}
}
