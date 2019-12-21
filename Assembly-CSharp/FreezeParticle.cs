using System;
using System.Collections.Generic;
using UnityEngine;

public class FreezeParticle : MonoBehaviour
{
	private void Start()
	{
		ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		this.m_Particles = new List<ParticleSystem>(componentsInChildren);
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			this.m_Particles[i].gameObject.SetActive(true);
		}
		base.Invoke("Freeze", 2f);
	}

	private void Freeze()
	{
		for (int i = 0; i < this.m_Particles.Count; i++)
		{
			this.m_Particles[i].Pause(true);
		}
	}

	private List<ParticleSystem> m_Particles;
}
