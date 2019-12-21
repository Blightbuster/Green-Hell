using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public sealed class TransportLayerSteam : ITransportLayer
{
	public ETransporLayerType GetLayerType()
	{
		return ETransporLayerType.Steam;
	}

	public void Init()
	{
	}

	public void Shutdown()
	{
	}

	public bool IsStarted()
	{
		return this.m_IsInLobby;
	}

	public bool IsMaster()
	{
		return this.m_LobbyOwner != CSteamID.Nil && this.m_LobbyOwner == this.m_LocalSteamID;
	}

	public int AddHost(HostTopology topology, ref IP2PAddress address)
	{
		return 0;
	}

	public int AddHostWithSimulator(HostTopology topology, int min_timeout, int max_timeout, ref IP2PAddress address)
	{
		return this.AddHost(topology, ref address);
	}

	public bool RemoveHost()
	{
		return false;
	}

	private void CheckStartupOptions()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string text = "";
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "+connect_lobby" && commandLineArgs.Length > i + 1)
			{
				text = commandLineArgs[i + 1];
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			ulong value = 0UL;
			if (ulong.TryParse(text, out value))
			{
				P2PTransportLayer.OnExternalLobbyJoinRequest(new P2PAddressSteam
				{
					m_SteamID = (CSteamID)value
				});
			}
		}
	}

	public int Connect(IP2PAddress address, out byte error)
	{
		error = 0;
		return -1;
	}

	public int ConnectWithSimulator(IP2PAddress address, out byte error, ConnectionSimulatorConfig config)
	{
		return this.Connect(address, out error);
	}

	public void GetConnectionInfo(int connection_id, out IP2PAddress address, out byte error)
	{
		CSteamID connectionSteamId = this.GetConnectionSteamId(connection_id);
		address = new P2PAddressSteam
		{
			m_SteamID = connectionSteamId
		};
		error = ((connectionSteamId == CSteamID.Nil) ? 2 : 0);
	}

	private int AssignNewConnectionForUser(CSteamID steam_id)
	{
		int num;
		if (this.m_Connections.TryGetValue(steam_id, out num))
		{
			if (P2PLogFilter.logInfo)
			{
				Debug.Log("[TransportLayerSteam] CSteamID " + steam_id.ToString() + " already was assigned connection id.");
			}
		}
		else
		{
			num = 1;
			while (this.m_Connections.ContainsValue(num))
			{
				num++;
			}
			this.m_Connections[steam_id] = num;
		}
		return num;
	}

	public bool Disconnect(int connection_id, out byte error)
	{
		error = 2;
		return false;
	}

	public bool Send(int connection_id, int channel_id, byte[] bytes, int num_bytes, out byte error)
	{
		error = 2;
		return false;
	}

	public NetworkEventType Receive(out int connection_id, out int channel_id, byte[] buffer, int buffer_size, out int received_size, out byte error)
	{
		error = 4;
		connection_id = -1;
		channel_id = -1;
		received_size = 0;
		return NetworkEventType.Nothing;
	}

	public void Update()
	{
		if (this.m_JoiningDelayed != null && !this.m_JoiningDelayed.GetEnumerator().MoveNext())
		{
			this.m_JoiningDelayed = null;
		}
	}

	private CSteamID GetConnectionSteamId(int connection_id)
	{
		foreach (KeyValuePair<CSteamID, int> keyValuePair in this.m_Connections)
		{
			if (keyValuePair.Value == connection_id)
			{
				return keyValuePair.Key;
			}
		}
		return CSteamID.Nil;
	}

	private void OnP2PSessionRequested(P2PSessionRequest_t callback)
	{
		if (P2PLogFilter.logDev)
		{
			Debug.Log("[TransportLayerSteam] P2P session request received");
		}
		CSteamID steamIDRemote = callback.m_steamIDRemote;
		if (this.IsMemberInSteamLobby(steamIDRemote) && !this.m_JoinRequests.Contains(steamIDRemote))
		{
			if (P2PLogFilter.logDev)
			{
				Debug.Log(string.Format("[TransportLayerSteam] Storing new join request from {0}", steamIDRemote));
			}
			this.m_JoinRequests.Push(steamIDRemote);
		}
	}

	private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback)
	{
		if (P2PLogFilter.logError)
		{
			Debug.LogError(string.Format("[TransportLayerSteam] Connection error: {0} with steam user {1}", (EP2PSessionError)callback.m_eP2PSessionError, callback.m_steamIDRemote));
		}
		if (callback.m_eP2PSessionError != 0)
		{
			SteamNetworking.CloseP2PSessionWithUser(callback.m_steamIDRemote);
			int item;
			if (this.m_Connections.TryGetValue(callback.m_steamIDRemote, out item))
			{
				this.m_ConnectionsBrokenInternal.Push(item);
			}
		}
	}

	public void CreateP2PConnectionWithPeer(CSteamID peer)
	{
		if (!SteamManager.Initialized)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("[TransportLayerSteam] SteamManager not initialized");
			}
			return;
		}
		if (P2PLogFilter.logDev)
		{
			Debug.Log("[TransportLayerSteam] Sending P2P acceptance message to peer: " + peer);
		}
		SteamNetworking.SendP2PPacket(peer, null, 0u, EP2PSend.k_EP2PSendReliable, 0);
	}

	private void OnLobbyEnter(LobbyEnter_t callback)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (this.m_LobbyId != (CSteamID)callback.m_ulSteamIDLobby)
		{
			this.m_LobbyId = new CSteamID(callback.m_ulSteamIDLobby);
		}
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(this.m_LobbyId);
		this.m_LobbyOwner = lobbyOwner;
		CSteamID steamID = SteamUser.GetSteamID();
		if (lobbyOwner.m_SteamID == steamID.m_SteamID)
		{
			if (P2PLogFilter.logInfo)
			{
				Debug.Log("[TransportLayerSteam] Connected to Steam lobby as owner");
			}
			SteamMatchmaking.SetLobbyData(this.m_LobbyId, "game_ver", GreenHellGame.s_GameVersion.ToString());
			SteamMatchmaking.SetLobbyData(this.m_LobbyId, "name", SteamFriends.GetPersonaName());
			SteamMatchmaking.SetLobbyMemberData(this.m_LobbyId, "member_name", SteamFriends.GetPersonaName());
			SteamMatchmaking.SetLobbyData(this.m_LobbyId, "lobby_type", EnumUtils<P2PGameVisibility>.GetName(this.GetGameVisibility()));
			P2PTransportLayer.OnLobbyEnter(true);
			return;
		}
		if (callback.m_EChatRoomEnterResponse == 1u)
		{
			if (P2PLogFilter.logInfo)
			{
				Debug.Log(string.Format("[TransportLayerSteam] Connected to Steam lobby {0} as member", this.m_LobbyId));
			}
			this.m_JoiningDelayed = this.RequestP2PConnectionWithHost();
			SteamMatchmaking.SetLobbyMemberData(this.m_LobbyId, "member_name", SteamFriends.GetPersonaName());
			P2PTransportLayer.OnLobbyEnter(false);
			return;
		}
		if (P2PLogFilter.logWarn)
		{
			string str = "[TransportLayerSteam] Lobby enter failure: ";
			EChatRoomEnterResponse echatRoomEnterResponse = (EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse;
			Debug.LogWarning(str + echatRoomEnterResponse.ToString());
		}
		this.Shutdown();
	}

	private IEnumerable<bool> RequestP2PConnectionWithHost()
	{
		while (!SteamManager.Initialized)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("[TransportLayerSteam] SteamManager not initialized");
			}
			yield return false;
		}
		P2PTransportLayer.CanStartSessionEventArgs canStartSessionEventArgs = new P2PTransportLayer.CanStartSessionEventArgs();
		P2PTransportLayer.CanStartSession(canStartSessionEventArgs);
		if (!canStartSessionEventArgs.IsSessionReady)
		{
			yield return false;
		}
		CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(this.m_LobbyId);
		if (P2PLogFilter.logInfo)
		{
			Debug.Log("[TransportLayerSteam] Requesting P2P connection (sending empty packet)");
		}
		SteamNetworking.SendP2PPacket(lobbyOwner, null, 0u, EP2PSend.k_EP2PSendReliable, 0);
		this.AssignNewConnectionForUser(lobbyOwner);
		yield break;
		yield break;
	}

	private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
	{
		if (P2PLogFilter.logInfo)
		{
			Debug.Log(string.Format("[TransportLayerSteam] GameLobbyJoinRequested - lobby: {0} -- friend_id: {1}", callback.m_steamIDLobby, callback.m_steamIDFriend));
		}
		if (!SteamManager.Initialized)
		{
			return;
		}
		P2PTransportLayer.OnExternalLobbyJoinRequest(new P2PAddressSteam
		{
			m_SteamID = callback.m_steamIDLobby
		});
	}

	private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
	{
		if (callback.m_bSuccess == 1)
		{
			if (this.m_LobbyToJoinId == (CSteamID)callback.m_ulSteamIDLobby)
			{
				if (!SteamMatchmaking.GetLobbyData(this.m_LobbyToJoinId, "game_ver").Equals(GreenHellGame.s_GameVersion.ToString()))
				{
					if (P2PLogFilter.logWarn)
					{
						Debug.LogWarning("[TransportLayerSteam] Connection to lobby aborted - different game versions.");
					}
					this.m_LobbyToJoinId = CSteamID.Nil;
					return;
				}
				CSteamID lobbyToJoinId = this.m_LobbyToJoinId;
				this.Shutdown();
				SteamMatchmaking.JoinLobby(lobbyToJoinId);
				this.m_LobbyToJoinId = CSteamID.Nil;
				this.m_IsInLobby = true;
				return;
			}
			else
			{
				CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner(this.m_LobbyId);
				if (lobbyOwner.m_SteamID != this.m_LobbyOwner.m_SteamID)
				{
					if (P2PLogFilter.logWarn)
					{
						Debug.LogWarning("[TransportLayerSteam] Lobby owner changed");
					}
					CSteamID lobbyOwner2 = this.m_LobbyOwner;
					this.m_LobbyOwner = lobbyOwner;
					int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.m_LobbyId);
					bool flag = true;
					for (int i = 0; i < numLobbyMembers; i++)
					{
						if (SteamMatchmaking.GetLobbyMemberByIndex(this.m_LobbyId, i).m_SteamID == lobbyOwner2.m_SteamID)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						if (P2PLogFilter.logWarn)
						{
							Debug.LogWarning("[TransportLayerSteam] Lobby owner left! Disconnecting.");
						}
						foreach (KeyValuePair<CSteamID, int> keyValuePair in this.m_Connections)
						{
							this.m_ConnectionsBrokenInternal.Push(keyValuePair.Value);
						}
						this.Shutdown();
					}
				}
				else if (this.m_QueriedLobbyList != null)
				{
					P2PLobbyInfo p2PLobbyInfo = this.m_QueriedLobbyList.FirstOrDefault((P2PLobbyInfo l) => ((P2PAddressSteam)l.m_Address).m_SteamID == (CSteamID)callback.m_ulSteamIDLobby);
					if (p2PLobbyInfo != null)
					{
						CSteamID steamID = ((P2PAddressSteam)p2PLobbyInfo.m_Address).m_SteamID;
						if (!SteamMatchmaking.GetLobbyData(steamID, "game_ver").Equals(GreenHellGame.s_GameVersion.ToString()))
						{
							this.m_QueriedLobbyList.Remove(p2PLobbyInfo);
						}
						else
						{
							p2PLobbyInfo.m_MemberCount = SteamMatchmaking.GetNumLobbyMembers(steamID);
							p2PLobbyInfo.m_SlotCount = SteamMatchmaking.GetLobbyMemberLimit(steamID);
							p2PLobbyInfo.m_Name = SteamMatchmaking.GetLobbyData(steamID, "name");
							p2PLobbyInfo.m_Ready = true;
						}
						bool flag2 = true;
						using (List<P2PLobbyInfo>.Enumerator enumerator2 = this.m_QueriedLobbyList.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (!enumerator2.Current.m_Ready)
								{
									flag2 = false;
									break;
								}
							}
						}
						if (flag2)
						{
							P2PTransportLayer.OnLobbyListAcquired(this.m_QueriedLobbyList);
							this.m_QueriedLobbyList = null;
						}
					}
				}
				if (this.m_LobbyId == (CSteamID)callback.m_ulSteamIDLobby)
				{
					this.UpdateLobbyMembers();
				}
			}
		}
	}

	private bool IsMemberInSteamLobby(CSteamID steamUser)
	{
		if (SteamManager.Initialized)
		{
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.m_LobbyId);
			for (int i = 0; i < numLobbyMembers; i++)
			{
				if (SteamMatchmaking.GetLobbyMemberByIndex(this.m_LobbyId, i).m_SteamID == steamUser.m_SteamID)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateLobbyMembers()
	{
		if (this.m_LobbyId == CSteamID.Nil)
		{
			this.m_LobbyMembers.Clear();
			return;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(this.m_LobbyId);
		this.m_LobbyMembers.Resize(numLobbyMembers);
		for (int i = 0; i < numLobbyMembers; i++)
		{
			CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(this.m_LobbyId, i);
			P2PLobbyMemberInfo p2PLobbyMemberInfo = this.m_LobbyMembers[i];
			p2PLobbyMemberInfo.m_Address = new P2PAddressSteam
			{
				m_SteamID = lobbyMemberByIndex
			};
			p2PLobbyMemberInfo.m_Name = SteamMatchmaking.GetLobbyMemberData(this.m_LobbyId, lobbyMemberByIndex, "member_name");
		}
	}

	private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
	{
		if (callback.m_rgfChatMemberStateChange == 2u && callback.m_ulSteamIDLobby == this.m_LobbyId.m_SteamID)
		{
			CSteamID csteamID = new CSteamID(callback.m_ulSteamIDUserChanged);
			if (P2PLogFilter.logInfo)
			{
				Debug.Log("[TransportLayerSteam] Peer has disconnected " + csteamID);
			}
			this.m_LeaveRequests.Push(csteamID);
		}
	}

	public void RequestLobbyList(P2PGameVisibility visibility)
	{
	}

	private void OnLobbyMatchList(LobbyMatchList_t callback, bool bIOFailure)
	{
	}

	public void SetGameVisibility(P2PGameVisibility visibility)
	{
		switch (visibility)
		{
		case P2PGameVisibility.Public:
			this.m_LobbyVisibility = ELobbyType.k_ELobbyTypePublic;
			break;
		case P2PGameVisibility.Friends:
			this.m_LobbyVisibility = ELobbyType.k_ELobbyTypeFriendsOnly;
			break;
		case P2PGameVisibility.Private:
			this.m_LobbyVisibility = ELobbyType.k_ELobbyTypePrivate;
			break;
		default:
			this.m_LobbyVisibility = ELobbyType.k_ELobbyTypePrivate;
			break;
		}
		if (this.m_LobbyId != CSteamID.Nil && SteamMatchmaking.GetLobbyOwner(this.m_LobbyId) == SteamUser.GetSteamID())
		{
			SteamMatchmaking.SetLobbyType(this.m_LobbyId, this.m_LobbyVisibility);
			SteamMatchmaking.SetLobbyData(this.m_LobbyId, "lobby_type", EnumUtils<P2PGameVisibility>.GetName(visibility));
		}
	}

	public P2PGameVisibility GetGameVisibility()
	{
		P2PGameVisibility result;
		if (this.m_LobbyId != CSteamID.Nil && EnumUtils<P2PGameVisibility>.TryGetValue(SteamMatchmaking.GetLobbyData(this.m_LobbyId, "lobby_type"), out result))
		{
			return result;
		}
		switch (this.m_LobbyVisibility)
		{
		case ELobbyType.k_ELobbyTypePrivate:
			return P2PGameVisibility.Private;
		case ELobbyType.k_ELobbyTypeFriendsOnly:
			return P2PGameVisibility.Friends;
		case ELobbyType.k_ELobbyTypePublic:
			return P2PGameVisibility.Public;
		default:
			return P2PGameVisibility.Singleplayer;
		}
	}

	public ReadOnlyCollection<P2PLobbyMemberInfo> GetCurrentLobbyMembers()
	{
		return this.m_LobbyMembersRead;
	}

	public string GetPlayerDisplayName(IP2PAddress address)
	{
		foreach (P2PLobbyMemberInfo p2PLobbyMemberInfo in this.m_LobbyMembers)
		{
			if (p2PLobbyMemberInfo.m_Address.Equals(address))
			{
				return p2PLobbyMemberInfo.m_Name;
			}
		}
		return SteamFriends.GetFriendPersonaName((address as P2PAddressSteam).m_SteamID);
	}

	public void SetLobbyData(string key, string data)
	{
		if (this.m_LobbyId != CSteamID.Nil && SteamMatchmaking.GetLobbyOwner(this.m_LobbyId) == SteamUser.GetSteamID())
		{
			SteamMatchmaking.SetLobbyData(this.m_LobbyId, key, data);
		}
	}

	public string GetLobbyData(string key)
	{
		return SteamMatchmaking.GetLobbyData(this.m_LobbyId, key);
	}

	private const int LOCALHOST_ID_DEFAULT = 0;

	private const int INVALID_CONNECTION_ID = -1;

	private const string GAME_VERSION_NAME = "game_ver";

	private const string LOBBY_NAME = "name";

	private const string MEMBER_NAME = "member_name";

	private const string LOBBY_TYPE = "lobby_type";

	private bool m_IsInLobby;

	private Dictionary<CSteamID, int> m_Connections = new Dictionary<CSteamID, int>();

	private Stack<CSteamID> m_JoinRequests = new Stack<CSteamID>();

	private Stack<CSteamID> m_LeaveRequests = new Stack<CSteamID>();

	private Stack<int> m_ConnectionsBrokenInternal = new Stack<int>();

	private CSteamID m_LocalSteamID = CSteamID.Nil;

	private CSteamID m_LobbyId = CSteamID.Nil;

	private CSteamID m_LobbyOwner = CSteamID.Nil;

	private CSteamID m_LobbyToJoinId = CSteamID.Nil;

	private ELobbyType m_LobbyVisibility;

	private List<P2PLobbyMemberInfo> m_LobbyMembers;

	private ReadOnlyCollection<P2PLobbyMemberInfo> m_LobbyMembersRead;

	private List<P2PLobbyInfo> m_QueriedLobbyList;

	private Callback<P2PSessionRequest_t> m_P2PSessionRequested;

	private Callback<LobbyEnter_t> m_CallbackLobbyEnter;

	private Callback<GameLobbyJoinRequested_t> m_CallbackLobbyJoinRequested;

	private Callback<LobbyDataUpdate_t> m_CallbackLobbyDataUpdate;

	private Callback<LobbyChatUpdate_t> m_CallbackLobbyChatUpdate;

	private Callback<P2PSessionConnectFail_t> m_CallbackP2PSessionConnectFail;

	private CallResult<LobbyMatchList_t> m_LobbyMatchList;

	private bool m_Initialized;

	private IEnumerable<bool> m_JoiningDelayed;
}
