using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class ApplicationSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance
		{
			get
			{
				if (ApplicationSingleton<T>._Instance != null)
				{
					return ApplicationSingleton<T>._Instance;
				}
				ApplicationSingleton<T>._Instance = (T)((object)UnityEngine.Object.FindObjectOfType(typeof(T)));
				if (ApplicationSingleton<T>._Instance != null)
				{
					return ApplicationSingleton<T>._Instance;
				}
				if (ApplicationSingleton<T>._Quiting)
				{
					return (T)((object)null);
				}
				GameObject gameObject = new GameObject("[" + typeof(T).Name + "] - instance")
				{
					hideFlags = HideFlags.HideInHierarchy
				};
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				ApplicationSingleton<T>._Instance = gameObject.AddComponent<T>();
				return ApplicationSingleton<T>._Instance;
			}
		}

		protected virtual void OnApplicationQuit()
		{
			ApplicationSingleton<T>._Quiting = true;
		}

		protected virtual void OnDestroy()
		{
			ApplicationSingleton<T>._Quiting = true;
		}

		private static T _Instance;

		private static bool _Quiting;
	}
}
