using System;
using UnityEngine;

public class LivingParticleController : MonoBehaviour
{
	private void Start()
	{
		this.psr = base.GetComponent<ParticleSystemRenderer>();
		if (this.m_Player)
		{
			this.affector = Player.Get().transform;
		}
	}

	private void Update()
	{
		this.psr.material.SetVector("_Affector", this.affector.position);
	}

	public Transform affector;

	private ParticleSystemRenderer psr;

	public bool m_Player;
}
