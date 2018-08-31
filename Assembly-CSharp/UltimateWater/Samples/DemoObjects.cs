using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Samples
{
	public class DemoObjects : MonoBehaviour
	{
		private void Awake()
		{
			this._Center = base.transform.position;
			base.Invoke("Disable", this._DisableTime);
		}

		private void FixedUpdate()
		{
			switch (this._Type)
			{
			case DemoObjects.Type.FishingRod:
				this.FishingRod();
				break;
			case DemoObjects.Type.Boat:
				this.Boat();
				break;
			case DemoObjects.Type.UserInput:
				this.UserInput();
				break;
			case DemoObjects.Type.Generator:
				this.Generator();
				break;
			}
		}

		private void Disable()
		{
			base.enabled = false;
		}

		private void FishingRod()
		{
			this._Angle += this._Speed * Time.deltaTime;
			base.transform.position = this._Center + this._Radius * new Vector3(Mathf.Cos(this._Angle), 0f, Mathf.Sin(this._Angle));
			base.transform.eulerAngles = new Vector3(0f, -this._Angle * 57.29578f, 0f);
		}

		private void Boat()
		{
			base.transform.position += base.transform.forward * this._Speed * Time.deltaTime;
		}

		private void UserInput()
		{
			float d = Input.GetAxis("Horizontal") * this._Speed * Time.deltaTime;
			float d2 = Input.GetAxis("Vertical") * this._Speed * Time.deltaTime;
			base.transform.position += base.transform.forward * d2;
			base.transform.position += base.transform.right * d;
			if (Input.GetKey(KeyCode.Q))
			{
				base.transform.position -= base.transform.up * this._Speed * Time.deltaTime * 0.33f;
			}
			if (Input.GetKey(KeyCode.E))
			{
				base.transform.position += base.transform.up * this._Speed * Time.deltaTime * 0.33f;
			}
		}

		private void Generator()
		{
			List<WaterForce.Data> data = new List<WaterForce.Data>
			{
				new WaterForce.Data
				{
					Position = base.transform.position,
					Force = this._Force * Mathf.Cos(Time.timeSinceLevelLoad * this._Speed)
				}
			};
			WaterRipples.AddForce(data, this._Radius);
		}

		[SerializeField]
		private float _Radius = 1f;

		[SerializeField]
		private float _Speed = 1f;

		[SerializeField]
		private Vector3 _Center;

		[SerializeField]
		private DemoObjects.Type _Type;

		[SerializeField]
		private float _Force;

		[SerializeField]
		private float _DisableTime = 1024f;

		private float _Angle;

		private enum Type
		{
			FishingRod,
			Boat,
			UserInput,
			Generator
		}
	}
}
