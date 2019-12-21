using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtension
{
	public static void Resize<T>(this List<T> list, int size, T element = default(T))
	{
		int count = list.Count;
		if (size < count)
		{
			list.RemoveRange(size, count - size);
			return;
		}
		if (size > count)
		{
			if (size > list.Capacity)
			{
				list.Capacity = size;
			}
			list.AddRange(Enumerable.Repeat<T>(element, size - count));
		}
	}

	public static void Resize<T>(this List<T> list, int size) where T : class, new()
	{
		int count = list.Count;
		list.Resize(size, default(T));
		for (int i = 0; i < size - count; i++)
		{
			list[i + count] = Activator.CreateInstance<T>();
		}
	}
}
