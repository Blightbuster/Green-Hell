using System;

public class AnimatorParamReplicatorBool : AnimatorParamReplicator<bool>
{
	public AnimatorParamReplicatorBool(string param_name, ReplicatedAnimator owner) : base(param_name, owner)
	{
	}

	protected override bool GetLocalValue()
	{
		return base.LocalAnimator.GetBool(this.m_ParamName);
	}

	protected override void SetNetValue(bool val)
	{
		base.NetAnimator.SetBool(this.m_ParamName, val);
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write(this.GetLocalValue());
	}

	public override void Deserialize(P2PNetworkReader reader)
	{
		this.SetNetValue(reader.ReadBoolean());
	}
}
