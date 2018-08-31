using System;
using System.Collections.Generic;
using UnityEngine;

public class CutData
{
	public string m_DummyName = string.Empty;

	public int m_BlendShapeIndex;

	public float m_MaxHealth = 100f;

	public float m_Health = 100f;

	public string m_DestroyedPrefabName = string.Empty;

	public float m_BlendFactor;

	public List<Vector3> m_VerticesToMorph;

	public List<Vector3> m_VerticesMorphTarget;

	public List<int> m_VerticesToMorphIndex;
}
