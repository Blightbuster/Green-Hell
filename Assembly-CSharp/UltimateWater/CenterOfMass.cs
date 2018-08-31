using System;
using UnityEngine;

namespace UltimateWater
{
	public sealed class CenterOfMass : MonoBehaviour
	{
		public void Apply()
		{
			Rigidbody componentInParent = base.GetComponentInParent<Rigidbody>();
			if (componentInParent != null)
			{
				componentInParent.centerOfMass = componentInParent.transform.worldToLocalMatrix.MultiplyPoint3x4(base.transform.position);
			}
		}

		private void OnEnable()
		{
			this.Apply();
		}
	}
}
