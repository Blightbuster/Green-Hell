using System;
using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(P2PNetworkManager))]
[EditorBrowsable(EditorBrowsableState.Never)]
public class P2PNetworkManagerHud : MonoBehaviour
{
	private void Awake()
	{
		this.m_Manager = base.GetComponent<P2PNetworkManager>();
	}

	private void OnGUI()
	{
		if (!this.m_ShowGUI)
		{
			return;
		}
		int num = 10 + this.m_OffsetX;
		int num2 = 40 + this.m_OffsetY;
		if (P2PSession.Instance == null || !P2PSession.Instance.IsValid())
		{
			if (GUI.Button(new Rect((float)num, (float)num2, 100f, 20f), "Start Host"))
			{
				P2PAddressUnet address = new P2PAddressUnet
				{
					m_IP = this.m_ConnectToIp,
					m_Port = this.m_Port
				};
				this.m_Manager.StartSession(address);
			}
			if (P2PSession.s_TransportLayerType == ETransporLayerType.UNet)
			{
				this.m_ConnectToIp = GUI.TextField(new Rect((float)(num + 100), (float)num2, 95f, 20f), this.m_ConnectToIp);
				num2 += 24;
				this.m_Port = int.Parse(GUI.TextField(new Rect((float)(num + 100), (float)num2, 95f, 20f), this.m_Port.ToString()));
				num2 += 24;
				return;
			}
		}
		else if (P2PSession.Instance.GetConnectionCount() < P2PSession.MAX_PLAYERS)
		{
			if (P2PTransportLayer.Instance != null && P2PTransportLayer.Instance.GetLayerType() == ETransporLayerType.UNet)
			{
				if (GUI.Button(new Rect((float)num, (float)num2, 100f, 20f), "Connect"))
				{
					this.m_Manager.Connect(this.m_ConnectToIp, this.m_Port);
				}
				this.m_ConnectToIp = GUI.TextField(new Rect((float)(num + 100), (float)num2, 95f, 20f), this.m_ConnectToIp);
				num2 += 24;
				this.m_Port = int.Parse(GUI.TextField(new Rect((float)(num + 100), (float)num2, 95f, 20f), this.m_Port.ToString()));
				num2 += 24;
			}
			GUI.Label(new Rect((float)num, (float)num2, 200f, 20f), "Connected to " + P2PSession.Instance.GetConnectionCount() + " peers");
			num2 += 24;
			if (GUI.Button(new Rect((float)num, (float)num2, 200f, 20f), "Shutdown"))
			{
				this.m_Manager.StopSession();
			}
		}
	}

	private P2PNetworkManager m_Manager;

	[SerializeField]
	public bool m_ShowGUI = true;

	[SerializeField]
	public int m_OffsetX;

	[SerializeField]
	public int m_OffsetY;

	public string m_ConnectToIp = "localhost";

	public int m_Port = 8888;

	private bool m_ShowServer;
}
