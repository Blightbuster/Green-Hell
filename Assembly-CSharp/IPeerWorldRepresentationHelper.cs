using System;

public static class IPeerWorldRepresentationHelper
{
	public static IPeerWorldRepresentation s_LocalNetPlayer { get; private set; }

	public static IPeerWorldRepresentation s_LocalOfflinePlayer { get; private set; }

	public static void RegisterForPeer(this IPeerWorldRepresentation representation, P2PPeer peer)
	{
		if (peer.m_Representation == null)
		{
			peer.m_Representation = representation;
		}
		if (peer == ReplTools.GetLocalPeer())
		{
			if (representation.IsReplicated())
			{
				IPeerWorldRepresentationHelper.s_LocalNetPlayer = representation;
				return;
			}
			IPeerWorldRepresentationHelper.s_LocalOfflinePlayer = representation;
		}
	}

	public static void UnregisterForPeer(this IPeerWorldRepresentation representation, P2PPeer peer)
	{
		if (peer.m_Representation == representation)
		{
			peer.m_Representation = null;
		}
		if (peer == ReplTools.GetLocalPeer())
		{
			if (representation.IsReplicated())
			{
				IPeerWorldRepresentationHelper.s_LocalNetPlayer = null;
				return;
			}
			IPeerWorldRepresentationHelper.s_LocalOfflinePlayer = null;
		}
	}

	public static bool IsLocalPeer(this IPeerWorldRepresentation representation)
	{
		return IPeerWorldRepresentationHelper.s_LocalNetPlayer == representation || IPeerWorldRepresentationHelper.s_LocalOfflinePlayer == representation;
	}
}
