using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class IKSolverFullBodyBiped : IKSolverFullBody
	{
		public IKEffector bodyEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.Body);
			}
		}

		public IKEffector leftShoulderEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.LeftShoulder);
			}
		}

		public IKEffector rightShoulderEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.RightShoulder);
			}
		}

		public IKEffector leftThighEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.LeftThigh);
			}
		}

		public IKEffector rightThighEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.RightThigh);
			}
		}

		public IKEffector leftHandEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.LeftHand);
			}
		}

		public IKEffector rightHandEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.RightHand);
			}
		}

		public IKEffector leftFootEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.LeftFoot);
			}
		}

		public IKEffector rightFootEffector
		{
			get
			{
				return this.GetEffector(FullBodyBipedEffector.RightFoot);
			}
		}

		public FBIKChain leftArmChain
		{
			get
			{
				return this.chain[1];
			}
		}

		public FBIKChain rightArmChain
		{
			get
			{
				return this.chain[2];
			}
		}

		public FBIKChain leftLegChain
		{
			get
			{
				return this.chain[3];
			}
		}

		public FBIKChain rightLegChain
		{
			get
			{
				return this.chain[4];
			}
		}

		public IKMappingLimb leftArmMapping
		{
			get
			{
				return this.limbMappings[0];
			}
		}

		public IKMappingLimb rightArmMapping
		{
			get
			{
				return this.limbMappings[1];
			}
		}

		public IKMappingLimb leftLegMapping
		{
			get
			{
				return this.limbMappings[2];
			}
		}

		public IKMappingLimb rightLegMapping
		{
			get
			{
				return this.limbMappings[3];
			}
		}

		public IKMappingBone headMapping
		{
			get
			{
				return this.boneMappings[0];
			}
		}

		public void SetChainWeights(FullBodyBipedChain c, float pull, float reach = 0f)
		{
			this.GetChain(c).pull = pull;
			this.GetChain(c).reach = reach;
		}

		public void SetEffectorWeights(FullBodyBipedEffector effector, float positionWeight, float rotationWeight)
		{
			this.GetEffector(effector).positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			this.GetEffector(effector).rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}

		public FBIKChain GetChain(FullBodyBipedChain c)
		{
			switch (c)
			{
			case FullBodyBipedChain.LeftArm:
				return this.chain[1];
			case FullBodyBipedChain.RightArm:
				return this.chain[2];
			case FullBodyBipedChain.LeftLeg:
				return this.chain[3];
			case FullBodyBipedChain.RightLeg:
				return this.chain[4];
			default:
				return null;
			}
		}

		public FBIKChain GetChain(FullBodyBipedEffector effector)
		{
			switch (effector)
			{
			case FullBodyBipedEffector.Body:
				return this.chain[0];
			case FullBodyBipedEffector.LeftShoulder:
				return this.chain[1];
			case FullBodyBipedEffector.RightShoulder:
				return this.chain[2];
			case FullBodyBipedEffector.LeftThigh:
				return this.chain[3];
			case FullBodyBipedEffector.RightThigh:
				return this.chain[4];
			case FullBodyBipedEffector.LeftHand:
				return this.chain[1];
			case FullBodyBipedEffector.RightHand:
				return this.chain[2];
			case FullBodyBipedEffector.LeftFoot:
				return this.chain[3];
			case FullBodyBipedEffector.RightFoot:
				return this.chain[4];
			default:
				return null;
			}
		}

		public IKEffector GetEffector(FullBodyBipedEffector effector)
		{
			switch (effector)
			{
			case FullBodyBipedEffector.Body:
				return this.effectors[0];
			case FullBodyBipedEffector.LeftShoulder:
				return this.effectors[1];
			case FullBodyBipedEffector.RightShoulder:
				return this.effectors[2];
			case FullBodyBipedEffector.LeftThigh:
				return this.effectors[3];
			case FullBodyBipedEffector.RightThigh:
				return this.effectors[4];
			case FullBodyBipedEffector.LeftHand:
				return this.effectors[5];
			case FullBodyBipedEffector.RightHand:
				return this.effectors[6];
			case FullBodyBipedEffector.LeftFoot:
				return this.effectors[7];
			case FullBodyBipedEffector.RightFoot:
				return this.effectors[8];
			default:
				return null;
			}
		}

		public IKEffector GetEndEffector(FullBodyBipedChain c)
		{
			switch (c)
			{
			case FullBodyBipedChain.LeftArm:
				return this.effectors[5];
			case FullBodyBipedChain.RightArm:
				return this.effectors[6];
			case FullBodyBipedChain.LeftLeg:
				return this.effectors[7];
			case FullBodyBipedChain.RightLeg:
				return this.effectors[8];
			default:
				return null;
			}
		}

		public IKMappingLimb GetLimbMapping(FullBodyBipedChain chain)
		{
			switch (chain)
			{
			case FullBodyBipedChain.LeftArm:
				return this.limbMappings[0];
			case FullBodyBipedChain.RightArm:
				return this.limbMappings[1];
			case FullBodyBipedChain.LeftLeg:
				return this.limbMappings[2];
			case FullBodyBipedChain.RightLeg:
				return this.limbMappings[3];
			default:
				return null;
			}
		}

		public IKMappingLimb GetLimbMapping(FullBodyBipedEffector effector)
		{
			switch (effector)
			{
			case FullBodyBipedEffector.LeftShoulder:
				return this.limbMappings[0];
			case FullBodyBipedEffector.RightShoulder:
				return this.limbMappings[1];
			case FullBodyBipedEffector.LeftThigh:
				return this.limbMappings[2];
			case FullBodyBipedEffector.RightThigh:
				return this.limbMappings[3];
			case FullBodyBipedEffector.LeftHand:
				return this.limbMappings[0];
			case FullBodyBipedEffector.RightHand:
				return this.limbMappings[1];
			case FullBodyBipedEffector.LeftFoot:
				return this.limbMappings[2];
			case FullBodyBipedEffector.RightFoot:
				return this.limbMappings[3];
			default:
				return null;
			}
		}

		public IKMappingSpine GetSpineMapping()
		{
			return this.spineMapping;
		}

		public IKMappingBone GetHeadMapping()
		{
			return this.boneMappings[0];
		}

		public IKConstraintBend GetBendConstraint(FullBodyBipedChain limb)
		{
			switch (limb)
			{
			case FullBodyBipedChain.LeftArm:
				return this.chain[1].bendConstraint;
			case FullBodyBipedChain.RightArm:
				return this.chain[2].bendConstraint;
			case FullBodyBipedChain.LeftLeg:
				return this.chain[3].bendConstraint;
			case FullBodyBipedChain.RightLeg:
				return this.chain[4].bendConstraint;
			default:
				return null;
			}
		}

		public override bool IsValid(ref string message)
		{
			if (!base.IsValid(ref message))
			{
				return false;
			}
			if (this.rootNode == null)
			{
				message = "Root Node bone is null. FBBIK will not initiate.";
				return false;
			}
			if (this.chain.Length != 5 || this.chain[0].nodes.Length != 1 || this.chain[1].nodes.Length != 3 || this.chain[2].nodes.Length != 3 || this.chain[3].nodes.Length != 3 || this.chain[4].nodes.Length != 3 || this.effectors.Length != 9 || this.limbMappings.Length != 4)
			{
				message = "Invalid FBBIK setup. Please right-click on the component header and select 'Reinitiate'.";
				return false;
			}
			return true;
		}

		public void SetToReferences(BipedReferences references, Transform rootNode = null)
		{
			this.root = references.root;
			if (rootNode == null)
			{
				rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(references);
			}
			this.rootNode = rootNode;
			if (this.chain == null || this.chain.Length != 5)
			{
				this.chain = new FBIKChain[5];
			}
			for (int i = 0; i < this.chain.Length; i++)
			{
				if (this.chain[i] == null)
				{
					this.chain[i] = new FBIKChain();
				}
			}
			this.chain[0].pin = 0f;
			this.chain[0].SetNodes(new Transform[]
			{
				rootNode
			});
			this.chain[0].children = new int[]
			{
				1,
				2,
				3,
				4
			};
			this.chain[1].SetNodes(new Transform[]
			{
				references.leftUpperArm,
				references.leftForearm,
				references.leftHand
			});
			this.chain[2].SetNodes(new Transform[]
			{
				references.rightUpperArm,
				references.rightForearm,
				references.rightHand
			});
			this.chain[3].SetNodes(new Transform[]
			{
				references.leftThigh,
				references.leftCalf,
				references.leftFoot
			});
			this.chain[4].SetNodes(new Transform[]
			{
				references.rightThigh,
				references.rightCalf,
				references.rightFoot
			});
			if (this.effectors.Length != 9)
			{
				this.effectors = new IKEffector[]
				{
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector(),
					new IKEffector()
				};
			}
			this.effectors[0].bone = rootNode;
			this.effectors[0].childBones = new Transform[]
			{
				references.leftThigh,
				references.rightThigh
			};
			this.effectors[1].bone = references.leftUpperArm;
			this.effectors[2].bone = references.rightUpperArm;
			this.effectors[3].bone = references.leftThigh;
			this.effectors[4].bone = references.rightThigh;
			this.effectors[5].bone = references.leftHand;
			this.effectors[6].bone = references.rightHand;
			this.effectors[7].bone = references.leftFoot;
			this.effectors[8].bone = references.rightFoot;
			this.effectors[5].planeBone1 = references.leftUpperArm;
			this.effectors[5].planeBone2 = references.rightUpperArm;
			this.effectors[5].planeBone3 = rootNode;
			this.effectors[6].planeBone1 = references.rightUpperArm;
			this.effectors[6].planeBone2 = references.leftUpperArm;
			this.effectors[6].planeBone3 = rootNode;
			this.effectors[7].planeBone1 = references.leftThigh;
			this.effectors[7].planeBone2 = references.rightThigh;
			this.effectors[7].planeBone3 = rootNode;
			this.effectors[8].planeBone1 = references.rightThigh;
			this.effectors[8].planeBone2 = references.leftThigh;
			this.effectors[8].planeBone3 = rootNode;
			this.chain[0].childConstraints = new FBIKChain.ChildConstraint[]
			{
				new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightThigh, 0f, 1f),
				new FBIKChain.ChildConstraint(references.rightUpperArm, references.leftThigh, 0f, 1f),
				new FBIKChain.ChildConstraint(references.leftUpperArm, references.rightUpperArm, 0f, 0f),
				new FBIKChain.ChildConstraint(references.leftThigh, references.rightThigh, 0f, 0f)
			};
			Transform[] array = new Transform[references.spine.Length + 1];
			array[0] = references.pelvis;
			for (int j = 0; j < references.spine.Length; j++)
			{
				array[j + 1] = references.spine[j];
			}
			if (this.spineMapping == null)
			{
				this.spineMapping = new IKMappingSpine();
				this.spineMapping.iterations = 3;
			}
			this.spineMapping.SetBones(array, references.leftUpperArm, references.rightUpperArm, references.leftThigh, references.rightThigh);
			int num = (references.head != null) ? 1 : 0;
			if (this.boneMappings.Length != num)
			{
				this.boneMappings = new IKMappingBone[num];
				for (int k = 0; k < this.boneMappings.Length; k++)
				{
					this.boneMappings[k] = new IKMappingBone();
				}
				if (num == 1)
				{
					this.boneMappings[0].maintainRotationWeight = 0f;
				}
			}
			if (this.boneMappings.Length != 0)
			{
				this.boneMappings[0].bone = references.head;
			}
			if (this.limbMappings.Length != 4)
			{
				this.limbMappings = new IKMappingLimb[]
				{
					new IKMappingLimb(),
					new IKMappingLimb(),
					new IKMappingLimb(),
					new IKMappingLimb()
				};
				this.limbMappings[2].maintainRotationWeight = 1f;
				this.limbMappings[3].maintainRotationWeight = 1f;
			}
			this.limbMappings[0].SetBones(references.leftUpperArm, references.leftForearm, references.leftHand, IKSolverFullBodyBiped.GetLeftClavicle(references));
			this.limbMappings[1].SetBones(references.rightUpperArm, references.rightForearm, references.rightHand, IKSolverFullBodyBiped.GetRightClavicle(references));
			this.limbMappings[2].SetBones(references.leftThigh, references.leftCalf, references.leftFoot, null);
			this.limbMappings[3].SetBones(references.rightThigh, references.rightCalf, references.rightFoot, null);
			if (Application.isPlaying)
			{
				base.Initiate(references.root);
			}
		}

		public static Transform DetectRootNodeBone(BipedReferences references)
		{
			if (!references.isFilled)
			{
				return null;
			}
			if (references.spine.Length < 1)
			{
				return null;
			}
			int num = references.spine.Length;
			if (num == 1)
			{
				return references.spine[0];
			}
			Vector3 b = Vector3.Lerp(references.leftThigh.position, references.rightThigh.position, 0.5f);
			Vector3 onNormal = Vector3.Lerp(references.leftUpperArm.position, references.rightUpperArm.position, 0.5f) - b;
			float magnitude = onNormal.magnitude;
			if (references.spine.Length < 2)
			{
				return references.spine[0];
			}
			int num2 = 0;
			for (int i = 1; i < num; i++)
			{
				Vector3 vector = Vector3.Project(references.spine[i].position - b, onNormal);
				if (Vector3.Dot(vector.normalized, onNormal.normalized) > 0f && vector.magnitude / magnitude < 0.5f)
				{
					num2 = i;
				}
			}
			return references.spine[num2];
		}

		public void SetLimbOrientations(BipedLimbOrientations o)
		{
			this.SetLimbOrientation(FullBodyBipedChain.LeftArm, o.leftArm);
			this.SetLimbOrientation(FullBodyBipedChain.RightArm, o.rightArm);
			this.SetLimbOrientation(FullBodyBipedChain.LeftLeg, o.leftLeg);
			this.SetLimbOrientation(FullBodyBipedChain.RightLeg, o.rightLeg);
		}

		public Vector3 pullBodyOffset { get; private set; }

		private void SetLimbOrientation(FullBodyBipedChain chain, BipedLimbOrientations.LimbOrientation limbOrientation)
		{
			if (chain == FullBodyBipedChain.LeftArm || chain == FullBodyBipedChain.RightArm)
			{
				this.GetBendConstraint(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis, -limbOrientation.lastBoneLeftAxis);
				this.GetLimbMapping(chain).SetLimbOrientation(-limbOrientation.upperBoneForwardAxis, -limbOrientation.lowerBoneForwardAxis);
				return;
			}
			this.GetBendConstraint(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis, limbOrientation.lastBoneLeftAxis);
			this.GetLimbMapping(chain).SetLimbOrientation(limbOrientation.upperBoneForwardAxis, limbOrientation.lowerBoneForwardAxis);
		}

		private static Transform GetLeftClavicle(BipedReferences references)
		{
			if (references.leftUpperArm == null)
			{
				return null;
			}
			if (!IKSolverFullBodyBiped.Contains(references.spine, references.leftUpperArm.parent))
			{
				return references.leftUpperArm.parent;
			}
			return null;
		}

		private static Transform GetRightClavicle(BipedReferences references)
		{
			if (references.rightUpperArm == null)
			{
				return null;
			}
			if (!IKSolverFullBodyBiped.Contains(references.spine, references.rightUpperArm.parent))
			{
				return references.rightUpperArm.parent;
			}
			return null;
		}

		private static bool Contains(Transform[] array, Transform transform)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == transform)
				{
					return true;
				}
			}
			return false;
		}

		protected override void ReadPose()
		{
			for (int i = 0; i < this.effectors.Length; i++)
			{
				this.effectors[i].SetToTarget();
			}
			this.PullBody();
			float pushElasticity = Mathf.Clamp(1f - this.spineStiffness, 0f, 1f);
			this.chain[0].childConstraints[0].pushElasticity = pushElasticity;
			this.chain[0].childConstraints[1].pushElasticity = pushElasticity;
			base.ReadPose();
		}

		private void PullBody()
		{
			if (this.iterations < 1)
			{
				return;
			}
			if (this.pullBodyVertical != 0f || this.pullBodyHorizontal != 0f)
			{
				Vector3 bodyOffset = this.GetBodyOffset();
				this.pullBodyOffset = V3Tools.ExtractVertical(bodyOffset, this.root.up, this.pullBodyVertical) + V3Tools.ExtractHorizontal(bodyOffset, this.root.up, this.pullBodyHorizontal);
				this.bodyEffector.positionOffset += this.pullBodyOffset;
			}
		}

		private Vector3 GetBodyOffset()
		{
			Vector3 a = Vector3.zero + this.GetHandBodyPull(this.leftHandEffector, this.leftArmChain, Vector3.zero) * Mathf.Clamp(this.leftHandEffector.positionWeight, 0f, 1f);
			return a + this.GetHandBodyPull(this.rightHandEffector, this.rightArmChain, a) * Mathf.Clamp(this.rightHandEffector.positionWeight, 0f, 1f);
		}

		private Vector3 GetHandBodyPull(IKEffector effector, FBIKChain arm, Vector3 offset)
		{
			Vector3 a = effector.position - (arm.nodes[0].transform.position + offset);
			float num = arm.nodes[0].length + arm.nodes[1].length;
			float magnitude = a.magnitude;
			if (magnitude < num)
			{
				return Vector3.zero;
			}
			float d = magnitude - num;
			return a / magnitude * d;
		}

		protected override void ApplyBendConstraints()
		{
			if (this.iterations > 0)
			{
				this.chain[1].bendConstraint.rotationOffset = this.leftHandEffector.planeRotationOffset;
				this.chain[2].bendConstraint.rotationOffset = this.rightHandEffector.planeRotationOffset;
				this.chain[3].bendConstraint.rotationOffset = this.leftFootEffector.planeRotationOffset;
				this.chain[4].bendConstraint.rotationOffset = this.rightFootEffector.planeRotationOffset;
			}
			else
			{
				this.offset = Vector3.Lerp(this.effectors[0].positionOffset, this.effectors[0].position - (this.effectors[0].bone.position + this.effectors[0].positionOffset), this.effectors[0].positionWeight);
				for (int i = 0; i < 5; i++)
				{
					this.effectors[i].GetNode(this).solverPosition += this.offset;
				}
			}
			base.ApplyBendConstraints();
		}

		protected override void WritePose()
		{
			if (this.iterations == 0)
			{
				this.spineMapping.spineBones[0].position += this.offset;
			}
			base.WritePose();
		}

		public Transform rootNode;

		[Range(0f, 1f)]
		public float spineStiffness = 0.5f;

		[Range(-1f, 1f)]
		public float pullBodyVertical = 0.5f;

		[Range(-1f, 1f)]
		public float pullBodyHorizontal;

		private Vector3 offset;
	}
}
