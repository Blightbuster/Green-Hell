using System;
using System.Collections.Generic;
using UnityEngine;

internal class P2PNetworkMessageHandlers
{
	public void RegisterHandler(short msgType, P2PNetworkMessageDelegate handler)
	{
		if (handler == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("RegisterHandler id:" + msgType + " handler is null");
			}
			return;
		}
		if (this.m_MsgHandlers.ContainsKey(msgType))
		{
			if (!this.m_MsgHandlers[msgType].Contains(handler))
			{
				this.m_MsgHandlers[msgType].Add(handler);
				return;
			}
		}
		else
		{
			List<P2PNetworkMessageDelegate> list = new List<P2PNetworkMessageDelegate>();
			list.Add(handler);
			this.m_MsgHandlers.Add(msgType, list);
		}
	}

	public void UnregisterHandler(short msgType)
	{
		this.m_MsgHandlers.Remove(msgType);
	}

	internal List<P2PNetworkMessageDelegate> GetHandlers(short msgType)
	{
		if (this.m_MsgHandlers.ContainsKey(msgType))
		{
			return this.m_MsgHandlers[msgType];
		}
		return null;
	}

	internal Dictionary<short, List<P2PNetworkMessageDelegate>> GetAllHandlers()
	{
		return this.m_MsgHandlers;
	}

	internal void ClearMessageHandlers()
	{
		this.m_MsgHandlers.Clear();
	}

	private Dictionary<short, List<P2PNetworkMessageDelegate>> m_MsgHandlers = new Dictionary<short, List<P2PNetworkMessageDelegate>>();
}
