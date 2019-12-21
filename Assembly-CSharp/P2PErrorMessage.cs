using System;

public class P2PErrorMessage : P2PMessageBase
{
	public override void Deserialize(P2PNetworkReader reader)
	{
		this.errorCode = (int)reader.ReadUInt16();
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write((ushort)this.errorCode);
	}

	public int errorCode;
}
