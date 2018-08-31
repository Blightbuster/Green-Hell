using System;
using System.Collections;
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
		IEnumerator enumerator = parent.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform2 = (Transform)obj;
				GameObject gameObject = transform2.gameObject.FindChild(name);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
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

	public static bool IsHumanAI(this GameObject obj)
	{
		return obj.GetComponent<HumanAI>() != null;
	}

	public static bool IsPlayer(this GameObject obj)
	{
		return Player.Get().gameObject == obj;
	}

	public static bool IsWater(this GameObject obj)
	{
		return obj.tag == "LiquidSource";
	}
}
