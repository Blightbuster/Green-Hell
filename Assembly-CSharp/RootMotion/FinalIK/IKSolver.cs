using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public abstract class IKSolver
	{
		public bool IsValid()
		{
			string empty = string.Empty;
			return this.IsValid(ref empty);
		}

		public abstract bool IsValid(ref string message);

		public void Initiate(Transform root)
		{
			if (this.OnPreInitiate != null)
			{
				this.OnPreInitiate();
			}
			if (root == null)
			{
				Debug.LogError("Initiating IKSolver with null root Transform.");
			}
			this.root = root;
			this.initiated = false;
			string empty = string.Empty;
			if (!this.IsValid(ref empty))
			{
				Warning.Log(empty, root, false);
				return;
			}
			this.OnInitiate();
			this.StoreDefaultLocalState();
			this.initiated = true;
			this.firstInitiation = false;
			if (this.OnPostInitiate != null)
			{
				this.OnPostInitiate();
			}
		}

		public void Update()
		{
			if (this.OnPreUpdate != null)
			{
				this.OnPreUpdate();
			}
			if (this.firstInitiation)
			{
				this.Initiate(this.root);
			}
			if (!this.initiated)
			{
				return;
			}
			this.OnUpdate();
			if (this.OnPostUpdate != null)
			{
				this.OnPostUpdate();
			}
		}

		public virtual Vector3 GetIKPosition()
		{
			return this.IKPosition;
		}

		public void SetIKPosition(Vector3 position)
		{
			this.IKPosition = position;
		}

		public float GetIKPositionWeight()
		{
			return this.IKPositionWeight;
		}

		public void SetIKPositionWeight(float weight)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		}

		public Transform GetRoot()
		{
			return this.root;
		}

		public bool initiated { get; private set; }

		public abstract IKSolver.Point[] GetPoints();

		public abstract IKSolver.Point GetPoint(Transform transform);

		public abstract void FixTransforms();

		public abstract void StoreDefaultLocalState();

		protected abstract void OnInitiate();

		protected abstract void OnUpdate();

		protected void LogWarning(string message)
		{
			Warning.Log(message, this.root, true);
		}

		public static Transform ContainsDuplicateBone(IKSolver.Bone[] bones)
		{
			for (int i = 0; i < bones.Length; i++)
			{
				for (int j = 0; j < bones.Length; j++)
				{
					if (i != j && bones[i].transform == bones[j].transform)
					{
						return bones[i].transform;
					}
				}
			}
			return null;
		}

		public static bool HierarchyIsValid(IKSolver.Bone[] bones)
		{
			for (int i = 1; i < bones.Length; i++)
			{
				if (!Hierarchy.IsAncestor(bones[i].transform, bones[i - 1].transform))
				{
					return false;
				}
			}
			return true;
		}

		protected static float PreSolveBones(ref IKSolver.Bone[] bones)
		{
			float num = 0f;
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i].solverPosition = bones[i].transform.position;
				bones[i].solverRotation = bones[i].transform.rotation;
			}
			for (int j = 0; j < bones.Length; j++)
			{
				if (j < bones.Length - 1)
				{
					bones[j].sqrMag = (bones[j + 1].solverPosition - bones[j].solverPosition).sqrMagnitude;
					bones[j].length = Mathf.Sqrt(bones[j].sqrMag);
					num += bones[j].length;
					bones[j].axis = Quaternion.Inverse(bones[j].solverRotation) * (bones[j + 1].solverPosition - bones[j].solverPosition);
				}
				else
				{
					bones[j].sqrMag = 0f;
					bones[j].length = 0f;
				}
			}
			return num;
		}

		[HideInInspector]
		public Vector3 IKPosition;

		[Range(0f, 1f)]
		[Tooltip("The positional or the master weight of the solver.")]
		public float IKPositionWeight = 1f;

		public IKSolver.UpdateDelegate OnPreInitiate;

		public IKSolver.UpdateDelegate OnPostInitiate;

		public IKSolver.UpdateDelegate OnPreUpdate;

		public IKSolver.UpdateDelegate OnPostUpdate;

		protected bool firstInitiation = true;

		[SerializeField]
		[HideInInspector]
		protected Transform root;

		[Serializable]
		public class Point
		{
			public void StoreDefaultLocalState()
			{
				this.defaultLocalPosition = this.transform.localPosition;
				this.defaultLocalRotation = this.transform.localRotation;
			}

			public void FixTransform()
			{
				if (this.transform.localPosition != this.defaultLocalPosition)
				{
					this.transform.localPosition = this.defaultLocalPosition;
				}
				if (this.transform.localRotation != this.defaultLocalRotation)
				{
					this.transform.localRotation = this.defaultLocalRotation;
				}
			}

			public void UpdateSolverPosition()
			{
				this.solverPosition = this.transform.position;
			}

			public void UpdateSolverLocalPosition()
			{
				this.solverPosition = this.transform.localPosition;
			}

			public void UpdateSolverState()
			{
				this.solverPosition = this.transform.position;
				this.solverRotation = this.transform.rotation;
			}

			public void UpdateSolverLocalState()
			{
				this.solverPosition = this.transform.localPosition;
				this.solverRotation = this.transform.localRotation;
			}

			public Transform transform;

			[Range(0f, 1f)]
			public float weight = 1f;

			public Vector3 solverPosition;

			public Quaternion solverRotation = Quaternion.identity;

			public Vector3 defaultLocalPosition;

			public Quaternion defaultLocalRotation;
		}

		[Serializable]
		public class Bone : IKSolver.Point
		{
			public Bone()
			{
			}

			public Bone(Transform transform)
			{
				this.transform = transform;
			}

			public Bone(Transform transform, float weight)
			{
				this.transform = transform;
				this.weight = weight;
			}

			public RotationLimit rotationLimit
			{
				get
				{
					if (!this.isLimited)
					{
						return null;
					}
					if (this._rotationLimit == null)
					{
						this._rotationLimit = this.transform.GetComponent<RotationLimit>();
					}
					this.isLimited = (this._rotationLimit != null);
					return this._rotationLimit;
				}
				set
				{
					this._rotationLimit = value;
					this.isLimited = (value != null);
				}
			}

			public void Swing(Vector3 swingTarget, float weight = 1f)
			{
				if (weight <= 0f)
				{
					return;
				}
				Quaternion quaternion = Quaternion.FromToRotation(this.transform.rotation * this.axis, swingTarget - this.transform.position);
				if (weight >= 1f)
				{
					this.transform.rotation = quaternion * this.transform.rotation;
					return;
				}
				this.transform.rotation = Quaternion.Lerp(Quaternion.identity, quaternion, weight) * this.transform.rotation;
			}

			public static void SolverSwing(IKSolver.Bone[] bones, int index, Vector3 swingTarget, float weight = 1f)
			{
				if (weight <= 0f)
				{
					return;
				}
				Quaternion quaternion = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
				if (weight >= 1f)
				{
					for (int i = index; i < bones.Length; i++)
					{
						bones[i].solverRotation = quaternion * bones[i].solverRotation;
					}
					return;
				}
				for (int j = index; j < bones.Length; j++)
				{
					bones[j].solverRotation = Quaternion.Lerp(Quaternion.identity, quaternion, weight) * bones[j].solverRotation;
				}
			}

			public void Swing2D(Vector3 swingTarget, float weight = 1f)
			{
				if (weight <= 0f)
				{
					return;
				}
				Vector3 vector = this.transform.rotation * this.axis;
				Vector3 vector2 = swingTarget - this.transform.position;
				float current = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
				float target = Mathf.Atan2(vector2.x, vector2.y) * 57.29578f;
				this.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(current, target) * weight, Vector3.back) * this.transform.rotation;
			}

			public void SetToSolverPosition()
			{
				this.transform.position = this.solverPosition;
			}

			public float length;

			public float sqrMag;

			public Vector3 axis = -Vector3.right;

			private RotationLimit _rotationLimit;

			private bool isLimited = true;
		}

		[Serializable]
		public class Node : IKSolver.Point
		{
			public Node()
			{
			}

			public Node(Transform transform)
			{
				this.transform = transform;
			}

			public Node(Transform transform, float weight)
			{
				this.transform = transform;
				this.weight = weight;
			}

			public float length;

			public float effectorPositionWeight;

			public float effectorRotationWeight;

			public Vector3 offset;
		}

		public delegate void UpdateDelegate();

		public delegate void IterationDelegate(int i);
	}
}
