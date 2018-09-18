using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[RequireComponent(typeof(Light))]
	public sealed class LightWaterEffects : MonoBehaviour
	{
		public Light UnityLight
		{
			get
			{
				return this._LocalLight;
			}
		}

		public LightWaterEffects.CausticsMode Mode
		{
			get
			{
				return this._CausticsMode;
			}
			set
			{
				this._CausticsMode = value;
				if (this._CausticsMode == LightWaterEffects.CausticsMode.None)
				{
					this.ResetCausticMaps();
					Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraGlobalPreCull));
				}
				else
				{
					if (this._RenderCamera == null)
					{
						this.CreateCausticsCamera();
					}
					if (Camera.onPreCull != null)
					{
						Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraGlobalPreCull));
					}
					Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraGlobalPreCull));
				}
			}
		}

		private void Awake()
		{
			LightWaterEffects._ShadowmapId = ShaderVariables.WaterShadowmap;
			this._TerrainSettingTemp = new bool[32];
			this._LocalLight = base.GetComponent<Light>();
			this._CausticUtilMat = new Material(this._CausticUtilShader)
			{
				hideFlags = HideFlags.DontSave
			};
		}

		private void OnEnable()
		{
			LightWaterEffects.Lights.Add(this);
			this.OnValidate();
			if (this._CausticsMode != LightWaterEffects.CausticsMode.None)
			{
				this.CreateCausticsCamera();
			}
			else
			{
				this.ResetCausticMaps();
			}
			this._Id = LightWaterEffects.Lights.Count - 1;
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraGlobalPreCull));
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraGlobalPreCull));
		}

		private void OnDisable()
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(this.OnSomeCameraGlobalPreCull));
			this.ResetCausticMaps();
			LightWaterEffects.Lights.Remove(this);
			TextureUtility.Release(ref this._WorldPosMap);
			TextureUtility.Release(ref this._CausticsMap);
			if (this._RenderCamera != null)
			{
				this._RenderCamera.gameObject.Destroy();
				this._RenderCamera = null;
			}
			if (this._WaterCamera != null)
			{
				this._WaterCamera.gameObject.Destroy();
				this._WaterCamera = null;
			}
		}

		private void Update()
		{
			if (this._RenderCamera == null)
			{
				this.CreateCausticsCamera();
			}
			if (this.ScrollDirectionPointer != null)
			{
				Vector3 forward = this.ScrollDirectionPointer.forward;
				float num = this.ScrollSpeed * this.UvScale * Time.deltaTime;
				this._Scroll.x = this._Scroll.x + forward.x * num;
				this._Scroll.y = this._Scroll.y + forward.z * num;
			}
			else
			{
				float num2 = 0.7f * this.ScrollSpeed * this.UvScale * Time.deltaTime;
				this._Scroll.x = this._Scroll.x + num2;
				this._Scroll.y = this._Scroll.y + num2;
			}
		}

		private void OnValidate()
		{
			if (this._WorldPosShader == null)
			{
				this._WorldPosShader = Shader.Find("UltimateWater/Caustics/WorldPos");
			}
			if (this._CausticsMapShader == null)
			{
				this._CausticsMapShader = Shader.Find("UltimateWater/Caustics/Map");
			}
			if (this._NormalMapperShader == null)
			{
				this._NormalMapperShader = Shader.Find("UltimateWater/Caustics/NormalMapper");
			}
			if (this._CausticUtilShader == null)
			{
				this._CausticUtilShader = Shader.Find("UltimateWater/Caustics/Utility");
			}
			this._Blur.Validate();
			if (this._CausticsMode == LightWaterEffects.CausticsMode.None)
			{
				this.ResetCausticMaps();
			}
		}

		public void PrepareRenderingOnCamera(WaterCamera targetCamera)
		{
			if (this._RenderingPrepared || !base.isActiveAndEnabled)
			{
				return;
			}
			this._RenderingPrepared = true;
			if (this.CastShadows)
			{
				this.PrepareShadows(targetCamera.CameraComponent);
			}
			this.PrepareCaustics(targetCamera);
		}

		public void CleanRenderingOnCamera()
		{
			if (!this._RenderingPrepared)
			{
				return;
			}
			this._RenderingPrepared = false;
			if (this._CopyShadowmap != null)
			{
				this._LocalLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, this._CopyShadowmap);
			}
		}

		public void AddWorldSpaceOffset(Vector3 offset)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			this._Offset.x = this._Offset.x + Vector3.Dot(offset, this._RenderCamera.transform.right) * this.UvScale / (this._RenderCamera.orthographicSize * 2f);
			this._Offset.y = this._Offset.y + Vector3.Dot(offset, this._RenderCamera.transform.up) * this.UvScale / (this._RenderCamera.orthographicSize * 2f);
		}

		private void OnSomeCameraGlobalPreCull(Camera cameraComponent)
		{
			if (this._RenderingPrepared)
			{
				base.transform.position = new Vector3((float)this._Id, 6.137f, 0f);
			}
		}

		private void CreateCausticsCamera()
		{
			if (this._CausticsMode == LightWaterEffects.CausticsMode.ProjectedTexture)
			{
				this._CausticsMap = new RenderTexture(256, 256, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear)
				{
					hideFlags = HideFlags.DontSave,
					wrapMode = TextureWrapMode.Repeat,
					name = "[UWS] LightWaterEffects - Caustics Map"
				};
			}
			else
			{
				this._WorldPosMap = new RenderTexture(256, 256, 32, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear)
				{
					hideFlags = HideFlags.DontSave,
					wrapMode = TextureWrapMode.Clamp,
					name = "[UWS] LightWaterEffects - WorldPosMap"
				};
				this._CausticsMap = new RenderTexture(512, 512, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
				{
					hideFlags = HideFlags.DontSave,
					wrapMode = TextureWrapMode.Clamp,
					name = "[UWS] LightWaterEffects - CausticsMap"
				};
			}
			GameObject gameObject = new GameObject("Caustic Camera")
			{
				hideFlags = HideFlags.DontSave
			};
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = base.transform.rotation;
			this._RenderCamera = gameObject.AddComponent<Camera>();
			this._RenderCamera.enabled = false;
			this._RenderCamera.orthographic = true;
			this._RenderCamera.orthographicSize = 85f;
			this._RenderCamera.farClipPlane = 5000f;
			this._RenderCamera.depthTextureMode = DepthTextureMode.None;
			this._RenderCamera.allowHDR = true;
			this._RenderCamera.useOcclusionCulling = false;
			this._RenderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			this._RenderCamera.renderingPath = RenderingPath.VertexLit;
			this._WaterCamera = gameObject.AddComponent<WaterCamera>();
			this._WaterCamera.RenderWaterDepth = false;
			this._WaterCamera.RenderVolumes = false;
			this._WaterCamera.Type = WaterCamera.CameraType.Effect;
			this._WaterCamera.GeometryType = WaterGeometryType.UniformGrid;
			gameObject.hideFlags |= HideFlags.HideInHierarchy;
		}

		private void PrepareShadows(Camera cameraComponent)
		{
			if (this._CopyShadowmap == null)
			{
				this._CopyShadowmap = new CommandBuffer
				{
					name = "[UWS] LightWaterEffects._CopyShadowmap"
				};
			}
			Shader.SetGlobalTexture(LightWaterEffects._ShadowmapId, DefaultTextures.Get(Color.white));
			this._CopyShadowmap.Clear();
			this._CopyShadowmap.GetTemporaryRT(LightWaterEffects._ShadowmapId, cameraComponent.pixelWidth, cameraComponent.pixelHeight, 32, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			this._CopyShadowmap.Blit(BuiltinRenderTextureType.CurrentActive, LightWaterEffects._ShadowmapId);
			this._CopyShadowmap.ReleaseTemporaryRT(LightWaterEffects._ShadowmapId);
			this._LocalLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, this._CopyShadowmap);
			this._LocalLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, this._CopyShadowmap);
		}

		private void PrepareCaustics(WaterCamera waterCamera)
		{
			LightWaterEffects.CausticsMode causticsMode = this._CausticsMode;
			if (causticsMode != LightWaterEffects.CausticsMode.ProjectedTexture)
			{
				if (causticsMode == LightWaterEffects.CausticsMode.Raymarching)
				{
					this.UpdateCausticsCameraPosition(waterCamera);
					this.RenderRaymarchedCaustics(waterCamera);
				}
			}
			else
			{
				this.UpdateCausticsCameraPosition(waterCamera);
				this.RenderProjectedTextureCaustics();
			}
		}

		private void UpdateCausticsCameraPosition(WaterCamera waterCamera)
		{
			Vector2 center = waterCamera.LocalMapsRect.center;
			this._RenderCamera.transform.position = new Vector3(center.x, 0f, center.y) - base.transform.forward * Mathf.Max(Mathf.Abs(waterCamera.transform.position.y * 2.2f), 300f);
			this._RenderCamera.transform.rotation = base.transform.rotation;
		}

		private void RenderProjectedTextureCaustics()
		{
			this._RenderCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
			this._WaterCamera.RenderWaterWithShader("[PW Water] Caustics Normal Map", this._CausticsMap, this._NormalMapperShader, true, false, false);
			Vector3 position = this._RenderCamera.transform.position;
			float num = Vector3.Dot(position, this._RenderCamera.transform.right) * this.UvScale / (this._RenderCamera.orthographicSize * 3.5f);
			float num2 = Vector3.Dot(position, this._RenderCamera.transform.up) * this.UvScale / (this._RenderCamera.orthographicSize * 2f);
			Shader.SetGlobalTexture("_CausticsMap", this.ProjectedTexture);
			Shader.SetGlobalTexture("_CausticsDistortionMap", this._CausticsMap);
			Shader.SetGlobalFloat("_CausticsMultiplier", this.Intensity * 5f);
			Shader.SetGlobalVector("_CausticsOffsetScale", new Vector4(this._Offset.x + this._Scroll.x + num, this._Offset.y + this._Scroll.y + num2, this.UvScale, this.Distortions1 * 0.02f));
			Shader.SetGlobalVector("_CausticsOffsetScale2", new Vector4(this._Offset.x - this._Scroll.x + num + 0.5f, this._Offset.y - this._Scroll.y + num2, this.UvScale, this.Distortions2 * 0.02f));
			Shader.SetGlobalMatrix("_CausticsMapProj", GL.GetGPUProjectionMatrix(this._RenderCamera.projectionMatrix, true) * this._RenderCamera.worldToCameraMatrix);
		}

		private void RenderRaymarchedCaustics(WaterCamera waterCamera)
		{
			this._CausticUtilMat.SetMatrix("_InvProjMatrix", Matrix4x4.Inverse(this._RenderCamera.projectionMatrix * this._RenderCamera.worldToCameraMatrix));
			Graphics.SetRenderTarget(this._WorldPosMap);
			GL.Clear(true, true, new Color(1f, 0f, 0f, 0f), 1f);
			Terrain[] array = null;
			if (this._SkipTerrainTrees)
			{
				array = Terrain.activeTerrains;
				if (this._TerrainSettingTemp.Length < array.Length)
				{
					Array.Resize<bool>(ref this._TerrainSettingTemp, array.Length * 2);
				}
				for (int i = 0; i < array.Length; i++)
				{
					this._TerrainSettingTemp[i] = array[i].drawTreesAndFoliage;
					array[i].drawTreesAndFoliage = false;
				}
			}
			this._WaterCamera.enabled = false;
			this._RenderCamera.orthographicSize = waterCamera.LocalMapsRect.width * 0.6f;
			this._RenderCamera.clearFlags = CameraClearFlags.Depth;
			this._RenderCamera.cullingMask = this._CausticReceiversMask;
			this._RenderCamera.targetTexture = this._WorldPosMap;
			this._RenderCamera.RenderWithShader(this._WorldPosShader, "RenderType");
			if (this._SkipTerrainTrees)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j].drawTreesAndFoliage = this._TerrainSettingTemp[j];
				}
			}
			Shader.SetGlobalTexture("_WorldPosMap", this._WorldPosMap);
			Shader.SetGlobalVector("_CausticLightDir", base.transform.forward);
			Shader.SetGlobalFloat("_CausticLightIntensity", this._LocalLight.intensity * this.Intensity * 1.5f);
			this._WaterCamera.enabled = true;
			this._RenderCamera.clearFlags = CameraClearFlags.Color;
			this._RenderCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
			this._RenderCamera.targetTexture = this._CausticsMap;
			this._RenderCamera.RenderWithShader(this._CausticsMapShader, "CustomType");
			this._Blur.Apply(this._CausticsMap);
			Graphics.Blit(null, this._CausticsMap, this._CausticUtilMat, 1);
			Shader.SetGlobalTexture("_CausticsMap", this._CausticsMap);
			Shader.SetGlobalFloat("_CausticsMultiplier", 1f);
			Shader.SetGlobalMatrix("_CausticsMapProj", GL.GetGPUProjectionMatrix(this._RenderCamera.projectionMatrix, true) * this._RenderCamera.worldToCameraMatrix);
		}

		private void ResetCausticMaps()
		{
			Shader.SetGlobalFloat("_CausticsMultiplier", 0f);
		}

		[Range(0f, 3f)]
		public float Intensity = 1f;

		public bool CastShadows = true;

		public Texture2D ProjectedTexture;

		[SerializeField]
		public float UvScale = 1f;

		[Range(0f, 0.25f)]
		public float ScrollSpeed = 0.01f;

		[Range(0f, 8f)]
		public float Distortions1 = 1f;

		[Range(0f, 8f)]
		public float Distortions2 = 1f;

		[Tooltip("Optional.")]
		[SerializeField]
		public Transform ScrollDirectionPointer;

		[SerializeField]
		[HideInInspector]
		private Shader _WorldPosShader;

		[SerializeField]
		[HideInInspector]
		private Shader _CausticsMapShader;

		[SerializeField]
		[HideInInspector]
		private Shader _NormalMapperShader;

		[HideInInspector]
		[SerializeField]
		private Shader _CausticUtilShader;

		[SerializeField]
		private LightWaterEffects.CausticsMode _CausticsMode = LightWaterEffects.CausticsMode.ProjectedTexture;

		[SerializeField]
		private LayerMask _CausticReceiversMask = int.MaxValue;

		[SerializeField]
		private Blur _Blur;

		[SerializeField]
		[Tooltip("Causes minor allocation per frame (no way around it), but makes caustics rendering a lot faster. Disable it, if you don't use terrains.")]
		private bool _SkipTerrainTrees = true;

		private Camera _RenderCamera;

		private WaterCamera _WaterCamera;

		private Material _CausticUtilMat;

		private Light _LocalLight;

		private Vector2 _Offset;

		private Vector2 _Scroll;

		private bool _RenderingPrepared;

		private bool[] _TerrainSettingTemp;

		private int _Id;

		private CommandBuffer _CopyShadowmap;

		private RenderTexture _WorldPosMap;

		private RenderTexture _CausticsMap;

		public static readonly List<LightWaterEffects> Lights = new List<LightWaterEffects>();

		private static int _ShadowmapId;

		public enum CausticsMode
		{
			None,
			ProjectedTexture,
			Raymarching
		}
	}
}
