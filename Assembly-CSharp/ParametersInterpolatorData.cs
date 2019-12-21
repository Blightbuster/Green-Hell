using System;
using System.Reflection;

public class ParametersInterpolatorData<T>
{
	public object m_Object;

	public T m_DefaultValue;

	public T m_TargetValue;

	public FieldInfo m_FieldInfo;

	public PropertyInfo m_PropertyInfo;

	public ParameterInterpolatorDataType m_DataType;
}
