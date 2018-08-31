using System;
using UnityEngine;

public class TextAssetParser : ScriptParser
{
	public TextAssetParser()
	{
	}

	public TextAssetParser(TextAsset asset)
	{
		this.Parse(asset);
	}

	public void Parse(TextAsset asset)
	{
		if (asset == null)
		{
			DebugUtils.Assert("[TextAssetParser::Parse] Error - asset is not set!", true, DebugUtils.AssertType.Info);
			return;
		}
		this.m_Text = asset.text;
		base.ParseTokens();
		base.ProcessTokens();
	}
}
