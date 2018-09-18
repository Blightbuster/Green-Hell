using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[SaveDuringPlay]
	[RequireComponent(typeof(CinemachinePipeline))]
	[DocumentationSorting(8f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("")]
	public class CinemachineBasicMultiChannelPerlin : CinemachineComponentBase
	{
		public override bool IsValid
		{
			get
			{
				return base.enabled && this.m_NoiseProfile != null;
			}
		}

		public override CinemachineCore.Stage Stage
		{
			get
			{
				return CinemachineCore.Stage.Noise;
			}
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			if (!this.IsValid || deltaTime < 0f)
			{
				return;
			}
			if (!this.mInitialized)
			{
				this.Initialize();
			}
			this.mNoiseTime += deltaTime * this.m_FrequencyGain;
			curState.PositionCorrection += curState.CorrectedOrientation * CinemachineBasicMultiChannelPerlin.GetCombinedFilterResults(this.m_NoiseProfile.PositionNoise, this.mNoiseTime, this.mNoiseOffsets) * this.m_AmplitudeGain;
			Quaternion rhs = Quaternion.Euler(CinemachineBasicMultiChannelPerlin.GetCombinedFilterResults(this.m_NoiseProfile.OrientationNoise, this.mNoiseTime, this.mNoiseOffsets) * this.m_AmplitudeGain);
			curState.OrientationCorrection *= rhs;
		}

		private void Initialize()
		{
			this.mInitialized = true;
			this.mNoiseTime = 0f;
			this.mNoiseOffsets = new Vector3(UnityEngine.Random.Range(-10000f, 10000f), UnityEngine.Random.Range(-10000f, 10000f), UnityEngine.Random.Range(-10000f, 10000f));
		}

		private static Vector3 GetCombinedFilterResults(NoiseSettings.TransformNoiseParams[] noiseParams, float time, Vector3 noiseOffsets)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			if (noiseParams != null)
			{
				foreach (NoiseSettings.TransformNoiseParams transformNoiseParams in noiseParams)
				{
					Vector3 a = new Vector3(transformNoiseParams.X.Frequency, transformNoiseParams.Y.Frequency, transformNoiseParams.Z.Frequency) * time;
					a += noiseOffsets;
					Vector3 vector = new Vector3(Mathf.PerlinNoise(a.x, 0f) - 0.5f, Mathf.PerlinNoise(a.y, 0f) - 0.5f, Mathf.PerlinNoise(a.z, 0f) - 0.5f);
					num += vector.x * transformNoiseParams.X.Amplitude;
					num2 += vector.y * transformNoiseParams.Y.Amplitude;
					num3 += vector.z * transformNoiseParams.Z.Amplitude;
				}
			}
			return new Vector3(num, num2, num3);
		}

		[FormerlySerializedAs("m_Definition")]
		[HideInInspector]
		[Tooltip("The asset containing the Noise Profile.  Define the frequencies and amplitudes there to make a characteristic noise profile.  Make your own or just use one of the many presets.")]
		public NoiseSettings m_NoiseProfile;

		[Tooltip("Gain to apply to the amplitudes defined in the NoiseSettings asset.  1 is normal.  Setting this to 0 completely mutes the noise.")]
		public float m_AmplitudeGain = 1f;

		[Tooltip("Scale factor to apply to the frequencies defined in the NoiseSettings asset.  1 is normal.  Larger magnitudes will make the noise shake more rapidly.")]
		public float m_FrequencyGain = 1f;

		private bool mInitialized;

		private float mNoiseTime;

		private Vector3 mNoiseOffsets = Vector3.zero;
	}
}
