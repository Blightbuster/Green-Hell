using System;
using System.Collections.Generic;
using Enums;

public class Localization
{
	public Localization()
	{
		this.ParseScript();
	}

	public SortedDictionary<string, string> GetLocalizedtexts()
	{
		return this.m_LocalizedTexts;
	}

	private void ParseScript()
	{
		Language language = GreenHellGame.Instance.m_Settings.m_Language;
		this.m_LocalizedTexts = new SortedDictionary<string, string>();
		if (language == Language.English)
		{
			this.ParseScript("Texts_EN");
		}
		else if (language == Language.French)
		{
			this.ParseScript("Texts_FR");
		}
		else if (language == Language.Italian)
		{
			this.ParseScript("Texts_IT");
		}
		else if (language == Language.German)
		{
			this.ParseScript("Texts_DE");
		}
		else if (language == Language.Spanish)
		{
			this.ParseScript("Texts_ES");
		}
		else if (language == Language.ChineseTraditional)
		{
			this.ParseScript("Texts_CHT");
		}
		else if (language == Language.ChineseSimplyfied)
		{
			this.ParseScript("Texts_CHS");
		}
		else if (language == Language.Portuguese)
		{
			this.ParseScript("Texts_PT");
		}
		else if (language == Language.PortugueseBrazilian)
		{
			this.ParseScript("Texts_BR");
		}
		else if (language == Language.Russian)
		{
			this.ParseScript("Texts_RU");
		}
		this.ParseScript("TextsTemp");
	}

	private void ParseScript(string name)
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Localization/" + name + ".txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "String" && !this.m_LocalizedTexts.ContainsKey(key.GetVariable(0).SValue))
			{
				this.m_LocalizedTexts.Add(key.GetVariable(0).SValue, key.GetVariable(1).SValue);
			}
		}
	}

	public bool Contains(string text)
	{
		string empty = string.Empty;
		return this.m_LocalizedTexts.TryGetValue(text, out empty);
	}

	public string Get(string key)
	{
		string result = string.Empty;
		if (!this.m_LocalizedTexts.TryGetValue(key, out result))
		{
			result = key + " = !!!MISSING TEXT!!!";
		}
		return result;
	}

	public void Reload()
	{
		this.ParseScript();
	}

	private SortedDictionary<string, string> m_LocalizedTexts = new SortedDictionary<string, string>();
}
