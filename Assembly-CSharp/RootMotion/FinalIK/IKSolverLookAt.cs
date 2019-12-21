using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverLookAt : IKSolver
	{
		public void SetLookAtWeight(float weight)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
		}

		public void SetLookAtWeight(float weight, float bodyWeight)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
		}

		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
		}

		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
			this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
		}

		public void SetLookAtWeight(float weight, float bodyWeight, float headWeight, float eyesWeight, float clampWeight)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
			this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
			this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
			this.clampWeightHead = this.clampWeight;
			this.clampWeightEyes = this.clampWeight;
		}

		public void SetLookAtWeight(float weight, float bodyWeight = 0f, float headWeight = 1f, float eyesWeight = 0.5f, float clampWeight = 0.5f, float clampWeightHead = 0.5f, float clampWeightEyes = 0.3f)
		{
			this.IKPositionWeight = Mathf.Clamp(weight, 0f, 1f);
			this.bodyWeight = Mathf.Clamp(bodyWeight, 0f, 1f);
			this.headWeight = Mathf.Clamp(headWeight, 0f, 1f);
			this.eyesWeight = Mathf.Clamp(eyesWeight, 0f, 1f);
			this.clampWeight = Mathf.Clamp(clampWeight, 0f, 1f);
			this.clampWeightHead = Mathf.Clamp(clampWeightHead, 0f, 1f);
			this.clampWeightEyes = Mathf.Clamp(clampWeightEyes, 0f, 1f);
		}

		public override void StoreDefaultLocalState()
		{
			for (int i = 0; i < this.spine.Length; i++)
			{
				this.spine[i].StoreDefaultLocalState();
			}
			for (int j = 0; j < this.eyes.Length; j++)
			{
				this.eyes[j].StoreDefaultLocalState();
			}
			if (this.head != null && this.head.transform != null)
			{
				this.head.StoreDefaultLocalState();
			}
		}

		public override void FixTransforms()
		{
			if (this.IKPositionWeight <= 0f)
			{
				return;
			}
			for (int i = 0; i < this.spine.Length; i++)
			{
				this.spine[i].FixTransform();
			}
			for (int j = 0; j < this.eyes.Length; j++)
			{
				this.eyes[j].FixTransform();
			}
			if (this.head != null && this.head.transform != null)
			{
				this.head.FixTransform();
			}
		}

		public override bool IsValid(ref string message)
		{
			if (!this.spineIsValid)
			{
				message = "IKSolverLookAt spine setup is invalid. Can't initiate solver.";
				return false;
			}
			if (!this.headIsValid)
			{
				message = "IKSolverLookAt head transform is null. Can't initiate solver.";
				return false;
			}
			if (!this.eyesIsValid)
			{
				message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
				return false;
			}
			if (this.spineIsEmpty && this.headIsEmpty && this.eyesIsEmpty)
			{
				message = "IKSolverLookAt eyes setup is invalid. Can't initiate solver.";
				return false;
			}
			IKSolver.Bone[] bones = this.spine;
			Transform transform = IKSolver.ContainsDuplicateBone(bones);
			if (transform != null)
			{
				message = transform.name + " is represented multiple times in a single IK chain. Can't initiate solver.";
				return false;
			}
			bones = this.eyes;
			Transform transform2 = IKSolver.ContainsDuplicateBone(bones);
			if (transform2 != null)
			{
				message = transform2.name + " is represented multiple times in a single IK chain. Can't initiate solver.";
				return false;
			}
			return true;
		}

		public override IKSolver.Point[] GetPoints()
		{
			IKSolver.Point[] array = new IKSolver.Point[this.spine.Length + this.eyes.Length + ((this.head.transform != null) ? 1 : 0)];
			for (int i = 0; i < this.spine.Length; i++)
			{
				array[i] = this.spine[i];
			}
			int num = 0;
			for (int j = this.spine.Length; j < array.Length; j++)
			{
				array[j] = this.eyes[num];
				num++;
			}
			if (this.head.transform != null)
			{
				array[array.Length - 1] = this.head;
			}
			return array;
		}

		public override IKSolver.Point GetPoint(Transform transform)
		{
			foreach (IKSolverLookAt.LookAtBone lookAtBone in this.spine)
			{
				if (lookAtBone.transform == transform)
				{
					return lookAtBone;
				}
			}
			foreach (IKSolverLookAt.LookAtBone lookAtBone2 in this.eyes)
			{
				if (lookAtBone2.transform == transform)
				{
					return lookAtBone2;
				}
			}
			if (this.head.transform == transform)
			{
				return this.head;
			}
			return null;
		}

		public bool SetChain(Transform[] spine, Transform head, Transform[] eyes, Transform root)
		{
			this.SetBones(spine, ref this.spine);
			this.head = new IKSolverLookAt.LookAtBone(head);
			this.SetBones(eyes, ref this.eyes);
			base.Initiate(root);
			return base.initiated;
		}

		protected override void OnInitiate()
		{
			if (this.firstInitiation || !Application.isPlaying)
			{
				if (this.spine.Length != 0)
				{
					this.IKPosition = this.spine[this.spine.Length - 1].transform.position + this.root.forward * 3f;
				}
				else if (this.head.transform != null)
				{
					this.IKPosition = this.head.transform.position + this.root.forward * 3f;
				}
				else if (this.eyes.Length != 0 && this.eyes[0].transform != null)
				{
					this.IKPosition = this.eyes[0].transform.position + this.root.forward * 3f;
				}
			}
			IKSolverLookAt.LookAtBone[] array = this.spine;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Initiate(this.root);
			}
			if (this.head != null)
			{
				this.head.Initiate(this.root);
			}
			array = this.eyes;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Initiate(this.root);
			}
			if (this.spineForwards == null || this.spineForwards.Length != this.spine.Length)
			{
				this.spineForwards = new Vector3[this.spine.Length];
			}
			if (this.headForwards == null)
			{
				this.headForwards = new Vector3[1];
			}
			if (this.eyeForward == null)
			{
				this.eyeForward = new Vector3[1];
			}
		}

		protected override void OnUpdate()
		{
			if (this.IKPositionWeight <= 0f)
			{
				return;
			}
			this.IKPositionWeight = Mathf.Clamp(this.IKPositionWeight, 0f, 1f);
			if (this.target != null)
			{
				this.IKPosition = this.target.position;
			}
			this.SolveSpine();
			this.SolveHead();
			this.SolveEyes();
		}

		private bool spineIsValid
		{
			get
			{
				if (this.spine == null)
				{
					return false;
				}
				if (this.spine.Length == 0)
				{
					return true;
				}
				for (int i = 0; i < this.spine.Length; i++)
				{
					if (this.spine[i] == null || this.spine[i].transform == null)
					{
						return false;
					}
				}
				return true;
			}
		}

		private bool spineIsEmpty
		{
			get
			{
				return this.spine.Length == 0;
			}
		}

		private void SolveSpine()
		{
			if (this.bodyWeight <= 0f)
			{
				return;
			}
			if (this.spineIsEmpty)
			{
				return;
			}
			Vector3 normalized = (this.IKPosition - this.spine[this.spine.Length - 1].transform.position).normalized;
			this.GetForwards(ref this.spineForwards, this.spine[0].forward, normalized, this.spine.Length, this.clampWeight);
			for (int i = 0; i < this.spine.Length; i++)
			{
				this.spine[i].LookAt(this.spineForwards[i], this.bodyWeight * this.IKPositionWeight);
			}
		}

		private bool headIsValid
		{
			get
			{
				return this.head != null;
			}
		}

		private bool headIsEmpty
		{
			get
			{
				return this.head.transform == null;
			}
		}

		private void SolveHead()
		{
			if (this.headWeight <= 0f)
			{
				return;
			}
			if (this.headIsEmpty)
			{
				return;
			}
			Vector3 vector = (this.spine.Length != 0 && this.spine[this.spine.Length - 1].transform != null) ? this.spine[this.spine.Length - 1].forward : this.head.forward;
			Vector3 normalized = Vector3.Lerp(vector, (this.IKPosition - this.head.transform.position).normalized, this.headWeight * this.IKPositionWeight).normalized;
			this.GetForwards(ref this.headForwards, vector, normalized, 1, this.clampWeightHead);
			this.head.LookAt(this.headForwards[0], this.headWeight * this.IKPositionWeight);
		}

		private bool eyesIsValid
		{
			get
			{
				if (this.eyes == null)
				{
					return false;
				}
				if (this.eyes.Length == 0)
				{
					return true;
				}
				for (int i = 0; i < this.eyes.Length; i++)
				{
					if (this.eyes[i] == null || this.eyes[i].transform == null)
					{
						return false;
					}
				}
				return true;
			}
		}

		private bool eyesIsEmpty
		{
			get
			{
				return this.eyes.Length == 0;
			}
		}

		private void SolveEyes()
		{
			if (this.eyesWeight <= 0f)
			{
				return;
			}
			if (this.eyesIsEmpty)
			{
				return;
			}
			for (int i = 0; i < this.eyes.Length; i++)
			{
				Vector3 baseForward = (this.head.transform != null) ? this.head.forward : this.eyes[i].forward;
				this.GetForwards(ref this.eyeForward, baseForward, (this.IKPosition - this.eyes[i].transform.position).normalized, 1, this.clampWeightEyes);
				this.eyes[i].LookAt(this.eyeForward[0], this.eyesWeight * this.IKPositionWeight);
			}
		}

		private Vector3[] GetForwards(ref Vector3[] forwards, Vector3 baseForward, Vector3 targetForward, int bones, float clamp)
		{
			if (clamp >= 1f || this.IKPositionWeight <= 0f)
			{
				for (int i = 0; i < forwards.Length; i++)
				{
					forwards[i] = baseForward;
				}
				return forwards;
			}
			float num = Vector3.Angle(baseForward, targetForward);
			float num2 = 1f - num / 180f;
			float num3 = (clamp > 0f) ? Mathf.Clamp(1f - (clamp - num2) / (1f - num2), 0f, 1f) : 1f;
			float num4 = (clamp > 0f) ? Mathf.Clamp(num2 / clamp, 0f, 1f) : 1f;
			for (int j = 0; j < this.clampSmoothing; j++)
			{
				num4 = Mathf.Sin(num4 * 3.14159274f * 0.5f);
			}
			if (forwards.Length == 1)
			{
				forwards[0] = Vector3.Slerp(baseForward, targetForward, num4 * num3);
			}
			else
			{
				float num5 = 1f / (float)(forwards.Length - 1);
				for (int k = 0; k < forwards.Length; k++)
				{
					forwards[k] = Vector3.Slerp(baseForward, targetForward, this.spineWeightCurve.Evaluate(num5 * (float)k) * num4 * num3);
				}
			}
			return forwards;
		}

		private void SetBones(Transform[] array, ref IKSolverLookAt.LookAtBone[] bones)
		{
			if (array == null)
			{
				bones = new IKSolverLookAt.LookAtBone[0];
				return;
			}
			if (bones.Length != array.Length)
			{
				bones = new IKSolverLookAt.LookAtBone[array.Length];
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (bones[i] == null)
				{
					bones[i] = new IKSolverLookAt.LookAtBone(array[i]);
				}
				else
				{
					bones[i].transform = array[i];
				}
			}
		}

		public Transform target;

		public IKSolverLookAt.LookAtBone[] spine = new IKSolverLookAt.LookAtBone[0];

		public IKSolverLookAt.LookAtBone head = new IKSolverLookAt.LookAtBone();

		public IKSolverLookAt.LookAtBone[] eyes = new IKSolverLookAt.LookAtBone[0];

		[Range(0f, 1f)]
		public float bodyWeight = 0.5f;

		[Range(0f, 1f)]
		public float headWeight = 0.5f;

		[Range(0f, 1f)]
		public float eyesWeight = 1f;

		[Range(0f, 1f)]
		public float clampWeight = 0.5f;

		[Range(0f, 1f)]
		public float clampWeightHead = 0.5f;

		[Range(0f, 1f)]
		public float clampWeightEyes = 0.5f;

		[Range(0f, 2f)]
		public int clampSmoothing = 2;

		public AnimationCurve spineWeightCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0.3f),
			new Keyframe(1f, 1f)
		});

		private Vector3[] spineForwards = new Vector3[0];

		private Vector3[] headForwards = new Vector3[1];

		private Vector3[] eyeForward = new Vector3[1];

		[Serializable]
		public class LookAtBone : IKSolver.Bone
		{
			public LookAtBone()
			{
			}

			public LookAtBone(Transform transform)
			{
				this.transform = transform;
			}

			public void Initiate(Transform root)
			{
				if (this.transform == null)
				{
					return;
				}
				this.axis = Quaternion.Inverse(this.transform.rotation) * root.forward;
			}

			public void LookAt(Vector3 direction, float weight)
			{
				Quaternion lhs = Quaternion.FromToRotation(this.forward, direction);
				Quaternion rotation = this.transform.rotation;
				this.transform.rotation = Quaternion.Lerp(rotation, lhs * rotation, weight);
			}

			public Vector3 forward
			{
				get
				{
					return this.transform.rotation * this.axis;
				}
			}
		}
	}
}
