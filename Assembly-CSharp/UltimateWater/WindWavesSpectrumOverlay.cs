using System;
using UnityEngine;

namespace UltimateWater
{
	public class WindWavesSpectrumOverlay
	{
		public WindWavesSpectrumOverlay(WindWaves windWaves)
		{
			this._WindWaves = windWaves;
			this._SpectrumData = new Vector2[4][];
			for (int i = 0; i < 4; i++)
			{
				this._SpectrumData[i] = new Vector2[windWaves.FinalResolution * windWaves.FinalResolution];
			}
		}

		public event Action Cleared;

		public void Destroy()
		{
			this._SpectrumData = null;
			this.Cleared = null;
			if (this._Texture != null)
			{
				UnityEngine.Object.Destroy(this._Texture);
				this._Texture = null;
			}
		}

		public Texture2D Texture
		{
			get
			{
				if (this._TextureDirty)
				{
					this.ValidateTexture();
				}
				return this._Texture;
			}
		}

		public Vector2[] GetSpectrumDataDirect(int tileIndex)
		{
			return this._SpectrumData[tileIndex];
		}

		public void Refresh()
		{
			int finalResolution = this._WindWaves.FinalResolution;
			int num = finalResolution * finalResolution;
			for (int i = 0; i < 4; i++)
			{
				Vector2[] array = this._SpectrumData[i];
				if (array.Length == num)
				{
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = new Vector2(0f, 0f);
					}
				}
				else
				{
					this._SpectrumData[i] = new Vector2[num];
				}
			}
			this._TextureDirty = true;
			if (this.Cleared != null)
			{
				this.Cleared();
			}
		}

		private void ValidateTexture()
		{
			this._TextureDirty = false;
			int finalResolution = this._WindWaves.FinalResolution;
			int num = finalResolution << 1;
			if (this._Texture != null && this._Texture.width != num)
			{
				UnityEngine.Object.Destroy(this._Texture);
				this._Texture = null;
			}
			if (this._Texture == null)
			{
				this._Texture = new Texture2D(num, num, TextureFormat.RGHalf, false, true)
				{
					filterMode = FilterMode.Point
				};
			}
			for (int i = 0; i < 4; i++)
			{
				Vector2[] array = this._SpectrumData[i];
				int num2 = (i != 1 && i != 3) ? 0 : finalResolution;
				int num3 = (i != 2 && i != 3) ? 0 : finalResolution;
				for (int j = finalResolution - 1; j >= 0; j--)
				{
					for (int k = finalResolution - 1; k >= 0; k--)
					{
						Vector2 vector = array[j * finalResolution + k];
						this._Texture.SetPixel(num2 + j, num3 + k, new Color(vector.x, vector.y, 0f, 0f));
					}
				}
			}
			this._Texture.Apply(false, false);
		}

		private Vector2[][] _SpectrumData;

		private Texture2D _Texture;

		private bool _TextureDirty = true;

		private readonly WindWaves _WindWaves;
	}
}
