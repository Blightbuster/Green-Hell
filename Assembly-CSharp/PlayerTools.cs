using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class PlayerTools
{
	private static void Initialize()
	{
		Player player = Player.Get();
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("PL"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hips"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:UpLeg.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Leg.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Foot.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:LeftToeBase"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:LeftToe_End"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:LeftToe_End_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound02"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound02_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Dressing02"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:UpLeg.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Leg.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Foot.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:RightToeBase"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:RightToe_End"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:RightToe_End_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound03"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound03_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Dressing03"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Spine"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Spine1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Spine2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:LeftShoulder"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Arm.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:ForeArm.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Dressing00"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("LHolder"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("LHolder_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("LFingerHolder"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("LFingerHolder_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound00"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound00_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Neck"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Head"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.L"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.L_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.R_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:RightShoulder"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Arm.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:ForeArm.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.R"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb1"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb2"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb3"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb4"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb4_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("RFingerHolder"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("RFingerHolder_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("RHolder"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("RHolder_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound01"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Wound01_end"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("Dressing01"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("OffsetHelper"));
		PlayerTools.s_AllTransform.Add(player.gameObject.transform.FindDeepChild("OffsetHelper_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("PL"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hips"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Spine"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Spine1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Spine2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:LeftShoulder"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Arm.L"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:ForeArm.L"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("Dressing00"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.L"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("LHolder"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("LHolder_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LIndex4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LMiddle4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LPinky4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LRing4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("LFingerHolder"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("LFingerHolder_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.LThumb4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("Wound00"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("Wound00_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Neck"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Head"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.L"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.L_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.R"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Eye.R_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:RightShoulder"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Arm.R"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:ForeArm.R"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.R"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RIndex4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RMiddle4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RPinky4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RRing4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb1"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb2"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb3"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb4"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("mixamorig:Hand.RThumb4_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("RFingerHolder"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("RFingerHolder_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("RHolder"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("RHolder_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("Wound01"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("Wound01_end"));
		PlayerTools.s_SpineTransform.Add(player.gameObject.transform.FindDeepChild("Dressing01"));
		for (int i = 0; i < PlayerTools.s_AllTransform.Count; i++)
		{
			if (!PlayerTools.s_SpineTransform.Contains(PlayerTools.s_AllTransform[i]))
			{
				PlayerTools.s_BottomTransform.Add(PlayerTools.s_AllTransform[i]);
			}
		}
		PlayerTools.s_Initialized = true;
	}

	public static void StoreBones(PlayerAvatarMask mask)
	{
		if (!PlayerTools.s_Initialized)
		{
			PlayerTools.Initialize();
		}
		List<Transform> list = null;
		if (mask != PlayerAvatarMask.Bottom)
		{
			if (mask != PlayerAvatarMask.Spine)
			{
				if (mask == PlayerAvatarMask.All)
				{
					list = PlayerTools.s_AllTransform;
				}
			}
			else
			{
				list = PlayerTools.s_SpineTransform;
			}
		}
		else
		{
			list = PlayerTools.s_BottomTransform;
		}
		CJPair<Vector3, Quaternion> cjpair = default(CJPair<Vector3, Quaternion>);
		for (int i = 0; i < list.Count; i++)
		{
			if (!PlayerTools.s_StoredTransform.TryGetValue(list[i], out cjpair))
			{
				PlayerTools.s_StoredTransform[list[i]] = default(CJPair<Vector3, Quaternion>);
			}
			CJPair<Vector3, Quaternion> value = new CJPair<Vector3, Quaternion>(list[i].position, list[i].rotation);
			PlayerTools.s_StoredTransform[list[i]] = value;
		}
	}

	public static void RestoreBones(PlayerAvatarMask mask)
	{
		if (!PlayerTools.s_Initialized)
		{
			PlayerTools.Initialize();
			return;
		}
		List<Transform> list = null;
		if (mask != PlayerAvatarMask.Bottom)
		{
			if (mask != PlayerAvatarMask.Spine)
			{
				if (mask == PlayerAvatarMask.All)
				{
					list = PlayerTools.s_AllTransform;
				}
			}
			else
			{
				list = PlayerTools.s_SpineTransform;
			}
		}
		else
		{
			list = PlayerTools.s_BottomTransform;
		}
		CJPair<Vector3, Quaternion> cjpair = default(CJPair<Vector3, Quaternion>);
		for (int i = 0; i < list.Count; i++)
		{
			if (PlayerTools.s_StoredTransform.TryGetValue(list[i], out cjpair))
			{
				list[i].position = cjpair.v1;
				list[i].rotation = cjpair.v2;
			}
		}
	}

	public static bool s_Initialized = false;

	public static List<Transform> s_AllTransform = new List<Transform>();

	public static List<Transform> s_SpineTransform = new List<Transform>();

	public static List<Transform> s_BottomTransform = new List<Transform>();

	private static Dictionary<Transform, CJPair<Vector3, Quaternion>> s_StoredTransform = new Dictionary<Transform, CJPair<Vector3, Quaternion>>();
}
