using System;
using CJTools;

public class AnimatorParamReplicatorFloat : AnimatorParamReplicator<float>
{
	public AnimatorParamReplicatorFloat(string param_name, ReplicatedAnimator owner) : base(param_name, owner)
	{
		this.m_Spring.Init(this.m_PrevValue, this.m_Owner.m_SpringSpeed);
	}

	protected override float GetLocalValue()
	{
		return base.LocalAnimator.GetFloat(this.m_ParamName);
	}

	protected override void SetNetValue(float val)
	{
		base.NetAnimator.SetFloat(this.m_ParamName, val);
	}

	public override int Compare(float x, float y)
	{
		if (System.Math.Truncate((double)(x * 1000f)) == System.Math.Truncate((double)(y * 1000f)))
		{
			return 0;
		}
		return x.CompareTo(y);
	}

	public override void Update(float dt)
	{
		this.m_Spring.Update(dt);
		this.SetNetValue(this.m_Spring.current);
	}

	public override void Serialize(P2PNetworkWriter writer)
	{
		writer.Write(this.GetLocalValue());
	}

	public override void Deserialize(P2PNetworkReader reader)
	{
		this.m_Spring.target = reader.ReadFloat();
	}

	private SpringFloat m_Spring;
}
