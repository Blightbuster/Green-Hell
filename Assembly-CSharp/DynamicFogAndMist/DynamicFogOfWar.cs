using System;
using UnityEngine;

namespace DynamicFogAndMist
{
	[ExecuteInEditMode]
	public class DynamicFogOfWar : MonoBehaviour
	{
		public static DynamicFogOfWar instance
		{
			get
			{
				if (DynamicFogOfWar._instance == null)
				{
					DynamicFogOfWar._instance = UnityEngine.Object.FindObjectOfType<DynamicFogOfWar>();
				}
				return DynamicFogOfWar._instance;
			}
		}

		private void OnEnable()
		{
			this.fogMat = base.GetComponent<MeshRenderer>().sharedMaterial;
			this.UpdateFogOfWarTexture();
		}

		private void OnDisable()
		{
			if (this.fogOfWarTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(this.fogOfWarTexture);
				this.fogOfWarTexture = null;
			}
		}

		private void Update()
		{
			this.fogMat.SetVector("_FogOfWarData", new Vector4(base.transform.position.x, base.transform.position.z, base.transform.localScale.x, base.transform.localScale.y));
		}

		private void UpdateFogOfWarTexture()
		{
			int scaledSize = this.GetScaledSize(this.fogOfWarTextureSize, 1f);
			this.fogOfWarTexture = new Texture2D(scaledSize, scaledSize, TextureFormat.ARGB32, false);
			this.fogOfWarTexture.hideFlags = HideFlags.DontSave;
			this.fogOfWarTexture.filterMode = FilterMode.Bilinear;
			this.fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
			this.fogMat.mainTexture = this.fogOfWarTexture;
			this.ResetFogOfWar();
		}

		private int GetScaledSize(int size, float factor)
		{
			size = (int)((float)size / factor);
			size /= 4;
			if (size < 1)
			{
				size = 1;
			}
			return size * 4;
		}

		public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha)
		{
			if (this.fogOfWarTexture == null)
			{
				return;
			}
			float num = (worldPosition.x - base.transform.position.x) / base.transform.localScale.x + 0.5f;
			if (num < 0f || num > 1f)
			{
				return;
			}
			float num2 = (worldPosition.z - base.transform.position.z) / base.transform.localScale.y + 0.5f;
			if (num2 < 0f || num2 > 1f)
			{
				return;
			}
			int width = this.fogOfWarTexture.width;
			int height = this.fogOfWarTexture.height;
			int num3 = (int)(num * (float)width);
			int num4 = (int)(num2 * (float)height);
			int num5 = num4 * width + num3;
			byte b = (byte)(fogNewAlpha * 255f);
			Color32 color = this.fogOfWarColorBuffer[num5];
			if (b != color.a)
			{
				float num6 = radius / base.transform.localScale.y;
				int num7 = Mathf.FloorToInt((float)height * num6);
				for (int i = num4 - num7; i <= num4 + num7; i++)
				{
					if (i > 0 && i < height - 1)
					{
						for (int j = num3 - num7; j <= num3 + num7; j++)
						{
							if (j > 0 && j < width - 1)
							{
								int num8 = (int)Mathf.Sqrt((float)((num4 - i) * (num4 - i) + (num3 - j) * (num3 - j)));
								if (num8 <= num7)
								{
									num5 = i * width + j;
									Color32 color2 = this.fogOfWarColorBuffer[num5];
									color2.a = (byte)Mathf.Lerp((float)b, (float)color2.a, (float)num8 / (float)num7);
									this.fogOfWarColorBuffer[num5] = color2;
									this.fogOfWarTexture.SetPixel(j, i, color2);
								}
							}
						}
					}
				}
				this.fogOfWarTexture.Apply();
			}
		}

		public void ResetFogOfWar()
		{
			if (this.fogOfWarTexture == null)
			{
				return;
			}
			int height = this.fogOfWarTexture.height;
			int width = this.fogOfWarTexture.width;
			int num = height * width;
			if (this.fogOfWarColorBuffer == null || this.fogOfWarColorBuffer.Length != num)
			{
				this.fogOfWarColorBuffer = new Color32[num];
			}
			Color32 color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			for (int i = 0; i < num; i++)
			{
				this.fogOfWarColorBuffer[i] = color;
			}
			this.fogOfWarTexture.SetPixels32(this.fogOfWarColorBuffer);
			this.fogOfWarTexture.Apply();
		}

		public void SetFogOfWarTerrainBoundary(Terrain terrain, float borderWidth)
		{
			TerrainData terrainData = terrain.terrainData;
			int heightmapWidth = terrainData.heightmapWidth;
			int heightmapHeight = terrainData.heightmapHeight;
			float y = terrainData.size.y;
			float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);
			float num = base.transform.position.y - 1f;
			float num2 = base.transform.position.y + 10f;
			Vector3 b = new Vector3(-terrainData.size.x * 0.5f, 0f, -terrainData.size.z * 0.5f);
			for (int i = 0; i < heightmapHeight; i++)
			{
				for (int j = 0; j < heightmapWidth; j++)
				{
					float num3 = heights[i, j] * y + terrain.transform.position.y;
					if (num3 > num && num3 < num2)
					{
						Vector3 worldPosition = base.transform.position + b + new Vector3(terrainData.size.x * ((float)j + 0.5f) / (float)heightmapWidth, 0f, terrainData.size.z * ((float)i + 0.5f) / (float)heightmapHeight);
						this.SetFogOfWarAlpha(worldPosition, borderWidth, 0f);
					}
				}
			}
		}

		public int fogOfWarTextureSize = 512;

		private Material fogMat;

		private static DynamicFogOfWar _instance;

		private Texture2D fogOfWarTexture;

		private Color32[] fogOfWarColorBuffer;
	}
}
