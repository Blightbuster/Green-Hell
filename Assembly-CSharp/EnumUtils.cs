using System;
using System.Collections;
using System.Collections.Generic;

public static class EnumUtils<T> where T : struct, IConvertible
{
	public static string GetName(int enum_value)
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		EnumUtils<T>.InitNames();
		return EnumUtils<T>.s_EnumNames[enum_value];
	}

	public static string GetName(T enum_value)
	{
		EnumUtils<T>.InitNames();
		string result;
		if (EnumUtils<T>.s_EnumNames.TryGetValue(Convert.ToInt32(enum_value), out result))
		{
			return result;
		}
		return string.Empty;
	}

	private static void InitNames()
	{
		if (EnumUtils<T>.s_EnumNames == null)
		{
			Array values = Enum.GetValues(typeof(T));
			EnumUtils<T>.s_EnumNames = new Dictionary<int, string>(values.Length);
			IEnumerator enumerator = values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!EnumUtils<T>.s_EnumNames.ContainsKey((int)enumerator.Current))
				{
					EnumUtils<T>.s_EnumNames.Add((int)enumerator.Current, enumerator.Current.ToString());
				}
			}
		}
	}

	public static T GetValue(string name)
	{
		EnumUtils<T>.IniValues();
		T result;
		if (!EnumUtils<T>.s_EnumValues.TryGetValue(name, out result))
		{
			DebugUtils.Assert(string.Format("Invalid enum {0} element name: {1}!", typeof(T), name), true, DebugUtils.AssertType.Info);
		}
		return result;
	}

	public static bool TryGetValue(string name, out T value)
	{
		EnumUtils<T>.IniValues();
		return EnumUtils<T>.s_EnumValues.TryGetValue(name, out value);
	}

	private static void IniValues()
	{
		if (EnumUtils<T>.s_EnumValues == null)
		{
			string[] names = Enum.GetNames(typeof(T));
			EnumUtils<T>.s_EnumValues = new Dictionary<string, T>(names.Length);
			IEnumerator enumerator = names.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (!EnumUtils<T>.s_EnumValues.ContainsKey((string)enumerator.Current))
				{
					EnumUtils<T>.s_EnumValues.Add((string)enumerator.Current, (T)((object)Enum.Parse(typeof(T), (string)enumerator.Current)));
				}
			}
		}
	}

	public static void ForeachName(Action<string> action)
	{
		EnumUtils<T>.InitNames();
		Dictionary<int, string>.Enumerator enumerator = EnumUtils<T>.s_EnumNames.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, string> keyValuePair = enumerator.Current;
			action(keyValuePair.Value);
		}
		enumerator.Dispose();
	}

	public static void ForeachValue(Action<T> action)
	{
		EnumUtils<T>.InitNames();
		Dictionary<int, string>.Enumerator enumerator = EnumUtils<T>.s_EnumNames.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Type typeFromHandle = typeof(T);
			KeyValuePair<int, string> keyValuePair = enumerator.Current;
			action((T)((object)Enum.ToObject(typeFromHandle, keyValuePair.Key)));
		}
		enumerator.Dispose();
	}

	public static bool ForeachValueSequence(Func<T, bool> func)
	{
		EnumUtils<T>.InitNames();
		Dictionary<int, string>.Enumerator enumerator = EnumUtils<T>.s_EnumNames.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Type typeFromHandle = typeof(T);
			KeyValuePair<int, string> keyValuePair = enumerator.Current;
			if (!func((T)((object)Enum.ToObject(typeFromHandle, keyValuePair.Key))))
			{
				enumerator.Dispose();
				return false;
			}
		}
		enumerator.Dispose();
		return true;
	}

	public static bool ForeachValueSelector(Func<T, bool> func)
	{
		EnumUtils<T>.InitNames();
		Dictionary<int, string>.Enumerator enumerator = EnumUtils<T>.s_EnumNames.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Type typeFromHandle = typeof(T);
			KeyValuePair<int, string> keyValuePair = enumerator.Current;
			if (func((T)((object)Enum.ToObject(typeFromHandle, keyValuePair.Key))))
			{
				enumerator.Dispose();
				return true;
			}
		}
		enumerator.Dispose();
		return false;
	}

	private static Dictionary<int, string> s_EnumNames;

	private static Dictionary<string, T> s_EnumValues;
}
