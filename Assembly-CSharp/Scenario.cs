using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Scenario : ISaveLoad
{
	public Scenario()
	{
		Scenario.s_Instance = this;
	}

	public static Scenario Get()
	{
		return Scenario.s_Instance;
	}

	private string GetScriptName()
	{
		if (GreenHellGame.TWITCH_DEMO)
		{
			return Scenario.s_TwitchDemoScript;
		}
		if (ChallengesManager.Get().IsChallengeActive())
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
		if (MainLevel.Instance.m_GameMode == GameMode.Debug)
		{
			return;
		}
		TextAsset asset = Resources.Load(Scenario.s_ScriptPath + this.GetScriptName()) as TextAsset;
		TextAssetParser textAssetParser = new TextAssetParser(asset);
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
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			if (scenarioNode.m_Name == node_name)
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
		if (node != null)
		{
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
				string text = this.m_ActionComponentToSave[gameObject2];
				if (text.Contains("Collider"))
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
	}

	public void Load()
	{
		this.m_ActionObjectsToLoad.Clear();
		this.m_ActionComponentToLoad.Clear();
		this.m_IsLoading = true;
		foreach (ScenarioNode scenarioNode in this.m_Nodes)
		{
			scenarioNode.Load();
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
				string text = this.m_ActionComponentToLoad[gameObject2];
				if (text.Contains("Collider"))
				{
					Collider collider = gameObject2.GetComponent(this.m_ActionComponentToLoad[gameObject2]) as Collider;
					collider.enabled = SaveGame.LoadBVal("AOC" + gameObject2.name);
				}
				else
				{
					Behaviour behaviour = gameObject2.GetComponent(this.m_ActionComponentToLoad[gameObject2]) as Behaviour;
					behaviour.enabled = SaveGame.LoadBVal("AOC" + gameObject2.name);
				}
			}
		}
		this.m_IsLoading = false;
	}

	public List<ScenarioNode> m_Nodes = new List<ScenarioNode>();

	public static string s_ScriptPath = "Scripts/Scenario/";

	public static string s_StoryScript = "Scenario";

	public static string s_SurvivalScript = "ScenarioSurvival";

	public static string s_ChallengeScript = "ScenarioChallenge";

	public static string s_TwitchDemoScript = "ScenarioTwitchDemo";

	public static string s_HelpScript = "ScenarioHelp";

	private static Scenario s_Instance;

	[HideInInspector]
	public bool m_IsLoading;

	public List<GameObject> m_ActionObjectsToSave = new List<GameObject>();

	public List<GameObject> m_ActionObjectsToLoad = new List<GameObject>();

	public Dictionary<GameObject, string> m_ActionComponentToSave = new Dictionary<GameObject, string>();

	public Dictionary<GameObject, string> m_ActionComponentToLoad = new Dictionary<GameObject, string>();
}
