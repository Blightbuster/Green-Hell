using System;
using System.Collections.ObjectModel;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public sealed class TransportLayerUNet : ITransportLayer
{
	public ETransporLayerType GetLayerType()
	{
		return ETransporLayerType.UNet;
	}

	public void Init()
	{
		NetworkTransport.Init();
	}

	public void Shutdown()
	{
		NetworkTransport.Shutdown();
	}

	public bool IsStarted()
	{
		return NetworkTransport.IsStarted;
	}

	public bool IsMaster()
	{
		return P2PSession.Instance.AmIMaster();
	}

	public int AddHost(HostTopology topology, ref IP2PAddress address)
	{
		P2PAddressUnet p2PAddressUnet = address as P2PAddressUnet;
		if (p2PAddressUnet == null)
		{
			Debug.LogError(string.Format("Using invalid IP2PAddress type {0}", address.GetType()));
			return -1;
		}
		if (p2PAddressUnet.m_IP.Empty())
		{
			return NetworkTransport.AddHost(topology, p2PAddressUnet.m_Port);
		}
		return NetworkTransport.AddHost(topology, p2PAddressUnet.m_Port, p2PAddressUnet.m_IP);
	}

	public int AddHostWithSimulator(HostTopology topology, int min_timeout, int max_timeout, ref IP2PAddress address)
	{
		P2PAddressUnet p2PAddressUnet = address as P2PAddressUnet;
		if (p2PAddressUnet != null)
		{
			return NetworkTransport.AddHostWithSimulator(topology, min_timeout, max_timeout, p2PAddressUnet.m_Port);
		}
		Debug.LogError(string.Format("Using invalid IP2PAddress type {0}", address.GetType()));
		return -1;
	}

	public bool RemoveHost()
	{
		return NetworkTransport.RemoveHost((int)P2PSession.Instance.LocalPeer.GetLocalHostId());
	}

	public int Connect(IP2PAddress address, out byte error)
	{
		P2PAddressUnet p2PAddressUnet = address as P2PAddressUnet;
		if (p2PAddressUnet != null)
		{
			return NetworkTransport.Connect((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), p2PAddressUnet.m_IP, p2PAddressUnet.m_Port, 0, out error);
		}
		Debug.LogError(string.Format("Using invalid IP2PAddress type {0}", address.GetType()));
		error = 0;
		return -1;
	}

	public int ConnectWithSimulator(IP2PAddress address, out byte error, ConnectionSimulatorConfig config)
	{
		P2PAddressUnet p2PAddressUnet = address as P2PAddressUnet;
		if (p2PAddressUnet != null)
		{
			return NetworkTransport.ConnectWithSimulator((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), p2PAddressUnet.m_IP, p2PAddressUnet.m_Port, 0, out error, config);
		}
		Debug.LogError(string.Format("Using invalid IP2PAddress type {0}", address.GetType()));
		error = 0;
		return -1;
	}

	public void GetConnectionInfo(int connection_id, out IP2PAddress address, out byte error)
	{
		address = new P2PAddressUnet();
		P2PAddressUnet p2PAddressUnet = (P2PAddressUnet)address;
		NetworkID networkID;
		NodeID nodeID;
		NetworkTransport.GetConnectionInfo((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), connection_id, out p2PAddressUnet.m_IP, out p2PAddressUnet.m_Port, out networkID, out nodeID, out error);
	}

	public bool Disconnect(int connection_id, out byte error)
	{
		return NetworkTransport.Disconnect((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), connection_id, out error);
	}

	public bool Send(int connection_id, int channel_id, byte[] bytes, int num_bytes, out byte error)
	{
		return NetworkTransport.Send((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), connection_id, channel_id, bytes, num_bytes, out error);
	}

	public NetworkEventType Receive(out int connection_id, out int channel_id, byte[] buffer, int buffer_size, out int received_size, out byte error)
	{
		return NetworkTransport.ReceiveFromHost((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), out connection_id, out channel_id, buffer, buffer_size, out received_size, out error);
	}

	public void Update()
	{
	}

	public static void SetPacketStat(int direction, int packetStat_id, int num_msgs, int num_bytes)
	{
		NetworkTransport.SetPacketStat(direction, packetStat_id, num_msgs, num_bytes);
	}

	public static int ConnectEndPoint(int hostId, EndPoint end_point, int exception_connection_id, out byte error)
	{
		return NetworkTransport.ConnectEndPoint((int)P2PSession.Instance.LocalPeer.GetLocalHostId(), end_point, exception_connection_id, out error);
	}

	public void RequestLobbyList(P2PGameVisibility visibility)
	{
	}

	public void SetGameVisibility(P2PGameVisibility visibility)
	{
	}

	public P2PGameVisibility GetGameVisibility()
	{
		return P2PGameVisibility.Public;
	}

	public ReadOnlyCollection<P2PLobbyMemberInfo> GetCurrentLobbyMembers()
	{
		return null;
	}

	public string GetPlayerDisplayName(IP2PAddress address)
	{
		return string.Empty;
	}

	public void SetLobbyData(string key, string data)
	{
	}

	public string GetLobbyData(string key)
	{
		return "";
	}
}
