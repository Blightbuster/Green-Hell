using System;
using System.Collections.Generic;
using UnityEngine;

public class SensorTerrainCollision : SensorBase
{
	protected override void Start()
	{
		base.Start();
		TerrainCollider[] array = UnityEngine.Object.FindObjectsOfType<TerrainCollider>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].bounds.Intersects(this.m_MeshRenderer.bounds))
			{
				this.m_TerrainColliders.Add(array[i]);
			}
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		for (int i = 0; i < this.m_TerrainColliders.Count; i++)
		{
			Physics.IgnoreCollision(Player.Get().m_Collider, this.m_TerrainColliders[i], this.m_DisableOnEnter);
		}
	}

	protected override void OnExit()
	{
		base.OnExit();
		for (int i = 0; i < this.m_TerrainColliders.Count; i++)
		{
			Physics.IgnoreCollision(Player.Get().m_Collider, this.m_TerrainColliders[i], !this.m_DisableOnEnter);
		}
	}

	private List<TerrainCollider> m_TerrainColliders = new List<TerrainCollider>();

	public bool m_DisableOnEnter = true;
}
