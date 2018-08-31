using System;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
	public ScenarioManager()
	{
		ScenarioManager.s_Instance = this;
	}

	public static ScenarioManager Get()
	{
		return ScenarioManager.s_Instance;
	}

	private void Awake()
	{
		ScenarioManager.s_Instance = this;
	}

	public void Initialize()
	{
		this.LoadSyntaxScript();
		this.m_Scenario = new Scenario();
		this.m_Scenario.LoadScript();
		this.m_Scenario.SetupParents();
		this.m_Scenario.SetupNodes();
		this.m_Initialized = true;
	}

	private void LoadSyntaxScript()
	{
		this.m_ScenarioSyntax.Clear();
		TextAsset asset = Resources.Load("Scripts/Scenario/ScenarioSyntax") as TextAsset;
		TextAssetParser textAssetParser = new TextAssetParser(asset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Syntax")
			{
				ScenarioSyntaxData scenarioSyntaxData = new ScenarioSyntaxData();
				scenarioSyntaxData.m_Encoded = key.GetVariable(1).SValue;
				this.m_ScenarioSyntax.Add(key.GetVariable(0).SValue, scenarioSyntaxData);
			}
		}
	}

	private void Update()
	{
		if (!this.m_Initialized)
		{
			return;
		}
		if (this.m_Scenario == null)
		{
			return;
		}
		if (!DeathController.Get().IsState(DeathController.DeathState.None))
		{
			return;
		}
		this.m_Scenario.Update();
	}

	public ScenarioSyntaxData EncodeContent(string content)
	{
		if (content == null || content.Length == 0)
		{
			return null;
		}
		if (this.m_ScenarioSyntax.Count == 0)
		{
			this.LoadSyntaxScript();
		}
		string[] array = content.Split(new char[]
		{
			':'
		});
		ScenarioSyntaxData result = null;
		if (!this.m_ScenarioSyntax.TryGetValue(array[0], out result))
		{
			return null;
		}
		return result;
	}

	public void DeactivateNode(string node_name)
	{
		this.m_Scenario.DeactivateNode(node_name);
	}

	public void ActivateNode(string node_name)
	{
		this.m_Scenario.ActivateNode(node_name);
	}

	public bool IsNodeCompleted(string node_name)
	{
		return this.m_Scenario.IsNodeCompleted(node_name);
	}

	public void PlayAudioSource(GameObject as_obj)
	{
		AudioSource audioSource = (!as_obj) ? null : as_obj.GetComponent<AudioSource>();
		if (audioSource)
		{
			audioSource.Play();
		}
	}

	public void StopAudioSource(GameObject as_obj)
	{
		AudioSource audioSource = (!as_obj) ? null : as_obj.GetComponent<AudioSource>();
		if (audioSource)
		{
			audioSource.Stop();
		}
	}

	public void PlayAnimation(GameObject anim_obj, string anim_name)
	{
		Animator animator = (!anim_obj) ? null : anim_obj.GetComponent<Animator>();
		if (animator)
		{
			animator.Play(anim_name, 0);
		}
		else
		{
			Animation animation = (!anim_obj) ? null : anim_obj.GetComponent<Animation>();
			if (animation)
			{
				animation.Play(anim_name);
			}
		}
	}

	public void EnableComponent(GameObject obj, string component_name)
	{
		if (!obj)
		{
			Debug.Log("Can't enable component - " + component_name + ". Object is not set!");
			return;
		}
		if (component_name.Contains("Collider"))
		{
			Collider component = obj.gameObject.GetComponent<Collider>();
			DebugUtils.Assert(component, "Object " + obj.name + " does not contains component " + component_name, true, DebugUtils.AssertType.Info);
			component.enabled = true;
			return;
		}
		Type type = Type.GetType(component_name);
		Behaviour behaviour = obj.GetComponent(type) as Behaviour;
		DebugUtils.Assert(behaviour, "Object " + obj.name + " does not contains component " + component_name, true, DebugUtils.AssertType.Info);
		behaviour.enabled = true;
	}

	public void DisableComponent(GameObject obj, string component_name)
	{
		if (!obj)
		{
			Debug.Log("Can't disable component - " + component_name + ". Object is null!");
			return;
		}
		if (component_name.Contains("Collider"))
		{
			Collider component = obj.gameObject.GetComponent<Collider>();
			DebugUtils.Assert(component, "Object " + obj.name + " does not contains component " + component_name, true, DebugUtils.AssertType.Info);
			component.enabled = false;
			return;
		}
		Type type = Type.GetType(component_name);
		Behaviour behaviour = obj.gameObject.GetComponent(type) as Behaviour;
		DebugUtils.Assert(behaviour, "Object " + obj.name + " does not contains component " + component_name, true, DebugUtils.AssertType.Info);
		behaviour.enabled = false;
	}

	public bool ShouldSkipTutorial()
	{
		return this.m_SkipTutorial;
	}

	private Scenario m_Scenario;

	private Dictionary<string, ScenarioSyntaxData> m_ScenarioSyntax = new Dictionary<string, ScenarioSyntaxData>();

	private bool m_Initialized;

	[HideInInspector]
	public bool m_SkipTutorial;

	private static ScenarioManager s_Instance;
}
