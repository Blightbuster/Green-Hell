using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuDebugSpawners : MenuScreen
{
	protected override void Awake()
	{
		base.Awake();
		this.ParseScript();
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Scenario/ScenarioDebugSpawners.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			if (scriptParser.GetKey(i).GetName() == "DebugSpawner")
			{
				string svalue = scriptParser.GetKey(i).GetVariable(0).SValue;
				this.m_List.AddElement(svalue, -1);
			}
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
	}

	public void OnButtonSpawn()
	{
		string selectedElementText = this.m_List.GetSelectedElementText();
		if (selectedElementText.Length == 0)
		{
			return;
		}
		DebugSpawner[] array = UnityEngine.Object.FindObjectsOfType<DebugSpawner>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.name == selectedElementText)
			{
				Player.Get().gameObject.transform.position = array[i].gameObject.transform.position;
				MenuInGameManager.Get().ShowPrevoiusScreen();
				return;
			}
		}
	}

	public UIList m_List;

	public Button m_ButtonSpawn;
}
