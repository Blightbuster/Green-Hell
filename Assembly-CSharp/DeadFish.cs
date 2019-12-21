using System;
using AIs;
using CJTools;
using Enums;
using UnityEngine;

public class DeadFish : MonoBehaviour
{
	private void Start()
	{
		ParticlesManager.Get().Spawn("Blood Effect", this.m_HitPos, Quaternion.identity, Vector3.zero, null, -1f, false);
		this.m_Item.StaticPhxRequestAdd();
		this.m_Item.UpdatePhx();
		this.m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
		if (this.m_KillItem)
		{
			this.m_KillItem.StaticPhxRequestAdd();
			this.m_KillItem.UpdatePhx();
			base.transform.rotation = this.m_KillItem.m_DamagerStart.rotation;
			if (this.m_ID == AI.AIID.Arowana)
			{
				base.transform.Rotate(Vector3.forward, -60f);
			}
			else if (this.m_ID == AI.AIID.Piranha)
			{
				base.transform.Rotate(Vector3.forward, -90f);
			}
			else if (this.m_ID == AI.AIID.Stingray)
			{
				base.transform.Rotate(Vector3.forward, -210f);
			}
			else
			{
				base.transform.Rotate(Vector3.forward, 0f);
			}
			base.transform.parent = this.m_KillItem.transform;
			this.m_LocalRot = base.transform.localRotation;
			base.transform.parent = null;
			Vector3 b = (this.m_KillItem.m_DamagerStart.position - this.m_KillItem.m_DamagerEnd.position).normalized * -0.07f;
			base.transform.position = this.m_KillItem.m_DamagerStart.position + b;
		}
		this.m_WaterBoxCollider = this.m_Tank.GetComponent<BoxCollider>();
	}

	private void Update()
	{
		if (this.m_KillItem)
		{
			if (this.m_KillItem.m_InInventory || Player.Get().GetCurrentItem(Hand.Right) == this.m_KillItem || Inventory3DManager.Get().m_CarriedItem == this.m_KillItem)
			{
				this.m_Item.Take();
				Animator componentDeepChild = General.GetComponentDeepChild<Animator>(this.m_Item.gameObject);
				if (componentDeepChild != null)
				{
					componentDeepChild.SetBool("Backpack", true);
				}
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (this.m_KillItemSpeed >= 0.5f)
			{
				this.m_KillItemSpeed -= this.m_KillItemSpeed * Time.deltaTime * 0.5f;
				if (this.m_KillItemSpeed <= 0.5f)
				{
					return;
				}
				Vector3 normalized = (this.m_KillItem.m_DamagerStart.position - this.m_KillItem.m_DamagerEnd.position).normalized;
				RaycastHit raycastHit;
				if (Physics.Raycast(this.m_KillItem.m_DamagerStart.position, normalized, out raycastHit, this.m_KillItemSpeed * Time.deltaTime))
				{
					this.m_KillItemSpeed = 0f;
				}
				else
				{
					this.m_KillItem.transform.position += normalized * this.m_KillItemSpeed * Time.deltaTime;
				}
			}
			else
			{
				Quaternion b = Quaternion.LookRotation(this.m_KillItem.transform.forward.GetNormalized2D(), (this.m_KillItem.transform.up.y > 0f) ? Vector3.up : (-Vector3.up));
				this.m_KillItem.transform.rotation = Quaternion.Slerp(this.m_KillItem.transform.rotation, b, Time.deltaTime * 0.5f);
				if (this.m_WaterBoxCollider && this.m_KillItem.transform.position.y >= this.m_WaterBoxCollider.bounds.max.y)
				{
					Vector3 position = this.m_KillItem.transform.position;
					position.y = this.m_WaterBoxCollider.bounds.max.y;
					this.m_KillItem.transform.position = position;
				}
				else
				{
					this.m_KillItem.transform.position += Vector3.up * Time.deltaTime * 0.2f;
				}
			}
			if (this.m_Item.m_InInventory || Inventory3DManager.Get().m_CarriedItem == this.m_Item)
			{
				this.m_Item.StaticPhxRequestRemove();
				UnityEngine.Object.Destroy(this);
				this.m_KillItem.Take();
				return;
			}
			base.transform.parent = this.m_KillItem.transform;
			base.transform.localRotation = this.m_LocalRot;
			base.transform.parent = null;
			Vector3 b2 = (this.m_KillItem.m_DamagerStart.position - this.m_KillItem.m_DamagerEnd.position).normalized * -0.07f;
			base.transform.position = this.m_KillItem.m_DamagerStart.position + b2;
			return;
		}
		else
		{
			Quaternion b3 = Quaternion.LookRotation(base.transform.forward.GetNormalized2D(), (base.transform.up.y > 0f) ? Vector3.left : (-Vector3.left));
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b3, Time.deltaTime * 0.5f);
			if (this.m_WaterBoxCollider && base.transform.position.y >= this.m_WaterBoxCollider.bounds.max.y)
			{
				Vector3 position2 = base.transform.position;
				position2.y = this.m_WaterBoxCollider.bounds.max.y;
				base.transform.position = position2;
				return;
			}
			base.transform.position += Vector3.up * Time.deltaTime * 0.2f;
			return;
		}
	}

	public AI.AIID m_ID = AI.AIID.None;

	public Item m_KillItem;

	private float m_KillItemSpeed = 1f;

	public Vector3 m_HitPos = Vector3.zero;

	public Item m_Item;

	public FishTank m_Tank;

	private BoxCollider m_BoxCollider;

	private BoxCollider m_WaterBoxCollider;

	private Quaternion m_LocalRot = Quaternion.identity;
}
