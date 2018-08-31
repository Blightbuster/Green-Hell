using System;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
	private void CSPlay()
	{
		if (!this.m_Playing)
		{
			this.myTimeline.Play();
			CameraManager.Get().enabled = false;
			this.m_Playing = true;
			CutsceneController.s_NumPlayingCutscenes++;
		}
	}

	private void Update()
	{
		if (this.m_Playing && this.myTimeline.state != PlayState.Playing)
		{
			CameraManager.Get().enabled = true;
			this.m_Playing = false;
			CutsceneController.s_NumPlayingCutscenes--;
		}
	}

	public PlayableDirector myTimeline;

	private bool m_Playing;

	public static int s_NumPlayingCutscenes;
}
