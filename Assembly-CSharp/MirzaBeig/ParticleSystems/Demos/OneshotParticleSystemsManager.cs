using System;
using System.Collections.Generic;
using UnityEngine;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class OneshotParticleSystemsManager : ParticleManager
	{
		public bool disableSpawn { get; set; }

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			this.disableSpawn = false;
			this.spawnedPrefabs = new List<ParticleSystems>();
		}

		private void OnEnable()
		{
		}

		public void Clear()
		{
			if (this.spawnedPrefabs != null)
			{
				for (int i = 0; i < this.spawnedPrefabs.Count; i++)
				{
					if (this.spawnedPrefabs[i])
					{
						UnityEngine.Object.Destroy(this.spawnedPrefabs[i].gameObject);
					}
				}
				this.spawnedPrefabs.Clear();
			}
		}

		protected override void Update()
		{
			base.Update();
		}

		public void InstantiateParticlePrefab(Vector2 mousePosition, float maxDistance)
		{
			if (this.spawnedPrefabs != null && !this.disableSpawn)
			{
				Vector3 position = mousePosition;
				position.z = maxDistance;
				Vector3 vector = Camera.main.ScreenToWorldPoint(position);
				Vector3 direction = vector - Camera.main.transform.position;
				RaycastHit raycastHit;
				Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * 0.01f, direction, out raycastHit, maxDistance);
				Vector3 position2;
				if (raycastHit.collider)
				{
					position2 = raycastHit.point;
				}
				else
				{
					position2 = vector;
				}
				ParticleSystems particleSystems = this.particlePrefabs[this.currentParticlePrefabIndex];
				ParticleSystems particleSystems2 = UnityEngine.Object.Instantiate<ParticleSystems>(particleSystems, position2, particleSystems.transform.rotation);
				particleSystems2.gameObject.SetActive(true);
				particleSystems2.transform.parent = base.transform;
				this.spawnedPrefabs.Add(particleSystems2);
			}
		}

		public void Randomize()
		{
			this.currentParticlePrefabIndex = UnityEngine.Random.Range(0, this.particlePrefabs.Count);
		}

		public override int GetParticleCount()
		{
			int num = 0;
			if (this.spawnedPrefabs != null)
			{
				for (int i = 0; i < this.spawnedPrefabs.Count; i++)
				{
					if (this.spawnedPrefabs[i])
					{
						num += this.spawnedPrefabs[i].getParticleCount();
					}
					else
					{
						this.spawnedPrefabs.RemoveAt(i);
					}
				}
			}
			return num;
		}

		public LayerMask mouseRaycastLayerMask;

		private List<ParticleSystems> spawnedPrefabs;
	}
}
