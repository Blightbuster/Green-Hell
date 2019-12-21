using System;
using UnityEngine;

public class CreepyRotator : MonoBehaviour
{
	private void Awake()
	{
		this.m_Renderer = base.gameObject.GetComponent<Renderer>();
		if (!this.m_Renderer)
		{
			this.m_Renderer = base.gameObject.GetComponentInChildren<Renderer>();
		}
	}

	private bool IsVisible()
	{
		return this.m_Renderer && Camera.main && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), this.m_Renderer.bounds);
	}

	private void Update()
	{
		if (this.IsVisible())
		{
			return;
		}
		base.transform.rotation = Quaternion.LookRotation((Player.Get().transform.position - base.transform.position).normalized, Vector3.up);
	}

	private Renderer m_Renderer;
}
