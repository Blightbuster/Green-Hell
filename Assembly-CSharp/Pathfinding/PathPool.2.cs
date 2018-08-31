using System;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[Obsolete("Generic version is now obsolete to trade an extremely tiny performance decrease for a large decrease in boilerplate for Path classes")]
	public static class PathPool<T> where T : Path, new()
	{
		public static void Recycle(T path)
		{
			PathPool.Pool(path);
		}

		public static void Warmup(int count, int length)
		{
			ListPool<GraphNode>.Warmup(count, length);
			ListPool<Vector3>.Warmup(count, length);
			Path[] array = new Path[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = PathPool<T>.GetPath();
				array[i].Claim(array);
			}
			for (int j = 0; j < count; j++)
			{
				array[j].Release(array, false);
			}
		}

		public static int GetTotalCreated()
		{
			return PathPool.GetTotalCreated(typeof(T));
		}

		public static int GetSize()
		{
			return PathPool.GetSize(typeof(T));
		}

		[Obsolete("Use PathPool.GetPath<T> instead of PathPool<T>.GetPath")]
		public static T GetPath()
		{
			return PathPool.GetPath<T>();
		}
	}
}
