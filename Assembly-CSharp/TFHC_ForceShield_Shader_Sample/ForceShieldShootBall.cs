using System;
using UnityEngine;

namespace TFHC_ForceShield_Shader_Sample
{
	public class ForceShieldShootBall : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetButtonDown("Fire1"))
			{
				Vector3 vector = new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.distance);
				vector = Camera.main.ScreenToWorldPoint(vector);
				Rigidbody rigidbody = UnityEngine.Object.Instantiate<Rigidbody>(this.bullet, base.transform.position, Quaternion.identity);
				rigidbody.transform.LookAt(vector);
				rigidbody.AddForce(rigidbody.transform.forward * this.speed);
			}
		}

		public Rigidbody bullet;

		public Transform origshoot;

		public float speed = 1000f;

		private float distance = 10f;
	}
}
