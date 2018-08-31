using System;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfNoChildren : MonoBehaviour
{
	public void OnObjectDestroyed()
	{
		base.Invoke("Check", 2f);
	}

	private void Check()
	{
		if (this.m_OnlySelected)
		{
			for (int i = 0; i < this.m_Children.Count; i++)
			{
				if (!(this.m_Children[i] == null))
				{
					if (this.m_Children[i].transform.parent != null && this.m_Children[i].transform.parent == base.gameObject.transform)
					{
						return;
					}
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (base.gameObject.transform.childCount == 0)
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
				if (!(this.m_Children[i] == null))
				{
					if (this.m_Children[i].transform.parent != null && this.m_Children[i].transform.parent == base.gameObject.transform)
					{
						return false;
					}
				}
			}
			return true;
		}
		return base.gameObject.transform.childCount == 0;
	}

	public bool m_OnlySelected;

	public List<GameObject> m_Children = new List<GameObject>();
}
