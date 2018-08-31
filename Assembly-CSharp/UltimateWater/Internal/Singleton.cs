using System;

namespace UltimateWater.Internal
{
	public class Singleton<T> where T : new()
	{
		public static T Instance
		{
			get
			{
				if (Singleton<T>._Instance == null)
				{
					Singleton<T>._Instance = Activator.CreateInstance<T>();
				}
				return Singleton<T>._Instance;
			}
		}

		private static T _Instance;
	}
}
