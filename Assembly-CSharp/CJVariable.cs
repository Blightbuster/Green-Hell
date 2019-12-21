using System;
using System.IO;

public class CJVariable
{
	public static string TypeToString(CJVariable.TYPE type)
	{
		switch (type)
		{
		case CJVariable.TYPE.String:
			return "String";
		case CJVariable.TYPE.Int:
			return "Int";
		case CJVariable.TYPE.Float:
			return "Float";
		case CJVariable.TYPE.Long:
			return "Long";
		case CJVariable.TYPE.Bool:
			return "Bool";
		default:
			return "Unknown";
		}
	}

	public void Reset()
	{
		this.FValue = 0f;
		this.IValue = 0;
		this.BValue = false;
		this.SValue = string.Empty;
		this.m_Type = CJVariable.TYPE.Unknown;
	}

	public CJVariable.TYPE GetVariableType()
	{
		return this.m_Type;
	}

	public string GetVariableName()
	{
		return this.m_Name;
	}

	public void SetVariableType(CJVariable.TYPE type)
	{
		this.m_Type = type;
	}

	public void SetVariableName(string name)
	{
		this.m_Name = name;
	}

	public CJVariable()
	{
	}

	public CJVariable(float var)
	{
		this.FValue = var;
	}

	public CJVariable(int var)
	{
		this.IValue = var;
	}

	public CJVariable(bool var)
	{
		this.BValue = var;
	}

	public CJVariable(string var)
	{
		this.SValue = var;
	}

	public void Write(StreamWriter stream)
	{
		switch (this.m_Type)
		{
		case CJVariable.TYPE.String:
			stream.Write("\"" + this.SValue + "\"");
			return;
		case CJVariable.TYPE.Int:
			stream.Write(this.IValue.ToString());
			return;
		case CJVariable.TYPE.Float:
			stream.Write(this.FValue.ToString("F4"));
			return;
		case CJVariable.TYPE.Long:
			break;
		case CJVariable.TYPE.Bool:
			stream.Write((this.BValue.ToString() ?? "") ?? "");
			break;
		default:
			return;
		}
	}

	public string SValue
	{
		get
		{
			return this.m_SValue;
		}
		set
		{
			this.m_SValue = value;
			this.m_Type = CJVariable.TYPE.String;
		}
	}

	public int IValue
	{
		get
		{
			return this.m_IValue;
		}
		set
		{
			this.m_IValue = value;
			this.m_Type = CJVariable.TYPE.Int;
		}
	}

	public float FValue
	{
		get
		{
			return this.m_FValue;
		}
		set
		{
			this.m_FValue = value;
			this.m_Type = CJVariable.TYPE.Float;
		}
	}

	public bool BValue
	{
		get
		{
			return this.m_BValue;
		}
		set
		{
			this.m_BValue = value;
			this.m_Type = CJVariable.TYPE.Bool;
		}
	}

	public static string[] GetTypesArray()
	{
		return CJVariable.m_TypesArray;
	}

	public bool Compare(string val, Condition.TYPE comp)
	{
		switch (this.m_Type)
		{
		case CJVariable.TYPE.String:
			if (comp == Condition.TYPE.Equal)
			{
				return this.SValue == val;
			}
			DebugUtils.Assert("Improper conditiotn for String", true, DebugUtils.AssertType.Info);
			return false;
		case CJVariable.TYPE.Int:
			switch (comp)
			{
			case Condition.TYPE.Equal:
				return this.IValue == int.Parse(val);
			case Condition.TYPE.Less:
				return this.IValue < int.Parse(val);
			case Condition.TYPE.Greater:
				return this.IValue > int.Parse(val);
			default:
				DebugUtils.Assert("Improper conditiotn for Int", true, DebugUtils.AssertType.Info);
				return false;
			}
			break;
		case CJVariable.TYPE.Float:
			switch (comp)
			{
			case Condition.TYPE.Equal:
				return this.FValue == float.Parse(val);
			case Condition.TYPE.Less:
				return this.FValue < float.Parse(val);
			case Condition.TYPE.Greater:
				return this.FValue > float.Parse(val);
			default:
				DebugUtils.Assert("Improper conditiotn for Float", true, DebugUtils.AssertType.Info);
				return false;
			}
			break;
		case CJVariable.TYPE.Bool:
			if (comp == Condition.TYPE.Equal)
			{
				return this.BValue == bool.Parse(val);
			}
			DebugUtils.Assert("Improper conditiotn for Bool", true, DebugUtils.AssertType.Info);
			return false;
		}
		DebugUtils.Assert("Improper conditiotn", true, DebugUtils.AssertType.Info);
		return false;
	}

	private string m_SValue;

	private int m_IValue;

	private float m_FValue;

	private bool m_BValue;

	public string m_Name = string.Empty;

	public CJVariable.TYPE m_Type;

	private static string[] m_TypesArray = new string[]
	{
		"Unknown",
		"String",
		"Int",
		"Float",
		"Long",
		"Bool"
	};

	public enum TYPE
	{
		Unknown,
		String,
		Int,
		Float,
		Long,
		Bool
	}
}
