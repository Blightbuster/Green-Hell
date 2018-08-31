using System;
using System.Collections.Generic;

namespace Pathfinding.Util
{
	public static class ArrayPool<T>
	{
		public static T[] Claim(int minimumLength)
		{
			if (minimumLength <= 0)
			{
				return ArrayPool<T>.ClaimWithExactLength(0);
			}
			int num = 0;
			while (1 << num < minimumLength && num < 30)
			{
				num++;
			}
			if (num == 30)
			{
				throw new ArgumentException("Too high minimum length");
			}
			object obj = ArrayPool<T>.pool;
			lock (obj)
			{
				if (ArrayPool<T>.pool[num] == null)
				{
					ArrayPool<T>.pool[num] = new Stack<T[]>();
				}
				if (ArrayPool<T>.pool[num].Count > 0)
				{
					T[] array = ArrayPool<T>.pool[num].Pop();
					ArrayPool<T>.inPool.Remove(array);
					return array;
				}
			}
			return new T[1 << num];
		}

		public static T[] ClaimWithExactLength(int length)
		{
			bool flag = length != 0 && (length & length - 1) == 0;
			if (flag)
			{
				return ArrayPool<T>.Claim(length);
			}
			object obj = ArrayPool<T>.pool;
			lock (obj)
			{
				Stack<T[]> stack;
				if (!ArrayPool<T>.exactPool.TryGetValue(length, out stack))
				{
					stack = new Stack<T[]>();
					ArrayPool<T>.exactPool[length] = stack;
				}
				if (stack.Count > 0)
				{
					T[] array = stack.Pop();
					ArrayPool<T>.inPool.Remove(array);
					return array;
				}
			}
			return new T[length];
		}

		public static void Release(ref T[] array, bool allowNonPowerOfTwo = false)
		{
			if (array.GetType() != typeof(T[]))
			{
				throw new ArgumentException(string.Concat(new string[]
				{
					"Expected array type ",
					typeof(T[]).Name,
					" but found ",
					array.GetType().Name,
					"\nAre you using the correct generic class?\n"
				}));
			}
			bool flag = array.Length != 0 && (array.Length & array.Length - 1) == 0;
			if (!flag && !allowNonPowerOfTwo && array.Length != 0)
			{
				throw new ArgumentException("Length is not a power of 2");
			}
			object obj = ArrayPool<T>.pool;
			lock (obj)
			{
				if (flag)
				{
					int num = 0;
					while (1 << num < array.Length && num < 30)
					{
						num++;
					}
					if (ArrayPool<T>.pool[num] == null)
					{
						ArrayPool<T>.pool[num] = new Stack<T[]>();
					}
					ArrayPool<T>.pool[num].Push(array);
				}
				else
				{
					Stack<T[]> stack;
					if (!ArrayPool<T>.exactPool.TryGetValue(array.Length, out stack))
					{
						stack = new Stack<T[]>();
						ArrayPool<T>.exactPool[array.Length] = stack;
					}
					stack.Push(array);
				}
			}
			array = null;
		}

		private static readonly Stack<T[]>[] pool = new Stack<T[]>[31];

		private static readonly Dictionary<int, Stack<T[]>> exactPool = new Dictionary<int, Stack<T[]>>();

		private static readonly HashSet<T[]> inPool = new HashSet<T[]>();
	}
}
