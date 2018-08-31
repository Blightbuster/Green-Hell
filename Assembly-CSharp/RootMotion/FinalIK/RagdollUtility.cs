using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[RequireComponent(typeof(Animator))]
	public class RagdollUtility : MonoBehaviour
	{
		public void EnableRagdoll()
		{
			if (this.isRagdoll)
			{
				return;
			}
			base.StopAllCoroutines();
			this.enableRagdollFlag = true;
		}

		public void DisableRagdoll()
		{
			if (!this.isRagdoll)
			{
				return;
			}
			this.StoreLocalState();
			base.StopAllCoroutines();
			base.StartCoroutine(this.DisableRagdollSmooth());
		}

		public void Start()
		{
			this.animator = base.GetComponent<Animator>();
			this.allIKComponents = base.GetComponentsInChildren<IK>();
			this.disabledIKComponents = new bool[this.allIKComponents.Length];
			this.fixTransforms = new bool[this.allIKComponents.Length];
			if (this.ik != null)
			{
				IKSolver iksolver = this.ik.GetIKSolver();
				iksolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver.OnPostUpdate, new IKSolver.UpdateDelegate(this.AfterLastIK));
			}
			Rigidbody[] componentsInChildren = base.GetComponentsInChildren<Rigidbody>();
			int num = (!(componentsInChildren[0].gameObject == base.gameObject)) ? 0 : 1;
			this.rigidbones = new RagdollUtility.Rigidbone[(num != 0) ? (componentsInChildren.Length - 1) : componentsInChildren.Length];
			for (int i = 0; i < this.rigidbones.Length; i++)
			{
				this.rigidbones[i] = new RagdollUtility.Rigidbone(componentsInChildren[i + num]);
			}
			Transform[] componentsInChildren2 = base.GetComponentsInChildren<Transform>();
			this.children = new RagdollUtility.Child[componentsInChildren2.Length - 1];
			for (int j = 0; j < this.children.Length; j++)
			{
				this.children[j] = new RagdollUtility.Child(componentsInChildren2[j + 1]);
			}
		}

		private IEnumerator DisableRagdollSmooth()
		{
			for (int i = 0; i < this.rigidbones.Length; i++)
			{
				this.rigidbones[i].r.isKinematic = true;
			}
			for (int j = 0; j < this.allIKComponents.Length; j++)
			{
				this.allIKComponents[j].fixTransforms = this.fixTransforms[j];
				if (this.disabledIKComponents[j])
				{
					this.allIKComponents[j].enabled = true;
				}
			}
			this.animator.updateMode = this.animatorUpdateMode;
			this.animator.enabled = true;
			while (this.ragdollWeight > 0f)
			{
				this.ragdollWeight = Mathf.SmoothDamp(this.ragdollWeight, 0f, ref this.ragdollWeightV, this.ragdollToAnimationTime);
				if (this.ragdollWeight < 0.001f)
				{
					this.ragdollWeight = 0f;
				}
				yield return null;
			}
			yield return null;
			yield break;
		}

		private void Update()
		{
			if (!this.isRagdoll)
			{
				return;
			}
			if (!this.applyIkOnRagdoll)
			{
				bool flag = false;
				for (int i = 0; i < this.allIKComponents.Length; i++)
				{
					if (this.allIKComponents[i].enabled)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					for (int j = 0; j < this.allIKComponents.Length; j++)
					{
						this.disabledIKComponents[j] = false;
					}
				}
				for (int k = 0; k < this.allIKComponents.Length; k++)
				{
					if (this.allIKComponents[k].enabled)
					{
						this.allIKComponents[k].enabled = false;
						this.disabledIKComponents[k] = true;
					}
				}
			}
			else
			{
				bool flag2 = false;
				for (int l = 0; l < this.allIKComponents.Length; l++)
				{
					if (this.disabledIKComponents[l])
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					for (int m = 0; m < this.allIKComponents.Length; m++)
					{
						if (this.disabledIKComponents[m])
						{
							this.allIKComponents[m].enabled = true;
						}
					}
					for (int n = 0; n < this.allIKComponents.Length; n++)
					{
						this.disabledIKComponents[n] = false;
					}
				}
			}
		}

		private void FixedUpdate()
		{
			if (this.isRagdoll && this.applyIkOnRagdoll)
			{
				this.FixTransforms(1f);
			}
			this.fixedFrame = true;
		}

		private void LateUpdate()
		{
			if (this.animator.updateMode != AnimatorUpdateMode.AnimatePhysics || (this.animator.updateMode == AnimatorUpdateMode.AnimatePhysics && this.fixedFrame))
			{
				this.AfterAnimation();
			}
			this.fixedFrame = false;
			if (!this.ikUsed)
			{
				this.OnFinalPose();
			}
		}

		private void AfterLastIK()
		{
			if (this.ikUsed)
			{
				this.OnFinalPose();
			}
		}

		private void AfterAnimation()
		{
			if (this.isRagdoll)
			{
				this.StoreLocalState();
			}
			else
			{
				this.FixTransforms(this.ragdollWeight);
			}
		}

		private void OnFinalPose()
		{
			if (!this.isRagdoll)
			{
				this.RecordVelocities();
			}
			if (this.enableRagdollFlag)
			{
				this.RagdollEnabler();
			}
		}

		private void RagdollEnabler()
		{
			this.StoreLocalState();
			for (int i = 0; i < this.allIKComponents.Length; i++)
			{
				this.disabledIKComponents[i] = false;
			}
			if (!this.applyIkOnRagdoll)
			{
				for (int j = 0; j < this.allIKComponents.Length; j++)
				{
					if (this.allIKComponents[j].enabled)
					{
						this.allIKComponents[j].enabled = false;
						this.disabledIKComponents[j] = true;
					}
				}
			}
			this.animatorUpdateMode = this.animator.updateMode;
			this.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
			this.animator.enabled = false;
			for (int k = 0; k < this.rigidbones.Length; k++)
			{
				this.rigidbones[k].WakeUp(this.applyVelocity, this.applyAngularVelocity);
			}
			for (int l = 0; l < this.fixTransforms.Length; l++)
			{
				this.fixTransforms[l] = this.allIKComponents[l].fixTransforms;
				this.allIKComponents[l].fixTransforms = false;
			}
			this.ragdollWeight = 1f;
			this.ragdollWeightV = 0f;
			this.enableRagdollFlag = false;
		}

		private bool isRagdoll
		{
			get
			{
				return !this.rigidbones[0].r.isKinematic && !this.animator.enabled;
			}
		}

		private void RecordVelocities()
		{
			foreach (RagdollUtility.Rigidbone rigidbone in this.rigidbones)
			{
				rigidbone.RecordVelocity();
			}
		}

		private bool ikUsed
		{
			get
			{
				if (this.ik == null)
				{
					return false;
				}
				if (this.ik.enabled && this.ik.GetIKSolver().IKPositionWeight > 0f)
				{
					return true;
				}
				foreach (IK ik in this.allIKComponents)
				{
					if (ik.enabled && ik.GetIKSolver().IKPositionWeight > 0f)
					{
						return true;
					}
				}
				return false;
			}
		}

		private void StoreLocalState()
		{
			foreach (RagdollUtility.Child child in this.children)
			{
				child.StoreLocalState();
			}
		}

		private void FixTransforms(float weight)
		{
			foreach (RagdollUtility.Child child in this.children)
			{
				child.FixTransform(weight);
			}
		}

		private void OnDestroy()
		{
			if (this.ik != null)
			{
				IKSolver iksolver = this.ik.GetIKSolver();
				iksolver.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iksolver.OnPostUpdate, new IKSolver.UpdateDelegate(this.AfterLastIK));
			}
		}

		[Tooltip("If you have multiple IK components, then this should be the one that solves last each frame.")]
		public IK ik;

		[Tooltip("How long does it take to blend from ragdoll to animation?")]
		public float ragdollToAnimationTime = 0.2f;

		[Tooltip("If true, IK can be used on top of physical ragdoll simulation.")]
		public bool applyIkOnRagdoll;

		[Tooltip("How much velocity transfer from animation to ragdoll?")]
		public float applyVelocity = 1f;

		[Tooltip("How much angular velocity to transfer from animation to ragdoll?")]
		public float applyAngularVelocity = 1f;

		private Animator animator;

		private RagdollUtility.Rigidbone[] rigidbones = new RagdollUtility.Rigidbone[0];

		private RagdollUtility.Child[] children = new RagdollUtility.Child[0];

		private bool enableRagdollFlag;

		private AnimatorUpdateMode animatorUpdateMode;

		private IK[] allIKComponents = new IK[0];

		private bool[] fixTransforms = new bool[0];

		private float ragdollWeight;

		private float ragdollWeightV;

		private bool fixedFrame;

		private bool[] disabledIKComponents = new bool[0];

		public class Rigidbone
		{
			public Rigidbone(Rigidbody r)
			{
				this.r = r;
				this.t = r.transform;
				this.joint = this.t.GetComponent<Joint>();
				this.collider = this.t.GetComponent<Collider>();
				if (this.joint != null)
				{
					this.c = this.joint.connectedBody;
					this.updateAnchor = (this.c != null);
				}
				this.lastPosition = this.t.position;
				this.lastRotation = this.t.rotation;
			}

			public void RecordVelocity()
			{
				this.deltaPosition = this.t.position - this.lastPosition;
				this.lastPosition = this.t.position;
				this.deltaRotation = QuaTools.FromToRotation(this.lastRotation, this.t.rotation);
				this.lastRotation = this.t.rotation;
				this.deltaTime = Time.deltaTime;
			}

			public void WakeUp(float velocityWeight, float angularVelocityWeight)
			{
				if (this.updateAnchor)
				{
					this.joint.connectedAnchor = this.t.InverseTransformPoint(this.c.position);
				}
				this.r.isKinematic = false;
				if (velocityWeight != 0f)
				{
					this.r.velocity = this.deltaPosition / this.deltaTime * velocityWeight;
				}
				if (angularVelocityWeight != 0f)
				{
					float num = 0f;
					Vector3 vector = Vector3.zero;
					this.deltaRotation.ToAngleAxis(out num, out vector);
					num *= 0.0174532924f;
					num /= this.deltaTime;
					vector *= num * angularVelocityWeight;
					this.r.angularVelocity = Vector3.ClampMagnitude(vector, this.r.maxAngularVelocity);
				}
				this.r.WakeUp();
			}

			public Rigidbody r;

			public Transform t;

			public Collider collider;

			public Joint joint;

			public Rigidbody c;

			public bool updateAnchor;

			public Vector3 deltaPosition;

			public Quaternion deltaRotation;

			public float deltaTime;

			public Vector3 lastPosition;

			public Quaternion lastRotation;
		}

		public class Child
		{
			public Child(Transform transform)
			{
				this.t = transform;
				this.localPosition = this.t.localPosition;
				this.localRotation = this.t.localRotation;
			}

			public void FixTransform(float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				if (weight >= 1f)
				{
					this.t.localPosition = this.localPosition;
					this.t.localRotation = this.localRotation;
					return;
				}
				this.t.localPosition = Vector3.Lerp(this.t.localPosition, this.localPosition, weight);
				this.t.localRotation = Quaternion.Lerp(this.t.localRotation, this.localRotation, weight);
			}

			public void StoreLocalState()
			{
				this.localPosition = this.t.localPosition;
				this.localRotation = this.t.localRotation;
			}

			public Transform t;

			public Vector3 localPosition;

			public Quaternion localRotation;
		}
	}
}
