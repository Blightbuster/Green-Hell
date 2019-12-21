using System;
using System.Collections.ObjectModel;
using UnityEngine.Networking;

public interface ITransportLayer
{
	ETransporLayerType GetLayerType();

	void Init();

	void Shutdown();

	bool IsStarted();

	bool IsMaster();

	int AddHost(HostTopology topology, ref IP2PAddress address);

	int AddHostWithSimulator(HostTopology topology, int min_timeout, int max_timeout, ref IP2PAddress address);

	bool RemoveHost();

	int Connect(IP2PAddress address, out byte error);

	int ConnectWithSimulator(IP2PAddress address, out byte error, ConnectionSimulatorConfig config);

	void GetConnectionInfo(int connection_id, out IP2PAddress address, out byte error);

	bool Disconnect(int connectionId, out byte error);

	bool Send(int connection_id, int channel_id, byte[] bytes, int num_bytes, out byte error);

	NetworkEventType Receive(out int connection_id, out int channel_id, byte[] buffer, int buffer_size, out int received_size, out byte error);

	void Update();

	void RequestLobbyList(P2PGameVisibility visibility);

	void SetGameVisibility(P2PGameVisibility visibility);

	P2PGameVisibility GetGameVisibility();

	ReadOnlyCollection<P2PLobbyMemberInfo> GetCurrentLobbyMembers();

	string GetPlayerDisplayName(IP2PAddress address);

	void SetLobbyData(string key, string data);

	string GetLobbyData(string key);
}
