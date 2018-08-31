using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cinemachine.Timeline
{
	[TrackBindingType(typeof(CinemachineBrain))]
	[TrackColor(0.53f, 0f, 0.08f)]
	[TrackMediaType(TimelineAsset.MediaType.Script)]
	[TrackClipType(typeof(CinemachineShot))]
	[Serializable]
	public class CinemachineTrack : TrackAsset
	{
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			foreach (TimelineClip timelineClip in base.GetClips())
			{
				CinemachineShot cinemachineShot = (CinemachineShot)timelineClip.asset;
				CinemachineVirtualCameraBase cinemachineVirtualCameraBase = cinemachineShot.VirtualCamera.Resolve(graph.GetResolver());
				if (cinemachineVirtualCameraBase != null)
				{
					timelineClip.displayName = cinemachineVirtualCameraBase.Name;
				}
			}
			ScriptPlayable<CinemachineMixer> playable = ScriptPlayable<CinemachineMixer>.Create(graph, 0);
			playable.SetInputCount(inputCount);
			return playable;
		}
	}
}
