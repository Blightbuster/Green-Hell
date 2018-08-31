using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	[RequireComponent(typeof(Water))]
	public sealed class WaterNormalMapAnimation : MonoBehaviour
	{
		private void Start()
		{
			this.OnValidate();
			this._NormalMap1 = new RenderTexture(this._Resolution, this._Resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
			{
				name = "[UWS] WaterNormalMapAnimation - Normal Map 1",
				wrapMode = TextureWrapMode.Repeat
			};
			this._NormalMapMaterial = new Material(this._NormalMapShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._Water = base.GetComponent<Water>();
			this._SourceNormalMap = this._Water.Materials.SurfaceMaterial.GetTexture("_BumpMap");
			this._Water.Materials.SurfaceMaterial.SetTexture("_BumpMap", this._NormalMap1);
		}

		private void OnValidate()
		{
			if (this._NormalMapShader == null)
			{
				this._NormalMapShader = Shader.Find("UltimateWater/Utilities/WaterNormalMap");
			}
		}

		private void Update()
		{
			this._NormalMapMaterial.SetVector(ShaderVariables.Offset, new Vector4(0f, 0f, Time.time * this._AnimationSpeed, 0f));
			this._NormalMapMaterial.SetVector(ShaderVariables.Period, new Vector4(this._Period, this._Period, this._Period, this._Period));
			this._NormalMapMaterial.SetFloat("_Param", this._Intensity);
			Graphics.Blit(this._SourceNormalMap, this._NormalMap1, this._NormalMapMaterial, 0);
		}

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("normalMapShader")]
		private Shader _NormalMapShader;

		[FormerlySerializedAs("resolution")]
		[SerializeField]
		private int _Resolution = 512;

		[SerializeField]
		[FormerlySerializedAs("period")]
		private float _Period = 60f;

		[FormerlySerializedAs("animationSpeed")]
		[SerializeField]
		private float _AnimationSpeed = 0.015f;

		[FormerlySerializedAs("intensity")]
		[SerializeField]
		private float _Intensity = 2f;

		private Water _Water;

		private RenderTexture _NormalMap1;

		private Texture _SourceNormalMap;

		private Material _NormalMapMaterial;
	}
}
