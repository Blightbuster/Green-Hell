using System;
using System.Collections.Generic;
using UnityEngine;

public class DreamSpawner : MonoBehaviour
{
	public DreamSpawner()
	{
		DreamSpawner.m_Spawners.Add(this);
	}

	public static DreamSpawner Find(string name)
	{
		int i = 0;
		while (i < DreamSpawner.m_Spawners.Count)
		{
			if (DreamSpawner.m_Spawners[i] == null)
			{
				DreamSpawner.m_Spawners.RemoveAt(i);
			}
			else
			{
				if (DreamSpawner.m_Spawners[i].name == name)
				{
					return DreamSpawner.m_Spawners[i];
				}
				i++;
			}
		}
		return null;
	}

	private static List<DreamSpawner> m_Spawners = new List<DreamSpawner>();
}
