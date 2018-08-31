using System;
using System.Collections.Generic;

namespace Pathfinding.Util
{
	public static class ListPool<T>
	{
		public static List<T> Claim()
		{
			object obj = ListPool<T>.pool;
			List<T> result;
			lock (obj)
			{
				if (ListPool<T>.pool.Count > 0)
				{
					List<T> list = ListPool<T>.pool[ListPool<T>.pool.Count - 1];
					ListPool<T>.pool.RemoveAt(ListPool<T>.pool.Count - 1);
					ListPool<T>.inPool.Remove(list);
					result = list;
				}
				else
				{
					result = new List<T>();
				}
			}
			return result;
		}

		private static int FindCandidate(List<List<T>> pool, int capacity)
		{
			List<T> list = null;
			int result = -1;
			int num = 0;
			while (num < pool.Count && num < 8)
			{
				List<T> list2 = pool[pool.Count - 1 - num];
				if ((list == null || list2.Capacity > list.Capacity) && list2.Capacity < capacity * 16)
				{
					list = list2;
					result = pool.Count - 1 - num;
					if (list.Capacity >= capacity)
					{
						return result;
					}
				}
				num++;
			}
			return result;
		}

		public static List<T> Claim(int capacity)
		{
			object obj = ListPool<T>.pool;
			List<T> result;
			lock (obj)
			{
				List<List<T>> list = ListPool<T>.pool;
				int num = ListPool<T>.FindCandidate(ListPool<T>.pool, capacity);
				if (capacity > 5000)
				{
					int num2 = ListPool<T>.FindCandidate(ListPool<T>.largePool, capacity);
					if (num2 != -1)
					{
						list = ListPool<T>.largePool;
						num = num2;
					}
				}
				if (num == -1)
				{
					result = new List<T>(capacity);
				}
				else
				{
					List<T> list2 = list[num];
					ListPool<T>.inPool.Remove(list2);
					list[num] = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					result = list2;
				}
			}
			return result;
		}

		public static void Warmup(int count, int size)
		{
			object obj = ListPool<T>.pool;
			lock (obj)
			{
				List<T>[] array = new List<T>[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = ListPool<T>.Claim(size);
				}
				for (int j = 0; j < count; j++)
				{
					ListPool<T>.Release(array[j]);
				}
			}
		}

		public static void Release(List<T> list)
		{
			if (list.Count * 2 < list.Capacity)
			{
				list.RemoveRange(0, list.Count);
			}
			else
			{
				list.Clear();
			}
			object obj = ListPool<T>.pool;
			lock (obj)
			{
				if (list.Capacity > 5000)
				{
					ListPool<T>.largePool.Add(list);
					if (ListPool<T>.largePool.Count > 8)
					{
						ListPool<T>.largePool.RemoveAt(0);
					}
				}
				else
				{
					ListPool<T>.pool.Add(list);
				}
			}
		}

		public static void Clear()
		{
			object obj = ListPool<T>.pool;
			lock (obj)
			{
				ListPool<T>.pool.Clear();
			}
		}

		public static int GetSize()
		{
			return ListPool<T>.pool.Count;
		}

		private static readonly List<List<T>> pool = new List<List<T>>();

		private static readonly List<List<T>> largePool = new List<List<T>>();

		private static readonly HashSet<List<T>> inPool = new HashSet<List<T>>();

		private const int MaxCapacitySearchLength = 8;

		private const int LargeThreshold = 5000;

		private const int MaxLargePoolSize = 8;
	}
}
