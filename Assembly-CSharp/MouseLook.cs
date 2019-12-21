﻿using System;
using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKey(KeyCode.F))
		{
			return;
		}
		if (this.axes == MouseLook.RotationAxes.MouseXAndY)
		{
			float y = base.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * this.sensitivityX;
			this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
			this.rotationY = Mathf.Clamp(this.rotationY, this.minimumY, this.maximumY);
			base.transform.localEulerAngles = new Vector3(-this.rotationY, y, 0f);
			return;
		}
		if (this.axes == MouseLook.RotationAxes.MouseX)
		{
			base.transform.Rotate(0f, Input.GetAxis("Mouse X") * this.sensitivityX, 0f);
			return;
		}
		this.rotationY += Input.GetAxis("Mouse Y") * this.sensitivityY;
		this.rotationY = Mathf.Clamp(this.rotationY, this.minimumY, this.maximumY);
		base.transform.localEulerAngles = new Vector3(-this.rotationY, base.transform.localEulerAngles.y, 0f);
	}

	private void Start()
	{
		if (base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	public MouseLook.RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityY = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	private float rotationY;

	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}
}
