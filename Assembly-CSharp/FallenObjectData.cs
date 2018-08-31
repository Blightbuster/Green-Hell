using System;
using System.Collections.Generic;
using UnityEngine;

public class FallenObjectData
{
	public FallenObjectData()
	{
	}

	public FallenObjectData(FallenObjectData src)
	{
		this.m_SourceTag = src.m_SourceTag;
		this.m_FallenPrefabName = src.m_FallenPrefabName;
		this.m_Chance = src.m_Chance;
		this.m_QuantityMin = src.m_QuantityMin;
		this.m_QuantityMax = src.m_QuantityMax;
		this.m_MinGenRadius = src.m_MinGenRadius;
		this.m_MaxGenRadius = src.m_MaxGenRadius;
		this.m_ObjectsSpawnNextTime = src.m_ObjectsSpawnNextTime;
		this.m_Cooldown = src.m_Cooldown;
	}

	public string m_SourceTag = string.Empty;

	public string m_FallenPrefabName = string.Empty;

	public float m_Chance = 1f;

	public int m_QuantityMin;

	public int m_QuantityMax;

	public float m_MinGenRadius = 1f;

	public float m_MaxGenRadius = 1f;

	public float m_ObjectsSpawnNextTime = -1f;

	public float m_Cooldown = 10f;

	public List<GameObject> m_GeneratedObjects = new List<GameObject>();

	public bool m_NoRespawn;

	public bool m_AlreadySpawned;
}
