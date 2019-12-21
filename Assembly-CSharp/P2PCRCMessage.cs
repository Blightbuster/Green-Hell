using System;

internal class P2PCRCMessage : P2PMessageBase
{
	public override void Deserialize(P2PNetworkReader reader)
	{
		int num = (int)reader.ReadUInt16();
		this.scripts = new P2PCRCMessageEntry[num];
		for (int i = 0; i < this.scripts.Length; i++)
		{
			P2PCRCMessageEntry p2PCRCMessageEntry = default(P2PCRCMessageEntry);
			p2PCRCMessageEntry.name = reader.ReadString();
			p2PCRCMessageEntry.channel = reader.ReadByte();
			this.scripts[i] = p2PCRCMessageEntry;
		}
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write((ushort)this.scripts.Length);
		foreach (P2PCRCMessageEntry p2PCRCMessageEntry in this.scripts)
		{
			writer.Write(p2PCRCMessageEntry.name);
			writer.Write(p2PCRCMessageEntry.channel);
		}
	}

	public P2PCRCMessageEntry[] scripts;
}
