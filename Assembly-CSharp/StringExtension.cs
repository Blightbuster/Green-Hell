using System;

public static class StringExtension
{
	public static bool Empty(this string str)
	{
		return str.Length <= 0;
	}
}
