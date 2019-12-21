using System;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver
{
	private void CreateGuid()
	{
		if (this.serializedGuid == null || this.serializedGuid.Length != GuidComponent.GUID_BYTES_CNT)
		{
			this.guid = Guid.NewGuid();
			this.serializedGuid = this.guid.ToByteArray();
		}
		else if (this.guid == Guid.Empty)
		{
			this.guid = new Guid(this.serializedGuid);
		}
		if (this.guid != Guid.Empty && !GuidManager.Add(this))
		{
			this.serializedGuid = null;
			this.guid = Guid.Empty;
			this.CreateGuid();
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (this.serializedGuid != null && this.serializedGuid.Length == GuidComponent.GUID_BYTES_CNT)
		{
			this.guid = new Guid(this.serializedGuid);
		}
	}

	private void Awake()
	{
		this.CreateGuid();
	}

	public Guid GetGuid()
	{
		if (this.guid == Guid.Empty && this.serializedGuid != null && this.serializedGuid.Length == GuidComponent.GUID_BYTES_CNT)
		{
			this.guid = new Guid(this.serializedGuid);
		}
		return this.guid;
	}

	public void ForceGuid(Guid new_guid)
	{
		if (this.guid.CompareTo(new_guid) == 0)
		{
			return;
		}
		if (base.gameObject.activeSelf)
		{
			Debug.LogError("Object active when assigning guid!");
		}
		this.serializedGuid = new_guid.ToByteArray();
		this.guid = new_guid;
		GuidManager.Add(this);
	}

	public void ForceGuid(byte[] new_guid_bytes)
	{
		if (base.gameObject.activeSelf)
		{
			Debug.LogError("Object active when assigning guid!");
		}
		if (this.serializedGuid == null || this.serializedGuid.Length != GuidComponent.GUID_BYTES_CNT)
		{
			this.serializedGuid = new byte[GuidComponent.GUID_BYTES_CNT];
		}
		new_guid_bytes.CopyTo(this.serializedGuid, 0);
		this.guid = new Guid(new_guid_bytes);
		GuidManager.Add(this);
	}

	public byte[] GetGuidBytes()
	{
		return this.serializedGuid;
	}

	public void OnDestroy()
	{
		GuidManager.Remove(this.guid);
	}

	public static int GUID_BYTES_CNT = 16;

	private Guid guid = Guid.Empty;

	[SerializeField]
	private byte[] serializedGuid;
}
