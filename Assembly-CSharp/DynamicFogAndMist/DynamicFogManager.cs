using System;
using UnityEngine;

namespace DynamicFogAndMist
{
	[HelpURL("http://kronnect.com/taptapgo")]
	[ExecuteInEditMode]
	public class DynamicFogManager : MonoBehaviour
	{
		private void OnEnable()
		{
			this.UpdateMaterialProperties();
		}

		private void Reset()
		{
			this.UpdateMaterialProperties();
		}

		private void Update()
		{
			if (this.sun != null)
			{
				bool flag = false;
				if (this.sun.transform.forward != this.sunDirection)
				{
					flag = true;
				}
				if (this.sunLight != null && (this.sunLight.color != this.sunColor || this.sunLight.intensity != this.sunIntensity))
				{
					flag = true;
				}
				if (flag)
				{
					this.UpdateFogColor();
				}
			}
			this.UpdateFogData();
		}

		public void UpdateMaterialProperties()
		{
			this.UpdateFogData();
			this.UpdateFogColor();
		}

		private void UpdateFogData()
		{
			Vector4 value = new Vector4(this.height + 0.001f, this.baselineHeight, Camera.main.farClipPlane * this.distance, this.heightFallOff);
			Shader.SetGlobalVector("_FogData", value);
			Shader.SetGlobalFloat("_FogData2", this.distanceFallOff * value.z + 0.0001f);
		}

		private void UpdateFogColor()
		{
			if (this.sun != null)
			{
				if (this.sunLight == null)
				{
					this.sunLight = this.sun.GetComponent<Light>();
				}
				if (this.sunLight != null && this.sunLight.transform != this.sun.transform)
				{
					this.sunLight = this.sun.GetComponent<Light>();
				}
				this.sunDirection = this.sun.transform.forward;
				if (this.sunLight != null)
				{
					this.sunColor = this.sunLight.color;
					this.sunIntensity = this.sunLight.intensity;
				}
			}
			float b = this.sunIntensity * Mathf.Clamp01(1f - this.sunDirection.y);
			Color value = this.color * this.sunColor * b;
			value.a = this.alpha;
			Shader.SetGlobalColor("_FogColor", value);
		}

		[Range(0f, 1f)]
		public float alpha = 1f;

		[Range(0f, 1f)]
		public float noiseStrength = 0.5f;

		[Range(0f, 0.999f)]
		public float distance = 0.2f;

		[Range(0f, 2f)]
		public float distanceFallOff = 1f;

		[Range(0f, 500f)]
		public float height = 1f;

		[Range(0f, 1f)]
		public float heightFallOff = 1f;

		public float baselineHeight;

		public Color color = new Color(0.89f, 0.89f, 0.89f, 1f);

		public GameObject sun;

		private Light sunLight;

		private Vector3 sunDirection = Vector3.zero;

		private Color sunColor = Color.white;

		private float sunIntensity = 1f;
	}
}
