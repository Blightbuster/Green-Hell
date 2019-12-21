using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/HBAO")]
[RequireComponent(typeof(Camera))]
public class HBAO : MonoBehaviour
{
	public HBAO.Presets presets
	{
		get
		{
			return this.m_Presets;
		}
		set
		{
			this.m_Presets = value;
		}
	}

	public HBAO.GeneralSettings generalSettings
	{
		get
		{
			return this.m_GeneralSettings;
		}
		set
		{
			this.m_GeneralSettings = value;
		}
	}

	public HBAO.AOSettings aoSettings
	{
		get
		{
			return this.m_AOSettings;
		}
		set
		{
			this.m_AOSettings = value;
		}
	}

	public HBAO.ColorBleedingSettings colorBleedingSettings
	{
		get
		{
			return this.m_ColorBleedingSettings;
		}
		set
		{
			this.m_ColorBleedingSettings = value;
		}
	}

	public HBAO.BlurSettings blurSettings
	{
		get
		{
			return this.m_BlurSettings;
		}
		set
		{
			this.m_BlurSettings = value;
		}
	}

	private void OnEnable()
	{
		if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			Debug.LogWarning("HBAO shader is not supported on this platform.");
			base.enabled = false;
			return;
		}
		if (this.hbaoShader != null && !this.hbaoShader.isSupported)
		{
			Debug.LogWarning("HBAO shader is not supported on this platform.");
			base.enabled = false;
			return;
		}
		if (this.hbaoShader == null)
		{
			return;
		}
		this.CreateMaterial();
		this._hbaoCamera.depthTextureMode |= DepthTextureMode.Depth;
		if (this.aoSettings.perPixelNormals == HBAO.PerPixelNormals.Camera)
		{
			this._hbaoCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
	}

	private void OnDisable()
	{
		if (this._hbaoMaterial != null)
		{
			UnityEngine.Object.DestroyImmediate(this._hbaoMaterial);
		}
		if (this.noiseTex != null)
		{
			UnityEngine.Object.DestroyImmediate(this.noiseTex);
		}
		if (this.quadMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.quadMesh);
		}
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (this.hbaoShader == null || this._hbaoCamera == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		this._hbaoCamera.depthTextureMode |= DepthTextureMode.Depth;
		if (this.aoSettings.perPixelNormals == HBAO.PerPixelNormals.Camera)
		{
			this._hbaoCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		this.CheckParameters();
		this.UpdateShaderProperties();
		this.UpdateShaderKeywords();
		if (this.generalSettings.deinterleaving == HBAO.Deinterleaving._2x)
		{
			this.RenderHBAODeinterleaved2x(source, destination);
			return;
		}
		if (this.generalSettings.deinterleaving == HBAO.Deinterleaving._4x)
		{
			this.RenderHBAODeinterleaved4x(source, destination);
			return;
		}
		this.RenderHBAO(source, destination);
	}

	private void RenderHBAO(RenderTexture source, RenderTexture destination)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(this._renderTarget.fullWidth / this._renderTarget.downsamplingFactor, this._renderTarget.fullHeight / this._renderTarget.downsamplingFactor);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary;
		GL.Clear(false, true, Color.white);
		RenderTexture.active = active;
		Graphics.Blit(source, temporary, this._hbaoMaterial, this.GetAoPass());
		if (this.blurSettings.amount != HBAO.Blur.None)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(this._renderTarget.fullWidth / this._renderTarget.downsamplingFactor / this._renderTarget.blurDownsamplingFactor, this._renderTarget.fullHeight / this._renderTarget.downsamplingFactor / this._renderTarget.blurDownsamplingFactor);
			Graphics.Blit(temporary, temporary2, this._hbaoMaterial, this.GetBlurXPass());
			temporary.DiscardContents();
			Graphics.Blit(temporary2, temporary, this._hbaoMaterial, this.GetBlurYPass());
			RenderTexture.ReleaseTemporary(temporary2);
		}
		this._hbaoMaterial.SetTexture("_HBAOTex", temporary);
		Graphics.Blit(source, destination, this._hbaoMaterial, this.GetFinalPass());
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void RenderHBAODeinterleaved2x(RenderTexture source, RenderTexture destination)
	{
		RenderTexture active = RenderTexture.active;
		for (int i = 0; i < 4; i++)
		{
			this._mrtTexDepth[i] = RenderTexture.GetTemporary(this._renderTarget.layerWidth, this._renderTarget.layerHeight, 0, RenderTextureFormat.RFloat);
			this._mrtTexNrm[i] = RenderTexture.GetTemporary(this._renderTarget.layerWidth, this._renderTarget.layerHeight, 0, RenderTextureFormat.ARGB2101010);
			this._mrtTexAO[i] = RenderTexture.GetTemporary(this._renderTarget.layerWidth, this._renderTarget.layerHeight);
			this._mrtRB[0][i] = this._mrtTexDepth[i].colorBuffer;
			this._mrtRBNrm[0][i] = this._mrtTexNrm[i].colorBuffer;
			RenderTexture.active = this._mrtTexAO[i];
			GL.Clear(false, true, Color.white);
		}
		this._hbaoMaterial.SetVector("_Deinterleaving_Offset00", new Vector2(0f, 0f));
		this._hbaoMaterial.SetVector("_Deinterleaving_Offset10", new Vector2(1f, 0f));
		this._hbaoMaterial.SetVector("_Deinterleaving_Offset01", new Vector2(0f, 1f));
		this._hbaoMaterial.SetVector("_Deinterleaving_Offset11", new Vector2(1f, 1f));
		Graphics.SetRenderTarget(this._mrtRB[0], this._mrtTexDepth[0].depthBuffer);
		this._hbaoMaterial.SetPass(10);
		Graphics.DrawMeshNow(this.quadMesh, Matrix4x4.identity);
		Graphics.SetRenderTarget(this._mrtRBNrm[0], this._mrtTexNrm[0].depthBuffer);
		this._hbaoMaterial.SetPass(12);
		Graphics.DrawMeshNow(this.quadMesh, Matrix4x4.identity);
		RenderTexture.active = active;
		for (int j = 0; j < 4; j++)
		{
			this._hbaoMaterial.SetTexture("_DepthTex", this._mrtTexDepth[j]);
			this._hbaoMaterial.SetTexture("_NormalsTex", this._mrtTexNrm[j]);
			this._hbaoMaterial.SetVector("_Jitter", this._jitter[j]);
			Graphics.Blit(source, this._mrtTexAO[j], this._hbaoMaterial, this.GetAoDeinterleavedPass());
			this._mrtTexDepth[j].DiscardContents();
			this._mrtTexNrm[j].DiscardContents();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(this._renderTarget.fullWidth, this._renderTarget.fullHeight);
		for (int k = 0; k < 4; k++)
		{
			this._hbaoMaterial.SetVector("_LayerOffset", new Vector2((float)((k & 1) * this._renderTarget.layerWidth), (float)((k >> 1) * this._renderTarget.layerHeight)));
			Graphics.Blit(this._mrtTexAO[k], temporary, this._hbaoMaterial, 14);
			RenderTexture.ReleaseTemporary(this._mrtTexAO[k]);
			RenderTexture.ReleaseTemporary(this._mrtTexNrm[k]);
			RenderTexture.ReleaseTemporary(this._mrtTexDepth[k]);
		}
		RenderTexture temporary2 = RenderTexture.GetTemporary(this._renderTarget.fullWidth, this._renderTarget.fullHeight);
		Graphics.Blit(temporary, temporary2, this._hbaoMaterial, 15);
		temporary.DiscardContents();
		if (this.blurSettings.amount != HBAO.Blur.None)
		{
			if (this.blurSettings.downsample)
			{
				RenderTexture temporary3 = RenderTexture.GetTemporary(this._renderTarget.fullWidth / this._renderTarget.blurDownsamplingFactor, this._renderTarget.fullHeight / this._renderTarget.blurDownsamplingFactor);
				Graphics.Blit(temporary2, temporary3, this._hbaoMaterial, this.GetBlurXPass());
				temporary2.DiscardContents();
				Graphics.Blit(temporary3, temporary2, this._hbaoMaterial, this.GetBlurYPass());
				RenderTexture.ReleaseTemporary(temporary3);
			}
			else
			{
				Graphics.Blit(temporary2, temporary, this._hbaoMaterial, this.GetBlurXPass());
				temporary2.DiscardContents();
				Graphics.Blit(temporary, temporary2, this._hbaoMaterial, this.GetBlurYPass());
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
		this._hbaoMaterial.SetTexture("_HBAOTex", temporary2);
		Graphics.Blit(source, destination, this._hbaoMaterial, this.GetFinalPass());
		RenderTexture.ReleaseTemporary(temporary2);
	}

	private void RenderHBAODeinterleaved4x(RenderTexture source, RenderTexture destination)
	{
		RenderTexture active = RenderTexture.active;
		for (int i = 0; i < 16; i++)
		{
			this._mrtTexDepth[i] = RenderTexture.GetTemporary(this._renderTarget.layerWidth, this._renderTarget.layerHeight, 0, RenderTextureFormat.RFloat);
			this._mrtTexNrm[i] = RenderTexture.GetTemporary(this._renderTarget.layerWidth, this._renderTarget.layerHeight, 0, RenderTextureFormat.ARGB2101010);
			this._mrtTexAO[i] = RenderTexture.GetTemporary(this._renderTarget.layerWidth, this._renderTarget.layerHeight);
			RenderTexture.active = this._mrtTexAO[i];
			GL.Clear(false, true, Color.white);
		}
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				this._mrtRB[j][k] = this._mrtTexDepth[k + 4 * j].colorBuffer;
				this._mrtRBNrm[j][k] = this._mrtTexNrm[k + 4 * j].colorBuffer;
			}
		}
		for (int l = 0; l < 4; l++)
		{
			int num = (l & 1) << 1;
			int num2 = l >> 1 << 1;
			this._hbaoMaterial.SetVector("_Deinterleaving_Offset00", new Vector2((float)num, (float)num2));
			this._hbaoMaterial.SetVector("_Deinterleaving_Offset10", new Vector2((float)(num + 1), (float)num2));
			this._hbaoMaterial.SetVector("_Deinterleaving_Offset01", new Vector2((float)num, (float)(num2 + 1)));
			this._hbaoMaterial.SetVector("_Deinterleaving_Offset11", new Vector2((float)(num + 1), (float)(num2 + 1)));
			Graphics.SetRenderTarget(this._mrtRB[l], this._mrtTexDepth[4 * l].depthBuffer);
			this._hbaoMaterial.SetPass(11);
			Graphics.DrawMeshNow(this.quadMesh, Matrix4x4.identity);
			Graphics.SetRenderTarget(this._mrtRBNrm[l], this._mrtTexNrm[4 * l].depthBuffer);
			this._hbaoMaterial.SetPass(13);
			Graphics.DrawMeshNow(this.quadMesh, Matrix4x4.identity);
		}
		RenderTexture.active = active;
		for (int m = 0; m < 16; m++)
		{
			this._hbaoMaterial.SetTexture("_DepthTex", this._mrtTexDepth[m]);
			this._hbaoMaterial.SetTexture("_NormalsTex", this._mrtTexNrm[m]);
			this._hbaoMaterial.SetVector("_Jitter", this._jitter[m]);
			Graphics.Blit(source, this._mrtTexAO[m], this._hbaoMaterial, this.GetAoDeinterleavedPass());
			this._mrtTexDepth[m].DiscardContents();
			this._mrtTexNrm[m].DiscardContents();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(this._renderTarget.fullWidth, this._renderTarget.fullHeight);
		for (int n = 0; n < 16; n++)
		{
			this._hbaoMaterial.SetVector("_LayerOffset", new Vector2((float)(((n & 1) + ((n & 7) >> 2 << 1)) * this._renderTarget.layerWidth), (float)((((n & 3) >> 1) + (n >> 3 << 1)) * this._renderTarget.layerHeight)));
			Graphics.Blit(this._mrtTexAO[n], temporary, this._hbaoMaterial, 14);
			RenderTexture.ReleaseTemporary(this._mrtTexAO[n]);
			RenderTexture.ReleaseTemporary(this._mrtTexNrm[n]);
			RenderTexture.ReleaseTemporary(this._mrtTexDepth[n]);
		}
		RenderTexture temporary2 = RenderTexture.GetTemporary(this._renderTarget.fullWidth, this._renderTarget.fullHeight);
		Graphics.Blit(temporary, temporary2, this._hbaoMaterial, 16);
		temporary.DiscardContents();
		if (this.blurSettings.amount != HBAO.Blur.None)
		{
			if (this.blurSettings.downsample)
			{
				RenderTexture temporary3 = RenderTexture.GetTemporary(this._renderTarget.fullWidth / this._renderTarget.blurDownsamplingFactor, this._renderTarget.fullHeight / this._renderTarget.blurDownsamplingFactor);
				Graphics.Blit(temporary2, temporary3, this._hbaoMaterial, this.GetBlurXPass());
				temporary2.DiscardContents();
				Graphics.Blit(temporary3, temporary2, this._hbaoMaterial, this.GetBlurYPass());
				RenderTexture.ReleaseTemporary(temporary3);
			}
			else
			{
				Graphics.Blit(temporary2, temporary, this._hbaoMaterial, this.GetBlurXPass());
				temporary2.DiscardContents();
				Graphics.Blit(temporary, temporary2, this._hbaoMaterial, this.GetBlurYPass());
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
		this._hbaoMaterial.SetTexture("_HBAOTex", temporary2);
		Graphics.Blit(source, destination, this._hbaoMaterial, this.GetFinalPass());
		RenderTexture.ReleaseTemporary(temporary2);
	}

	private void CreateMaterial()
	{
		if (this._hbaoMaterial == null)
		{
			this._hbaoMaterial = new Material(this.hbaoShader);
			this._hbaoMaterial.hideFlags = HideFlags.HideAndDontSave;
			this._hbaoCamera = base.GetComponent<Camera>();
		}
		if (this.quadMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(this.quadMesh);
		}
		this.quadMesh = new Mesh();
		this.quadMesh.vertices = new Vector3[]
		{
			new Vector3(-0.5f, -0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f),
			new Vector3(0.5f, -0.5f, 0f),
			new Vector3(-0.5f, 0.5f, 0f)
		};
		this.quadMesh.uv = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f)
		};
		this.quadMesh.triangles = new int[]
		{
			0,
			1,
			2,
			1,
			0,
			3
		};
		this._renderTarget = new HBAO.RenderTarget();
	}

	private void UpdateShaderProperties()
	{
		this._renderTarget.orthographic = this._hbaoCamera.orthographic;
		this._renderTarget.hdr = this._hbaoCamera.allowHDR;
		this._renderTarget.width = this._hbaoCamera.pixelWidth;
		this._renderTarget.height = this._hbaoCamera.pixelHeight;
		this._renderTarget.downsamplingFactor = ((this.generalSettings.resolution == HBAO.Resolution.Full) ? 1 : ((this.generalSettings.resolution == HBAO.Resolution.Half) ? 2 : 4));
		this._renderTarget.deinterleavingFactor = this.GetDeinterleavingFactor();
		this._renderTarget.blurDownsamplingFactor = (this.blurSettings.downsample ? 2 : 1);
		float num = Mathf.Tan(0.5f * this._hbaoCamera.fieldOfView * 0.0174532924f);
		float num2 = 1f / (1f / num * ((float)this._renderTarget.height / (float)this._renderTarget.width));
		float num3 = 1f / (1f / num);
		this._hbaoMaterial.SetVector("_UVToView", new Vector4(2f * num2, -2f * num3, -1f * num2, 1f * num3));
		this._hbaoMaterial.SetMatrix("_WorldToCameraMatrix", this._hbaoCamera.worldToCameraMatrix);
		if (this.generalSettings.deinterleaving != HBAO.Deinterleaving.Disabled)
		{
			this._renderTarget.fullWidth = this._renderTarget.width + ((this._renderTarget.width % this._renderTarget.deinterleavingFactor == 0) ? 0 : (this._renderTarget.deinterleavingFactor - this._renderTarget.width % this._renderTarget.deinterleavingFactor));
			this._renderTarget.fullHeight = this._renderTarget.height + ((this._renderTarget.height % this._renderTarget.deinterleavingFactor == 0) ? 0 : (this._renderTarget.deinterleavingFactor - this._renderTarget.height % this._renderTarget.deinterleavingFactor));
			this._renderTarget.layerWidth = this._renderTarget.fullWidth / this._renderTarget.deinterleavingFactor;
			this._renderTarget.layerHeight = this._renderTarget.fullHeight / this._renderTarget.deinterleavingFactor;
			this._hbaoMaterial.SetVector("_FullRes_TexelSize", new Vector4(1f / (float)this._renderTarget.fullWidth, 1f / (float)this._renderTarget.fullHeight, (float)this._renderTarget.fullWidth, (float)this._renderTarget.fullHeight));
			this._hbaoMaterial.SetVector("_LayerRes_TexelSize", new Vector4(1f / (float)this._renderTarget.layerWidth, 1f / (float)this._renderTarget.layerHeight, (float)this._renderTarget.layerWidth, (float)this._renderTarget.layerHeight));
			this._hbaoMaterial.SetVector("_TargetScale", new Vector4((float)this._renderTarget.fullWidth / (float)this._renderTarget.width, (float)this._renderTarget.fullHeight / (float)this._renderTarget.height, 1f / ((float)this._renderTarget.fullWidth / (float)this._renderTarget.width), 1f / ((float)this._renderTarget.fullHeight / (float)this._renderTarget.height)));
		}
		else
		{
			this._renderTarget.fullWidth = this._renderTarget.width;
			this._renderTarget.fullHeight = this._renderTarget.height;
			if (this.generalSettings.resolution == HBAO.Resolution.Half && this.aoSettings.perPixelNormals == HBAO.PerPixelNormals.Reconstruct)
			{
				this._hbaoMaterial.SetVector("_TargetScale", new Vector4(((float)this._renderTarget.width + 0.5f) / (float)this._renderTarget.width, ((float)this._renderTarget.height + 0.5f) / (float)this._renderTarget.height, 1f, 1f));
			}
			else
			{
				this._hbaoMaterial.SetVector("_TargetScale", new Vector4(1f, 1f, 1f, 1f));
			}
		}
		if (this.noiseTex == null || this._quality != this.generalSettings.quality || this._noiseType != this.generalSettings.noiseType)
		{
			if (this.noiseTex != null)
			{
				UnityEngine.Object.DestroyImmediate(this.noiseTex);
			}
			float num4 = (float)((this.generalSettings.noiseType == HBAO.NoiseType.Dither) ? 4 : 64);
			this.CreateRandomTexture((int)num4);
		}
		this._quality = this.generalSettings.quality;
		this._noiseType = this.generalSettings.noiseType;
		this._hbaoMaterial.SetTexture("_NoiseTex", this.noiseTex);
		this._hbaoMaterial.SetFloat("_NoiseTexSize", (float)((this._noiseType == HBAO.NoiseType.Dither) ? 4 : 64));
		this._hbaoMaterial.SetFloat("_Radius", this.aoSettings.radius * 0.5f * ((float)this._renderTarget.height / (num * 2f)) / (float)this._renderTarget.deinterleavingFactor);
		this._hbaoMaterial.SetFloat("_MaxRadiusPixels", this.aoSettings.maxRadiusPixels / (float)this._renderTarget.deinterleavingFactor);
		this._hbaoMaterial.SetFloat("_NegInvRadius2", -1f / (this.aoSettings.radius * this.aoSettings.radius));
		this._hbaoMaterial.SetFloat("_AngleBias", this.aoSettings.bias);
		this._hbaoMaterial.SetFloat("_AOmultiplier", 2f * (1f / (1f - this.aoSettings.bias)));
		this._hbaoMaterial.SetFloat("_Intensity", this.aoSettings.intensity);
		this._hbaoMaterial.SetFloat("_LuminanceInfluence", this.aoSettings.luminanceInfluence);
		this._hbaoMaterial.SetFloat("_MaxDistance", this.aoSettings.maxDistance);
		this._hbaoMaterial.SetFloat("_DistanceFalloff", this.aoSettings.distanceFalloff);
		this._hbaoMaterial.SetColor("_BaseColor", this.aoSettings.baseColor);
		this._hbaoMaterial.SetFloat("_ColorBleedSaturation", this.colorBleedingSettings.saturation);
		this._hbaoMaterial.SetFloat("_AlbedoMultiplier", this.colorBleedingSettings.albedoMultiplier);
		this._hbaoMaterial.SetFloat("_BlurSharpness", this.blurSettings.sharpness);
	}

	private void UpdateShaderKeywords()
	{
		this._hbaoShaderKeywords[0] = (this.colorBleedingSettings.enabled ? "COLOR_BLEEDING_ON" : "__");
		if (this._renderTarget.orthographic)
		{
			this._hbaoShaderKeywords[1] = "ORTHOGRAPHIC_PROJECTION_ON";
		}
		else
		{
			this._hbaoShaderKeywords[1] = (this.IsDeferredShading() ? "DEFERRED_SHADING_ON" : "__");
		}
		this._hbaoShaderKeywords[2] = ((this.aoSettings.perPixelNormals == HBAO.PerPixelNormals.Camera) ? "NORMALS_CAMERA" : ((this.aoSettings.perPixelNormals == HBAO.PerPixelNormals.Reconstruct) ? "NORMALS_RECONSTRUCT" : "__"));
		this._hbaoMaterial.shaderKeywords = this._hbaoShaderKeywords;
	}

	private void CheckParameters()
	{
		if (!this.IsDeferredShading() && this.aoSettings.perPixelNormals == HBAO.PerPixelNormals.GBuffer)
		{
			this.m_AOSettings.perPixelNormals = HBAO.PerPixelNormals.Camera;
		}
		if (this.generalSettings.deinterleaving != HBAO.Deinterleaving.Disabled && SystemInfo.supportedRenderTargetCount < 4)
		{
			this.m_GeneralSettings.deinterleaving = HBAO.Deinterleaving.Disabled;
		}
	}

	private bool IsDeferredShading()
	{
		return this._hbaoCamera.actualRenderingPath == RenderingPath.DeferredShading;
	}

	private int GetDeinterleavingFactor()
	{
		switch (this.generalSettings.deinterleaving)
		{
		case HBAO.Deinterleaving._2x:
			return 2;
		case HBAO.Deinterleaving._4x:
			return 4;
		}
		return 1;
	}

	private int GetAoPass()
	{
		switch (this.generalSettings.quality)
		{
		case HBAO.Quality.Lowest:
			return 0;
		case HBAO.Quality.Low:
			return 1;
		case HBAO.Quality.Medium:
			return 2;
		case HBAO.Quality.High:
			return 3;
		case HBAO.Quality.Highest:
			return 4;
		default:
			return 2;
		}
	}

	private int GetAoDeinterleavedPass()
	{
		switch (this.generalSettings.quality)
		{
		case HBAO.Quality.Lowest:
			return 5;
		case HBAO.Quality.Low:
			return 6;
		case HBAO.Quality.Medium:
			return 7;
		case HBAO.Quality.High:
			return 8;
		case HBAO.Quality.Highest:
			return 9;
		default:
			return 7;
		}
	}

	private int GetBlurXPass()
	{
		switch (this.blurSettings.amount)
		{
		case HBAO.Blur.Narrow:
			return 17;
		case HBAO.Blur.Medium:
			return 18;
		case HBAO.Blur.Wide:
			return 19;
		case HBAO.Blur.ExtraWide:
			return 20;
		default:
			return 18;
		}
	}

	private int GetBlurYPass()
	{
		switch (this.blurSettings.amount)
		{
		case HBAO.Blur.Narrow:
			return 21;
		case HBAO.Blur.Medium:
			return 22;
		case HBAO.Blur.Wide:
			return 23;
		case HBAO.Blur.ExtraWide:
			return 24;
		default:
			return 22;
		}
	}

	private int GetFinalPass()
	{
		switch (this.generalSettings.displayMode)
		{
		case HBAO.DisplayMode.Normal:
			return 25;
		case HBAO.DisplayMode.AOOnly:
			return 26;
		case HBAO.DisplayMode.ColorBleedingOnly:
			return 27;
		case HBAO.DisplayMode.SplitWithoutAOAndWithAO:
			return 28;
		case HBAO.DisplayMode.SplitWithAOAndAOOnly:
			return 29;
		case HBAO.DisplayMode.SplitWithoutAOAndAOOnly:
			return 30;
		default:
			return 25;
		}
	}

	private void CreateRandomTexture(int size)
	{
		this.noiseTex = new Texture2D(size, size, TextureFormat.RGB24, false, true);
		this.noiseTex.filterMode = FilterMode.Point;
		this.noiseTex.wrapMode = TextureWrapMode.Repeat;
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num2 = (this.generalSettings.noiseType == HBAO.NoiseType.Dither) ? HBAO.MersenneTwister.Numbers[num++] : UnityEngine.Random.Range(0f, 1f);
				float b = (this.generalSettings.noiseType == HBAO.NoiseType.Dither) ? HBAO.MersenneTwister.Numbers[num++] : UnityEngine.Random.Range(0f, 1f);
				float f = 6.28318548f * num2 / (float)this._numSampleDirections[this.GetAoPass()];
				Color color = new Color(Mathf.Cos(f), Mathf.Sin(f), b);
				this.noiseTex.SetPixel(i, j, color);
			}
		}
		this.noiseTex.Apply();
		int k = 0;
		int num3 = 0;
		while (k < this._jitter.Length)
		{
			float num4 = HBAO.MersenneTwister.Numbers[num3++];
			float z = HBAO.MersenneTwister.Numbers[num3++];
			float f2 = 6.28318548f * num4 / (float)this._numSampleDirections[this.GetAoPass()];
			this._jitter[k] = new Vector4(Mathf.Cos(f2), Mathf.Sin(f2), z, 0f);
			k++;
		}
	}

	public void ApplyPreset(HBAO.Preset preset)
	{
		if (preset == HBAO.Preset.Custom)
		{
			this.m_Presets.preset = preset;
			return;
		}
		HBAO.DisplayMode displayMode = this.generalSettings.displayMode;
		this.m_GeneralSettings = HBAO.GeneralSettings.defaultSettings;
		this.m_AOSettings = HBAO.AOSettings.defaultSettings;
		this.m_ColorBleedingSettings = HBAO.ColorBleedingSettings.defaultSettings;
		this.m_BlurSettings = HBAO.BlurSettings.defaultSettings;
		this.m_GeneralSettings.displayMode = displayMode;
		switch (preset)
		{
		case HBAO.Preset.FastestPerformance:
			this.m_GeneralSettings.quality = HBAO.Quality.Lowest;
			this.m_AOSettings.radius = 0.5f;
			this.m_AOSettings.maxRadiusPixels = 64f;
			this.m_BlurSettings.amount = HBAO.Blur.ExtraWide;
			this.m_BlurSettings.downsample = true;
			break;
		case HBAO.Preset.FastPerformance:
			this.m_GeneralSettings.quality = HBAO.Quality.Low;
			this.m_AOSettings.radius = 0.5f;
			this.m_AOSettings.maxRadiusPixels = 64f;
			this.m_BlurSettings.amount = HBAO.Blur.Wide;
			this.m_BlurSettings.downsample = true;
			break;
		case HBAO.Preset.HighQuality:
			this.m_GeneralSettings.quality = HBAO.Quality.High;
			this.m_AOSettings.radius = 1f;
			break;
		case HBAO.Preset.HighestQuality:
			this.m_GeneralSettings.quality = HBAO.Quality.Highest;
			this.m_AOSettings.radius = 1.2f;
			this.m_AOSettings.maxRadiusPixels = 256f;
			this.m_BlurSettings.amount = HBAO.Blur.Narrow;
			break;
		}
		this.m_Presets.preset = preset;
	}

	public Texture2D noiseTex;

	public Mesh quadMesh;

	public Shader hbaoShader;

	[SerializeField]
	[HBAO.SettingsGroup]
	private HBAO.Presets m_Presets = HBAO.Presets.defaultPresets;

	[SerializeField]
	[HBAO.SettingsGroup]
	private HBAO.GeneralSettings m_GeneralSettings = HBAO.GeneralSettings.defaultSettings;

	[SerializeField]
	[HBAO.SettingsGroup]
	private HBAO.AOSettings m_AOSettings = HBAO.AOSettings.defaultSettings;

	[SerializeField]
	[HBAO.SettingsGroup]
	private HBAO.ColorBleedingSettings m_ColorBleedingSettings = HBAO.ColorBleedingSettings.defaultSettings;

	[SerializeField]
	[HBAO.SettingsGroup]
	private HBAO.BlurSettings m_BlurSettings = HBAO.BlurSettings.defaultSettings;

	private HBAO.Quality _quality;

	private HBAO.NoiseType _noiseType;

	private HBAO.RenderTarget _renderTarget;

	private const int NUM_MRTS = 4;

	private RenderTexture[] _mrtTexDepth = new RenderTexture[16];

	private RenderTexture[] _mrtTexNrm = new RenderTexture[16];

	private RenderTexture[] _mrtTexAO = new RenderTexture[16];

	private RenderBuffer[][] _mrtRB = new RenderBuffer[][]
	{
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4]
	};

	private RenderBuffer[][] _mrtRBNrm = new RenderBuffer[][]
	{
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4]
	};

	private Vector4[] _jitter = new Vector4[16];

	private string[] _hbaoShaderKeywords = new string[3];

	private Material _hbaoMaterial;

	private Camera _hbaoCamera;

	private int[] _numSampleDirections = new int[]
	{
		3,
		4,
		6,
		8,
		8
	};

	public enum Preset
	{
		FastestPerformance,
		FastPerformance,
		Normal,
		HighQuality,
		HighestQuality,
		Custom
	}

	public enum Quality
	{
		Lowest,
		Low,
		Medium,
		High,
		Highest
	}

	public enum Resolution
	{
		Full,
		Half,
		Quarter
	}

	public enum Deinterleaving
	{
		Disabled,
		_2x,
		_4x
	}

	public enum DisplayMode
	{
		Normal,
		AOOnly,
		ColorBleedingOnly,
		SplitWithoutAOAndWithAO,
		SplitWithAOAndAOOnly,
		SplitWithoutAOAndAOOnly
	}

	public enum Blur
	{
		None,
		Narrow,
		Medium,
		Wide,
		ExtraWide
	}

	public enum NoiseType
	{
		Random,
		Dither
	}

	public enum PerPixelNormals
	{
		GBuffer,
		Camera,
		Reconstruct
	}

	[Serializable]
	public struct Presets
	{
		[SerializeField]
		public static HBAO.Presets defaultPresets
		{
			get
			{
				return new HBAO.Presets
				{
					preset = HBAO.Preset.Normal
				};
			}
		}

		public HBAO.Preset preset;
	}

	[Serializable]
	public struct GeneralSettings
	{
		[SerializeField]
		public static HBAO.GeneralSettings defaultSettings
		{
			get
			{
				return new HBAO.GeneralSettings
				{
					quality = HBAO.Quality.Medium,
					deinterleaving = HBAO.Deinterleaving.Disabled,
					resolution = HBAO.Resolution.Full,
					noiseType = HBAO.NoiseType.Dither,
					displayMode = HBAO.DisplayMode.Normal
				};
			}
		}

		[Tooltip("The quality of the AO.")]
		[Space(6f)]
		public HBAO.Quality quality;

		[Tooltip("The deinterleaving factor.")]
		public HBAO.Deinterleaving deinterleaving;

		[Tooltip("The resolution at which the AO is calculated.")]
		public HBAO.Resolution resolution;

		[Tooltip("The type of noise to use.")]
		[Space(10f)]
		public HBAO.NoiseType noiseType;

		[Tooltip("The way the AO is displayed on screen.")]
		[Space(10f)]
		public HBAO.DisplayMode displayMode;
	}

	[Serializable]
	public struct AOSettings
	{
		[SerializeField]
		public static HBAO.AOSettings defaultSettings
		{
			get
			{
				return new HBAO.AOSettings
				{
					radius = 0.8f,
					maxRadiusPixels = 128f,
					bias = 0.05f,
					intensity = 1f,
					luminanceInfluence = 0f,
					maxDistance = 150f,
					distanceFalloff = 50f,
					perPixelNormals = HBAO.PerPixelNormals.GBuffer,
					baseColor = Color.black
				};
			}
		}

		[Tooltip("AO radius: this is the distance outside which occluders are ignored.")]
		[Space(6f)]
		[Range(0f, 2f)]
		public float radius;

		[Tooltip("Maximum radius in pixels: this prevents the radius to grow too much with close-up object and impact on performances.")]
		[Range(32f, 256f)]
		public float maxRadiusPixels;

		[Tooltip("For low-tessellated geometry, occlusion variations tend to appear at creases and ridges, which betray the underlying tessellation. To remove these artifacts, we use an angle bias parameter which restricts the hemisphere.")]
		[Range(0f, 0.5f)]
		public float bias;

		[Tooltip("This value allows to scale up the ambient occlusion values.")]
		[Range(0f, 10f)]
		public float intensity;

		[Tooltip("This value allows to attenuate ambient occlusion depending on final color luminance.")]
		[Range(0f, 1f)]
		public float luminanceInfluence;

		[Tooltip("The max distance to display AO.")]
		public float maxDistance;

		[Tooltip("The distance before max distance at which AO start to decrease.")]
		public float distanceFalloff;

		[Tooltip("The type of per pixel normals to use.")]
		[Space(10f)]
		public HBAO.PerPixelNormals perPixelNormals;

		[Tooltip("This setting allow you to set the base color if the AO, the alpha channel value is unused.")]
		[Space(10f)]
		public Color baseColor;
	}

	[Serializable]
	public struct ColorBleedingSettings
	{
		[SerializeField]
		public static HBAO.ColorBleedingSettings defaultSettings
		{
			get
			{
				return new HBAO.ColorBleedingSettings
				{
					enabled = false,
					saturation = 1f,
					albedoMultiplier = 4f
				};
			}
		}

		[Space(6f)]
		public bool enabled;

		[Tooltip("This value allows to control the saturation of the color bleeding.")]
		[Space(10f)]
		[Range(0f, 4f)]
		public float saturation;

		[Tooltip("This value allows to scale the contribution of the color bleeding samples.")]
		[Range(0f, 32f)]
		public float albedoMultiplier;
	}

	[Serializable]
	public struct BlurSettings
	{
		[SerializeField]
		public static HBAO.BlurSettings defaultSettings
		{
			get
			{
				return new HBAO.BlurSettings
				{
					amount = HBAO.Blur.Medium,
					sharpness = 8f,
					downsample = false
				};
			}
		}

		[Tooltip("The type of blur to use.")]
		[Space(6f)]
		public HBAO.Blur amount;

		[Tooltip("This parameter controls the depth-dependent weight of the bilateral filter, to avoid bleeding across edges. A zero sharpness is a pure Gaussian blur. Increasing the blur sharpness removes bleeding by using lower weights for samples with large depth delta from the current pixel.")]
		[Space(10f)]
		[Range(0f, 16f)]
		public float sharpness;

		[Tooltip("Is the blur downsampled.")]
		public bool downsample;
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class SettingsGroup : Attribute
	{
	}

	private static class MersenneTwister
	{
		public static float[] Numbers = new float[]
		{
			0.463937f,
			0.340042f,
			0.223035f,
			0.468465f,
			0.322224f,
			0.979269f,
			0.031798f,
			0.973392f,
			0.778313f,
			0.456168f,
			0.258593f,
			0.330083f,
			0.387332f,
			0.380117f,
			0.179842f,
			0.910755f,
			0.511623f,
			0.092933f,
			0.180794f,
			0.620153f,
			0.101348f,
			0.556342f,
			0.642479f,
			0.442008f,
			0.215115f,
			0.475218f,
			0.157357f,
			0.568868f,
			0.501241f,
			0.629229f,
			0.699218f,
			0.707733f
		};
	}

	private static class Pass
	{
		public const int AO_LowestQuality = 0;

		public const int AO_LowQuality = 1;

		public const int AO_MediumQuality = 2;

		public const int AO_HighQuality = 3;

		public const int AO_HighestQuality = 4;

		public const int AO_Deinterleaved_LowestQuality = 5;

		public const int AO_Deinterleaved_LowQuality = 6;

		public const int AO_Deinterleaved_MediumQuality = 7;

		public const int AO_Deinterleaved_HighQuality = 8;

		public const int AO_Deinterleaved_HighestQuality = 9;

		public const int Depth_Deinterleaving_2x2 = 10;

		public const int Depth_Deinterleaving_4x4 = 11;

		public const int Normals_Deinterleaving_2x2 = 12;

		public const int Normals_Deinterleaving_4x4 = 13;

		public const int Atlas = 14;

		public const int Reinterleaving_2x2 = 15;

		public const int Reinterleaving_4x4 = 16;

		public const int Blur_X_Narrow = 17;

		public const int Blur_X_Medium = 18;

		public const int Blur_X_Wide = 19;

		public const int Blur_X_ExtraWide = 20;

		public const int Blur_Y_Narrow = 21;

		public const int Blur_Y_Medium = 22;

		public const int Blur_Y_Wide = 23;

		public const int Blur_Y_ExtraWide = 24;

		public const int Composite = 25;

		public const int Debug_AO_Only = 26;

		public const int Debug_ColorBleeding_Only = 27;

		public const int Debug_Split_WithoutAO_WithAO = 28;

		public const int Debug_Split_WithAO_AOOnly = 29;

		public const int Debug_Split_WithoutAO_AOOnly = 30;
	}

	private class RenderTarget
	{
		public bool orthographic;

		public bool hdr;

		public int width;

		public int height;

		public int fullWidth;

		public int fullHeight;

		public int layerWidth;

		public int layerHeight;

		public int downsamplingFactor;

		public int deinterleavingFactor;

		public int blurDownsamplingFactor;
	}
}
