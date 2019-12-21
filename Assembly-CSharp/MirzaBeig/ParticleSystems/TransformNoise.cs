using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	public class TransformNoise : MonoBehaviour
	{
		private void Start()
		{
			this.positionNoise.init();
			this.rotationNoise.init();
		}

		private void Update()
		{
			this.time = ((!this.unscaledTime) ? Time.time : Time.unscaledTime);
			base.transform.localPosition = this.positionNoise.GetXYZ(this.time);
			base.transform.localEulerAngles = this.rotationNoise.GetXYZ(this.time);
		}

		public PerlinNoiseXYZ positionNoise;

		public PerlinNoiseXYZ rotationNoise;

		public bool unscaledTime;

		private float time;
	}
}
