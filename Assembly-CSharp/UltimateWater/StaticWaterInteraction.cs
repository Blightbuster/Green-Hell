using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public sealed class StaticWaterInteraction : MonoBehaviour, IWaterShore, ILocalDisplacementMaskRenderer, IDynamicWaterEffects
	{
		public Bounds Bounds
		{
			get
			{
				return this._TotalBounds;
			}
		}

		public RenderTexture IntensityMask
		{
			get
			{
				if (this._IntensityMask == null)
				{
					this._IntensityMask = new RenderTexture(this._MapResolution, this._MapResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
					{
						name = "[UWS] StaticWaterInteraction - IntensityMask",
						hideFlags = HideFlags.DontSave,
						filterMode = FilterMode.Bilinear
					};
				}
				return this._IntensityMask;
			}
		}

		public Renderer InteractionRenderer
		{
			get
			{
				return this._InteractionMaskRenderer;
			}
		}

		[ContextMenu("Refresh Intensity Mask (Runtime Only)")]
		public void Refresh()
		{
			if (this._InteractionMaskRenderer == null)
			{
				return;
			}
			this.RenderShorelineIntensityMask();
			Vector3 localScale = this._TotalBounds.size * 0.5f;
			localScale.x /= base.transform.localScale.x;
			localScale.y /= base.transform.localScale.y;
			localScale.z /= base.transform.localScale.z;
			this._InteractionMaskRenderer.gameObject.transform.localScale = localScale;
		}

		public void SetUniformDepth(float depth, float boundsSize)
		{
			this.OnValidate();
			this.OnDestroy();
			this._TotalBounds = new Bounds(base.transform.position + new Vector3(boundsSize * 0.5f, 0f, boundsSize * 0.5f), new Vector3(boundsSize, 1f, boundsSize));
			RenderTexture intensityMask = this.IntensityMask;
			float num = Mathf.Sqrt((float)Math.Tanh((double)depth * -0.01));
			Graphics.SetRenderTarget(intensityMask);
			GL.Clear(true, true, new Color(num, num, num, num));
			Graphics.SetRenderTarget(null);
			if (this._InteractionMaskRenderer == null)
			{
				this.CreateMaskRenderer();
			}
		}

		public float GetDepthAt(float x, float z)
		{
			x = (x + this._OffsetX) * this._ScaleX;
			z = (z + this._OffsetZ) * this._ScaleZ;
			int num = (int)x;
			if ((float)num > x)
			{
				num--;
			}
			int num2 = (int)z;
			if ((float)num2 > z)
			{
				num2--;
			}
			if (num >= this._Width || num < 0 || num2 >= this._Height || num2 < 0)
			{
				return 100f;
			}
			x -= (float)num;
			z -= (float)num2;
			int num3 = num2 * this._Width + num;
			float num4 = this._HeightMapData[num3] * (1f - x) + this._HeightMapData[num3 + 1] * x;
			float num5 = this._HeightMapData[num3 + this._Width] * (1f - x) + this._HeightMapData[num3 + this._Width + 1] * x;
			return num4 * (1f - z) + num5 * z;
		}

		public static float GetTotalDepthAt(float x, float z)
		{
			float num = 100f;
			int count = StaticWaterInteraction.StaticWaterInteractions.Count;
			for (int i = 0; i < count; i++)
			{
				float depthAt = StaticWaterInteraction.StaticWaterInteractions[i].GetDepthAt(x, z);
				if (num > depthAt)
				{
					num = depthAt;
				}
			}
			return num;
		}

		public void RenderLocalMask(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			Vector3 position = this._InteractionMaskRenderer.transform.position;
			position.y = overlays.DynamicWater.Water.transform.position.y;
			this._InteractionMaskRenderer.transform.position = position;
			commandBuffer.DrawMesh(this._InteractionMaskRenderer.GetComponent<MeshFilter>().sharedMesh, this._InteractionMaskRenderer.transform.localToWorldMatrix, this._InteractionMaskMaterial, 0, 0);
		}

		public void Enable()
		{
			throw new NotImplementedException();
		}

		public void Disable()
		{
			throw new NotImplementedException();
		}

		public static StaticWaterInteraction AttachTo(GameObject target, float shoreSmoothness, bool hasBottomFaces, StaticWaterInteraction.UnderwaterAreasMode underwaterAreasMode, int mapResolution, float waveDampingThreshold = 4f, float depthScale = 1f)
		{
			StaticWaterInteraction staticWaterInteraction = target.AddComponent<StaticWaterInteraction>();
			staticWaterInteraction._ShoreSmoothness = shoreSmoothness;
			staticWaterInteraction._HasBottomFaces = hasBottomFaces;
			staticWaterInteraction._UnderwaterAreasMode = underwaterAreasMode;
			staticWaterInteraction._MapResolution = mapResolution;
			staticWaterInteraction._WaveDampingThreshold = waveDampingThreshold;
			staticWaterInteraction._DepthScale = depthScale;
			return staticWaterInteraction;
		}

		private void OnValidate()
		{
			if (this._MaskGenerateShader == null)
			{
				this._MaskGenerateShader = Shader.Find("UltimateWater/Utility/ShorelineMaskGenerate");
			}
			if (this._MaskDisplayShader == null)
			{
				this._MaskDisplayShader = Shader.Find("UltimateWater/Utility/ShorelineMaskRender");
			}
			if (this._HeightMapperShader == null)
			{
				this._HeightMapperShader = Shader.Find("UltimateWater/Utility/HeightMapper");
			}
			if (this._HeightMapperShaderAlt == null)
			{
				this._HeightMapperShaderAlt = Shader.Find("UltimateWater/Utility/HeightMapperAlt");
			}
			if (this._InteractionMaskMaterial != null)
			{
				this._InteractionMaskMaterial.SetFloat("_WaveDampingThreshold", this._WaveDampingThreshold);
			}
			if (this._InteractionMaskMaterial != null)
			{
				this._InteractionMaskMaterial.SetFloat("_WaveDampingThreshold", this._WaveDampingThreshold);
			}
		}

		private void OnEnable()
		{
			StaticWaterInteraction.StaticWaterInteractions.Add(this);
			DynamicWater.AddRenderer<ILocalDisplacementMaskRenderer>(this);
		}

		private void OnDisable()
		{
			DynamicWater.RemoveRenderer<ILocalDisplacementMaskRenderer>(this);
			StaticWaterInteraction.StaticWaterInteractions.Remove(this);
		}

		private void OnDestroy()
		{
			if (this._IntensityMask != null)
			{
				this._IntensityMask.Destroy();
				this._IntensityMask = null;
			}
			if (this._InteractionMaskMaterial != null)
			{
				this._InteractionMaskMaterial.Destroy();
				this._InteractionMaskMaterial = null;
			}
			if (this._InteractionMaskRenderer != null)
			{
				this._InteractionMaskRenderer.Destroy();
				this._InteractionMaskRenderer = null;
			}
		}

		private void Start()
		{
			this.OnValidate();
			if (this._IntensityMask == null)
			{
				this.RenderShorelineIntensityMask();
			}
			if (this._InteractionMaskRenderer == null)
			{
				this.CreateMaskRenderer();
			}
		}

		private void RenderShorelineIntensityMask()
		{
			try
			{
				this.PrepareRenderers();
				float num = 1f / this._ShoreSmoothness;
				this._TotalBounds = this._Bounds;
				if (this._UnderwaterAreasMode == StaticWaterInteraction.UnderwaterAreasMode.Generate)
				{
					float num2 = 80f / num;
					this._TotalBounds.Expand(new Vector3(num2, 0f, num2));
				}
				float y = base.transform.position.y;
				RenderTexture renderTexture = this.RenderHeightMap(this._MapResolution, this._MapResolution);
				RenderTexture intensityMask = this.IntensityMask;
				this._OffsetX = -this._TotalBounds.min.x;
				this._OffsetZ = -this._TotalBounds.min.z;
				this._ScaleX = (float)this._MapResolution / this._TotalBounds.size.x;
				this._ScaleZ = (float)this._MapResolution / this._TotalBounds.size.z;
				this._Width = this._MapResolution;
				this._Height = this._MapResolution;
				RenderTexture temporary = RenderTexture.GetTemporary(this._MapResolution, this._MapResolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				RenderTexture temporary2 = RenderTexture.GetTemporary(this._MapResolution, this._MapResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
				Material material = new Material(this._MaskGenerateShader);
				material.SetVector("_ShorelineExtendRange", new Vector2(this._TotalBounds.size.x / this._Bounds.size.x - 1f, this._TotalBounds.size.z / this._Bounds.size.z - 1f));
				material.SetFloat("_TerrainMinPoint", y);
				material.SetFloat("_Steepness", Mathf.Max(this._TotalBounds.size.x, this._TotalBounds.size.z) * num);
				material.SetFloat("_Offset1", 1f / (float)this._MapResolution);
				material.SetFloat("_Offset2", 1.41421354f / (float)this._MapResolution);
				RenderTexture renderTexture2 = null;
				if (this._UnderwaterAreasMode == StaticWaterInteraction.UnderwaterAreasMode.Generate)
				{
					RenderTexture temporary3 = RenderTexture.GetTemporary(this._MapResolution, this._MapResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
					renderTexture2 = RenderTexture.GetTemporary(this._MapResolution, this._MapResolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
					Graphics.Blit(renderTexture, temporary3, material, 2);
					StaticWaterInteraction.ComputeDistanceMap(material, temporary3, renderTexture2);
					RenderTexture.ReleaseTemporary(temporary3);
					renderTexture2.filterMode = FilterMode.Bilinear;
					material.SetTexture("_DistanceMap", renderTexture2);
					material.SetFloat("_GenerateUnderwaterAreas", 1f);
				}
				else
				{
					material.SetFloat("_GenerateUnderwaterAreas", 0f);
				}
				material.SetFloat("_DepthScale", this._DepthScale);
				Graphics.Blit(renderTexture, temporary, material, 0);
				RenderTexture.ReleaseTemporary(renderTexture);
				if (renderTexture2 != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture2);
				}
				Graphics.Blit(temporary, temporary2);
				this.ReadBackHeightMap(temporary);
				Graphics.Blit(temporary, intensityMask, material, 1);
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture.ReleaseTemporary(temporary);
				UnityEngine.Object.Destroy(material);
			}
			finally
			{
				this.RestoreRenderers();
			}
		}

		private void PrepareRenderers()
		{
			bool flag = false;
			this._Bounds = default(Bounds);
			List<GameObject> list = new List<GameObject>();
			Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>(false);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!(componentsInChildren[i].name == "Shoreline Mask"))
				{
					StaticWaterInteraction component = componentsInChildren[i].GetComponent<StaticWaterInteraction>();
					if (component == null || component == this)
					{
						list.Add(componentsInChildren[i].gameObject);
						if (flag)
						{
							this._Bounds.Encapsulate(componentsInChildren[i].bounds);
						}
						else
						{
							this._Bounds = componentsInChildren[i].bounds;
							flag = true;
						}
					}
				}
			}
			this._Terrains = base.GetComponentsInChildren<Terrain>(false);
			this._OriginalTerrainPixelErrors = new float[this._Terrains.Length];
			this._OriginalDrawTrees = new bool[this._Terrains.Length];
			for (int j = 0; j < this._Terrains.Length; j++)
			{
				this._OriginalTerrainPixelErrors[j] = this._Terrains[j].heightmapPixelError;
				this._OriginalDrawTrees[j] = this._Terrains[j].drawTreesAndFoliage;
				StaticWaterInteraction component2 = this._Terrains[j].GetComponent<StaticWaterInteraction>();
				if (component2 == null || component2 == this)
				{
					list.Add(this._Terrains[j].gameObject);
					this._Terrains[j].heightmapPixelError = 1f;
					this._Terrains[j].drawTreesAndFoliage = false;
					if (flag)
					{
						this._Bounds.Encapsulate(this._Terrains[j].transform.position);
						this._Bounds.Encapsulate(this._Terrains[j].transform.position + this._Terrains[j].terrainData.size);
					}
					else
					{
						this._Bounds = new Bounds(this._Terrains[j].transform.position + this._Terrains[j].terrainData.size * 0.5f, this._Terrains[j].terrainData.size);
						flag = true;
					}
				}
			}
			this._GameObjects = list.ToArray();
			this._OriginalRendererLayers = new int[this._GameObjects.Length];
			for (int k = 0; k < this._GameObjects.Length; k++)
			{
				this._OriginalRendererLayers[k] = this._GameObjects[k].layer;
				this._GameObjects[k].layer = WaterProjectSettings.Instance.WaterTempLayer;
			}
		}

		private void RestoreRenderers()
		{
			if (this._Terrains != null)
			{
				for (int i = 0; i < this._Terrains.Length; i++)
				{
					this._Terrains[i].heightmapPixelError = this._OriginalTerrainPixelErrors[i];
					this._Terrains[i].drawTreesAndFoliage = this._OriginalDrawTrees[i];
				}
			}
			if (this._GameObjects != null)
			{
				for (int j = this._GameObjects.Length - 1; j >= 0; j--)
				{
					this._GameObjects[j].layer = this._OriginalRendererLayers[j];
				}
			}
		}

		private RenderTexture RenderHeightMap(int width, int height)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 32, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
			temporary.wrapMode = TextureWrapMode.Clamp;
			RenderTexture.active = temporary;
			GL.Clear(true, true, new Color(-4000f, -4000f, -4000f, -4000f), 1000000f);
			RenderTexture.active = null;
			GameObject gameObject = new GameObject();
			Camera camera = gameObject.AddComponent<Camera>();
			camera.enabled = false;
			camera.clearFlags = CameraClearFlags.Nothing;
			camera.depthTextureMode = DepthTextureMode.None;
			camera.orthographic = true;
			camera.cullingMask = 1 << WaterProjectSettings.Instance.WaterTempLayer;
			camera.nearClipPlane = 0.95f;
			camera.farClipPlane = this._Bounds.size.y + 2f;
			camera.orthographicSize = this._Bounds.size.z * 0.5f;
			camera.aspect = this._Bounds.size.x / this._Bounds.size.z;
			Vector3 center = this._Bounds.center;
			center.y = this._Bounds.max.y + 1f;
			camera.transform.position = center;
			camera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
			camera.targetTexture = temporary;
			camera.RenderWithShader((!this._HasBottomFaces) ? this._HeightMapperShader : this._HeightMapperShaderAlt, "RenderType");
			camera.targetTexture = null;
			UnityEngine.Object.Destroy(gameObject);
			return temporary;
		}

		private static void ComputeDistanceMap(Material material, RenderTexture sa, RenderTexture sb)
		{
			sa.filterMode = FilterMode.Point;
			sb.filterMode = FilterMode.Point;
			RenderTexture renderTexture = sa;
			RenderTexture renderTexture2 = sb;
			int num = (int)((float)Mathf.Max(sa.width, sa.height) * 0.7f);
			for (int i = 0; i < num; i++)
			{
				Graphics.Blit(renderTexture, renderTexture2, material, 3);
				RenderTexture renderTexture3 = renderTexture;
				renderTexture = renderTexture2;
				renderTexture2 = renderTexture3;
			}
			if (renderTexture != sb)
			{
				Graphics.Blit(renderTexture, sb, material, 3);
			}
		}

		private void ReadBackHeightMap(RenderTexture source)
		{
			int width = this._IntensityMask.width;
			int height = this._IntensityMask.height;
			this._HeightMapData = new float[width * height + width + 1];
			RenderTexture.active = source;
			Texture2D texture2D = new Texture2D(this._IntensityMask.width, this._IntensityMask.height, TextureFormat.RGBAFloat, false, true);
			texture2D.ReadPixels(new Rect(0f, 0f, (float)this._IntensityMask.width, (float)this._IntensityMask.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			int num = 0;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num2 = texture2D.GetPixel(j, i).r;
					if (num2 > 0f && num2 < 1f)
					{
						num2 = Mathf.Sqrt(num2);
					}
					this._HeightMapData[num++] = num2;
				}
			}
			UnityEngine.Object.Destroy(texture2D);
		}

		private void CreateMaskRenderer()
		{
			GameObject gameObject = new GameObject("Shoreline Mask")
			{
				hideFlags = HideFlags.DontSave,
				layer = WaterProjectSettings.Instance.WaterTempLayer
			};
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = Quads.BipolarXZ;
			this._InteractionMaskMaterial = new Material(this._MaskDisplayShader)
			{
				hideFlags = HideFlags.DontSave
			};
			this._InteractionMaskMaterial.SetTexture("_MainTex", this._IntensityMask);
			this.OnValidate();
			this._InteractionMaskRenderer = gameObject.AddComponent<MeshRenderer>();
			this._InteractionMaskRenderer.sharedMaterial = this._InteractionMaskMaterial;
			this._InteractionMaskRenderer.enabled = false;
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.position = new Vector3(this._TotalBounds.center.x, 0f, this._TotalBounds.center.z);
			gameObject.transform.localRotation = Quaternion.identity;
			Vector3 localScale = this._TotalBounds.size * 0.5f;
			localScale.x /= base.transform.localScale.x;
			localScale.y /= base.transform.localScale.y;
			localScale.z /= base.transform.localScale.z;
			gameObject.transform.localScale = localScale;
		}

		[FormerlySerializedAs("staticWaterInteractions")]
		public static List<StaticWaterInteraction> StaticWaterInteractions = new List<StaticWaterInteraction>();

		[FormerlySerializedAs("shoreSmoothness")]
		[SerializeField]
		[Tooltip("Specifies a distance from the shore over which a water gets one meter deeper (value of 50 means that water has a depth of 1m at a distance of 50m from the shore).")]
		[Range(0.001f, 80f)]
		private float _ShoreSmoothness = 50f;

		[FormerlySerializedAs("hasBottomFaces")]
		[Tooltip("If set to true, geometry that floats above water is correctly ignored.\n\nUse for objects that are closed and have faces at the bottom like basic primitives and most custom meshes, but not terrain.")]
		[SerializeField]
		private bool _HasBottomFaces;

		[FormerlySerializedAs("underwaterAreasMode")]
		[SerializeField]
		private StaticWaterInteraction.UnderwaterAreasMode _UnderwaterAreasMode;

		[FormerlySerializedAs("mapResolution")]
		[Resolution(1024, new int[]
		{
			128,
			256,
			512,
			1024,
			2048
		})]
		[SerializeField]
		private int _MapResolution = 1024;

		[FormerlySerializedAs("waveDampingThreshold")]
		[Tooltip("All waves bigger than this (in scene units) will be dampened near the shore.")]
		[SerializeField]
		private float _WaveDampingThreshold = 4f;

		[FormerlySerializedAs("depthScale")]
		[SerializeField]
		private float _DepthScale = 1f;

		[FormerlySerializedAs("maskGenerateShader")]
		[HideInInspector]
		[SerializeField]
		private Shader _MaskGenerateShader;

		[FormerlySerializedAs("maskDisplayShader")]
		[HideInInspector]
		[SerializeField]
		private Shader _MaskDisplayShader;

		[FormerlySerializedAs("heightMapperShader")]
		[HideInInspector]
		[SerializeField]
		private Shader _HeightMapperShader;

		[FormerlySerializedAs("heightMapperShaderAlt")]
		[HideInInspector]
		[SerializeField]
		private Shader _HeightMapperShaderAlt;

		private GameObject[] _GameObjects;

		private Terrain[] _Terrains;

		private int[] _OriginalRendererLayers;

		private float[] _OriginalTerrainPixelErrors;

		private bool[] _OriginalDrawTrees;

		private RenderTexture _IntensityMask;

		private MeshRenderer _InteractionMaskRenderer;

		private Material _InteractionMaskMaterial;

		private Bounds _Bounds;

		private Bounds _TotalBounds;

		private float[] _HeightMapData;

		private float _OffsetX;

		private float _OffsetZ;

		private float _ScaleX;

		private float _ScaleZ;

		private int _Width;

		private int _Height;

		public enum UnderwaterAreasMode
		{
			Generate,
			UseExisting
		}
	}
}
