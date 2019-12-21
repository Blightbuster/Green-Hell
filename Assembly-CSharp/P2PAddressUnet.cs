using System;

public class P2PAddressUnet : IP2PAddress, IEquatable<IP2PAddress>
{
	public bool Equals(IP2PAddress other)
	{
		P2PAddressUnet p2PAddressUnet = other as P2PAddressUnet;
		return p2PAddressUnet != null && this.m_IP.CompareTo(p2PAddressUnet.m_IP) == 0 && this.m_Port == p2PAddressUnet.m_Port;
	}

	public override string ToString()
	{
		return string.Format("{0}/{1}", this.m_IP, this.m_Port);
	}

	public string m_IP;

	public int m_Port;
}
