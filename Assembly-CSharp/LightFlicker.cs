using System;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	private void Start()
	{
		this.initialTime = UnityEngine.Random.value * 100f;
		this.lightRef = base.GetComponent<Light>();
		if (this.lightRef)
		{
			this.initialValue = this.lightRef.intensity;
		}
		if (this.scaleObject == null)
		{
			this.scaleObject = base.transform;
		}
		this.initialPosition = base.transform.position;
		this.initialScale = this.scaleObject.localScale;
	}

	private void Update()
	{
		float num = Mathf.PerlinNoise(Time.time * this.speed, this.initialTime);
		if (this.lightRef)
		{
			this.lightRef.intensity = this.initialValue + num * this.amount;
		}
		if (this.adjustLocation)
		{
			Vector3 a = new Vector3(Mathf.PerlinNoise(Time.time * this.speed, this.initialTime + 5f) - 0.5f, num - 0.5f, Mathf.PerlinNoise(Time.time * this.speed, this.initialTime + 10f) - 0.5f);
			base.transform.position = this.initialPosition + a * this.locationAdjustAmount * 2f;
		}
		if (this.adjustScale)
		{
			this.scaleObject.localScale = this.initialScale * ((num - 0.5f) * this.scaleAdjustAmount + 1f);
		}
	}

	private float initialValue;

	private Vector3 initialPosition;

	private Vector3 initialScale;

	private float initialTime;

	private Light lightRef;

	private float amount = 0.01f;

	private float speed = 8f;

	private bool adjustLocation;

	private float locationAdjustAmount = 1f;

	private bool adjustScale;

	private float scaleAdjustAmount = 1f;

	private Transform scaleObject;
}
