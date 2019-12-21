using System;
using UnityEngine;

public class PostProcessObject : MonoBehaviour
{
	public PostProcessManager.Effect m_Effect = PostProcessManager.Effect.None;

	public float m_Radius = 1f;

	[HideInInspector]
	public float m_MaxRadius = 10f;
}
