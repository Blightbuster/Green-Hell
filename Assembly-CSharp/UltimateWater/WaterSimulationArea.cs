using System;
using System.Collections.Generic;
using UltimateWater.Internal;
using UltimateWater.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace UltimateWater
{
	[RequireComponent(typeof(MeshFilter))]
	[AddComponentMenu("Ultimate Water/Dynamic/Water Simulation Area")]
	public class WaterSimulationArea : MonoBehaviour, ILocalDisplacementRenderer, IDynamicWaterEffects
	{
		public WaterRipplesProfile Profile
		{
			get
			{
				return this._Profile;
			}
			set
			{
				this._Profile = value;
				this.UpdateShaderVariables();
			}
		}

		public Vector2 Resolution
		{
			get
			{
				Vector2 vector = this.Size * (float)this._PixelsPerUnit;
				return new Vector2((float)((int)vector.x), (float)((int)vector.y));
			}
		}

		public Vector2 Size
		{
			get
			{
				return new Vector2(base.transform.localScale.x * 10f, base.transform.localScale.z * 10f);
			}
			set
			{
				base.transform.localScale = new Vector3(value.x * 0.1f, 1f, value.y * 0.1f);
			}
		}

		public Vector2 DepthResolution
		{
			get
			{
				return this.Resolution * this._DepthScale;
			}
		}

		public Vector2 GetLocalPixelPosition(Vector3 globalPosition)
		{
			Vector2 a = new Vector2(base.transform.position.x, base.transform.position.z);
			Vector2 result = a - new Vector2(globalPosition.x, globalPosition.z) + this.Size * 0.5f;
			result.x *= (float)this._PixelsPerUnit;
			result.y *= (float)this._PixelsPerUnit;
			return result;
		}

		public void AddForce(List<WaterForce.Data> data, float radius = 1f)
		{
			Vector2 resolution = this.Resolution;
			int num = 0;
			for (int i = 0; i < data.Count; i++)
			{
				Vector2 localPixelPosition = this.GetLocalPixelPosition(data[i].Position);
				if (WaterSimulationArea.ContainsLocalRaw(localPixelPosition, resolution))
				{
					if (data[i].Force > 0f)
					{
						WaterSimulationArea._Array[num * 4] = localPixelPosition.x;
						WaterSimulationArea._Array[num * 4 + 1] = localPixelPosition.y;
						WaterSimulationArea._Array[num * 4 + 2] = data[i].Force * 500f;
						WaterSimulationArea._Array[num * 4 + 3] = 0f;
						num++;
						if (num == 512)
						{
							this.DispatchAddForce(num);
							num = 0;
						}
					}
				}
			}
			this.DispatchAddForce(num);
		}

		private void Awake()
		{
			this._MeshFilter = base.GetComponent<MeshFilter>();
			this._TranslateMaterial = new Material(ShaderUtility.Instance.Get(ShaderList.Translate));
			if (this._Water == null)
			{
				this._Water = Utilities.GetWaterReference();
			}
			if (this._DisplacementMaterial == null)
			{
				Material source = Resources.Load<Material>("Systems/Ultimate Water System/Materials/Overlay (Displacements)");
				this._DisplacementMaterial = new Material(source);
			}
		}

		private void OnEnable()
		{
			this._Position = base.transform.position;
			this._Scale = base.transform.localScale;
			this._CommandBuffer = new CommandBuffer
			{
				name = "[Ultimate Water]: Water Simulation Area"
			};
			this.CreateDepthCamera();
			this.CreateTextures();
			this.UpdateShaderVariables();
			this.RenderStaticDepthTexture();
			DynamicWater.AddRenderer<WaterSimulationArea>(this);
			WaterRipples.Register(this);
		}

		private void OnDisable()
		{
			WaterRipples.Unregister(this);
			DynamicWater.RemoveRenderer<WaterSimulationArea>(this);
			this.ReleaseDepthCamera();
			this.ReleaseTextures();
			if (this._CommandBuffer != null)
			{
				this._CommandBuffer.Release();
				this._CommandBuffer = null;
			}
		}

		private void OnDestroy()
		{
			this.OnDisable();
		}

		private void Update()
		{
			base.transform.position.Set(base.transform.position.x, this._Water.transform.position.y, base.transform.position.z);
			base.transform.rotation = Quaternion.identity;
			base.transform.localScale.Set(this._Scale.x, 1f, this._Scale.z);
			this.RenderDepth();
		}

		private void OnValidate()
		{
			ShaderUtility.Instance.Use(ShaderList.Simulation);
			ShaderUtility.Instance.Use(ShaderList.Translate);
			ShaderUtility.Instance.Use(ShaderList.Velocity);
			ShaderUtility.Instance.Use(ShaderList.Depth);
		}

		private void Reset()
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
			MeshFilter component = base.GetComponent<MeshFilter>();
			component.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			component.hideFlags = HideFlags.HideInInspector;
			UnityEngine.Object.DestroyImmediate(gameObject);
		}

		private Material _RipplesMaterial
		{
			get
			{
				if (this._RipplesMaterialCache != null)
				{
					return this._RipplesMaterialCache;
				}
				Shader shader = ShaderUtility.Instance.Get(ShaderList.Simulation);
				this._RipplesMaterialCache = new Material(shader);
				return this._RipplesMaterialCache;
			}
		}

		private int _Width
		{
			get
			{
				return (int)this.Resolution.x;
			}
		}

		private int _Height
		{
			get
			{
				return (int)this.Resolution.y;
			}
		}

		private int _DepthWidth
		{
			get
			{
				return (int)(this.Resolution.x * this._DepthScale);
			}
		}

		private int _DepthHeight
		{
			get
			{
				return (int)(this.Resolution.y * this._DepthScale);
			}
		}

		private int _StaticWidth
		{
			get
			{
				return (int)(this.Resolution.x * this._StaticDepthScale);
			}
		}

		private int _StaticHeight
		{
			get
			{
				return (int)(this.Resolution.y * this._StaticDepthScale);
			}
		}

		internal void Simulate()
		{
			Shader.SetGlobalFloat("_WaterHeight", this._Water.transform.position.y);
			if (base.transform.position.x != this._Position.x || base.transform.position.z != this._Position.z)
			{
				this.RenderStaticDepthTexture();
				this.MoveSimulation();
				this._Position = base.transform.position;
			}
			if (this._Buffers[0].Verify(false) || this._Buffers[1].Verify(false))
			{
				this._Buffers[0].Clear(Color.clear, false, true);
				this._Buffers[1].Clear(Color.clear, false, true);
			}
			RipplesShader.SetPrimary(this._Buffers[0], this._RipplesMaterial);
			RipplesShader.SetSecondary(this._Buffers[1], this._RipplesMaterial);
			WaterRipplesData.ShaderModes shaderMode = WaterQualitySettings.Instance.Ripples.ShaderMode;
			if (shaderMode != WaterRipplesData.ShaderModes.ComputeShader)
			{
				if (shaderMode == WaterRipplesData.ShaderModes.PixelShader)
				{
					this.SimulatePixelShader();
				}
			}
			else
			{
				this.SimulateComputeShader();
			}
		}

		internal void Smooth()
		{
			if (this.Profile.Sigma <= 0.1f)
			{
				return;
			}
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(this._Width, this._Height, 0, WaterQualitySettings.Instance.Ripples.SimulationFormat, true, true, false);
			temporary.Verify(true);
			GaussianShader.VerticalInput = this._Buffers[1];
			GaussianShader.VerticalOutput = temporary;
			GaussianShader.Dispatch(GaussianShader.KernelType.Vertical, this._Width, this._Height);
			GaussianShader.HorizontalInput = temporary;
			GaussianShader.HorizontalOutput = this._Buffers[1];
			GaussianShader.Dispatch(GaussianShader.KernelType.Horizontal, this._Width, this._Height);
			temporary.Dispose();
		}

		internal void Swap()
		{
			TextureUtility.Swap<RenderTexture>(ref this._Buffers[0], ref this._Buffers[1]);
		}

		internal void UpdateShaderVariables()
		{
			if (this.Profile == null)
			{
				return;
			}
			if (this._DisplacementMaterial.IsNullReference(this))
			{
				return;
			}
			float num = (float)this._PixelsPerUnit / 32f;
			RipplesShader.SetPropagation(this.Profile.Propagation * num, this._RipplesMaterial);
			RipplesShader.SetStaticDepth(DefaultTextures.Get(Color.clear), this._RipplesMaterial);
			RipplesShader.SetDamping(this.Profile.Damping / 32f, this._RipplesMaterial);
			RipplesShader.SetGain(this.Profile.Gain, this._RipplesMaterial);
			RipplesShader.SetHeightGain(this.Profile.HeightGain, this._RipplesMaterial);
			RipplesShader.SetHeightOffset(this.Profile.HeightOffset, this._RipplesMaterial);
			float[] array = UltimateWater.Utils.Math.GaussianTerms(this.Profile.Sigma);
			GaussianShader.Term0 = array[0];
			GaussianShader.Term1 = array[1];
			GaussianShader.Term2 = array[2];
			this._DisplacementMaterial.SetFloat("_Amplitude", this.Profile.Amplitude * 0.05f);
			this._DisplacementMaterial.SetFloat("_Spread", this.Profile.Spread);
			this._DisplacementMaterial.SetFloat("_Multiplier", this.Profile.Multiplier);
			this._DisplacementMaterial.SetFloat("_Fadeout", (!this._Fade) ? 0f : 1f);
			this._DisplacementMaterial.SetFloat("_FadePower", this._FadePower);
		}

		private void DispatchAddForce(int count)
		{
			if (count <= 0)
			{
				return;
			}
			ComputeShader shader = SetupShader.Shader;
			shader.SetTexture(SetupShader.Multi, "Previous", this._Buffers[0]);
			shader.SetTexture(SetupShader.Multi, "Current", this._Buffers[1]);
			shader.SetFloats("data", WaterSimulationArea._Array);
			shader.Dispatch(SetupShader.Multi, count, 1, 1);
		}

		private void SimulateComputeShader()
		{
			RipplesShader.Size = this.Resolution;
			RipplesShader.Dispatch(this._Width, this._Height);
		}

		private void SimulatePixelShader()
		{
			RenderTexture renderTexture = this._Buffers[1].CreateTemporary();
			renderTexture.Clear(Color.clear, false, true);
			Graphics.Blit(null, renderTexture, this._RipplesMaterial);
			Graphics.Blit(renderTexture, this._Buffers[1]);
			renderTexture.ReleaseTemporary();
		}

		private void CreateDepthCamera()
		{
			if (this.Resolution.x <= 0f || this.Resolution.y <= 0f)
			{
				Debug.LogError("WaterSimulationArea: invalid resolution");
			}
			GameObject gameObject = new GameObject("Depth");
			gameObject.transform.SetParent(base.transform);
			this._DepthCamera = gameObject.AddComponent<Camera>();
			this._DepthCamera.enabled = false;
			this._DepthCamera.backgroundColor = Color.clear;
			this._DepthCamera.clearFlags = CameraClearFlags.Color;
			this._DepthCamera.orthographic = true;
			this._DepthCamera.orthographicSize = this.Size.y * 0.5f;
			this._DepthCamera.aspect = this.Resolution.x / this.Resolution.y;
			this._DepthCamera.transform.localRotation = Quaternion.Euler(new Vector3(90f, 180f, 0f));
			this._DepthCamera.transform.localScale = Vector3.one;
			this._DepthCamera.depthTextureMode = DepthTextureMode.Depth;
			WaterSimulationArea.SetDepthCameraParameters(ref this._DepthCamera, 10f, 0f, 20f);
		}

		private void ReleaseDepthCamera()
		{
			if (this._DepthCamera != null)
			{
				UnityEngine.Object.Destroy(this._DepthCamera.gameObject);
			}
			this._DepthCamera = null;
		}

		private void RenderDepth()
		{
			if (this._DepthCamera.IsNullReference(this))
			{
				return;
			}
			TextureUtility.Swap<RenderTexture>(ref this._Depth, ref this._PreviousDepth);
			GL.PushMatrix();
			GL.modelview = this._DepthCamera.worldToCameraMatrix;
			GL.LoadProjectionMatrix(this._DepthCamera.projectionMatrix);
			this._CommandBuffer.Clear();
			this._CommandBuffer.SetRenderTarget(this._Depth);
			this._CommandBuffer.ClearRenderTarget(true, true, Color.clear);
			List<IWavesInteractive> interactions = DynamicWater.Interactions;
			for (int i = 0; i < interactions.Count; i++)
			{
				interactions[i].Render(this._CommandBuffer);
			}
			Graphics.ExecuteCommandBuffer(this._CommandBuffer);
			GL.PopMatrix();
			RipplesShader.SetDepth(this._Depth, this._RipplesMaterial);
			RipplesShader.SetPreviousDepth(this._PreviousDepth, this._RipplesMaterial);
		}

		private void MoveSimulation()
		{
			Vector3 vector = this._Position - base.transform.position;
			float x = vector.x * (float)this._PixelsPerUnit;
			float y = vector.z * (float)this._PixelsPerUnit;
			this._TranslateMaterial.SetVector("_Offset", new Vector4(x, y, 0f, 0f));
			for (int i = 0; i < 2; i++)
			{
				Graphics.Blit(this._Buffers[i], this._Buffers[2], this._TranslateMaterial);
				TextureUtility.Swap<RenderTexture>(ref this._Buffers[i], ref this._Buffers[2]);
			}
			this.SendSimulationMatrix(this._Buffers[1]);
		}

		private static bool ContainsLocalRaw(Vector2 localPosition, Vector2 resolution)
		{
			return localPosition.x >= 0f && localPosition.x < resolution.x && localPosition.y >= 0f && localPosition.y < resolution.y;
		}

		private void RenderStaticDepthTexture()
		{
			if (!this._EnableStaticCalculations)
			{
				if (this._StaticDepth != null)
				{
					this._StaticDepth.Release();
					this._StaticDepth = null;
				}
				return;
			}
			if (this._DepthCamera.IsNullReference(this))
			{
				return;
			}
			if (this._StaticDepth == null)
			{
				TextureUtility.RenderTextureDesc desc = new TextureUtility.RenderTextureDesc("[UWS] - Static Depth")
				{
					Width = this._StaticWidth,
					Height = this._StaticHeight,
					Depth = 24,
					Format = RenderTextureFormat.RFloat,
					Filter = FilterMode.Point
				};
				this._StaticDepth = desc.CreateRenderTexture();
				this._StaticDepth.Clear(Color.clear, false, true);
			}
			float y = this._DepthCamera.transform.localPosition.y;
			float nearClipPlane = this._DepthCamera.nearClipPlane;
			float farClipPlane = this._DepthCamera.farClipPlane;
			WaterSimulationArea.SetDepthCameraParameters(ref this._DepthCamera, 40f, 0f, 80f);
			this._DepthCamera.cullingMask = WaterQualitySettings.Instance.Ripples.StaticDepthMask;
			this._DepthCamera.targetTexture = this._StaticDepth;
			this._DepthCamera.SetReplacementShader(ShaderUtility.Instance.Get(ShaderList.Depth), string.Empty);
			this._DepthCamera.Render();
			WaterSimulationArea.SetDepthCameraParameters(ref this._DepthCamera, y, nearClipPlane, farClipPlane);
			RipplesShader.SetStaticDepth(this._StaticDepth, this._RipplesMaterial);
		}

		private void CreateTextures()
		{
			this.ReleaseTextures();
			TextureUtility.RenderTextureDesc desc = new TextureUtility.RenderTextureDesc("[UWS] WaterSimulationArea - Simulation")
			{
				Width = this._Width,
				Height = this._Height,
				Format = WaterQualitySettings.Instance.Ripples.SimulationFormat,
				Filter = FilterMode.Bilinear,
				EnableRandomWrite = true
			};
			for (int i = 0; i < 3; i++)
			{
				this._Buffers[i] = desc.CreateRenderTexture();
				this._Buffers[i].name = "[UWS] WaterSimulationArea - Buffer[" + i + "]";
				this._Buffers[i].Clear(Color.clear, false, true);
			}
			this.SendSimulationMatrix(this._Buffers[1]);
			TextureUtility.RenderTextureDesc desc2 = new TextureUtility.RenderTextureDesc("[UWS] WaterSimulationArea - Depth")
			{
				Width = this._DepthWidth,
				Height = this._DepthHeight,
				Depth = 24,
				Format = RenderTextureFormat.ARGBHalf,
				Filter = FilterMode.Point
			};
			this._Depth = desc2.CreateRenderTexture();
			this._PreviousDepth = desc2.CreateRenderTexture();
			this._Depth.Clear(Color.clear, false, true);
			this._PreviousDepth.Clear(Color.clear, false, true);
		}

		private void ReleaseTextures()
		{
			TextureUtility.Release(ref this._Depth);
			TextureUtility.Release(ref this._PreviousDepth);
			TextureUtility.Release(ref this._StaticDepth);
			TextureUtility.Release(ref this._Buffers[0]);
			TextureUtility.Release(ref this._Buffers[1]);
			TextureUtility.Release(ref this._Buffers[2]);
		}

		private void SendSimulationMatrix(Texture texture)
		{
			this._DisplacementMaterial.SetTexture("_SimulationMatrix", texture);
		}

		private static void SetDepthCameraParameters(ref Camera camera, float height = 10f, float near = 0f, float far = 20f)
		{
			camera.nearClipPlane = near;
			camera.farClipPlane = far;
			camera.transform.localPosition = new Vector3(0f, height, 0f);
		}

		public void RenderLocalDisplacement(CommandBuffer commandBuffer, DynamicWaterCameraData overlays)
		{
			commandBuffer.DrawMesh(this._MeshFilter.sharedMesh, base.transform.localToWorldMatrix, this._DisplacementMaterial);
		}

		public void Enable()
		{
		}

		public void Disable()
		{
		}

		[SerializeField]
		private Water _Water;

		[SerializeField]
		private WaterRipplesProfile _Profile;

		[SerializeField]
		[Range(1f, 32f)]
		[Header("Settings")]
		[Tooltip("How many simulation pixels per one unit are used")]
		private int _PixelsPerUnit = 16;

		[Range(0.125f, 2f)]
		[SerializeField]
		[Tooltip("What resolution is used for dynamic depth rendering")]
		private float _DepthScale = 0.5f;

		[SerializeField]
		[Tooltip("Does the water can be  stopped by Blocking objects")]
		private bool _EnableStaticCalculations;

		[SerializeField]
		[Tooltip("What resolution is used for static depth information")]
		[Range(0.125f, 2f)]
		private float _StaticDepthScale = 1f;

		[SerializeField]
		[Header("Edge fade")]
		private bool _Fade;

		[SerializeField]
		private float _FadePower = 0.5f;

		private Camera _DepthCamera;

		private readonly RenderTexture[] _Buffers = new RenderTexture[3];

		private RenderTexture _PreviousDepth;

		private RenderTexture _Depth;

		private RenderTexture _StaticDepth;

		private Material _RipplesMaterialCache;

		private MeshFilter _MeshFilter;

		[SerializeField]
		[HideInInspector]
		private Material _DisplacementMaterial;

		private Vector3 _Position;

		private Vector3 _Scale;

		private Material _TranslateMaterial;

		private CommandBuffer _CommandBuffer;

		private static readonly float[] _Array = new float[2048];

		private const int _MaxArrayElements = 512;
	}
}
