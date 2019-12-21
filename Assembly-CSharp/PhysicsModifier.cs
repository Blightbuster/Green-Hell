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
		Vector3 vector = this.m_RigidBody.transform.InverseTransformDirection(this.m_RigidBody.angularVelocity);
		Vector3 zero = Vector3.zero;
		bool flag = false;
		if (vector.x > this.m_MaxAngularVelocity.x)
		{
			zero.x = vector.x - this.m_MaxAngularVelocity.x;
			vector.x = this.m_MaxAngularVelocity.x;
			flag = true;
		}
		if (vector.y > this.m_MaxAngularVelocity.y)
		{
			zero.y = vector.y - this.m_MaxAngularVelocity.y;
			vector.y = this.m_MaxAngularVelocity.y;
			flag = true;
		}
		if (vector.z > this.m_MaxAngularVelocity.z)
		{
			zero.z = vector.z - this.m_MaxAngularVelocity.z;
			vector.z = this.m_MaxAngularVelocity.z;
			flag = true;
		}
		if (flag)
		{
			this.m_RigidBody.angularVelocity = this.m_RigidBody.transform.TransformDirection(vector);
		}
	}

	public Rigidbody m_RigidBody;

	public Vector3 m_MaxAngularVelocity = new Vector3(7f, 7f, 7f);

	public Vector3 m_TorqueMul = new Vector3(1f, 1f, 1f);
}
