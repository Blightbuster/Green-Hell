using System;
using AIs;
using UnityEngine;

public class RagdollBone : MonoBehaviour, ITriggerThrough
{
	private void Awake()
	{
		this.m_AIParent = base.gameObject.GetComponentInParent<AI>();
		this.m_ParentObject = this.m_AIParent.gameObject;
		this.m_Rigidbody = base.gameObject.GetComponent<Rigidbody>();
		this.m_Rigidbody.isKinematic = true;
		this.m_Rigidbody.detectCollisions = false;
		this.m_Collider = base.gameObject.GetComponent<Collider>();
		this.m_Collider.enabled = false;
		this.m_Collider.isTrigger = false;
		this.m_AIParent.m_RagdollBones.Add(this);
	}

	public float GetDamageMultiplier(bool thrown_damage = false)
	{
		switch (this.m_BoneType)
		{
		case RagdollBone.BoneType.Animal_Head:
			return 1.5f;
		case RagdollBone.BoneType.Animal_Spine:
			return 1f;
		case RagdollBone.BoneType.Animal_Leg:
			return 0.7f;
		case RagdollBone.BoneType.Human_Head:
			return 1.5f;
		case RagdollBone.BoneType.Human_Arm:
			return 0.85f;
		case RagdollBone.BoneType.Human_Forearm:
			return 0.7f;
		case RagdollBone.BoneType.Human_Spine:
			return 1f;
		case RagdollBone.BoneType.Human_Thigh:
			return 0.85f;
		case RagdollBone.BoneType.Human_Calf:
			return 0.7f;
		default:
			return 1f;
		}
	}

	public RagdollBone.BoneType m_BoneType;

	[HideInInspector]
	public GameObject m_ParentObject;

	private AI m_AIParent;

	[HideInInspector]
	public Collider m_Collider;

	[HideInInspector]
	public Rigidbody m_Rigidbody;

	public enum BoneType
	{
		None,
		Animal_Head,
		Animal_Spine,
		Animal_Leg,
		Human_Head,
		Human_Arm,
		Human_Forearm,
		Human_Spine,
		Human_Thigh,
		Human_Calf
	}
}
