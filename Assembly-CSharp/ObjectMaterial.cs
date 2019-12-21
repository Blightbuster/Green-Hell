using System;
using UnityEngine;

public class ObjectMaterial : MonoBehaviour
{
	public EObjectMaterial GetObjectMaterial()
	{
		return this.m_ObjectMaterial;
	}

	public static bool IsMaterialHard(EObjectMaterial mat)
	{
		return mat == EObjectMaterial.Wood || mat == EObjectMaterial.Stone || mat - EObjectMaterial.TurtleShell <= 2;
	}

	public static float GetDamageSelfMul(EObjectMaterial mat)
	{
		switch (mat)
		{
		case EObjectMaterial.Wood:
		case EObjectMaterial.WoodTree:
			return 0.7f;
		case EObjectMaterial.Bush:
			return 0.2f;
		case EObjectMaterial.Stone:
			return 1f;
		case EObjectMaterial.DryLeaves:
		case EObjectMaterial.Grass:
		case EObjectMaterial.Mud:
		case EObjectMaterial.Sand:
			return 0.1f;
		case EObjectMaterial.Flesh:
			return 0.4f;
		}
		return 1f;
	}

	public EObjectMaterial m_ObjectMaterial;
}
