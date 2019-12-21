using System;
using System.Linq;

public class TokenFloat : Token
{
	public TokenFloat(ScriptParser parser) : base(parser)
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
		while (ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[num]) || this.m_Parser.GetText()[num] == ScriptParser.PERIOD || this.m_Parser.GetText()[num] == ScriptParser.MINUS)
		{
			if (this.m_Parser.GetText()[num] == ScriptParser.PERIOD)
			{
				flag = true;
			}
			num++;
		}
		if (flag)
		{
			this.m_Value = this.m_Parser.GetText().Substring(this.m_Parser.Position, num - this.m_Parser.Position);
			this.m_Parser.Position = num;
			return true;
		}
		return false;
	}
}
