using System;
using System.Collections.Generic;
using Enums;

public class ReplicatedPlayerSounds : ReplicatedBehaviour
{
	public void ReplicateSound(ReplicatedPlayerSounds.SSoundData data)
	{
		if (ReplTools.IsPlayingAlone())
		{
			return;
		}
		DebugUtils.Assert(data.type > ReplicatedPlayerSounds.EReplicatedSoundType.Unknown, true);
		this.m_Sounds.Add(data);
		this.ReplSetDirty();
	}

	private void Awake()
	{
		this.m_AudioModule = base.GetComponent<PlayerAudioModule>();
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_Sounds.Count);
		foreach (ReplicatedPlayerSounds.SSoundData ssoundData in this.m_Sounds)
		{
			writer.Write((int)ssoundData.type);
			switch (ssoundData.type)
			{
			case ReplicatedPlayerSounds.EReplicatedSoundType.Hit:
				writer.Write((int)ssoundData.material);
				writer.Write((int)ssoundData.item_id);
				break;
			case ReplicatedPlayerSounds.EReplicatedSoundType.Swing:
				writer.Write((int)ssoundData.item_id);
				break;
			case ReplicatedPlayerSounds.EReplicatedSoundType.Grunt:
				writer.Write((int)ssoundData.grunt);
				break;
			}
		}
		this.m_Sounds.Clear();
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		int num = reader.ReadInt32();
		if (num > 0)
		{
			this.m_AudioModule.InitSources();
		}
		for (int i = 0; i < num; i++)
		{
			switch (reader.ReadInt32())
			{
			case 1:
				this.m_AudioModule.MakeHitSound((EObjectMaterial)reader.ReadInt32(), (ItemID)reader.ReadInt32());
				break;
			case 2:
				this.m_AudioModule.PlaySwingSound((ItemID)reader.ReadInt32());
				break;
			case 3:
				this.m_AudioModule.PlayGruntSound((PlayerAudioModule.GruntPriority)reader.ReadInt32(), null, 1f, false, Noise.Type.None, 0f);
				break;
			}
		}
	}

	private List<ReplicatedPlayerSounds.SSoundData> m_Sounds = new List<ReplicatedPlayerSounds.SSoundData>(10);

	private PlayerAudioModule m_AudioModule;

	public enum EReplicatedSoundType
	{
		Unknown,
		Hit,
		Swing,
		Grunt
	}

	public struct SSoundData
	{
		public ReplicatedPlayerSounds.EReplicatedSoundType type;

		public EObjectMaterial material;

		public ItemID item_id;

		public PlayerAudioModule.GruntPriority grunt;
	}
}
