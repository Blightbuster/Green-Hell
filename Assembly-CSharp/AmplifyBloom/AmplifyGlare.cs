using System;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public sealed class AmplifyGlare : IAmplifyItem
	{
		public AmplifyGlare()
		{
			this.m_currentGlareIdx = (int)this.m_currentGlareType;
			this.m_cromaticAberrationGrad = new Gradient();
			GradientColorKey[] colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.white, 0f),
				new GradientColorKey(Color.blue, 0.25f),
				new GradientColorKey(Color.green, 0.5f),
				new GradientColorKey(Color.yellow, 0.75f),
				new GradientColorKey(Color.red, 1f)
			};
			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 0.25f),
				new GradientAlphaKey(1f, 0.5f),
				new GradientAlphaKey(1f, 0.75f),
				new GradientAlphaKey(1f, 1f)
			};
			this.m_cromaticAberrationGrad.SetKeys(colorKeys, alphaKeys);
			this._rtBuffer = new RenderTexture[16];
			this.m_weigthsMat = new Matrix4x4[4];
			this.m_offsetsMat = new Matrix4x4[4];
			this.m_amplifyGlareCache = new AmplifyGlareCache();
			this.m_whiteReference = new Color(0.63f, 0.63f, 0.63f, 0f);
			this.m_aTanFoV = Mathf.Atan(0.3926991f);
			this.m_starDefArr = new StarDefData[]
			{
				new StarDefData(StarLibType.Cross, "Cross", 2, 4, 1f, 0.85f, 0f, 0.5f, -1f, 90f),
				new StarDefData(StarLibType.Cross_Filter, "CrossFilter", 2, 4, 1f, 0.95f, 0f, 0.5f, -1f, 90f),
				new StarDefData(StarLibType.Snow_Cross, "snowCross", 3, 4, 1f, 0.96f, 0.349f, 0.5f, -1f, -1f),
				new StarDefData(StarLibType.Vertical, "Vertical", 1, 4, 1f, 0.96f, 0f, 0f, -1f, -1f),
				new StarDefData(StarLibType.Sunny_Cross, "SunnyCross", 4, 4, 1f, 0.88f, 0f, 0f, 0.95f, 45f)
			};
			this.m_glareDefArr = new GlareDefData[]
			{
				new GlareDefData(StarLibType.Cross, 0f, 0.5f),
				new GlareDefData(StarLibType.Cross_Filter, 0.44f, 0.5f),
				new GlareDefData(StarLibType.Cross_Filter, 1.22f, 1.5f),
				new GlareDefData(StarLibType.Snow_Cross, 0.17f, 0.5f),
				new GlareDefData(StarLibType.Snow_Cross, 0.7f, 1.5f),
				new GlareDefData(StarLibType.Sunny_Cross, 0f, 0.5f),
				new GlareDefData(StarLibType.Sunny_Cross, 0.79f, 1.5f),
				new GlareDefData(StarLibType.Vertical, 1.57f, 0.5f),
				new GlareDefData(StarLibType.Vertical, 0f, 0.5f)
			};
		}

		public void Destroy()
		{
			for (int i = 0; i < this.m_starDefArr.Length; i++)
			{
				this.m_starDefArr[i].Destroy();
			}
			this.m_glareDefArr = null;
			this.m_weigthsMat = null;
			this.m_offsetsMat = null;
			for (int j = 0; j < this._rtBuffer.Length; j++)
			{
				if (this._rtBuffer[j] != null)
				{
					AmplifyUtils.ReleaseTempRenderTarget(this._rtBuffer[j]);
					this._rtBuffer[j] = null;
				}
			}
			this._rtBuffer = null;
			this.m_amplifyGlareCache.Destroy();
			this.m_amplifyGlareCache = null;
		}

		public void SetDirty()
		{
			this.m_isDirty = true;
		}

		public void OnRenderFromCache(RenderTexture source, RenderTexture dest, Material material, float glareIntensity, float cameraRotation)
		{
			for (int i = 0; i < this.m_amplifyGlareCache.TotalRT; i++)
			{
				this._rtBuffer[i] = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
			}
			int j = 0;
			for (int k = 0; k < this.m_amplifyGlareCache.StarDef.StarlinesCount; k++)
			{
				for (int l = 0; l < this.m_amplifyGlareCache.CurrentPassCount; l++)
				{
					this.UpdateMatrixesForPass(material, this.m_amplifyGlareCache.Starlines[k].Passes[l].Offsets, this.m_amplifyGlareCache.Starlines[k].Passes[l].Weights, glareIntensity, cameraRotation * this.m_amplifyGlareCache.StarDef.CameraRotInfluence);
					if (l == 0)
					{
						Graphics.Blit(source, this._rtBuffer[j], material, 2);
					}
					else
					{
						Graphics.Blit(this._rtBuffer[j - 1], this._rtBuffer[j], material, 2);
					}
					j++;
				}
			}
			for (int m = 0; m < this.m_amplifyGlareCache.StarDef.StarlinesCount; m++)
			{
				material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[m], this.m_amplifyGlareCache.AverageWeight);
				int num = (m + 1) * this.m_amplifyGlareCache.CurrentPassCount - 1;
				material.SetTexture(AmplifyUtils.AnamorphicRTS[m], this._rtBuffer[num]);
			}
			int pass = 19 + this.m_amplifyGlareCache.StarDef.StarlinesCount - 1;
			dest.DiscardContents();
			Graphics.Blit(this._rtBuffer[0], dest, material, pass);
			for (j = 0; j < this._rtBuffer.Length; j++)
			{
				AmplifyUtils.ReleaseTempRenderTarget(this._rtBuffer[j]);
				this._rtBuffer[j] = null;
			}
		}

		public void UpdateMatrixesForPass(Material material, Vector4[] offsets, Vector4[] weights, float glareIntensity, float rotation)
		{
			float num = Mathf.Cos(rotation);
			float num2 = Mathf.Sin(rotation);
			for (int i = 0; i < 16; i++)
			{
				int num3 = i >> 2;
				int row = i & 3;
				this.m_offsetsMat[num3][row, 0] = offsets[i].x * num - offsets[i].y * num2;
				this.m_offsetsMat[num3][row, 1] = offsets[i].x * num2 + offsets[i].y * num;
				this.m_weigthsMat[num3][row, 0] = glareIntensity * weights[i].x;
				this.m_weigthsMat[num3][row, 1] = glareIntensity * weights[i].y;
				this.m_weigthsMat[num3][row, 2] = glareIntensity * weights[i].z;
			}
			for (int j = 0; j < 4; j++)
			{
				material.SetMatrix(AmplifyUtils.AnamorphicGlareOffsetsMatStr[j], this.m_offsetsMat[j]);
				material.SetMatrix(AmplifyUtils.AnamorphicGlareWeightsMatStr[j], this.m_weigthsMat[j]);
			}
		}

		public void OnRenderImage(Material material, RenderTexture source, RenderTexture dest, float cameraRot)
		{
			Graphics.Blit(Texture2D.blackTexture, dest);
			if (this.m_isDirty || this.m_currentWidth != source.width || this.m_currentHeight != source.height)
			{
				this.m_isDirty = false;
				this.m_currentWidth = source.width;
				this.m_currentHeight = source.height;
				bool flag = false;
				GlareDefData glareDefData;
				if (this.m_currentGlareType == GlareLibType.Custom)
				{
					if (this.m_customGlareDef != null && this.m_customGlareDef.Length != 0)
					{
						glareDefData = this.m_customGlareDef[this.m_customGlareDefIdx];
						flag = true;
					}
					else
					{
						glareDefData = this.m_glareDefArr[0];
					}
				}
				else
				{
					glareDefData = this.m_glareDefArr[this.m_currentGlareIdx];
				}
				this.m_amplifyGlareCache.GlareDef = glareDefData;
				float num = (float)source.width;
				float num2 = (float)source.height;
				StarDefData starDefData = flag ? glareDefData.CustomStarData : this.m_starDefArr[(int)glareDefData.StarType];
				this.m_amplifyGlareCache.StarDef = starDefData;
				int num3 = (this.m_glareMaxPassCount < starDefData.PassCount) ? this.m_glareMaxPassCount : starDefData.PassCount;
				this.m_amplifyGlareCache.CurrentPassCount = num3;
				float num4 = glareDefData.StarInclination + starDefData.Inclination;
				for (int i = 0; i < this.m_glareMaxPassCount; i++)
				{
					float t = (float)(i + 1) / (float)this.m_glareMaxPassCount;
					for (int j = 0; j < 8; j++)
					{
						Color b = this._overallTint * Color.Lerp(this.m_cromaticAberrationGrad.Evaluate((float)j / 7f), this.m_whiteReference, t);
						this.m_amplifyGlareCache.CromaticAberrationMat[i, j] = Color.Lerp(this.m_whiteReference, b, glareDefData.ChromaticAberration);
					}
				}
				this.m_amplifyGlareCache.TotalRT = starDefData.StarlinesCount * num3;
				for (int k = 0; k < this.m_amplifyGlareCache.TotalRT; k++)
				{
					this._rtBuffer[k] = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
				}
				int l = 0;
				for (int m = 0; m < starDefData.StarlinesCount; m++)
				{
					StarLineData starLineData = starDefData.StarLinesArr[m];
					float f = num4 + starLineData.Inclination;
					float num5 = Mathf.Sin(f);
					float num6 = Mathf.Cos(f);
					Vector2 vector = default(Vector2);
					vector.x = num6 / num * (starLineData.SampleLength * this.m_overallStreakScale);
					vector.y = num5 / num2 * (starLineData.SampleLength * this.m_overallStreakScale);
					float num7 = (this.m_aTanFoV + 0.1f) * 280f / (num + num2) * 1.2f;
					for (int n = 0; n < num3; n++)
					{
						for (int num8 = 0; num8 < 8; num8++)
						{
							float d = Mathf.Pow(starLineData.Attenuation, num7 * (float)num8);
							this.m_amplifyGlareCache.Starlines[m].Passes[n].Weights[num8] = this.m_amplifyGlareCache.CromaticAberrationMat[num3 - 1 - n, num8] * d * ((float)n + 1f) * 0.5f;
							this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num8].x = vector.x * (float)num8;
							this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num8].y = vector.y * (float)num8;
							if (Mathf.Abs(this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num8].x) >= 0.9f || Mathf.Abs(this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num8].y) >= 0.9f)
							{
								this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num8].x = 0f;
								this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num8].y = 0f;
								this.m_amplifyGlareCache.Starlines[m].Passes[n].Weights[num8] *= 0f;
							}
						}
						for (int num9 = 8; num9 < 16; num9++)
						{
							this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num9] = -this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets[num9 - 8];
							this.m_amplifyGlareCache.Starlines[m].Passes[n].Weights[num9] = this.m_amplifyGlareCache.Starlines[m].Passes[n].Weights[num9 - 8];
						}
						this.UpdateMatrixesForPass(material, this.m_amplifyGlareCache.Starlines[m].Passes[n].Offsets, this.m_amplifyGlareCache.Starlines[m].Passes[n].Weights, this.m_intensity, starDefData.CameraRotInfluence * cameraRot);
						if (n == 0)
						{
							Graphics.Blit(source, this._rtBuffer[l], material, 2);
						}
						else
						{
							Graphics.Blit(this._rtBuffer[l - 1], this._rtBuffer[l], material, 2);
						}
						l++;
						vector *= this.m_perPassDisplacement;
						num7 *= this.m_perPassDisplacement;
					}
				}
				this.m_amplifyGlareCache.AverageWeight = Vector4.one / (float)starDefData.StarlinesCount;
				for (int num10 = 0; num10 < starDefData.StarlinesCount; num10++)
				{
					material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[num10], this.m_amplifyGlareCache.AverageWeight);
					int num11 = (num10 + 1) * num3 - 1;
					material.SetTexture(AmplifyUtils.AnamorphicRTS[num10], this._rtBuffer[num11]);
				}
				int pass = 19 + starDefData.StarlinesCount - 1;
				dest.DiscardContents();
				Graphics.Blit(this._rtBuffer[0], dest, material, pass);
				for (l = 0; l < this._rtBuffer.Length; l++)
				{
					AmplifyUtils.ReleaseTempRenderTarget(this._rtBuffer[l]);
					this._rtBuffer[l] = null;
				}
				return;
			}
			this.OnRenderFromCache(source, dest, material, this.m_intensity, cameraRot);
		}

		public GlareLibType CurrentGlare
		{
			get
			{
				return this.m_currentGlareType;
			}
			set
			{
				if (this.m_currentGlareType != value)
				{
					this.m_currentGlareType = value;
					this.m_currentGlareIdx = (int)value;
					this.m_isDirty = true;
				}
			}
		}

		public int GlareMaxPassCount
		{
			get
			{
				return this.m_glareMaxPassCount;
			}
			set
			{
				this.m_glareMaxPassCount = value;
				this.m_isDirty = true;
			}
		}

		public float PerPassDisplacement
		{
			get
			{
				return this.m_perPassDisplacement;
			}
			set
			{
				this.m_perPassDisplacement = value;
				this.m_isDirty = true;
			}
		}

		public float Intensity
		{
			get
			{
				return this.m_intensity;
			}
			set
			{
				this.m_intensity = ((value < 0f) ? 0f : value);
				this.m_isDirty = true;
			}
		}

		public Color OverallTint
		{
			get
			{
				return this._overallTint;
			}
			set
			{
				this._overallTint = value;
				this.m_isDirty = true;
			}
		}

		public bool ApplyLensGlare
		{
			get
			{
				return this.m_applyGlare;
			}
			set
			{
				this.m_applyGlare = value;
			}
		}

		public Gradient CromaticColorGradient
		{
			get
			{
				return this.m_cromaticAberrationGrad;
			}
			set
			{
				this.m_cromaticAberrationGrad = value;
				this.m_isDirty = true;
			}
		}

		public float OverallStreakScale
		{
			get
			{
				return this.m_overallStreakScale;
			}
			set
			{
				this.m_overallStreakScale = value;
				this.m_isDirty = true;
			}
		}

		public GlareDefData[] CustomGlareDef
		{
			get
			{
				return this.m_customGlareDef;
			}
			set
			{
				this.m_customGlareDef = value;
			}
		}

		public int CustomGlareDefIdx
		{
			get
			{
				return this.m_customGlareDefIdx;
			}
			set
			{
				this.m_customGlareDefIdx = value;
			}
		}

		public int CustomGlareDefAmount
		{
			get
			{
				return this.m_customGlareDefAmount;
			}
			set
			{
				if (value == this.m_customGlareDefAmount)
				{
					return;
				}
				if (value == 0)
				{
					this.m_customGlareDef = null;
					this.m_customGlareDefIdx = 0;
					this.m_customGlareDefAmount = 0;
					return;
				}
				GlareDefData[] array = new GlareDefData[value];
				for (int i = 0; i < value; i++)
				{
					if (i < this.m_customGlareDefAmount)
					{
						array[i] = this.m_customGlareDef[i];
					}
					else
					{
						array[i] = new GlareDefData();
					}
				}
				this.m_customGlareDefIdx = Mathf.Clamp(this.m_customGlareDefIdx, 0, value - 1);
				this.m_customGlareDef = array;
				this.m_customGlareDefAmount = value;
			}
		}

		public const int MaxLineSamples = 8;

		public const int MaxTotalSamples = 16;

		public const int MaxStarLines = 4;

		public const int MaxPasses = 4;

		public const int MaxCustomGlare = 32;

		[SerializeField]
		private GlareDefData[] m_customGlareDef;

		[SerializeField]
		private int m_customGlareDefIdx;

		[SerializeField]
		private int m_customGlareDefAmount;

		[SerializeField]
		private bool m_applyGlare = true;

		[SerializeField]
		private Color _overallTint = Color.white;

		[SerializeField]
		private Gradient m_cromaticAberrationGrad;

		[SerializeField]
		private int m_glareMaxPassCount = 4;

		private StarDefData[] m_starDefArr;

		private GlareDefData[] m_glareDefArr;

		private Matrix4x4[] m_weigthsMat;

		private Matrix4x4[] m_offsetsMat;

		private Color m_whiteReference;

		private float m_aTanFoV;

		private AmplifyGlareCache m_amplifyGlareCache;

		[SerializeField]
		private int m_currentWidth;

		[SerializeField]
		private int m_currentHeight;

		[SerializeField]
		private GlareLibType m_currentGlareType;

		[SerializeField]
		private int m_currentGlareIdx;

		[SerializeField]
		private float m_perPassDisplacement = 4f;

		[SerializeField]
		private float m_intensity = 0.17f;

		[SerializeField]
		private float m_overallStreakScale = 1f;

		private bool m_isDirty = true;

		private RenderTexture[] _rtBuffer;
	}
}
