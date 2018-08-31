using System;
using System.Reflection;
using UnityEngine;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectField
	{
		public VolumeEffectField(string fieldName, string fieldType)
		{
			this.fieldName = fieldName;
			this.fieldType = fieldType;
		}

		public VolumeEffectField(FieldInfo pi, Component c) : this(pi.Name, pi.FieldType.FullName)
		{
			object value = pi.GetValue(c);
			this.UpdateValue(value);
		}

		public static bool IsValidType(string type)
		{
			if (type != null)
			{
				if (type == "System.Single" || type == "System.Boolean" || type == "UnityEngine.Color" || type == "UnityEngine.Vector2" || type == "UnityEngine.Vector3" || type == "UnityEngine.Vector4")
				{
					return true;
				}
			}
			return false;
		}

		public void UpdateValue(object val)
		{
			string text = this.fieldType;
			if (text != null)
			{
				if (!(text == "System.Single"))
				{
					if (!(text == "System.Boolean"))
					{
						if (!(text == "UnityEngine.Color"))
						{
							if (!(text == "UnityEngine.Vector2"))
							{
								if (!(text == "UnityEngine.Vector3"))
								{
									if (text == "UnityEngine.Vector4")
									{
										this.valueVector4 = (Vector4)val;
									}
								}
								else
								{
									this.valueVector3 = (Vector3)val;
								}
							}
							else
							{
								this.valueVector2 = (Vector2)val;
							}
						}
						else
						{
							this.valueColor = (Color)val;
						}
					}
					else
					{
						this.valueBoolean = (bool)val;
					}
				}
				else
				{
					this.valueSingle = (float)val;
				}
			}
		}

		public string fieldName;

		public string fieldType;

		public float valueSingle;

		public Color valueColor;

		public bool valueBoolean;

		public Vector2 valueVector2;

		public Vector3 valueVector3;

		public Vector4 valueVector4;
	}
}
