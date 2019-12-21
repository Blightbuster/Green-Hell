using System;

public class P2PEmptyMessage : P2PMessageBase
{
	public override void Deserialize(P2PNetworkReader reader)
	{
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
	}
}
