using System;
using UnityEngine.UI;

public class MenuDebugSounds : MenuDebugScreen
{
	protected override void Update()
	{
		base.Update();
		GreenHellGame.Instance.m_ShowSounds3D = this.m_ShowSounds3D.isOn;
		GreenHellGame.Instance.m_ShowSoundsCurrentlyPlaying = this.m_ShowSoundsCurrentlyPlaying.isOn;
	}

	public Toggle m_ShowSounds3D;

	public Toggle m_ShowSoundsCurrentlyPlaying;
}
