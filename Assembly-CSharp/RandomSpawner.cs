using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
	private void Start()
	{
		if (this.m_Objects != null)
		{
			foreach (GameObject gameObject in this.m_Objects)
			{
				if (!(gameObject == null))
				{
					gameObject.SetActive(UnityEngine.Random.Range(0f, 1f) < this.m_SpawnChance);
					if (!gameObject.activeSelf)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
		}
	}

	public List<GameObject> m_Objects;

	public float m_SpawnChance;
}
