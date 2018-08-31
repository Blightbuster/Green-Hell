using System;

public class Arrow : Weapon
{
	protected override void Awake()
	{
		base.Awake();
	}

	public override bool CanTrigger()
	{
		return !this.m_Loaded && base.CanTrigger();
	}

	public bool m_Loaded;
}
