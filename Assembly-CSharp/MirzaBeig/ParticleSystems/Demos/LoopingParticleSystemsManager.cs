using System;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class LoopingParticleSystemsManager : ParticleManager
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			this.particlePrefabs[this.currentParticlePrefabIndex].gameObject.SetActive(true);
		}

		public override void Next()
		{
			this.particlePrefabs[this.currentParticlePrefabIndex].gameObject.SetActive(false);
			base.Next();
			this.particlePrefabs[this.currentParticlePrefabIndex].gameObject.SetActive(true);
		}

		public override void Previous()
		{
			this.particlePrefabs[this.currentParticlePrefabIndex].gameObject.SetActive(false);
			base.Previous();
			this.particlePrefabs[this.currentParticlePrefabIndex].gameObject.SetActive(true);
		}

		protected override void Update()
		{
			base.Update();
		}

		public override int GetParticleCount()
		{
			return this.particlePrefabs[this.currentParticlePrefabIndex].getParticleCount();
		}
	}
}
