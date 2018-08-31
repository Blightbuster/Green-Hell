using System;
using System.Collections.Generic;
using UnityEngine;

public class PushLeaves : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.GetComponentsInChildren<Renderer>(PushLeaves.s_RendererCache);
		for (int i = 0; i < PushLeaves.s_RendererCache.Count; i++)
		{
			Renderer renderer = PushLeaves.s_RendererCache[i];
			Material[] materials = renderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				this.m_AllMaterials.Add(materials[j]);
				if (materials[j].shader == LeavesPusher.s_Shader)
				{
					this.m_MaterialsToModify.Add(materials[j]);
				}
			}
		}
	}

	public List<Material> GetMaterialsToModify()
	{
		return this.m_MaterialsToModify;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < this.m_MaterialsToModify.Count; i++)
		{
			UnityEngine.Object.Destroy(this.m_MaterialsToModify[i]);
		}
	}

	public float m_ShakeAdd = 1.2f;

	public float m_ShakeTimeAdd = 4f;

	public float m_HitShakeAdd = 0.6f;

	public float m_HitShakeTimeAdd = 4f;

	public bool m_SmallPlant;

	private static List<Renderer> s_RendererCache = new List<Renderer>(30);

	private List<Material> m_AllMaterials = new List<Material>();

	private List<Material> m_MaterialsToModify = new List<Material>();
}
