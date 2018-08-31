using System;

namespace UltimateWater.Internal
{
	public interface IWavesParticleSystemPlugin
	{
		void UpdateParticles(float time, float deltaTime);
	}
}
