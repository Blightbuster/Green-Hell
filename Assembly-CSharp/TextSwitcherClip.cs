using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TextSwitcherClip : PlayableAsset, ITimelineClipAsset
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
		ScriptPlayable<TextSwitcherBehaviour> playable = ScriptPlayable<TextSwitcherBehaviour>.Create(graph, this.template, 0);
		return playable;
	}

	public TextSwitcherBehaviour template = new TextSwitcherBehaviour();
}
