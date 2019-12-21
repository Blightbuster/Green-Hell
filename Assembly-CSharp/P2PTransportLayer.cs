using System;
using System.Collections.Generic;

public static class P2PTransportLayer
{
	public static event P2PTransportLayer.OnExternalLobbyJoinRequestDel OnExternalLobbyJoinRequestEvent;

	public static event P2PTransportLayer.OnLobbyListAcquiredDel OnLobbyListAcquiredEvent;

	public static event Action<bool> OnLobbyEnterEvent;

	public static event P2PTransportLayer.CanStartSessionDel CanStartSessionCheckEvent;

	public static ITransportLayer Instance
	{
		get
		{
			return P2PTransportLayer.s_Instance;
		}
	}

	public static void Create(ETransporLayerType type)
	{
		DebugUtils.Assert(P2PTransportLayer.Instance == null || P2PTransportLayer.Instance.GetLayerType() == type, true);
		if (P2PTransportLayer.Instance != null)
		{
			return;
		}
		if (type == ETransporLayerType.UNet)
		{
			P2PTransportLayer.s_Instance = new TransportLayerUNet();
			return;
		}
		if (type != ETransporLayerType.Steam)
		{
			throw new NotImplementedException();
		}
		P2PTransportLayer.s_Instance = new TransportLayerSteam();
	}

	public static void OnExternalLobbyJoinRequest(IP2PAddress address)
	{
		P2PTransportLayer.OnExternalLobbyJoinRequestDel onExternalLobbyJoinRequestEvent = P2PTransportLayer.OnExternalLobbyJoinRequestEvent;
		if (onExternalLobbyJoinRequestEvent == null)
		{
			return;
		}
		onExternalLobbyJoinRequestEvent(address);
	}

	public static void OnLobbyListAcquired(List<P2PLobbyInfo> lobbies)
	{
		P2PTransportLayer.OnLobbyListAcquiredDel onLobbyListAcquiredEvent = P2PTransportLayer.OnLobbyListAcquiredEvent;
		if (onLobbyListAcquiredEvent == null)
		{
			return;
		}
		onLobbyListAcquiredEvent(lobbies);
	}

	public static void OnLobbyEnter(bool is_owner)
	{
		Action<bool> onLobbyEnterEvent = P2PTransportLayer.OnLobbyEnterEvent;
		if (onLobbyEnterEvent == null)
		{
			return;
		}
		onLobbyEnterEvent(is_owner);
	}

	public static void CanStartSession(P2PTransportLayer.CanStartSessionEventArgs args)
	{
		P2PTransportLayer.CanStartSessionDel canStartSessionCheckEvent = P2PTransportLayer.CanStartSessionCheckEvent;
		if (canStartSessionCheckEvent == null)
		{
			return;
		}
		canStartSessionCheckEvent(args);
	}

	public static void Shutdown()
	{
		if (P2PTransportLayer.Instance != null)
		{
			P2PTransportLayer.Instance.Shutdown();
		}
	}

	private static ITransportLayer s_Instance;

	public delegate void OnExternalLobbyJoinRequestDel(IP2PAddress address);

	public delegate void OnLobbyListAcquiredDel(List<P2PLobbyInfo> lobbies);

	public delegate void CanStartSessionDel(P2PTransportLayer.CanStartSessionEventArgs args);

	public class CanStartSessionEventArgs : EventArgs
	{
		public bool IsSessionReady
		{
			get
			{
				return this.m_WasSet && this.m_IsSessionReadyInternal;
			}
			set
			{
				this.m_WasSet = true;
				this.m_IsSessionReadyInternal = (this.m_IsSessionReadyInternal && value);
			}
		}

		private bool m_WasSet;

		private bool m_IsSessionReadyInternal = true;
	}
}
