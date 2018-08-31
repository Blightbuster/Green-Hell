using System;

public class TokenBool : Token
{
	public TokenBool(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		if (!base.Check())
		{
			return false;
		}
		int num = this.m_Parser.Position;
		string text = string.Empty;
		while (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[num].ToString()))
		{
			text += this.m_Parser.GetText()[num].ToString();
			num++;
		}
		text = text.ToLower();
		return text == "true" || text == "false";
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while (ScriptParser.LETTERS.Contains(this.m_Parser.GetText()[this.m_Parser.Position].ToString()))
		{
			this.m_Value += this.m_Parser.GetText()[this.m_Parser.Position];
			this.m_Parser.Position++;
		}
		return true;
	}
}
