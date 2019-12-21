using System;

public class P2PIntegerMessage : P2PMessageBase
{
	public P2PIntegerMessage()
	{
	}

	public P2PIntegerMessage(int v)
	{
		this.value = v;
	}

	public override void Deserialize(P2PNetworkReader reader)
	{
		this.value = (int)reader.ReadPackedUInt32();
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.WritePackedUInt32((uint)this.value);
	}

	public int value;
}
