using System;
using UnityEngine;

[RequireComponent(typeof(CharacterMotorCS))]
public class FPSInputControllerCS : MonoBehaviour
{
	private void Awake()
	{
		this.motor = base.GetComponent<CharacterMotorCS>();
	}

	private void Update()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (vector != Vector3.zero)
		{
			float num = vector.magnitude;
			vector /= num;
			num = Mathf.Min(1f, num);
			num *= num;
			vector *= num;
		}
		this.motor.inputMoveDirection = base.transform.rotation * vector;
		this.motor.inputJump = Input.GetButton("Jump");
	}

	private CharacterMotorCS motor;
}
