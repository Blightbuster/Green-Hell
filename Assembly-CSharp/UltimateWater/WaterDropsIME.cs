using System;
using UltimateWater.Internal;
using UltimateWater.Utils;
using UnityEngine;

namespace UltimateWater
{
	public class WaterDropsIME : MonoBehaviour, IWaterImageEffect
	{
		public RenderTexture Mask
		{
			get
			{
				return this._Masking._MaskB;
			}
		}

		public void OnWaterCameraEnabled()
		{
		}

		public void OnWaterCameraPreCull()
		{
		}

		private void Awake()
		{
			this._Masking._Material = ShaderUtility.Instance.CreateMaterial(ShaderList.WaterdropsMask, HideFlags.None);
			this.AssignModule();
		}

		private void OnValidate()
		{
			ShaderUtility.Instance.Use(ShaderList.WaterdropsMask);
			this.AssignModule();
			this._SelectedModule.Validate();
			if (Application.isPlaying && this._Masking._Material != null)
			{
				this._Masking._Material.SetFloat("_Fadeout", this.Fade * 0.98f);
			}
		}

		private void OnPreCull()
		{
			this._SelectedModule.Advance();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (Application.isPlaying)
			{
				this.CheckResources();
				this._Masking.Blit();
				this._Masking.Swap();
				this._SelectedModule.UpdateMask(this._Masking._MaskB);
			}
			this._SelectedModule.RenderImage(source, destination);
		}

		private void Reset()
		{
			this.Normal.Intensity = 1f;
			this.Blur.FadeSpeed = 1f;
		}

		private void OnDestroy()
		{
			this._Masking.Release();
		}

		private void AssignModule()
		{
			WaterDropsIME.Type type = this._Type;
			if (type != WaterDropsIME.Type.Blur)
			{
				if (type == WaterDropsIME.Type.NormalMap)
				{
					this._SelectedModule = this.Normal;
				}
			}
			else
			{
				this._SelectedModule = this.Blur;
			}
			this._SelectedModule.Initialize(this);
		}

		private void CheckResources()
		{
			if (this._Masking._MaskA == null || this._Masking._MaskA.width != Screen.width >> 1 || this._Masking._MaskA.height != Screen.height >> 1)
			{
				this._Masking._MaskA = WaterDropsIME.CreateMaskRt();
				this._Masking._MaskB = WaterDropsIME.CreateMaskRt();
				this._Masking._MaskA.name = "[UWS] WaterDropsIME - Mask A";
				this._Masking._MaskB.name = "[UWS] WaterDropsIME - Mask B";
			}
		}

		private static RenderTexture CreateMaskRt()
		{
			RenderTexture renderTexture = new RenderTexture(Screen.width >> 1, Screen.height >> 1, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
			{
				hideFlags = HideFlags.DontSave,
				filterMode = FilterMode.Bilinear,
				name = "[UWS] WaterDropsIME - CreateMaskRt"
			};
			Graphics.SetRenderTarget(renderTexture);
			GL.Clear(false, true, Color.black);
			return renderTexture;
		}

		private string Validation()
		{
			string text = string.Empty;
			if (this._Type == WaterDropsIME.Type.NormalMap && this.Normal.NormalMap == null)
			{
				text += "warning: select normal map texture";
			}
			return text;
		}

		[Range(0.95f, 1f)]
		[Tooltip("How slow the effects disappear")]
		public float Fade = 1f;

		public WaterDropsIME.BlurModule Blur;

		public WaterDropsIME.NormalModule Normal;

		[SerializeField]
		private WaterDropsIME.Type _Type;

		private WaterDropsIME.MaskingModule _Masking;

		private WaterDropsIME.IWaterDropsModule _SelectedModule;

		[SerializeField]
		[InspectorWarning("Validation", InspectorWarningAttribute.InfoType.Warning)]
		private string _Validation;

		public enum Type
		{
			NormalMap,
			Blur
		}

		public interface IWaterDropsModule
		{
			void Initialize(WaterDropsIME ime);

			void Validate();

			void Advance();

			void UpdateMask(RenderTexture mask);

			void RenderImage(RenderTexture source, RenderTexture destination);
		}

		[Serializable]
		public struct BlurModule : WaterDropsIME.IWaterDropsModule
		{
			public void Initialize(WaterDropsIME effect)
			{
				this._Reference = effect;
				this._Camera = effect.GetComponent<WaterCamera>();
			}

			public void Validate()
			{
				this._Blur.Validate("UltimateWater/Utilities/Blur (VisionBlur)", null, 0);
			}

			public void Advance()
			{
				if (!Application.isPlaying)
				{
					return;
				}
				this._Intensity += Mathf.Max(0f, this._Camera.WaterLevel - this._Reference.transform.position.y);
				this._Intensity *= 1f - Time.deltaTime * this.FadeSpeed;
				this._Intensity = Mathf.Clamp01(this._Intensity);
			}

			public void RenderImage(RenderTexture source, RenderTexture destination)
			{
				float size = this._Blur.Size;
				this._Blur.Size *= this._Intensity;
				this._Blur.Apply(source);
				this._Blur.Size = size;
				Graphics.Blit(source, destination);
			}

			public void UpdateMask(RenderTexture mask)
			{
			}

			public float FadeSpeed;

			private WaterDropsIME _Reference;

			private WaterCamera _Camera;

			private float _Intensity;

			[HideInInspector]
			[SerializeField]
			private Blur _Blur;
		}

		[Serializable]
		public struct NormalModule : WaterDropsIME.IWaterDropsModule
		{
			public void Initialize(WaterDropsIME effect)
			{
				this._Material = ShaderUtility.Instance.CreateMaterial(ShaderList.WaterdropsNormal, HideFlags.None);
				this._Material.SetTexture("_NormalMap", this.NormalMap);
			}

			public void UpdateMask(RenderTexture mask)
			{
				if (this.Preview)
				{
					Graphics.Blit(DefaultTextures.Get(Color.white), mask);
				}
				this._Material.SetTexture("_Mask", mask);
			}

			public void RenderImage(RenderTexture source, RenderTexture destination)
			{
				if (!Application.isPlaying && !this.Preview)
				{
					Graphics.Blit(source, destination);
					return;
				}
				this._Material.SetFloat("_Intensity", this.Intensity);
				Graphics.Blit(source, destination, this._Material);
			}

			public void Validate()
			{
				ShaderUtility.Instance.Use(ShaderList.WaterdropsNormal);
				if (this._Material == null)
				{
					this._Material = ShaderUtility.Instance.CreateMaterial(ShaderList.WaterdropsNormal, HideFlags.None);
				}
				this._Material.SetTexture("_NormalMap", this.NormalMap);
			}

			public void Advance()
			{
			}

			public Texture NormalMap;

			public float Intensity;

			public bool Preview;

			private Material _Material;
		}

		private struct MaskingModule
		{
			internal void Blit()
			{
				Graphics.Blit(this._MaskA, this._MaskB, this._Material, 0);
			}

			internal void Swap()
			{
				RenderTexture maskA = this._MaskA;
				this._MaskA = this._MaskB;
				this._MaskB = maskA;
			}

			internal void Release()
			{
				TextureUtility.Release(ref this._MaskA);
				TextureUtility.Release(ref this._MaskB);
			}

			internal Material _Material;

			internal RenderTexture _MaskA;

			internal RenderTexture _MaskB;
		}
	}
}
