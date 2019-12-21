using System;
using Steamworks;

public class P2PAddressSteam : IP2PAddress, IEquatable<IP2PAddress>
{
	public bool Equals(IP2PAddress other)
	{
		P2PAddressSteam p2PAddressSteam = other as P2PAddressSteam;
		return p2PAddressSteam != null && this.m_SteamID == p2PAddressSteam.m_SteamID;
	}

	public override string ToString()
	{
		return this.m_SteamID.ToString();
	}

	public CSteamID m_SteamID;
}
