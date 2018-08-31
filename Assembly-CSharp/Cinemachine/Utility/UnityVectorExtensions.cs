using System;
using UnityEngine;

namespace Cinemachine.Utility
{
	public static class UnityVectorExtensions
	{
		public static float ClosestPointOnSegment(this Vector3 p, Vector3 s0, Vector3 s1)
		{
			Vector3 vector = s1 - s0;
			float num = Vector3.SqrMagnitude(vector);
			if (num < 0.0001f)
			{
				return 0f;
			}
			return Mathf.Clamp01(Vector3.Dot(p - s0, vector) / num);
		}

		public static float ClosestPointOnSegment(this Vector2 p, Vector2 s0, Vector2 s1)
		{
			Vector2 vector = s1 - s0;
			float num = Vector2.SqrMagnitude(vector);
			if (num < 0.0001f)
			{
				return 0f;
			}
			return Mathf.Clamp01(Vector2.Dot(p - s0, vector) / num);
		}

		public static Vector3 ProjectOntoPlane(this Vector3 vector, Vector3 planeNormal)
		{
			return vector - Vector3.Dot(vector, planeNormal) * planeNormal;
		}

		public static bool AlmostZero(this Vector3 v)
		{
			return v.sqrMagnitude < 9.999999E-09f;
		}

		public static float SignedAngle(Vector3 from, Vector3 to, Vector3 refNormal)
		{
			from.Normalize();
			to.Normalize();
			float num = Vector3.Dot(Vector3.Cross(from, to), refNormal);
			if (Mathf.Abs(num) < -0.0001f)
			{
				return (float)((Vector3.Dot(from, to) >= 0f) ? 0 : 180);
			}
			float num2 = Vector3.Angle(from, to);
			if (num < 0f)
			{
				return -num2;
			}
			return num2;
		}

		public static Vector3 SlerpWithReferenceUp(Vector3 vA, Vector3 vB, float t, Vector3 up)
		{
			float magnitude = vA.magnitude;
			float magnitude2 = vB.magnitude;
			if (magnitude < 0.0001f || magnitude2 < 0.0001f)
			{
				return Vector3.Lerp(vA, vB, t);
			}
			Vector3 forward = vA / magnitude;
			Vector3 forward2 = vB / magnitude2;
			Quaternion qA = Quaternion.LookRotation(forward, up);
			Quaternion qB = Quaternion.LookRotation(forward2, up);
			Quaternion rotation = UnityQuaternionExtensions.SlerpWithReferenceUp(qA, qB, t, up);
			Vector3 a = rotation * Vector3.forward;
			return a * Mathf.Lerp(magnitude, magnitude2, t);
		}

		public const float Epsilon = 0.0001f;
	}
}
