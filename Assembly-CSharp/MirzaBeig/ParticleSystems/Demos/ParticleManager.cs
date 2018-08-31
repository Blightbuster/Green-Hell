using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class ParticleManager : MonoBehaviour
	{
		public void Init()
		{
			this.particlePrefabs = base.GetComponentsInChildren<ParticleSystems>(true).ToList<ParticleSystems>();
			this.particlePrefabs.AddRange(this.particlePrefabsAppend);
			if (this.disableChildrenAtStart)
			{
				for (int i = 0; i < this.particlePrefabs.Count; i++)
				{
					this.particlePrefabs[i].gameObject.SetActive(false);
				}
			}
			this.initialized = true;
		}

		protected virtual void Awake()
		{
		}

		protected virtual void Start()
		{
			if (this.initialized)
			{
				this.Init();
			}
		}

		public virtual void Next()
		{
			this.currentParticlePrefabIndex++;
			if (this.currentParticlePrefabIndex > this.particlePrefabs.Count - 1)
			{
				this.currentParticlePrefabIndex = 0;
			}
		}

		public virtual void Previous()
		{
			this.currentParticlePrefabIndex--;
			if (this.currentParticlePrefabIndex < 0)
			{
				this.currentParticlePrefabIndex = this.particlePrefabs.Count - 1;
			}
		}

		public string GetCurrentPrefabName(bool shorten = false)
		{
			string text = this.particlePrefabs[this.currentParticlePrefabIndex].name;
			if (shorten)
			{
				int num = 0;
				for (int i = 0; i < this.prefabNameUnderscoreCountCutoff; i++)
				{
					num = text.IndexOf("_", num) + 1;
					if (num == 0)
					{
						MonoBehaviour.print("Iteration of underscore not found.");
						break;
					}
				}
				text = text.Substring(num, text.Length - num);
			}
			return string.Concat(new string[]
			{
				"PARTICLE SYSTEM: #",
				(this.currentParticlePrefabIndex + 1).ToString("00"),
				" / ",
				this.particlePrefabs.Count.ToString("00"),
				" (",
				text,
				")"
			});
		}

		public virtual int GetParticleCount()
		{
			return 0;
		}

		protected virtual void Update()
		{
		}

		protected List<ParticleSystems> particlePrefabs;

		public int currentParticlePrefabIndex;

		public List<ParticleSystems> particlePrefabsAppend;

		public int prefabNameUnderscoreCountCutoff = 4;

		public bool disableChildrenAtStart = true;

		private bool initialized;
	}
}
