using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverFABRIKRoot : IKSolver
	{
		public override bool IsValid(ref string message)
		{
			if (this.chains.Length == 0)
			{
				message = "IKSolverFABRIKRoot contains no chains.";
				return false;
			}
			foreach (FABRIKChain fabrikchain in this.chains)
			{
				if (!fabrikchain.IsValid(ref message))
				{
					return false;
				}
			}
			for (int j = 0; j < this.chains.Length; j++)
			{
				for (int k = 0; k < this.chains.Length; k++)
				{
					if (j != k && this.chains[j].ik == this.chains[k].ik)
					{
						message = this.chains[j].ik.name + " is represented more than once in IKSolverFABRIKRoot chain.";
						return false;
					}
				}
			}
			for (int l = 0; l < this.chains.Length; l++)
			{
				for (int m = 0; m < this.chains[l].children.Length; m++)
				{
					int num = this.chains[l].children[m];
					if (num < 0)
					{
						message = string.Concat(new object[]
						{
							this.chains[l].ik.name,
							"IKSolverFABRIKRoot chain at index ",
							l,
							" has invalid children array. Child index is < 0."
						});
						return false;
					}
					if (num == l)
					{
						message = string.Concat(new object[]
						{
							this.chains[l].ik.name,
							"IKSolverFABRIKRoot chain at index ",
							l,
							" has invalid children array. Child index is referencing to itself."
						});
						return false;
					}
					if (num >= this.chains.Length)
					{
						message = string.Concat(new object[]
						{
							this.chains[l].ik.name,
							"IKSolverFABRIKRoot chain at index ",
							l,
							" has invalid children array. Child index > number of chains"
						});
						return false;
					}
					for (int n = 0; n < this.chains.Length; n++)
					{
						if (num == n)
						{
							for (int num2 = 0; num2 < this.chains[n].children.Length; num2++)
							{
								if (this.chains[n].children[num2] == l)
								{
									message = string.Concat(new string[]
									{
										"Circular parenting. ",
										this.chains[n].ik.name,
										" already has ",
										this.chains[l].ik.name,
										" listed as it's child."
									});
									return false;
								}
							}
						}
					}
					for (int num3 = 0; num3 < this.chains[l].children.Length; num3++)
					{
						if (m != num3 && this.chains[l].children[num3] == num)
						{
							message = string.Concat(new object[]
							{
								"Chain number ",
								num,
								" is represented more than once in the children of ",
								this.chains[l].ik.name
							});
							return false;
						}
					}
				}
			}
			return true;
		}

		public override void StoreDefaultLocalState()
		{
			this.rootDefaultPosition = this.root.localPosition;
			for (int i = 0; i < this.chains.Length; i++)
			{
				this.chains[i].ik.solver.StoreDefaultLocalState();
			}
		}

		public override void FixTransforms()
		{
			this.root.localPosition = this.rootDefaultPosition;
			for (int i = 0; i < this.chains.Length; i++)
			{
				this.chains[i].ik.solver.FixTransforms();
			}
		}

		protected override void OnInitiate()
		{
			for (int i = 0; i < this.chains.Length; i++)
			{
				this.chains[i].Initiate();
			}
			this.isRoot = new bool[this.chains.Length];
			for (int j = 0; j < this.chains.Length; j++)
			{
				this.isRoot[j] = this.IsRoot(j);
			}
		}

		private bool IsRoot(int index)
		{
			for (int i = 0; i < this.chains.Length; i++)
			{
				for (int j = 0; j < this.chains[i].children.Length; j++)
				{
					if (this.chains[i].children[j] == index)
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override void OnUpdate()
		{
			if (this.IKPositionWeight <= 0f && this.zeroWeightApplied)
			{
				return;
			}
			this.IKPositionWeight = Mathf.Clamp(this.IKPositionWeight, 0f, 1f);
			for (int i = 0; i < this.chains.Length; i++)
			{
				this.chains[i].ik.solver.IKPositionWeight = this.IKPositionWeight;
			}
			if (this.IKPositionWeight <= 0f)
			{
				this.zeroWeightApplied = true;
				return;
			}
			this.zeroWeightApplied = false;
			for (int j = 0; j < this.iterations; j++)
			{
				for (int k = 0; k < this.chains.Length; k++)
				{
					if (this.isRoot[k])
					{
						this.chains[k].Stage1(this.chains);
					}
				}
				Vector3 centroid = this.GetCentroid();
				this.root.position = centroid;
				for (int l = 0; l < this.chains.Length; l++)
				{
					if (this.isRoot[l])
					{
						this.chains[l].Stage2(centroid, this.chains);
					}
				}
			}
		}

		public override IKSolver.Point[] GetPoints()
		{
			IKSolver.Point[] result = new IKSolver.Point[0];
			for (int i = 0; i < this.chains.Length; i++)
			{
				this.AddPointsToArray(ref result, this.chains[i]);
			}
			return result;
		}

		public override IKSolver.Point GetPoint(Transform transform)
		{
			for (int i = 0; i < this.chains.Length; i++)
			{
				IKSolver.Point point = this.chains[i].ik.solver.GetPoint(transform);
				if (point != null)
				{
					return point;
				}
			}
			return null;
		}

		private void AddPointsToArray(ref IKSolver.Point[] array, FABRIKChain chain)
		{
			IKSolver.Point[] points = chain.ik.solver.GetPoints();
			Array.Resize<IKSolver.Point>(ref array, array.Length + points.Length);
			int num = 0;
			for (int i = array.Length - points.Length; i < array.Length; i++)
			{
				array[i] = points[num];
				num++;
			}
		}

		private Vector3 GetCentroid()
		{
			Vector3 vector = this.root.position;
			if (this.rootPin >= 1f)
			{
				return vector;
			}
			float num = 0f;
			for (int i = 0; i < this.chains.Length; i++)
			{
				if (this.isRoot[i])
				{
					num += this.chains[i].pull;
				}
			}
			for (int j = 0; j < this.chains.Length; j++)
			{
				if (this.isRoot[j] && num > 0f)
				{
					vector += (this.chains[j].ik.solver.bones[0].solverPosition - this.root.position) * (this.chains[j].pull / Mathf.Clamp(num, 1f, num));
				}
			}
			return Vector3.Lerp(vector, this.root.position, this.rootPin);
		}

		public int iterations = 4;

		[Range(0f, 1f)]
		public float rootPin;

		public FABRIKChain[] chains = new FABRIKChain[0];

		private bool zeroWeightApplied;

		private bool[] isRoot;

		private Vector3 rootDefaultPosition;
	}
}
