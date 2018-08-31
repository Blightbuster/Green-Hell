using System;
using UnityEngine;

namespace TFHC_ForceShield_Shader_Sample
{
	public class ForceShieldImpactDetection : MonoBehaviour
	{
		private void Start()
		{
			this.mat = base.GetComponent<Renderer>().material;
		}

		private void Update()
		{
			if (this.hitTime > 0f)
			{
				this.hitTime -= Time.deltaTime * 1000f;
				if (this.hitTime < 0f)
				{
					this.hitTime = 0f;
				}
				this.mat.SetFloat("_HitTime", this.hitTime);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			foreach (ContactPoint contactPoint in collision.contacts)
			{
				this.mat.SetVector("_HitPosition", base.transform.InverseTransformPoint(contactPoint.point));
				this.hitTime = 500f;
				this.mat.SetFloat("_HitTime", this.hitTime);
			}
		}

		private float hitTime;

		private Material mat;
	}
}
