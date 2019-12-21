using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadUnloadRequest
{
	public bool IsDone()
	{
		return this.m_Result.IsDone();
	}

	public SceneLoadUnloadRequest.UpdateResult Update()
	{
		DebugUtils.Assert(this.m_Reason != SceneLoadUnloadRequest.Reason.Any, true);
		DebugUtils.Assert(this.m_OpType != SceneLoadUnloadRequest.OpType.Any, true);
		if (!this.CanLoadOrUnloadScene())
		{
			this.m_Result = SceneLoadUnloadRequest.UpdateResult.None;
		}
		else if (this.m_AsyncOp == null)
		{
			SceneLoadUnloadRequest.OpType opType = this.m_OpType;
			if (opType != SceneLoadUnloadRequest.OpType.Load)
			{
				if (opType == SceneLoadUnloadRequest.OpType.Unload)
				{
					this.m_AsyncOp = this.UnloadSceneAsync(this.m_SceneName);
				}
			}
			else
			{
				this.m_AsyncOp = SceneLoadUnloadRequest.LoadSceneAsync(this.m_SceneName);
			}
			this.m_Result = ((this.m_AsyncOp != null) ? SceneLoadUnloadRequest.UpdateResult.Started : SceneLoadUnloadRequest.UpdateResult.Failed);
		}
		else
		{
			this.m_Result = (this.m_AsyncOp.isDone ? SceneLoadUnloadRequest.UpdateResult.Done : SceneLoadUnloadRequest.UpdateResult.InProgress);
		}
		return this.m_Result;
	}

	private bool CanLoadOrUnloadScene()
	{
		return MainLevel.Instance.m_SceneAsyncOperation.Count == 0;
	}

	public static bool IsLoadOrUnloadInProgress()
	{
		return MainLevel.Instance.m_SceneAsyncOperation.Count != 0;
	}

	public static AsyncOperation LoadSceneAsync(string scene_name)
	{
		Scene sceneByName = SceneManager.GetSceneByName(scene_name);
		if (!sceneByName.IsValid() || !sceneByName.isLoaded)
		{
			Scene sceneByName2 = SceneManager.GetSceneByName(scene_name);
			if (sceneByName2.IsValid() && sceneByName2.isLoaded)
			{
				return null;
			}
			AsyncOperation asyncOperation = null;
			if (asyncOperation == null)
			{
				asyncOperation = SceneManager.LoadSceneAsync(scene_name, LoadSceneMode.Additive);
			}
			if (asyncOperation != null)
			{
				MainLevel.Instance.m_SceneAsyncOperation.Add(asyncOperation);
				return asyncOperation;
			}
		}
		return null;
	}

	private AsyncOperation UnloadSceneAsync(string scene_name)
	{
		Scene sceneByName = SceneManager.GetSceneByName(scene_name);
		if (sceneByName.IsValid() && sceneByName.isLoaded)
		{
			AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneByName);
			if (asyncOperation != null)
			{
				MainLevel.Instance.m_SceneAsyncOperation.Add(asyncOperation);
				return asyncOperation;
			}
		}
		return null;
	}

	public string m_SceneName;

	public SceneLoadUnloadRequest.OpType m_OpType = SceneLoadUnloadRequest.OpType.Any;

	public SceneLoadUnloadRequest.Reason m_Reason = SceneLoadUnloadRequest.Reason.Any;

	private AsyncOperation m_AsyncOp;

	public SceneLoadUnloadRequest.UpdateResult m_Result = SceneLoadUnloadRequest.UpdateResult.None;

	public enum OpType
	{
		Any = -1,
		Load,
		Unload
	}

	public enum Reason
	{
		Any = -1,
		Scenario,
		SavegameLoad,
		Streamer
	}

	public struct UpdateResult
	{
		private UpdateResult(int val)
		{
			this.m_Value = val;
		}

		public bool IsDone()
		{
			return this.m_Value == SceneLoadUnloadRequest.UpdateResult.Done || this.m_Value == SceneLoadUnloadRequest.UpdateResult.Failed;
		}

		public static implicit operator int(SceneLoadUnloadRequest.UpdateResult r)
		{
			return r.m_Value;
		}

		public static implicit operator SceneLoadUnloadRequest.UpdateResult(int val)
		{
			return new SceneLoadUnloadRequest.UpdateResult(val);
		}

		public static readonly SceneLoadUnloadRequest.UpdateResult None = 0;

		public static readonly SceneLoadUnloadRequest.UpdateResult PreStart = 1;

		public static readonly SceneLoadUnloadRequest.UpdateResult Started = 2;

		public static readonly SceneLoadUnloadRequest.UpdateResult InProgress = 3;

		public static readonly SceneLoadUnloadRequest.UpdateResult Done = 4;

		public static readonly SceneLoadUnloadRequest.UpdateResult Failed = 5;

		private int m_Value;
	}
}
