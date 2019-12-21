using System;
using UnityEngine;

public class PanWM : MonoBehaviour
{
	private void Start()
	{
		this.mTrans = base.transform;
		this.mStart = this.mTrans.localRotation;
	}

	private void Update()
	{
		this.t += PanWM.shake_speed * Time.deltaTime;
		this.shake = new Vector2(Mathf.Sin(this.t * 5f) * PanWM.shake_value, Mathf.Sin(this.t * 3f) * PanWM.shake_value);
		float deltaTime = Time.deltaTime;
		Vector3 mousePosition = Input.mousePosition;
		float num = (float)Screen.width * 0.5f;
		float num2 = (float)Screen.height * 0.5f;
		if (this.range < 0.1f)
		{
			this.range = 0.1f;
		}
		float x = Mathf.Clamp((mousePosition.x - num) / num / this.range, -1f, 1f);
		float y = Mathf.Clamp((mousePosition.y - num2) / num2 / this.range, -1f, 1f);
		this.mRot = Vector2.Lerp(this.mRot, new Vector2(x, y), deltaTime * 5f);
		this.mTrans.localRotation = this.mStart * Quaternion.Euler(-this.mRot.y * this.degrees.y + this.shake.y, this.mRot.x * this.degrees.x + this.shake.x, 0f);
		base.transform.eulerAngles = new Vector3(base.transform.eulerAngles.x, base.transform.eulerAngles.y, 0f);
	}

	public Vector2 degrees = new Vector2(5f, 3f);

	private Vector2 shake = new Vector2(5f, 3f);

	public static float shake_value = 0f;

	public static float shake_speed = 10f;

	public float range = 1f;

	private float t;

	private Transform mTrans;

	private Quaternion mStart;

	private Vector2 mRot = Vector2.zero;
}
