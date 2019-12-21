using System;
using UnityEngine;

public class FOVObject : MonoBehaviour
{
	public float m_FOV = 65f;

	public float m_Radius = 1f;

	[HideInInspector]
	public float m_MaxRadius = 10f;
}
