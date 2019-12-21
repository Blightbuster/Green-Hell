using System;
using CJTools;
using UnityEngine;

[DisallowMultipleComponent]
public class AttachmentSynchronizer : ReplicatedBehaviour
{
	private void Awake()
	{
		this.m_Holder = (base.transform.Find(this.m_HolderName) ?? base.gameObject.transform);
		this.m_BoxCollider = base.GetComponent<BoxCollider>();
		this.m_Rigidbody = base.GetComponent<Rigidbody>();
		this.m_Item = base.GetComponent<Item>();
	}

	public void Attach(GameObject parent, string attachment_name)
	{
	}

	private void AttachTransforms(Transform parent_transform)
	{
		bool flag = parent_transform != base.transform.parent;
		Quaternion rhs = Quaternion.Inverse(this.m_Holder.localRotation);
		base.gameObject.transform.rotation = parent_transform.rotation;
		if (this.m_Holder != base.transform)
		{
			base.gameObject.transform.rotation *= rhs;
		}
		Vector3 b = this.m_Holder.parent ? (this.m_Holder.parent.position - this.m_Holder.position) : Vector3.zero;
		base.gameObject.transform.position = parent_transform.position;
		base.gameObject.transform.localPosition += b;
		base.gameObject.transform.parent = parent_transform;
		if (flag)
		{
			this.ConfigurePhysics();
		}
	}

	public void Detach()
	{
		base.gameObject.transform.parent = null;
		this.m_ReplParent = null;
		this.m_AttachmentName = string.Empty;
		if (!this.ReplIsBeingDeserialized(false))
		{
			this.ReplSetDirty();
		}
		this.ConfigurePhysics();
	}

	private void ConfigurePhysics()
	{
		bool flag = base.transform.parent == null;
		if (this.m_BoxCollider)
		{
			this.m_BoxCollider.isTrigger = !flag;
		}
		if (this.m_Rigidbody)
		{
			this.m_Rigidbody.isKinematic = !flag;
		}
		if (this.m_Item)
		{
			if (flag)
			{
				this.m_Item.StaticPhxRequestRemove();
				return;
			}
			this.m_Item.StaticPhxRequestAdd();
		}
	}

	public override void OnReplicationPrepare()
	{
		if (this.m_ParentIsLocalPlayer && ReplicatedLogicalPlayer.s_LocalLogicalPlayer != null)
		{
			this.m_ReplParent = ReplicatedLogicalPlayer.s_LocalLogicalPlayer.gameObject;
			this.m_ParentIsLocalPlayer = false;
			this.ReplSetDirty();
		}
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initialState)
	{
		writer.Write(this.m_ReplAttachmentIdx);
		writer.Write(this.m_ReplParent);
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initialState)
	{
		this.m_ReplAttachmentIdx = reader.ReadInt32();
		this.m_ReplParent = reader.ReadGameObject();
	}

	public override void OnReplicationResolve()
	{
		if (this.m_ReplParent != this.m_LastParent)
		{
			GameObject replParent = this.m_ReplParent;
			if (this.m_LastParent)
			{
				this.Detach();
			}
			this.m_LastParent = (this.m_ReplParent = replParent);
			if (this.m_ReplParent)
			{
				Transform transform = this.m_ReplParent.transform.FindDeepChild(this.m_ReplAttachmentIdx);
				if (transform)
				{
					this.AttachTransforms(transform);
					return;
				}
				if (P2PLogFilter.logDebug)
				{
					Debug.LogError("Attaching " + base.name + " failed - parent not found!");
				}
			}
		}
	}

	[SerializeField]
	private bool m_AttachLocalObject;

	[SerializeField]
	private string m_HolderName = "Holder";

	private int m_ReplAttachmentIdx = -1;

	private bool m_ParentIsLocalPlayer;

	private string m_AttachmentName;

	private GameObject m_ReplParent;

	private GameObject m_LastParent;

	private Transform m_Holder;

	private BoxCollider m_BoxCollider;

	private Rigidbody m_Rigidbody;

	private Item m_Item;

	private const bool HASH_MODE = true;
}
