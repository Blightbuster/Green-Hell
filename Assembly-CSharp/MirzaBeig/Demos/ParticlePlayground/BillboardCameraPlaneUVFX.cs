using System;
using UnityEngine;

namespace MirzaBeig.Demos.ParticlePlayground
{
	public class BillboardCameraPlaneUVFX : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
			this.cameraTransform = Camera.main.transform;
		}

		private void Update()
		{
		}

		private void LateUpdate()
		{
			base.transform.forward = -this.cameraTransform.forward;
		}

		private Transform cameraTransform;
	}
}
