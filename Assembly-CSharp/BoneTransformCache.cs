using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class BoneTransformCache : MonoBehaviour
{
	public void Awake()
	{
		if (this.m_CasheAllChildren)
		{
			this.CacheChildren(null);
		}
	}

	public Transform GetBone(string bone_name)
	{
		Transform result;
		if (this.m_BoneTransforms.TryGetValue(bone_name, out result))
		{
			return result;
		}
		Transform transform = base.gameObject.transform.FindDeepChild(bone_name);
		if (transform != null)
		{
			this.m_BoneTransforms.Add(bone_name, transform);
			return transform;
		}
		DebugUtils.Assert(string.Concat(new string[]
		{
			"Missing bone ",
			bone_name,
			" in ",
			base.name,
			"!"
		}), true, DebugUtils.AssertType.Info);
		return null;
	}

	public void CacheChildren(Transform parent = null)
	{
		if (parent == null)
		{
			parent = base.transform;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			this.m_BoneTransforms.Add(child.name, child);
			this.CacheChildren(child);
		}
	}

	public bool m_CasheAllChildren;

	private Dictionary<string, Transform> m_BoneTransforms = new Dictionary<string, Transform>();
}
