using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

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
		else if (language == Language.Polish)
		{
			this.ParseScript("Texts_PL");
		}
		else if (language == Language.Japanese)
		{
			this.ParseScript("Texts_JP");
		}
		else if (language == Language.Korean)
		{
			this.ParseScript("Texts_KO");
		}
		else if (language == Language.Vietnamese)
		{
			this.ParseScript("Texts_VN");
		}
		else if (language == Language.Thai)
		{
			this.ParseScript("Texts_TH");
		}
		else if (language == Language.Czech)
		{
			this.ParseScript("Texts_CZ");
		}
		else if (language == Language.Swedish)
		{
			this.ParseScript("Texts_SW");
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
		string text = string.Empty;
		if (!this.m_LocalizedTexts.TryGetValue(key, out text))
		{
			text = key + " = !!!MISSING TEXT!!!";
			return text;
		}
		if (text.Length == 0)
		{
			return text;
		}
		int i = 0;
		int num = 0;
		int num2 = 0;
		i = text.IndexOf("[", i);
		string text2 = text;
		bool flag = false;
		while (i >= 0)
		{
			flag = true;
			num = text2.IndexOf("]", i);
			if (num2 == 0)
			{
				text = text2.Substring(num2, i - num2 + 1);
			}
			else
			{
				text += text2.Substring(num2 + 1, i - num2);
			}
			string text3 = text2.Substring(i + 1, num - i - 1);
			bool flag2 = true;
			InputsManager.InputAction key2 = InputsManager.InputAction.SkipCutscene;
			TriggerAction.TYPE key3 = TriggerAction.TYPE.Take;
			bool flag3 = false;
			bool flag4 = false;
			try
			{
				key2 = (InputsManager.InputAction)Enum.Parse(typeof(InputsManager.InputAction), text3);
				flag3 = true;
			}
			catch (ArgumentException)
			{
				flag2 = false;
			}
			if (!flag2)
			{
				try
				{
					key3 = (TriggerAction.TYPE)Enum.Parse(typeof(TriggerAction.TYPE), text3);
					flag4 = true;
				}
				catch (ArgumentException)
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				string str = string.Empty;
				if (flag3)
				{
					KeyCode keyCode = InputsManager.Get().GetActionsByInputAction()[(int)key2].m_KeyCode;
					str = KeyCodeToString.GetString(keyCode);
				}
				else if (flag4)
				{
					KeyCode keyCode2 = InputsManager.Get().GetActionsByTriggerAction()[(int)key3].m_KeyCode;
					str = KeyCodeToString.GetString(keyCode2);
				}
				text = text + str + "]";
			}
			else
			{
				text = text + text3 + "]";
			}
			num2 = num;
			i = text2.IndexOf("[", num2);
		}
		if (flag)
		{
			text += text2.Substring(num + 1, text2.Length - num - 1);
		}
		return text;
	}

	public void Reload()
	{
		this.ParseScript();
	}

	private SortedDictionary<string, string> m_LocalizedTexts = new SortedDictionary<string, string>();
}
