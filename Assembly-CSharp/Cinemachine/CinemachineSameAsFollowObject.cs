using System;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(27f, DocumentationSortingAttribute.Level.UserRef)]
	[SaveDuringPlay]
	[RequireComponent(typeof(CinemachinePipeline))]
	[AddComponentMenu("")]
	public class CinemachineSameAsFollowObject : CinemachineComponentBase
	{
		public override bool IsValid
		{
			get
			{
				return base.enabled && base.FollowTarget != null;
			}
		}

		public override CinemachineCore.Stage Stage
		{
			get
			{
				return CinemachineCore.Stage.Aim;
			}
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			if (this.IsValid)
			{
				curState.RawOrientation = base.FollowTarget.transform.rotation;
			}
		}
	}
}
