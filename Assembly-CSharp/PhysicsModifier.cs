using System;
using UnityEngine;

public class PhysicsModifier : MonoBehaviour
{
	private void FixedUpdate()
	{
		if (this.m_RigidBody == null)
		{
			return;
		}
		Vector3 direction = this.m_RigidBody.transform.InverseTransformDirection(this.m_RigidBody.angularVelocity);
		Vector3 zero = Vector3.zero;
		bool flag = false;
		if (direction.x > this.m_MaxAngularVelocity.x)
		{
			zero.x = direction.x - this.m_MaxAngularVelocity.x;
			direction.x = this.m_MaxAngularVelocity.x;
			flag = true;
		}
		if (direction.y > this.m_MaxAngularVelocity.y)
		{
			zero.y = direction.y - this.m_MaxAngularVelocity.y;
			direction.y = this.m_MaxAngularVelocity.y;
			flag = true;
		}
		if (direction.z > this.m_MaxAngularVelocity.z)
		{
			zero.z = direction.z - this.m_MaxAngularVelocity.z;
			direction.z = this.m_MaxAngularVelocity.z;
			flag = true;
		}
		if (flag)
		{
			this.m_RigidBody.angularVelocity = this.m_RigidBody.transform.TransformDirection(direction);
		}
	}

	public Rigidbody m_RigidBody;

	public Vector3 m_MaxAngularVelocity = new Vector3(7f, 7f, 7f);

	public Vector3 m_TorqueMul = new Vector3(1f, 1f, 1f);
}
