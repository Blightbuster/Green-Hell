using System;

public class P2PStringMessage : P2PMessageBase
{
	public P2PStringMessage()
	{
	}

	public P2PStringMessage(string v)
	{
		this.value = v;
	}

	public override void Deserialize(P2PNetworkReader reader)
	{
		this.value = reader.ReadString();
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write(this.value);
	}

	public string value;
}
