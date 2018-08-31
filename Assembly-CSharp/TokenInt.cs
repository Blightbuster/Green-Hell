using System;

public class TokenInt : Token
{
	public TokenInt(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		if (!base.Check())
		{
			return false;
		}
		int num = this.m_Parser.Position;
		bool flag = false;
		char c = this.m_Parser.GetText()[num];
		while (ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[num].ToString()) || c == ScriptParser.MINUS)
		{
			if (c == ScriptParser.COMA)
			{
				flag = true;
			}
			num++;
			c = this.m_Parser.GetText()[num];
		}
		return !flag && num > this.m_Parser.Position;
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while (ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[this.m_Parser.Position].ToString()) || this.m_Parser.GetText()[this.m_Parser.Position] == ScriptParser.MINUS)
		{
			this.m_Value += this.m_Parser.GetText()[this.m_Parser.Position];
			this.m_Parser.Position++;
		}
		return true;
	}
}
