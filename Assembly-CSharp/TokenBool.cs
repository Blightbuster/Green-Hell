using System;
using System.Linq;

public class TokenBool : Token
{
	public TokenBool(ScriptParser parser) : base(parser)
	{
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		int num = this.m_Parser.Position;
		string text = string.Empty;
		while (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[num]))
		{
			num++;
		}
		text = this.m_Parser.GetText().Substring(this.m_Parser.Position, num - this.m_Parser.Position).ToLower();
		if (text == "true" || text == "false")
		{
			this.m_Value = text;
			this.m_Parser.Position = num;
			return true;
		}
		return false;
	}
}
