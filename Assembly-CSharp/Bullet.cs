using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	private void Awake()
	{
		this.m_Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		DebugUtils.Assert(this.m_Rigidbody, "[Bullet:Awake] Missing RigidBody component!", true, DebugUtils.AssertType.Info);
		base.enabled = false;
	}

	public void Setup(IBulletParent parent, Vector3 start_vel, bool stick_on_hit = false, Transform damage_start = null, Transform damage_end = null)
	{
		this.m_Parent = parent;
		this.m_Velocity = start_vel;
		this.m_DamageStart = damage_start;
		this.m_DamageEnd = damage_end;
		this.m_StickOnHit = stick_on_hit;
		this.m_Rigidbody.isKinematic = true;
		base.enabled = true;
	}

	private void FixedUpdate()
	{
		float d = -9.81f;
		float num = -20f;
		this.m_Velocity += Vector3.up * d * Time.fixedDeltaTime;
		if (this.m_Velocity.y < num)
		{
			this.m_Velocity.y = num;
		}
		Matrix4x4 m = default(Matrix4x4);
		m.SetColumn(0, this.m_Velocity.normalized);
		m.m03 = base.gameObject.transform.position.x;
		m.SetColumn(1, Vector3.Cross(m.GetColumn(0), Vector3.up));
		m.m13 = base.gameObject.transform.position.y;
		m.SetColumn(2, Vector3.Cross(m.GetColumn(0), m.GetColumn(1)));
		m.m23 = base.gameObject.transform.position.z;
		Quaternion rotation = Quaternion.Slerp(CJTools.Math.QuaternionFromMatrix(m), base.gameObject.transform.rotation, 0.95f);
		base.gameObject.transform.rotation = rotation;
		base.gameObject.transform.position += this.m_Velocity * Time.fixedDeltaTime;
		if (this.CheckHit())
		{
			this.OnHit();
		}
	}

	private void OnHit()
	{
		if (this.m_StickOnHit)
		{
			this.Stick();
		}
		this.m_Velocity = Vector3.zero;
		this.m_Parent.OnHit(this.m_HitObject);
		base.enabled = false;
	}

	private bool CheckHit()
	{
		Vector3 position = this.m_DamageEnd.position;
		Vector3 normalized = this.m_Velocity.normalized;
		Vector3 e = position + normalized * this.m_Velocity.magnitude * Time.fixedDeltaTime;
		RaycastHit raycastHit;
		if (!Physics.Raycast(position, normalized, out raycastHit, this.m_Velocity.magnitude * Time.fixedDeltaTime))
		{
			return false;
		}
		this.m_HitObject = raycastHit.collider.gameObject;
		CJObject componentDeepChild = General.GetComponentDeepChild<CJObject>(raycastHit.collider.gameObject);
		if (!componentDeepChild)
		{
			return true;
		}
		if (componentDeepChild.GetHitCollisionType() == HitCollisionType.Bones)
		{
			Vector3 zero = Vector3.zero;
			List<OBB> colliderBoxes = componentDeepChild.GetColliderBoxes();
			for (int i = 0; i < colliderBoxes.Count; i++)
			{
				if (colliderBoxes[i].IntersectsLine(position, e, ref zero))
				{
					this.GiveDamage(componentDeepChild);
					return true;
				}
			}
			return false;
		}
		this.GiveDamage(componentDeepChild);
		return true;
	}

	private void GiveDamage(CJObject cj_object)
	{
		cj_object.TakeDamage(new DamageInfo
		{
			m_Damage = this.m_Parent.GetDamage(),
			m_Damager = Player.Get().gameObject
		});
	}

	private void Stick()
	{
		float num = UnityEngine.Random.Range(0.2f, 0.4f);
		base.gameObject.transform.position = base.gameObject.transform.position + (this.m_DamageEnd.position - this.m_DamageStart.position) * (1f - num);
		this.m_Rigidbody.Sleep();
	}

	private IBulletParent m_Parent;

	private Rigidbody m_Rigidbody;

	private Vector3 m_Velocity = Vector3.zero;

	private bool m_StickOnHit;

	private GameObject m_HitObject;

	private Transform m_DamageStart;

	private Transform m_DamageEnd;
}
