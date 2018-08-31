using System;
using UnityEngine;

namespace UltimateWater
{
	public class GerstnerWave
	{
		public GerstnerWave()
		{
			this.Direction = new Vector2(0f, 1f);
			this.Frequency = 1f;
		}

		public GerstnerWave(WaterWave wave, Vector2[] scaleOffsets)
		{
			float w = wave._W;
			float num = (scaleOffsets[(int)wave._ScaleIndex].x * wave._Nkx + scaleOffsets[(int)wave._ScaleIndex].y * wave._Nky) * wave.K;
			this.Direction = new Vector2(wave._Nkx, wave._Nky);
			this.Amplitude = wave._Amplitude;
			this.Offset = num + wave._Offset;
			this.Frequency = wave.K;
			this.Speed = w;
		}

		public GerstnerWave(Vector2 direction, float amplitude, float offset, float frequency, float speed)
		{
			this.Direction = direction;
			this.Amplitude = amplitude;
			this.Offset = offset;
			this.Frequency = frequency;
			this.Speed = speed;
		}

		public Vector2 Direction;

		public float Amplitude;

		public float Offset;

		public float Frequency;

		public float Speed;
	}
}
