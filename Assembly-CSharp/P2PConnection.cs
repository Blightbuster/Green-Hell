using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class P2PConnection : IDisposable
{
	public P2PPeer m_Peer { get; private set; }

	public virtual void Initialize(P2PPeer peer, int networkConnectionId, HostTopology hostTopology)
	{
		this.m_Writer = new P2PNetworkWriter();
		this.m_Reader = new P2PNetworkReader();
		this.m_ConnectionId = networkConnectionId;
		this.m_Peer = peer;
		int channelCount = hostTopology.DefaultConfig.ChannelCount;
		int packetSize = (int)hostTopology.DefaultConfig.PacketSize;
		if (hostTopology.DefaultConfig.UsePlatformSpecificProtocols && Application.platform != RuntimePlatform.PS4)
		{
			throw new ArgumentOutOfRangeException("Platform specific protocols are not supported on this platform");
		}
		this.m_Channels = new P2PChannelBuffer[channelCount];
		for (int i = 0; i < channelCount; i++)
		{
			ChannelQOS channelQOS = hostTopology.DefaultConfig.Channels[i];
			int bufferSize = packetSize;
			if (channelQOS.QOS == QosType.ReliableFragmented || channelQOS.QOS == QosType.UnreliableFragmented)
			{
				bufferSize = (int)(hostTopology.DefaultConfig.FragmentSize * 128);
			}
			this.m_Channels[i] = new P2PChannelBuffer(this, bufferSize, (byte)i, P2PConnection.IsReliableQoS(channelQOS.QOS));
		}
	}

	~P2PConnection()
	{
		this.Dispose(false);
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this.m_Disposed && this.m_Channels != null)
		{
			for (int i = 0; i < this.m_Channels.Length; i++)
			{
				this.m_Channels[i].Dispose();
			}
		}
		this.m_Channels = null;
		this.m_Disposed = true;
	}

	private static bool IsReliableQoS(QosType qos)
	{
		return qos == QosType.Reliable || qos == QosType.ReliableFragmented || qos == QosType.ReliableSequenced || qos == QosType.ReliableStateUpdate;
	}

	public bool SetChannelOption(int channelId, P2PChannelOption option, int value)
	{
		return this.m_Channels != null && channelId >= 0 && channelId < this.m_Channels.Length && this.m_Channels[channelId].SetOption(option, value);
	}

	public P2PConnection()
	{
		this.m_Writer = new P2PNetworkWriter();
	}

	public void Disconnect()
	{
		this.m_IsReady = false;
		if (P2PSession.Instance.LocalPeer.GetLocalHostId() == P2PPeer.s_InvalidId)
		{
			return;
		}
		byte b;
		P2PTransportLayer.Instance.Disconnect(this.m_ConnectionId, out b);
	}

	internal void SetHandlers(P2PNetworkMessageHandlers handlers)
	{
		this.m_MessageHandlers = handlers;
		this.m_MessageHandlersDict = handlers.GetAllHandlers();
	}

	public bool CheckHandler(short msgType)
	{
		return this.m_MessageHandlersDict.ContainsKey(msgType);
	}

	public bool InvokeHandlerNoData(short msgType)
	{
		return this.InvokeHandler(msgType, null, 0);
	}

	public bool InvokeHandler(short msgType, P2PNetworkReader reader, int channelId)
	{
		if (!this.m_MessageHandlersDict.ContainsKey(msgType))
		{
			return false;
		}
		this.m_MessageInfo.m_MsgType = msgType;
		this.m_MessageInfo.m_Connection = this;
		this.m_MessageInfo.m_Reader = reader;
		this.m_MessageInfo.m_ChannelId = channelId;
		List<P2PNetworkMessageDelegate> list = this.m_MessageHandlersDict[msgType];
		if (list == null)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("NetworkConnection InvokeHandler no handler for " + msgType);
			}
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i](this.m_MessageInfo);
		}
		return true;
	}

	public bool InvokeHandler(P2PNetworkMessage netMsg)
	{
		if (this.m_MessageHandlersDict.ContainsKey(netMsg.m_MsgType))
		{
			List<P2PNetworkMessageDelegate> list = this.m_MessageHandlersDict[netMsg.m_MsgType];
			for (int i = 0; i < list.Count; i++)
			{
				list[i](this.m_MessageInfo);
			}
			return true;
		}
		return false;
	}

	public void RegisterHandler(short msgType, P2PNetworkMessageDelegate handler)
	{
		this.m_MessageHandlers.RegisterHandler(msgType, handler);
	}

	public void UnregisterHandler(short msgType)
	{
		this.m_MessageHandlers.UnregisterHandler(msgType);
	}

	public void FlushChannels()
	{
		if (this.m_Channels == null)
		{
			return;
		}
		P2PChannelBuffer[] channels = this.m_Channels;
		for (int i = 0; i < channels.Length; i++)
		{
			channels[i].CheckInternalBuffer();
		}
	}

	public void SetMaxDelay(float seconds)
	{
		if (this.m_Channels == null)
		{
			return;
		}
		P2PChannelBuffer[] channels = this.m_Channels;
		for (int i = 0; i < channels.Length; i++)
		{
			channels[i].maxDelay = seconds;
		}
	}

	public virtual bool Send(short msgType, P2PMessageBase msg)
	{
		return this.SendByChannel(msgType, msg, 0);
	}

	public virtual bool SendUnreliable(short msgType, P2PMessageBase msg)
	{
		return this.SendByChannel(msgType, msg, 1);
	}

	public virtual bool SendByChannel(short msgType, P2PMessageBase msg, int channelId)
	{
		this.m_Writer.StartMessage(msgType);
		msg.Serialize(this.m_Writer);
		this.m_Writer.FinishMessage();
		return this.SendWriter(this.m_Writer, channelId);
	}

	public virtual bool SendBytes(byte[] bytes, int numBytes, int channelId)
	{
		if (this.m_LogNetworkMessages)
		{
			this.LogSend(bytes);
		}
		return this.CheckChannel(channelId) && this.m_Channels[channelId].SendBytes(bytes, numBytes);
	}

	public virtual bool SendWriter(P2PNetworkWriter writer, int channelId)
	{
		if (this.m_LogNetworkMessages)
		{
			this.LogSend(writer.ToArray());
		}
		return this.CheckChannel(channelId) && this.m_Channels[channelId].SendWriter(writer);
	}

	private void LogSend(byte[] bytes)
	{
		P2PNetworkReader p2PNetworkReader = new P2PNetworkReader(bytes);
		ushort num = p2PNetworkReader.ReadUInt16();
		ushort num2 = p2PNetworkReader.ReadUInt16();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 4; i < (int)(4 + num); i++)
		{
			stringBuilder.AppendFormat("{0:X2}", bytes[i]);
			if (i > 150)
			{
				break;
			}
		}
		Debug.Log(string.Concat(new object[]
		{
			"ConnectionSend con:",
			this.m_ConnectionId,
			" bytes:",
			num,
			" msgId:",
			num2,
			" ",
			stringBuilder
		}));
	}

	private bool CheckChannel(int channelId)
	{
		if (this.m_Channels == null)
		{
			if (P2PLogFilter.logWarn)
			{
				Debug.LogWarning("Channels not initialized sending on id '" + channelId);
			}
			return false;
		}
		if (channelId < 0 || channelId >= this.m_Channels.Length)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Invalid channel when sending buffered data, '",
					channelId,
					"'. Current channel count is ",
					this.m_Channels.Length
				}));
			}
			return false;
		}
		return true;
	}

	public void ResetStats()
	{
	}

	protected void HandleBytes(byte[] buffer, int receivedSize, int channelId)
	{
		byte[] buffer2 = this.m_Reader.Replace(buffer);
		this.HandleReader(this.m_Reader, receivedSize, channelId);
		this.m_Reader.Replace(buffer2);
	}

	protected void HandleReader(P2PNetworkReader reader, int received_size, int channel_id)
	{
		while ((ulong)reader.Position < (ulong)((long)received_size))
		{
			ushort num = reader.ReadUInt16();
			short num2 = reader.ReadInt16();
			uint position = reader.Position;
			reader.SetGuard(position + (uint)num);
			P2PConnection.s_Size = position + (uint)num;
			if (this.m_LogNetworkMessages)
			{
				Debug.Log(string.Concat(new object[]
				{
					"ConnectionRecv con:",
					this.m_ConnectionId,
					" bytes:",
					num,
					" msgId:",
					num2
				}));
			}
			List<P2PNetworkMessageDelegate> list = null;
			if (this.m_MessageHandlersDict.ContainsKey(num2))
			{
				list = this.m_MessageHandlersDict[num2];
			}
			if (list != null)
			{
				this.m_NetMsg.m_MsgType = num2;
				this.m_NetMsg.m_Reader = reader;
				this.m_NetMsg.m_Connection = this;
				this.m_NetMsg.m_ChannelId = channel_id;
				for (int i = 0; i < list.Count; i++)
				{
					list[i](this.m_NetMsg);
				}
				this.m_LastMessageTime = Time.time;
			}
			else if (P2PLogFilter.logError)
			{
				Debug.LogError(string.Format("Unknown message type {0} connection id: {1}", P2PMsgType.MsgTypeToString(num2), this.m_ConnectionId));
			}
			if (position + (uint)num != reader.Position)
			{
				int num3 = (int)(position + (uint)num - reader.Position);
				if (num3 > 0 && P2PLogFilter.logInfo)
				{
					Debug.Log(string.Format("Message {0} conn_id: {1} was not fully read, performing seek by {2} bytes", P2PMsgType.MsgTypeToString(num2), this.m_ConnectionId, num3));
				}
				if (num3 < 0 && P2PLogFilter.logError)
				{
					Debug.LogError(string.Format("Message {0} conn_id: {1} reading over the buffer limit (this is really bad), performing seek by {2} bytes", P2PMsgType.MsgTypeToString(num2), this.m_ConnectionId, num3));
				}
				reader.Seek(num3);
			}
			reader.RemoveGuard();
		}
	}

	public virtual void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
	{
		numMsgs = 0;
		numBufferedMsgs = 0;
		numBytes = 0;
		lastBufferedPerSecond = 0;
		foreach (P2PChannelBuffer p2PChannelBuffer in this.m_Channels)
		{
			numMsgs += p2PChannelBuffer.numMsgsOut;
			numBufferedMsgs += p2PChannelBuffer.numBufferedMsgsOut;
			numBytes += p2PChannelBuffer.numBytesOut;
			lastBufferedPerSecond += p2PChannelBuffer.lastBufferedPerSecond;
		}
	}

	public virtual void GetStatsIn(out int numMsgs, out int numBytes)
	{
		numMsgs = 0;
		numBytes = 0;
		foreach (P2PChannelBuffer p2PChannelBuffer in this.m_Channels)
		{
			numMsgs += p2PChannelBuffer.numMsgsIn;
			numBytes += p2PChannelBuffer.numBytesIn;
		}
	}

	public override string ToString()
	{
		return string.Format("connectionId: {0} isReady: {1} channel count: {2}", this.m_ConnectionId, this.m_IsReady, (this.m_Channels != null) ? this.m_Channels.Length : 0);
	}

	public virtual void TransportReceive(byte[] bytes, int numBytes, int channelId)
	{
		this.HandleBytes(bytes, numBytes, channelId);
	}

	public virtual bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error)
	{
		return P2PTransportLayer.Instance.Send(this.m_ConnectionId, channelId, bytes, numBytes, out error);
	}

	private P2PChannelBuffer[] m_Channels;

	private P2PNetworkMessage m_NetMsg = new P2PNetworkMessage();

	private P2PNetworkWriter m_Writer;

	private P2PNetworkReader m_Reader;

	private Dictionary<short, List<P2PNetworkMessageDelegate>> m_MessageHandlersDict;

	private P2PNetworkMessageHandlers m_MessageHandlers;

	private P2PNetworkMessage m_MessageInfo = new P2PNetworkMessage();

	private const int k_MaxMessageLogSize = 150;

	public int m_ConnectionId = -1;

	public bool m_IsReady;

	public float m_LastMessageTime;

	public bool m_LogNetworkMessages;

	private bool m_Disposed;

	public static uint s_Size;
}
