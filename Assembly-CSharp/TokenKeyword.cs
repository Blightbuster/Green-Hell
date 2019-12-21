using System;
using System.Linq;

public class TokenKeyword : Token
{
	public TokenKeyword(ScriptParser parser) : base(parser)
	{
	}

	protected override bool Check()
	{
		return base.Check() && (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[this.m_Parser.Position]) || ScriptParser.UNDERLINE.Equals(this.m_Parser.GetText()[this.m_Parser.Position]));
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		int position = this.m_Parser.Position;
		while (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[this.m_Parser.Position]) || ScriptParser.UNDERLINE.Equals(this.m_Parser.GetText()[this.m_Parser.Position]) || ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[this.m_Parser.Position]))
		{
			ScriptParser parser = this.m_Parser;
			int position2 = parser.Position + 1;
			parser.Position = position2;
		}
		this.m_Value = this.m_Parser.GetText().Substring(position, this.m_Parser.Position - position);
		return true;
	}
}
