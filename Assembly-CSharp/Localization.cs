using System;
using System.Collections.Generic;
using System.Text;
using Enums;
using UnityEngine;

public class Localization
{
	public SortedDictionary<string, string> GetLocalizedtexts()
	{
		return this.m_LocalizedTexts;
	}

	public Localization()
	{
		this.ParseScript();
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
		else if (language == Language.Hungarian)
		{
			this.ParseScript("Texts_HU");
		}
		this.ParseScript("TextsTemp");
	}

	private void ParseScript(string name)
	{
		ScriptParser scriptParser = new ScriptParser();
		bool flag = true;
		bool flag2;
		if (flag)
		{
			flag2 = scriptParser.Parse("Localization/" + name + ".txt", flag);
		}
		else
		{
			flag2 = scriptParser.Parse("Resources/Scripts/Localization/" + name + ".txt", flag);
		}
		if (!flag2)
		{
			scriptParser.Parse("Localization/Texts_EN.txt", true);
		}
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
		return this.m_LocalizedTexts.ContainsKey(text);
	}

	public string Get(string key, bool replace = true)
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
		if (replace)
		{
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
				InputsManager.InputAction input_action = InputsManager.InputAction.SkipCutscene;
				TriggerAction.TYPE trigger_action = TriggerAction.TYPE.Take;
				bool flag3 = false;
				bool flag4 = false;
				if (Enum.TryParse<InputsManager.InputAction>(text3, true, out input_action))
				{
					flag3 = true;
				}
				else
				{
					flag2 = false;
				}
				if (!flag2)
				{
					if (Enum.TryParse<TriggerAction.TYPE>(text3, true, out trigger_action))
					{
						flag4 = true;
					}
					else
					{
						flag2 = false;
					}
				}
				if (flag2)
				{
					string str = string.Empty;
					if (flag3)
					{
						InputActionData actionDataByInputAction = InputsManager.Get().GetActionDataByInputAction(input_action, ControllerType._Count);
						if (actionDataByInputAction == null)
						{
							actionDataByInputAction = InputsManager.Get().GetActionDataByInputAction(input_action, (GreenHellGame.Instance.m_Settings.m_ControllerType == ControllerType.Pad) ? ControllerType.PC : ControllerType.Pad);
						}
						KeyCode keyCode = (actionDataByInputAction != null) ? actionDataByInputAction.m_KeyCode : KeyCode.None;
						str = ((keyCode != KeyCode.None) ? KeyCodeToString.GetString(keyCode) : text3);
					}
					else if (flag4)
					{
						InputActionData actionDataByTriggerAction = InputsManager.Get().GetActionDataByTriggerAction(trigger_action, ControllerType._Count);
						str = KeyCodeToString.GetString((actionDataByTriggerAction != null) ? actionDataByTriggerAction.m_KeyCode : KeyCode.None);
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
		}
		return text;
	}

	public string GetMixed(string key, params string[] replacements)
	{
		string text = this.Get(key, true);
		string[] array = text.Split(new string[]
		{
			"%s"
		}, StringSplitOptions.None);
		if (array.Length <= 1)
		{
			return text;
		}
		int i = 0;
		StringBuilder stringBuilder = new StringBuilder(array[i++]);
		while (i < array.Length)
		{
			if (i - 1 < replacements.Length)
			{
				stringBuilder.Append(this.Contains(replacements[i - 1]) ? this.Get(replacements[i - 1], true) : replacements[i - 1]);
			}
			stringBuilder.Append(array[i]);
			i++;
		}
		return stringBuilder.ToString();
	}

	public void Reload()
	{
		this.ParseScript();
	}

	private SortedDictionary<string, string> m_LocalizedTexts = new SortedDictionary<string, string>();
}
