using System;
using System.Collections.Generic;
using System.Reflection;
using CJTools;
using UnityEngine;

public class ParametersInterpolator : MonoBehaviour
{
	public ParametersInterpolator()
	{
		ParametersInterpolator.s_Interpolators.Add(this);
	}

	private void OnDestroy()
	{
		ParametersInterpolator.s_Interpolators.Remove(this);
	}

	public static ParametersInterpolator GetInterpolator(string commander_name)
	{
		int i = 0;
		while (i < ParametersInterpolator.s_Interpolators.Count)
		{
			if (ParametersInterpolator.s_Interpolators[i].Equals(null))
			{
				ParametersInterpolator.s_Interpolators.RemoveAt(i);
			}
			else
			{
				if (ParametersInterpolator.s_Interpolators[i].gameObject.name == commander_name)
				{
					return ParametersInterpolator.s_Interpolators[i];
				}
				i++;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		this.SetupData();
	}

	public void SetupData()
	{
		if (this.m_ComponentName.Length < 1)
		{
			return;
		}
		this.m_FProperties.Clear();
		for (int i = 0; i < this.m_FFieldNames.Count; i++)
		{
			object obj = base.GetComponent(this.m_ComponentName);
			if (obj == null)
			{
				return;
			}
			FieldInfo fieldInfo = null;
			PropertyInfo propertyInfo = null;
			string text = this.m_FFieldNames[i];
			if (text.Contains(";"))
			{
				string[] array = text.Split(new char[]
				{
					';'
				});
				for (int j = 0; j < array.Length; j++)
				{
					fieldInfo = obj.GetType().GetField(array[j]);
					if (!(fieldInfo == null))
					{
						object value = fieldInfo.GetValue(obj);
						if (!value.IsNumber())
						{
							obj = value;
						}
					}
				}
			}
			else
			{
				fieldInfo = obj.GetType().GetField(text);
				if (fieldInfo == null)
				{
					MemberInfo[] member = obj.GetType().GetMember(text);
					if (member != null)
					{
						foreach (MemberInfo memberInfo in member)
						{
							if (memberInfo.MemberType == MemberTypes.Property)
							{
								propertyInfo = (PropertyInfo)memberInfo;
							}
						}
					}
				}
			}
			if ((!(fieldInfo == null) || !(propertyInfo == null)) && ((fieldInfo != null && fieldInfo.FieldType == typeof(float)) || (propertyInfo != null && propertyInfo.PropertyType == typeof(float))))
			{
				if (i >= this.m_FFieldValues.Count)
				{
					this.m_FFieldValues.Add(0f);
				}
				ParametersInterpolatorData<float> parametersInterpolatorData = new ParametersInterpolatorData<float>();
				if (fieldInfo != null)
				{
					parametersInterpolatorData.m_DefaultValue = (float)fieldInfo.GetValue(obj);
				}
				else if (propertyInfo != null)
				{
					parametersInterpolatorData.m_DefaultValue = (float)propertyInfo.GetValue(obj);
				}
				parametersInterpolatorData.m_FieldInfo = fieldInfo;
				parametersInterpolatorData.m_PropertyInfo = propertyInfo;
				parametersInterpolatorData.m_Object = obj;
				parametersInterpolatorData.m_TargetValue = this.m_FFieldValues[i];
				if (propertyInfo != null)
				{
					parametersInterpolatorData.m_DataType = ParameterInterpolatorDataType.Property;
				}
				this.m_FProperties.Add(parametersInterpolatorData);
			}
		}
	}

	public void OnFFieldNameChanged(int idx, string field_name)
	{
		this.m_FFieldNames[idx] = field_name;
		this.SetupData();
	}

	public void OnFFieldValueChanged(int idx, float val)
	{
		this.m_FFieldValues[idx] = val;
		this.SetupData();
	}

	public void OnNumFFieldsChanged(int new_size)
	{
		List<string> ffieldNames = this.m_FFieldNames;
		this.m_FFieldNames = new List<string>();
		for (int i = 0; i < new_size; i++)
		{
			if (i < ffieldNames.Count)
			{
				this.m_FFieldNames.Add(ffieldNames[i]);
			}
			else
			{
				this.m_FFieldNames.Add(string.Empty);
			}
		}
		List<float> ffieldValues = this.m_FFieldValues;
		this.m_FFieldValues = new List<float>();
		for (int j = 0; j < new_size; j++)
		{
			if (j < ffieldValues.Count)
			{
				this.m_FFieldValues.Add(ffieldValues[j]);
			}
			else
			{
				this.m_FFieldValues.Add(1f);
			}
		}
	}

	public void OnComponentNameChanged(string component_name)
	{
		this.m_ComponentName = component_name;
		this.SetupData();
	}

	public bool FieldExists(string field_name)
	{
		for (int i = 0; i < this.m_FProperties.Count; i++)
		{
			if (this.m_FProperties[i].m_DataType == ParameterInterpolatorDataType.Field)
			{
				if (this.m_FProperties[i].m_FieldInfo.Name == field_name)
				{
					return true;
				}
			}
			else if (this.m_FProperties[i].m_PropertyInfo.Name == field_name)
			{
				return true;
			}
		}
		return false;
	}

	public void SetWeight(float weight)
	{
		for (int i = 0; i < this.m_FProperties.Count; i++)
		{
			ParametersInterpolatorData<float> parametersInterpolatorData = this.m_FProperties[i];
			float proportional = CJTools.Math.GetProportional(parametersInterpolatorData.m_DefaultValue, parametersInterpolatorData.m_TargetValue, weight, 0f, 1f);
			if (parametersInterpolatorData.m_DataType == ParameterInterpolatorDataType.Field && parametersInterpolatorData != null && parametersInterpolatorData.m_FieldInfo != null && parametersInterpolatorData.m_Object != null)
			{
				parametersInterpolatorData.m_FieldInfo.SetValue(parametersInterpolatorData.m_Object, proportional);
			}
			else if (parametersInterpolatorData != null && parametersInterpolatorData.m_PropertyInfo != null && parametersInterpolatorData.m_Object != null)
			{
				parametersInterpolatorData.m_PropertyInfo.SetValue(parametersInterpolatorData.m_Object, proportional);
			}
		}
	}

	public string m_ComponentName = string.Empty;

	public List<string> m_FFieldNames = new List<string>();

	public List<float> m_FFieldValues = new List<float>();

	public List<ParametersInterpolatorData<float>> m_FProperties = new List<ParametersInterpolatorData<float>>();

	private static List<ParametersInterpolator> s_Interpolators = new List<ParametersInterpolator>();
}
