using System;
using System.Collections.Generic;
using UnityEngine;

internal class P2PChannelBuffer : IDisposable
{
	public int numMsgsOut { get; private set; }

	public int numBufferedMsgsOut { get; private set; }

	public int numBytesOut { get; private set; }

	public int numMsgsIn { get; private set; }

	public int numBytesIn { get; private set; }

	public int numBufferedPerSecond { get; private set; }

	public int lastBufferedPerSecond { get; private set; }

	public P2PChannelBuffer(P2PConnection conn, int bufferSize, byte cid, bool isReliable)
	{
		this.m_Connection = conn;
		this.m_MaxPacketSize = bufferSize - 100;
		this.m_CurrentPacket = new P2PChannelPacket(this.m_MaxPacketSize, isReliable);
		this.m_ChannelId = cid;
		this.m_MaxPendingPacketCount = 16;
		this.m_IsReliable = isReliable;
		if (isReliable)
		{
			this.m_PendingPackets = new Queue<P2PChannelPacket>();
			if (P2PChannelBuffer.s_FreePackets == null)
			{
				P2PChannelBuffer.s_FreePackets = new List<P2PChannelPacket>();
			}
		}
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this.m_Disposed && disposing && this.m_PendingPackets != null)
		{
			while (this.m_PendingPackets.Count > 0)
			{
				P2PChannelBuffer.pendingPacketCount--;
				P2PChannelPacket item = this.m_PendingPackets.Dequeue();
				if (P2PChannelBuffer.s_FreePackets.Count < 512)
				{
					P2PChannelBuffer.s_FreePackets.Add(item);
				}
			}
			this.m_PendingPackets.Clear();
		}
		this.m_Disposed = true;
	}

	public bool SetOption(P2PChannelOption option, int value)
	{
		if (option != P2PChannelOption.MaxPendingBuffers)
		{
			return false;
		}
		if (!this.m_IsReliable)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("Cannot set MaxPendingBuffers on unreliable channel " + this.m_ChannelId);
			}
			return false;
		}
		if (value < 0 || value >= 512)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Invalid MaxPendingBuffers for channel ",
					this.m_ChannelId,
					". Must be greater than zero and less than ",
					512
				}));
			}
			return false;
		}
		this.m_MaxPendingPacketCount = value;
		return true;
	}

	public void CheckInternalBuffer()
	{
		if (Time.realtimeSinceStartup - this.m_LastFlushTime > this.maxDelay && !this.m_CurrentPacket.IsEmpty())
		{
			this.SendInternalBuffer();
			this.m_LastFlushTime = Time.realtimeSinceStartup;
		}
		if (Time.realtimeSinceStartup - this.m_LastBufferedMessageCountTimer > 1f)
		{
			this.lastBufferedPerSecond = this.numBufferedPerSecond;
			this.numBufferedPerSecond = 0;
			this.m_LastBufferedMessageCountTimer = Time.realtimeSinceStartup;
		}
	}

	public bool SendWriter(P2PNetworkWriter writer)
	{
		return this.SendBytes(writer.AsArraySegment().Array, writer.AsArraySegment().Count);
	}

	public bool Send(short msgType, P2PMessageBase msg)
	{
		P2PChannelBuffer.s_SendWriter.StartMessage(msgType);
		msg.Serialize(P2PChannelBuffer.s_SendWriter);
		P2PChannelBuffer.s_SendWriter.FinishMessage();
		this.numMsgsOut++;
		return this.SendWriter(P2PChannelBuffer.s_SendWriter);
	}

	internal bool SendBytes(byte[] bytes, int bytesToSend)
	{
		if (bytesToSend <= 0)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError("ChannelBuffer:SendBytes cannot send zero bytes");
			}
			return false;
		}
		if (bytesToSend > this.m_MaxPacketSize)
		{
			if (P2PLogFilter.logError)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Failed to send big message of ",
					bytesToSend,
					" bytes. The maximum is ",
					this.m_MaxPacketSize,
					" bytes on this channel."
				}));
			}
			return false;
		}
		if (this.m_CurrentPacket.HasSpace(bytesToSend))
		{
			this.m_CurrentPacket.Write(bytes, bytesToSend);
			return this.maxDelay != 0f || this.SendInternalBuffer();
		}
		if (this.m_IsReliable)
		{
			if (this.m_PendingPackets.Count == 0)
			{
				if (!this.m_CurrentPacket.SendToTransport(this.m_Connection, (int)this.m_ChannelId))
				{
					this.QueuePacket();
				}
				this.m_CurrentPacket.Write(bytes, bytesToSend);
				return true;
			}
			if (this.m_PendingPackets.Count >= this.m_MaxPendingPacketCount)
			{
				if (!this.m_IsBroken && P2PLogFilter.logError)
				{
					Debug.LogError("ChannelBuffer buffer limit of " + this.m_PendingPackets.Count + " packets reached.");
				}
				this.m_IsBroken = true;
				return false;
			}
			this.QueuePacket();
			this.m_CurrentPacket.Write(bytes, bytesToSend);
			return true;
		}
		else
		{
			if (!this.m_CurrentPacket.SendToTransport(this.m_Connection, (int)this.m_ChannelId))
			{
				if (P2PLogFilter.logError)
				{
					Debug.Log("ChannelBuffer SendBytes no space on unreliable channel " + this.m_ChannelId);
				}
				return false;
			}
			this.m_CurrentPacket.Write(bytes, bytesToSend);
			return true;
		}
	}

	private void QueuePacket()
	{
		P2PChannelBuffer.pendingPacketCount++;
		this.m_PendingPackets.Enqueue(this.m_CurrentPacket);
		this.m_CurrentPacket = this.AllocPacket();
	}

	private P2PChannelPacket AllocPacket()
	{
		if (P2PChannelBuffer.s_FreePackets.Count == 0)
		{
			return new P2PChannelPacket(this.m_MaxPacketSize, this.m_IsReliable);
		}
		P2PChannelPacket result = P2PChannelBuffer.s_FreePackets[P2PChannelBuffer.s_FreePackets.Count - 1];
		P2PChannelBuffer.s_FreePackets.RemoveAt(P2PChannelBuffer.s_FreePackets.Count - 1);
		result.Reset();
		return result;
	}

	private static void FreePacket(P2PChannelPacket packet)
	{
		if (P2PChannelBuffer.s_FreePackets.Count >= 512)
		{
			return;
		}
		P2PChannelBuffer.s_FreePackets.Add(packet);
	}

	public bool SendInternalBuffer()
	{
		if (this.m_IsReliable && this.m_PendingPackets.Count > 0)
		{
			while (this.m_PendingPackets.Count > 0)
			{
				P2PChannelPacket p2PChannelPacket = this.m_PendingPackets.Dequeue();
				if (!p2PChannelPacket.SendToTransport(this.m_Connection, (int)this.m_ChannelId))
				{
					this.m_PendingPackets.Enqueue(p2PChannelPacket);
					break;
				}
				P2PChannelBuffer.pendingPacketCount--;
				P2PChannelBuffer.FreePacket(p2PChannelPacket);
				if (this.m_IsBroken && this.m_PendingPackets.Count < this.m_MaxPendingPacketCount / 2)
				{
					if (P2PLogFilter.logWarn)
					{
						Debug.LogWarning("ChannelBuffer recovered from overflow but data was lost.");
					}
					this.m_IsBroken = false;
				}
			}
			return true;
		}
		return this.m_CurrentPacket.SendToTransport(this.m_Connection, (int)this.m_ChannelId);
	}

	private P2PConnection m_Connection;

	private P2PChannelPacket m_CurrentPacket;

	private float m_LastFlushTime;

	private byte m_ChannelId;

	private int m_MaxPacketSize;

	private bool m_IsReliable;

	private bool m_IsBroken;

	private int m_MaxPendingPacketCount;

	private const int k_MaxFreePacketCount = 512;

	private const int k_MaxPendingPacketCount = 16;

	private Queue<P2PChannelPacket> m_PendingPackets;

	private static List<P2PChannelPacket> s_FreePackets;

	internal static int pendingPacketCount;

	public float maxDelay = 0.01f;

	private float m_LastBufferedMessageCountTimer = Time.realtimeSinceStartup;

	private static P2PNetworkWriter s_SendWriter = new P2PNetworkWriter();

	private const int k_PacketHeaderReserveSize = 100;

	private bool m_Disposed;
}
