using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadUnloadRequestHolder : MonoBehaviour
{
	public static SceneLoadUnloadRequestHolder Get()
	{
		return SceneLoadUnloadRequestHolder.s_Instance;
	}

	public static event SceneLoadUnloadRequestHolder.OnSceneLoadDel OnSceneLoad;

	public static event SceneLoadUnloadRequestHolder.OnSceneUnloadDel OnSceneUnload;

	private void Awake()
	{
		SceneLoadUnloadRequestHolder.s_Instance = this;
	}

	public void LoadScene(string scene_name, SceneLoadUnloadRequest.Reason reason)
	{
		if (SceneManager.GetSceneByName(scene_name).isLoaded)
		{
			Debug.LogWarning("Scene " + scene_name + " already loaded.");
			return;
		}
		this.Add(new SceneLoadUnloadRequest
		{
			m_SceneName = scene_name,
			m_OpType = SceneLoadUnloadRequest.OpType.Load,
			m_Reason = reason
		});
	}

	public void UnloadScene(string scene_name, SceneLoadUnloadRequest.Reason reason)
	{
		SceneLoadUnloadRequest sceneLoadUnloadRequest = new SceneLoadUnloadRequest
		{
			m_SceneName = scene_name,
			m_OpType = SceneLoadUnloadRequest.OpType.Unload,
			m_Reason = reason
		};
		sceneLoadUnloadRequest.m_Result = SceneLoadUnloadRequest.UpdateResult.PreStart;
		SceneLoadUnloadRequestHolder.OnSceneUnload(SceneManager.GetSceneByName(sceneLoadUnloadRequest.m_SceneName), sceneLoadUnloadRequest);
		this.Add(sceneLoadUnloadRequest);
	}

	public void Add(SceneLoadUnloadRequest request)
	{
		this.m_SceneLoadUnloadRequests.Add(request);
	}

	public bool IsLoadingUnloadingScenes()
	{
		return this.m_SceneLoadUnloadRequests.Count > 0;
	}

	public bool IsAnyRequest(SceneLoadUnloadRequest.Reason reason = SceneLoadUnloadRequest.Reason.Any, SceneLoadUnloadRequest.OpType op_type = SceneLoadUnloadRequest.OpType.Any, string scene_name = "")
	{
		foreach (SceneLoadUnloadRequest sceneLoadUnloadRequest in this.m_SceneLoadUnloadRequests)
		{
			if ((reason == SceneLoadUnloadRequest.Reason.Any || sceneLoadUnloadRequest.m_Reason == reason) && (op_type == SceneLoadUnloadRequest.OpType.Any || sceneLoadUnloadRequest.m_OpType == op_type) && (scene_name.Length == 0 || sceneLoadUnloadRequest.m_SceneName == scene_name))
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		this.UpdateRequests();
	}

	private void UpdateRequests()
	{
		int count = this.m_SceneLoadUnloadRequests.Count;
		int i = 0;
		while (i < this.m_SceneLoadUnloadRequests.Count)
		{
			SceneLoadUnloadRequest sceneLoadUnloadRequest = this.m_SceneLoadUnloadRequests[i];
			SceneLoadUnloadRequest.UpdateResult r = sceneLoadUnloadRequest.Update();
			if (r == SceneLoadUnloadRequest.UpdateResult.Started)
			{
				this.OnSceneStatusChanged(sceneLoadUnloadRequest);
				i++;
			}
			else if (r == SceneLoadUnloadRequest.UpdateResult.Done)
			{
				this.OnSceneStatusChanged(sceneLoadUnloadRequest);
				this.m_SceneLoadUnloadRequests.RemoveAt(i);
			}
			else if (r == SceneLoadUnloadRequest.UpdateResult.Failed)
			{
				this.OnSceneStatusChanged(sceneLoadUnloadRequest);
				this.m_SceneLoadUnloadRequests.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private void OnSceneStatusChanged(SceneLoadUnloadRequest request)
	{
		if (request.m_OpType == SceneLoadUnloadRequest.OpType.Load)
		{
			SceneLoadUnloadRequestHolder.OnSceneLoad(SceneManager.GetSceneByName(request.m_SceneName), request);
			return;
		}
		if (request.m_OpType == SceneLoadUnloadRequest.OpType.Unload)
		{
			SceneLoadUnloadRequestHolder.OnSceneUnload(SceneManager.GetSceneByName(request.m_SceneName), request);
		}
	}

	private static SceneLoadUnloadRequestHolder s_Instance;

	private List<SceneLoadUnloadRequest> m_SceneLoadUnloadRequests = new List<SceneLoadUnloadRequest>();

	public delegate void OnSceneLoadDel(Scene scene, SceneLoadUnloadRequest request);

	public delegate void OnSceneUnloadDel(Scene scene, SceneLoadUnloadRequest request);
}
