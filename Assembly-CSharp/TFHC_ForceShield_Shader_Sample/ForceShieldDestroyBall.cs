using System;
using UnityEngine;

namespace TFHC_ForceShield_Shader_Sample
{
	public class ForceShieldDestroyBall : MonoBehaviour
	{
		private void Start()
		{
			UnityEngine.Object.Destroy(base.gameObject, this.lifetime);
		}

		public float lifetime = 5f;
	}
}
