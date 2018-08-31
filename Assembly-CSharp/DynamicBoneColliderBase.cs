using System;
using UnityEngine;

public class DynamicBoneColliderBase : MonoBehaviour
{
	public virtual void Collide(ref Vector3 particlePosition, float particleRadius)
	{
	}

	public DynamicBoneColliderBase.Direction m_Direction = DynamicBoneColliderBase.Direction.Y;

	public Vector3 m_Center = Vector3.zero;

	public DynamicBoneColliderBase.Bound m_Bound;

	public enum Direction
	{
		X,
		Y,
		Z
	}

	public enum Bound
	{
		Outside,
		Inside
	}
}
