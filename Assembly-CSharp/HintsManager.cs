using System;
using System.Collections.Generic;
using UnityEngine;

public class HintsManager : MonoBehaviour, ISaveLoad
{
	public static HintsManager Get()
	{
		return HintsManager.s_Instance;
	}

	private void Awake()
	{
		HintsManager.s_Instance = this;
		this.ParseScript();
	}

	private void Start()
	{
		this.m_HUDHint = (HUDHint)HUDManager.Get().GetHUD(typeof(HUDHint));
		DebugUtils.Assert(this.m_HUDHint, true);
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Hints/Hints", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Hint")
			{
				Hint hint = new Hint();
				hint.m_Name = key.GetVariable(0).SValue;
				hint.m_ShowNTimes = key.GetVariable(1).IValue;
				for (int j = 0; j < key.GetKeysCount(); j++)
				{
					Key key2 = key.GetKey(j);
					if (key2.GetName() == "Text")
					{
						hint.m_Text = key2.GetVariable(0).SValue;
						hint.m_LocalizedText = GreenHellGame.Instance.GetLocalization().Get(hint.m_Text);
					}
					else if (key2.GetName() == "Duration")
					{
						hint.m_Duration = key2.GetVariable(0).FValue;
					}
				}
				this.m_Hints.Add(hint);
			}
		}
	}

	private Hint FindHint(string hint_name)
	{
		for (int i = 0; i < this.m_Hints.Count; i++)
		{
			Hint hint = this.m_Hints[i];
			if (hint.m_Name == hint_name)
			{
				return hint;
			}
		}
		return null;
	}

	public void ShowHint(string hint_name, float duration = 10f)
	{
		if (GreenHellGame.TWITCH_DEMO)
		{
			return;
		}
		Hint hint = this.FindHint(hint_name);
		if (hint == null)
		{
			return;
		}
		if (this.m_HUDHint.IsHint(hint))
		{
			return;
		}
		if (hint.m_ShowNTimes == 0 || hint.m_ShowedNTimes < hint.m_ShowNTimes)
		{
			hint.m_Duration = duration;
			this.m_HUDHint.ShowHint(hint);
			hint.m_ShowedNTimes++;
		}
	}

	public void HideHint(string hint_name)
	{
		Hint hint = this.FindHint(hint_name);
		if (hint == null)
		{
			return;
		}
		this.m_HUDHint.HideHint(hint);
	}

	public void HideAllHints()
	{
		for (int i = 0; i < this.m_Hints.Count; i++)
		{
			this.m_HUDHint.HideHint(this.m_Hints[i]);
		}
	}

	public void Save()
	{
		for (int i = 0; i < this.m_Hints.Count; i++)
		{
			SaveGame.SaveVal("Hints" + this.m_Hints[i].m_Name, this.m_Hints[i].m_ShowedNTimes);
		}
	}

	public void Load()
	{
		for (int i = 0; i < this.m_Hints.Count; i++)
		{
			int showedNTimes = 0;
			SaveGame.LoadVal("Hints" + this.m_Hints[i].m_Name, out showedNTimes, false);
			this.m_Hints[i].m_ShowedNTimes = showedNTimes;
		}
	}

	private List<Hint> m_Hints = new List<Hint>();

	private HUDHint m_HUDHint;

	private static HintsManager s_Instance;
}
