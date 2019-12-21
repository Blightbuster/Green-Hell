using System;
using UnityEngine;

namespace IESLights
{
	[ExecuteInEditMode]
	public class IESToCubemap : MonoBehaviour
	{
		private void OnDestroy()
		{
			if (this._horizontalMirrorMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(this._horizontalMirrorMaterial);
			}
		}

		public void CreateCubemap(Texture2D iesTexture, IESData iesData, int resolution, out Cubemap cubemap)
		{
			this.PrepMaterial(iesTexture, iesData);
			this.CreateCubemap(resolution, out cubemap);
		}

		public Color[] CreateRawCubemap(Texture2D iesTexture, IESData iesData, int resolution)
		{
			this.PrepMaterial(iesTexture, iesData);
			RenderTexture[] array = new RenderTexture[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				array[i].filterMode = FilterMode.Trilinear;
			}
			Camera[] componentsInChildren = base.transform.GetChild(0).GetComponentsInChildren<Camera>();
			for (int j = 0; j < 6; j++)
			{
				componentsInChildren[j].targetTexture = array[j];
				componentsInChildren[j].Render();
				componentsInChildren[j].targetTexture = null;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(resolution * 6, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			temporary.filterMode = FilterMode.Trilinear;
			if (this._horizontalMirrorMaterial == null)
			{
				this._horizontalMirrorMaterial = new Material(Shader.Find("Hidden/IES/HorizontalFlip"));
			}
			RenderTexture.active = temporary;
			for (int k = 0; k < 6; k++)
			{
				GL.PushMatrix();
				GL.LoadPixelMatrix(0f, (float)(resolution * 6), 0f, (float)resolution);
				Graphics.DrawTexture(new Rect((float)(k * resolution), 0f, (float)resolution, (float)resolution), array[k], this._horizontalMirrorMaterial);
				GL.PopMatrix();
			}
			Texture2D texture2D = new Texture2D(resolution * 6, resolution, TextureFormat.RGBAFloat, false, true)
			{
				filterMode = FilterMode.Trilinear
			};
			texture2D.ReadPixels(new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), 0, 0);
			Color[] pixels = texture2D.GetPixels();
			RenderTexture.active = null;
			RenderTexture[] array2 = array;
			for (int l = 0; l < array2.Length; l++)
			{
				RenderTexture.ReleaseTemporary(array2[l]);
			}
			RenderTexture.ReleaseTemporary(temporary);
			UnityEngine.Object.DestroyImmediate(texture2D);
			return pixels;
		}

		private void PrepMaterial(Texture2D iesTexture, IESData iesData)
		{
			if (this._iesMaterial == null)
			{
				this._iesMaterial = base.GetComponent<Renderer>().sharedMaterial;
			}
			this._iesMaterial.mainTexture = iesTexture;
			this.SetShaderKeywords(iesData, this._iesMaterial);
		}

		private void SetShaderKeywords(IESData iesData, Material iesMaterial)
		{
			if (iesData.VerticalType == VerticalType.Bottom)
			{
				iesMaterial.EnableKeyword("BOTTOM_VERTICAL");
				iesMaterial.DisableKeyword("TOP_VERTICAL");
				iesMaterial.DisableKeyword("FULL_VERTICAL");
			}
			else if (iesData.VerticalType == VerticalType.Top)
			{
				iesMaterial.EnableKeyword("TOP_VERTICAL");
				iesMaterial.DisableKeyword("BOTTOM_VERTICAL");
				iesMaterial.DisableKeyword("FULL_VERTICAL");
			}
			else
			{
				iesMaterial.DisableKeyword("TOP_VERTICAL");
				iesMaterial.DisableKeyword("BOTTOM_VERTICAL");
				iesMaterial.EnableKeyword("FULL_VERTICAL");
			}
			if (iesData.HorizontalType == HorizontalType.None)
			{
				iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
				iesMaterial.DisableKeyword("HALF_HORIZONTAL");
				iesMaterial.DisableKeyword("FULL_HORIZONTAL");
				return;
			}
			if (iesData.HorizontalType == HorizontalType.Quadrant)
			{
				iesMaterial.EnableKeyword("QUAD_HORIZONTAL");
				iesMaterial.DisableKeyword("HALF_HORIZONTAL");
				iesMaterial.DisableKeyword("FULL_HORIZONTAL");
				return;
			}
			if (iesData.HorizontalType == HorizontalType.Half)
			{
				iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
				iesMaterial.EnableKeyword("HALF_HORIZONTAL");
				iesMaterial.DisableKeyword("FULL_HORIZONTAL");
				return;
			}
			if (iesData.HorizontalType == HorizontalType.Full)
			{
				iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
				iesMaterial.DisableKeyword("HALF_HORIZONTAL");
				iesMaterial.EnableKeyword("FULL_HORIZONTAL");
			}
		}

		private void CreateCubemap(int resolution, out Cubemap cubemap)
		{
			cubemap = new Cubemap(resolution, TextureFormat.ARGB32, false)
			{
				filterMode = FilterMode.Trilinear
			};
			base.GetComponent<Camera>().RenderToCubemap(cubemap);
		}

		private Material _iesMaterial;

		private Material _horizontalMirrorMaterial;
	}
}
