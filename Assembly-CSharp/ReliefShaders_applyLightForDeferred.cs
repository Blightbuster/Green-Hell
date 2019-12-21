using System;
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
				this._renderer = base.GetComponent<Renderer>();
			}
			if (base.GetComponent<Renderer>())
			{
				if (this.lightForSelfShadowing.type == LightType.Directional)
				{
					for (int i = 0; i < this._renderer.sharedMaterials.Length; i++)
					{
						this._renderer.sharedMaterials[i].SetVector("_WorldSpaceLightPosCustom", -this.lightForSelfShadowing.transform.forward);
					}
					return;
				}
				for (int j = 0; j < this._renderer.materials.Length; j++)
				{
					this._renderer.sharedMaterials[j].SetVector("_WorldSpaceLightPosCustom", new Vector4(this.lightForSelfShadowing.transform.position.x, this.lightForSelfShadowing.transform.position.y, this.lightForSelfShadowing.transform.position.z, 1f));
				}
				return;
			}
			else
			{
				if (this.lightForSelfShadowing.type == LightType.Directional)
				{
					Shader.SetGlobalVector("_WorldSpaceLightPosCustom", -this.lightForSelfShadowing.transform.forward);
					return;
				}
				Shader.SetGlobalVector("_WorldSpaceLightPosCustom", new Vector4(this.lightForSelfShadowing.transform.position.x, this.lightForSelfShadowing.transform.position.y, this.lightForSelfShadowing.transform.position.z, 1f));
			}
		}
	}

	public Light lightForSelfShadowing;

	private Renderer _renderer;
}
