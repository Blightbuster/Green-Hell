using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class DynamicWaterCameraData
	{
		public DynamicWaterCameraData(DynamicWater dynamicWater, WaterCamera camera, int antialiasing)
		{
			this.ResolveFormats();
			this.Camera = camera;
			this._DynamicWater = dynamicWater;
			this._Antialiasing = antialiasing;
			camera.RenderTargetResized += this.Camera_RenderTargetResized;
			this.CreateRenderTargets();
		}

		public RenderTexture DynamicDisplacementMap
		{
			get
			{
				return this._Textures[0];
			}
		}

		public RenderTexture NormalMap
		{
			get
			{
				return this._Textures[2];
			}
		}

		public RenderTexture FoamMap
		{
			get
			{
				return this._Textures[3];
			}
		}

		public RenderTexture FoamMapPrevious
		{
			get
			{
				return this._Textures[4];
			}
		}

		public RenderTexture DisplacementsMask
		{
			get
			{
				return this._Textures[1];
			}
		}

		public RenderTexture DiffuseMap
		{
			get
			{
				return this._Textures[5];
			}
		}

		public DynamicWater DynamicWater
		{
			get
			{
				return this._DynamicWater;
			}
		}

		public RenderTexture TotalDisplacementMap
		{
			get
			{
				RenderTexture renderTexture = this._Textures[7];
				if (!this._TotalDisplacementMapDirty)
				{
					return renderTexture;
				}
				this._DynamicWater.RenderTotalDisplacementMap(this.Camera, renderTexture);
				this._TotalDisplacementMapDirty = false;
				return renderTexture;
			}
		}

		public WaterCamera Camera { get; private set; }

		public RenderTexture GetDebugMap(bool createIfNotExists = false)
		{
			if (this._Textures.ContainsKey(6))
			{
				return this._Textures[6];
			}
			if (!createIfNotExists)
			{
				return null;
			}
			this._Textures.Add(6, this.DynamicDisplacementMap.CreateRenderTexture());
			this._Textures[6].name = "[UWS] DynamicWaterCameraData - Debug";
			return this._Textures[6];
		}

		public RenderTexture GetTotalDisplacementMap()
		{
			return this.TotalDisplacementMap;
		}

		public void Dispose()
		{
			this.Camera.RenderTargetResized -= this.Camera_RenderTargetResized;
			this.DisposeTextures();
		}

		public void ClearOverlays()
		{
			this.ValidateRTs();
			Dictionary<int, RenderTexture>.Enumerator enumerator = this._Textures.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, RenderTexture> keyValuePair = enumerator.Current;
				Graphics.SetRenderTarget(keyValuePair.Value);
				GL.Clear(false, true, (keyValuePair.Key != 1) ? Color.clear : Color.white);
			}
			enumerator.Dispose();
			Graphics.SetRenderTarget(null);
			this._TotalDisplacementMapDirty = true;
		}

		public void ValidateRTs()
		{
			Dictionary<int, RenderTexture>.Enumerator enumerator = this._Textures.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, RenderTexture> keyValuePair = enumerator.Current;
				keyValuePair.Value.Verify(true);
			}
			enumerator.Dispose();
		}

		public void SwapFoamMaps()
		{
			RenderTexture value = this._Textures[4];
			this._Textures[4] = this._Textures[3];
			this._Textures[3] = value;
		}

		private void DisposeTextures()
		{
			foreach (KeyValuePair<int, RenderTexture> keyValuePair in this._Textures)
			{
				keyValuePair.Value.Destroy();
			}
			this._Textures.Clear();
		}

		private void Camera_RenderTargetResized(WaterCamera camera)
		{
			this.CreateRenderTargets();
		}

		private void CreateRenderTargets()
		{
			this.DisposeTextures();
			int width = Mathf.RoundToInt((float)this.Camera.CameraComponent.pixelWidth);
			int height = Mathf.RoundToInt((float)this.Camera.CameraComponent.pixelHeight);
			TextureUtility.RenderTextureDesc desc = new TextureUtility.RenderTextureDesc("[UWS] DynamicWaterCameraData - Displacement")
			{
				Width = width,
				Height = height,
				Antialiasing = this._Antialiasing,
				Format = this._DisplacementFormat
			};
			TextureUtility.RenderTextureDesc desc2 = new TextureUtility.RenderTextureDesc("[UWS] DynamicWaterCameraData - Normals")
			{
				Width = width,
				Height = height,
				Antialiasing = this._Antialiasing,
				Format = this._NormalFormat
			};
			TextureUtility.RenderTextureDesc desc3 = new TextureUtility.RenderTextureDesc("[UWS] DynamicWaterCameraData - Foam")
			{
				Width = width,
				Height = height,
				Antialiasing = this._Antialiasing,
				Format = this._FoamFormat
			};
			TextureUtility.RenderTextureDesc desc4 = new TextureUtility.RenderTextureDesc("[UWS] DynamicWaterCameraData - Diffuse")
			{
				Width = width,
				Height = height,
				Antialiasing = this._Antialiasing,
				Format = this._DiffuseFormat
			};
			TextureUtility.RenderTextureDesc desc5 = new TextureUtility.RenderTextureDesc("[UWS] DynamicWaterCameraData - Total Displacement")
			{
				Width = 256,
				Height = 256,
				Antialiasing = 1,
				Format = this._TotalDisplacementFormat
			};
			this._Textures.Add(0, desc.CreateRenderTexture());
			this._Textures.Add(1, desc.CreateRenderTexture());
			this._Textures.Add(2, desc2.CreateRenderTexture());
			this._Textures.Add(3, desc3.CreateRenderTexture());
			this._Textures.Add(4, desc3.CreateRenderTexture());
			this._Textures.Add(5, desc4.CreateRenderTexture());
			this._Textures.Add(7, desc5.CreateRenderTexture());
		}

		private void ResolveFormats()
		{
			RenderTextureFormat? format = Compatibility.GetFormat(RenderTextureFormat.ARGBHalf, new RenderTextureFormat[]
			{
				RenderTextureFormat.ARGBFloat
			});
			this.SetFormat(format, ref this._DisplacementFormat);
			this.SetFormat(format, ref this._TotalDisplacementFormat);
			format = Compatibility.GetFormat(RenderTextureFormat.RGHalf, new RenderTextureFormat[]
			{
				RenderTextureFormat.RGFloat
			});
			this.SetFormat(format, ref this._NormalFormat);
			this.SetFormat(format, ref this._FoamFormat);
			format = Compatibility.GetFormat(RenderTextureFormat.ARGB32, new RenderTextureFormat[]
			{
				RenderTextureFormat.ARGBHalf,
				RenderTextureFormat.ARGBFloat
			});
			this.SetFormat(format, ref this._DiffuseFormat);
		}

		private void SetFormat(RenderTextureFormat? src, ref RenderTextureFormat dest)
		{
			if (src == null)
			{
				Debug.LogError("Target device does not support DynamicWaterEffects texture formats");
				return;
			}
			dest = src.Value;
		}

		internal int _LastFrameUsed;

		private bool _TotalDisplacementMapDirty;

		private readonly int _Antialiasing;

		private readonly DynamicWater _DynamicWater;

		private readonly Dictionary<int, RenderTexture> _Textures = new Dictionary<int, RenderTexture>();

		private RenderTextureFormat _DisplacementFormat;

		private RenderTextureFormat _NormalFormat;

		private RenderTextureFormat _FoamFormat;

		private RenderTextureFormat _DiffuseFormat;

		private RenderTextureFormat _TotalDisplacementFormat;

		private enum TextureTypes
		{
			Displacement,
			DisplacementMask,
			Normal,
			Foam,
			FoamPrevious,
			Diffuse,
			Debug,
			TotalDisplacement
		}
	}
}
