using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenario
{
	public static Scenario Get()
	{
		return Scenario.s_Instance;
	}

	public Scenario()
	{
		Scenario.s_Instance = this;
	}

	private string GetScriptName()
	{
		if (GreenHellGame.TWITCH_DEMO)
		{
			return Scenario.s_TwitchDemoScript;
		}
		if (ChallengesManager.Get() && ChallengesManager.Get().IsChallengeActive())
		{
			return Scenario.s_ChallengeScript;
		}
		if (MainLevel.Instance.m_GameMode == GameMode.Story)
		{
			return Scenario.s_StoryScript;
		}
		if (MainLevel.Instance.m_GameMode == GameMode.Survival)
		{
			return Scenario.s_SurvivalScript;
		}
		return Scenario.s_StoryScript;
	}

	public void LoadScript()
	{
		this.m_FilesToInclude.Clear();
		this.LoadScript(this.GetScriptName());
		for (int i = 0; i < this.m_FilesToInclude.Count; i++)
		{
			this.LoadScript(this.m_FilesToInclude[i]);
		}
	}

	public void LoadScript(string script_name)
	{
		if (MainLevel.Instance.m_GameMode == GameMode.Debug)
		{
			return;
		}
		TextAssetParser textAssetParser = new TextAssetParser(Resources.Load(Scenario.s_ScriptPath + script_name) as TextAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Node")
			{
				ScenarioNode scenarioNode = new ScenarioNode();
				scenarioNode.Load(key);
				this.AddNode(scenarioNode);
			}
		}
	}

	public void AddNode(ScenarioNode node)
	{
		this.m_Nodes.Add(node);
	}

	public void RemoveNode(ScenarioNode node)
	{
		this.m_Nodes.Remove(node);
	}

	public void SetupParents()
	{
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			scenarioNode.m_Parents.Clear();
			if (scenarioNode.m_ParentNames != null && scenarioNode.m_ParentNames != string.Empty)
			{
				string[] array = scenarioNode.m_ParentNames.Split(new char[]
				{
					':'
				});
				for (int i = 0; i < array.Length; i++)
				{
					ScenarioNode node = this.GetNode(array[i]);
					if (node == null)
					{
						DebugUtils.Assert(DebugUtils.AssertType.Info);
					}
					else
					{
						scenarioNode.m_Parents.Add(node);
						node.m_Childs.Add(scenarioNode);
					}
				}
			}
		}
	}

	public void SetupNodes()
	{
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			scenarioNode.Setup();
			if (scenarioNode.m_State == ScenarioNode.State.None && scenarioNode.m_Parents.Count == 0)
			{
				scenarioNode.Activate();
			}
		}
	}

	public void Update()
	{
		if (LoadingScreen.Get() != null && LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (MainLevel.Instance == null || MainLevel.Instance.IsPause())
		{
			return;
		}
		for (int i = 0; i < this.m_Nodes.Count; i++)
		{
			if (this.m_Nodes[i].IsActive())
			{
				this.m_Nodes[i].Update();
			}
		}
	}

	public ScenarioNode GetNode(string node_name)
	{
		int num = Animator.StringToHash(node_name);
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			if (scenarioNode.m_NameHash == num)
			{
				return scenarioNode;
			}
		}
		return null;
	}

	public ScenarioNode GetNode(int index)
	{
		if (index < 0 || index >= this.m_Nodes.Count)
		{
			return null;
		}
		return this.m_Nodes[index];
	}

	public List<ScenarioNode> GetAllNodes()
	{
		return this.m_Nodes;
	}

	public void DeactivateNode(string node_name)
	{
		ScenarioNode node = this.GetNode(node_name);
		if (node != null)
		{
			node.Deactivate();
		}
	}

	public void ActivateNode(string node_name)
	{
		ScenarioNode node = this.GetNode(node_name);
		if (node != null && !node.IsCompleted())
		{
			node.Activate();
		}
	}

	public void ExecuteNode(string node_name)
	{
		ScenarioNode node = this.GetNode(node_name);
		if (node != null)
		{
			node.Reset();
			node.Activate();
			node.Update();
		}
	}

	public void RestartNode(string node_name)
	{
		ScenarioNode node = this.GetNode(node_name);
		if (node != null)
		{
			node.Reset();
			node.Activate();
		}
	}

	public bool IsNodeCompleted(string node_name)
	{
		ScenarioNode node = this.GetNode(node_name);
		return node != null && node.m_State == ScenarioNode.State.Completed;
	}

	public void Save()
	{
		this.m_ActionObjectsToSave.Clear();
		this.m_ActionComponentToSave.Clear();
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			scenarioNode.Save();
		}
		foreach (GameObject gameObject in this.m_ActionObjectsToSave)
		{
			if (gameObject)
			{
				SaveGame.SaveVal("AO" + gameObject.name, gameObject.activeSelf);
			}
		}
		foreach (GameObject gameObject2 in this.m_ActionComponentToSave.Keys)
		{
			if (gameObject2)
			{
				if (this.m_ActionComponentToSave[gameObject2].Contains("Collider"))
				{
					Collider collider = gameObject2.GetComponent(this.m_ActionComponentToSave[gameObject2]) as Collider;
					SaveGame.SaveVal("AOC" + gameObject2.name, collider.enabled);
				}
				else
				{
					Behaviour behaviour = gameObject2.GetComponent(this.m_ActionComponentToSave[gameObject2]) as Behaviour;
					SaveGame.SaveVal("AOC" + gameObject2.name, behaviour.enabled);
				}
			}
		}
		this.SaveBoolVariables();
		this.SaveIntVariables();
	}

	public void Load()
	{
		ScenarioManager.Get().ResetVariables();
		this.m_ActionObjectsToLoad.Clear();
		this.m_ActionComponentToLoad.Clear();
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			scenarioNode.Reset();
		}
		this.m_IsLoading = true;
		foreach (ScenarioNode scenarioNode2 in this.m_Nodes)
		{
			scenarioNode2.Load();
			if (scenarioNode2.m_State == ScenarioNode.State.None && scenarioNode2.m_Parents.Count == 0)
			{
				scenarioNode2.Activate();
			}
		}
		foreach (GameObject gameObject in this.m_ActionObjectsToLoad)
		{
			if (gameObject)
			{
				gameObject.SetActive(SaveGame.LoadBVal("AO" + gameObject.name));
			}
		}
		foreach (GameObject gameObject2 in this.m_ActionComponentToLoad.Keys)
		{
			if (gameObject2)
			{
				if (this.m_ActionComponentToLoad[gameObject2].Contains("Collider"))
				{
					(gameObject2.GetComponent(this.m_ActionComponentToLoad[gameObject2]) as Collider).enabled = SaveGame.LoadBVal("AOC" + gameObject2.name);
				}
				else
				{
					(gameObject2.GetComponent(this.m_ActionComponentToLoad[gameObject2]) as Behaviour).enabled = SaveGame.LoadBVal("AOC" + gameObject2.name);
				}
			}
		}
		foreach (ScenarioNode scenarioNode3 in this.m_Nodes)
		{
			if (scenarioNode3.m_State != ScenarioNode.State.Inactive && scenarioNode3.m_State != ScenarioNode.State.None)
			{
				foreach (ScenarioElement scenarioElement in scenarioNode3.m_Elements)
				{
					scenarioElement.PostLoad();
				}
			}
		}
		this.LoadBoolvariables();
		this.LoadIntVariables();
		MainLevel.Instance.LoadDayTime();
		this.m_IsLoading = false;
	}

	private void SaveBoolVariables()
	{
		Dictionary<int, bool> boolVariables = ScenarioManager.Get().m_BoolVariables;
		Dictionary<int, string> hashNames = ScenarioManager.Get().m_HashNames;
		SaveGame.SaveVal("BoolVariablesCount", boolVariables.Count);
		for (int i = 0; i < boolVariables.Count; i++)
		{
			int key = boolVariables.ElementAt(i).Key;
			string val = hashNames[key];
			SaveGame.SaveVal("BoolVariableName" + i, val);
			SaveGame.SaveVal("BoolVariable" + i, boolVariables.ElementAt(i).Value);
		}
	}

	private void SaveIntVariables()
	{
		Dictionary<string, int> intVariables = ScenarioManager.Get().m_IntVariables;
		SaveGame.SaveVal("IntVariablesCount", intVariables.Count);
		for (int i = 0; i < intVariables.Count; i++)
		{
			SaveGame.SaveVal("IntVariableName" + i, intVariables.ElementAt(i).Key);
			SaveGame.SaveVal("IntVariable" + i, intVariables.ElementAt(i).Value);
		}
	}

	private void LoadBoolvariables()
	{
		int num = SaveGame.LoadIVal("BoolVariablesCount");
		for (int i = 0; i < num; i++)
		{
			string name = SaveGame.LoadSVal("BoolVariableName" + i);
			bool val = SaveGame.LoadBVal("BoolVariable" + i);
			ScenarioManager.Get().SetBoolVariable(name, val);
		}
	}

	private void LoadIntVariables()
	{
		int num = SaveGame.LoadIVal("IntVariablesCount");
		for (int i = 0; i < num; i++)
		{
			string name = SaveGame.LoadSVal("IntVariableName" + i);
			int val = SaveGame.LoadIVal("IntVariable" + i);
			ScenarioManager.Get().SetIntVariable(name, val);
		}
	}

	public void OnSceneLoaded()
	{
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			foreach (ScenarioElement scenarioElement in scenarioNode.m_Elements)
			{
				scenarioElement.OnSceneLoaded();
			}
		}
	}

	public void OnSceneUnload(Scene scene)
	{
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			foreach (ScenarioElement scenarioElement in scenarioNode.m_Elements)
			{
				scenarioElement.OnSceneUnload(scene);
			}
		}
	}

	public void OnItemCreated(GameObject go)
	{
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			foreach (ScenarioElement scenarioElement in scenarioNode.m_Elements)
			{
				if (scenarioElement.m_HasNullObject)
				{
					scenarioElement.CheckObjects(go);
				}
			}
		}
	}

	public List<ScenarioNode> m_Nodes = new List<ScenarioNode>();

	public static string s_ScriptPath = "Scripts/Scenario/";

	public static string s_StoryScript = "Story";

	public static string s_SurvivalScript = "Survival";

	public static string s_ChallengeScript = "Challenge";

	public static string s_TwitchDemoScript = "ScenarioTwitchDemo";

	public static string s_HelpScript = "ScenarioHelp";

	public static string s_StoryDemoScript = "Story2";

	private static Scenario s_Instance = null;

	[HideInInspector]
	public bool m_IsLoading;

	public List<GameObject> m_ActionObjectsToSave = new List<GameObject>();

	public List<GameObject> m_ActionObjectsToLoad = new List<GameObject>();

	public Dictionary<GameObject, string> m_ActionComponentToSave = new Dictionary<GameObject, string>();

	public Dictionary<GameObject, string> m_ActionComponentToLoad = new Dictionary<GameObject, string>();

	public List<string> m_FilesToInclude = new List<string>();
}
