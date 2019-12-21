using System;

internal interface IAnimatorParamReplicator
{
	void UpdateCurrent();

	void Update(float dt);

	void TestUpdate();

	void Serialize(P2PNetworkWriter writer);

	void Deserialize(P2PNetworkReader reader);
}
