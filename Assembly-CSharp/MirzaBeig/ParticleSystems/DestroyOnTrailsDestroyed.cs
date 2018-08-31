using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	public class DestroyOnTrailsDestroyed : TrailRenderers
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
			bool flag = true;
			for (int i = 0; i < this.trailRenderers.Length; i++)
			{
				if (this.trailRenderers[i] != null)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
