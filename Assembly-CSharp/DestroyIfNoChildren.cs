using System;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfNoChildren : MonoBehaviour
{
	private void Start()
	{
		this.UpdateCount();
	}

	private void UpdateCount()
	{
		if (this.m_OnlySelected)
		{
			int num = 0;
			for (int i = 0; i < this.m_Children.Count; i++)
			{
				if (this.m_Children[i] != null && (!this.m_OnlyActiveChildrenCount || this.m_Children[i].activeSelf))
				{
					num++;
				}
			}
			this.m_NumChildren = num;
			return;
		}
		if (this.m_OnlyActiveChildrenCount)
		{
			int num2 = 0;
			for (int j = 0; j < base.gameObject.transform.childCount; j++)
			{
				Transform child = base.gameObject.transform.GetChild(j);
				if (child != null && child.gameObject.activeSelf)
				{
					num2++;
				}
			}
			this.m_NumChildren = num2;
			return;
		}
		this.m_NumChildren = base.gameObject.transform.childCount;
	}

	public void OnObjectDestroyed()
	{
		base.Invoke("Check", 2f);
	}

	private void Check()
	{
		this.UpdateCount();
		if (this.m_OnlySelected)
		{
			for (int i = 0; i < this.m_Children.Count; i++)
			{
				if (!(this.m_Children[i] == null) && (!this.m_OnlyActiveChildrenCount || this.m_Children[i].activeSelf) && this.m_Children[i].transform.parent == base.gameObject.transform)
				{
					return;
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (this.m_OnlyActiveChildrenCount)
		{
			for (int j = 0; j < base.gameObject.transform.childCount; j++)
			{
				Transform child = base.gameObject.transform.GetChild(j);
				if (child != null && child.gameObject.activeSelf)
				{
					return;
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (base.gameObject.transform.childCount == 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public bool CheckNoChildren()
	{
		if (this.m_OnlySelected)
		{
			for (int i = 0; i < this.m_Children.Count; i++)
			{
				if (!(this.m_Children[i] == null) && (!this.m_OnlyActiveChildrenCount || this.m_Children[i].activeSelf) && this.m_Children[i].transform.parent != null && this.m_Children[i].transform.parent == base.gameObject.transform)
				{
					return false;
				}
			}
			return true;
		}
		if (this.m_OnlyActiveChildrenCount)
		{
			for (int j = 0; j < base.gameObject.transform.childCount; j++)
			{
				Transform child = base.gameObject.transform.GetChild(j);
				if (child != null && child.gameObject.activeSelf)
				{
					return false;
				}
			}
			return true;
		}
		return base.gameObject.transform.childCount == 0;
	}

	public bool m_OnlySelected;

	public bool m_OnlyActiveChildrenCount;

	public List<GameObject> m_Children = new List<GameObject>();

	public int m_NumChildren;
}
