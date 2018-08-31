using System;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfSelectedInactive : MonoBehaviour
{
	private void Update()
	{
		for (int i = 0; i < this.m_Objects.Count; i++)
		{
			if (this.m_Objects[i].activeSelf)
			{
				return;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public List<GameObject> m_Objects = new List<GameObject>();
}
