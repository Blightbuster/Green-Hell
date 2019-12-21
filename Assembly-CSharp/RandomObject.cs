using System;
using UnityEngine;

public class RandomObject : MonoBehaviour
{
	private void Start()
	{
		if (this.m_Prefab && UnityEngine.Random.Range(0f, 1f) < this.m_SpawnChance)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_Prefab, base.transform.position, base.transform.rotation).transform.parent = base.transform.parent;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public GameObject m_Prefab;

	public float m_SpawnChance;
}
