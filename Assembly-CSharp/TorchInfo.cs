using System;

public class TorchInfo : WeaponInfo
{
	public float m_BurningDurationInMinutes { get; set; }

	public float m_DamageWhenBurning { get; set; }

	public TorchInfo()
	{
		this.m_BurningDurationInMinutes = 60f;
		this.m_DamageWhenBurning = 0.01f;
	}

	protected override void LoadParams(Key key)
	{
		base.LoadParams(key);
		if (key.GetName() == "BurningDurationInMinutes")
		{
			this.m_BurningDurationInMinutes = key.GetVariable(0).FValue;
		}
	}

	public override bool IsTorch()
	{
		return true;
	}
}
