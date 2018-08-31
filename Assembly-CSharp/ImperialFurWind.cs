using System;
using UnityEngine;

public class ImperialFurWind : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		float max = (this.maxWindForce - this.minWindForce) * 2f;
		float x = this.maxWindForce - UnityEngine.Random.Range(0f, max);
		float y = this.maxWindForce - UnityEngine.Random.Range(0f, max);
		float z = this.maxWindForce - UnityEngine.Random.Range(0f, max);
		ImperialFurWind.windForce = Vector3.Lerp(ImperialFurWind.windForce, new Vector3(x, y, z), Time.deltaTime * this.windDamping);
	}

	public float windDamping = 0.5f;

	public float minWindForce;

	public float maxWindForce = 2f;

	[HideInInspector]
	public static Vector3 windForce = Vector3.zero;
}
