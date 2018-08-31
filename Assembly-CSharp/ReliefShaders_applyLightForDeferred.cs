using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ReliefShaders_applyLightForDeferred : MonoBehaviour
{
	private void Reset()
	{
		if (base.GetComponent<Light>())
		{
			this.lightForSelfShadowing = base.GetComponent<Light>();
		}
	}

	private void Update()
	{
		if (this.lightForSelfShadowing)
		{
			if (this._renderer == null)
			{
				base.GetComponents<Renderer>(this.m_Renderers);
				if (this.m_Renderers.Count > 0)
				{
					this._renderer = this.m_Renderers[0];
				}
			}
			base.GetComponents<Renderer>(this.m_Renderers);
			if (this.m_Renderers.Count > 0)
			{
				if (this.lightForSelfShadowing.type == LightType.Directional)
				{
					for (int i = 0; i < this._renderer.sharedMaterials.Length; i++)
					{
						this._renderer.sharedMaterials[i].SetVector("_WorldSpaceLightPosCustom", -this.lightForSelfShadowing.transform.forward);
					}
				}
				else
				{
					for (int j = 0; j < this._renderer.materials.Length; j++)
					{
						this._renderer.sharedMaterials[j].SetVector("_WorldSpaceLightPosCustom", new Vector4(this.lightForSelfShadowing.transform.position.x, this.lightForSelfShadowing.transform.position.y, this.lightForSelfShadowing.transform.position.z, 1f));
					}
				}
			}
			else if (this.lightForSelfShadowing.type == LightType.Directional)
			{
				Shader.SetGlobalVector("_WorldSpaceLightPosCustom", -this.lightForSelfShadowing.transform.forward);
			}
			else
			{
				Shader.SetGlobalVector("_WorldSpaceLightPosCustom", new Vector4(this.lightForSelfShadowing.transform.position.x, this.lightForSelfShadowing.transform.position.y, this.lightForSelfShadowing.transform.position.z, 1f));
			}
		}
	}

	public Light lightForSelfShadowing;

	private Renderer _renderer;

	private List<Renderer> m_Renderers = new List<Renderer>(5);
}
