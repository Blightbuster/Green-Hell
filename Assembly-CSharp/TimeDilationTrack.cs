using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(TimeDilationClip))]
[TrackColor(0.855f, 0.8623f, 0.87f)]
public class TimeDilationTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<TimeDilationMixerBehaviour>.Create(graph, inputCount);
	}
}
