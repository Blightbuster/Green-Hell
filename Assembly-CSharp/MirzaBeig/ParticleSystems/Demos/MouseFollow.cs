using System;
using UnityEngine;

namespace MirzaBeig.ParticleSystems.Demos
{
	public class MouseFollow : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = this.distanceFromCamera;
			Vector3 b = Camera.main.ScreenToWorldPoint(mousePosition);
			float num = this.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			Vector3 position = Vector3.Lerp(base.transform.position, b, 1f - Mathf.Exp(-this.speed * num));
			base.transform.position = position;
		}

		private void LateUpdate()
		{
		}

		public float speed = 8f;

		public float distanceFromCamera = 5f;

		public bool ignoreTimeScale;
	}
}
