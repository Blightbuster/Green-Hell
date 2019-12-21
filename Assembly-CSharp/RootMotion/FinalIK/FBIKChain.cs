using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class FBIKChain
	{
		public FBIKChain()
		{
		}

		public FBIKChain(float pin, float pull, params Transform[] nodeTransforms)
		{
			this.pin = pin;
			this.pull = pull;
			this.SetNodes(nodeTransforms);
			this.children = new int[0];
		}

		public void SetNodes(params Transform[] boneTransforms)
		{
			this.nodes = new IKSolver.Node[boneTransforms.Length];
			for (int i = 0; i < boneTransforms.Length; i++)
			{
				this.nodes[i] = new IKSolver.Node(boneTransforms[i]);
			}
		}

		public int GetNodeIndex(Transform boneTransform)
		{
			for (int i = 0; i < this.nodes.Length; i++)
			{
				if (this.nodes[i].transform == boneTransform)
				{
					return i;
				}
			}
			return -1;
		}

		public bool IsValid(ref string message)
		{
			if (this.nodes.Length == 0)
			{
				message = "FBIK chain contains no nodes.";
				return false;
			}
			IKSolver.Node[] array = this.nodes;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform == null)
				{
					message = "Node transform is null in FBIK chain.";
					return false;
				}
			}
			return true;
		}

		public void Initiate(IKSolverFullBody solver)
		{
			this.initiated = false;
			foreach (IKSolver.Node node in this.nodes)
			{
				node.solverPosition = node.transform.position;
			}
			this.CalculateBoneLengths(solver);
			FBIKChain.ChildConstraint[] array2 = this.childConstraints;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Initiate(solver);
			}
			if (this.nodes.Length == 3)
			{
				this.bendConstraint.SetBones(this.nodes[0].transform, this.nodes[1].transform, this.nodes[2].transform);
				this.bendConstraint.Initiate(solver);
			}
			this.crossFades = new float[this.children.Length];
			this.initiated = true;
		}

		public void ReadPose(IKSolverFullBody solver, bool fullBody)
		{
			if (!this.initiated)
			{
				return;
			}
			for (int i = 0; i < this.nodes.Length; i++)
			{
				this.nodes[i].solverPosition = this.nodes[i].transform.position + this.nodes[i].offset;
			}
			this.CalculateBoneLengths(solver);
			if (fullBody)
			{
				for (int j = 0; j < this.childConstraints.Length; j++)
				{
					this.childConstraints[j].OnPreSolve(solver);
				}
				if (this.children.Length != 0)
				{
					float num = this.nodes[this.nodes.Length - 1].effectorPositionWeight;
					for (int k = 0; k < this.children.Length; k++)
					{
						num += solver.chain[this.children[k]].nodes[0].effectorPositionWeight * solver.chain[this.children[k]].pull;
					}
					num = Mathf.Clamp(num, 1f, float.PositiveInfinity);
					for (int l = 0; l < this.children.Length; l++)
					{
						this.crossFades[l] = solver.chain[this.children[l]].nodes[0].effectorPositionWeight * solver.chain[this.children[l]].pull / num;
					}
				}
				this.pullParentSum = 0f;
				for (int m = 0; m < this.children.Length; m++)
				{
					this.pullParentSum += solver.chain[this.children[m]].pull;
				}
				this.pullParentSum = Mathf.Clamp(this.pullParentSum, 1f, float.PositiveInfinity);
				if (this.nodes.Length == 3)
				{
					this.reachForce = this.reach * Mathf.Clamp(this.nodes[2].effectorPositionWeight, 0f, 1f);
				}
				else
				{
					this.reachForce = 0f;
				}
				if (this.push > 0f && this.nodes.Length > 1)
				{
					this.distance = Vector3.Distance(this.nodes[0].transform.position, this.nodes[this.nodes.Length - 1].transform.position);
				}
			}
		}

		private void CalculateBoneLengths(IKSolverFullBody solver)
		{
			this.length = 0f;
			for (int i = 0; i < this.nodes.Length - 1; i++)
			{
				this.nodes[i].length = Vector3.Distance(this.nodes[i].transform.position, this.nodes[i + 1].transform.position);
				this.length += this.nodes[i].length;
				if (this.nodes[i].length == 0f)
				{
					Warning.Log(string.Concat(new string[]
					{
						"Bone ",
						this.nodes[i].transform.name,
						" - ",
						this.nodes[i + 1].transform.name,
						" length is zero, can not solve."
					}), this.nodes[i].transform, false);
					return;
				}
			}
			for (int j = 0; j < this.children.Length; j++)
			{
				solver.chain[this.children[j]].rootLength = (solver.chain[this.children[j]].nodes[0].transform.position - this.nodes[this.nodes.Length - 1].transform.position).magnitude;
				if (solver.chain[this.children[j]].rootLength == 0f)
				{
					return;
				}
			}
			if (this.nodes.Length == 3)
			{
				this.sqrMag1 = this.nodes[0].length * this.nodes[0].length;
				this.sqrMag2 = this.nodes[1].length * this.nodes[1].length;
				this.sqrMagDif = this.sqrMag1 - this.sqrMag2;
			}
		}

		public void Reach(IKSolverFullBody solver)
		{
			if (!this.initiated)
			{
				return;
			}
			for (int i = 0; i < this.children.Length; i++)
			{
				solver.chain[this.children[i]].Reach(solver);
			}
			if (this.reachForce <= 0f)
			{
				return;
			}
			Vector3 vector = this.nodes[2].solverPosition - this.nodes[0].solverPosition;
			if (vector == Vector3.zero)
			{
				return;
			}
			float magnitude = vector.magnitude;
			Vector3 a = vector / magnitude * this.length;
			float num = Mathf.Clamp(magnitude / this.length, 1f - this.reachForce, 1f + this.reachForce) - 1f;
			num = Mathf.Clamp(num + this.reachForce, -1f, 1f);
			FBIKChain.Smoothing smoothing = this.reachSmoothing;
			if (smoothing != FBIKChain.Smoothing.Exponential)
			{
				if (smoothing == FBIKChain.Smoothing.Cubic)
				{
					num *= num * num;
				}
			}
			else
			{
				num *= num;
			}
			Vector3 vector2 = a * Mathf.Clamp(num, 0f, magnitude);
			this.nodes[0].solverPosition += vector2 * (1f - this.nodes[0].effectorPositionWeight);
			this.nodes[2].solverPosition += vector2;
		}

		public Vector3 Push(IKSolverFullBody solver)
		{
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < this.children.Length; i++)
			{
				vector += solver.chain[this.children[i]].Push(solver) * solver.chain[this.children[i]].pushParent;
			}
			this.nodes[this.nodes.Length - 1].solverPosition += vector;
			if (this.nodes.Length < 2)
			{
				return Vector3.zero;
			}
			if (this.push <= 0f)
			{
				return Vector3.zero;
			}
			Vector3 a = this.nodes[2].solverPosition - this.nodes[0].solverPosition;
			float magnitude = a.magnitude;
			if (magnitude == 0f)
			{
				return Vector3.zero;
			}
			float num = 1f - magnitude / this.distance;
			if (num <= 0f)
			{
				return Vector3.zero;
			}
			FBIKChain.Smoothing smoothing = this.pushSmoothing;
			if (smoothing != FBIKChain.Smoothing.Exponential)
			{
				if (smoothing == FBIKChain.Smoothing.Cubic)
				{
					num *= num * num;
				}
			}
			else
			{
				num *= num;
			}
			Vector3 vector2 = -a * num * this.push;
			this.nodes[0].solverPosition += vector2;
			return vector2;
		}

		public void SolveTrigonometric(IKSolverFullBody solver, bool calculateBendDirection = false)
		{
			if (!this.initiated)
			{
				return;
			}
			for (int i = 0; i < this.children.Length; i++)
			{
				solver.chain[this.children[i]].SolveTrigonometric(solver, calculateBendDirection);
			}
			if (this.nodes.Length != 3)
			{
				return;
			}
			Vector3 a = this.nodes[2].solverPosition - this.nodes[0].solverPosition;
			float magnitude = a.magnitude;
			if (magnitude == 0f)
			{
				return;
			}
			float num = Mathf.Clamp(magnitude, 0f, this.length * 0.99999f);
			Vector3 direction = a / magnitude * num;
			Vector3 bendDirection = (calculateBendDirection && this.bendConstraint.initiated) ? this.bendConstraint.GetDir(solver) : (this.nodes[1].solverPosition - this.nodes[0].solverPosition);
			Vector3 dirToBendPoint = this.GetDirToBendPoint(direction, bendDirection, num);
			this.nodes[1].solverPosition = this.nodes[0].solverPosition + dirToBendPoint;
		}

		public void Stage1(IKSolverFullBody solver)
		{
			for (int i = 0; i < this.children.Length; i++)
			{
				solver.chain[this.children[i]].Stage1(solver);
			}
			if (this.children.Length == 0)
			{
				this.ForwardReach(this.nodes[this.nodes.Length - 1].solverPosition);
				return;
			}
			Vector3 a = this.nodes[this.nodes.Length - 1].solverPosition;
			this.SolveChildConstraints(solver);
			for (int j = 0; j < this.children.Length; j++)
			{
				Vector3 a2 = solver.chain[this.children[j]].nodes[0].solverPosition;
				if (solver.chain[this.children[j]].rootLength > 0f)
				{
					a2 = this.SolveFABRIKJoint(this.nodes[this.nodes.Length - 1].solverPosition, solver.chain[this.children[j]].nodes[0].solverPosition, solver.chain[this.children[j]].rootLength);
				}
				if (this.pullParentSum > 0f)
				{
					a += (a2 - this.nodes[this.nodes.Length - 1].solverPosition) * (solver.chain[this.children[j]].pull / this.pullParentSum);
				}
			}
			this.ForwardReach(Vector3.Lerp(a, this.nodes[this.nodes.Length - 1].solverPosition, this.pin));
		}

		public void Stage2(IKSolverFullBody solver, Vector3 position)
		{
			this.BackwardReach(position);
			int num = Mathf.Clamp(solver.iterations, 2, 4);
			if (this.childConstraints.Length != 0)
			{
				for (int i = 0; i < num; i++)
				{
					this.SolveConstraintSystems(solver);
				}
			}
			for (int j = 0; j < this.children.Length; j++)
			{
				solver.chain[this.children[j]].Stage2(solver, this.nodes[this.nodes.Length - 1].solverPosition);
			}
		}

		public void SolveConstraintSystems(IKSolverFullBody solver)
		{
			this.SolveChildConstraints(solver);
			for (int i = 0; i < this.children.Length; i++)
			{
				this.SolveLinearConstraint(this.nodes[this.nodes.Length - 1], solver.chain[this.children[i]].nodes[0], this.crossFades[i], solver.chain[this.children[i]].rootLength);
			}
		}

		private Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
		{
			return pos2 + (pos1 - pos2).normalized * length;
		}

		protected Vector3 GetDirToBendPoint(Vector3 direction, Vector3 bendDirection, float directionMagnitude)
		{
			float num = (directionMagnitude * directionMagnitude + this.sqrMagDif) / 2f / directionMagnitude;
			float y = (float)Math.Sqrt((double)Mathf.Clamp(this.sqrMag1 - num * num, 0f, float.PositiveInfinity));
			if (direction == Vector3.zero)
			{
				return Vector3.zero;
			}
			return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, num);
		}

		private void SolveChildConstraints(IKSolverFullBody solver)
		{
			for (int i = 0; i < this.childConstraints.Length; i++)
			{
				this.childConstraints[i].Solve(solver);
			}
		}

		private void SolveLinearConstraint(IKSolver.Node node1, IKSolver.Node node2, float crossFade, float distance)
		{
			Vector3 a = node2.solverPosition - node1.solverPosition;
			float magnitude = a.magnitude;
			if (distance == magnitude)
			{
				return;
			}
			if (magnitude == 0f)
			{
				return;
			}
			Vector3 a2 = a * (1f - distance / magnitude);
			node1.solverPosition += a2 * crossFade;
			node2.solverPosition -= a2 * (1f - crossFade);
		}

		public void ForwardReach(Vector3 position)
		{
			this.nodes[this.nodes.Length - 1].solverPosition = position;
			for (int i = this.nodes.Length - 2; i > -1; i--)
			{
				this.nodes[i].solverPosition = this.SolveFABRIKJoint(this.nodes[i].solverPosition, this.nodes[i + 1].solverPosition, this.nodes[i].length);
			}
		}

		private void BackwardReach(Vector3 position)
		{
			if (this.rootLength > 0f)
			{
				position = this.SolveFABRIKJoint(this.nodes[0].solverPosition, position, this.rootLength);
			}
			this.nodes[0].solverPosition = position;
			for (int i = 1; i < this.nodes.Length; i++)
			{
				this.nodes[i].solverPosition = this.SolveFABRIKJoint(this.nodes[i].solverPosition, this.nodes[i - 1].solverPosition, this.nodes[i - 1].length);
			}
		}

		[Range(0f, 1f)]
		public float pin;

		[Range(0f, 1f)]
		public float pull = 1f;

		[Range(0f, 1f)]
		public float push;

		[Range(-1f, 1f)]
		public float pushParent;

		[Range(0f, 1f)]
		public float reach = 0.1f;

		public FBIKChain.Smoothing reachSmoothing = FBIKChain.Smoothing.Exponential;

		public FBIKChain.Smoothing pushSmoothing = FBIKChain.Smoothing.Exponential;

		public IKSolver.Node[] nodes = new IKSolver.Node[0];

		public int[] children = new int[0];

		public FBIKChain.ChildConstraint[] childConstraints = new FBIKChain.ChildConstraint[0];

		public IKConstraintBend bendConstraint = new IKConstraintBend();

		private float rootLength;

		private bool initiated;

		private float length;

		private float distance;

		private IKSolver.Point p;

		private float reachForce;

		private float pullParentSum;

		private float[] crossFades;

		private float sqrMag1;

		private float sqrMag2;

		private float sqrMagDif;

		private const float maxLimbLength = 0.99999f;

		[Serializable]
		public class ChildConstraint
		{
			public float nominalDistance { get; private set; }

			public bool isRigid { get; private set; }

			public ChildConstraint(Transform bone1, Transform bone2, float pushElasticity = 0f, float pullElasticity = 0f)
			{
				this.bone1 = bone1;
				this.bone2 = bone2;
				this.pushElasticity = pushElasticity;
				this.pullElasticity = pullElasticity;
			}

			public void Initiate(IKSolverFullBody solver)
			{
				this.chain1Index = solver.GetChainIndex(this.bone1);
				this.chain2Index = solver.GetChainIndex(this.bone2);
				this.OnPreSolve(solver);
			}

			public void OnPreSolve(IKSolverFullBody solver)
			{
				this.nominalDistance = Vector3.Distance(solver.chain[this.chain1Index].nodes[0].transform.position, solver.chain[this.chain2Index].nodes[0].transform.position);
				this.isRigid = (this.pushElasticity <= 0f && this.pullElasticity <= 0f);
				if (this.isRigid)
				{
					float num = solver.chain[this.chain1Index].pull - solver.chain[this.chain2Index].pull;
					this.crossFade = 1f - (0.5f + num * 0.5f);
				}
				else
				{
					this.crossFade = 0.5f;
				}
				this.inverseCrossFade = 1f - this.crossFade;
			}

			public void Solve(IKSolverFullBody solver)
			{
				if (this.pushElasticity >= 1f && this.pullElasticity >= 1f)
				{
					return;
				}
				Vector3 a = solver.chain[this.chain2Index].nodes[0].solverPosition - solver.chain[this.chain1Index].nodes[0].solverPosition;
				float magnitude = a.magnitude;
				if (magnitude == this.nominalDistance)
				{
					return;
				}
				if (magnitude == 0f)
				{
					return;
				}
				float num = 1f;
				if (!this.isRigid)
				{
					float num2 = (magnitude > this.nominalDistance) ? this.pullElasticity : this.pushElasticity;
					num = 1f - num2;
				}
				num *= 1f - this.nominalDistance / magnitude;
				Vector3 a2 = a * num;
				solver.chain[this.chain1Index].nodes[0].solverPosition += a2 * this.crossFade;
				solver.chain[this.chain2Index].nodes[0].solverPosition -= a2 * this.inverseCrossFade;
			}

			public float pushElasticity;

			public float pullElasticity;

			[SerializeField]
			private Transform bone1;

			[SerializeField]
			private Transform bone2;

			private float crossFade;

			private float inverseCrossFade;

			private int chain1Index;

			private int chain2Index;
		}

		[Serializable]
		public enum Smoothing
		{
			None,
			Exponential,
			Cubic
		}
	}
}
