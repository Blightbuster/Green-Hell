using System;
using System.Reflection;
using UnityEngine;

public class ScenarioCndParam : ScenarioElement
{
	public override void Setup()
	{
		base.Setup();
		string[] array = this.m_EncodedContent.Split(new char[]
		{
			':'
		});
		if (array.Length != 5)
		{
			DebugUtils.Assert(string.Concat(new string[]
			{
				"[ScenarioAction:Setup] Error in element - ",
				this.m_Content,
				", node - ",
				this.m_Node.m_Name,
				". Check spelling!"
			}), true, DebugUtils.AssertType.Info);
		}
		string text = array[1];
		Type type = Type.GetType(text);
		this.m_Object = ScenarioManager.Get().GetObjectOfType(type);
		DebugUtils.Assert(this.m_Object != null, "[ScenarioCndParam:Setup] ERROR - Can't find object " + text, true, DebugUtils.AssertType.Info);
		this.m_Property = type.GetProperty("m_" + array[2]);
		this.m_CndType = (ScenarioCndParam.CndType)Enum.Parse(typeof(ScenarioCndParam.CndType), array[3]);
		if (this.m_Property.PropertyType == typeof(string))
		{
			this.m_Var.SValue = array[4];
			return;
		}
		if (this.m_Property.PropertyType == typeof(float))
		{
			this.m_Var.FValue = float.Parse(array[4]);
			return;
		}
		if (this.m_Property.PropertyType == typeof(bool))
		{
			this.m_Var.BValue = bool.Parse(array[4]);
			return;
		}
		if (this.m_Property.PropertyType == typeof(int))
		{
			this.m_Var.IValue = int.Parse(array[4]);
		}
	}

	protected override bool ShouldComplete()
	{
		switch (this.m_CndType)
		{
		case ScenarioCndParam.CndType.Equal:
			switch (this.m_Var.GetVariableType())
			{
			case CJVariable.TYPE.String:
				return (string)this.m_Property.GetValue(this.m_Object, null) == this.m_Var.SValue;
			case CJVariable.TYPE.Int:
				return (int)this.m_Property.GetValue(this.m_Object, null) == this.m_Var.IValue;
			case CJVariable.TYPE.Float:
				return (float)this.m_Property.GetValue(this.m_Object, null) == this.m_Var.FValue;
			case CJVariable.TYPE.Bool:
				return (bool)this.m_Property.GetValue(this.m_Object, null) == this.m_Var.BValue;
			}
			break;
		case ScenarioCndParam.CndType.Less:
		{
			CJVariable.TYPE variableType = this.m_Var.GetVariableType();
			if (variableType == CJVariable.TYPE.Int)
			{
				return (int)this.m_Property.GetValue(this.m_Object, null) < this.m_Var.IValue;
			}
			if (variableType == CJVariable.TYPE.Float)
			{
				return (float)this.m_Property.GetValue(this.m_Object, null) < this.m_Var.FValue;
			}
			break;
		}
		case ScenarioCndParam.CndType.Greater:
		{
			CJVariable.TYPE variableType = this.m_Var.GetVariableType();
			if (variableType == CJVariable.TYPE.Int)
			{
				return (int)this.m_Property.GetValue(this.m_Object, null) > this.m_Var.IValue;
			}
			if (variableType == CJVariable.TYPE.Float)
			{
				return (float)this.m_Property.GetValue(this.m_Object, null) > this.m_Var.FValue;
			}
			break;
		}
		}
		DebugUtils.Assert(DebugUtils.AssertType.Info);
		return false;
	}

	public override bool IsCondition()
	{
		return true;
	}

	private PropertyInfo m_Property;

	private ScenarioCndParam.CndType m_CndType;

	private CJVariable m_Var = new CJVariable();

	private UnityEngine.Object m_Object;

	private enum CndType
	{
		Equal,
		Less,
		Greater
	}
}
