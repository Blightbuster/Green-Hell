using System;

public interface IReplicatedBehaviour
{
	void ReplOnChangedOwner(bool was_owner);

	void ReplOnSpawned();

	void OnReplicationPrepare();

	void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state);

	void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state);

	void OnReplicationResolve();
}
