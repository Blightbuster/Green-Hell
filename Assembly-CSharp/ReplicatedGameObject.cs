using System;
using System.Collections.Generic;
using UnityEngine;

public struct ReplicatedGameObject : IEquatable<GameObject>
{
	public ReplicatedGameObject(byte[] bytes)
	{
		this.m_IsNull = true;
		this.m_Resolved = false;
		this.m_ResolvedObj = null;
		if (bytes != null)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] != 0)
				{
					this.m_IsNull = false;
					break;
				}
			}
		}
		this.m_GameObjectBytes = bytes;
	}

	public ReplicatedGameObject(GameObject obj)
	{
		this.m_IsNull = true;
		this.m_Resolved = true;
		this.m_ResolvedObj = obj;
		byte[] gameObjectBytes;
		if (obj == null)
		{
			gameObjectBytes = null;
		}
		else
		{
			GuidComponent component = obj.GetComponent<GuidComponent>();
			gameObjectBytes = ((component != null) ? component.GetGuidBytes() : null);
		}
		this.m_GameObjectBytes = gameObjectBytes;
		if (this.m_GameObjectBytes != null)
		{
			for (int i = 0; i < this.m_GameObjectBytes.Length; i++)
			{
				if (this.m_GameObjectBytes[i] != 0)
				{
					this.m_IsNull = false;
					return;
				}
			}
		}
	}

	public GameObject ResolveGameObject()
	{
		if (this.IsNull())
		{
			return null;
		}
		if (this.m_Resolved)
		{
			return this.m_ResolvedObj;
		}
		this.m_ResolvedObj = GuidManager.ResolveGuid(this.m_GameObjectBytes);
		if (this.m_ResolvedObj != null)
		{
			this.m_Resolved = true;
		}
		return this.m_ResolvedObj;
	}

	public bool IsNull()
	{
		return this.m_IsNull || this.m_GameObjectBytes == null;
	}

	public static explicit operator ReplicatedGameObject(GameObject other)
	{
		byte[] bytes;
		if (other == null)
		{
			bytes = null;
		}
		else
		{
			GuidComponent component = other.GetComponent<GuidComponent>();
			bytes = ((component != null) ? component.GetGuidBytes() : null);
		}
		return new ReplicatedGameObject(bytes);
	}

	public bool Equals(GameObject other)
	{
		if (other == null)
		{
			return this.IsNull();
		}
		GameObject x = this.ResolveGameObject();
		return !(x == null) && x == other;
	}

	public static bool operator ==(ReplicatedGameObject o1, ReplicatedGameObject o2)
	{
		if (o1.IsNull() != o2.IsNull())
		{
			return false;
		}
		if (o1.IsNull())
		{
			return true;
		}
		GameObject gameObject = o1.ResolveGameObject();
		GameObject gameObject2 = o2.ResolveGameObject();
		if (gameObject == null && gameObject2 == null)
		{
			return EqualityComparer<byte[]>.Default.Equals(o1.m_GameObjectBytes, o2.m_GameObjectBytes);
		}
		return gameObject == gameObject2;
	}

	public static bool operator !=(ReplicatedGameObject o1, ReplicatedGameObject o2)
	{
		return !(o1 == o2);
	}

	public override bool Equals(object other)
	{
		GameObject gameObject = other as GameObject;
		return !(gameObject == null) && this.ResolveGameObject() == gameObject;
	}

	public readonly byte[] m_GameObjectBytes;

	private bool m_IsNull;

	private bool m_Resolved;

	private GameObject m_ResolvedObj;
}
