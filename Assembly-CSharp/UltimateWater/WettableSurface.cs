using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public sealed class WettableSurface : MonoBehaviour
	{
		public WaterCamera MainCamera
		{
			get
			{
				return this._MainCamera;
			}
			set
			{
				this._MainCamera = value;
			}
		}

		private void LateUpdate()
		{
			if (!this._MainCamera.enabled)
			{
				return;
			}
			WettableSurface.Mode mode = this._Mode;
			if (mode != WettableSurface.Mode.TextureSpace)
			{
				if (mode == WettableSurface.Mode.NearCamera)
				{
					int waterTempLayer = WaterProjectSettings.Instance.WaterTempLayer;
					for (int i = 0; i < this._Terrains.Length; i++)
					{
						this._TerrainDrawTrees[i] = this._Terrains[i].drawTreesAndFoliage;
						this._Terrains[i].drawTreesAndFoliage = false;
						GameObject gameObject = this._Terrains[i].gameObject;
						this._TerrainLayers[i] = gameObject.layer;
						gameObject.layer = waterTempLayer;
					}
					Shader.SetGlobalTexture("_WetnessMapPrevious", this._WetnessMapB);
					Shader.SetGlobalTexture("_TotalDisplacementMap", this._Water.DynamicWater.GetCameraOverlaysData(this._MainCamera.GetComponent<Camera>(), true).GetTotalDisplacementMap());
					Shader.SetGlobalVector("_LocalMapsCoords", this._MainCamera.LocalMapsShaderCoords);
					Rect localMapsRectPrevious = this._MainCamera.LocalMapsRectPrevious;
					Shader.SetGlobalVector("_LocalMapsCoordsPrevious", new Vector4(-localMapsRectPrevious.xMin / localMapsRectPrevious.width, -localMapsRectPrevious.yMin / localMapsRectPrevious.width, 1f / localMapsRectPrevious.width, localMapsRectPrevious.width));
					Rect localMapsRect = this._MainCamera.LocalMapsRect;
					Camera wettingCamera = this.GetWettingCamera();
					wettingCamera.transform.position = new Vector3(localMapsRect.center.x, this._Terrains[0].transform.position.y + this._Terrains[0].terrainData.size.y + 1f, localMapsRect.center.y);
					wettingCamera.orthographicSize = localMapsRect.width * 0.5f;
					wettingCamera.nearClipPlane = 1f;
					wettingCamera.farClipPlane = this._Terrains[0].terrainData.size.y * 2f;
					wettingCamera.targetTexture = this._WetnessMapA;
					wettingCamera.clearFlags = CameraClearFlags.Color;
					wettingCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
					wettingCamera.RenderWithShader(this._WettableUtilNearShader, "CustomType");
					for (int j = 0; j < this._Materials.Length; j++)
					{
						this._Materials[j].SetTexture("_WetnessMap", this._WetnessMapA);
					}
					for (int k = 0; k < this._Terrains.Length; k++)
					{
						this._Terrains[k].drawTreesAndFoliage = this._TerrainDrawTrees[k];
						this._Terrains[k].gameObject.layer = this._TerrainLayers[k];
					}
					RenderTexture wetnessMapB = this._WetnessMapB;
					this._WetnessMapB = this._WetnessMapA;
					this._WetnessMapA = wetnessMapB;
					for (int l = 0; l < this._Materials.Length; l++)
					{
						this._Materials[l].SetVector("_LocalMapsCoords", this._MainCamera.LocalMapsShaderCoords);
					}
				}
			}
			else
			{
				Graphics.SetRenderTarget(this._WetnessMapA);
				this._WettableUtilMaterial.SetPass(0);
				this._WettableUtilMaterial.SetTexture("_TotalDisplacementMap", this._Water.DynamicWater.GetCameraOverlaysData(this._MainCamera.GetComponent<Camera>(), true).GetTotalDisplacementMap());
				this._WettableUtilMaterial.SetVector("_LocalMapsCoords", this._MainCamera.LocalMapsShaderCoords);
				for (int m = 0; m < this._MeshFilters.Length; m++)
				{
					Graphics.DrawMeshNow(this._MeshFilters[m].sharedMesh, this._MeshFilters[m].transform.localToWorldMatrix);
				}
				if (this._Terrains.Length != 0)
				{
					int waterTempLayer2 = WaterProjectSettings.Instance.WaterTempLayer;
					for (int n = 0; n < this._Terrains.Length; n++)
					{
						GameObject gameObject2 = this._Terrains[n].gameObject;
						this._TerrainLayers[n] = gameObject2.layer;
						gameObject2.layer = waterTempLayer2;
					}
					Shader.SetGlobalTexture("_TotalDisplacementMap", this._Water.DynamicWater.GetCameraOverlaysData(this._MainCamera.GetComponent<Camera>(), true).GetTotalDisplacementMap());
					Shader.SetGlobalVector("_LocalMapsCoords", this._MainCamera.LocalMapsShaderCoords);
					Camera wettingCamera2 = this.GetWettingCamera();
					wettingCamera2.transform.position = this._Terrains[0].transform.position + this._Terrains[0].terrainData.size * 0.5f;
					wettingCamera2.RenderWithShader(this._WettableUtilShader, "CustomType");
					for (int num = 0; num < this._Terrains.Length; num++)
					{
						this._Terrains[num].gameObject.layer = this._TerrainLayers[num];
					}
				}
			}
		}

		private void OnValidate()
		{
			if (this._WettableUtilShader == null)
			{
				this._WettableUtilShader = Shader.Find("UltimateWater/Utility/Wetness Update");
			}
			if (this._WettableUtilNearShader == null)
			{
				this._WettableUtilNearShader = Shader.Find("UltimateWater/Utility/Wetness Update (Near Camera)");
			}
			if (this._MeshRenderers == null || this._MeshRenderers.Length == 0)
			{
				this._MeshRenderers = base.GetComponentsInChildren<MeshRenderer>(true);
			}
			if (this._Terrains == null || this._Terrains.Length == 0)
			{
				this._Terrains = base.GetComponentsInChildren<Terrain>(true);
			}
			Material[] array = this._Materials;
			if (!Application.isPlaying)
			{
				if (array == null)
				{
					array = this._MeshRenderers.SelectMany((MeshRenderer mr) => mr.sharedMaterials).Concat(from t in this._Terrains
					select t.materialTemplate).ToArray<Material>();
				}
				if (this._Mode == WettableSurface.Mode.NearCamera)
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i].EnableKeyword("_WET_NEAR_CAMERA");
					}
				}
				else
				{
					for (int j = 0; j < array.Length; j++)
					{
						array[j].DisableKeyword("_WET_NEAR_CAMERA");
					}
				}
			}
		}

		private void Awake()
		{
			if (this._Water == null || this._Water.DynamicWater == null)
			{
				base.enabled = false;
				return;
			}
			this.OnValidate();
			this._TerrainLayers = new int[this._Terrains.Length];
			this._TerrainDrawTrees = new bool[this._Terrains.Length];
			this._MeshFilters = new MeshFilter[this._MeshRenderers.Length];
			for (int i = 0; i < this._MeshFilters.Length; i++)
			{
				this._MeshFilters[i] = this._MeshRenderers[i].GetComponent<MeshFilter>();
			}
			this._WetnessMapA = new RenderTexture(this._Resolution, this._Resolution, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
			{
				name = "[UWS] WettableSurface - Wetness Map",
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Bilinear
			};
			if (this._Mode == WettableSurface.Mode.NearCamera)
			{
				this._WetnessMapB = new RenderTexture(this._Resolution, this._Resolution, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
				{
					name = "[UWS] WettableSurface - Wetness Map",
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Bilinear
				};
			}
			Material[] materials = this._MeshRenderers.SelectMany((MeshRenderer mr) => mr.sharedMaterials).Concat(from t in this._Terrains
			select t.materialTemplate).Distinct<Material>().ToArray<Material>();
			this._Materials = this.InstantiateMaterials(materials);
			this.OnValidate();
			this._WettableUtilMaterial = new Material((this._Mode != WettableSurface.Mode.TextureSpace) ? this._WettableUtilNearShader : this._WettableUtilShader)
			{
				hideFlags = HideFlags.DontSave
			};
		}

		private Material[] InstantiateMaterials(Material[] materials)
		{
			Material[] array = new Material[materials.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = UnityEngine.Object.Instantiate<Material>(materials[i]);
			}
			for (int j = 0; j < this._MeshRenderers.Length; j++)
			{
				MeshRenderer meshRenderer = this._MeshRenderers[j];
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				for (int k = 0; k < sharedMaterials.Length; k++)
				{
					int num = Array.IndexOf<Material>(materials, sharedMaterials[k]);
					sharedMaterials[k] = array[num];
				}
				meshRenderer.sharedMaterials = sharedMaterials;
			}
			for (int l = 0; l < this._Terrains.Length; l++)
			{
				Terrain terrain = this._Terrains[l];
				int num2 = Array.IndexOf<Material>(materials, terrain.materialTemplate);
				terrain.materialTemplate = array[num2];
			}
			for (int m = 0; m < array.Length; m++)
			{
				array[m].SetTexture("_WetnessMap", this._WetnessMapA);
			}
			return array;
		}

		private Camera GetWettingCamera()
		{
			if (this._WettingCamera == null)
			{
				GameObject gameObject = new GameObject("Wetting Camera")
				{
					hideFlags = HideFlags.DontSave
				};
				gameObject.transform.position = new Vector3(0f, 100000f, 0f);
				gameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
				this._WettingCamera = gameObject.AddComponent<Camera>();
				this._WettingCamera.enabled = false;
				this._WettingCamera.orthographic = true;
				this._WettingCamera.orthographicSize = 1000000f;
				this._WettingCamera.nearClipPlane = 10f;
				this._WettingCamera.farClipPlane = 1000000f;
				this._WettingCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterTempLayer;
				this._WettingCamera.renderingPath = RenderingPath.VertexLit;
				this._WettingCamera.clearFlags = CameraClearFlags.Nothing;
				this._WettingCamera.targetTexture = this._WetnessMapA;
			}
			return this._WettingCamera;
		}

		[HideInInspector]
		[SerializeField]
		[FormerlySerializedAs("wettableUtilShader")]
		private Shader _WettableUtilShader;

		[FormerlySerializedAs("wettableUtilNearShader")]
		[SerializeField]
		[HideInInspector]
		private Shader _WettableUtilNearShader;

		[FormerlySerializedAs("water")]
		[SerializeField]
		private Water _Water;

		[FormerlySerializedAs("mainCamera")]
		[SerializeField]
		[Tooltip("Surface wetting near this camera will be more precise.")]
		private WaterCamera _MainCamera;

		[FormerlySerializedAs("mode")]
		[Tooltip("Texture space is good for small objects, especially convex ones.\nNear camera mode is better for terrains and big meshes that are static and don't have geometry at the bottom.")]
		[SerializeField]
		private WettableSurface.Mode _Mode;

		[FormerlySerializedAs("resolution")]
		[SerializeField]
		private int _Resolution = 512;

		[FormerlySerializedAs("meshRenderers")]
		[SerializeField]
		[Header("Direct references (Optional)")]
		private MeshRenderer[] _MeshRenderers;

		[SerializeField]
		[FormerlySerializedAs("terrains")]
		private Terrain[] _Terrains;

		private MeshFilter[] _MeshFilters;

		private Material _WettableUtilMaterial;

		private RenderTexture _WetnessMapA;

		private RenderTexture _WetnessMapB;

		private Camera _WettingCamera;

		private Material[] _Materials;

		private bool[] _TerrainDrawTrees;

		private int[] _TerrainLayers;

		public enum Mode
		{
			TextureSpace,
			NearCamera
		}
	}
}
