using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class Grounding
	{
		public Grounding.Leg[] legs { get; private set; }

		public Grounding.Pelvis pelvis { get; private set; }

		public bool isGrounded { get; private set; }

		public Transform root { get; private set; }

		public RaycastHit rootHit { get; private set; }

		public bool rootGrounded
		{
			get
			{
				return this.rootHit.distance < this.maxStep * 2f;
			}
		}

		public RaycastHit GetRootHit(float maxDistanceMlp = 10f)
		{
			RaycastHit result = default(RaycastHit);
			Vector3 up = this.up;
			Vector3 a = Vector3.zero;
			foreach (Grounding.Leg leg in this.legs)
			{
				a += leg.transform.position;
			}
			a /= (float)this.legs.Length;
			result.point = a - up * this.maxStep * 10f;
			float num = maxDistanceMlp + 1f;
			result.distance = this.maxStep * num;
			if (this.maxStep <= 0f)
			{
				return result;
			}
			if (this.quality != Grounding.Quality.Best)
			{
				Physics.Raycast(a + up * this.maxStep, -up, out result, this.maxStep * num, this.layers);
			}
			else
			{
				Physics.SphereCast(a + up * this.maxStep, this.rootSphereCastRadius, -this.up, out result, this.maxStep * num, this.layers);
			}
			return result;
		}

		public bool IsValid(ref string errorMessage)
		{
			if (this.root == null)
			{
				errorMessage = "Root transform is null. Can't initiate Grounding.";
				return false;
			}
			if (this.legs == null)
			{
				errorMessage = "Grounding legs is null. Can't initiate Grounding.";
				return false;
			}
			if (this.pelvis == null)
			{
				errorMessage = "Grounding pelvis is null. Can't initiate Grounding.";
				return false;
			}
			if (this.legs.Length == 0)
			{
				errorMessage = "Grounding has 0 legs. Can't initiate Grounding.";
				return false;
			}
			return true;
		}

		public void Initiate(Transform root, Transform[] feet)
		{
			this.root = root;
			this.initiated = false;
			this.rootHit = default(RaycastHit);
			if (this.legs == null)
			{
				this.legs = new Grounding.Leg[feet.Length];
			}
			if (this.legs.Length != feet.Length)
			{
				this.legs = new Grounding.Leg[feet.Length];
			}
			for (int i = 0; i < feet.Length; i++)
			{
				if (this.legs[i] == null)
				{
					this.legs[i] = new Grounding.Leg();
				}
			}
			if (this.pelvis == null)
			{
				this.pelvis = new Grounding.Pelvis();
			}
			string empty = string.Empty;
			if (!this.IsValid(ref empty))
			{
				Warning.Log(empty, root, false);
				return;
			}
			if (Application.isPlaying)
			{
				for (int j = 0; j < feet.Length; j++)
				{
					this.legs[j].Initiate(this, feet[j]);
				}
				this.pelvis.Initiate(this);
				this.initiated = true;
			}
		}

		public void Update()
		{
			if (!this.initiated)
			{
				return;
			}
			if (this.layers == 0)
			{
				this.LogWarning("Grounding layers are set to nothing. Please add a ground layer.");
			}
			this.maxStep = Mathf.Clamp(this.maxStep, 0f, this.maxStep);
			this.footRadius = Mathf.Clamp(this.footRadius, 0.0001f, this.maxStep);
			this.pelvisDamper = Mathf.Clamp(this.pelvisDamper, 0f, 1f);
			this.rootSphereCastRadius = Mathf.Clamp(this.rootSphereCastRadius, 0.0001f, this.rootSphereCastRadius);
			this.maxFootRotationAngle = Mathf.Clamp(this.maxFootRotationAngle, 0f, 90f);
			this.prediction = Mathf.Clamp(this.prediction, 0f, this.prediction);
			this.footSpeed = Mathf.Clamp(this.footSpeed, 0f, this.footSpeed);
			this.rootHit = this.GetRootHit(10f);
			float num = float.NegativeInfinity;
			float num2 = float.PositiveInfinity;
			this.isGrounded = false;
			foreach (Grounding.Leg leg in this.legs)
			{
				leg.Process();
				if (leg.IKOffset > num)
				{
					num = leg.IKOffset;
				}
				if (leg.IKOffset < num2)
				{
					num2 = leg.IKOffset;
				}
				if (leg.isGrounded)
				{
					this.isGrounded = true;
				}
			}
			this.pelvis.Process(-num * this.lowerPelvisWeight, -num2 * this.liftPelvisWeight, this.isGrounded);
		}

		public Vector3 GetLegsPlaneNormal()
		{
			if (!this.initiated)
			{
				return Vector3.up;
			}
			Vector3 up = this.up;
			Vector3 vector = up;
			for (int i = 0; i < this.legs.Length; i++)
			{
				Vector3 vector2 = this.legs[i].IKPosition - this.root.position;
				Vector3 vector3 = up;
				Vector3 fromDirection = vector2;
				Vector3.OrthoNormalize(ref vector3, ref fromDirection);
				Quaternion rotation = Quaternion.FromToRotation(fromDirection, vector2);
				vector = rotation * vector;
			}
			return vector;
		}

		public void Reset()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.pelvis.Reset();
			foreach (Grounding.Leg leg in this.legs)
			{
				leg.Reset();
			}
		}

		public void LogWarning(string message)
		{
			Warning.Log(message, this.root, false);
		}

		public Vector3 up
		{
			get
			{
				return (!this.useRootRotation) ? Vector3.up : this.root.up;
			}
		}

		public float GetVerticalOffset(Vector3 p1, Vector3 p2)
		{
			if (this.useRootRotation)
			{
				return (Quaternion.Inverse(this.root.rotation) * (p1 - p2)).y;
			}
			return p1.y - p2.y;
		}

		public Vector3 Flatten(Vector3 v)
		{
			if (this.useRootRotation)
			{
				Vector3 onNormal = v;
				Vector3 up = this.root.up;
				Vector3.OrthoNormalize(ref up, ref onNormal);
				return Vector3.Project(v, onNormal);
			}
			v.y = 0f;
			return v;
		}

		private bool useRootRotation
		{
			get
			{
				return this.rotateSolver && !(this.root.up == Vector3.up);
			}
		}

		public Vector3 GetFootCenterOffset()
		{
			return this.root.forward * this.footRadius + this.root.forward * this.footCenterOffset;
		}

		[Tooltip("Layers to ground the character to. Make sure to exclude the layer of the character controller.")]
		public LayerMask layers;

		[Tooltip("Max step height. Maximum vertical distance of Grounding from the root of the character.")]
		public float maxStep = 0.5f;

		[Tooltip("The height offset of the root.")]
		public float heightOffset;

		[Tooltip("The speed of moving the feet up/down.")]
		public float footSpeed = 2.5f;

		[Tooltip("CapsuleCast radius. Should match approximately with the size of the feet.")]
		public float footRadius = 0.15f;

		[Tooltip("Offset of the foot center along character forward axis.")]
		[HideInInspector]
		public float footCenterOffset;

		[Tooltip("Amount of velocity based prediction of the foot positions.")]
		public float prediction = 0.05f;

		[Tooltip("Weight of rotating the feet to the ground normal offset.")]
		[Range(0f, 1f)]
		public float footRotationWeight = 1f;

		[Tooltip("Speed of slerping the feet to their grounded rotations.")]
		public float footRotationSpeed = 7f;

		[Range(0f, 90f)]
		[Tooltip("Max Foot Rotation Angle. Max angular offset from the foot's rotation.")]
		public float maxFootRotationAngle = 45f;

		[Tooltip("If true, solver will rotate with the character root so the character can be grounded for example to spherical planets. For performance reasons leave this off unless needed.")]
		public bool rotateSolver;

		[Tooltip("The speed of moving the character up/down.")]
		public float pelvisSpeed = 5f;

		[Range(0f, 1f)]
		[Tooltip("Used for smoothing out vertical pelvis movement (range 0 - 1).")]
		public float pelvisDamper;

		[Tooltip("The weight of lowering the pelvis to the lowest foot.")]
		public float lowerPelvisWeight = 1f;

		[Tooltip("The weight of lifting the pelvis to the highest foot. This is useful when you don't want the feet to go too high relative to the body when crouching.")]
		public float liftPelvisWeight;

		[Tooltip("The radius of the spherecast from the root that determines whether the character root is grounded.")]
		public float rootSphereCastRadius = 0.1f;

		[Tooltip("The raycasting quality. Fastest is a single raycast per foot, Simple is three raycasts, Best is one raycast and a capsule cast per foot.")]
		public Grounding.Quality quality = Grounding.Quality.Best;

		private bool initiated;

		[Serializable]
		public enum Quality
		{
			Fastest,
			Simple,
			Best
		}

		public class Leg
		{
			public bool isGrounded { get; private set; }

			public Vector3 IKPosition { get; private set; }

			public bool initiated { get; private set; }

			public float heightFromGround { get; private set; }

			public Vector3 velocity { get; private set; }

			public Transform transform { get; private set; }

			public float IKOffset { get; private set; }

			public void Initiate(Grounding grounding, Transform transform)
			{
				this.initiated = false;
				this.grounding = grounding;
				this.transform = transform;
				this.up = Vector3.up;
				this.IKPosition = transform.position;
				this.rotationOffset = Quaternion.identity;
				this.initiated = true;
				this.OnEnable();
			}

			public void OnEnable()
			{
				if (!this.initiated)
				{
					return;
				}
				this.lastPosition = this.transform.position;
				this.lastTime = Time.deltaTime;
			}

			public void Reset()
			{
				this.lastPosition = this.transform.position;
				this.lastTime = Time.deltaTime;
				this.IKOffset = 0f;
				this.IKPosition = this.transform.position;
				this.rotationOffset = Quaternion.identity;
			}

			public void Process()
			{
				if (!this.initiated)
				{
					return;
				}
				if (this.grounding.maxStep <= 0f)
				{
					return;
				}
				this.deltaTime = Time.time - this.lastTime;
				this.lastTime = Time.time;
				if (this.deltaTime == 0f)
				{
					return;
				}
				this.up = this.grounding.up;
				this.heightFromGround = float.PositiveInfinity;
				this.velocity = (this.transform.position - this.lastPosition) / this.deltaTime;
				this.velocity = this.grounding.Flatten(this.velocity);
				this.lastPosition = this.transform.position;
				Vector3 vector = this.velocity * this.grounding.prediction;
				if (this.grounding.footRadius <= 0f)
				{
					this.grounding.quality = Grounding.Quality.Fastest;
				}
				Grounding.Quality quality = this.grounding.quality;
				if (quality != Grounding.Quality.Fastest)
				{
					if (quality != Grounding.Quality.Simple)
					{
						if (quality == Grounding.Quality.Best)
						{
							this.heelHit = this.GetRaycastHit((!this.invertFootCenter) ? Vector3.zero : (-this.grounding.GetFootCenterOffset()));
							RaycastHit capsuleHit = this.GetCapsuleHit(vector);
							this.SetFootToPlane(capsuleHit.normal, capsuleHit.point, this.heelHit.point);
						}
					}
					else
					{
						this.heelHit = this.GetRaycastHit(Vector3.zero);
						Vector3 a = this.grounding.GetFootCenterOffset();
						if (this.invertFootCenter)
						{
							a = -a;
						}
						RaycastHit raycastHit = this.GetRaycastHit(a + vector);
						RaycastHit raycastHit2 = this.GetRaycastHit(this.grounding.root.right * this.grounding.footRadius * 0.5f);
						Vector3 vector2 = Vector3.Cross(raycastHit.point - this.heelHit.point, raycastHit2.point - this.heelHit.point).normalized;
						if (Vector3.Dot(vector2, this.up) < 0f)
						{
							vector2 = -vector2;
						}
						this.SetFootToPlane(vector2, this.heelHit.point, this.heelHit.point);
					}
				}
				else
				{
					RaycastHit raycastHit3 = this.GetRaycastHit(vector);
					this.SetFootToPoint(raycastHit3.normal, raycastHit3.point);
				}
				this.isGrounded = (this.heightFromGround < this.grounding.maxStep);
				float num = this.stepHeightFromGround;
				if (!this.grounding.rootGrounded)
				{
					num = 0f;
				}
				this.IKOffset = Interp.LerpValue(this.IKOffset, num, this.grounding.footSpeed, this.grounding.footSpeed);
				this.IKOffset = Mathf.Lerp(this.IKOffset, num, this.deltaTime * this.grounding.footSpeed);
				float verticalOffset = this.grounding.GetVerticalOffset(this.transform.position, this.grounding.root.position);
				float num2 = Mathf.Clamp(this.grounding.maxStep - verticalOffset, 0f, this.grounding.maxStep);
				this.IKOffset = Mathf.Clamp(this.IKOffset, -num2, this.IKOffset);
				this.RotateFoot();
				this.IKPosition = this.transform.position - this.up * this.IKOffset;
				float footRotationWeight = this.grounding.footRotationWeight;
				this.rotationOffset = ((footRotationWeight < 1f) ? Quaternion.Slerp(Quaternion.identity, this.r, footRotationWeight) : this.r);
			}

			public float stepHeightFromGround
			{
				get
				{
					return Mathf.Clamp(this.heightFromGround, -this.grounding.maxStep, this.grounding.maxStep);
				}
			}

			private RaycastHit GetCapsuleHit(Vector3 offsetFromHeel)
			{
				RaycastHit result = default(RaycastHit);
				Vector3 vector = this.grounding.GetFootCenterOffset();
				if (this.invertFootCenter)
				{
					vector = -vector;
				}
				Vector3 a = this.transform.position + vector;
				result.point = a - this.up * this.grounding.maxStep * 2f;
				result.normal = this.up;
				Vector3 vector2 = a + this.grounding.maxStep * this.up;
				Vector3 point = vector2 + offsetFromHeel;
				if (Physics.CapsuleCast(vector2, point, this.grounding.footRadius, -this.up, out result, this.grounding.maxStep * 3f, this.grounding.layers) && float.IsNaN(result.point.x))
				{
					result.point = a - this.up * this.grounding.maxStep * 2f;
					result.normal = this.up;
				}
				return result;
			}

			private RaycastHit GetRaycastHit(Vector3 offsetFromHeel)
			{
				RaycastHit result = default(RaycastHit);
				Vector3 a = this.transform.position + offsetFromHeel;
				result.point = a - this.up * this.grounding.maxStep * 2f;
				result.normal = this.up;
				if (this.grounding.maxStep <= 0f)
				{
					return result;
				}
				Physics.Raycast(a + this.grounding.maxStep * this.up, -this.up, out result, this.grounding.maxStep * 3f, this.grounding.layers);
				return result;
			}

			private Vector3 RotateNormal(Vector3 normal)
			{
				if (this.grounding.quality == Grounding.Quality.Best)
				{
					return normal;
				}
				return Vector3.RotateTowards(this.up, normal, this.grounding.maxFootRotationAngle * 0.0174532924f, this.deltaTime);
			}

			private void SetFootToPoint(Vector3 normal, Vector3 point)
			{
				this.toHitNormal = Quaternion.FromToRotation(this.up, this.RotateNormal(normal));
				this.heightFromGround = this.GetHeightFromGround(point);
			}

			private void SetFootToPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 heelHitPoint)
			{
				planeNormal = this.RotateNormal(planeNormal);
				this.toHitNormal = Quaternion.FromToRotation(this.up, planeNormal);
				Vector3 hitPoint = V3Tools.LineToPlane(this.transform.position + this.up * this.grounding.maxStep, -this.up, planeNormal, planePoint);
				this.heightFromGround = this.GetHeightFromGround(hitPoint);
				float heightFromGround = this.GetHeightFromGround(heelHitPoint);
				this.heightFromGround = Mathf.Clamp(this.heightFromGround, float.NegativeInfinity, heightFromGround);
			}

			private float GetHeightFromGround(Vector3 hitPoint)
			{
				return this.grounding.GetVerticalOffset(this.transform.position, hitPoint) - this.rootYOffset;
			}

			private void RotateFoot()
			{
				Quaternion rotationOffsetTarget = this.GetRotationOffsetTarget();
				this.r = Quaternion.Slerp(this.r, rotationOffsetTarget, this.deltaTime * this.grounding.footRotationSpeed);
			}

			private Quaternion GetRotationOffsetTarget()
			{
				if (this.grounding.maxFootRotationAngle <= 0f)
				{
					return Quaternion.identity;
				}
				if (this.grounding.maxFootRotationAngle >= 180f)
				{
					return this.toHitNormal;
				}
				return Quaternion.RotateTowards(Quaternion.identity, this.toHitNormal, this.grounding.maxFootRotationAngle);
			}

			private float rootYOffset
			{
				get
				{
					return this.grounding.GetVerticalOffset(this.transform.position, this.grounding.root.position - this.up * this.grounding.heightOffset);
				}
			}

			public Quaternion rotationOffset = Quaternion.identity;

			public bool invertFootCenter;

			private Grounding grounding;

			private float lastTime;

			private float deltaTime;

			private Vector3 lastPosition;

			private Quaternion toHitNormal;

			private Quaternion r;

			private RaycastHit heelHit;

			private Vector3 up = Vector3.up;
		}

		public class Pelvis
		{
			public Vector3 IKOffset { get; private set; }

			public float heightOffset { get; private set; }

			public void Initiate(Grounding grounding)
			{
				this.grounding = grounding;
				this.initiated = true;
				this.OnEnable();
			}

			public void Reset()
			{
				this.lastRootPosition = this.grounding.root.transform.position;
				this.lastTime = Time.deltaTime;
				this.IKOffset = Vector3.zero;
				this.heightOffset = 0f;
			}

			public void OnEnable()
			{
				if (!this.initiated)
				{
					return;
				}
				this.lastRootPosition = this.grounding.root.transform.position;
				this.lastTime = Time.time;
			}

			public void Process(float lowestOffset, float highestOffset, bool isGrounded)
			{
				if (!this.initiated)
				{
					return;
				}
				float num = Time.time - this.lastTime;
				this.lastTime = Time.time;
				if (num <= 0f)
				{
					return;
				}
				float b = lowestOffset + highestOffset;
				if (!this.grounding.rootGrounded)
				{
					b = 0f;
				}
				this.heightOffset = Mathf.Lerp(this.heightOffset, b, num * this.grounding.pelvisSpeed);
				Vector3 p = this.grounding.root.position - this.lastRootPosition;
				this.lastRootPosition = this.grounding.root.position;
				this.damperF = Interp.LerpValue(this.damperF, (!isGrounded) ? 0f : 1f, 1f, 10f);
				this.heightOffset -= this.grounding.GetVerticalOffset(p, Vector3.zero) * this.grounding.pelvisDamper * this.damperF;
				this.IKOffset = this.grounding.up * this.heightOffset;
			}

			private Grounding grounding;

			private Vector3 lastRootPosition;

			private float damperF;

			private bool initiated;

			private float lastTime;
		}
	}
}
