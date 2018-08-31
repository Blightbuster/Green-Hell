using System;
using UnityEngine;

public class SensorMusic : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		Music.Get().Play(this.m_AudioClip);
	}

	public AudioClip m_AudioClip;
}
