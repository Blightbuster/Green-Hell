using System;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(9f, DocumentationSortingAttribute.Level.UserRef)]
	public sealed class NoiseSettings : ScriptableObject
	{
		public NoiseSettings.TransformNoiseParams[] PositionNoise
		{
			get
			{
				return this.m_Position;
			}
		}

		public NoiseSettings.TransformNoiseParams[] OrientationNoise
		{
			get
			{
				return this.m_Orientation;
			}
		}

		public void CopyFrom(NoiseSettings other)
		{
			this.m_Position = new NoiseSettings.TransformNoiseParams[other.m_Position.Length];
			other.m_Position.CopyTo(this.m_Position, 0);
			this.m_Orientation = new NoiseSettings.TransformNoiseParams[other.m_Orientation.Length];
			other.m_Orientation.CopyTo(this.m_Orientation, 0);
		}

		[SerializeField]
		[Tooltip("These are the noise channels for the virtual camera's position. Convincing noise setups typically mix low, medium and high frequencies together, so start with a size of 3")]
		private NoiseSettings.TransformNoiseParams[] m_Position = new NoiseSettings.TransformNoiseParams[0];

		[SerializeField]
		[Tooltip("These are the noise channels for the virtual camera's orientation. Convincing noise setups typically mix low, medium and high frequencies together, so start with a size of 3")]
		private NoiseSettings.TransformNoiseParams[] m_Orientation = new NoiseSettings.TransformNoiseParams[0];

		[DocumentationSorting(9.1f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct NoiseParams
		{
			[Tooltip("The amplitude of the noise for this channel.  Larger numbers vibrate higher.")]
			public float Amplitude;

			[Tooltip("The frequency of noise for this channel.  Higher magnitudes vibrate faster.")]
			public float Frequency;
		}

		[DocumentationSorting(9.2f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct TransformNoiseParams
		{
			[Tooltip("Noise definition for X-axis")]
			public NoiseSettings.NoiseParams X;

			[Tooltip("Noise definition for Y-axis")]
			public NoiseSettings.NoiseParams Y;

			[Tooltip("Noise definition for Z-axis")]
			public NoiseSettings.NoiseParams Z;
		}
	}
}
