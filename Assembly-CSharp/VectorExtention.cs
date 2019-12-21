﻿using System;
using UnityEngine;

public static class VectorExtention
{
	public static bool IsZero(this Vector3 vector)
	{
		return vector == Vector3.zero;
	}

	public static float Distance(this Vector3 vector, Vector3 to)
	{
		return Vector3.Distance(vector, to);
	}

	public static float Distance2D(this Vector3 vector, Vector3 to)
	{
		vector = vector.To2D();
		to = to.To2D();
		return Vector3.Distance(vector, to);
	}

	public static Vector3 GetNormalized2D(this Vector3 vector)
	{
		vector.y = 0f;
		return vector.normalized;
	}

	public static float AngleSigned(this Vector3 v1, Vector3 v2, Vector3 n)
	{
		return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
	}

	public static Vector3 To2D(this Vector3 vector)
	{
		vector.y = 0f;
		return vector;
	}

	public static float Dot2D(this Vector3 vector, Vector3 other)
	{
		vector.y = 0f;
		other.y = 0f;
		return Vector3.Dot(vector, other);
	}

	public static Vector3 To3D(this Vector2 vector)
	{
		return new Vector3(vector.x, 0f, vector.y);
	}

	public static Vector3 Round(this Vector3 vector, int digits)
	{
		return new Vector3((float)Math.Round((double)vector.x, digits), (float)Math.Round((double)vector.y, digits), (float)Math.Round((double)vector.z, digits));
	}
}
