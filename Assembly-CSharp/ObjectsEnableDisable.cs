using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsEnableDisable : MonoBehaviour
{
	public void OnTriggerEnter(Collider collider)
	{
		for (int i = 0; i < this.m_Objects.Count; i++)
		{
			if (this.m_Objects[i] != null)
			{
				this.m_Objects[i].SetActive(true);
			}
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		for (int i = 0; i < this.m_Objects.Count; i++)
		{
			if (this.m_Objects[i] != null)
			{
				this.m_Objects[i].SetActive(false);
			}
		}
	}

	public List<GameObject> m_Objects = new List<GameObject>();
}
