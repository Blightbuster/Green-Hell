using System;
using UnityEngine;

namespace MirzaBeig.Demos.Wallpapers
{
	public class GravityClockInteractivityUVFX : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void SetGravityClockVisualEffectsActive(bool value)
		{
			if (value)
			{
				if (this.enableGravityClockVisualEffects)
				{
					this.gravityClock = UnityEngine.Object.Instantiate<ParticleSystem>(this.gravityClockPrefab, base.transform);
					this.gravityClock.transform.localPosition = Vector3.zero;
				}
			}
			else if (this.gravityClock)
			{
				this.gravityClock.Stop();
				this.gravityClock.transform.SetParent(null, true);
			}
		}

		public void SetGravityClockAttractionForceActive(bool value)
		{
			if (value)
			{
				if (this.enableGravityClockAttractionForce)
				{
					this.forceAffectors.gameObject.SetActive(true);
					this.forceAffectors2.gameObject.SetActive(true);
				}
			}
			else
			{
				this.forceAffectors.gameObject.SetActive(false);
				this.forceAffectors2.gameObject.SetActive(false);
			}
		}

		public GameObject forceAffectors;

		public GameObject forceAffectors2;

		public ParticleSystem gravityClockPrefab;

		private ParticleSystem gravityClock;

		public bool enableGravityClockVisualEffects = true;

		public bool enableGravityClockAttractionForce = true;
	}
}
