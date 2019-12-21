using System;
using UnityEngine;
using UnityEngine.Rendering;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
public class NGSS_ContactShadows : MonoBehaviour
{
	private Camera mCamera
	{
		get
		{
			if (this._mCamera == null)
			{
				this._mCamera = base.GetComponent<Camera>();
				if (this._mCamera == null)
				{
					this._mCamera = Camera.main;
				}
				if (this._mCamera == null)
				{
					Debug.LogError("NGSS Error: No MainCamera found, please provide one.", this);
				}
				else
				{
					this._mCamera.depthTextureMode |= DepthTextureMode.Depth;
				}
			}
			return this._mCamera;
		}
	}

	private Material mMaterial
	{
		get
		{
			if (this._mMaterial == null)
			{
				if (this.contactShadowsShader == null)
				{
					Shader.Find("Hidden/NGSS_ContactShadows");
				}
				this._mMaterial = new Material(this.contactShadowsShader);
				if (this._mMaterial == null)
				{
					Debug.LogWarning("NGSS Warning: can't find NGSS_ContactShadows shader, make sure it's on your project.", this);
					base.enabled = false;
					return null;
				}
			}
			return this._mMaterial;
		}
	}

	private void AddCommandBuffers()
	{
		this.computeShadowsCB = new CommandBuffer
		{
			name = "NGSS ContactShadows: Compute"
		};
		this.blendShadowsCB = new CommandBuffer
		{
			name = "NGSS ContactShadows: Mix"
		};
		bool flag = this.mCamera.actualRenderingPath == RenderingPath.Forward;
		if (this.mCamera)
		{
			CommandBuffer[] commandBuffers = this.mCamera.GetCommandBuffers(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting);
			for (int i = 0; i < commandBuffers.Length; i++)
			{
				if (commandBuffers[i].name == this.computeShadowsCB.name)
				{
					return;
				}
			}
			this.mCamera.AddCommandBuffer(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, this.computeShadowsCB);
		}
		if (this.mainDirectionalLight)
		{
			CommandBuffer[] commandBuffers = this.mainDirectionalLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask);
			for (int i = 0; i < commandBuffers.Length; i++)
			{
				if (commandBuffers[i].name == this.blendShadowsCB.name)
				{
					return;
				}
			}
			this.mainDirectionalLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, this.blendShadowsCB);
		}
	}

	private void RemoveCommandBuffers()
	{
		this._mMaterial = null;
		bool flag = this.mCamera.actualRenderingPath == RenderingPath.Forward;
		if (this.mCamera)
		{
			this.mCamera.RemoveCommandBuffer(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, this.computeShadowsCB);
		}
		if (this.mainDirectionalLight)
		{
			this.mainDirectionalLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, this.blendShadowsCB);
		}
		this.isInitialized = false;
	}

	private void Init()
	{
		if (this.isInitialized || this.mainDirectionalLight == null)
		{
			return;
		}
		if (this.mCamera.renderingPath == RenderingPath.UsePlayerSettings || this.mCamera.renderingPath == RenderingPath.VertexLit)
		{
			Debug.LogWarning("Please set your camera rendering path to either Forward or Deferred and re-enable this component.", this);
			base.enabled = false;
			return;
		}
		this.AddCommandBuffers();
		int nameID = Shader.PropertyToID("NGSS_ContactShadowRT");
		int nameID2 = Shader.PropertyToID("NGSS_DepthSourceRT");
		this.computeShadowsCB.GetTemporaryRT(nameID, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
		this.computeShadowsCB.GetTemporaryRT(nameID2, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
		this.computeShadowsCB.Blit(nameID, nameID2, this.mMaterial, 0);
		this.computeShadowsCB.Blit(nameID2, nameID, this.mMaterial, 1);
		this.computeShadowsCB.Blit(nameID, nameID2, this.mMaterial, 2);
		this.blendShadowsCB.Blit(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CurrentActive, this.mMaterial, 3);
		this.computeShadowsCB.SetGlobalTexture("NGSS_ContactShadowsTexture", nameID2);
		this.isInitialized = true;
	}

	private void OnEnable()
	{
		this.Init();
	}

	private void OnDisable()
	{
		if (this.isInitialized)
		{
			this.RemoveCommandBuffers();
		}
	}

	private void OnApplicationQuit()
	{
		if (this.isInitialized)
		{
			this.RemoveCommandBuffers();
		}
	}

	private void OnPreRender()
	{
		this.Init();
		if (!this.isInitialized || this.mainDirectionalLight == null)
		{
			return;
		}
		this.mMaterial.SetVector("LightDir", this.mCamera.transform.InverseTransformDirection(this.mainDirectionalLight.transform.forward));
		this.mMaterial.SetFloat("ShadowsOpacity", 1f - this.mainDirectionalLight.shadowStrength);
		this.mMaterial.SetFloat("ShadowsSoftness", this.shadowsSoftness);
		this.mMaterial.SetFloat("ShadowsDistance", this.shadowsDistance);
		this.mMaterial.SetFloat("ShadowsFade", this.shadowsFade);
		this.mMaterial.SetFloat("ShadowsBias", this.shadowsBias);
		this.mMaterial.SetFloat("RayWidth", this.rayWidth);
		this.mMaterial.SetInt("RaySamples", this.raySamples);
		if (this.noiseFilter)
		{
			this.mMaterial.EnableKeyword("NGSS_CONTACT_SHADOWS_USE_NOISE");
			return;
		}
		this.mMaterial.DisableKeyword("NGSS_CONTACT_SHADOWS_USE_NOISE");
	}

	public Light mainDirectionalLight;

	public Shader contactShadowsShader;

	public bool noiseFilter;

	[Range(0f, 3f)]
	public float shadowsSoftness = 1f;

	[Range(1f, 4f)]
	public float shadowsDistance = 2f;

	[Range(0.1f, 4f)]
	public float shadowsFade = 1f;

	[Range(0f, 0.02f)]
	public float shadowsBias = 0.0065f;

	[Range(0f, 1f)]
	public float rayWidth = 0.1f;

	[Range(16f, 128f)]
	public int raySamples = 64;

	private CommandBuffer blendShadowsCB;

	private CommandBuffer computeShadowsCB;

	private bool isInitialized;

	private Camera _mCamera;

	private Material _mMaterial;
}
