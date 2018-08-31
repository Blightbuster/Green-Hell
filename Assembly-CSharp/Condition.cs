using System;

public class Condition
{
	public Condition()
	{
	}

	public Condition(CJVariable var, Condition.TYPE type)
	{
		this.m_Variable = var;
		this.m_Type = type;
	}

	public static string ToString(Condition.TYPE type)
	{
		switch (type)
		{
		case Condition.TYPE.Equal:
			return "Equal";
		case Condition.TYPE.Less:
			return "Less";
		case Condition.TYPE.Greater:
			return "Greater";
		default:
			return "Unknown";
		}
	}

	public bool Compare(string value)
	{
		return this.m_Variable != null && this.m_Variable.Compare(value, this.m_Type);
	}

	public bool Compare()
	{
		return this.m_Variable != null && this.m_ToCompare != null && this.m_ToCompare.Length != 0 && this.m_Variable.Compare(this.m_ToCompare, this.m_Type);
	}

	public void SetVariable(CJVariable var)
	{
		this.m_Variable = var;
	}

	public CJVariable GetVariable()
	{
		return this.m_Variable;
	}

	public void SetConditionType(Condition.TYPE type)
	{
		this.m_Type = type;
	}

	public Condition.TYPE GetConditionType()
	{
		return this.m_Type;
	}

	private CJVariable m_Variable;

	private Condition.TYPE m_Type;

	public string m_ToCompare = string.Empty;

	public enum TYPE
	{
		Unknown,
		Equal,
		Less,
		Greater,
		Count
	}
}
