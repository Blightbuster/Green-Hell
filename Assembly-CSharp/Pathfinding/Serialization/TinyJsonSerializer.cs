using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Pathfinding.Util;
using Pathfinding.WindowsStore;
using UnityEngine;

namespace Pathfinding.Serialization
{
	public class TinyJsonSerializer
	{
		private TinyJsonSerializer()
		{
			this.serializers[typeof(float)] = delegate(object v)
			{
				this.output.Append(((float)v).ToString("R", TinyJsonSerializer.invariantCulture));
			};
			this.serializers[typeof(bool)] = delegate(object v)
			{
				this.output.Append((!(bool)v) ? "false" : "true");
			};
			Dictionary<Type, Action<object>> dictionary = this.serializers;
			Type typeFromHandle = typeof(Version);
			Action<object> action = delegate(object v)
			{
				this.output.Append(v.ToString());
			};
			this.serializers[typeof(int)] = action;
			action = action;
			this.serializers[typeof(uint)] = action;
			dictionary[typeFromHandle] = action;
			this.serializers[typeof(string)] = delegate(object v)
			{
				this.output.AppendFormat("\"{0}\"", v.ToString().Replace("\"", "\\\""));
			};
			this.serializers[typeof(Vector2)] = delegate(object v)
			{
				this.output.AppendFormat("{{ \"x\": {0}, \"y\": {1} }}", ((Vector2)v).x.ToString("R", TinyJsonSerializer.invariantCulture), ((Vector2)v).y.ToString("R", TinyJsonSerializer.invariantCulture));
			};
			this.serializers[typeof(Vector3)] = delegate(object v)
			{
				this.output.AppendFormat("{{ \"x\": {0}, \"y\": {1}, \"z\": {2} }}", ((Vector3)v).x.ToString("R", TinyJsonSerializer.invariantCulture), ((Vector3)v).y.ToString("R", TinyJsonSerializer.invariantCulture), ((Vector3)v).z.ToString("R", TinyJsonSerializer.invariantCulture));
			};
			this.serializers[typeof(Pathfinding.Util.Guid)] = delegate(object v)
			{
				this.output.AppendFormat("{{ \"value\": \"{0}\" }}", v.ToString());
			};
			this.serializers[typeof(LayerMask)] = delegate(object v)
			{
				this.output.AppendFormat("{{ \"value\": {0} }}", ((LayerMask)v).ToString());
			};
		}

		public static void Serialize(object obj, StringBuilder output)
		{
			new TinyJsonSerializer
			{
				output = output
			}.Serialize(obj);
		}

		private void Serialize(object obj)
		{
			if (obj == null)
			{
				this.output.Append("null");
				return;
			}
			Type type = obj.GetType();
			Type typeInfo = WindowsStoreCompatibility.GetTypeInfo(type);
			if (this.serializers.ContainsKey(type))
			{
				this.serializers[type](obj);
			}
			else if (typeInfo.IsEnum)
			{
				this.output.Append('"' + obj.ToString() + '"');
			}
			else if (obj is IList)
			{
				this.output.Append("[");
				IList list = obj as IList;
				for (int i = 0; i < list.Count; i++)
				{
					if (i != 0)
					{
						this.output.Append(", ");
					}
					this.Serialize(list[i]);
				}
				this.output.Append("]");
			}
			else if (obj is UnityEngine.Object)
			{
				this.SerializeUnityObject(obj as UnityEngine.Object);
			}
			else
			{
				bool flag = typeInfo.GetCustomAttributes(typeof(JsonOptInAttribute), true).Length > 0;
				this.output.Append("{");
				FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				bool flag2 = false;
				foreach (FieldInfo fieldInfo in fields)
				{
					if ((!flag && fieldInfo.IsPublic) || fieldInfo.GetCustomAttributes(typeof(JsonMemberAttribute), true).Length > 0)
					{
						if (flag2)
						{
							this.output.Append(", ");
						}
						flag2 = true;
						this.output.AppendFormat("\"{0}\": ", fieldInfo.Name);
						this.Serialize(fieldInfo.GetValue(obj));
					}
				}
				this.output.Append("}");
			}
		}

		private void QuotedField(string name, string contents)
		{
			this.output.AppendFormat("\"{0}\": \"{1}\"", name, contents);
		}

		private void SerializeUnityObject(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				this.Serialize(null);
				return;
			}
			this.output.Append("{");
			this.QuotedField("Name", obj.name);
			this.output.Append(", ");
			this.QuotedField("Type", obj.GetType().FullName);
			Component component = obj as Component;
			GameObject gameObject = obj as GameObject;
			if (component != null || gameObject != null)
			{
				if (component != null && gameObject == null)
				{
					gameObject = component.gameObject;
				}
				UnityReferenceHelper unityReferenceHelper = gameObject.GetComponent<UnityReferenceHelper>();
				if (unityReferenceHelper == null)
				{
					Debug.Log("Adding UnityReferenceHelper to Unity Reference '" + obj.name + "'");
					unityReferenceHelper = gameObject.AddComponent<UnityReferenceHelper>();
				}
				unityReferenceHelper.Reset();
				this.output.Append(", ");
				this.QuotedField("GUID", unityReferenceHelper.GetGUID().ToString());
			}
			this.output.Append("}");
		}

		private StringBuilder output = new StringBuilder();

		private Dictionary<Type, Action<object>> serializers = new Dictionary<Type, Action<object>>();

		private static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;
	}
}
