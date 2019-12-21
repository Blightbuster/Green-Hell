using System;
using UnityEngine;

internal class P2PObjectSpawnMessage : P2PMessageBase
{
	public override void Deserialize(P2PNetworkReader reader)
	{
		this.guid_bytes = reader.ReadGuidBytesTemporary();
		this.assetId = reader.ReadNetworkHash128();
		this.position = reader.ReadVector3();
		this.payload = reader.ReadBytesAndSize();
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write(this.guid_bytes, GuidComponent.GUID_BYTES_CNT);
		writer.Write(this.assetId);
		writer.Write(this.position);
		writer.WriteBytesFull(this.payload);
	}

	public byte[] guid_bytes;

	public P2PNetworkHash128 assetId;

	public Vector3 position;

	public byte[] payload;
}
