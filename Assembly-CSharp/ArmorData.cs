using System;
using System.Collections.Generic;
using UnityEngine;

public class ArmorData
{
	public void Reset()
	{
		this.m_ArmorType = ArmorType.None;
		this.m_Absorption = 0f;
		this.m_Health = 0f;
		this.m_MinAbsorption = 0.6f;
		this.m_AttachedArmor = null;
		this.m_Mesh = null;
		this.m_Destroyed = false;
		foreach (KeyValuePair<int, GameObject> keyValuePair in this.m_MeshMap)
		{
			keyValuePair.Value.SetActive(false);
		}
		this.m_DestroyedMesh.SetActive(false);
	}

	public bool IsArmorDestroyed()
	{
		return this.m_ArmorType != ArmorType.None && this.m_Destroyed;
	}

	public ArmorType m_ArmorType;

	public float m_Absorption;

	public float m_Health;

	public float m_MinAbsorption = 0.6f;

	public ArmorSlot m_Slot;

	public Dictionary<int, GameObject> m_MeshMap = new Dictionary<int, GameObject>();

	public GameObject m_DestroyedMesh;

	public Cloth m_Cloth;

	public Item m_AttachedArmor;

	public Vector3 m_AttachedArmorCollCenter = Vector3.zero;

	public Vector3 m_AttachedArmorCollSize = Vector3.zero;

	public Vector3 m_AttachedArmorOrigCollCenter = Vector3.zero;

	public Vector3 m_AttachedArmorOrigCollSize = Vector3.zero;

	public GameObject m_Mesh;

	public bool m_Destroyed;
}
