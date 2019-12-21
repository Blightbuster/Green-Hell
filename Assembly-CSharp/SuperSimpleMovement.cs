using System;
using UnityEngine;

public class SuperSimpleMovement : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * this.movementSpeed, 0f, Input.GetAxis("Vertical") * Time.deltaTime * this.movementSpeed);
	}

	public float movementSpeed = 3f;
}
