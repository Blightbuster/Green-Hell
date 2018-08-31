using System;

public class TokenLeftCurlyBracket : Token
{
	public TokenLeftCurlyBracket(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		return base.Check() && this.m_Parser.GetText()[this.m_Parser.Position] == '{';
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		this.m_Parser.Position++;
		return true;
	}
}
