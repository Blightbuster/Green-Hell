using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	[Serializable]
	public class PerlinNoise
	{
		public void init()
		{
			this.offset.x = UnityEngine.Random.Range(-32f, 32f);
			this.offset.y = UnityEngine.Random.Range(-32f, 32f);
		}

		public float GetValue(float time)
		{
			float num = time * this.frequency;
			return (Mathf.PerlinNoise(num + this.offset.x, num + this.offset.y) - 0.5f) * this.amplitude;
		}

		private Vector2 offset;

		public float amplitude = 1f;

		public float frequency = 1f;

		public bool unscaledTime;
	}
}
