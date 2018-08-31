using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	public class TrailRenderers : MonoBehaviour
	{
		protected virtual void Awake()
		{
		}

		protected virtual void Start()
		{
			this.trailRenderers = base.GetComponentsInChildren<TrailRenderer>();
		}

		protected virtual void Update()
		{
		}

		public void setAutoDestruct(bool value)
		{
			for (int i = 0; i < this.trailRenderers.Length; i++)
			{
				this.trailRenderers[i].autodestruct = value;
			}
		}

		[HideInInspector]
		public TrailRenderer[] trailRenderers;
	}
}
