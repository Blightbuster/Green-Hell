using System;

namespace MirzaBeig.ParticleSystems
{
	public class ParticleSystemsSimulationSpeed : ParticleSystems
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
		}

		protected override void Update()
		{
			base.Update();
			base.setPlaybackSpeed(this.speed);
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();
		}

		public float speed = 1f;
	}
}
