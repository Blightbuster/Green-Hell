using System;
using UnityEngine;

namespace Cinemachine
{
	[SaveDuringPlay]
	[RequireComponent(typeof(CinemachinePipeline))]
	[DocumentationSorting(23f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("")]
	public class CinemachineHardLockToTarget : CinemachineComponentBase
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
				return CinemachineCore.Stage.Body;
			}
		}

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			if (this.IsValid)
			{
				curState.RawPosition = base.FollowTarget.position;
			}
		}
	}
}
