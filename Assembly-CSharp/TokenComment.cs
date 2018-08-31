using System;

public class TokenComment : Token
{
	public TokenComment(ScriptParser parser) : base(parser)
	{
	}

	public override bool Check()
	{
		return base.Check() && (this.m_Parser.GetText()[this.m_Parser.Position] == '/' && this.m_Parser.GetText()[this.m_Parser.Position + 1] == '/');
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		while (this.m_Parser.GetText()[this.m_Parser.Position].ToString().IndexOfAny(ScriptParser.LINE_END) != 0)
		{
			this.m_Value += this.m_Parser.GetText()[this.m_Parser.Position];
			this.m_Parser.Position++;
			if (this.m_Parser.Position >= this.m_Parser.GetText().Length)
			{
				break;
			}
		}
		return true;
	}
}
