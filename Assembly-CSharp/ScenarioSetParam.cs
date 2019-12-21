using System;
using System.Reflection;
using UnityEngine;

public class ScenarioSetParam : ScenarioElement
{
	public override void Setup()
	{
		base.Setup();
		this.m_PerformOnLoad = this.m_ScenarioSyntaxData.m_PerformOnLoad;
		string[] array = this.m_EncodedContent.Split(new char[]
		{
			':'
		});
		if (array.Length != 4)
		{
			DebugUtils.Assert(string.Concat(new string[]
			{
				"[ScenarioSetParam:Setup] Error in element - ",
				this.m_Content,
				", node - ",
				this.m_Node.m_Name,
				". Check spelling!"
			}), true, DebugUtils.AssertType.Info);
		}
		string text = array[1];
		Type type = Type.GetType(text);
		this.m_Object = ScenarioManager.Get().GetObjectOfType(type);
		DebugUtils.Assert(this.m_Object != null, "[ScenarioSetParam:Setup] ERROR - Can't find object " + text, true, DebugUtils.AssertType.Info);
		this.m_Property = type.GetProperty("m_" + array[2]);
		if (this.m_Property.PropertyType == typeof(string))
		{
			this.m_Var.SValue = array[3];
			return;
		}
		if (this.m_Property.PropertyType == typeof(float))
		{
			this.m_Var.FValue = float.Parse(array[3]);
			return;
		}
		if (this.m_Property.PropertyType == typeof(bool))
		{
			this.m_Var.BValue = bool.Parse(array[3]);
			return;
		}
		if (this.m_Property.PropertyType == typeof(int))
		{
			this.m_Var.IValue = int.Parse(array[3]);
		}
	}

	public override bool IsAction()
	{
		return true;
	}

	public override void Activate()
	{
		base.Activate();
		switch (this.m_Var.GetVariableType())
		{
		case CJVariable.TYPE.String:
			this.m_Property.SetValue(this.m_Object, this.m_Var.SValue);
			break;
		case CJVariable.TYPE.Int:
			this.m_Property.SetValue(this.m_Object, this.m_Var.IValue);
			break;
		case CJVariable.TYPE.Float:
			this.m_Property.SetValue(this.m_Object, this.m_Var.FValue);
			break;
		case CJVariable.TYPE.Bool:
			this.m_Property.SetValue(this.m_Object, this.m_Var.BValue);
			break;
		}
		base.Complete();
	}

	protected override bool ShouldComplete()
	{
		return true;
	}

	private bool m_PerformOnLoad = true;

	private PropertyInfo m_Property;

	private CJVariable m_Var = new CJVariable();

	private UnityEngine.Object m_Object;
}
