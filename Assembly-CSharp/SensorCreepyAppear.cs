using System;
using System.Collections.Generic;
using UnityEngine;

public class SensorCreepyAppear : SensorBase
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Renderer = base.gameObject.GetComponent<Renderer>();
	}

	private bool IsVisible()
	{
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), this.m_Renderer.bounds);
	}

	protected override void Update()
	{
		if (this.m_IsInside)
		{
			return;
		}
		if (this.m_SensorCreepyAppearGroup && !this.m_SensorCreepyAppearGroup.CanAppearObject())
		{
			return;
		}
		if (!this.IsVisible())
		{
			this.SpawnObject();
		}
	}

	private void SpawnObject()
	{
		if (this.m_Prefabs.Count > 0)
		{
			GameObject gameObject = this.m_Prefabs[UnityEngine.Random.Range(0, this.m_Prefabs.Count)];
			if (gameObject == null)
			{
				return;
			}
			UnityEngine.Object.Instantiate<GameObject>(gameObject, base.transform.position, this.m_RotatetToPlayer ? Quaternion.LookRotation((Player.Get().transform.position - base.transform.position).normalized, Vector3.up) : base.transform.rotation, base.transform.parent);
		}
		else if (this.m_AppearObjects.Count > 0)
		{
			GameObject gameObject2 = this.m_AppearObjects[UnityEngine.Random.Range(0, this.m_AppearObjects.Count)];
			if (gameObject2 == null)
			{
				return;
			}
			gameObject2.SetActive(true);
		}
		if (this.m_SensorCreepyAppearGroup)
		{
			this.m_SensorCreepyAppearGroup.OnAppearObject();
		}
		base.enabled = false;
	}

	private Renderer m_Renderer;

	[HideInInspector]
	public SensorCreepyAppearGroup m_SensorCreepyAppearGroup;

	public List<GameObject> m_Prefabs = new List<GameObject>();

	public List<GameObject> m_AppearObjects = new List<GameObject>();

	public bool m_RotatetToPlayer = true;
}
