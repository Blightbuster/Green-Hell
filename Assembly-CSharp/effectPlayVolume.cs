using System;
using UnityEngine;

public class effectPlayVolume : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		this.armed = true;
	}

	private void OnTriggerExit(Collider other)
	{
		this.armed = false;
	}

	private void Update()
	{
		if (this.armed && Input.GetKeyDown(KeyCode.Space))
		{
			foreach (ParticleSystem particleSystem in this.effectsToPlay)
			{
				if (particleSystem.isPlaying)
				{
					particleSystem.Clear();
					particleSystem.Play();
				}
				else
				{
					particleSystem.Play();
				}
			}
		}
	}

	public ParticleSystem[] effectsToPlay;

	public bool armed;
}
