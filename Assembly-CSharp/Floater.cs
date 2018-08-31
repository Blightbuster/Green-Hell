using System;
using CJTools;
using UltimateWater;
using UnityEngine;

public class Floater
{
	public void Update()
	{
		this.UpdateTransform();
		this.UpdateForces();
	}

	private void UpdateTransform()
	{
		this.m_Rotation = this.m_ObjectRigidBody.transform.rotation;
		this.m_Bounds.center = this.m_ObjectRigidBody.transform.position;
		this.m_Bounds.center = this.m_Bounds.center + -this.m_ObjectRigidBody.transform.right * this.m_LocalPos.x;
		this.m_Bounds.center = this.m_Bounds.center + -this.m_ObjectRigidBody.transform.up * this.m_LocalPos.y;
		this.m_Bounds.center = this.m_Bounds.center + -this.m_ObjectRigidBody.transform.forward * this.m_LocalPos.z;
		DebugRender.DrawBounds(this.m_Bounds, this.m_Rotation, Color.red, 0f);
	}

	private void UpdateForces()
	{
		this.m_InWater = false;
		Collider[] array = Physics.OverlapBox(this.m_Bounds.center, this.m_Bounds.extents, this.m_Rotation);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.GetComponent<Water>() != null)
			{
				float num = array[i].bounds.center.y + array[i].bounds.extents.y;
				float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, num - (this.m_Bounds.center.y - this.m_Bounds.extents.y), 0f, 2f);
				this.m_ObjectRigidBody.AddForceAtPosition(Vector3.up * this.m_Force * proportionalClamp, this.m_Bounds.center);
				this.m_InWater = true;
			}
		}
	}

	public Rigidbody m_ObjectRigidBody;

	public Bounds m_Bounds;

	public Vector3 m_LocalPos = default(Vector3);

	public Quaternion m_Rotation = default(Quaternion);

	public bool m_InWater;

	public float m_Force = 110f;
}
