using System;
using System.Collections.Generic;

namespace Pathfinding.Util
{
	public static class ObjectPoolSimple<T> where T : class, new()
	{
		public static T Claim()
		{
			object obj = ObjectPoolSimple<T>.pool;
			T result;
			lock (obj)
			{
				if (ObjectPoolSimple<T>.pool.Count > 0)
				{
					T t = ObjectPoolSimple<T>.pool[ObjectPoolSimple<T>.pool.Count - 1];
					ObjectPoolSimple<T>.pool.RemoveAt(ObjectPoolSimple<T>.pool.Count - 1);
					ObjectPoolSimple<T>.inPool.Remove(t);
					result = t;
				}
				else
				{
					result = Activator.CreateInstance<T>();
				}
			}
			return result;
		}

		public static void Release(ref T obj)
		{
			object obj2 = ObjectPoolSimple<T>.pool;
			lock (obj2)
			{
				ObjectPoolSimple<T>.pool.Add(obj);
			}
			obj = (T)((object)null);
		}

		public static void Clear()
		{
			object obj = ObjectPoolSimple<T>.pool;
			lock (obj)
			{
				ObjectPoolSimple<T>.pool.Clear();
			}
		}

		public static int GetSize()
		{
			return ObjectPoolSimple<T>.pool.Count;
		}

		private static List<T> pool = new List<T>();

		private static readonly HashSet<T> inPool = new HashSet<T>();
	}
}
