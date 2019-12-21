using System;
using System.Linq;

public class TokenInt : Token
{
	public TokenInt(ScriptParser parser) : base(parser)
	{
	}

	public override bool TryToGet()
	{
		if (!base.Check())
		{
			return false;
		}
		int num = this.m_Parser.Position;
		bool flag = false;
		char c = this.m_Parser.GetText()[num];
		while (ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[num]) || c == ScriptParser.MINUS)
		{
			if (c == ScriptParser.COMA)
			{
				flag = true;
			}
			num++;
			c = this.m_Parser.GetText()[num];
		}
		if (!flag && num > this.m_Parser.Position)
		{
			this.m_Value = this.m_Parser.GetText().Substring(this.m_Parser.Position, num - this.m_Parser.Position);
			this.m_Parser.Position = num;
			return true;
		}
		return false;
	}
}
