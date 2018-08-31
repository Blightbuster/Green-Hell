using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TimeDilationClip : PlayableAsset, ITimelineClipAsset
{
	public ClipCaps clipCaps
	{
		get
		{
			return ClipCaps.Extrapolation | ClipCaps.Blending;
		}
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<TimeDilationBehaviour> playable = ScriptPlayable<TimeDilationBehaviour>.Create(graph, this.template, 0);
		return playable;
	}

	public TimeDilationBehaviour template = new TimeDilationBehaviour();
}
