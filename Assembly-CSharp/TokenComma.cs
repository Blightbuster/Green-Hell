﻿using System;

public class TokenComma : Token
{
	public TokenComma(ScriptParser parser) : base(parser)
	{
	}

	protected override bool Check()
	{
		return base.Check() && this.m_Parser.GetText()[this.m_Parser.Position] == ',';
	}

	public override bool TryToGet()
	{
		if (!this.Check())
		{
			return false;
		}
		ScriptParser parser = this.m_Parser;
		int position = parser.Position + 1;
		parser.Position = position;
		return true;
	}
}
