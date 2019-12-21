using System;

public abstract class P2PMessageBase
{
	public virtual void Deserialize(P2PNetworkReader reader)
	{
	}

	public virtual void Serialize(P2PNetworkWriter writer)
	{
	}
}
