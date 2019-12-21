using System;
using AIs;
using UnityEngine;
using UnityEngine.AI;

public class P2PNetworkManager : MonoBehaviour
{
	private void Start()
	{
		P2PSession.Instance.RegisterHandler(32, new P2PNetworkMessageDelegate(this.OnConnect));
		P2PSession.Instance.RegisterHandler(33, new P2PNetworkMessageDelegate(this.OnDisconnect));
		if (this.m_LogicalPlayer == null)
		{
			this.m_LogicalPlayer = UnityEngine.Object.Instantiate<GameObject>(this.m_PlayerPrefab, Player.Get().transform.position, Player.Get().transform.rotation);
		}
	}

	private void OnApplicationQuit()
	{
		P2PSession.Instance.OnAppQuit();
	}

	private void OnConnect(P2PNetworkMessage net_msg)
	{
		if (net_msg.m_Connection.m_Peer == P2PSession.Instance.GetSessionMaster())
		{
			ItemsManager.Get().ClearFallenObjects();
			ItemsManager.Get().Preload();
			MainLevel.Instance.ResetGameBeforeLoad();
			Vector2 vector = UnityEngine.Random.insideUnitCircle * 2f;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(P2PSession.Instance.m_SpawnRefPosition + new Vector3(vector.x, 0f, vector.y), out navMeshHit, 10f, AIManager.s_WalkableAreaMask))
			{
				Player.Get().Reposition(navMeshHit.position, null);
				Player.Get().m_RespawnPosition = navMeshHit.position;
			}
			if (this.m_LogicalPlayer == null)
			{
				this.m_LogicalPlayer = UnityEngine.Object.Instantiate<GameObject>(this.m_PlayerPrefab, Player.Get().transform.position, Player.Get().transform.rotation);
			}
			SaveGame.LoadCoop();
		}
	}

	private void OnDisconnect(P2PNetworkMessage net_msg)
	{
	}

	public void StartSession()
	{
		if (P2PSession.Instance != null)
		{
			P2PSession.Instance.Start();
		}
	}

	public void StartSession(IP2PAddress address)
	{
		if (P2PSession.Instance != null)
		{
			P2PSession.Instance.Start(address);
		}
	}

	public void Connect(string ip_address, int port)
	{
		if (P2PSession.Instance != null)
		{
			P2PSession.Instance.Connect(new P2PAddressUnet
			{
				m_IP = ip_address,
				m_Port = port
			});
		}
	}

	public void StopSession()
	{
		if (P2PSession.Instance != null)
		{
			P2PSession.Instance.DisconnectAllConnections();
			P2PSession.Instance.Stop();
		}
	}

	private void Update()
	{
		if (P2PSession.Instance != null)
		{
			P2PSession.Instance.Update();
		}
	}

	[NonSerialized]
	public GameObject m_LogicalPlayer;

	[SerializeField]
	private GameObject m_PlayerPrefab;

	public bool m_TPPTest;

	public static bool s_TPPTest;
}
