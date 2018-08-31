using System;
using UnityEngine;

namespace RootMotion
{
	public static class BipedNaming
	{
		public static Transform[] GetBonesOfType(BipedNaming.BoneType boneType, Transform[] bones)
		{
			Transform[] array = new Transform[0];
			foreach (Transform transform in bones)
			{
				if (transform != null && BipedNaming.GetBoneType(transform.name) == boneType)
				{
					Array.Resize<Transform>(ref array, array.Length + 1);
					array[array.Length - 1] = transform;
				}
			}
			return array;
		}

		public static Transform[] GetBonesOfSide(BipedNaming.BoneSide boneSide, Transform[] bones)
		{
			Transform[] array = new Transform[0];
			foreach (Transform transform in bones)
			{
				if (transform != null && BipedNaming.GetBoneSide(transform.name) == boneSide)
				{
					Array.Resize<Transform>(ref array, array.Length + 1);
					array[array.Length - 1] = transform;
				}
			}
			return array;
		}

		public static Transform[] GetBonesOfTypeAndSide(BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide, Transform[] bones)
		{
			Transform[] bonesOfType = BipedNaming.GetBonesOfType(boneType, bones);
			return BipedNaming.GetBonesOfSide(boneSide, bonesOfType);
		}

		public static Transform GetFirstBoneOfTypeAndSide(BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide, Transform[] bones)
		{
			Transform[] bonesOfTypeAndSide = BipedNaming.GetBonesOfTypeAndSide(boneType, boneSide, bones);
			if (bonesOfTypeAndSide.Length == 0)
			{
				return null;
			}
			return bonesOfTypeAndSide[0];
		}

		public static Transform GetNamingMatch(Transform[] transforms, params string[][] namings)
		{
			foreach (Transform transform in transforms)
			{
				bool flag = true;
				foreach (string namingConvention in namings)
				{
					if (!BipedNaming.matchesNaming(transform.name, namingConvention))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return transform;
				}
			}
			return null;
		}

		public static BipedNaming.BoneType GetBoneType(string boneName)
		{
			if (BipedNaming.isSpine(boneName))
			{
				return BipedNaming.BoneType.Spine;
			}
			if (BipedNaming.isHead(boneName))
			{
				return BipedNaming.BoneType.Head;
			}
			if (BipedNaming.isArm(boneName))
			{
				return BipedNaming.BoneType.Arm;
			}
			if (BipedNaming.isLeg(boneName))
			{
				return BipedNaming.BoneType.Leg;
			}
			if (BipedNaming.isTail(boneName))
			{
				return BipedNaming.BoneType.Tail;
			}
			if (BipedNaming.isEye(boneName))
			{
				return BipedNaming.BoneType.Eye;
			}
			return BipedNaming.BoneType.Unassigned;
		}

		public static BipedNaming.BoneSide GetBoneSide(string boneName)
		{
			if (BipedNaming.isLeft(boneName))
			{
				return BipedNaming.BoneSide.Left;
			}
			if (BipedNaming.isRight(boneName))
			{
				return BipedNaming.BoneSide.Right;
			}
			return BipedNaming.BoneSide.Center;
		}

		public static Transform GetBone(Transform[] transforms, BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide = BipedNaming.BoneSide.Center, params string[][] namings)
		{
			Transform[] bonesOfTypeAndSide = BipedNaming.GetBonesOfTypeAndSide(boneType, boneSide, transforms);
			return BipedNaming.GetNamingMatch(bonesOfTypeAndSide, namings);
		}

		private static bool isLeft(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeLeft) || BipedNaming.lastLetter(boneName) == "L" || BipedNaming.firstLetter(boneName) == "L";
		}

		private static bool isRight(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeRight) || BipedNaming.lastLetter(boneName) == "R" || BipedNaming.firstLetter(boneName) == "R";
		}

		private static bool isSpine(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeSpine) && !BipedNaming.excludesNaming(boneName, BipedNaming.typeExcludeSpine);
		}

		private static bool isHead(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeHead) && !BipedNaming.excludesNaming(boneName, BipedNaming.typeExcludeHead);
		}

		private static bool isArm(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeArm) && !BipedNaming.excludesNaming(boneName, BipedNaming.typeExcludeArm);
		}

		private static bool isLeg(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeLeg) && !BipedNaming.excludesNaming(boneName, BipedNaming.typeExcludeLeg);
		}

		private static bool isTail(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeTail) && !BipedNaming.excludesNaming(boneName, BipedNaming.typeExcludeTail);
		}

		private static bool isEye(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeEye) && !BipedNaming.excludesNaming(boneName, BipedNaming.typeExcludeEye);
		}

		private static bool isTypeExclude(string boneName)
		{
			return BipedNaming.matchesNaming(boneName, BipedNaming.typeExclude);
		}

		private static bool matchesNaming(string boneName, string[] namingConvention)
		{
			if (BipedNaming.excludesNaming(boneName, BipedNaming.typeExclude))
			{
				return false;
			}
			foreach (string value in namingConvention)
			{
				if (boneName.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static bool excludesNaming(string boneName, string[] namingConvention)
		{
			foreach (string value in namingConvention)
			{
				if (boneName.Contains(value))
				{
					return true;
				}
			}
			return false;
		}

		private static bool matchesLastLetter(string boneName, string[] namingConvention)
		{
			foreach (string letter in namingConvention)
			{
				if (BipedNaming.LastLetterIs(boneName, letter))
				{
					return true;
				}
			}
			return false;
		}

		private static bool LastLetterIs(string boneName, string letter)
		{
			string a = boneName.Substring(boneName.Length - 1, 1);
			return a == letter;
		}

		private static string firstLetter(string boneName)
		{
			if (boneName.Length > 0)
			{
				return boneName.Substring(0, 1);
			}
			return string.Empty;
		}

		private static string lastLetter(string boneName)
		{
			if (boneName.Length > 0)
			{
				return boneName.Substring(boneName.Length - 1, 1);
			}
			return string.Empty;
		}

		public static string[] typeLeft = new string[]
		{
			" L ",
			"_L_",
			"-L-",
			" l ",
			"_l_",
			"-l-",
			"Left",
			"left",
			"CATRigL"
		};

		public static string[] typeRight = new string[]
		{
			" R ",
			"_R_",
			"-R-",
			" r ",
			"_r_",
			"-r-",
			"Right",
			"right",
			"CATRigR"
		};

		public static string[] typeSpine = new string[]
		{
			"Spine",
			"spine",
			"Pelvis",
			"pelvis",
			"Root",
			"root",
			"Torso",
			"torso",
			"Body",
			"body",
			"Hips",
			"hips",
			"Neck",
			"neck",
			"Chest",
			"chest"
		};

		public static string[] typeHead = new string[]
		{
			"Head",
			"head"
		};

		public static string[] typeArm = new string[]
		{
			"Arm",
			"arm",
			"Hand",
			"hand",
			"Wrist",
			"Wrist",
			"Elbow",
			"elbow",
			"Palm",
			"palm"
		};

		public static string[] typeLeg = new string[]
		{
			"Leg",
			"leg",
			"Thigh",
			"thigh",
			"Calf",
			"calf",
			"Femur",
			"femur",
			"Knee",
			"knee",
			"Foot",
			"foot",
			"Ankle",
			"ankle",
			"Hip",
			"hip"
		};

		public static string[] typeTail = new string[]
		{
			"Tail",
			"tail"
		};

		public static string[] typeEye = new string[]
		{
			"Eye",
			"eye"
		};

		public static string[] typeExclude = new string[]
		{
			"Nub",
			"Dummy",
			"dummy",
			"Tip",
			"IK",
			"Mesh"
		};

		public static string[] typeExcludeSpine = new string[]
		{
			"Head",
			"head"
		};

		public static string[] typeExcludeHead = new string[]
		{
			"Top",
			"End"
		};

		public static string[] typeExcludeArm = new string[]
		{
			"Collar",
			"collar",
			"Clavicle",
			"clavicle",
			"Finger",
			"finger",
			"Index",
			"index",
			"Mid",
			"mid",
			"Pinky",
			"pinky",
			"Ring",
			"Thumb",
			"thumb",
			"Adjust",
			"adjust",
			"Twist",
			"twist"
		};

		public static string[] typeExcludeLeg = new string[]
		{
			"Toe",
			"toe",
			"Platform",
			"Adjust",
			"adjust",
			"Twist",
			"twist"
		};

		public static string[] typeExcludeTail = new string[0];

		public static string[] typeExcludeEye = new string[]
		{
			"Lid",
			"lid",
			"Brow",
			"brow",
			"Lash",
			"lash"
		};

		public static string[] pelvis = new string[]
		{
			"Pelvis",
			"pelvis",
			"Hip",
			"hip"
		};

		public static string[] hand = new string[]
		{
			"Hand",
			"hand",
			"Wrist",
			"wrist",
			"Palm",
			"palm"
		};

		public static string[] foot = new string[]
		{
			"Foot",
			"foot",
			"Ankle",
			"ankle"
		};

		[Serializable]
		public enum BoneType
		{
			Unassigned,
			Spine,
			Head,
			Arm,
			Leg,
			Tail,
			Eye
		}

		[Serializable]
		public enum BoneSide
		{
			Center,
			Left,
			Right
		}
	}
}
