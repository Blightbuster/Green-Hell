using System;
using UnityEngine;

public class DestroyDelay : MonoBehaviour
{
	private void Awake()
	{
		base.Invoke("DestroyMe", this.m_Delay);
	}

	private void DestroyMe()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public float m_Delay;
}
