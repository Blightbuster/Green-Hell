using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ScreenFaderClip : PlayableAsset, ITimelineClipAsset
{
	public ClipCaps clipCaps
	{
		get
		{
			return ClipCaps.Blending;
		}
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<ScreenFaderBehaviour> playable = ScriptPlayable<ScreenFaderBehaviour>.Create(graph, this.template, 0);
		return playable;
	}

	public ScreenFaderBehaviour template = new ScreenFaderBehaviour();
}
