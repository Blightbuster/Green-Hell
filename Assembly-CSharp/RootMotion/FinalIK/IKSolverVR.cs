using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverVR : IKSolver
	{
		public void SetToReferences(VRIK.References references)
		{
			if (!references.isFilled)
			{
				Debug.LogError("Invalid references, one or more Transforms are missing.");
				return;
			}
			this.solverTransforms = references.GetTransforms();
			this.hasChest = (this.solverTransforms[3] != null);
			this.hasNeck = (this.solverTransforms[4] != null);
			this.hasShoulders = (this.solverTransforms[6] != null && this.solverTransforms[10] != null);
			this.hasToes = (this.solverTransforms[17] != null && this.solverTransforms[21] != null);
			this.readPositions = new Vector3[this.solverTransforms.Length];
			this.readRotations = new Quaternion[this.solverTransforms.Length];
			this.DefaultAnimationCurves();
			this.GuessHandOrientations(references, true);
		}

		public void GuessHandOrientations(VRIK.References references, bool onlyIfZero)
		{
			if (!references.isFilled)
			{
				Debug.LogWarning("VRIK References are not filled in, can not guess hand orientations. Right-click on VRIK header and slect 'Guess Hand Orientations' when you have filled in the References.");
				return;
			}
			if (this.leftArm.wristToPalmAxis == Vector3.zero || !onlyIfZero)
			{
				this.leftArm.wristToPalmAxis = this.GuessWristToPalmAxis(references.leftHand, references.leftForearm);
			}
			if (this.leftArm.palmToThumbAxis == Vector3.zero || !onlyIfZero)
			{
				this.leftArm.palmToThumbAxis = this.GuessPalmToThumbAxis(references.leftHand, references.leftForearm);
			}
			if (this.rightArm.wristToPalmAxis == Vector3.zero || !onlyIfZero)
			{
				this.rightArm.wristToPalmAxis = this.GuessWristToPalmAxis(references.rightHand, references.rightForearm);
			}
			if (this.rightArm.palmToThumbAxis == Vector3.zero || !onlyIfZero)
			{
				this.rightArm.palmToThumbAxis = this.GuessPalmToThumbAxis(references.rightHand, references.rightForearm);
			}
		}

		public void DefaultAnimationCurves()
		{
			if (this.locomotion.stepHeight == null)
			{
				this.locomotion.stepHeight = new AnimationCurve();
			}
			if (this.locomotion.heelHeight == null)
			{
				this.locomotion.heelHeight = new AnimationCurve();
			}
			if (this.locomotion.stepHeight.keys.Length == 0)
			{
				this.locomotion.stepHeight.keys = IKSolverVR.GetSineKeyframes(0.03f);
			}
			if (this.locomotion.heelHeight.keys.Length == 0)
			{
				this.locomotion.heelHeight.keys = IKSolverVR.GetSineKeyframes(0.03f);
			}
		}

		public void AddPositionOffset(IKSolverVR.PositionOffset positionOffset, Vector3 value)
		{
			switch (positionOffset)
			{
			case IKSolverVR.PositionOffset.Pelvis:
				this.spine.pelvisPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.Chest:
				this.spine.chestPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.Head:
				this.spine.headPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.LeftHand:
				this.leftArm.handPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.RightHand:
				this.rightArm.handPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.LeftFoot:
				this.leftLeg.footPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.RightFoot:
				this.rightLeg.footPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.LeftHeel:
				this.leftLeg.heelPositionOffset += value;
				return;
			case IKSolverVR.PositionOffset.RightHeel:
				this.rightLeg.heelPositionOffset += value;
				return;
			default:
				return;
			}
		}

		public void AddRotationOffset(IKSolverVR.RotationOffset rotationOffset, Vector3 value)
		{
			this.AddRotationOffset(rotationOffset, Quaternion.Euler(value));
		}

		public void AddRotationOffset(IKSolverVR.RotationOffset rotationOffset, Quaternion value)
		{
			if (rotationOffset == IKSolverVR.RotationOffset.Pelvis)
			{
				this.spine.pelvisRotationOffset = value * this.spine.pelvisRotationOffset;
				return;
			}
			if (rotationOffset == IKSolverVR.RotationOffset.Chest)
			{
				this.spine.chestRotationOffset = value * this.spine.chestRotationOffset;
				return;
			}
			if (rotationOffset != IKSolverVR.RotationOffset.Head)
			{
				return;
			}
			this.spine.headRotationOffset = value * this.spine.headRotationOffset;
		}

		public void AddPlatformMotion(Vector3 deltaPosition, Quaternion deltaRotation, Vector3 platformPivot)
		{
			this.locomotion.AddDeltaPosition(deltaPosition);
			this.raycastOriginPelvis += deltaPosition;
			this.locomotion.AddDeltaRotation(deltaRotation, platformPivot);
			this.spine.faceDirection = deltaRotation * this.spine.faceDirection;
		}

		public void Reset()
		{
			if (!base.initiated)
			{
				return;
			}
			this.UpdateSolverTransforms();
			this.Read(this.readPositions, this.readRotations, this.hasChest, this.hasNeck, this.hasShoulders, this.hasToes);
			this.spine.faceDirection = this.rootBone.readRotation * Vector3.forward;
			this.locomotion.Reset(this.readPositions, this.readRotations);
			this.raycastOriginPelvis = this.spine.pelvis.readPosition;
		}

		public override void StoreDefaultLocalState()
		{
			this.defaultPelvisLocalPosition = this.solverTransforms[1].localPosition;
			for (int i = 1; i < this.solverTransforms.Length; i++)
			{
				if (this.solverTransforms[i] != null)
				{
					this.defaultLocalRotations[i - 1] = this.solverTransforms[i].localRotation;
				}
			}
		}

		public override void FixTransforms()
		{
			this.solverTransforms[1].localPosition = this.defaultPelvisLocalPosition;
			for (int i = 1; i < this.solverTransforms.Length; i++)
			{
				if (this.solverTransforms[i] != null)
				{
					this.solverTransforms[i].localRotation = this.defaultLocalRotations[i - 1];
				}
			}
		}

		public override IKSolver.Point[] GetPoints()
		{
			Debug.LogError("GetPoints() is not applicable to IKSolverVR.");
			return null;
		}

		public override IKSolver.Point GetPoint(Transform transform)
		{
			Debug.LogError("GetPoint is not applicable to IKSolverVR.");
			return null;
		}

		public override bool IsValid(ref string message)
		{
			if (this.solverTransforms == null || this.solverTransforms.Length == 0)
			{
				message = "Trying to initiate IKSolverVR with invalid bone references.";
				return false;
			}
			if (this.leftArm.wristToPalmAxis == Vector3.zero)
			{
				message = "Left arm 'Wrist To Palm Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the wrist towards the palm. If the arrow points away from the palm, axis must be negative.";
				return false;
			}
			if (this.rightArm.wristToPalmAxis == Vector3.zero)
			{
				message = "Right arm 'Wrist To Palm Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the wrist towards the palm. If the arrow points away from the palm, axis must be negative.";
				return false;
			}
			if (this.leftArm.palmToThumbAxis == Vector3.zero)
			{
				message = "Left arm 'Palm To Thumb Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the palm towards the thumb. If the arrow points away from the thumb, axis must be negative.";
				return false;
			}
			if (this.rightArm.palmToThumbAxis == Vector3.zero)
			{
				message = "Right arm 'Palm To Thumb Axis' needs to be set in VRIK. Please select the hand bone, set it to the axis that points from the palm towards the thumb. If the arrow points away from the thumb, axis must be negative.";
				return false;
			}
			return true;
		}

		private Vector3 GetNormal(Transform[] transforms)
		{
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			for (int i = 0; i < transforms.Length; i++)
			{
				vector2 += transforms[i].position;
			}
			vector2 /= (float)transforms.Length;
			for (int j = 0; j < transforms.Length - 1; j++)
			{
				vector += Vector3.Cross(transforms[j].position - vector2, transforms[j + 1].position - vector2).normalized;
			}
			return vector;
		}

		private Vector3 GuessWristToPalmAxis(Transform hand, Transform forearm)
		{
			Vector3 vector = forearm.position - hand.position;
			Vector3 vector2 = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, vector));
			if (Vector3.Dot(vector, hand.rotation * vector2) > 0f)
			{
				vector2 = -vector2;
			}
			return vector2;
		}

		private Vector3 GuessPalmToThumbAxis(Transform hand, Transform forearm)
		{
			if (hand.childCount == 0)
			{
				Debug.LogWarning("Hand " + hand.name + " does not have any fingers, VRIK can not guess the hand bone's orientation. Please assign 'Wrist To Palm Axis' and 'Palm To Thumb Axis' manually for both arms in VRIK settings.", hand);
				return Vector3.zero;
			}
			float num = float.PositiveInfinity;
			int index = 0;
			for (int i = 0; i < hand.childCount; i++)
			{
				float num2 = Vector3.SqrMagnitude(hand.GetChild(i).position - hand.position);
				if (num2 < num)
				{
					num = num2;
					index = i;
				}
			}
			Vector3 lhs = Vector3.Cross(hand.position - forearm.position, hand.GetChild(index).position - hand.position);
			Vector3 vector = Vector3.Cross(lhs, hand.position - forearm.position);
			Vector3 vector2 = AxisTools.ToVector3(AxisTools.GetAxisToDirection(hand, vector));
			if (Vector3.Dot(vector, hand.rotation * vector2) < 0f)
			{
				vector2 = -vector2;
			}
			return vector2;
		}

		private static Keyframe[] GetSineKeyframes(float mag)
		{
			Keyframe[] array = new Keyframe[3];
			array[0].time = 0f;
			array[0].value = 0f;
			array[1].time = 0.5f;
			array[1].value = mag;
			array[2].time = 1f;
			array[2].value = 0f;
			return array;
		}

		private void UpdateSolverTransforms()
		{
			for (int i = 0; i < this.solverTransforms.Length; i++)
			{
				if (this.solverTransforms[i] != null)
				{
					this.readPositions[i] = this.solverTransforms[i].position;
					this.readRotations[i] = this.solverTransforms[i].rotation;
				}
			}
		}

		protected override void OnInitiate()
		{
			this.UpdateSolverTransforms();
			this.Read(this.readPositions, this.readRotations, this.hasChest, this.hasNeck, this.hasShoulders, this.hasToes);
		}

		protected override void OnUpdate()
		{
			if (this.IKPositionWeight > 0f)
			{
				this.UpdateSolverTransforms();
				this.Read(this.readPositions, this.readRotations, this.hasChest, this.hasNeck, this.hasShoulders, this.hasToes);
				this.Solve();
				this.Write();
				this.WriteTransforms();
			}
		}

		private void WriteTransforms()
		{
			for (int i = 0; i < this.solverTransforms.Length; i++)
			{
				if (this.solverTransforms[i] != null)
				{
					if (i < 2)
					{
						this.solverTransforms[i].position = V3Tools.Lerp(this.solverTransforms[i].position, this.GetPosition(i), this.IKPositionWeight);
					}
					this.solverTransforms[i].rotation = QuaTools.Lerp(this.solverTransforms[i].rotation, this.GetRotation(i), this.IKPositionWeight);
				}
			}
		}

		private void Read(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes)
		{
			if (this.rootBone == null)
			{
				this.rootBone = new IKSolverVR.VirtualBone(positions[0], rotations[0]);
			}
			else
			{
				this.rootBone.Read(positions[0], rotations[0]);
			}
			this.spine.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, 0, 1);
			this.leftArm.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, (!hasChest) ? 2 : 3, 6);
			this.rightArm.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, (!hasChest) ? 2 : 3, 10);
			this.leftLeg.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, 1, 14);
			this.rightLeg.Read(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, 1, 18);
			for (int i = 0; i < rotations.Length; i++)
			{
				if (i < 2)
				{
					this.solvedPositions[i] = positions[i];
				}
				this.solvedRotations[i] = rotations[i];
			}
			if (!base.initiated)
			{
				this.legs = new IKSolverVR.Leg[]
				{
					this.leftLeg,
					this.rightLeg
				};
				this.arms = new IKSolverVR.Arm[]
				{
					this.leftArm,
					this.rightArm
				};
				this.locomotion.Initiate(positions, rotations, hasToes);
				this.raycastOriginPelvis = this.spine.pelvis.readPosition;
				this.spine.faceDirection = this.readRotations[0] * Vector3.forward;
			}
		}

		private void Solve()
		{
			this.spine.PreSolve();
			foreach (IKSolverVR.Arm arm in this.arms)
			{
				arm.PreSolve();
			}
			foreach (IKSolverVR.Leg leg in this.legs)
			{
				leg.PreSolve();
			}
			foreach (IKSolverVR.Arm arm2 in this.arms)
			{
				arm2.ApplyOffsets();
			}
			this.spine.ApplyOffsets();
			this.spine.Solve(this.rootBone, this.legs, this.arms);
			if (this.spine.pelvisPositionWeight > 0f && this.plantFeet)
			{
				Warning.Log("If VRIK 'Pelvis Position Weight' is > 0, 'Plant Feet' should be disabled to improve performance and stability.", this.root, false);
			}
			if (this.locomotion.weight > 0f)
			{
				Vector3 a = Vector3.zero;
				Vector3 a2 = Vector3.zero;
				Quaternion identity = Quaternion.identity;
				Quaternion identity2 = Quaternion.identity;
				float num = 0f;
				float num2 = 0f;
				float d = 0f;
				float d2 = 0f;
				this.locomotion.Solve(this.rootBone, this.spine, this.leftLeg, this.rightLeg, this.leftArm, this.rightArm, this.supportLegIndex, out a, out a2, out identity, out identity2, out num, out num2, out d, out d2);
				a += this.root.up * num;
				a2 += this.root.up * num2;
				this.leftLeg.footPositionOffset += (a - this.leftLeg.lastBone.solverPosition) * this.IKPositionWeight * (1f - this.leftLeg.positionWeight) * this.locomotion.weight;
				this.rightLeg.footPositionOffset += (a2 - this.rightLeg.lastBone.solverPosition) * this.IKPositionWeight * (1f - this.rightLeg.positionWeight) * this.locomotion.weight;
				this.leftLeg.heelPositionOffset += this.root.up * d * this.locomotion.weight;
				this.rightLeg.heelPositionOffset += this.root.up * d2 * this.locomotion.weight;
				Quaternion quaternion = QuaTools.FromToRotation(this.leftLeg.lastBone.solverRotation, identity);
				Quaternion quaternion2 = QuaTools.FromToRotation(this.rightLeg.lastBone.solverRotation, identity2);
				quaternion = Quaternion.Lerp(Quaternion.identity, quaternion, this.IKPositionWeight * (1f - this.leftLeg.rotationWeight) * this.locomotion.weight);
				quaternion2 = Quaternion.Lerp(Quaternion.identity, quaternion2, this.IKPositionWeight * (1f - this.rightLeg.rotationWeight) * this.locomotion.weight);
				this.leftLeg.footRotationOffset = quaternion * this.leftLeg.footRotationOffset;
				this.rightLeg.footRotationOffset = quaternion2 * this.rightLeg.footRotationOffset;
				Vector3 vector = Vector3.Lerp(this.leftLeg.position + this.leftLeg.footPositionOffset, this.rightLeg.position + this.rightLeg.footPositionOffset, 0.5f);
				vector = V3Tools.PointToPlane(vector, this.rootBone.solverPosition, this.root.up);
				this.rootVelocity += (vector - this.rootBone.solverPosition) * Time.deltaTime * 10f;
				Vector3 b = V3Tools.ExtractVertical(this.rootVelocity, this.root.up, 1f);
				this.rootVelocity -= b;
				Vector3 vector2 = this.rootBone.solverPosition + this.rootVelocity * Time.deltaTime * 2f * this.locomotion.weight;
				vector2 = Vector3.Lerp(vector2, vector, Time.deltaTime * this.locomotion.rootSpeed * this.locomotion.weight);
				this.rootBone.solverPosition = vector2;
				float d3 = num + num2;
				this.bodyOffset = Vector3.Lerp(this.bodyOffset, this.root.up * d3, Time.deltaTime * 3f);
				this.bodyOffset = Vector3.Lerp(Vector3.zero, this.bodyOffset, this.locomotion.weight);
			}
			foreach (IKSolverVR.Leg leg2 in this.legs)
			{
				leg2.ApplyOffsets();
			}
			if (!this.plantFeet)
			{
				this.spine.InverseTranslateToHead(this.legs, false, false, this.bodyOffset, 1f);
				foreach (IKSolverVR.Leg leg3 in this.legs)
				{
					leg3.TranslateRoot(this.spine.pelvis.solverPosition, this.spine.pelvis.solverRotation);
				}
				foreach (IKSolverVR.Leg leg4 in this.legs)
				{
					leg4.Solve();
				}
			}
			else
			{
				for (int num3 = 0; num3 < 2; num3++)
				{
					this.spine.InverseTranslateToHead(this.legs, true, num3 == 0, this.bodyOffset, 1f);
					foreach (IKSolverVR.Leg leg5 in this.legs)
					{
						leg5.TranslateRoot(this.spine.pelvis.solverPosition, this.spine.pelvis.solverRotation);
					}
					foreach (IKSolverVR.Leg leg6 in this.legs)
					{
						leg6.Solve();
					}
				}
			}
			for (int num6 = 0; num6 < this.arms.Length; num6++)
			{
				this.arms[num6].TranslateRoot(this.spine.chest.solverPosition, this.spine.chest.solverRotation);
				this.arms[num6].Solve(num6 == 0);
			}
			this.spine.ResetOffsets();
			foreach (IKSolverVR.Leg leg7 in this.legs)
			{
				leg7.ResetOffsets();
			}
			foreach (IKSolverVR.Arm arm3 in this.arms)
			{
				arm3.ResetOffsets();
			}
			this.spine.pelvisPositionOffset += this.GetPelvisOffset();
			this.spine.chestPositionOffset += this.spine.pelvisPositionOffset;
			this.Write();
			this.supportLegIndex = -1;
			float num9 = float.PositiveInfinity;
			for (int num10 = 0; num10 < this.legs.Length; num10++)
			{
				float num11 = Vector3.SqrMagnitude(this.legs[num10].lastBone.solverPosition - this.legs[num10].bones[0].solverPosition);
				if (num11 < num9)
				{
					this.supportLegIndex = num10;
					num9 = num11;
				}
			}
		}

		private Vector3 GetPosition(int index)
		{
			if (index >= 2)
			{
				Debug.LogError("Can only get root and pelvis positions from IKSolverVR. GetPosition index out of range.");
			}
			return this.solvedPositions[index];
		}

		private Quaternion GetRotation(int index)
		{
			return this.solvedRotations[index];
		}

		[HideInInspector]
		public IKSolverVR.VirtualBone rootBone { get; private set; }

		private void Write()
		{
			this.solvedPositions[0] = this.rootBone.solverPosition;
			this.solvedRotations[0] = this.rootBone.solverRotation;
			this.spine.Write(ref this.solvedPositions, ref this.solvedRotations);
			foreach (IKSolverVR.Leg leg in this.legs)
			{
				leg.Write(ref this.solvedPositions, ref this.solvedRotations);
			}
			foreach (IKSolverVR.Arm arm in this.arms)
			{
				arm.Write(ref this.solvedPositions, ref this.solvedRotations);
			}
		}

		private Vector3 GetPelvisOffset()
		{
			if (this.locomotion.weight <= 0f)
			{
				return Vector3.zero;
			}
			if (this.locomotion.blockingLayers == -1)
			{
				return Vector3.zero;
			}
			Vector3 vector = this.raycastOriginPelvis;
			vector.y = this.spine.pelvis.solverPosition.y;
			Vector3 vector2 = this.spine.pelvis.readPosition;
			vector2.y = this.spine.pelvis.solverPosition.y;
			Vector3 direction = vector2 - vector;
			RaycastHit raycastHit;
			if (this.locomotion.raycastRadius <= 0f)
			{
				if (Physics.Raycast(vector, direction, out raycastHit, direction.magnitude * 1.1f, this.locomotion.blockingLayers))
				{
					vector2 = raycastHit.point;
				}
			}
			else if (Physics.SphereCast(vector, this.locomotion.raycastRadius * 1.1f, direction, out raycastHit, direction.magnitude, this.locomotion.blockingLayers))
			{
				vector2 = vector + direction.normalized * raycastHit.distance / 1.1f;
			}
			Vector3 a = this.spine.pelvis.solverPosition;
			direction = a - vector2;
			if (this.locomotion.raycastRadius <= 0f)
			{
				if (Physics.Raycast(vector2, direction, out raycastHit, direction.magnitude, this.locomotion.blockingLayers))
				{
					a = raycastHit.point;
				}
			}
			else if (Physics.SphereCast(vector2, this.locomotion.raycastRadius, direction, out raycastHit, direction.magnitude, this.locomotion.blockingLayers))
			{
				a = vector2 + direction.normalized * raycastHit.distance;
			}
			this.lastOffset = Vector3.Lerp(this.lastOffset, Vector3.zero, Time.deltaTime * 3f);
			a += Vector3.ClampMagnitude(this.lastOffset, 0.75f);
			a.y = this.spine.pelvis.solverPosition.y;
			this.lastOffset = Vector3.Lerp(this.lastOffset, a - this.spine.pelvis.solverPosition, Time.deltaTime * 15f);
			return this.lastOffset;
		}

		private Transform[] solverTransforms = new Transform[0];

		private bool hasChest;

		private bool hasNeck;

		private bool hasShoulders;

		private bool hasToes;

		private Vector3[] readPositions = new Vector3[0];

		private Quaternion[] readRotations = new Quaternion[0];

		private Vector3[] solvedPositions = new Vector3[2];

		private Quaternion[] solvedRotations = new Quaternion[22];

		private Vector3 defaultPelvisLocalPosition;

		private Quaternion[] defaultLocalRotations = new Quaternion[21];

		private Vector3 rootV;

		private Vector3 rootVelocity;

		private Vector3 bodyOffset;

		private int supportLegIndex;

		[Tooltip("If true, will keep the toes planted even if head target is out of reach.")]
		public bool plantFeet = true;

		[Tooltip("The spine solver.")]
		public IKSolverVR.Spine spine = new IKSolverVR.Spine();

		[Tooltip("The left arm solver.")]
		public IKSolverVR.Arm leftArm = new IKSolverVR.Arm();

		[Tooltip("The right arm solver.")]
		public IKSolverVR.Arm rightArm = new IKSolverVR.Arm();

		[Tooltip("The left leg solver.")]
		public IKSolverVR.Leg leftLeg = new IKSolverVR.Leg();

		[Tooltip("The right leg solver.")]
		public IKSolverVR.Leg rightLeg = new IKSolverVR.Leg();

		[Tooltip("The procedural locomotion solver.")]
		public IKSolverVR.Locomotion locomotion = new IKSolverVR.Locomotion();

		private IKSolverVR.Leg[] legs = new IKSolverVR.Leg[2];

		private IKSolverVR.Arm[] arms = new IKSolverVR.Arm[2];

		private Vector3 headPosition;

		private Vector3 headDeltaPosition;

		private Vector3 raycastOriginPelvis;

		private Vector3 lastOffset;

		private Vector3 debugPos1;

		private Vector3 debugPos2;

		private Vector3 debugPos3;

		private Vector3 debugPos4;

		[Serializable]
		public class Arm : IKSolverVR.BodyPart
		{
			public Vector3 position { get; private set; }

			public Quaternion rotation { get; private set; }

			private IKSolverVR.VirtualBone shoulder
			{
				get
				{
					return this.bones[0];
				}
			}

			private IKSolverVR.VirtualBone upperArm
			{
				get
				{
					return this.bones[1];
				}
			}

			private IKSolverVR.VirtualBone forearm
			{
				get
				{
					return this.bones[2];
				}
			}

			private IKSolverVR.VirtualBone hand
			{
				get
				{
					return this.bones[3];
				}
			}

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				Vector3 position = positions[index];
				Quaternion rotation = rotations[index];
				Vector3 position2 = positions[index + 1];
				Quaternion rotation2 = rotations[index + 1];
				Vector3 position3 = positions[index + 2];
				Quaternion rotation3 = rotations[index + 2];
				Vector3 vector = positions[index + 3];
				Quaternion quaternion = rotations[index + 3];
				if (!this.initiated)
				{
					this.IKPosition = vector;
					this.IKRotation = quaternion;
					this.rotation = this.IKRotation;
					this.hasShoulder = hasShoulders;
					this.bones = new IKSolverVR.VirtualBone[(!this.hasShoulder) ? 3 : 4];
					if (this.hasShoulder)
					{
						this.bones[0] = new IKSolverVR.VirtualBone(position, rotation);
						this.bones[1] = new IKSolverVR.VirtualBone(position2, rotation2);
						this.bones[2] = new IKSolverVR.VirtualBone(position3, rotation3);
						this.bones[3] = new IKSolverVR.VirtualBone(vector, quaternion);
					}
					else
					{
						this.bones[0] = new IKSolverVR.VirtualBone(position2, rotation2);
						this.bones[1] = new IKSolverVR.VirtualBone(position3, rotation3);
						this.bones[2] = new IKSolverVR.VirtualBone(vector, quaternion);
					}
					this.chestForwardAxis = Quaternion.Inverse(this.rootRotation) * (rotations[0] * Vector3.forward);
					this.chestUpAxis = Quaternion.Inverse(this.rootRotation) * (rotations[0] * Vector3.up);
				}
				if (this.hasShoulder)
				{
					this.bones[0].Read(position, rotation);
					this.bones[1].Read(position2, rotation2);
					this.bones[2].Read(position3, rotation3);
					this.bones[3].Read(vector, quaternion);
				}
				else
				{
					this.bones[0].Read(position2, rotation2);
					this.bones[1].Read(position3, rotation3);
					this.bones[2].Read(vector, quaternion);
				}
			}

			public override void PreSolve()
			{
				if (this.target != null)
				{
					this.IKPosition = this.target.position;
					this.IKRotation = this.target.rotation;
				}
				this.position = V3Tools.Lerp(this.hand.solverPosition, this.IKPosition, this.positionWeight);
				this.rotation = QuaTools.Lerp(this.hand.solverRotation, this.IKRotation, this.rotationWeight);
				this.shoulder.axis = this.shoulder.axis.normalized;
				this.forearmRelToUpperArm = Quaternion.Inverse(this.upperArm.solverRotation) * this.forearm.solverRotation;
			}

			public override void ApplyOffsets()
			{
				this.position += this.handPositionOffset;
			}

			public void Solve(bool isLeft)
			{
				this.chestRotation = Quaternion.LookRotation(this.rootRotation * this.chestForwardAxis, this.rootRotation * this.chestUpAxis);
				this.chestForward = this.chestRotation * Vector3.forward;
				this.chestUp = this.chestRotation * Vector3.up;
				if (this.hasShoulder && this.shoulderRotationWeight > 0f)
				{
					IKSolverVR.Arm.ShoulderRotationMode shoulderRotationMode = this.shoulderRotationMode;
					if (shoulderRotationMode != IKSolverVR.Arm.ShoulderRotationMode.YawPitch)
					{
						if (shoulderRotationMode == IKSolverVR.Arm.ShoulderRotationMode.FromTo)
						{
							Quaternion solverRotation = this.shoulder.solverRotation;
							Quaternion quaternion = Quaternion.FromToRotation((this.upperArm.solverPosition - this.shoulder.solverPosition).normalized + this.chestForward, this.position - this.shoulder.solverPosition);
							quaternion = Quaternion.Slerp(Quaternion.identity, quaternion, 0.5f * this.shoulderRotationWeight * this.positionWeight);
							IKSolverVR.VirtualBone.RotateBy(this.bones, quaternion);
							IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, 0, 2, 3, this.position, Vector3.Cross(this.forearm.solverPosition - this.shoulder.solverPosition, this.hand.solverPosition - this.shoulder.solverPosition), 0.5f * this.shoulderRotationWeight * this.positionWeight);
							IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, 1, 2, 3, this.position, this.GetBendNormal(this.position - this.upperArm.solverPosition), this.positionWeight);
							Quaternion rotation = Quaternion.Inverse(Quaternion.LookRotation(this.chestUp, this.chestForward));
							Vector3 vector = rotation * (solverRotation * this.shoulder.axis);
							Vector3 vector2 = rotation * (this.shoulder.solverRotation * this.shoulder.axis);
							float current = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
							float num = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
							float num2 = Mathf.DeltaAngle(current, num);
							if (isLeft)
							{
								num2 = -num2;
							}
							num2 = Mathf.Clamp(num2 * 2f * this.positionWeight, 0f, 180f);
							this.shoulder.solverRotation = Quaternion.AngleAxis(num2, this.shoulder.solverRotation * ((!isLeft) ? (-this.shoulder.axis) : this.shoulder.axis)) * this.shoulder.solverRotation;
							this.upperArm.solverRotation = Quaternion.AngleAxis(num2, this.upperArm.solverRotation * ((!isLeft) ? (-this.upperArm.axis) : this.upperArm.axis)) * this.upperArm.solverRotation;
						}
					}
					else
					{
						Vector3 point = (this.position - this.shoulder.solverPosition).normalized;
						float num3 = (!isLeft) ? -45f : 45f;
						Quaternion lhs = Quaternion.AngleAxis(((!isLeft) ? 90f : -90f) + num3, this.chestUp);
						Quaternion quaternion2 = lhs * this.chestRotation;
						Vector3 lhs2 = Quaternion.Inverse(quaternion2) * point;
						float num4 = Mathf.Atan2(lhs2.x, lhs2.z) * 57.29578f;
						float num5 = Vector3.Dot(lhs2, Vector3.up);
						num5 = 1f - Mathf.Abs(num5);
						num4 *= num5;
						num4 -= num3;
						num4 = this.DamperValue(num4, -45f - num3, 45f - num3, 0.7f);
						Vector3 fromDirection = this.shoulder.solverRotation * this.shoulder.axis;
						Vector3 toDirection = quaternion2 * (Quaternion.AngleAxis(num4, Vector3.up) * Vector3.forward);
						Quaternion rhs = Quaternion.FromToRotation(fromDirection, toDirection);
						Quaternion lhs3 = Quaternion.AngleAxis((!isLeft) ? 90f : -90f, this.chestUp);
						quaternion2 = lhs3 * this.chestRotation;
						quaternion2 = Quaternion.AngleAxis((!isLeft) ? 30f : -30f, this.chestForward) * quaternion2;
						point = this.position - (this.shoulder.solverPosition + this.chestRotation * ((!isLeft) ? Vector3.left : Vector3.right) * base.mag);
						lhs2 = Quaternion.Inverse(quaternion2) * point;
						float num6 = Mathf.Atan2(lhs2.y, lhs2.z) * 57.29578f;
						num6 -= -30f;
						num6 = this.DamperValue(num6, -15f, 75f, 1f);
						Quaternion lhs4 = Quaternion.AngleAxis(-num6, quaternion2 * Vector3.right);
						Quaternion quaternion3 = lhs4 * rhs;
						if (this.shoulderRotationWeight * this.positionWeight < 1f)
						{
							quaternion3 = Quaternion.Lerp(Quaternion.identity, quaternion3, this.shoulderRotationWeight * this.positionWeight);
						}
						IKSolverVR.VirtualBone.RotateBy(this.bones, quaternion3);
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, 1, 2, 3, this.position, this.GetBendNormal(this.position - this.upperArm.solverPosition), this.positionWeight);
						float angle = Mathf.Clamp(num6 * 2f * this.positionWeight, 0f, 180f);
						this.shoulder.solverRotation = Quaternion.AngleAxis(angle, this.shoulder.solverRotation * ((!isLeft) ? (-this.shoulder.axis) : this.shoulder.axis)) * this.shoulder.solverRotation;
						this.upperArm.solverRotation = Quaternion.AngleAxis(angle, this.upperArm.solverRotation * ((!isLeft) ? (-this.upperArm.axis) : this.upperArm.axis)) * this.upperArm.solverRotation;
					}
				}
				else
				{
					IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, 1, 2, 3, this.position, this.GetBendNormal(this.position - this.upperArm.solverPosition), this.positionWeight);
				}
				Quaternion quaternion4 = this.upperArm.solverRotation * this.forearmRelToUpperArm;
				Quaternion lhs5 = Quaternion.FromToRotation(quaternion4 * this.forearm.axis, this.hand.solverPosition - this.forearm.solverPosition);
				base.RotateTo(this.forearm, lhs5 * quaternion4, this.positionWeight);
				if (this.rotationWeight >= 1f)
				{
					this.hand.solverRotation = this.rotation;
				}
				else if (this.rotationWeight > 0f)
				{
					this.hand.solverRotation = Quaternion.Lerp(this.hand.solverRotation, this.rotation, this.rotationWeight);
				}
			}

			public override void ResetOffsets()
			{
				this.handPositionOffset = Vector3.zero;
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
			{
				if (this.hasShoulder)
				{
					solvedRotations[this.index] = this.shoulder.solverRotation;
				}
				solvedRotations[this.index + 1] = this.upperArm.solverRotation;
				solvedRotations[this.index + 2] = this.forearm.solverRotation;
				solvedRotations[this.index + 3] = this.hand.solverRotation;
			}

			private float DamperValue(float value, float min, float max, float weight = 1f)
			{
				float num = max - min;
				if (weight < 1f)
				{
					float num2 = max - num * 0.5f;
					float num3 = value - num2;
					num3 *= 0.5f;
					value = num2 + num3;
				}
				value -= min;
				float t = Mathf.Clamp(value / num, 0f, 1f);
				float t2 = Interp.Float(t, InterpolationMode.InOutQuintic);
				return Mathf.Lerp(min, max, t2);
			}

			private Vector3 GetBendNormal(Vector3 dir)
			{
				if (this.bendGoal != null)
				{
					this.bendDirection = this.bendGoal.position - this.bones[0].solverPosition;
				}
				if (this.bendGoalWeight < 1f)
				{
					Vector3 vector = this.bones[0].solverRotation * this.bones[0].axis;
					Vector3 fromDirection = Vector3.down;
					Vector3 toDirection = Quaternion.Inverse(this.chestRotation) * dir.normalized + Vector3.forward;
					Quaternion rotation = Quaternion.FromToRotation(fromDirection, toDirection);
					Vector3 vector2 = rotation * Vector3.back;
					fromDirection = Quaternion.Inverse(this.chestRotation) * vector;
					toDirection = Quaternion.Inverse(this.chestRotation) * dir;
					rotation = Quaternion.FromToRotation(fromDirection, toDirection);
					vector2 = rotation * vector2;
					vector2 = this.chestRotation * vector2;
					vector2 += vector;
					vector2 -= this.rotation * this.wristToPalmAxis;
					vector2 -= this.rotation * this.palmToThumbAxis * 0.5f;
					if (this.bendGoalWeight > 0f)
					{
						vector2 = Vector3.Slerp(vector2, this.bendDirection, this.bendGoalWeight);
					}
					if (this.swivelOffset != 0f)
					{
						vector2 = Quaternion.AngleAxis(this.swivelOffset, -dir) * vector2;
					}
					return Vector3.Cross(vector2, dir);
				}
				return Vector3.Cross(this.bendDirection, dir);
			}

			private void Visualize(IKSolverVR.VirtualBone bone1, IKSolverVR.VirtualBone bone2, IKSolverVR.VirtualBone bone3, Color color)
			{
				Debug.DrawLine(bone1.solverPosition, bone2.solverPosition, color);
				Debug.DrawLine(bone2.solverPosition, bone3.solverPosition, color);
			}

			[Tooltip("The hand target")]
			public Transform target;

			[Tooltip("The elbow will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
			public Transform bendGoal;

			[Range(0f, 1f)]
			[Tooltip("Positional weight of the hand target.")]
			public float positionWeight = 1f;

			[Range(0f, 1f)]
			[Tooltip("Rotational weight of the hand target")]
			public float rotationWeight = 1f;

			[Tooltip("Different techniques for shoulder bone rotation.")]
			public IKSolverVR.Arm.ShoulderRotationMode shoulderRotationMode;

			[Range(0f, 1f)]
			[Tooltip("The weight of shoulder rotation")]
			public float shoulderRotationWeight = 1f;

			[Range(0f, 1f)]
			[Tooltip("If greater than 0, will bend the elbow towards the 'Bend Goal' Transform.")]
			public float bendGoalWeight;

			[Range(-180f, 180f)]
			[Tooltip("Angular offset of the elbow bending direction.")]
			public float swivelOffset;

			[Tooltip("Local axis of the hand bone that points from the wrist towards the palm. Used for defining hand bone orientation.")]
			public Vector3 wristToPalmAxis = Vector3.zero;

			[Tooltip("Local axis of the hand bone that points from the palm towards the thumb. Used for defining hand bone orientation.")]
			public Vector3 palmToThumbAxis = Vector3.zero;

			[HideInInspector]
			[NonSerialized]
			public Vector3 IKPosition;

			[HideInInspector]
			[NonSerialized]
			public Quaternion IKRotation = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Vector3 bendDirection = Vector3.back;

			[HideInInspector]
			[NonSerialized]
			public Vector3 handPositionOffset;

			private bool hasShoulder;

			private Vector3 chestForwardAxis;

			private Vector3 chestUpAxis;

			private Quaternion chestRotation = Quaternion.identity;

			private Vector3 chestForward;

			private Vector3 chestUp;

			private Quaternion forearmRelToUpperArm = Quaternion.identity;

			private const float yawOffsetAngle = 45f;

			private const float pitchOffsetAngle = -30f;

			[Serializable]
			public enum ShoulderRotationMode
			{
				YawPitch,
				FromTo
			}
		}

		[Serializable]
		public abstract class BodyPart
		{
			protected abstract void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index);

			public abstract void PreSolve();

			public abstract void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations);

			public abstract void ApplyOffsets();

			public abstract void ResetOffsets();

			public float sqrMag { get; private set; }

			public float mag { get; private set; }

			public void Read(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				this.index = index;
				this.rootPosition = positions[rootIndex];
				this.rootRotation = rotations[rootIndex];
				this.OnRead(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, rootIndex, index);
				this.mag = IKSolverVR.VirtualBone.PreSolve(ref this.bones);
				this.sqrMag = this.mag * this.mag;
				this.initiated = true;
			}

			public void MovePosition(Vector3 position)
			{
				Vector3 b = position - this.bones[0].solverPosition;
				foreach (IKSolverVR.VirtualBone virtualBone in this.bones)
				{
					virtualBone.solverPosition += b;
				}
			}

			public void MoveRotation(Quaternion rotation)
			{
				Quaternion rotation2 = QuaTools.FromToRotation(this.bones[0].solverRotation, rotation);
				IKSolverVR.VirtualBone.RotateAroundPoint(this.bones, 0, this.bones[0].solverPosition, rotation2);
			}

			public void Translate(Vector3 position, Quaternion rotation)
			{
				this.MovePosition(position);
				this.MoveRotation(rotation);
			}

			public void TranslateRoot(Vector3 newRootPos, Quaternion newRootRot)
			{
				Vector3 b = newRootPos - this.rootPosition;
				this.rootPosition = newRootPos;
				foreach (IKSolverVR.VirtualBone virtualBone in this.bones)
				{
					virtualBone.solverPosition += b;
				}
				Quaternion rotation = QuaTools.FromToRotation(this.rootRotation, newRootRot);
				this.rootRotation = newRootRot;
				IKSolverVR.VirtualBone.RotateAroundPoint(this.bones, 0, newRootPos, rotation);
			}

			public void RotateTo(IKSolverVR.VirtualBone bone, Quaternion rotation, float weight = 1f)
			{
				if (weight <= 0f)
				{
					return;
				}
				Quaternion quaternion = QuaTools.FromToRotation(bone.solverRotation, rotation);
				if (weight < 1f)
				{
					quaternion = Quaternion.Slerp(Quaternion.identity, quaternion, weight);
				}
				for (int i = 0; i < this.bones.Length; i++)
				{
					if (this.bones[i] == bone)
					{
						IKSolverVR.VirtualBone.RotateAroundPoint(this.bones, i, this.bones[i].solverPosition, quaternion);
						return;
					}
				}
			}

			public void Visualize(Color color)
			{
				for (int i = 0; i < this.bones.Length - 1; i++)
				{
					Debug.DrawLine(this.bones[i].solverPosition, this.bones[i + 1].solverPosition, color);
				}
			}

			public void Visualize()
			{
				this.Visualize(Color.white);
			}

			[HideInInspector]
			public IKSolverVR.VirtualBone[] bones = new IKSolverVR.VirtualBone[0];

			protected bool initiated;

			protected Vector3 rootPosition;

			protected Quaternion rootRotation = Quaternion.identity;

			protected int index = -1;
		}

		[Serializable]
		public class Footstep
		{
			public Footstep(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation, Vector3 characterSpaceOffset)
			{
				this.characterSpaceOffset = characterSpaceOffset;
				this.Reset(rootRotation, footPosition, footRotation);
			}

			public bool isStepping
			{
				get
				{
					return this.stepProgress < 1f;
				}
			}

			public float stepProgress { get; private set; }

			public void Reset(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation)
			{
				this.position = footPosition;
				this.rotation = footRotation;
				this.stepFrom = this.position;
				this.stepTo = this.position;
				this.stepFromRot = this.rotation;
				this.stepToRot = this.rotation;
				this.stepToRootRot = rootRotation;
				this.stepProgress = 1f;
				this.footRelativeToRoot = Quaternion.Inverse(rootRotation) * this.rotation;
			}

			public void StepTo(Vector3 p, Quaternion rootRotation)
			{
				this.stepFrom = this.position;
				this.stepTo = p;
				this.stepFromRot = this.rotation;
				this.stepToRootRot = rootRotation;
				this.stepToRot = rootRotation * this.footRelativeToRoot;
				this.stepProgress = 0f;
			}

			public void UpdateStepping(Vector3 p, Quaternion rootRotation, float speed)
			{
				this.stepTo = Vector3.Lerp(this.stepTo, p, Time.deltaTime * speed);
				this.stepToRot = Quaternion.Lerp(this.stepToRot, rootRotation * this.footRelativeToRoot, Time.deltaTime * speed);
				this.stepToRootRot = this.stepToRot * Quaternion.Inverse(this.footRelativeToRoot);
			}

			public void UpdateStanding(Quaternion rootRotation, float minAngle, float speed)
			{
				if (speed <= 0f || minAngle >= 180f)
				{
					return;
				}
				Quaternion quaternion = rootRotation * this.footRelativeToRoot;
				float num = Quaternion.Angle(this.rotation, quaternion);
				if (num > minAngle)
				{
					this.rotation = Quaternion.RotateTowards(this.rotation, quaternion, Mathf.Min(Time.deltaTime * speed * (1f - this.supportLegW), num - minAngle));
				}
			}

			public void Update(InterpolationMode interpolation, UnityEvent onStep)
			{
				float target = (!this.isSupportLeg) ? 0f : 1f;
				this.supportLegW = Mathf.SmoothDamp(this.supportLegW, target, ref this.supportLegWV, 0.2f);
				if (!this.isStepping)
				{
					return;
				}
				this.stepProgress = Mathf.MoveTowards(this.stepProgress, 1f, Time.deltaTime * this.stepSpeed);
				if (this.stepProgress >= 1f)
				{
					onStep.Invoke();
				}
				float t = Interp.Float(this.stepProgress, interpolation);
				this.position = Vector3.Lerp(this.stepFrom, this.stepTo, t);
				this.rotation = Quaternion.Lerp(this.stepFromRot, this.stepToRot, t);
			}

			public float stepSpeed = 3f;

			public Vector3 characterSpaceOffset;

			public Vector3 position;

			public Quaternion rotation = Quaternion.identity;

			public Quaternion stepToRootRot = Quaternion.identity;

			public bool isSupportLeg;

			public Vector3 stepFrom;

			public Vector3 stepTo;

			public Quaternion stepFromRot = Quaternion.identity;

			public Quaternion stepToRot = Quaternion.identity;

			private Quaternion footRelativeToRoot = Quaternion.identity;

			private float supportLegW;

			private float supportLegWV;
		}

		[Serializable]
		public class Leg : IKSolverVR.BodyPart
		{
			public Vector3 position { get; private set; }

			public Quaternion rotation { get; private set; }

			public bool hasToes { get; private set; }

			public IKSolverVR.VirtualBone thigh
			{
				get
				{
					return this.bones[0];
				}
			}

			private IKSolverVR.VirtualBone calf
			{
				get
				{
					return this.bones[1];
				}
			}

			private IKSolverVR.VirtualBone foot
			{
				get
				{
					return this.bones[2];
				}
			}

			private IKSolverVR.VirtualBone toes
			{
				get
				{
					return this.bones[3];
				}
			}

			public IKSolverVR.VirtualBone lastBone
			{
				get
				{
					return this.bones[this.bones.Length - 1];
				}
			}

			public Vector3 thighRelativeToPelvis { get; private set; }

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				Vector3 position = positions[index];
				Quaternion rotation = rotations[index];
				Vector3 position2 = positions[index + 1];
				Quaternion rotation2 = rotations[index + 1];
				Vector3 vector = positions[index + 2];
				Quaternion quaternion = rotations[index + 2];
				Vector3 vector2 = positions[index + 3];
				Quaternion quaternion2 = rotations[index + 3];
				if (!this.initiated)
				{
					this.hasToes = hasToes;
					this.bones = new IKSolverVR.VirtualBone[(!hasToes) ? 3 : 4];
					if (hasToes)
					{
						this.bones[0] = new IKSolverVR.VirtualBone(position, rotation);
						this.bones[1] = new IKSolverVR.VirtualBone(position2, rotation2);
						this.bones[2] = new IKSolverVR.VirtualBone(vector, quaternion);
						this.bones[3] = new IKSolverVR.VirtualBone(vector2, quaternion2);
						this.IKPosition = vector2;
						this.IKRotation = quaternion2;
					}
					else
					{
						this.bones[0] = new IKSolverVR.VirtualBone(position, rotation);
						this.bones[1] = new IKSolverVR.VirtualBone(position2, rotation2);
						this.bones[2] = new IKSolverVR.VirtualBone(vector, quaternion);
						this.IKPosition = vector;
						this.IKRotation = quaternion;
					}
					this.rotation = this.IKRotation;
				}
				if (hasToes)
				{
					this.bones[0].Read(position, rotation);
					this.bones[1].Read(position2, rotation2);
					this.bones[2].Read(vector, quaternion);
					this.bones[3].Read(vector2, quaternion2);
				}
				else
				{
					this.bones[0].Read(position, rotation);
					this.bones[1].Read(position2, rotation2);
					this.bones[2].Read(vector, quaternion);
				}
			}

			public override void PreSolve()
			{
				if (this.target != null)
				{
					this.IKPosition = this.target.position;
					this.IKRotation = this.target.rotation;
				}
				this.footPosition = this.foot.solverPosition;
				this.footRotation = this.foot.solverRotation;
				this.position = this.lastBone.solverPosition;
				this.rotation = this.lastBone.solverRotation;
				if (this.rotationWeight > 0f)
				{
					this.ApplyRotationOffset(QuaTools.FromToRotation(this.rotation, this.IKRotation), this.rotationWeight);
				}
				if (this.positionWeight > 0f)
				{
					this.ApplyPositionOffset(this.IKPosition - this.position, this.positionWeight);
				}
				this.thighRelativeToPelvis = Quaternion.Inverse(this.rootRotation) * (this.thigh.solverPosition - this.rootPosition);
				this.calfRelToThigh = Quaternion.Inverse(this.thigh.solverRotation) * this.calf.solverRotation;
				this.bendNormal = Vector3.Cross(this.calf.solverPosition - this.thigh.solverPosition, this.foot.solverPosition - this.calf.solverPosition);
			}

			public override void ApplyOffsets()
			{
				this.ApplyPositionOffset(this.footPositionOffset, 1f);
				this.ApplyRotationOffset(this.footRotationOffset, 1f);
				Quaternion quaternion = Quaternion.FromToRotation(this.footPosition - this.position, this.footPosition + this.heelPositionOffset - this.position);
				this.footPosition = this.position + quaternion * (this.footPosition - this.position);
				this.footRotation = quaternion * this.footRotation;
				float num = 0f;
				if (this.bendGoal != null && this.bendGoalWeight > 0f)
				{
					Vector3 point = Vector3.Cross(this.bendGoal.position - this.thigh.solverPosition, this.foot.solverPosition - this.thigh.solverPosition);
					Quaternion rotation = Quaternion.LookRotation(this.bendNormal, this.thigh.solverPosition - this.foot.solverPosition);
					Vector3 vector = Quaternion.Inverse(rotation) * point;
					num = Mathf.Atan2(vector.x, vector.z) * 57.29578f * this.bendGoalWeight;
				}
				float num2 = this.swivelOffset + num;
				if (num2 != 0f)
				{
					this.bendNormal = Quaternion.AngleAxis(num2, this.thigh.solverPosition - this.lastBone.solverPosition) * this.bendNormal;
					this.thigh.solverRotation = Quaternion.AngleAxis(-num2, this.thigh.solverRotation * this.thigh.axis) * this.thigh.solverRotation;
				}
			}

			private void ApplyPositionOffset(Vector3 offset, float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				offset *= weight;
				this.footPosition += offset;
				this.position += offset;
			}

			private void ApplyRotationOffset(Quaternion offset, float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				if (weight < 1f)
				{
					offset = Quaternion.Lerp(Quaternion.identity, offset, weight);
				}
				this.footRotation = offset * this.footRotation;
				this.rotation = offset * this.rotation;
				this.bendNormal = offset * this.bendNormal;
				this.footPosition = this.position + offset * (this.footPosition - this.position);
			}

			public void Solve()
			{
				IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, 0, 1, 2, this.footPosition, this.bendNormal, 1f);
				base.RotateTo(this.foot, this.footRotation, 1f);
				if (!this.hasToes)
				{
					return;
				}
				Vector3 vector = Vector3.Cross(this.foot.solverPosition - this.thigh.solverPosition, this.toes.solverPosition - this.foot.solverPosition);
				IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, 0, 2, 3, this.position, vector, 1f);
				Quaternion quaternion = this.thigh.solverRotation * this.calfRelToThigh;
				Quaternion lhs = Quaternion.FromToRotation(quaternion * this.calf.axis, this.foot.solverPosition - this.calf.solverPosition);
				base.RotateTo(this.calf, lhs * quaternion, 1f);
				this.toes.solverRotation = this.rotation;
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
			{
				solvedRotations[this.index] = this.thigh.solverRotation;
				solvedRotations[this.index + 1] = this.calf.solverRotation;
				solvedRotations[this.index + 2] = this.foot.solverRotation;
				if (this.hasToes)
				{
					solvedRotations[this.index + 3] = this.toes.solverRotation;
				}
			}

			public override void ResetOffsets()
			{
				this.footPositionOffset = Vector3.zero;
				this.footRotationOffset = Quaternion.identity;
				this.heelPositionOffset = Vector3.zero;
			}

			[Tooltip("The toe/foot target.")]
			public Transform target;

			[Tooltip("The knee will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
			public Transform bendGoal;

			[Range(0f, 1f)]
			[Tooltip("Positional weight of the toe/foot target.")]
			public float positionWeight;

			[Range(0f, 1f)]
			[Tooltip("Rotational weight of the toe/foot target.")]
			public float rotationWeight;

			[Range(0f, 1f)]
			[Tooltip("If greater than 0, will bend the knee towards the 'Bend Goal' Transform.")]
			public float bendGoalWeight;

			[Range(-180f, 180f)]
			[Tooltip("Angular offset of the knee bending direction.")]
			public float swivelOffset;

			[HideInInspector]
			[NonSerialized]
			public Vector3 IKPosition;

			[HideInInspector]
			[NonSerialized]
			public Quaternion IKRotation = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Vector3 footPositionOffset;

			[HideInInspector]
			[NonSerialized]
			public Vector3 heelPositionOffset;

			[HideInInspector]
			[NonSerialized]
			public Quaternion footRotationOffset = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public float currentMag;

			private Vector3 footPosition;

			private Quaternion footRotation = Quaternion.identity;

			private Vector3 bendNormal;

			private Quaternion calfRelToThigh = Quaternion.identity;
		}

		[Serializable]
		public class Locomotion
		{
			public Vector3 centerOfMass { get; private set; }

			public void Initiate(Vector3[] positions, Quaternion[] rotations, bool hasToes)
			{
				this.leftFootIndex = ((!hasToes) ? 16 : 17);
				this.rightFootIndex = ((!hasToes) ? 20 : 21);
				this.footsteps = new IKSolverVR.Footstep[]
				{
					new IKSolverVR.Footstep(rotations[0], positions[this.leftFootIndex], rotations[this.leftFootIndex], this.footDistance * Vector3.left),
					new IKSolverVR.Footstep(rotations[0], positions[this.rightFootIndex], rotations[this.rightFootIndex], this.footDistance * Vector3.right)
				};
			}

			public void Reset(Vector3[] positions, Quaternion[] rotations)
			{
				this.lastComPosition = Vector3.Lerp(positions[1], positions[5], 0.25f) + rotations[0] * this.offset;
				this.comVelocity = Vector3.zero;
				this.footsteps[0].Reset(rotations[0], positions[this.leftFootIndex], rotations[this.leftFootIndex]);
				this.footsteps[1].Reset(rotations[0], positions[this.rightFootIndex], rotations[this.rightFootIndex]);
			}

			public void AddDeltaRotation(Quaternion delta, Vector3 pivot)
			{
				Vector3 point = this.lastComPosition - pivot;
				this.lastComPosition = pivot + delta * point;
				foreach (IKSolverVR.Footstep footstep in this.footsteps)
				{
					footstep.rotation = delta * footstep.rotation;
					footstep.stepFromRot = delta * footstep.stepFromRot;
					footstep.stepToRot = delta * footstep.stepToRot;
					footstep.stepToRootRot = delta * footstep.stepToRootRot;
					Vector3 point2 = footstep.position - pivot;
					footstep.position = pivot + delta * point2;
					Vector3 point3 = footstep.stepFrom - pivot;
					footstep.stepFrom = pivot + delta * point3;
					Vector3 point4 = footstep.stepTo - pivot;
					footstep.stepTo = pivot + delta * point4;
				}
			}

			public void AddDeltaPosition(Vector3 delta)
			{
				this.lastComPosition += delta;
				foreach (IKSolverVR.Footstep footstep in this.footsteps)
				{
					footstep.position += delta;
					footstep.stepFrom += delta;
					footstep.stepTo += delta;
				}
			}

			public void Solve(IKSolverVR.VirtualBone rootBone, IKSolverVR.Spine spine, IKSolverVR.Leg leftLeg, IKSolverVR.Leg rightLeg, IKSolverVR.Arm leftArm, IKSolverVR.Arm rightArm, int supportLegIndex, out Vector3 leftFootPosition, out Vector3 rightFootPosition, out Quaternion leftFootRotation, out Quaternion rightFootRotation, out float leftFootOffset, out float rightFootOffset, out float leftHeelOffset, out float rightHeelOffset)
			{
				if (this.weight <= 0f)
				{
					leftFootPosition = Vector3.zero;
					rightFootPosition = Vector3.zero;
					leftFootRotation = Quaternion.identity;
					rightFootRotation = Quaternion.identity;
					leftFootOffset = 0f;
					rightFootOffset = 0f;
					leftHeelOffset = 0f;
					rightHeelOffset = 0f;
					return;
				}
				Vector3 vector = rootBone.solverRotation * Vector3.up;
				Vector3 vector2 = spine.pelvis.solverPosition + spine.pelvis.solverRotation * leftLeg.thighRelativeToPelvis;
				Vector3 vector3 = spine.pelvis.solverPosition + spine.pelvis.solverRotation * rightLeg.thighRelativeToPelvis;
				this.footsteps[0].characterSpaceOffset = this.footDistance * Vector3.left;
				this.footsteps[1].characterSpaceOffset = this.footDistance * Vector3.right;
				Vector3 vector4 = spine.faceDirection;
				Vector3 b = V3Tools.ExtractVertical(vector4, vector, 1f);
				vector4 -= b;
				Quaternion quaternion = Quaternion.LookRotation(vector4, vector);
				float num = 1f;
				float num2 = 1f;
				float num3 = 0.2f;
				float d = num + num2 + 2f * num3;
				this.centerOfMass = Vector3.zero;
				this.centerOfMass += spine.pelvis.solverPosition * num;
				this.centerOfMass += spine.head.solverPosition * num2;
				this.centerOfMass += leftArm.position * num3;
				this.centerOfMass += rightArm.position * num3;
				this.centerOfMass /= d;
				this.centerOfMass += rootBone.solverRotation * this.offset;
				this.comVelocity = ((Time.deltaTime <= 0f) ? Vector3.zero : ((this.centerOfMass - this.lastComPosition) / Time.deltaTime));
				this.lastComPosition = this.centerOfMass;
				this.comVelocity = Vector3.ClampMagnitude(this.comVelocity, this.maxVelocity) * this.velocityFactor;
				Vector3 vector5 = this.centerOfMass + this.comVelocity;
				Vector3 a = V3Tools.PointToPlane(spine.pelvis.solverPosition, rootBone.solverPosition, vector);
				Vector3 a2 = V3Tools.PointToPlane(vector5, rootBone.solverPosition, vector);
				Vector3 b2 = Vector3.Lerp(this.footsteps[0].position, this.footsteps[1].position, 0.5f);
				Vector3 from = vector5 - b2;
				float num4 = Vector3.Angle(from, rootBone.solverRotation * Vector3.up) * this.comAngleMlp;
				for (int i = 0; i < this.footsteps.Length; i++)
				{
					this.footsteps[i].isSupportLeg = (supportLegIndex == i);
				}
				for (int j = 0; j < this.footsteps.Length; j++)
				{
					if (this.footsteps[j].isStepping)
					{
						Vector3 vector6 = a2 + rootBone.solverRotation * this.footsteps[j].characterSpaceOffset;
						if (!this.StepBlocked(this.footsteps[j].stepFrom, vector6, rootBone.solverPosition))
						{
							this.footsteps[j].UpdateStepping(vector6, quaternion, 10f);
						}
					}
					else
					{
						this.footsteps[j].UpdateStanding(quaternion, this.relaxLegTwistMinAngle, this.relaxLegTwistSpeed);
					}
				}
				if (this.CanStep())
				{
					int num5 = -1;
					float num6 = float.NegativeInfinity;
					for (int k = 0; k < this.footsteps.Length; k++)
					{
						if (!this.footsteps[k].isStepping)
						{
							Vector3 vector7 = a2 + rootBone.solverRotation * this.footsteps[k].characterSpaceOffset;
							float num7 = (k != 0) ? rightLeg.mag : leftLeg.mag;
							Vector3 b3 = (k != 0) ? vector3 : vector2;
							float num8 = Vector3.Distance(this.footsteps[k].position, b3);
							bool flag = false;
							if (num8 >= num7 * this.maxLegStretch)
							{
								vector7 = a + rootBone.solverRotation * this.footsteps[k].characterSpaceOffset;
								flag = true;
							}
							bool flag2 = false;
							for (int l = 0; l < this.footsteps.Length; l++)
							{
								if (l != k && !flag)
								{
									if (Vector3.Distance(this.footsteps[k].position, this.footsteps[l].position) >= 0.25f || (this.footsteps[k].position - vector7).sqrMagnitude >= (this.footsteps[l].position - vector7).sqrMagnitude)
									{
										flag2 = IKSolverVR.Locomotion.GetLineSphereCollision(this.footsteps[k].position, vector7, this.footsteps[l].position, 0.25f);
									}
									if (flag2)
									{
										break;
									}
								}
							}
							float num9 = Quaternion.Angle(quaternion, this.footsteps[k].stepToRootRot);
							if (!flag2 || num9 > this.angleThreshold)
							{
								float num10 = Vector3.Distance(this.footsteps[k].position, vector7);
								float num11 = Mathf.Lerp(this.stepThreshold, this.stepThreshold * 0.1f, num4 * 0.015f);
								if (flag)
								{
									num11 *= 0.5f;
								}
								if (k == 0)
								{
									num11 *= 0.9f;
								}
								if (!this.StepBlocked(this.footsteps[k].position, vector7, rootBone.solverPosition) && (num10 > num11 || num9 > this.angleThreshold))
								{
									float num12 = 0f;
									num12 -= num10;
									if (num12 > num6)
									{
										num5 = k;
										num6 = num12;
									}
								}
							}
						}
					}
					if (num5 != -1)
					{
						Vector3 p = a2 + rootBone.solverRotation * this.footsteps[num5].characterSpaceOffset;
						this.footsteps[num5].stepSpeed = UnityEngine.Random.Range(this.stepSpeed, this.stepSpeed * 1.5f);
						this.footsteps[num5].StepTo(p, quaternion);
					}
				}
				this.footsteps[0].Update(this.stepInterpolation, this.onLeftFootstep);
				this.footsteps[1].Update(this.stepInterpolation, this.onRightFootstep);
				leftFootPosition = this.footsteps[0].position;
				rightFootPosition = this.footsteps[1].position;
				leftFootPosition = V3Tools.PointToPlane(leftFootPosition, leftLeg.lastBone.readPosition, vector);
				rightFootPosition = V3Tools.PointToPlane(rightFootPosition, rightLeg.lastBone.readPosition, vector);
				leftFootOffset = this.stepHeight.Evaluate(this.footsteps[0].stepProgress);
				rightFootOffset = this.stepHeight.Evaluate(this.footsteps[1].stepProgress);
				leftHeelOffset = this.heelHeight.Evaluate(this.footsteps[0].stepProgress);
				rightHeelOffset = this.heelHeight.Evaluate(this.footsteps[1].stepProgress);
				leftFootRotation = this.footsteps[0].rotation;
				rightFootRotation = this.footsteps[1].rotation;
			}

			public Vector3 leftFootstepPosition
			{
				get
				{
					return this.footsteps[0].position;
				}
			}

			public Vector3 rightFootstepPosition
			{
				get
				{
					return this.footsteps[1].position;
				}
			}

			public Quaternion leftFootstepRotation
			{
				get
				{
					return this.footsteps[0].rotation;
				}
			}

			public Quaternion rightFootstepRotation
			{
				get
				{
					return this.footsteps[1].rotation;
				}
			}

			private bool StepBlocked(Vector3 fromPosition, Vector3 toPosition, Vector3 rootPosition)
			{
				if (this.blockingLayers == -1 || !this.blockingEnabled)
				{
					return false;
				}
				Vector3 vector = fromPosition;
				vector.y = rootPosition.y + this.raycastHeight + this.raycastRadius;
				Vector3 direction = toPosition - vector;
				direction.y = 0f;
				RaycastHit raycastHit;
				if (this.raycastRadius <= 0f)
				{
					return Physics.Raycast(vector, direction, out raycastHit, direction.magnitude, this.blockingLayers);
				}
				return Physics.SphereCast(vector, this.raycastRadius, direction, out raycastHit, direction.magnitude, this.blockingLayers);
			}

			private bool CanStep()
			{
				foreach (IKSolverVR.Footstep footstep in this.footsteps)
				{
					if (footstep.isStepping && footstep.stepProgress < 0.8f)
					{
						return false;
					}
				}
				return true;
			}

			private static bool GetLineSphereCollision(Vector3 lineStart, Vector3 lineEnd, Vector3 sphereCenter, float sphereRadius)
			{
				Vector3 forward = lineEnd - lineStart;
				Vector3 vector = sphereCenter - lineStart;
				float magnitude = vector.magnitude;
				float num = magnitude - sphereRadius;
				if (num > forward.magnitude)
				{
					return false;
				}
				Quaternion rotation = Quaternion.LookRotation(forward, vector);
				Vector3 vector2 = Quaternion.Inverse(rotation) * vector;
				if (vector2.z < 0f)
				{
					return num < 0f;
				}
				return vector2.y - sphereRadius < 0f;
			}

			[Tooltip("Used for blending in/out of procedural locomotion.")]
			[Range(0f, 1f)]
			public float weight = 1f;

			[Tooltip("Tries to maintain this distance between the legs.")]
			public float footDistance = 0.3f;

			[Tooltip("Makes a step only if step target position is at least this far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past the 'Angle Threshold'.")]
			public float stepThreshold = 0.4f;

			[Tooltip("Makes a step only if step target position is at least 'Step Threshold' far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past this value.")]
			public float angleThreshold = 60f;

			[Tooltip("Multiplies angle of the center of mass - center of pressure vector. Larger value makes the character step sooner if losing balance.")]
			public float comAngleMlp = 1f;

			[Tooltip("Maximum magnitude of head/hand target velocity used in prediction.")]
			public float maxVelocity = 0.4f;

			[Tooltip("The amount of head/hand target velocity prediction.")]
			public float velocityFactor = 0.4f;

			[Tooltip("How much can a leg be extended before it is forced to step to another position? 1 means fully stretched.")]
			[Range(0.9f, 1f)]
			public float maxLegStretch = 1f;

			[Tooltip("The speed of lerping the root of the character towards the horizontal mid-point of the footsteps.")]
			public float rootSpeed = 20f;

			[Tooltip("The speed of steps.")]
			public float stepSpeed = 3f;

			[Tooltip("The height of the foot by normalized step progress (0 - 1).")]
			public AnimationCurve stepHeight;

			[Tooltip("The height offset of the heel by normalized step progress (0 - 1).")]
			public AnimationCurve heelHeight;

			[Range(0f, 180f)]
			[Tooltip("Rotates the foot while the leg is not stepping to relax the twist rotation of the leg if ideal rotation is past this angle.")]
			public float relaxLegTwistMinAngle = 20f;

			[Tooltip("The speed of rotating the foot while the leg is not stepping to relax the twist rotation of the leg.")]
			public float relaxLegTwistSpeed = 400f;

			[Tooltip("Interpolation mode of the step.")]
			public InterpolationMode stepInterpolation = InterpolationMode.InOutSine;

			[Tooltip("Offset for the approximated center of mass.")]
			public Vector3 offset;

			[HideInInspector]
			public bool blockingEnabled;

			[HideInInspector]
			public LayerMask blockingLayers;

			[HideInInspector]
			public float raycastRadius = 0.2f;

			[HideInInspector]
			public float raycastHeight = 0.2f;

			[Tooltip("Called when the left foot has finished a step.")]
			public UnityEvent onLeftFootstep = new UnityEvent();

			[Tooltip("Called when the right foot has finished a step")]
			public UnityEvent onRightFootstep = new UnityEvent();

			private IKSolverVR.Footstep[] footsteps = new IKSolverVR.Footstep[0];

			private Vector3 lastComPosition;

			private Vector3 comVelocity;

			private int leftFootIndex;

			private int rightFootIndex;
		}

		[Serializable]
		public class Spine : IKSolverVR.BodyPart
		{
			public IKSolverVR.VirtualBone pelvis
			{
				get
				{
					return this.bones[this.pelvisIndex];
				}
			}

			public IKSolverVR.VirtualBone firstSpineBone
			{
				get
				{
					return this.bones[this.spineIndex];
				}
			}

			public IKSolverVR.VirtualBone chest
			{
				get
				{
					if (this.hasChest)
					{
						return this.bones[this.chestIndex];
					}
					return this.bones[this.spineIndex];
				}
			}

			private IKSolverVR.VirtualBone neck
			{
				get
				{
					return this.bones[this.neckIndex];
				}
			}

			public IKSolverVR.VirtualBone head
			{
				get
				{
					return this.bones[this.headIndex];
				}
			}

			public Quaternion anchorRotation { get; private set; }

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, int rootIndex, int index)
			{
				Vector3 vector = positions[index];
				Quaternion quaternion = rotations[index];
				Vector3 vector2 = positions[index + 1];
				Quaternion quaternion2 = rotations[index + 1];
				Vector3 vector3 = positions[index + 2];
				Quaternion quaternion3 = rotations[index + 2];
				Vector3 position = positions[index + 3];
				Quaternion rotation = rotations[index + 3];
				Vector3 vector4 = positions[index + 4];
				Quaternion quaternion4 = rotations[index + 4];
				if (!hasChest)
				{
					vector3 = vector2;
					quaternion3 = quaternion2;
				}
				if (!this.initiated)
				{
					this.hasChest = hasChest;
					this.hasNeck = hasNeck;
					this.headHeight = V3Tools.ExtractVertical(vector4 - positions[0], rotations[0] * Vector3.up, 1f).magnitude;
					int num = 3;
					if (hasChest)
					{
						num++;
					}
					if (hasNeck)
					{
						num++;
					}
					this.bones = new IKSolverVR.VirtualBone[num];
					this.chestIndex = ((!hasChest) ? 1 : 2);
					this.neckIndex = 1;
					if (hasChest)
					{
						this.neckIndex++;
					}
					if (hasNeck)
					{
						this.neckIndex++;
					}
					this.headIndex = 2;
					if (hasChest)
					{
						this.headIndex++;
					}
					if (hasNeck)
					{
						this.headIndex++;
					}
					this.bones[0] = new IKSolverVR.VirtualBone(vector, quaternion);
					this.bones[1] = new IKSolverVR.VirtualBone(vector2, quaternion2);
					if (hasChest)
					{
						this.bones[this.chestIndex] = new IKSolverVR.VirtualBone(vector3, quaternion3);
					}
					if (hasNeck)
					{
						this.bones[this.neckIndex] = new IKSolverVR.VirtualBone(position, rotation);
					}
					this.bones[this.headIndex] = new IKSolverVR.VirtualBone(vector4, quaternion4);
					this.pelvisRotationOffset = Quaternion.identity;
					this.chestRotationOffset = Quaternion.identity;
					this.headRotationOffset = Quaternion.identity;
					this.anchorRelativeToHead = Quaternion.Inverse(quaternion4) * rotations[0];
					this.pelvisRelativeRotation = Quaternion.Inverse(quaternion4) * quaternion;
					this.chestRelativeRotation = Quaternion.Inverse(quaternion4) * quaternion3;
					this.chestForward = Quaternion.Inverse(quaternion3) * (rotations[0] * Vector3.forward);
					this.faceDirection = rotations[0] * Vector3.forward;
					this.IKPositionHead = vector4;
					this.IKRotationHead = quaternion4;
					this.IKPositionPelvis = vector;
					this.IKRotationPelvis = quaternion;
					this.goalPositionChest = vector3 + rotations[0] * Vector3.forward;
				}
				this.bones[0].Read(vector, quaternion);
				this.bones[1].Read(vector2, quaternion2);
				if (hasChest)
				{
					this.bones[this.chestIndex].Read(vector3, quaternion3);
				}
				if (hasNeck)
				{
					this.bones[this.neckIndex].Read(position, rotation);
				}
				this.bones[this.headIndex].Read(vector4, quaternion4);
				float num2 = Vector3.Distance(vector, vector4);
				this.sizeMlp = num2 / 0.7f;
			}

			public override void PreSolve()
			{
				if (this.headTarget != null)
				{
					this.IKPositionHead = this.headTarget.position;
					this.IKRotationHead = this.headTarget.rotation;
				}
				if (this.chestGoal != null)
				{
					this.goalPositionChest = this.chestGoal.position;
				}
				if (this.pelvisTarget != null)
				{
					this.IKPositionPelvis = this.pelvisTarget.position;
					this.IKRotationPelvis = this.pelvisTarget.rotation;
				}
				this.headPosition = V3Tools.Lerp(this.head.solverPosition, this.IKPositionHead, this.positionWeight);
				this.headRotation = QuaTools.Lerp(this.head.solverRotation, this.IKRotationHead, this.rotationWeight);
			}

			public override void ApplyOffsets()
			{
				this.headPosition += this.headPositionOffset;
				Vector3 vector = this.rootRotation * Vector3.up;
				if (vector == Vector3.up)
				{
					this.headPosition.y = Math.Max(this.rootPosition.y + this.minHeadHeight, this.headPosition.y);
				}
				else
				{
					Vector3 vector2 = this.headPosition - this.rootPosition;
					Vector3 b = V3Tools.ExtractHorizontal(vector2, vector, 1f);
					Vector3 vector3 = vector2 - b;
					float num = Vector3.Dot(vector3, vector);
					if (num > 0f)
					{
						if (vector3.magnitude < this.minHeadHeight)
						{
							vector3 = vector3.normalized * this.minHeadHeight;
						}
					}
					else
					{
						vector3 = -vector3.normalized * this.minHeadHeight;
					}
					this.headPosition = this.rootPosition + b + vector3;
				}
				this.headRotation = this.headRotationOffset * this.headRotation;
				this.headDeltaPosition = this.headPosition - this.head.solverPosition;
				this.pelvisDeltaRotation = QuaTools.FromToRotation(this.pelvis.solverRotation, this.headRotation * this.pelvisRelativeRotation);
				this.anchorRotation = this.headRotation * this.anchorRelativeToHead;
			}

			private void CalculateChestTargetRotation(IKSolverVR.VirtualBone rootBone, IKSolverVR.Arm[] arms)
			{
				this.chestTargetRotation = this.headRotation * this.chestRelativeRotation;
				this.AdjustChestByHands(ref this.chestTargetRotation, arms);
				this.faceDirection = Vector3.Cross(this.anchorRotation * Vector3.right, rootBone.readRotation * Vector3.up) + this.anchorRotation * Vector3.forward;
			}

			public void Solve(IKSolverVR.VirtualBone rootBone, IKSolverVR.Leg[] legs, IKSolverVR.Arm[] arms)
			{
				this.CalculateChestTargetRotation(rootBone, arms);
				if (this.maxRootAngle < 180f)
				{
					Vector3 vector = Quaternion.Inverse(rootBone.solverRotation) * this.faceDirection;
					float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					float angle = 0f;
					float num2 = 25f;
					if (num > num2)
					{
						angle = num - num2;
					}
					if (num < -num2)
					{
						angle = num + num2;
					}
					rootBone.solverRotation = Quaternion.AngleAxis(angle, rootBone.readRotation * Vector3.up) * rootBone.solverRotation;
				}
				Vector3 solverPosition = this.pelvis.solverPosition;
				this.TranslatePelvis(legs, this.headDeltaPosition, this.pelvisDeltaRotation);
				IKSolverVR.VirtualBone.SolveFABRIK(this.bones, Vector3.Lerp(this.pelvis.solverPosition, solverPosition, this.maintainPelvisPosition) + this.pelvisPositionOffset - this.chestPositionOffset, this.headPosition - this.chestPositionOffset, 1f, 1f, 1, base.mag);
				this.Bend(this.bones, this.pelvisIndex, this.chestIndex, this.chestTargetRotation, this.chestRotationOffset, this.chestClampWeight, false, this.neckStiffness);
				if (this.chestGoalWeight > 0f)
				{
					Quaternion targetRotation = Quaternion.FromToRotation(this.bones[this.chestIndex].solverRotation * this.chestForward, this.goalPositionChest - this.bones[this.chestIndex].solverPosition) * this.bones[this.chestIndex].solverRotation;
					this.Bend(this.bones, this.pelvisIndex, this.chestIndex, targetRotation, this.chestRotationOffset, this.chestClampWeight, false, this.chestGoalWeight);
				}
				this.InverseTranslateToHead(legs, false, false, Vector3.zero, 1f);
				IKSolverVR.VirtualBone.SolveFABRIK(this.bones, Vector3.Lerp(this.pelvis.solverPosition, solverPosition, this.maintainPelvisPosition) + this.pelvisPositionOffset - this.chestPositionOffset, this.headPosition - this.chestPositionOffset, 1f, 1f, 1, base.mag);
				this.Bend(this.bones, this.neckIndex, this.headIndex, this.headRotation, this.headClampWeight, true, 1f);
				this.SolvePelvis();
			}

			private void SolvePelvis()
			{
				if (this.pelvisPositionWeight > 0f)
				{
					Quaternion solverRotation = this.head.solverRotation;
					Vector3 b = (this.IKPositionPelvis + this.pelvisPositionOffset - this.pelvis.solverPosition) * this.pelvisPositionWeight;
					foreach (IKSolverVR.VirtualBone virtualBone in this.bones)
					{
						virtualBone.solverPosition += b;
					}
					Vector3 bendNormal = this.anchorRotation * Vector3.right;
					if (this.hasChest && this.hasNeck)
					{
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.pelvisIndex, this.spineIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 0.6f);
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.spineIndex, this.chestIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 0.6f);
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.chestIndex, this.neckIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 1f);
					}
					else if (this.hasChest && !this.hasNeck)
					{
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.pelvisIndex, this.spineIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 0.75f);
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.spineIndex, this.chestIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 1f);
					}
					else if (!this.hasChest && this.hasNeck)
					{
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.pelvisIndex, this.spineIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 0.75f);
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.spineIndex, this.neckIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight * 1f);
					}
					else if (!this.hasNeck && !this.hasChest)
					{
						IKSolverVR.VirtualBone.SolveTrigonometric(this.bones, this.pelvisIndex, this.spineIndex, this.headIndex, this.headPosition, bendNormal, this.pelvisPositionWeight);
					}
					this.head.solverRotation = solverRotation;
				}
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations)
			{
				solvedPositions[this.index] = this.bones[0].solverPosition;
				solvedRotations[this.index] = this.bones[0].solverRotation;
				solvedRotations[this.index + 1] = this.bones[1].solverRotation;
				if (this.hasChest)
				{
					solvedRotations[this.index + 2] = this.bones[this.chestIndex].solverRotation;
				}
				if (this.hasNeck)
				{
					solvedRotations[this.index + 3] = this.bones[this.neckIndex].solverRotation;
				}
				solvedRotations[this.index + 4] = this.bones[this.headIndex].solverRotation;
			}

			public override void ResetOffsets()
			{
				this.pelvisPositionOffset = Vector3.zero;
				this.chestPositionOffset = Vector3.zero;
				this.headPositionOffset = this.locomotionHeadPositionOffset;
				this.pelvisRotationOffset = Quaternion.identity;
				this.chestRotationOffset = Quaternion.identity;
				this.headRotationOffset = Quaternion.identity;
			}

			private void AdjustChestByHands(ref Quaternion chestTargetRotation, IKSolverVR.Arm[] arms)
			{
				Quaternion rotation = Quaternion.Inverse(this.anchorRotation);
				Vector3 vector = rotation * (arms[0].position - this.headPosition) / this.sizeMlp;
				Vector3 vector2 = rotation * (arms[1].position - this.headPosition) / this.sizeMlp;
				Vector3 forward = Vector3.forward;
				forward.x += vector.x * Mathf.Abs(vector.x);
				forward.x += vector.z * Mathf.Abs(vector.z);
				forward.x += vector2.x * Mathf.Abs(vector2.x);
				forward.x -= vector2.z * Mathf.Abs(vector2.z);
				forward.x *= 5f;
				Quaternion lhs = Quaternion.FromToRotation(Vector3.forward, forward);
				chestTargetRotation = lhs * chestTargetRotation;
				Vector3 up = Vector3.up;
				up.x += vector.y;
				up.x -= vector2.y;
				up.x *= 0.5f;
				lhs = Quaternion.FromToRotation(Vector3.up, this.anchorRotation * up);
				chestTargetRotation = lhs * chestTargetRotation;
			}

			public void InverseTranslateToHead(IKSolverVR.Leg[] legs, bool limited, bool useCurrentLegMag, Vector3 offset, float w)
			{
				Vector3 vector = this.pelvis.solverPosition + (this.headPosition + offset - this.head.solverPosition) * w * (1f - this.pelvisPositionWeight);
				base.MovePosition((!limited) ? vector : this.LimitPelvisPosition(legs, vector, useCurrentLegMag, 2));
			}

			private void TranslatePelvis(IKSolverVR.Leg[] legs, Vector3 deltaPosition, Quaternion deltaRotation)
			{
				Vector3 solverPosition = this.head.solverPosition;
				deltaRotation = QuaTools.ClampRotation(deltaRotation, this.chestClampWeight, 2);
				Quaternion quaternion = Quaternion.Slerp(Quaternion.identity, deltaRotation, this.bodyRotStiffness);
				quaternion = Quaternion.Slerp(quaternion, QuaTools.FromToRotation(this.pelvis.solverRotation, this.IKRotationPelvis), this.pelvisRotationWeight);
				IKSolverVR.VirtualBone.RotateAroundPoint(this.bones, 0, this.pelvis.solverPosition, this.pelvisRotationOffset * quaternion);
				deltaPosition -= this.head.solverPosition - solverPosition;
				Vector3 a = this.rootRotation * Vector3.forward;
				a.y = 0f;
				float d = deltaPosition.y * 0.35f * this.headHeight;
				deltaPosition += a * d;
				base.MovePosition(this.LimitPelvisPosition(legs, this.pelvis.solverPosition + deltaPosition * this.bodyPosStiffness, false, 2));
			}

			private Vector3 LimitPelvisPosition(IKSolverVR.Leg[] legs, Vector3 pelvisPosition, bool useCurrentLegMag, int it = 2)
			{
				if (useCurrentLegMag)
				{
					foreach (IKSolverVR.Leg leg in legs)
					{
						leg.currentMag = Vector3.Distance(leg.thigh.solverPosition, leg.lastBone.solverPosition);
					}
				}
				for (int j = 0; j < it; j++)
				{
					foreach (IKSolverVR.Leg leg2 in legs)
					{
						Vector3 b = pelvisPosition - this.pelvis.solverPosition;
						Vector3 vector = leg2.thigh.solverPosition + b;
						Vector3 vector2 = vector - leg2.position;
						float maxLength = (!useCurrentLegMag) ? leg2.mag : leg2.currentMag;
						Vector3 a = leg2.position + Vector3.ClampMagnitude(vector2, maxLength);
						pelvisPosition += a - vector;
					}
				}
				return pelvisPosition;
			}

			private void Bend(IKSolverVR.VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, float clampWeight, bool uniformWeight, float w)
			{
				if (w <= 0f)
				{
					return;
				}
				if (bones.Length == 0)
				{
					return;
				}
				int num = lastIndex + 1 - firstIndex;
				if (num < 1)
				{
					return;
				}
				Quaternion quaternion = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
				quaternion = QuaTools.ClampRotation(quaternion, clampWeight, 2);
				float num2 = (!uniformWeight) ? 0f : (1f / (float)num);
				for (int i = firstIndex; i < lastIndex + 1; i++)
				{
					if (!uniformWeight)
					{
						num2 = Mathf.Clamp((float)((i - firstIndex + 1) / num), 0f, 1f);
					}
					IKSolverVR.VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.identity, quaternion, num2 * w));
				}
			}

			private void Bend(IKSolverVR.VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, Quaternion rotationOffset, float clampWeight, bool uniformWeight, float w)
			{
				if (w <= 0f)
				{
					return;
				}
				if (bones.Length == 0)
				{
					return;
				}
				int num = lastIndex + 1 - firstIndex;
				if (num < 1)
				{
					return;
				}
				Quaternion quaternion = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
				quaternion = QuaTools.ClampRotation(quaternion, clampWeight, 2);
				float num2 = (!uniformWeight) ? 0f : (1f / (float)num);
				for (int i = firstIndex; i < lastIndex + 1; i++)
				{
					if (!uniformWeight)
					{
						num2 = Mathf.Clamp((float)((i - firstIndex + 1) / num), 0f, 1f);
					}
					IKSolverVR.VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.Slerp(Quaternion.identity, rotationOffset, num2), quaternion, num2 * w));
				}
			}

			[Tooltip("The head target.")]
			public Transform headTarget;

			[Tooltip("The pelvis target, useful with seated rigs.")]
			public Transform pelvisTarget;

			[Range(0f, 1f)]
			[Tooltip("Positional weight of the head target.")]
			public float positionWeight = 1f;

			[Range(0f, 1f)]
			[Tooltip("Rotational weight of the head target.")]
			public float rotationWeight = 1f;

			[Tooltip("Positional weight of the pelvis target.")]
			[Range(0f, 1f)]
			public float pelvisPositionWeight;

			[Range(0f, 1f)]
			[Tooltip("Rotational weight of the pelvis target.")]
			public float pelvisRotationWeight;

			[Tooltip("If 'Chest Goal Weight' is greater than 0, the chest will be turned towards this Transform.")]
			public Transform chestGoal;

			[Tooltip("Rotational weight of the chest target.")]
			[Range(0f, 1f)]
			public float chestGoalWeight;

			[Tooltip("Minimum height of the head from the root of the character.")]
			public float minHeadHeight = 0.8f;

			[Tooltip("Determines how much the body will follow the position of the head.")]
			[Range(0f, 1f)]
			public float bodyPosStiffness = 0.55f;

			[Range(0f, 1f)]
			[Tooltip("Determines how much the body will follow the rotation of the head.")]
			public float bodyRotStiffness = 0.1f;

			[Range(0f, 1f)]
			[FormerlySerializedAs("chestRotationWeight")]
			[Tooltip("Determines how much the chest will rotate to the rotation of the head.")]
			public float neckStiffness = 0.2f;

			[Range(0f, 1f)]
			[Tooltip("Clamps chest rotation.")]
			public float chestClampWeight = 0.5f;

			[Range(0f, 1f)]
			[Tooltip("Clamps head rotation.")]
			public float headClampWeight = 0.6f;

			[Range(0f, 1f)]
			[Tooltip("How much will the pelvis maintain it's animated position?")]
			public float maintainPelvisPosition = 0.2f;

			[Range(0f, 180f)]
			[Tooltip("Will automatically rotate the root of the character if the head target has turned past this angle.")]
			public float maxRootAngle = 25f;

			[HideInInspector]
			[NonSerialized]
			public Vector3 IKPositionHead;

			[HideInInspector]
			[NonSerialized]
			public Quaternion IKRotationHead = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Vector3 IKPositionPelvis;

			[HideInInspector]
			[NonSerialized]
			public Quaternion IKRotationPelvis = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Vector3 goalPositionChest;

			[HideInInspector]
			[NonSerialized]
			public Vector3 pelvisPositionOffset;

			[HideInInspector]
			[NonSerialized]
			public Vector3 chestPositionOffset;

			[HideInInspector]
			[NonSerialized]
			public Vector3 headPositionOffset;

			[HideInInspector]
			[NonSerialized]
			public Quaternion pelvisRotationOffset = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Quaternion chestRotationOffset = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Quaternion headRotationOffset = Quaternion.identity;

			[HideInInspector]
			[NonSerialized]
			public Vector3 faceDirection;

			[HideInInspector]
			[NonSerialized]
			public Vector3 locomotionHeadPositionOffset;

			[HideInInspector]
			[NonSerialized]
			public Vector3 headPosition;

			private Quaternion headRotation = Quaternion.identity;

			private Quaternion anchorRelativeToHead = Quaternion.identity;

			private Quaternion pelvisRelativeRotation = Quaternion.identity;

			private Quaternion chestRelativeRotation = Quaternion.identity;

			private Vector3 headDeltaPosition;

			private Quaternion pelvisDeltaRotation = Quaternion.identity;

			private Quaternion chestTargetRotation = Quaternion.identity;

			private int pelvisIndex;

			private int spineIndex = 1;

			private int chestIndex = -1;

			private int neckIndex = -1;

			private int headIndex = -1;

			private float length;

			private bool hasChest;

			private bool hasNeck;

			private float headHeight;

			private float sizeMlp;

			private Vector3 chestForward;
		}

		[Serializable]
		public enum PositionOffset
		{
			Pelvis,
			Chest,
			Head,
			LeftHand,
			RightHand,
			LeftFoot,
			RightFoot,
			LeftHeel,
			RightHeel
		}

		[Serializable]
		public enum RotationOffset
		{
			Pelvis,
			Chest,
			Head
		}

		[Serializable]
		public class VirtualBone
		{
			public VirtualBone(Vector3 position, Quaternion rotation)
			{
				this.Read(position, rotation);
			}

			public void Read(Vector3 position, Quaternion rotation)
			{
				this.readPosition = position;
				this.readRotation = rotation;
				this.solverPosition = position;
				this.solverRotation = rotation;
			}

			public static void SwingRotation(IKSolverVR.VirtualBone[] bones, int index, Vector3 swingTarget, float weight = 1f)
			{
				if (weight <= 0f)
				{
					return;
				}
				Quaternion quaternion = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
				if (weight < 1f)
				{
					quaternion = Quaternion.Lerp(Quaternion.identity, quaternion, weight);
				}
				for (int i = index; i < bones.Length; i++)
				{
					bones[i].solverRotation = quaternion * bones[i].solverRotation;
				}
			}

			public static float PreSolve(ref IKSolverVR.VirtualBone[] bones)
			{
				float num = 0f;
				for (int i = 0; i < bones.Length; i++)
				{
					if (i < bones.Length - 1)
					{
						bones[i].sqrMag = (bones[i + 1].solverPosition - bones[i].solverPosition).sqrMagnitude;
						bones[i].length = Mathf.Sqrt(bones[i].sqrMag);
						num += bones[i].length;
						bones[i].axis = Quaternion.Inverse(bones[i].solverRotation) * (bones[i + 1].solverPosition - bones[i].solverPosition);
					}
					else
					{
						bones[i].sqrMag = 0f;
						bones[i].length = 0f;
					}
				}
				return num;
			}

			public static void RotateAroundPoint(IKSolverVR.VirtualBone[] bones, int index, Vector3 point, Quaternion rotation)
			{
				for (int i = index; i < bones.Length; i++)
				{
					if (bones[i] != null)
					{
						Vector3 point2 = bones[i].solverPosition - point;
						bones[i].solverPosition = point + rotation * point2;
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateBy(IKSolverVR.VirtualBone[] bones, int index, Quaternion rotation)
			{
				for (int i = index; i < bones.Length; i++)
				{
					if (bones[i] != null)
					{
						Vector3 point = bones[i].solverPosition - bones[index].solverPosition;
						bones[i].solverPosition = bones[index].solverPosition + rotation * point;
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateBy(IKSolverVR.VirtualBone[] bones, Quaternion rotation)
			{
				for (int i = 0; i < bones.Length; i++)
				{
					if (bones[i] != null)
					{
						if (i > 0)
						{
							Vector3 point = bones[i].solverPosition - bones[0].solverPosition;
							bones[i].solverPosition = bones[0].solverPosition + rotation * point;
						}
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateTo(IKSolverVR.VirtualBone[] bones, int index, Quaternion rotation)
			{
				Quaternion rotation2 = QuaTools.FromToRotation(bones[index].solverRotation, rotation);
				IKSolverVR.VirtualBone.RotateAroundPoint(bones, index, bones[index].solverPosition, rotation2);
			}

			public static void SolveTrigonometric(IKSolverVR.VirtualBone[] bones, int first, int second, int third, Vector3 targetPosition, Vector3 bendNormal, float weight)
			{
				if (weight <= 0f)
				{
					return;
				}
				targetPosition = Vector3.Lerp(bones[third].solverPosition, targetPosition, weight);
				Vector3 vector = targetPosition - bones[first].solverPosition;
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude == 0f)
				{
					return;
				}
				float directionMag = Mathf.Sqrt(sqrMagnitude);
				float sqrMagnitude2 = (bones[second].solverPosition - bones[first].solverPosition).sqrMagnitude;
				float sqrMagnitude3 = (bones[third].solverPosition - bones[second].solverPosition).sqrMagnitude;
				Vector3 bendDirection = Vector3.Cross(vector, bendNormal);
				Vector3 directionToBendPoint = IKSolverVR.VirtualBone.GetDirectionToBendPoint(vector, directionMag, bendDirection, sqrMagnitude2, sqrMagnitude3);
				Quaternion quaternion = Quaternion.FromToRotation(bones[second].solverPosition - bones[first].solverPosition, directionToBendPoint);
				if (weight < 1f)
				{
					quaternion = Quaternion.Lerp(Quaternion.identity, quaternion, weight);
				}
				IKSolverVR.VirtualBone.RotateAroundPoint(bones, first, bones[first].solverPosition, quaternion);
				Quaternion quaternion2 = Quaternion.FromToRotation(bones[third].solverPosition - bones[second].solverPosition, targetPosition - bones[second].solverPosition);
				if (weight < 1f)
				{
					quaternion2 = Quaternion.Lerp(Quaternion.identity, quaternion2, weight);
				}
				IKSolverVR.VirtualBone.RotateAroundPoint(bones, second, bones[second].solverPosition, quaternion2);
			}

			private static Vector3 GetDirectionToBendPoint(Vector3 direction, float directionMag, Vector3 bendDirection, float sqrMag1, float sqrMag2)
			{
				float num = (directionMag * directionMag + (sqrMag1 - sqrMag2)) / 2f / directionMag;
				float y = (float)Math.Sqrt((double)Mathf.Clamp(sqrMag1 - num * num, 0f, float.PositiveInfinity));
				if (direction == Vector3.zero)
				{
					return Vector3.zero;
				}
				return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, num);
			}

			public static void SolveFABRIK(IKSolverVR.VirtualBone[] bones, Vector3 startPosition, Vector3 targetPosition, float weight, float minNormalizedTargetDistance, int iterations, float length)
			{
				if (weight <= 0f)
				{
					return;
				}
				if (minNormalizedTargetDistance > 0f)
				{
					Vector3 a = targetPosition - startPosition;
					float magnitude = a.magnitude;
					targetPosition = startPosition + a / magnitude * Mathf.Max(length * minNormalizedTargetDistance, magnitude);
				}
				for (int i = 0; i < iterations; i++)
				{
					bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, targetPosition, weight);
					for (int j = bones.Length - 2; j > -1; j--)
					{
						bones[j].solverPosition = IKSolverVR.VirtualBone.SolveFABRIKJoint(bones[j].solverPosition, bones[j + 1].solverPosition, bones[j].length);
					}
					bones[0].solverPosition = startPosition;
					for (int k = 1; k < bones.Length; k++)
					{
						bones[k].solverPosition = IKSolverVR.VirtualBone.SolveFABRIKJoint(bones[k].solverPosition, bones[k - 1].solverPosition, bones[k - 1].length);
					}
				}
				for (int l = 0; l < bones.Length - 1; l++)
				{
					IKSolverVR.VirtualBone.SwingRotation(bones, l, bones[l + 1].solverPosition, 1f);
				}
			}

			private static Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
			{
				return pos2 + (pos1 - pos2).normalized * length;
			}

			public static void SolveCCD(IKSolverVR.VirtualBone[] bones, Vector3 targetPosition, float weight, int iterations)
			{
				if (weight <= 0f)
				{
					return;
				}
				for (int i = 0; i < iterations; i++)
				{
					for (int j = bones.Length - 2; j > -1; j--)
					{
						Vector3 fromDirection = bones[bones.Length - 1].solverPosition - bones[j].solverPosition;
						Vector3 toDirection = targetPosition - bones[j].solverPosition;
						Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
						if (weight >= 1f)
						{
							IKSolverVR.VirtualBone.RotateBy(bones, j, quaternion);
						}
						else
						{
							IKSolverVR.VirtualBone.RotateBy(bones, j, Quaternion.Lerp(Quaternion.identity, quaternion, weight));
						}
					}
				}
			}

			public Vector3 readPosition;

			public Quaternion readRotation;

			public Vector3 solverPosition;

			public Quaternion solverRotation;

			public float length;

			public float sqrMag;

			public Vector3 axis;
		}
	}
}
