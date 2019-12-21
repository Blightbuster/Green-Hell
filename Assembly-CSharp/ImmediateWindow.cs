using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class ImmediateWindow
{
	public static ImmediateWindow.Result RunCommand(string command, out string result)
	{
		result = string.Empty;
		while (command.Contains("  "))
		{
			command = command.Replace("  ", " ");
		}
		if (command.StartsWith(" "))
		{
			command = command.Substring(1);
		}
		if (command.EndsWith(" "))
		{
			command = command.Substring(0, command.Length - 1);
		}
		string[] array = command.Split(new char[]
		{
			' '
		});
		if (array.Length == 0)
		{
			return ImmediateWindow.Result.NoInput;
		}
		List<Type> list = new List<Type>();
		string[] array2 = array[0].Split(new char[]
		{
			'.'
		});
		string text = string.Empty;
		for (int i = 0; i < array2.Length; i++)
		{
			if (!text.Empty())
			{
				text += ".";
			}
			text += array2[i];
			List<Type> types = ImmediateWindow.GetTypes(text);
			if (types.Count != 1)
			{
				list = list.Concat(types).ToList<Type>();
				break;
			}
			list = types;
		}
		if (list.Count == 0)
		{
			return ImmediateWindow.Result.InvalidType;
		}
		if (list.Count > 1)
		{
			result = string.Join<Type>(" ", list);
			return ImmediateWindow.Result.IncompleteType;
		}
		if (array.Length < 2)
		{
			return ImmediateWindow.Result.InvalidMember;
		}
		string[] members = array[1].Split(new char[]
		{
			'.'
		});
		string[] parameters = array.Skip(2).ToArray<string>();
		return ImmediateWindow.Execute(list[0], members, parameters, ref result);
	}

	public static List<Type> GetTypes(string type_name)
	{
		if (ImmediateWindow.m_Assemblies == null)
		{
			ImmediateWindow.PopulateAssemblyCache();
		}
		List<Type> list = new List<Type>();
		Type type = null;
		for (int i = 0; i < ImmediateWindow.m_Assemblies.Count; i++)
		{
			Assembly assembly = ImmediateWindow.m_Assemblies[i];
			Type type2 = assembly.GetType(type_name, false, true);
			if (!(type2 == null))
			{
				type = type2;
				break;
			}
			foreach (Type type3 in assembly.GetTypes())
			{
				if (type3.Name.StartsWith(type_name, true, CultureInfo.InvariantCulture))
				{
					list.Add(type3);
				}
			}
		}
		if (type != null)
		{
			list.Clear();
			list.Add(type);
		}
		return list;
	}

	private static ImmediateWindow.Result Execute(Type type, string[] members, string[] parameters, ref string result)
	{
		ImmediateWindow.Result result2;
		try
		{
			List<MemberInfo> list = new List<MemberInfo>();
			string text = string.Empty;
			bool flag = false;
			Type type2 = type;
			int i = 0;
			while (i < members.Length)
			{
				string text2 = members[i];
				result = string.Empty;
				MemberInfo memberInfo = null;
				MemberInfo[] members2 = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				MemberInfo[] members3 = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MemberInfo memberInfo2 in members2)
				{
					if (memberInfo2.Name.ICompare(text2) == 0)
					{
						memberInfo = memberInfo2;
						break;
					}
					if (memberInfo2.Name.StartsWith(text2, true, CultureInfo.InvariantCulture))
					{
						if (text.Empty())
						{
							result = result + memberInfo2.Name + " ";
						}
						else
						{
							result = string.Concat(new string[]
							{
								result,
								text,
								".",
								memberInfo2.Name,
								" "
							});
						}
					}
				}
				if (memberInfo == null)
				{
					MemberInfo[] array = members3;
					int j = 0;
					while (j < array.Length)
					{
						MemberInfo memberInfo3 = array[j];
						if (memberInfo3.Name.ICompare(text2) == 0)
						{
							memberInfo = memberInfo3;
							if (i == 0)
							{
								flag = true;
								break;
							}
							break;
						}
						else
						{
							if (memberInfo3.Name.StartsWith(text2, true, CultureInfo.InvariantCulture))
							{
								if (text.Empty())
								{
									result = result + memberInfo3.Name + " ";
								}
								else
								{
									result = string.Concat(new string[]
									{
										result,
										text,
										".",
										memberInfo3.Name,
										" "
									});
								}
							}
							j++;
						}
					}
				}
				if (memberInfo == null)
				{
					if (result.Length > 0)
					{
						return ImmediateWindow.Result.IncompleteMember;
					}
					return ImmediateWindow.Result.InvalidMember;
				}
				else
				{
					if (!text.Empty())
					{
						text += ".";
					}
					text += memberInfo.Name;
					result = string.Empty;
					type = ImmediateWindow.GetMemberUndelyingType(memberInfo);
					list.Add(memberInfo);
					i++;
				}
			}
			object obj = null;
			if (flag)
			{
				if (parameters.Length == 0)
				{
					return ImmediateWindow.Result.InstanceNameRequired;
				}
				foreach (UnityEngine.Object @object in UnityEngine.Object.FindObjectsOfTypeAll(type2))
				{
					if (parameters[0].ICompare("?") == 0 || @object.name.ICompare(parameters[0]) == 0)
					{
						obj = @object;
						break;
					}
				}
				if (obj == null)
				{
					return ImmediateWindow.Result.InvalidInstanceName;
				}
				parameters = parameters.Skip(1).ToArray<string>();
			}
			if (list.Count > 0)
			{
				for (int k = 0; k < list.Count; k++)
				{
					MemberInfo memberInfo4 = list[k];
					bool flag2 = k == list.Count - 1;
					if (memberInfo4.MemberType == MemberTypes.Field)
					{
						FieldInfo fieldInfo = (FieldInfo)memberInfo4;
						if (!flag2)
						{
							obj = fieldInfo.GetValue(obj);
						}
						else if (parameters.Length != 0)
						{
							object value = fieldInfo.GetValue(obj);
							((FieldInfo)memberInfo4).SetValue(obj, ImmediateWindow.ConvertFromString(parameters[0], fieldInfo.FieldType));
							result = "> " + ((value != null) ? value.ToString() : null) + " -> " + parameters[0];
						}
						else
						{
							object value2 = fieldInfo.GetValue(obj);
							result = "> " + ((value2 != null) ? value2.ToString() : null) + " " + ImmediateWindow.GetNameMemberValue(value2);
						}
					}
					else if (memberInfo4.MemberType == MemberTypes.Property)
					{
						PropertyInfo propertyInfo = (PropertyInfo)memberInfo4;
						if (!flag2)
						{
							obj = propertyInfo.GetValue(obj);
						}
						else if (parameters.Length != 0)
						{
							object value3 = propertyInfo.GetValue(obj);
							((PropertyInfo)memberInfo4).SetValue(obj, ImmediateWindow.ConvertFromString(parameters[0], propertyInfo.PropertyType));
							result = "> " + ((value3 != null) ? value3.ToString() : null) + " -> " + parameters[0];
						}
						else
						{
							object value4 = propertyInfo.GetValue(obj);
							result = "> " + ((value4 != null) ? value4.ToString() : null) + " " + ImmediateWindow.GetNameMemberValue(value4);
						}
					}
					else if (memberInfo4.MemberType == MemberTypes.Method)
					{
						MethodInfo methodInfo = (MethodInfo)memberInfo4;
						ParameterInfo[] parameters2 = methodInfo.GetParameters();
						if (!flag2)
						{
							ParameterInfo[] array3 = parameters2;
							for (int j = 0; j < array3.Length; j++)
							{
								if (!array3[j].IsOptional)
								{
									return ImmediateWindow.Result.MethodRequiresParameters;
								}
							}
							object[] parameters3 = Enumerable.Repeat<object>(Type.Missing, parameters2.Length).ToArray<object>();
							obj = methodInfo.Invoke(obj, parameters3);
						}
						else
						{
							List<object> list2 = new List<object>();
							for (int l = 0; l < parameters2.Length; l++)
							{
								if (parameters.Length > l)
								{
									list2.Add(ImmediateWindow.ConvertFromString(parameters[l], parameters2[l].ParameterType));
								}
							}
							object obj2 = methodInfo.Invoke(obj, list2.ToArray());
							result = "> " + ((obj2 != null) ? obj2.ToString() : null) + " " + ImmediateWindow.GetNameMemberValue(obj2);
						}
					}
				}
				result2 = ImmediateWindow.Result.Success;
			}
			else if (result.Length > 0)
			{
				result2 = ImmediateWindow.Result.IncompleteMember;
			}
			else
			{
				result2 = ImmediateWindow.Result.InvalidMember;
			}
		}
		catch (Exception ex)
		{
			result = ex.ToString();
			result2 = ImmediateWindow.Result.Exception;
		}
		return result2;
	}

	private static string GetNameMemberValue(object obj)
	{
		string text = string.Empty;
		MemberInfo[] array = (obj != null) ? obj.GetType().GetMember("name") : null;
		if (array != null)
		{
			foreach (MemberInfo memberInfo in array)
			{
				if (memberInfo.MemberType == MemberTypes.Property)
				{
					text += string.Format(" ({0})", ((PropertyInfo)memberInfo).GetValue(obj));
				}
				else if (memberInfo.MemberType == MemberTypes.Field)
				{
					text += string.Format(" ({0})", ((FieldInfo)memberInfo).GetValue(obj));
				}
			}
		}
		return text;
	}

	private static Type GetMemberUndelyingType(MemberInfo member)
	{
		if (member == null)
		{
			return null;
		}
		if (member.MemberType == MemberTypes.Field)
		{
			return ((FieldInfo)member).FieldType;
		}
		if (member.MemberType == MemberTypes.Property)
		{
			return ((PropertyInfo)member).PropertyType;
		}
		if (member.MemberType == MemberTypes.Method)
		{
			return ((MethodInfo)member).ReturnType;
		}
		return null;
	}

	private static void PopulateAssemblyCache()
	{
		IList<string> userAssemblyPaths = ImmediateWindow.GetUserAssemblyPaths();
		foreach (string item in Directory.GetFiles((Application.dataPath + "/Managed/") ?? "", "*.dll"))
		{
			userAssemblyPaths.Add(item);
		}
		ImmediateWindow.m_Assemblies = new Assembly[userAssemblyPaths.Count];
		for (int j = 0; j < userAssemblyPaths.Count; j++)
		{
			ImmediateWindow.m_Assemblies[j] = Assembly.LoadFile(userAssemblyPaths[j]);
		}
	}

	private static IList<string> GetUserAssemblyPaths()
	{
		List<string> result = new List<string>(20);
		ImmediateWindow.FindAssemblies(Application.dataPath + "/../Library/ScriptAssemblies/", 2, result);
		return result;
	}

	private static void FindAssemblies(string systemPath, int maxDepth, List<string> result)
	{
		if (maxDepth > 0)
		{
			try
			{
				if (Directory.Exists(systemPath))
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(systemPath);
					result.AddRange(from file in directoryInfo.GetFiles()
					where ImmediateWindow.IsManagedAssembly(file.FullName)
					select file.FullName);
					DirectoryInfo[] directories = directoryInfo.GetDirectories();
					for (int i = 0; i < directories.Length; i++)
					{
						ImmediateWindow.FindAssemblies(directories[i].FullName, maxDepth - 1, result);
					}
				}
			}
			catch
			{
			}
		}
	}

	private static bool IsManagedAssembly(string systemPath)
	{
		return true;
	}

	private static object ConvertFromString(string value, Type t)
	{
		if (t.IsEnum)
		{
			return Enum.Parse(t, value, true);
		}
		return Convert.ChangeType(value, t);
	}

	private static IList<Assembly> m_Assemblies;

	public enum Result
	{
		NoInput,
		InvalidType,
		IncompleteType,
		InvalidMember,
		IncompleteMember,
		InvalidInstanceName,
		InstanceNameRequired,
		MethodRequiresParameters,
		Success,
		Exception
	}
}
