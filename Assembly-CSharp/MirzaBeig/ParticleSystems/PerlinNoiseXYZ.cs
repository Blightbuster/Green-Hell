using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	[Serializable]
	public class PerlinNoiseXYZ
	{
		public void init()
		{
			this.x.init();
			this.y.init();
			this.z.init();
		}

		public Vector3 GetXYZ(float time)
		{
			float time2 = time * this.frequencyScale;
			return new Vector3(this.x.GetValue(time2), this.y.GetValue(time2), this.z.GetValue(time2)) * this.amplitudeScale;
		}

		public PerlinNoise x;

		public PerlinNoise y;

		public PerlinNoise z;

		public bool unscaledTime;

		public float amplitudeScale = 1f;

		public float frequencyScale = 1f;
	}
}
