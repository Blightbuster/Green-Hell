using System;
using UnityEngine;

public interface ISoundEventsReceiver
{
	void OnSoundEvent(AudioClip clip);

	bool IsActive();
}
