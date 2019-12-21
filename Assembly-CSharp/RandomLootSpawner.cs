using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class RandomLootSpawner : MonoBehaviour
{
	private void Start()
	{
		if (this.m_Objects == null || this.m_Objects.Count == 0)
		{
			return;
		}
		int num = 0;
		switch (DifficultySettings.ActivePreset.m_BaseDifficulty)
		{
		case GameDifficulty.Easy:
			num = UnityEngine.Random.Range(this.m_EasyChance.x, this.m_EasyChance.y + 1);
			break;
		case GameDifficulty.Normal:
			num = UnityEngine.Random.Range(this.m_MediumChance.x, this.m_MediumChance.y + 1);
			break;
		case GameDifficulty.Hard:
			num = UnityEngine.Random.Range(this.m_HardChance.x, this.m_HardChance.y + 1);
			break;
		}
		this.m_TempObjects.Clear();
		while (this.m_TempObjects.Count < num && this.m_Objects.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, this.m_Objects.Count);
			GameObject gameObject = this.m_Objects[index];
			this.m_Objects.RemoveAt(index);
			if (!(gameObject == null))
			{
				this.m_TempObjects.Add(gameObject);
			}
		}
		foreach (GameObject gameObject2 in this.m_TempObjects)
		{
			gameObject2.transform.parent = null;
			gameObject2.SetActive(true);
		}
		foreach (GameObject obj in this.m_Objects)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	public List<GameObject> m_Objects;

	private List<GameObject> m_TempObjects = new List<GameObject>();

	public Vector2Int m_EasyChance = Vector2Int.zero;

	public Vector2Int m_MediumChance = Vector2Int.zero;

	public Vector2Int m_HardChance = Vector2Int.zero;
}
