using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuDebugSpawners : MenuDebugScreen
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
				string[] data = (scriptParser.GetKey(i).GetVariablesCount() > 1) ? scriptParser.GetKey(i).GetVariable(1).SValue.Split(new char[]
				{
					','
				}) : new string[0];
				this.m_List.AddElement<string[]>(svalue, data);
			}
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
		if (this.m_ButtonSpawnAndLoad != null)
		{
			this.m_ButtonSpawnAndLoad.gameObject.SetActive(false);
		}
	}

	public void OnButtonSpawn(bool load_scene)
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
				Player.Get().Reposition(array[i].gameObject.transform.position, null);
				MenuInGameManager.Get().ShowPrevoiusScreen();
				return;
			}
		}
	}

	public UIList m_List;

	public Button m_ButtonSpawn;

	public Button m_ButtonSpawnAndLoad;
}
