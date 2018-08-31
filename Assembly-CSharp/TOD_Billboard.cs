using System;
using UnityEngine;

public class TOD_Billboard : MonoBehaviour
{
	private T GetComponentInParents<T>() where T : Component
	{
		Transform transform = base.transform;
		T component = transform.GetComponent<T>();
		while (component == null && transform.parent != null)
		{
			transform = transform.parent;
			component = transform.GetComponent<T>();
		}
		return component;
	}

	public float Altitude;

	public float Azimuth;

	public float Distance = 1f;

	public float Size = 1f;
}
