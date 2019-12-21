using System;
using UnityEngine;

namespace MirzaBeig.Demos
{
	public class MouseRotateCamera : MonoBehaviour
	{
		private void Awake()
		{
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void LateUpdate()
		{
			Vector2 vector = Input.mousePosition;
			float num = (float)Screen.width / 2f;
			float num2 = (float)Screen.height / 2f;
			float num3 = (vector.x - num) / num;
			float num4 = (vector.y - num2) / num2;
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.y = num3 * -this.maxRotation;
			localEulerAngles.x = num4 * this.maxRotation;
			float t = ((!this.unscaledTime) ? Time.deltaTime : Time.unscaledDeltaTime) * this.speed;
			localEulerAngles.x = Mathf.LerpAngle(base.transform.localEulerAngles.x, localEulerAngles.x, t);
			localEulerAngles.y = Mathf.LerpAngle(base.transform.localEulerAngles.y, localEulerAngles.y, t);
			base.transform.localEulerAngles = localEulerAngles;
		}

		public float maxRotation = 5f;

		public float speed = 2f;

		public bool unscaledTime;
	}
}
