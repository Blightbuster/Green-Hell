using System;
using UnityEngine;

namespace Cinemachine
{
	[AddComponentMenu("")]
	[DocumentationSorting(23f, DocumentationSortingAttribute.Level.UserRef)]
	[RequireComponent(typeof(CinemachinePipeline))]
	[SaveDuringPlay]
	public class CinemachineHardLookAt : CinemachineComponentBase
	{
		public override bool IsValid
		{
			get
			{
				return base.enabled && base.LookAtTarget != null;
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
			if (this.IsValid && curState.HasLookAt)
			{
				Vector3 vector = curState.ReferenceLookAt - curState.CorrectedPosition;
				if (vector.magnitude > 0.0001f)
				{
					if (Vector3.Cross(vector.normalized, curState.ReferenceUp).magnitude < 0.0001f)
					{
						curState.RawOrientation = Quaternion.FromToRotation(Vector3.forward, vector);
					}
					else
					{
						curState.RawOrientation = Quaternion.LookRotation(vector, curState.ReferenceUp);
					}
				}
			}
		}
	}
}
