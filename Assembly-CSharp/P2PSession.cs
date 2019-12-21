using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

public class P2PSession
{
	public static HostTopology s_HostTopology
	{
		get
		{
			if (P2PSession.s_HostTopologyInternal == null)
			{
				ConnectionConfig connectionConfig = new ConnectionConfig();
				connectionConfig.AddChannel(QosType.ReliableSequenced);
				connectionConfig.AddChannel(QosType.Unreliable);
				P2PSession.s_HostTopologyInternal = new HostTopology(connectionConfig, P2PSession.MAX_PLAYERS);
			}
			return P2PSession.s_HostTopologyInternal;
		}
	}

	public bool IsAppQuitInProgress()
	{
		return this.m_AppQuitting;
	}

	public P2PPeer LocalPeer
	{
		get
		{
			return this.m_LocalPeer;
		}
	}

	public bool IsValid()
	{
		return this.m_LocalPeer.GetLocalHostId() != -1;
	}

	public bool AmIMaster()
	{
		return this.m_AmIMaster;
	}

	public string GetSessionId()
	{
		if (!this.AmIMaster())
		{
			this.m_SessionId = P2PTransportLayer.Instance.GetLobbyData("SESSION_ID");
		}
		return this.m_SessionId;
	}

	public void SetSessionId(string session_id = null)
	{
		if (session_id != null)
		{
			this.m_SessionId = session_id;
		}
		else if (this.m_SessionId.Length == 0)
		{
			this.m_SessionId = Guid.NewGuid().ToString();
		}
		P2PTransportLayer.Instance.SetLobbyData("SESSION_ID", this.m_SessionId);
	}

	public P2PSession.ESessionStatus Status { get; private set; }

	public P2PSession()
	{
		P2PTransportLayer.OnExternalLobbyJoinRequestEvent += this.JoinLobby;
		this.m_RemotePeers = new ReadOnlyCollection<P2PPeer>(this.m_RemotePeersInternal);
		P2PTransportLayer.CanStartSessionCheckEvent += delegate(P2PTransportLayer.CanStartSessionEventArgs args)
		{
			args.IsSessionReady = (MainLevel.Instance != null && MainLevel.Instance.m_LevelStarted);
		};
		P2PTransportLayer.OnLobbyEnterEvent += this.OnLobbyEnter;
		P2PTransportLayer.Create(P2PSession.s_TransportLayerType);
	}

	public void OnAppQuit()
	{
		this.m_AppQuitting = true;
		P2PTransportLayer.Shutdown();
	}

	public P2PPeer GetPeerById(short id)
	{
		if (this.LocalPeer.GetHostId() == id)
		{
			return this.LocalPeer;
		}
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_Peer.GetHostId() == id)
			{
				return this.m_Connections[i].m_Peer;
			}
		}
		return P2PPeer.s_Invalid;
	}

	public int GetRemotePeerCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_Peer != null)
			{
				num++;
			}
		}
		return num;
	}

	public int GetConnectionCount()
	{
		int num = 0;
		using (List<P2PConnection>.Enumerator enumerator = this.m_Connections.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current != null)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void ForEachPeerReturn<U>(Action<P2PPeer, U> func, U ret) where U : class
	{
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_Peer != null)
			{
				func(this.m_Connections[i].m_Peer, ret);
			}
		}
	}

	public void SetGameVisibility(P2PGameVisibility visibility)
	{
		DebugUtils.Assert(P2PTransportLayer.Instance != null, true);
		if (P2PTransportLayer.Instance != null)
		{
			P2PTransportLayer.Instance.SetGameVisibility(visibility);
		}
	}

	public P2PGameVisibility GetGameVisibility()
	{
		if (P2PTransportLayer.Instance != null)
		{
			return P2PTransportLayer.Instance.GetGameVisibility();
		}
		return P2PGameVisibility.Singleplayer;
	}

	private void RegisterMessageHandlers()
	{
	}

	public void RegisterHandler(short msg_type, P2PNetworkMessageDelegate handler)
	{
		this.m_MessageHandlers.RegisterHandler(msg_type, handler);
	}

	public void UnregisterHandlers(short msg_type)
	{
		this.m_MessageHandlers.UnregisterHandler(msg_type);
	}

	public void ClearHandlers()
	{
		this.m_MessageHandlers.ClearMessageHandlers();
	}

	public void JoinLobby(IP2PAddress address)
	{
	}

	private void OnLobbyEnter(bool is_owner)
	{
		if (is_owner)
		{
			this.SetSessionId(null);
		}
	}

	private void OnConnectInternal(P2PNetworkMessage net_msg)
	{
		this.OnConnectInternal(net_msg.m_Connection);
	}

	private void OnConnectInternal(P2PConnection conn)
	{
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession:OnConnectInternal");
		}
		if (conn.m_Peer.IsValid())
		{
			this.SendPeerInfoOnConnectionEstablished(conn);
		}
		Replicator.Singleton.OnPeerConnected(conn.m_Peer);
	}

	private void OnDisconnectInternal(P2PNetworkMessage net_msg)
	{
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession:OnDisconnectInternal");
		}
		Replicator.Singleton.OnPeerDisconnected(net_msg.m_Connection.m_Peer);
	}

	private void OnReplication(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnReplicationMessage(net_msg);
	}

	private void OnObjectSpawn(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnSpawnMessage(net_msg, false);
	}

	private void ObjectSpawnAndGiveOwnership(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnSpawnMessage(net_msg, true);
	}

	private void OnObjectDespawn(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnDespawnMessage(net_msg);
	}

	private void OnRequestOwnership(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnRequestOwnership(net_msg);
	}

	private void OnGiveOwnership(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnGiveOwnership(net_msg);
	}

	private void OnNeedFullObjectInfo(P2PNetworkMessage net_msg)
	{
		Replicator.Singleton.OnNeedFullObjectInfo(net_msg);
	}

	private void OnTextChat(P2PNetworkMessage net_msg)
	{
		HUDMessages.Get().AddMessage(net_msg.m_Connection.m_Peer.GetDisplayName() + ": " + net_msg.m_Reader.ReadString(), null, HUDMessageIcon.None, "", null);
	}

	private void SendPeerInfoOnConnectionEstablished(P2PConnection conn_established)
	{
	}

	private void OnPeerInfo(P2PNetworkMessage net_msg)
	{
	}

	private void UpdateLocalPeerId(P2PPeer peer)
	{
		if (this.LocalPeer.GetHostId() != peer.GetHostId())
		{
			this.LocalPeer.SetHostId(peer.GetHostId());
		}
	}

	private bool StorePendingPeer(P2PPeer peer)
	{
		bool flag = false;
		if (peer.GetHostId() != P2PPeer.s_InvalidId)
		{
			for (int i = 0; i < this.m_Connections.Count; i++)
			{
				if (this.m_Connections[i] != null)
				{
					P2PPeer peer2 = this.m_Connections[i].m_Peer;
					if (peer2.IsSameAddress(peer))
					{
						bool flag2 = !peer2.IsSameId(peer);
						this.m_Connections[i].m_Peer.SetFrom(peer);
						if (flag2)
						{
							this.OnConnected(this.m_Connections[i]);
						}
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			this.m_PendingPeers.Add(peer);
			return true;
		}
		return false;
	}

	private void StartInternal()
	{
		if (!P2PTransportLayer.Instance.IsStarted())
		{
			this.m_Started = false;
		}
		if (this.m_Started)
		{
			return;
		}
		if (this.m_RunInBackground)
		{
			Application.runInBackground = true;
		}
		this.m_Started = true;
		this.m_AmIMaster = true;
		P2PTransportLayer.Instance.Init();
		this.m_MsgBuffer = new byte[65535];
		this.m_MsgReader = new P2PNetworkReader(this.m_MsgBuffer);
		if (this.LocalPeer.m_Address == null)
		{
			this.LocalPeer.m_Address = P2PAddressCreator.CreateAddress(P2PSession.s_TransportLayerType);
		}
		this.RegisterMessageHandlers();
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession started.");
		}
	}

	public bool Start()
	{
		return this.Start(P2PAddressCreator.CreateAddress(P2PSession.s_TransportLayerType));
	}

	public bool Start(IP2PAddress address)
	{
		this.StartInternal();
		if (P2PTransportLayer.Instance.GetLayerType() == ETransporLayerType.UNet)
		{
			P2PAddressUnet p2PAddressUnet = address as P2PAddressUnet;
			if (p2PAddressUnet.m_IP.Equals("localhost"))
			{
				p2PAddressUnet.m_IP = "127.0.0.1";
			}
			else if (p2PAddressUnet.m_IP.IndexOf(":") != -1 && !P2PSession.IsValidIpV6(p2PAddressUnet.m_IP) && P2PLogFilter.logError)
			{
				Debug.LogError("Invalid ipv6 address " + p2PAddressUnet.m_IP);
			}
		}
		this.SetGameVisibility(GreenHellGame.Instance.m_Settings.m_GameVisibility);
		this.m_LocalPeer.SetLocalHostId((short)P2PTransportLayer.Instance.AddHost(P2PSession.s_HostTopology, ref address));
		if (this.m_LocalPeer.GetLocalHostId() == P2PPeer.s_InvalidId)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("StartServer listen failed.");
			}
			return false;
		}
		this.m_LocalPeer.m_Address = address;
		this.m_LocalPeer.SetHostId(this.m_LocalPeer.GetLocalHostId());
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession listen start with id: " + this.GetSessionId());
		}
		this.Status = P2PSession.ESessionStatus.Listening;
		return true;
	}

	public void Restart()
	{
		if (!this.AmIMaster())
		{
			SaveGame.SaveCoop(this.m_SessionId);
		}
		this.DisconnectAllConnections();
		this.Stop();
		this.Start(null);
	}

	private static bool IsValidIpV6(string address)
	{
		foreach (char c in address)
		{
			if (c != ':' && (c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
			{
				return false;
			}
		}
		return true;
	}

	public void Connect(IP2PAddress address)
	{
		this.m_LocalPeer.SetHostId(P2PPeer.s_InvalidId);
		this.ConnectInternal(address);
	}

	private void ConnectInternal(IP2PAddress address)
	{
		this.StartInternal();
		this.m_AmIMaster = false;
		P2PAddressUnet p2PAddressUnet = address as P2PAddressUnet;
		if (p2PAddressUnet != null)
		{
			if (p2PAddressUnet.m_IP.Equals("localhost"))
			{
				p2PAddressUnet.m_IP = "127.0.0.1";
			}
			else if (p2PAddressUnet.m_IP.IndexOf(":") != -1 && !P2PSession.IsValidIpV6(p2PAddressUnet.m_IP) && P2PLogFilter.logError)
			{
				Debug.LogError("Invalid ipv6 address " + p2PAddressUnet.m_IP);
			}
		}
		int networkConnectionId;
		if (this.m_UseSimulator)
		{
			int num = this.m_SimulatedLatency / 3;
			if (num < 1)
			{
				num = 1;
			}
			if (P2PLogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Connect Using Simulator ",
					this.m_SimulatedLatency / 3,
					"/",
					this.m_SimulatedLatency
				}));
			}
			ConnectionSimulatorConfig config = new ConnectionSimulatorConfig(num, this.m_SimulatedLatency, num, this.m_SimulatedLatency, this.m_PacketLoss);
			byte b;
			networkConnectionId = P2PTransportLayer.Instance.ConnectWithSimulator(address, out b, config);
		}
		else
		{
			byte b;
			networkConnectionId = P2PTransportLayer.Instance.Connect(address, out b);
		}
		P2PConnection p2PConnection = (P2PConnection)Activator.CreateInstance(typeof(P2PConnection));
		p2PConnection.SetHandlers(this.m_MessageHandlers);
		P2PPeer p2PPeer = null;
		for (int i = 0; i < this.m_PendingPeers.Count; i++)
		{
			if (this.m_PendingPeers[i].IsSameAddress(address))
			{
				p2PPeer = this.m_PendingPeers[i];
				this.m_PendingPeers.RemoveAt(i);
				break;
			}
		}
		if (p2PPeer == null)
		{
			p2PPeer = new P2PPeer();
			p2PPeer.m_Address = address;
		}
		if (!this.m_RemotePeersInternal.Contains(p2PPeer))
		{
			this.m_RemotePeersInternal.Add(p2PPeer);
		}
		p2PConnection.Initialize(p2PPeer, networkConnectionId, P2PSession.s_HostTopology);
		this.SetConnectionAtIndex(p2PConnection);
		if (this.Status == P2PSession.ESessionStatus.Idle)
		{
			this.Status = P2PSession.ESessionStatus.Connecting;
		}
	}

	public void Stop()
	{
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession Stop()");
		}
		if (P2PTransportLayer.Instance.IsStarted())
		{
			P2PTransportLayer.Instance.RemoveHost();
			P2PTransportLayer.Shutdown();
		}
		this.m_LocalPeer.SetLocalHostId(P2PPeer.s_InvalidId);
		this.m_AmIMaster = true;
		this.m_RemotePeersInternal.Clear();
		this.Status = P2PSession.ESessionStatus.Idle;
		this.m_SessionId = string.Empty;
	}

	public void Update()
	{
		P2PTransportLayer.Instance.Update();
	}

	private void UpdateConnections()
	{
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			P2PConnection p2PConnection = this.m_Connections[i];
			if (p2PConnection != null)
			{
				p2PConnection.FlushChannels();
			}
		}
	}

	private P2PConnection FindConnection(int connectionId)
	{
		if (connectionId < 0 || connectionId >= this.m_Connections.Count)
		{
			return null;
		}
		return this.m_Connections[connectionId];
	}

	private P2PConnection FindConnection(P2PPeer peer)
	{
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_Peer == peer)
			{
				return this.m_Connections[i];
			}
		}
		return null;
	}

	private bool SetConnectionAtIndex(P2PConnection conn)
	{
		while (this.m_Connections.Count <= conn.m_ConnectionId)
		{
			this.m_Connections.Add(null);
		}
		if (this.m_Connections[conn.m_ConnectionId] != null)
		{
			return false;
		}
		this.m_Connections[conn.m_ConnectionId] = conn;
		conn.SetHandlers(this.m_MessageHandlers);
		return true;
	}

	private bool RemoveConnectionAtIndex(int connectionId)
	{
		if (connectionId < 0 || connectionId >= this.m_Connections.Count)
		{
			return false;
		}
		this.m_Connections[connectionId] = null;
		return true;
	}

	private void HandleConnect(int connection_id, byte error)
	{
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession accepted client:" + connection_id);
		}
		if (error != 0)
		{
			if (this.Status == P2PSession.ESessionStatus.Connecting)
			{
				HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("SessionInfo_ConnectionError", true), null, HUDMessageIcon.None, "", null);
			}
			this.OnConnectError(connection_id, error);
			return;
		}
		P2PSession.ESessionStatus status = this.Status;
		this.Status = P2PSession.ESessionStatus.Connected;
	}

	private void HandleDisconnect(int connectionId, byte error)
	{
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession disconnect client:" + connectionId);
		}
	}

	public P2PPeer GetSessionMaster()
	{
		P2PPeer p2PPeer = this.LocalPeer;
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_Peer.IsValid() && (p2PPeer == null || this.m_Connections[i].m_Peer.GetHostId() < p2PPeer.GetHostId()))
			{
				p2PPeer = this.m_Connections[i].m_Peer;
			}
		}
		return p2PPeer;
	}

	private void CheckMasterOnDisconnection(P2PConnection disconnected_conn)
	{
		short hostId = this.LocalPeer.GetHostId();
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i] != disconnected_conn && this.m_Connections[i].m_Peer.GetHostId() < hostId)
			{
				hostId = this.m_Connections[i].m_Peer.GetHostId();
			}
		}
		if (P2PLogFilter.logDebug)
		{
			Debug.Log("P2PSession new master should be peer id: " + hostId);
		}
		this.m_AmIMaster = (this.LocalPeer.GetHostId() == hostId);
	}

	private void HandleData(int connection_id, int channel_id, int received_size, byte error)
	{
		P2PConnection p2PConnection = this.FindConnection(connection_id);
		if (p2PConnection == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("P2PSession HandleData Unknown connectionId:" + connection_id);
			}
			return;
		}
		if (error != 0)
		{
			this.OnDataError(p2PConnection, error);
			return;
		}
		this.m_MsgReader.SeekZero();
		this.OnData(p2PConnection, received_size, channel_id);
	}

	private void SendBytesTo(int connection_id, byte[] bytes, int num_bytes, int channel_id)
	{
		P2PConnection p2PConnection = this.FindConnection(connection_id);
		if (p2PConnection == null)
		{
			return;
		}
		p2PConnection.SendBytes(bytes, num_bytes, channel_id);
	}

	private void SendWriterTo(int connection_id, P2PNetworkWriter writer, int channel_id)
	{
		P2PConnection p2PConnection = this.FindConnection(connection_id);
		if (p2PConnection == null)
		{
			return;
		}
		p2PConnection.SendWriter(writer, channel_id);
	}

	public void SendWriterTo(P2PPeer peer, P2PNetworkWriter writer, int channel_id)
	{
		P2PConnection p2PConnection = this.FindConnection(peer);
		if (p2PConnection == null || !p2PConnection.m_IsReady)
		{
			if (P2PLogFilter.logError)
			{
				if (p2PConnection == null)
				{
					Debug.LogError("Trying to send data without connection for peer: " + peer.GetHostId());
					return;
				}
				Debug.LogError("Trying to send data on unready connection connection_id:" + p2PConnection.m_ConnectionId);
			}
			return;
		}
		p2PConnection.SendWriter(writer, channel_id);
	}

	public void SendWriterToAll(P2PNetworkWriter writer, int channelId)
	{
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_IsReady)
			{
				this.m_Connections[i].SendWriter(writer, channelId);
			}
		}
	}

	private void SendWriterToAllExcept(int connection_id, P2PNetworkWriter writer, int channel_id)
	{
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			if (this.m_Connections[i] != null && this.m_Connections[i].m_ConnectionId != connection_id)
			{
				this.m_Connections[i].SendWriter(writer, channel_id);
			}
		}
	}

	private void Disconnect(int connection_id)
	{
		P2PConnection p2PConnection = this.FindConnection(connection_id);
		if (p2PConnection == null)
		{
			return;
		}
		p2PConnection.Disconnect();
		this.m_Connections[connection_id] = null;
	}

	public void DisconnectAllConnections()
	{
		this.m_AmIMaster = true;
		this.m_SpawnRefPosition = Vector3.zero;
		for (int i = 0; i < this.m_Connections.Count; i++)
		{
			P2PConnection p2PConnection = this.m_Connections[i];
			if (p2PConnection != null)
			{
				this.m_RemotePeersInternal.Remove(p2PConnection.m_Peer);
				this.OnDisconnected(p2PConnection);
				this.m_Connections[p2PConnection.m_ConnectionId] = null;
				p2PConnection.Disconnect();
				p2PConnection.Dispose();
			}
		}
		this.m_RemotePeersInternal.Clear();
		this.m_PendingPeers.Clear();
	}

	public void SendTextChatMessage(string message)
	{
		P2PNetworkWriter p2PNetworkWriter = new P2PNetworkWriter();
		p2PNetworkWriter.StartMessage(10);
		p2PNetworkWriter.Write(message);
		p2PNetworkWriter.FinishMessage();
		this.SendWriterToAll(p2PNetworkWriter, 1);
	}

	private void OnLeftAloneAsGuest()
	{
		this.Restart();
	}

	public virtual void OnConnectError(int connectionId, byte error)
	{
		Debug.LogError("OnConnectError error:" + error);
	}

	public virtual void OnDataError(P2PConnection conn, byte error)
	{
		Debug.LogError("OnDataError error:" + error);
	}

	public virtual void OnDisconnectError(P2PConnection conn, byte error)
	{
		Debug.LogError("OnDisconnectError error:" + error);
	}

	public virtual void OnConnected(P2PConnection conn)
	{
		conn.m_IsReady = true;
		conn.InvokeHandlerNoData(32);
	}

	public virtual void OnDisconnected(P2PConnection conn)
	{
		conn.InvokeHandlerNoData(33);
	}

	public virtual void OnData(P2PConnection conn, int receivedSize, int channelId)
	{
		conn.TransportReceive(this.m_MsgBuffer, receivedSize, channelId);
	}

	public static int MAX_PLAYERS = 4;

	public static readonly ETransporLayerType s_TransportLayerType = ETransporLayerType.Steam;

	public static readonly P2PSession Instance = new P2PSession();

	private static HostTopology s_HostTopologyInternal;

	private bool m_Started;

	private bool m_RunInBackground = true;

	private byte[] m_MsgBuffer;

	private P2PNetworkReader m_MsgReader;

	private bool m_AppQuitting;

	private List<P2PPeer> m_PendingPeers = new List<P2PPeer>();

	private List<P2PPeer> m_RemotePeersInternal = new List<P2PPeer>();

	public ReadOnlyCollection<P2PPeer> m_RemotePeers;

	private P2PPeer m_LocalPeer = new P2PPeer(-1);

	private bool m_AmIMaster = true;

	public Vector3 m_SpawnRefPosition;

	public bool m_UseSimulator;

	public int m_SimulatedLatency;

	private float m_PacketLoss;

	private const string SESSION_ID = "SESSION_ID";

	public string m_SessionId = string.Empty;

	private List<P2PConnection> m_Connections = new List<P2PConnection>();

	private P2PNetworkMessageHandlers m_MessageHandlers = new P2PNetworkMessageHandlers();

	public enum ESessionStatus
	{
		Idle,
		Listening,
		Connecting,
		Connected
	}
}
