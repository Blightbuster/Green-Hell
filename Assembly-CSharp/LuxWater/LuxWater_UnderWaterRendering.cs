using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater
{
	[RequireComponent(typeof(Camera))]
	public class LuxWater_UnderWaterRendering : MonoBehaviour
	{
		private void OnEnable()
		{
			if (LuxWater_UnderWaterRendering.instance != null)
			{
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				LuxWater_UnderWaterRendering.instance = this;
			}
			this.mat = new Material(Shader.Find("Lux Water/WaterMask"));
			this.blurMaterial = new Material(Shader.Find("Lux Water/BlurEffectConeTap"));
			this.blitMaterial = new Material(Shader.Find("Lux Water/UnderWaterPost"));
			this.cam = base.GetComponent<Camera>();
			this.cam.depthTextureMode |= DepthTextureMode.Depth;
			this.camTransform = base.transform;
			this.UnderWaterMaskPID = Shader.PropertyToID("_UnderWaterMask");
			this.Lux_FrustumCornersWSPID = Shader.PropertyToID("_Lux_FrustumCornersWS");
			this.Lux_CameraWSPID = Shader.PropertyToID("_Lux_CameraWS");
			this.GerstnerEnabledPID = Shader.PropertyToID("_GerstnerEnabled");
			this.LuxWaterMask_GerstnerVertexIntensityPID = Shader.PropertyToID("_LuxWaterMask_GerstnerVertexIntensity");
			this.GerstnerVertexIntensityPID = Shader.PropertyToID("_GerstnerVertexIntensity");
			this.LuxWaterMask_GAmplitudePID = Shader.PropertyToID("_LuxWaterMask_GAmplitude");
			this.GAmplitudePID = Shader.PropertyToID("_GAmplitude");
			this.LuxWaterMask_GFinalFrequencyPID = Shader.PropertyToID("_LuxWaterMask_GFinalFrequency");
			this.GFinalFrequencyPID = Shader.PropertyToID("_GFinalFrequency");
			this.LuxWaterMask_GSteepnessPID = Shader.PropertyToID("_LuxWaterMask_GSteepness");
			this.GSteepnessPID = Shader.PropertyToID("_GSteepness");
			this.LuxWaterMask_GFinalSpeedPID = Shader.PropertyToID("_LuxWaterMask_GFinalSpeed");
			this.GFinalSpeedPID = Shader.PropertyToID("_GFinalSpeed");
			this.LuxWaterMask_GDirectionABPID = Shader.PropertyToID("_LuxWaterMask_GDirectionAB");
			this.GDirectionABPID = Shader.PropertyToID("_GDirectionAB");
			this.LuxWaterMask_GDirectionCDPID = Shader.PropertyToID("_LuxWaterMask_GDirectionCD");
			this.GDirectionCDPID = Shader.PropertyToID("_GDirectionCD");
			this.Lux_UnderWaterAmbientSkyLightPID = Shader.PropertyToID("_Lux_UnderWaterAmbientSkyLight");
			this.Lux_UnderWaterSunColorPID = Shader.PropertyToID("_Lux_UnderWaterSunColor");
			this.Lux_UnderWaterSunDirPID = Shader.PropertyToID("_Lux_UnderWaterSunDir");
			this.islinear = (QualitySettings.desiredColorSpace == ColorSpace.Linear);
		}

		private void OnDisable()
		{
			LuxWater_UnderWaterRendering.instance = null;
			if (this.UnderWaterMask != null)
			{
				this.UnderWaterMask = null;
			}
			if (this.mat)
			{
				UnityEngine.Object.DestroyImmediate(this.mat);
			}
			if (this.blurMaterial)
			{
				UnityEngine.Object.DestroyImmediate(this.blurMaterial);
			}
			if (this.blitMaterial)
			{
				UnityEngine.Object.DestroyImmediate(this.blitMaterial);
			}
		}

		public void RegisterWaterVolume(LuxWater_WaterVolume item)
		{
			this.RegisteredWaterVolumes.Add(item);
			this.WaterMeshes.Add(item.WaterVolumeMesh);
			this.WaterMaterials.Add(item.transform.GetComponent<Renderer>().sharedMaterial);
			this.WaterTransforms.Add(item.transform);
		}

		public void DeRegisterWaterVolume(LuxWater_WaterVolume item)
		{
			int num = this.RegisteredWaterVolumes.IndexOf(item);
			if (this.activeWaterVolume == num)
			{
				this.activeWaterVolume = -1;
			}
			this.RegisteredWaterVolumes.RemoveAt(num);
			this.WaterMeshes.RemoveAt(num);
			this.WaterMaterials.RemoveAt(num);
			this.WaterTransforms.RemoveAt(num);
		}

		public void EnteredWaterVolume(LuxWater_WaterVolume item)
		{
			this.DoUnderWaterRendering = true;
			int num = this.RegisteredWaterVolumes.IndexOf(item);
			if (num != this.activeWaterVolume)
			{
				this.activeWaterVolume = num;
				this.WaterSurfacePos = this.WaterTransforms[this.activeWaterVolume].position.y;
				for (int i = 0; i < this.m_aboveWatersurface.Count; i++)
				{
					this.m_aboveWatersurface[i].renderQueue = 2998;
				}
				for (int j = 0; j < this.m_belowWatersurface.Count; j++)
				{
					this.m_belowWatersurface[j].renderQueue = 3001;
				}
			}
		}

		public void LeftWaterVolume(LuxWater_WaterVolume item)
		{
			this.DoUnderWaterRendering = false;
			int num = this.RegisteredWaterVolumes.IndexOf(item);
			if (this.activeWaterVolume == num)
			{
				this.activeWaterVolume = -1;
				for (int i = 0; i < this.m_aboveWatersurface.Count; i++)
				{
					this.m_aboveWatersurface[i].renderQueue = 3000;
				}
				for (int j = 0; j < this.m_belowWatersurface.Count; j++)
				{
					this.m_belowWatersurface[j].renderQueue = 2998;
				}
			}
		}

		private void OnPreCull()
		{
			this.cam = base.GetComponent<Camera>();
			this.cam.depthTextureMode |= DepthTextureMode.Depth;
			this.camTransform = this.cam.transform;
			Shader.SetGlobalFloat("_Lux_Time", Time.timeSinceLevelLoad);
			if (!this.UnderWaterMask)
			{
				this.UnderWaterMask = new RenderTexture(this.cam.pixelWidth, this.cam.pixelHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			}
			else if (this.UnderWaterMask.width != this.cam.pixelWidth)
			{
				this.UnderWaterMask = new RenderTexture(this.cam.pixelWidth, this.cam.pixelHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			}
			Graphics.SetRenderTarget(this.UnderWaterMask);
			this.cam.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), this.cam.farClipPlane, this.cam.stereoActiveEye, this.frustumCorners);
			Vector3 v = this.camTransform.TransformVector(this.frustumCorners[0]);
			Vector3 v2 = this.camTransform.TransformVector(this.frustumCorners[1]);
			Vector3 v3 = this.camTransform.TransformVector(this.frustumCorners[2]);
			Vector3 v4 = this.camTransform.TransformVector(this.frustumCorners[3]);
			this.frustumCornersArray.SetRow(0, v);
			this.frustumCornersArray.SetRow(1, v4);
			this.frustumCornersArray.SetRow(2, v2);
			this.frustumCornersArray.SetRow(3, v3);
			Shader.SetGlobalMatrix(this.Lux_FrustumCornersWSPID, this.frustumCornersArray);
			Shader.SetGlobalVector(this.Lux_CameraWSPID, this.camTransform.position);
			if (this.DoUnderWaterRendering && this.activeWaterVolume > -1)
			{
				Shader.EnableKeyword("LUXWATERENABLED");
				if (this.WaterMaterials[this.activeWaterVolume].GetFloat(this.GerstnerEnabledPID) == 1f)
				{
					this.mat.EnableKeyword("GERSTNERENABLED");
					this.mat.SetVector(this.LuxWaterMask_GerstnerVertexIntensityPID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GerstnerVertexIntensityPID));
					this.mat.SetVector(this.LuxWaterMask_GAmplitudePID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GAmplitudePID));
					this.mat.SetVector(this.LuxWaterMask_GFinalFrequencyPID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GFinalFrequencyPID));
					this.mat.SetVector(this.LuxWaterMask_GSteepnessPID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GSteepnessPID));
					this.mat.SetVector(this.LuxWaterMask_GFinalSpeedPID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GFinalSpeedPID));
					this.mat.SetVector(this.LuxWaterMask_GDirectionABPID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GDirectionABPID));
					this.mat.SetVector(this.LuxWaterMask_GDirectionCDPID, this.WaterMaterials[this.activeWaterVolume].GetVector(this.GDirectionCDPID));
				}
				else
				{
					this.mat.DisableKeyword("GERSTNERENABLED");
				}
				GL.Clear(true, true, Color.black, 1f);
				this.camProj = this.cam.projectionMatrix;
				GL.LoadProjectionMatrix(this.camProj);
				this.mat.SetPass(0);
				Graphics.DrawMeshNow(this.WaterMeshes[this.activeWaterVolume], this.WaterTransforms[this.activeWaterVolume].localToWorldMatrix, 0);
				this.mat.SetPass(1);
				Graphics.DrawMeshNow(this.WaterMeshes[this.activeWaterVolume], this.WaterTransforms[this.activeWaterVolume].localToWorldMatrix, 1);
				this.mat.SetPass(2);
				Graphics.DrawMeshNow(this.WaterMeshes[this.activeWaterVolume], this.WaterTransforms[this.activeWaterVolume].localToWorldMatrix, 1);
				Shader.SetGlobalTexture(this.UnderWaterMaskPID, this.UnderWaterMask);
			}
			else
			{
				Shader.DisableKeyword("LUXWATERENABLED");
			}
			this.ambientProbe = RenderSettings.ambientProbe;
			this.ambientProbe.Evaluate(this.directions, this.AmbientLightingSamples);
			if (this.islinear)
			{
				Shader.SetGlobalColor(this.Lux_UnderWaterAmbientSkyLightPID, (this.AmbientLightingSamples[0] * RenderSettings.ambientIntensity).linear);
				return;
			}
			Shader.SetGlobalColor(this.Lux_UnderWaterAmbientSkyLightPID, this.AmbientLightingSamples[0] * RenderSettings.ambientIntensity);
		}

		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (this.Sun)
			{
				Vector3 vector = -this.Sun.forward;
				Light component = this.Sun.GetComponent<Light>();
				Color a = component.color * component.intensity;
				if (this.islinear)
				{
					a = a.linear;
				}
				Shader.SetGlobalColor(this.Lux_UnderWaterSunColorPID, a * Mathf.Clamp01(Vector3.Dot(vector, Vector3.up)));
				Shader.SetGlobalVector(this.Lux_UnderWaterSunDirPID, -vector);
			}
			if (this.DoUnderWaterRendering && this.activeWaterVolume > -1)
			{
				if (this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticsEnabled") == 1f)
				{
					this.blitMaterial.EnableKeyword("GEOM_TYPE_FROND");
					if (this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticMode") == 1f)
					{
						this.blitMaterial.EnableKeyword("GEOM_TYPE_LEAF");
					}
					else
					{
						this.blitMaterial.DisableKeyword("GEOM_TYPE_LEAF");
					}
				}
				else
				{
					this.blitMaterial.DisableKeyword("GEOM_TYPE_FROND");
				}
				if (this.islinear)
				{
					Shader.SetGlobalColor("_Lux_UnderWaterFogColor", this.WaterMaterials[this.activeWaterVolume].GetColor("_Color").linear);
				}
				else
				{
					Shader.SetGlobalColor("_Lux_UnderWaterFogColor", this.WaterMaterials[this.activeWaterVolume].GetColor("_Color"));
				}
				Shader.SetGlobalFloat("_Lux_UnderWaterFogDensity", this.WaterMaterials[this.activeWaterVolume].GetFloat("_Density"));
				Shader.SetGlobalFloat("_Lux_UnderWaterFogAbsorptionCancellation", this.WaterMaterials[this.activeWaterVolume].GetFloat("_FogAbsorptionCancellation"));
				Shader.SetGlobalFloat("_Lux_UnderWaterAbsorptionHeight", this.WaterMaterials[this.activeWaterVolume].GetFloat("_AbsorptionHeight"));
				Shader.SetGlobalFloat("_Lux_UnderWaterAbsorptionMaxHeight", this.WaterMaterials[this.activeWaterVolume].GetFloat("_AbsorptionMaxHeight"));
				Shader.SetGlobalFloat("_Lux_UnderWaterAbsorptionDepth", this.WaterMaterials[this.activeWaterVolume].GetFloat("_AbsorptionDepth"));
				Shader.SetGlobalFloat("_Lux_UnderWaterAbsorptionColorStrength", this.WaterMaterials[this.activeWaterVolume].GetFloat("_AbsorptionColorStrength"));
				Shader.SetGlobalFloat("_Lux_UnderWaterAbsorptionStrength", this.WaterMaterials[this.activeWaterVolume].GetFloat("_AbsorptionStrength"));
				Shader.SetGlobalTexture("_Lux_UnderWaterCaustics", this.WaterMaterials[this.activeWaterVolume].GetTexture("_CausticTex"));
				Shader.SetGlobalFloat("_Lux_UnderWaterCausticsTiling", this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticsTiling"));
				Shader.SetGlobalFloat("_Lux_UnderWaterCausticsScale", this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticsScale"));
				Shader.SetGlobalFloat("_Lux_UnderWaterCausticsSpeed", this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticsSpeed"));
				Shader.SetGlobalFloat("_Lux_UnderWaterCausticsTiling", this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticsTiling"));
				Shader.SetGlobalFloat("_Lux_UnderWaterCausticsSelfDistortion", this.WaterMaterials[this.activeWaterVolume].GetFloat("_CausticsSelfDistortion"));
				Shader.SetGlobalVector("_Lux_UnderWaterFinalBumpSpeed01", this.WaterMaterials[this.activeWaterVolume].GetVector("_FinalBumpSpeed01"));
				this.blitMaterial.SetFloat("_Lux_UnderWaterWaterSurfacePos", this.WaterSurfacePos);
				this.blitMaterial.SetVector("_Lux_UnderWaterFogDepthAtten", this.WaterMaterials[this.activeWaterVolume].GetVector("_DepthAtten"));
				RenderTexture temporary = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.DefaultHDR);
				Graphics.Blit(src, temporary, this.blitMaterial, 0);
				Shader.SetGlobalTexture("_UnderWaterTex", temporary);
				Graphics.Blit(src, dest, this.blitMaterial, 1);
				RenderTexture.ReleaseTemporary(temporary);
				return;
			}
			Graphics.Blit(src, dest);
		}

		public static LuxWater_UnderWaterRendering instance;

		[Space(8f)]
		public Transform Sun;

		[Space(8f)]
		[NonSerialized]
		public int activeWaterVolume = -1;

		[NonSerialized]
		public float WaterSurfacePos;

		[Space(8f)]
		[NonSerialized]
		public List<LuxWater_WaterVolume> RegisteredWaterVolumes = new List<LuxWater_WaterVolume>();

		private List<Mesh> WaterMeshes = new List<Mesh>();

		private List<Transform> WaterTransforms = new List<Transform>();

		private List<Material> WaterMaterials = new List<Material>();

		private RenderTexture UnderWaterMask;

		[Space(2f)]
		[Header("Managed transparent Materials")]
		[Space(4f)]
		public List<Material> m_aboveWatersurface = new List<Material>();

		public List<Material> m_belowWatersurface = new List<Material>();

		[Space(2f)]
		[Header("Debug")]
		[Space(4f)]
		public bool enableDebug;

		[Space(8f)]
		private Material mat;

		private Material blurMaterial;

		private Material blitMaterial;

		private Camera cam;

		private Transform camTransform;

		private Matrix4x4 frustumCornersArray = Matrix4x4.identity;

		private SphericalHarmonicsL2 ambientProbe;

		private Vector3[] directions = new Vector3[]
		{
			new Vector3(0f, 1f, 0f)
		};

		private Color[] AmbientLightingSamples = new Color[1];

		private CommandBuffer cb_DepthGrab;

		private CommandBuffer cb_AfterFinalPass;

		private bool DoUnderWaterRendering;

		private Matrix4x4 camProj;

		private Vector3[] frustumCorners = new Vector3[4];

		private int UnderWaterMaskPID;

		private int Lux_FrustumCornersWSPID;

		private int Lux_CameraWSPID;

		private int GerstnerEnabledPID;

		private int LuxWaterMask_GerstnerVertexIntensityPID;

		private int GerstnerVertexIntensityPID;

		private int LuxWaterMask_GAmplitudePID;

		private int GAmplitudePID;

		private int LuxWaterMask_GFinalFrequencyPID;

		private int GFinalFrequencyPID;

		private int LuxWaterMask_GSteepnessPID;

		private int GSteepnessPID;

		private int LuxWaterMask_GFinalSpeedPID;

		private int GFinalSpeedPID;

		private int LuxWaterMask_GDirectionABPID;

		private int GDirectionABPID;

		private int LuxWaterMask_GDirectionCDPID;

		private int GDirectionCDPID;

		private int Lux_UnderWaterAmbientSkyLightPID;

		private int Lux_UnderWaterSunColorPID;

		private int Lux_UnderWaterSunDirPID;

		private bool islinear;
	}
}
