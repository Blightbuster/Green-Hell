using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioManager : MonoBehaviour, ISaveLoad
{
	public static ScenarioManager Get()
	{
		return ScenarioManager.s_Instance;
	}

	public ScenarioManager()
	{
		ScenarioManager.s_Instance = this;
	}

	private void Awake()
	{
		ScenarioManager.s_Instance = this;
	}

	private void Start()
	{
		SceneLoadUnloadRequestHolder.OnSceneLoad += this.OnSceneLoad;
		SceneLoadUnloadRequestHolder.OnSceneUnload += this.OnSceneUnload;
	}

	public void Initialize()
	{
		this.LoadSyntaxScript();
		this.m_Scenario = new Scenario();
		this.m_Scenario.LoadScript();
		this.m_Scenario.SetupParents();
		this.m_Scenario.SetupNodes();
		this.ClearObjectCache();
		this.m_Initialized = true;
	}

	private void LoadSyntaxScript()
	{
		this.m_ScenarioSyntax.Clear();
		TextAssetParser textAssetParser = new TextAssetParser(Resources.Load("Scripts/Scenario/ScenarioSyntax") as TextAsset);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Syntax")
			{
				ScenarioSyntaxData scenarioSyntaxData = new ScenarioSyntaxData();
				scenarioSyntaxData.m_Encoded = key.GetVariable(1).SValue;
				this.m_ScenarioSyntax.Add(key.GetVariable(0).SValue, scenarioSyntaxData);
				if (key.GetVariablesCount() >= 3)
				{
					scenarioSyntaxData.m_PerformOnLoad = key.GetVariable(2).BValue;
				}
			}
		}
	}

	public void OnFullLoadEnd()
	{
		if (this.IsBoolVariableTrue("IsTutorialFinished"))
		{
			this.ExecuteNode("DeactivateTutorialNodes");
		}
	}

	private void Update()
	{
		if (this.m_Initialized && this.m_Scenario != null)
		{
			this.UpdateSceneLoadUnload();
			if (DeathController.Get().IsState(DeathController.DeathState.None))
			{
				this.m_Scenario.Update();
			}
		}
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

	public void ExecuteNode(string node_name)
	{
		this.m_Scenario.ExecuteNode(node_name);
	}

	public void RestartNode(string node_name)
	{
		this.m_Scenario.RestartNode(node_name);
	}

	public bool IsNodeCompleted(string node_name)
	{
		return this.m_Scenario.IsNodeCompleted(node_name);
	}

	public void PlayAudioSource(GameObject as_obj)
	{
		AudioSource audioSource = as_obj ? as_obj.GetComponent<AudioSource>() : null;
		if (audioSource)
		{
			audioSource.Play();
		}
	}

	public void StopAudioSource(GameObject as_obj)
	{
		AudioSource audioSource = as_obj ? as_obj.GetComponent<AudioSource>() : null;
		if (audioSource)
		{
			audioSource.Stop();
		}
	}

	public void PlayAnimation(GameObject anim_obj, string anim_name)
	{
		Animator animator = anim_obj ? anim_obj.GetComponent<Animator>() : null;
		if (animator)
		{
			for (int i = 0; i < animator.layerCount; i++)
			{
				if (animator.HasState(i, Animator.StringToHash(anim_name)))
				{
					animator.CrossFade(anim_name, 0f, i);
				}
			}
			return;
		}
		Animation animation = anim_obj ? anim_obj.GetComponent<Animation>() : null;
		if (animation)
		{
			animation.Play(anim_name);
		}
	}

	public void SetAnimatorBool(GameObject anim_obj, string bool_name, bool value)
	{
		Animator animator = anim_obj ? anim_obj.GetComponent<Animator>() : null;
		if (animator)
		{
			animator.SetBool(bool_name, value);
		}
	}

	public void EnableComponent(GameObject obj, string component_name)
	{
		if (!obj)
		{
			global::Logger.Log("Can't enable component - " + component_name + ". Object is not set!");
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
			global::Logger.Log("Can't disable component - " + component_name + ". Object is null!");
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

	public void ResetVariables()
	{
		this.m_HashNames.Clear();
		this.m_BoolVariables.Clear();
		this.m_IntVariables.Clear();
	}

	public void CreateBoolVariable(string name, bool val)
	{
		int key = Animator.StringToHash(name);
		bool flag = false;
		if (!this.m_BoolVariables.TryGetValue(key, out flag))
		{
			this.m_BoolVariables.Add(key, val);
		}
		string empty = string.Empty;
		if (!this.m_HashNames.TryGetValue(key, out empty))
		{
			this.m_HashNames.Add(key, name);
		}
	}

	public void SetBoolVariable(string name, bool val)
	{
		int key = Animator.StringToHash(name);
		bool flag = false;
		if (this.m_BoolVariables.TryGetValue(key, out flag))
		{
			this.m_BoolVariables[key] = val;
			return;
		}
		this.CreateBoolVariable(name, val);
	}

	public bool IsBoolVariableTrue(string name)
	{
		int key = Animator.StringToHash(name);
		bool result = false;
		this.m_BoolVariables.TryGetValue(key, out result);
		return result;
	}

	public bool IsBoolVariableFalse(string name)
	{
		int key = Animator.StringToHash(name);
		bool flag = false;
		this.m_BoolVariables.TryGetValue(key, out flag);
		return !flag;
	}

	public void CreateIntVariable(string name, int val)
	{
		int num = 0;
		if (!this.m_IntVariables.TryGetValue(name, out num))
		{
			this.m_IntVariables.Add(name, val);
			return;
		}
		this.m_IntVariables[name] = val;
	}

	public void SetIntVariable(string name, int val)
	{
		int num = 0;
		if (this.m_IntVariables.TryGetValue(name, out num))
		{
			this.m_IntVariables[name] = val;
		}
	}

	public void IntVariableAdd(string name, int val)
	{
		if (this.m_IntVariables.ContainsKey(name))
		{
			Dictionary<string, int> intVariables = this.m_IntVariables;
			intVariables[name] += val;
		}
	}

	public bool IsIntVariableEqual(string name, int val)
	{
		int num = 0;
		this.m_IntVariables.TryGetValue(name, out num);
		return num == val;
	}

	public bool IsIntVariableGreater(string name, int val)
	{
		int num = 0;
		this.m_IntVariables.TryGetValue(name, out num);
		return num > val;
	}

	public bool IsIntVariableGreaterOrEqual(string name, int val)
	{
		int num = 0;
		this.m_IntVariables.TryGetValue(name, out num);
		return num >= val;
	}

	public void Disappear(GameObject obj, string prefab_name)
	{
		GameObject prefab = GreenHellGame.Instance.GetPrefab(prefab_name);
		if (!prefab)
		{
			return;
		}
		UnityEngine.Object.Instantiate<GameObject>(prefab, obj.transform.position, obj.transform.rotation).GetComponent<DisappearEffect>().Initialize(obj.transform);
	}

	public void Save()
	{
		int num = 0;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (this.IsValidSceneToSave(sceneAt.name))
			{
				SaveGame.SaveVal("Scene" + num, sceneAt.name);
				num++;
			}
		}
		SaveGame.SaveVal("ScenesCount", num);
		this.m_Scenario.Save();
	}

	public void Load()
	{
		this.m_ScenesToLoad.Clear();
		int num = SaveGame.LoadIVal("ScenesCount");
		for (int i = 0; i < num; i++)
		{
			string item = SaveGame.LoadSVal("Scene" + i);
			this.m_ScenesToLoad.Add(item);
		}
		this.m_LoadState = ScenarioManager.LoadState.UnloadingScenes;
		foreach (string scene_name in GreenHellGame.Instance.m_ScenarioScenes)
		{
			SceneLoadUnloadRequestHolder.Get().UnloadScene(scene_name, SceneLoadUnloadRequest.Reason.SavegameLoad);
		}
	}

	private bool IsValidSceneToSave(string scene_name)
	{
		return !(scene_name == "Terrain") && !scene_name.Contains("assets") && !SceneLoadUnloadRequestHolder.Get().IsAnyRequest(SceneLoadUnloadRequest.Reason.Any, SceneLoadUnloadRequest.OpType.Unload, scene_name);
	}

	public void OnSceneLoad(Scene scene, SceneLoadUnloadRequest request)
	{
		if (request.m_Result == SceneLoadUnloadRequest.UpdateResult.Done && this.m_LoadState != ScenarioManager.LoadState.UnloadingScenes && this.m_LoadState != ScenarioManager.LoadState.LoadingScenes)
		{
			MainLevel.Instance.InitObjects();
			Scenario scenario = this.m_Scenario;
			if (scenario == null)
			{
				return;
			}
			scenario.OnSceneLoaded();
		}
	}

	public void OnSceneUnload(Scene scene, SceneLoadUnloadRequest request)
	{
		if (request.m_Result == SceneLoadUnloadRequest.UpdateResult.PreStart && request.m_Reason == SceneLoadUnloadRequest.Reason.Scenario)
		{
			this.OnSceneUnload(scene);
		}
	}

	private void UpdateSceneLoadUnload()
	{
		ScenarioManager.LoadState loadState = this.m_LoadState;
		if (loadState != ScenarioManager.LoadState.UnloadingScenes)
		{
			if (loadState != ScenarioManager.LoadState.LoadingScenes)
			{
				return;
			}
			if (!SceneLoadUnloadRequestHolder.Get().IsAnyRequest(SceneLoadUnloadRequest.Reason.SavegameLoad, SceneLoadUnloadRequest.OpType.Load, ""))
			{
				MainLevel.Instance.InitObjects();
				this.m_Scenario = new Scenario();
				this.m_Scenario.LoadScript();
				this.m_Scenario.SetupParents();
				this.m_Scenario.SetupNodes();
				this.m_Scenario.Load();
				this.ClearObjectCache();
				this.m_LoadState = ScenarioManager.LoadState.LoadingCompleted;
			}
		}
		else if (!SceneLoadUnloadRequestHolder.Get().IsAnyRequest(SceneLoadUnloadRequest.Reason.SavegameLoad, SceneLoadUnloadRequest.OpType.Unload, ""))
		{
			foreach (string scene_name in this.m_ScenesToLoad)
			{
				SceneLoadUnloadRequestHolder.Get().LoadScene(scene_name, SceneLoadUnloadRequest.Reason.SavegameLoad);
			}
			this.m_LoadState = ScenarioManager.LoadState.LoadingScenes;
			return;
		}
	}

	public bool LoadingCompleted()
	{
		return this.m_LoadState == ScenarioManager.LoadState.LoadingCompleted;
	}

	public void LoadScene(string scene_name)
	{
		if (!GreenHellGame.Instance.m_ScenarioScenes.Contains(scene_name) && MainLevel.Instance.GetStreamers().Count > 0)
		{
			global::Logger.LogWarning("LoadScene called for non-scenario scene " + scene_name + "! This will have no result.");
			return;
		}
		SceneLoadUnloadRequestHolder.Get().LoadScene(scene_name, SceneLoadUnloadRequest.Reason.Scenario);
	}

	public void UnloadScene(string scene_name)
	{
		if (!GreenHellGame.Instance.m_ScenarioScenes.Contains(scene_name) && MainLevel.Instance.GetStreamers().Count > 0)
		{
			global::Logger.LogWarning("UnloadScene called for non-scenario scene! This will have no result.");
			return;
		}
		SceneLoadUnloadRequestHolder.Get().UnloadScene(scene_name, SceneLoadUnloadRequest.Reason.Scenario);
	}

	public bool IsSceneLoading()
	{
		return SceneLoadUnloadRequestHolder.Get().IsLoadingUnloadingScenes();
	}

	public bool IsCreepyAppearGroupCompleted(GameObject obj)
	{
		SensorCreepyAppearGroup sensorCreepyAppearGroup = obj ? obj.GetComponent<SensorCreepyAppearGroup>() : null;
		return sensorCreepyAppearGroup && sensorCreepyAppearGroup.IsCompleted();
	}

	private void OnSceneUnload(Scene scene)
	{
		if (!this.m_Initialized || !GreenHellGame.Instance.m_ScenarioScenes.Contains(scene.name))
		{
			return;
		}
		this.m_Scenario.OnSceneUnload(scene);
	}

	public void OnItemCreated(GameObject go)
	{
		if (!this.m_Initialized)
		{
			return;
		}
		this.m_Scenario.OnItemCreated(go);
	}

	public UnityEngine.Object GetObjectOfType(Type type)
	{
		UnityEngine.Object @object;
		if (!this.m_CachedObjects.TryGetValue(type, out @object) || @object == null)
		{
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(type);
			if (array.Length != 0)
			{
				@object = array[0];
				this.m_CachedObjects[type] = @object;
			}
		}
		return @object;
	}

	public void ClearObjectCache()
	{
		this.m_CachedObjects.Clear();
	}

	public bool IsDream()
	{
		return this.IsBoolVariableTrue("DreamActive");
	}

	public bool IsPreDream()
	{
		return this.IsBoolVariableTrue("PreDreamActive");
	}

	public bool IsDreamOrPreDream()
	{
		return this.IsDream() || this.IsPreDream();
	}

	private Scenario m_Scenario;

	private Dictionary<string, ScenarioSyntaxData> m_ScenarioSyntax = new Dictionary<string, ScenarioSyntaxData>();

	private Dictionary<Type, UnityEngine.Object> m_CachedObjects = new Dictionary<Type, UnityEngine.Object>();

	private bool m_Initialized;

	[HideInInspector]
	public bool m_SkipTutorial;

	public Dictionary<int, bool> m_BoolVariables = new Dictionary<int, bool>();

	public Dictionary<string, int> m_IntVariables = new Dictionary<string, int>();

	public Dictionary<int, string> m_HashNames = new Dictionary<int, string>();

	private List<string> m_ScenesToLoad = new List<string>();

	private ScenarioManager.LoadState m_LoadState;

	private static ScenarioManager s_Instance;

	private enum LoadState
	{
		None,
		UnloadingScenes,
		LoadingScenes,
		LoadingCompleted
	}
}
