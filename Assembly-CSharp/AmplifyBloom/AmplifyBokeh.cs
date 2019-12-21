using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplifyBloom
{
	[Serializable]
	public sealed class AmplifyBokeh : IAmplifyItem, ISerializationCallbackReceiver
	{
		public AmplifyBokeh()
		{
			this.m_bokehOffsets = new List<AmplifyBokehData>();
			this.CreateBokehOffsets(ApertureShape.Hexagon);
		}

		public void Destroy()
		{
			for (int i = 0; i < this.m_bokehOffsets.Count; i++)
			{
				this.m_bokehOffsets[i].Destroy();
			}
		}

		private void CreateBokehOffsets(ApertureShape shape)
		{
			this.m_bokehOffsets.Clear();
			switch (shape)
			{
			case ApertureShape.Square:
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation)));
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 90f)));
				return;
			case ApertureShape.Hexagon:
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation)));
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation - 75f)));
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 75f)));
				return;
			case ApertureShape.Octagon:
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation)));
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 65f)));
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 90f)));
				this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 115f)));
				return;
			default:
				return;
			}
		}

		private Vector4[] CalculateBokehSamples(int sampleCount, float angle)
		{
			Vector4[] array = new Vector4[sampleCount];
			float f = 0.0174532924f * angle;
			float num = (float)Screen.width / (float)Screen.height;
			Vector4 vector = new Vector4(this.m_bokehSampleRadius * Mathf.Cos(f), this.m_bokehSampleRadius * Mathf.Sin(f));
			vector.x /= num;
			for (int i = 0; i < sampleCount; i++)
			{
				float t = (float)i / ((float)sampleCount - 1f);
				array[i] = Vector4.Lerp(-vector, vector, t);
			}
			return array;
		}

		public void ApplyBokehFilter(RenderTexture source, Material material)
		{
			for (int i = 0; i < this.m_bokehOffsets.Count; i++)
			{
				this.m_bokehOffsets[i].BokehRenderTexture = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
			}
			material.SetVector(AmplifyUtils.BokehParamsId, this.m_bokehCameraProperties);
			for (int j = 0; j < this.m_bokehOffsets.Count; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[k], this.m_bokehOffsets[j].Offsets[k]);
				}
				Graphics.Blit(source, this.m_bokehOffsets[j].BokehRenderTexture, material, 27);
			}
			for (int l = 0; l < this.m_bokehOffsets.Count - 1; l++)
			{
				material.SetTexture(AmplifyUtils.AnamorphicRTS[l], this.m_bokehOffsets[l].BokehRenderTexture);
			}
			source.DiscardContents();
			Graphics.Blit(this.m_bokehOffsets[this.m_bokehOffsets.Count - 1].BokehRenderTexture, source, material, 28 + (this.m_bokehOffsets.Count - 2));
			for (int m = 0; m < this.m_bokehOffsets.Count; m++)
			{
				AmplifyUtils.ReleaseTempRenderTarget(this.m_bokehOffsets[m].BokehRenderTexture);
				this.m_bokehOffsets[m].BokehRenderTexture = null;
			}
		}

		public void OnAfterDeserialize()
		{
			this.CreateBokehOffsets(this.m_apertureShape);
		}

		public void OnBeforeSerialize()
		{
		}

		public ApertureShape ApertureShape
		{
			get
			{
				return this.m_apertureShape;
			}
			set
			{
				if (this.m_apertureShape != value)
				{
					this.m_apertureShape = value;
					this.CreateBokehOffsets(value);
				}
			}
		}

		public bool ApplyBokeh
		{
			get
			{
				return this.m_isActive;
			}
			set
			{
				this.m_isActive = value;
			}
		}

		public bool ApplyOnBloomSource
		{
			get
			{
				return this.m_applyOnBloomSource;
			}
			set
			{
				this.m_applyOnBloomSource = value;
			}
		}

		public float BokehSampleRadius
		{
			get
			{
				return this.m_bokehSampleRadius;
			}
			set
			{
				this.m_bokehSampleRadius = value;
			}
		}

		public float OffsetRotation
		{
			get
			{
				return this.m_offsetRotation;
			}
			set
			{
				this.m_offsetRotation = value;
			}
		}

		public Vector4 BokehCameraProperties
		{
			get
			{
				return this.m_bokehCameraProperties;
			}
			set
			{
				this.m_bokehCameraProperties = value;
			}
		}

		public float Aperture
		{
			get
			{
				return this.m_bokehCameraProperties.x;
			}
			set
			{
				this.m_bokehCameraProperties.x = value;
			}
		}

		public float FocalLength
		{
			get
			{
				return this.m_bokehCameraProperties.y;
			}
			set
			{
				this.m_bokehCameraProperties.y = value;
			}
		}

		public float FocalDistance
		{
			get
			{
				return this.m_bokehCameraProperties.z;
			}
			set
			{
				this.m_bokehCameraProperties.z = value;
			}
		}

		public float MaxCoCDiameter
		{
			get
			{
				return this.m_bokehCameraProperties.w;
			}
			set
			{
				this.m_bokehCameraProperties.w = value;
			}
		}

		private const int PerPassSampleCount = 8;

		[SerializeField]
		private bool m_isActive;

		[SerializeField]
		private bool m_applyOnBloomSource;

		[SerializeField]
		private float m_bokehSampleRadius = 0.5f;

		[SerializeField]
		private Vector4 m_bokehCameraProperties = new Vector4(0.05f, 0.018f, 1.34f, 0.18f);

		[SerializeField]
		private float m_offsetRotation;

		[SerializeField]
		private ApertureShape m_apertureShape = ApertureShape.Hexagon;

		private List<AmplifyBokehData> m_bokehOffsets;
	}
}
