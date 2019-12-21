using System;
using AIs;
using UnityEngine;

public static class GameObjectExtension
{
	public static GameObject FindChild(this GameObject parent, string name)
	{
		Transform transform = parent.transform.Find(name);
		if (transform != null)
		{
			return transform.gameObject;
		}
		foreach (object obj in parent.transform)
		{
			GameObject gameObject = ((Transform)obj).gameObject.FindChild(name);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public static Mesh GetMesh(this GameObject obj)
	{
		MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
		if (meshFilter && meshFilter.sharedMesh)
		{
			return meshFilter.sharedMesh;
		}
		meshFilter = obj.GetComponentInChildren<MeshFilter>();
		if (meshFilter && meshFilter.sharedMesh)
		{
			return meshFilter.sharedMesh;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer)
		{
			return skinnedMeshRenderer.sharedMesh;
		}
		skinnedMeshRenderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer)
		{
			return skinnedMeshRenderer.sharedMesh;
		}
		return null;
	}

	public static SkinnedMeshRenderer GetSkinnedMeshRenderer(this GameObject obj)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer)
		{
			return skinnedMeshRenderer;
		}
		skinnedMeshRenderer = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer)
		{
			return skinnedMeshRenderer;
		}
		return null;
	}

	public static bool IsAI(this GameObject obj)
	{
		return obj.GetComponent<AI>() != null;
	}

	public static bool IsFish(this GameObject obj)
	{
		return obj.GetComponent<Fish>() != null;
	}

	public static bool IsHumanAI(this GameObject obj)
	{
		return obj.GetComponent<HumanAI>() != null;
	}

	public static bool IsPlayer(this GameObject obj)
	{
		return obj.CompareTag("Player") || obj.CompareTag("PlayerController");
	}

	public static bool IsRemotePlayer(this GameObject obj)
	{
		foreach (ReplicatedLogicalPlayer replicatedLogicalPlayer in ReplicatedLogicalPlayer.s_AllLogicalPlayers)
		{
			if (obj == replicatedLogicalPlayer.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsWater(this GameObject obj)
	{
		return obj.CompareTag("LiquidSource");
	}

	public static T AddComponentWithEvent<T>(this GameObject go) where T : Component
	{
		T t = go.AddComponent<T>();
		if (t != null)
		{
			IAddComponentEventListener[] components = go.GetComponents<IAddComponentEventListener>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnComponentAdded(t);
			}
		}
		return t;
	}

	public static void DestroyComponentWithEvent<T>(this GameObject go, T component) where T : Component
	{
		if (component != null)
		{
			IDeleteComponentEventListener[] components = go.GetComponents<IDeleteComponentEventListener>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnComponentDestroy(component);
			}
			UnityEngine.Object.Destroy(component);
		}
	}

	public static bool IsInvisibleInaccurateCheck(this GameObject go)
	{
		Camera mainCamera = CameraManager.Get().m_MainCamera;
		return mainCamera == null || mainCamera.WorldToScreenPoint(go.transform.position).z < 0f;
	}
}
