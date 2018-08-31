using System;
using UnityEngine;

public class Noise
{
	public Vector3 m_Position;

	public float m_Time;

	public Noise.Type m_Type = Noise.Type.None;

	public enum Type
	{
		None = -1,
		Sneak,
		Walk,
		Run,
		Swim,
		Action,
		Count
	}
}
