using System;
using UnityEngine;

public class TOD_Animation : MonoBehaviour
{
	public Vector3 CloudUV { get; set; }

	public Vector3 OffsetUV
	{
		get
		{
			Vector3 point = base.transform.position * 0.0001f;
			return Quaternion.Euler(0f, -base.transform.rotation.eulerAngles.y, 0f) * point;
		}
	}

	protected void Start()
	{
		this.sky = base.GetComponent<TOD_Sky>();
		this.CloudUV = new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
	}

	protected void Update()
	{
		float num = Mathf.Sin(0.0174532924f * this.WindDegrees);
		float num2 = Mathf.Cos(0.0174532924f * this.WindDegrees);
		float num3 = 0.001f * Time.deltaTime;
		float num4 = this.WindSpeed * num3;
		float num5 = this.CloudUV.x;
		float num6 = this.CloudUV.y;
		float num7 = this.CloudUV.z;
		num6 += num3 * 0.1f;
		num5 -= num4 * num;
		num7 -= num4 * num2;
		num5 -= Mathf.Floor(num5);
		num6 -= Mathf.Floor(num6);
		num7 -= Mathf.Floor(num7);
		this.CloudUV = new Vector3(num5, num6, num7);
		this.sky.Components.BillboardTransform.localRotation = Quaternion.Euler(0f, num6 * 360f, 0f);
	}

	[Tooltip("Wind direction in degrees.")]
	public float WindDegrees;

	[Tooltip("Speed of the wind that is acting on the clouds.")]
	public float WindSpeed = 1f;

	private TOD_Sky sky;
}
