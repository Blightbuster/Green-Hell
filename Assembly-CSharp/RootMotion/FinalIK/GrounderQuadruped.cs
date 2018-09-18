using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("http://www.root-motion.com/finalikdox/html/page11.html")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Grounder/Grounder Quadruped")]
	public class GrounderQuadruped : Grounder
	{
		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page11.html");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_grounder_quadruped.html");
		}

		public override void ResetPosition()
		{
			this.solver.Reset();
			this.forelegSolver.Reset();
		}

		private bool IsReadyToInitiate()
		{
			return !(this.pelvis == null) && !(this.lastSpineBone == null) && this.legs.Length != 0 && this.forelegs.Length != 0 && !(this.characterRoot == null) && this.IsReadyToInitiateLegs(this.legs) && this.IsReadyToInitiateLegs(this.forelegs);
		}

		private bool IsReadyToInitiateLegs(IK[] ikComponents)
		{
			foreach (IK ik in ikComponents)
			{
				if (ik == null)
				{
					return false;
				}
				if (ik is FullBodyBipedIK)
				{
					base.LogWarning("GrounderIK does not support FullBodyBipedIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead. If you want to use FullBodyBipedIK, use the GrounderFBBIK component.");
					return false;
				}
				if (ik is FABRIKRoot)
				{
					base.LogWarning("GrounderIK does not support FABRIKRoot, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
				if (ik is AimIK)
				{
					base.LogWarning("GrounderIK does not support AimIK, use CCDIK, FABRIK, LimbIK or TrigonometricIK instead.");
					return false;
				}
			}
			return true;
		}

		private void OnDisable()
		{
			if (!this.initiated)
			{
				return;
			}
			for (int i = 0; i < this.feet.Length; i++)
			{
				if (this.feet[i].solver != null)
				{
					this.feet[i].solver.IKPositionWeight = 0f;
				}
			}
		}

		private void Update()
		{
			this.weight = Mathf.Clamp(this.weight, 0f, 1f);
			if (this.weight <= 0f)
			{
				return;
			}
			this.solved = false;
			if (this.initiated)
			{
				return;
			}
			if (!this.IsReadyToInitiate())
			{
				return;
			}
			this.Initiate();
		}

		private void Initiate()
		{
			this.feet = new GrounderQuadruped.Foot[this.legs.Length + this.forelegs.Length];
			Transform[] array = this.InitiateFeet(this.legs, ref this.feet, 0);
			Transform[] array2 = this.InitiateFeet(this.forelegs, ref this.feet, this.legs.Length);
			this.animatedPelvisLocalPosition = this.pelvis.localPosition;
			this.animatedPelvisLocalRotation = this.pelvis.localRotation;
			if (this.head != null)
			{
				this.animatedHeadLocalRotation = this.head.localRotation;
			}
			this.forefeetRoot = new GameObject().transform;
			this.forefeetRoot.parent = base.transform;
			this.forefeetRoot.name = "Forefeet Root";
			this.solver.Initiate(base.transform, array);
			this.forelegSolver.Initiate(this.forefeetRoot, array2);
			for (int i = 0; i < array.Length; i++)
			{
				this.feet[i].leg = this.solver.legs[i];
			}
			for (int j = 0; j < array2.Length; j++)
			{
				this.feet[j + this.legs.Length].leg = this.forelegSolver.legs[j];
			}
			this.initiated = true;
		}

		private Transform[] InitiateFeet(IK[] ikComponents, ref GrounderQuadruped.Foot[] f, int indexOffset)
		{
			Transform[] array = new Transform[ikComponents.Length];
			for (int i = 0; i < ikComponents.Length; i++)
			{
				IKSolver.Point[] points = ikComponents[i].GetIKSolver().GetPoints();
				f[i + indexOffset] = new GrounderQuadruped.Foot(ikComponents[i].GetIKSolver(), points[points.Length - 1].transform);
				array[i] = f[i + indexOffset].transform;
				IKSolver solver = f[i + indexOffset].solver;
				solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
				IKSolver solver2 = f[i + indexOffset].solver;
				solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostSolverUpdate));
			}
			return array;
		}

		private void LateUpdate()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			this.rootRotationWeight = Mathf.Clamp(this.rootRotationWeight, 0f, 1f);
			this.minRootRotation = Mathf.Clamp(this.minRootRotation, -90f, this.maxRootRotation);
			this.maxRootRotation = Mathf.Clamp(this.maxRootRotation, this.minRootRotation, 90f);
			this.rootRotationSpeed = Mathf.Clamp(this.rootRotationSpeed, 0f, this.rootRotationSpeed);
			this.maxLegOffset = Mathf.Clamp(this.maxLegOffset, 0f, this.maxLegOffset);
			this.maxForeLegOffset = Mathf.Clamp(this.maxForeLegOffset, 0f, this.maxForeLegOffset);
			this.maintainHeadRotationWeight = Mathf.Clamp(this.maintainHeadRotationWeight, 0f, 1f);
			this.RootRotation();
		}

		private void RootRotation()
		{
			if (this.rootRotationWeight <= 0f)
			{
				return;
			}
			if (this.rootRotationSpeed <= 0f)
			{
				return;
			}
			this.solver.rotateSolver = true;
			this.forelegSolver.rotateSolver = true;
			Vector3 forward = this.characterRoot.forward;
			Vector3 vector = -this.gravity;
			Vector3.OrthoNormalize(ref vector, ref forward);
			Quaternion quaternion = Quaternion.LookRotation(forward, -this.gravity);
			Vector3 point = this.forelegSolver.rootHit.point - this.solver.rootHit.point;
			Vector3 vector2 = Quaternion.Inverse(quaternion) * point;
			float num = Mathf.Atan2(vector2.y, vector2.z) * 57.29578f;
			num = Mathf.Clamp(num * this.rootRotationWeight, this.minRootRotation, this.maxRootRotation);
			this.angle = Mathf.Lerp(this.angle, num, Time.deltaTime * this.rootRotationSpeed);
			this.characterRoot.rotation = Quaternion.Slerp(this.characterRoot.rotation, Quaternion.AngleAxis(-this.angle, this.characterRoot.right) * quaternion, this.weight);
		}

		private void OnSolverUpdate()
		{
			if (!base.enabled)
			{
				return;
			}
			if (this.weight <= 0f)
			{
				if (this.lastWeight <= 0f)
				{
					return;
				}
				this.OnDisable();
			}
			this.lastWeight = this.weight;
			if (this.solved)
			{
				return;
			}
			if (this.OnPreGrounder != null)
			{
				this.OnPreGrounder();
			}
			if (this.pelvis.localPosition != this.solvedPelvisLocalPosition)
			{
				this.animatedPelvisLocalPosition = this.pelvis.localPosition;
			}
			else
			{
				this.pelvis.localPosition = this.animatedPelvisLocalPosition;
			}
			if (this.pelvis.localRotation != this.solvedPelvisLocalRotation)
			{
				this.animatedPelvisLocalRotation = this.pelvis.localRotation;
			}
			else
			{
				this.pelvis.localRotation = this.animatedPelvisLocalRotation;
			}
			if (this.head != null)
			{
				if (this.head.localRotation != this.solvedHeadLocalRotation)
				{
					this.animatedHeadLocalRotation = this.head.localRotation;
				}
				else
				{
					this.head.localRotation = this.animatedHeadLocalRotation;
				}
			}
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].rotation = this.feet[i].transform.rotation;
			}
			if (this.head != null)
			{
				this.headRotation = this.head.rotation;
			}
			this.UpdateForefeetRoot();
			this.solver.Update();
			this.forelegSolver.Update();
			this.pelvis.position += this.solver.pelvis.IKOffset * this.weight;
			Vector3 fromDirection = this.lastSpineBone.position - this.pelvis.position;
			Vector3 a = this.lastSpineBone.position + this.forelegSolver.root.up * Mathf.Clamp(this.forelegSolver.pelvis.heightOffset, float.NegativeInfinity, 0f) - this.solver.root.up * this.solver.pelvis.heightOffset;
			Vector3 toDirection = a - this.pelvis.position;
			Quaternion b = Quaternion.FromToRotation(fromDirection, toDirection);
			this.pelvis.rotation = Quaternion.Slerp(Quaternion.identity, b, this.weight) * this.pelvis.rotation;
			for (int j = 0; j < this.feet.Length; j++)
			{
				this.SetFootIK(this.feet[j], (j >= 2) ? this.maxForeLegOffset : this.maxLegOffset);
			}
			this.solved = true;
			this.solvedFeet = 0;
			if (this.OnPostGrounder != null)
			{
				this.OnPostGrounder();
			}
		}

		private void UpdateForefeetRoot()
		{
			Vector3 a = Vector3.zero;
			for (int i = 0; i < this.forelegSolver.legs.Length; i++)
			{
				a += this.forelegSolver.legs[i].transform.position;
			}
			a /= (float)this.forelegs.Length;
			Vector3 vector = a - base.transform.position;
			Vector3 up = base.transform.up;
			Vector3 vector2 = vector;
			Vector3.OrthoNormalize(ref up, ref vector2);
			this.forefeetRoot.position = base.transform.position + vector2.normalized * vector.magnitude;
		}

		private void SetFootIK(GrounderQuadruped.Foot foot, float maxOffset)
		{
			Vector3 vector = foot.leg.IKPosition - foot.transform.position;
			foot.solver.IKPosition = foot.transform.position + Vector3.ClampMagnitude(vector, maxOffset);
			foot.solver.IKPositionWeight = this.weight;
		}

		private void OnPostSolverUpdate()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			if (!base.enabled)
			{
				return;
			}
			this.solvedFeet++;
			if (this.solvedFeet < this.feet.Length)
			{
				return;
			}
			for (int i = 0; i < this.feet.Length; i++)
			{
				this.feet[i].transform.rotation = Quaternion.Slerp(Quaternion.identity, this.feet[i].leg.rotationOffset, this.weight) * this.feet[i].rotation;
			}
			if (this.head != null)
			{
				this.head.rotation = Quaternion.Lerp(this.head.rotation, this.headRotation, this.maintainHeadRotationWeight * this.weight);
			}
			this.solvedPelvisLocalPosition = this.pelvis.localPosition;
			this.solvedPelvisLocalRotation = this.pelvis.localRotation;
			if (this.head != null)
			{
				this.solvedHeadLocalRotation = this.head.localRotation;
			}
		}

		private void OnDestroy()
		{
			if (this.initiated)
			{
				this.DestroyLegs(this.legs);
				this.DestroyLegs(this.forelegs);
			}
		}

		private void DestroyLegs(IK[] ikComponents)
		{
			foreach (IK ik in ikComponents)
			{
				if (ik != null)
				{
					IKSolver iksolver = ik.GetIKSolver();
					iksolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iksolver.OnPreUpdate, new IKSolver.UpdateDelegate(this.OnSolverUpdate));
					IKSolver iksolver2 = ik.GetIKSolver();
					iksolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(iksolver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.OnPostSolverUpdate));
				}
			}
		}

		[Tooltip("The Grounding solver for the forelegs.")]
		public Grounding forelegSolver = new Grounding();

		[Range(0f, 1f)]
		[Tooltip("The weight of rotating the character root to the ground angle (range: 0 - 1).")]
		public float rootRotationWeight = 0.5f;

		[Range(-90f, 0f)]
		[Tooltip("The maximum angle of rotating the quadruped downwards (going downhill, range: -90 - 0).")]
		public float minRootRotation = -25f;

		[Range(0f, 90f)]
		[Tooltip("The maximum angle of rotating the quadruped upwards (going uphill, range: 0 - 90).")]
		public float maxRootRotation = 45f;

		[Tooltip("The speed of interpolating the character root rotation (range: 0 - inf).")]
		public float rootRotationSpeed = 5f;

		[Tooltip("The maximum IK offset for the legs (range: 0 - inf).")]
		public float maxLegOffset = 0.5f;

		[Tooltip("The maximum IK offset for the forelegs (range: 0 - inf).")]
		public float maxForeLegOffset = 0.5f;

		[Range(0f, 1f)]
		[Tooltip("The weight of maintaining the head's rotation as it was before solving the Grounding (range: 0 - 1).")]
		public float maintainHeadRotationWeight = 0.5f;

		[Tooltip("The root Transform of the character, with the rigidbody and the collider.")]
		public Transform characterRoot;

		[Tooltip("The pelvis transform. Common ancestor of both legs and the spine.")]
		public Transform pelvis;

		[Tooltip("The last bone in the spine that is the common parent for both forelegs.")]
		public Transform lastSpineBone;

		[Tooltip("The head (optional, if you intend to maintain it's rotation).")]
		public Transform head;

		public IK[] legs;

		public IK[] forelegs;

		[HideInInspector]
		public Vector3 gravity = Vector3.down;

		private GrounderQuadruped.Foot[] feet = new GrounderQuadruped.Foot[0];

		private Vector3 animatedPelvisLocalPosition;

		private Quaternion animatedPelvisLocalRotation;

		private Quaternion animatedHeadLocalRotation;

		private Vector3 solvedPelvisLocalPosition;

		private Quaternion solvedPelvisLocalRotation;

		private Quaternion solvedHeadLocalRotation;

		private int solvedFeet;

		private bool solved;

		private float angle;

		private Transform forefeetRoot;

		private Quaternion headRotation;

		private float lastWeight;

		public struct Foot
		{
			public Foot(IKSolver solver, Transform transform)
			{
				this.solver = solver;
				this.transform = transform;
				this.leg = null;
				this.rotation = transform.rotation;
			}

			public IKSolver solver;

			public Transform transform;

			public Quaternion rotation;

			public Grounding.Leg leg;
		}
	}
}
