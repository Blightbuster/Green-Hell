using System;
using System.Collections.Generic;
using System.Reflection;

public static class ReplicationComponentReflection
{
	private static MethodInfo GetMethodInfo(Type type, ReplicationComponentReflection.ReflectedMethodType method_type)
	{
		MethodInfo[] array;
		if (!ReplicationComponentReflection.s_ReflectedMethods.TryGetValue(type, out array))
		{
			array = new MethodInfo[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = type.GetMethod(EnumUtils<ReplicationComponentReflection.ReflectedMethodType>.GetName(i), BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
			}
			ReplicationComponentReflection.s_ReflectedMethods[type] = array;
		}
		return array[(int)method_type];
	}

	public static TDelegate GetReflectedMethod<TDelegate>(this IReplicatedBehaviour obj, ReplicationComponentReflection.ReflectedMethodType method_type) where TDelegate : class
	{
		MethodInfo methodInfo = ReplicationComponentReflection.GetMethodInfo(obj.GetType(), method_type);
		if (methodInfo != null)
		{
			return Delegate.CreateDelegate(typeof(TDelegate), obj, methodInfo) as TDelegate;
		}
		return default(TDelegate);
	}

	private static Dictionary<Type, MethodInfo[]> s_ReflectedMethods = new Dictionary<Type, MethodInfo[]>();

	public enum ReflectedMethodType
	{
		OnReplicationPrepare_CJGenerated,
		OnReplicationSerialize_CJGenerated,
		OnReplicationDeserialize_CJGenerated,
		OnReplicationResolve_CJGenerated,
		_Count
	}
}
