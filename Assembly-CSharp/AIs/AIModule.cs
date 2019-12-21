using System;
using Enums;

namespace AIs
{
	public class AIModule : BeingModule, IAnimationEventsReceiver
	{
		public override void Initialize(Being being)
		{
			base.Initialize(being);
			this.m_AI = base.gameObject.GetComponent<AI>();
			DebugUtils.Assert(this.m_AI, true);
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

		public virtual void OnUpdate()
		{
		}

		public virtual void OnLateUpdate()
		{
		}

		protected AI m_AI;
	}
}
