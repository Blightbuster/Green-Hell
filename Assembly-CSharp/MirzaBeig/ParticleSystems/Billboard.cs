using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems
{
	public class Billboard : MonoBehaviour
	{
		private void LateUpdate()
		{
			base.transform.LookAt(Camera.main.transform.position);
		}
	}
}
