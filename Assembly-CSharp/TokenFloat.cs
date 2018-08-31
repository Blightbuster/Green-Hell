using System;

public class TokenFloat : Token
{
	public TokenFloat(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		if (!base.Check())
		{
			return false;
		}
		int num = this.m_Parser.Position;
		bool result = false;
		while (ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[num].ToString()) || this.m_Parser.GetText()[num] == ScriptParser.PERIOD || this.m_Parser.GetText()[num] == ScriptParser.MINUS)
		{
			if (this.m_Parser.GetText()[num] == ScriptParser.PERIOD)
			{
				result = true;
			}
			num++;
		}
		return result;
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while (ScriptParser.DIGITS.Contains(this.m_Parser.GetText()[this.m_Parser.Position].ToString()) || this.m_Parser.GetText()[this.m_Parser.Position] == ScriptParser.PERIOD || this.m_Parser.GetText()[this.m_Parser.Position] == ScriptParser.MINUS)
		{
			this.m_Value += this.m_Parser.GetText()[this.m_Parser.Position];
			this.m_Parser.Position++;
		}
		return true;
	}
}
