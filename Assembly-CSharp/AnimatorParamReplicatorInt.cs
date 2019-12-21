using System;

public class AnimatorParamReplicatorInt : AnimatorParamReplicator<int>
{
	public AnimatorParamReplicatorInt(string param_name, ReplicatedAnimator owner) : base(param_name, owner)
	{
	}

	protected override int GetLocalValue()
	{
		return base.LocalAnimator.GetInteger(this.m_ParamName);
	}

	protected override void SetNetValue(int val)
	{
		base.NetAnimator.SetInteger(this.m_ParamName, val);
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write(this.GetLocalValue());
	}

	public override void Deserialize(P2PNetworkReader reader)
	{
		this.SetNetValue(reader.ReadInt32());
	}
}
