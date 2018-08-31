using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	public class IgnoreTimeScale : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
			this.particleSystem = base.GetComponent<ParticleSystem>();
		}

		private void Update()
		{
			this.particleSystem.Simulate(Time.unscaledDeltaTime, true, false);
		}

		private ParticleSystem particleSystem;
	}
}
