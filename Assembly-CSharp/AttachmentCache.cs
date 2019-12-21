using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class AttachmentCache : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < this.m_AttachmentNames.Count; i++)
		{
			this.m_AttachmentXforms.Add(base.transform.FindDeepChild(this.m_AttachmentNames[i]));
		}
	}

	public int GetAttachmentIdx(string attachment_name)
	{
		for (int i = 0; i < this.m_AttachmentNames.Count; i++)
		{
			if (attachment_name.Equals(this.m_AttachmentNames[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public Transform GetAttachmentTransformByIdx(int idx)
	{
		if (idx >= 0 && idx < this.m_AttachmentXforms.Count)
		{
			return this.m_AttachmentXforms[idx];
		}
		return null;
	}

	private static readonly string EMPTY_STRING;

	public List<string> m_AttachmentNames = new List<string>();

	private List<Transform> m_AttachmentXforms = new List<Transform>();
}
