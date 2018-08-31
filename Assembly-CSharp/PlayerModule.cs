using System;
using Enums;

public class PlayerModule : BeingModule, IAnimationEventsReceiver
{
	public override void Initialize()
	{
		base.Initialize();
		this.m_Player = base.gameObject.GetComponent<Player>();
		DebugUtils.Assert(this.m_Player, true);
	}

	public virtual bool ForceReceiveAnimEvent()
	{
		return false;
	}

	public virtual void OnAnimEvent(AnimEventID id)
	{
	}

	public virtual bool IsActive()
	{
		return true;
	}

	protected Player m_Player;
}
