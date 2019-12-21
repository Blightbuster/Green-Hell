using System;
using System.Linq;

public class TokenNewline : Token
{
	public TokenNewline(ScriptParser parser) : base(parser)
	{
	}

	protected override bool Check()
	{
		return base.Check() && ScriptParser.LINE_END.Contains(this.m_Parser.GetText()[this.m_Parser.Position]);
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while (this.m_Parser.Position < this.m_Parser.GetText().Length && ScriptParser.LINE_END.Contains(this.m_Parser.GetText()[this.m_Parser.Position]))
		{
			ScriptParser parser = this.m_Parser;
			int position = parser.Position + 1;
			parser.Position = position;
		}
		return true;
	}
}
