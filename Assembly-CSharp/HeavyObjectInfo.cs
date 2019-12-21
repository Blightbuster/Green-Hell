using System;

public class HeavyObjectInfo : ItemInfo
{
	public float m_DirtinessOnTake { get; set; }

	public override bool IsHeavyObject()
	{
		return true;
	}

	public HeavyObjectInfo()
	{
		this.m_DirtinessOnTake = 0f;
	}

	protected override void LoadParams(Key key)
	{
		if (key.GetName() == "DirtinessOnTake")
		{
			this.m_DirtinessOnTake = key.GetVariable(0).FValue;
			return;
		}
		base.LoadParams(key);
	}
}
