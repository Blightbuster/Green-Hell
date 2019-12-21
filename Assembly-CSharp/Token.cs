using System;

public class Token
{
	public Token(ScriptParser parser)
	{
		this.m_Parser = parser;
	}

	protected virtual bool Check()
	{
		return !this.m_Parser.Equals(null) && this.m_Parser.Position < this.m_Parser.GetText().Length;
	}

	public virtual bool TryToGet()
	{
		return this.m_Parser.Equals(null);
	}

	public string GetValue()
	{
		return this.m_Value;
	}

	protected ScriptParser m_Parser;

	protected string m_Value = string.Empty;
}
