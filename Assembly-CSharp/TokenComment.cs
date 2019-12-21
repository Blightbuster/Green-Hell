using System;
using System.Linq;

public class TokenComment : Token
{
	public TokenComment(ScriptParser parser) : base(parser)
	{
	}

	protected override bool Check()
	{
		return base.Check() && (this.m_Parser.GetText()[this.m_Parser.Position] == '/' && this.m_Parser.GetText()[this.m_Parser.Position + 1] == '/');
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while ((this.m_Parser.Position < this.m_Parser.GetText().Length && this.m_Parser.GetText()[this.m_Parser.Position] == '/') || this.m_Parser.GetText()[this.m_Parser.Position] == ' ')
		{
			ScriptParser parser = this.m_Parser;
			int position = parser.Position + 1;
			parser.Position = position;
		}
		while (this.m_Parser.Position < this.m_Parser.GetText().Length && !ScriptParser.LINE_END.Contains(this.m_Parser.GetText()[this.m_Parser.Position]))
		{
			ScriptParser parser2 = this.m_Parser;
			int position = parser2.Position + 1;
			parser2.Position = position;
			if (this.m_Parser.Position >= this.m_Parser.GetText().Length)
			{
				break;
			}
		}
		return true;
	}
}
