using System;
using UnityEngine;

public class LivingParticleArrayController : MonoBehaviour
{
	private void Start()
	{
		this.psr = base.GetComponent<ParticleSystemRenderer>();
		Vector4[] values = new Vector4[20];
		this.psr.material.SetVectorArray("_Affectors", values);
	}

	private void Update()
	{
		this.positions = new Vector4[this.affectors.Length];
		for (int i = 0; i < this.positions.Length; i++)
		{
			this.positions[i] = this.affectors[i].position;
		}
		this.psr.material.SetVectorArray("_Affectors", this.positions);
		this.psr.material.SetInt("_AffectorCount", this.affectors.Length);
	}

	public Transform[] affectors;

	private Vector4[] positions;

	private ParticleSystemRenderer psr;
}
