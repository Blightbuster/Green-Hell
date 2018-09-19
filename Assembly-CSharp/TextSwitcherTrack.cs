using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[TrackClipType(typeof(TextSwitcherClip))]
[TrackBindingType(typeof(Text))]
[TrackColor(0.1394896f, 0.4411765f, 0.3413077f)]
public class TextSwitcherTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<TextSwitcherMixerBehaviour>.Create(graph, inputCount);
	}

	public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		base.GatherProperties(director, driver);
	}
}
