using System;
using UnityEngine;

public interface IAnimationEventsReceiverEx : IAnimationEventsReceiver
{
	void OnAnimEventEx(AnimationEvent anim_event);
}
