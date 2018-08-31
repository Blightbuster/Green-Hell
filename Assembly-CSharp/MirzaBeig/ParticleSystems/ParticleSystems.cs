using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	public class ParticleSystems : MonoBehaviour
	{
		public ParticleSystem[] particleSystems { get; set; }

		public event ParticleSystems.onParticleSystemsDeadEventHandler onParticleSystemsDeadEvent;

		protected virtual void Awake()
		{
			this.particleSystems = base.GetComponentsInChildren<ParticleSystem>();
		}

		protected virtual void Start()
		{
		}

		protected virtual void Update()
		{
		}

		protected virtual void LateUpdate()
		{
			if (this.onParticleSystemsDeadEvent != null && !this.isAlive())
			{
				this.onParticleSystemsDeadEvent();
			}
		}

		public void reset()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].time = 0f;
			}
		}

		public void play()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].Play(false);
			}
		}

		public void pause()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].Pause(false);
			}
		}

		public void stop()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].Stop(false);
			}
		}

		public void clear()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].Clear(false);
			}
		}

		public void setLoop(bool loop)
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].main.loop = loop;
			}
		}

		public void setPlaybackSpeed(float speed)
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].main.simulationSpeed = speed;
			}
		}

		public void simulate(float time, bool reset = false)
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				this.particleSystems[i].Simulate(time, false, reset);
			}
		}

		public bool isAlive()
		{
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				if (this.particleSystems[i] && this.particleSystems[i].IsAlive())
				{
					return true;
				}
			}
			return false;
		}

		public bool isPlaying(bool checkAll = false)
		{
			if (this.particleSystems.Length == 0)
			{
				return false;
			}
			if (!checkAll)
			{
				return this.particleSystems[0].isPlaying;
			}
			for (int i = 0; i < 0; i++)
			{
				if (!this.particleSystems[i].isPlaying)
				{
					return false;
				}
			}
			return true;
		}

		public int getParticleCount()
		{
			int num = 0;
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				if (this.particleSystems[i])
				{
					num += this.particleSystems[i].particleCount;
				}
			}
			return num;
		}

		public delegate void onParticleSystemsDeadEventHandler();
	}
}
