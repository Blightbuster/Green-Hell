using System;
using System.Collections.Generic;

public class IntArrayComparer : IEqualityComparer<int[]>
{
	public bool Equals(int[] x, int[] y)
	{
		if (x.Length != y.Length)
		{
			return false;
		}
		for (int i = 0; i < x.Length; i++)
		{
			if (x[i] != y[i])
			{
				return false;
			}
		}
		return true;
	}

	public int GetHashCode(int[] obj)
	{
		int num = 17;
		for (int i = 0; i < obj.Length; i++)
		{
			num = num * 23 + obj[i];
		}
		return num;
	}
}
