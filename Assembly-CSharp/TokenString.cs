using System;

public class TokenString : Token
{
	public TokenString(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		return base.Check() && this.m_Parser.GetText()[this.m_Parser.Position] == '"';
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		this.m_Parser.Position++;
		while (this.m_Parser.Position < this.m_Parser.GetText().Length && this.m_Parser.GetText()[this.m_Parser.Position] != '"')
		{
			this.m_Value += this.m_Parser.GetText()[this.m_Parser.Position];
			this.m_Parser.Position++;
		}
		this.m_Parser.Position++;
		return true;
	}
}
