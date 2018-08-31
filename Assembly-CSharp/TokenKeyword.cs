using System;

public class TokenKeyword : Token
{
	public TokenKeyword(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		return base.Check() && (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[this.m_Parser.Position].ToString()) || ScriptParser.UNDERLINE.Equals(this.m_Parser.GetText()[this.m_Parser.Position]));
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[this.m_Parser.Position].ToString()) || ScriptParser.UNDERLINE.Equals(this.m_Parser.GetText()[this.m_Parser.Position]) || ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[this.m_Parser.Position].ToString()))
		{
			this.m_Value += this.m_Parser.GetText()[this.m_Parser.Position];
			this.m_Parser.Position++;
		}
		return true;
	}
}
