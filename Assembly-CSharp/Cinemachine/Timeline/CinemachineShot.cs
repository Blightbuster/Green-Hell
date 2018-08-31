using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cinemachine.Timeline
{
	public sealed class CinemachineShot : PlayableAsset, IPropertyPreview
	{
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			ScriptPlayable<CinemachineShotPlayable> playable = ScriptPlayable<CinemachineShotPlayable>.Create(graph, 0);
			playable.GetBehaviour().VirtualCamera = this.VirtualCamera.Resolve(graph.GetResolver());
			return playable;
		}

		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			driver.AddFromName<Transform>("m_LocalPosition.x");
			driver.AddFromName<Transform>("m_LocalPosition.y");
			driver.AddFromName<Transform>("m_LocalPosition.z");
			driver.AddFromName<Transform>("m_LocalRotation.x");
			driver.AddFromName<Transform>("m_LocalRotation.y");
			driver.AddFromName<Transform>("m_LocalRotation.z");
			driver.AddFromName<Camera>("field of view");
			driver.AddFromName<Camera>("near clip plane");
			driver.AddFromName<Camera>("far clip plane");
		}

		public ExposedReference<CinemachineVirtualCameraBase> VirtualCamera;
	}
}
