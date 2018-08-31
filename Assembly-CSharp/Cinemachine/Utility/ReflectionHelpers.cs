using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Cinemachine.Utility
{
	[DocumentationSorting(0f, DocumentationSortingAttribute.Level.Undoc)]
	public static class ReflectionHelpers
	{
		public static void CopyFields(object src, object dst, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		{
			if (src != null && dst != null)
			{
				Type type = src.GetType();
				FieldInfo[] fields = type.GetFields(bindingAttr);
				for (int i = 0; i < fields.Length; i++)
				{
					if (!fields[i].IsStatic)
					{
						fields[i].SetValue(dst, fields[i].GetValue(src));
					}
				}
			}
		}

		public static T AccessInternalField<T>(this Type type, object obj, string memberName)
		{
			if (string.IsNullOrEmpty(memberName) || type == null)
			{
				return default(T);
			}
			BindingFlags bindingFlags = BindingFlags.NonPublic;
			if (obj != null)
			{
				bindingFlags |= BindingFlags.Instance;
			}
			else
			{
				bindingFlags |= BindingFlags.Static;
			}
			FieldInfo field = type.GetField(memberName, bindingFlags);
			if (field != null && field.FieldType == typeof(T))
			{
				return (T)((object)field.GetValue(obj));
			}
			return default(T);
		}

		public static object GetParentObject(string path, object obj)
		{
			string[] array = path.Split(new char[]
			{
				'.'
			});
			if (array.Length == 1)
			{
				return obj;
			}
			FieldInfo field = obj.GetType().GetField(array[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			obj = field.GetValue(obj);
			return ReflectionHelpers.GetParentObject(string.Join(".", array, 1, array.Length - 1), obj);
		}

		public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
		{
			ExpressionType nodeType = expr.Body.NodeType;
			if (nodeType != ExpressionType.MemberAccess)
			{
				throw new InvalidOperationException();
			}
			MemberExpression memberExpression = expr.Body as MemberExpression;
			List<string> list = new List<string>();
			while (memberExpression != null)
			{
				list.Add(memberExpression.Member.Name);
				memberExpression = (memberExpression.Expression as MemberExpression);
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				stringBuilder.Append(list[i]);
				if (i > 0)
				{
					stringBuilder.Append('.');
				}
			}
			return stringBuilder.ToString();
		}
	}
}
