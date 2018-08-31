using System;
using UnityEngine;

namespace RootMotion
{
	public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		public static T instance
		{
			get
			{
				return Singleton<T>.sInstance;
			}
		}

		protected virtual void Awake()
		{
			if (Singleton<T>.sInstance != null)
			{
				Debug.LogError(base.name + "error: already initialized", this);
			}
			Singleton<T>.sInstance = (T)((object)this);
		}

		private static T sInstance;
	}
}
