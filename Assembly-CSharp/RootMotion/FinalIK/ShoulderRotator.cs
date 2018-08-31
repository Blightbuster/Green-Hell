using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class ShoulderRotator : MonoBehaviour
	{
		private void Start()
		{
			this.ik = base.GetComponent<FullBodyBipedIK>();
			IKSolverFullBodyBiped solver = this.ik.solver;
			solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPostUpdate, new IKSolver.UpdateDelegate(this.RotateShoulders));
		}

		private void RotateShoulders()
		{
			if (this.ik == null)
			{
				return;
			}
			if (this.ik.solver.IKPositionWeight <= 0f)
			{
				return;
			}
			if (this.skip)
			{
				this.skip = false;
				return;
			}
			this.RotateShoulder(FullBodyBipedChain.LeftArm, this.weight, this.offset);
			this.RotateShoulder(FullBodyBipedChain.RightArm, this.weight, this.offset);
			this.skip = true;
			this.ik.solver.Update();
		}

		private void RotateShoulder(FullBodyBipedChain chain, float weight, float offset)
		{
			Quaternion b = Quaternion.FromToRotation(this.GetParentBoneMap(chain).swingDirection, this.ik.solver.GetEndEffector(chain).position - this.GetParentBoneMap(chain).transform.position);
			Vector3 vector = this.ik.solver.GetEndEffector(chain).position - this.ik.solver.GetLimbMapping(chain).bone1.position;
			float num = this.ik.solver.GetChain(chain).nodes[0].length + this.ik.solver.GetChain(chain).nodes[1].length;
			float num2 = vector.magnitude / num - 1f + offset;
			num2 = Mathf.Clamp(num2 * weight, 0f, 1f);
			Quaternion lhs = Quaternion.Lerp(Quaternion.identity, b, num2 * this.ik.solver.GetEndEffector(chain).positionWeight * this.ik.solver.IKPositionWeight);
			this.ik.solver.GetLimbMapping(chain).parentBone.rotation = lhs * this.ik.solver.GetLimbMapping(chain).parentBone.rotation;
		}

		private IKMapping.BoneMap GetParentBoneMap(FullBodyBipedChain chain)
		{
			return this.ik.solver.GetLimbMapping(chain).GetBoneMap(IKMappingLimb.BoneMapType.Parent);
		}

		private void OnDestroy()
		{
			if (this.ik != null)
			{
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPostUpdate, new IKSolver.UpdateDelegate(this.RotateShoulders));
			}
		}

		[Tooltip("Weight of shoulder rotation")]
		public float weight = 1.5f;

		[Tooltip("The greater the offset, the sooner the shoulder will start rotating")]
		public float offset = 0.2f;

		private FullBodyBipedIK ik;

		private bool skip;
	}
}
