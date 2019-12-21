using System;

public static class StringExtension
{
	public static bool Empty(this string str)
	{
		return str == null || str.Length <= 0;
	}

	public static int ICompare(this string first, string second)
	{
		return string.Compare(first, second, StringComparison.OrdinalIgnoreCase);
	}

	public const string EmptyString = "";
}
