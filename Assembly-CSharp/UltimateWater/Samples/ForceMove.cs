using System;
using UnityEngine;

namespace UltimateWater.Samples
{
	public class ForceMove : MonoBehaviour
	{
		private void FixedUpdate()
		{
			this._Rigidbody.AddForce(base.transform.forward * this._Force);
		}

		[SerializeField]
		private Rigidbody _Rigidbody;

		[SerializeField]
		private float _Force;
	}
}
