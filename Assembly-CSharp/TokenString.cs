using System;

public class TokenString : Token
{
	public TokenString(ScriptParser parser) : base(parser)
	{
	}

	protected override bool Check()
	{
		return base.Check() && this.m_Parser.GetText()[this.m_Parser.Position] == '"';
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		ScriptParser parser = this.m_Parser;
		int position = parser.Position + 1;
		parser.Position = position;
		int position2 = this.m_Parser.Position;
		while (this.m_Parser.Position < this.m_Parser.GetText().Length && this.m_Parser.GetText()[this.m_Parser.Position] != '"')
		{
			ScriptParser parser2 = this.m_Parser;
			position = parser2.Position + 1;
			parser2.Position = position;
		}
		this.m_Value = this.m_Parser.GetText().Substring(position2, this.m_Parser.Position - position2);
		ScriptParser parser3 = this.m_Parser;
		position = parser3.Position + 1;
		parser3.Position = position;
		return true;
	}
}
