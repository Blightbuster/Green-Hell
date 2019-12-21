using System;
using UnityEngine;

public class P2PPeer
{
	public bool IsValid()
	{
		return this.m_HostId != P2PPeer.s_InvalidId;
	}

	public short GetHostId()
	{
		return this.m_HostId;
	}

	public void SetHostId(short id)
	{
		this.m_HostId = id;
	}

	public short GetLocalHostId()
	{
		return this.m_LocalHostId;
	}

	public void SetLocalHostId(short id)
	{
		this.m_LocalHostId = id;
	}

	public P2PPeer()
	{
	}

	public P2PPeer(short id)
	{
		this.m_HostId = id;
	}

	public void SetFrom(P2PPeer other)
	{
		this.m_HostId = other.GetHostId();
		this.m_Address = other.m_Address;
	}

	public bool IsSameId(P2PPeer other)
	{
		return this.m_HostId == other.m_HostId;
	}

	public bool IsSameAddress(P2PPeer other)
	{
		return this.m_Address.Equals(other.m_Address);
	}

	public bool IsSameAddress(IP2PAddress addr)
	{
		return this.m_Address.Equals(addr);
	}

	public bool IsLocalPeer()
	{
		return (this.IsValid() || ReplTools.IsPlayingAlone()) && P2PSession.Instance.LocalPeer.IsSameId(this);
	}

	public string GetDisplayName()
	{
		return P2PTransportLayer.Instance.GetPlayerDisplayName(this.m_Address);
	}

	public static readonly short s_InvalidId = -1;

	public static readonly P2PPeer s_Invalid = new P2PPeer(P2PPeer.s_InvalidId);

	[SerializeField]
	private short m_HostId = P2PPeer.s_InvalidId;

	private short m_LocalHostId = P2PPeer.s_InvalidId;

	public IP2PAddress m_Address;

	public IPeerWorldRepresentation m_Representation;
}
