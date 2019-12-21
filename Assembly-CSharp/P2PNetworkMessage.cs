using System;

public class P2PNetworkMessage
{
	public static string Dump(byte[] payload, int sz)
	{
		string text = "[";
		for (int i = 0; i < sz; i++)
		{
			text = text + payload[i] + " ";
		}
		return text + "]";
	}

	public TMsg ReadMessage<TMsg>() where TMsg : P2PMessageBase, new()
	{
		TMsg tmsg = Activator.CreateInstance<TMsg>();
		tmsg.Deserialize(this.m_Reader);
		return tmsg;
	}

	public void ReadMessage<TMsg>(TMsg msg) where TMsg : P2PMessageBase
	{
		msg.Deserialize(this.m_Reader);
	}

	public const int MaxMessageSize = 65535;

	public short m_MsgType;

	public P2PConnection m_Connection;

	public P2PNetworkReader m_Reader;

	public int m_ChannelId;
}
