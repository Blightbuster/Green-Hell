using System;

namespace Pathfinding.Util
{
	public static class ObjectPool<T> where T : class, IAstarPooledObject, new()
	{
		public static T Claim()
		{
			return ObjectPoolSimple<T>.Claim();
		}

		public static void Release(ref T obj)
		{
			T t = obj;
			ObjectPoolSimple<T>.Release(ref obj);
			t.OnEnterPool();
		}
	}
}
