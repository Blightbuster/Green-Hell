using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class Reflection
	{
		public static Camera CreateCamera(Camera camera)
		{
			GameObject gameObject = new GameObject("Reflection Camera");
			gameObject.transform.SetParent(camera.transform);
			Camera camera2 = gameObject.AddComponent<Camera>();
			camera2.renderingPath = RenderingPath.Forward;
			camera2.stereoTargetEye = StereoTargetEyeMask.None;
			camera2.enabled = false;
			camera2.depthTextureMode = DepthTextureMode.None;
			return camera2;
		}

		public static Camera GetReflectionCamera(Camera camera)
		{
			WaterCamera component = camera.gameObject.GetComponent<WaterCamera>();
			if (component == null)
			{
				Debug.LogError("WaterCamera not found");
				return null;
			}
			if (component.ReflectionCamera == null)
			{
				component.ReflectionCamera = Reflection.CreateCamera(camera);
			}
			return component.ReflectionCamera;
		}

		public static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = 1f - 2f * plane.x * plane.x;
			reflectionMat.m01 = -2f * plane.x * plane.y;
			reflectionMat.m02 = -2f * plane.x * plane.z;
			reflectionMat.m03 = -2f * plane.w * plane.x;
			reflectionMat.m10 = -2f * plane.y * plane.x;
			reflectionMat.m11 = 1f - 2f * plane.y * plane.y;
			reflectionMat.m12 = -2f * plane.y * plane.z;
			reflectionMat.m13 = -2f * plane.w * plane.y;
			reflectionMat.m20 = -2f * plane.z * plane.x;
			reflectionMat.m21 = -2f * plane.z * plane.y;
			reflectionMat.m22 = 1f - 2f * plane.z * plane.z;
			reflectionMat.m23 = -2f * plane.w * plane.z;
			reflectionMat.m30 = 0f;
			reflectionMat.m31 = 0f;
			reflectionMat.m32 = 0f;
			reflectionMat.m33 = 1f;
			return reflectionMat;
		}

		public static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
		{
			Vector4 b = projection.inverse * new Vector4(Mathf.Sign(clipPlane.x), Mathf.Sign(clipPlane.y), 1f, 1f);
			Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
			projection[2] = vector.x - projection[3];
			projection[6] = vector.y - projection[7];
			projection[10] = vector.z - projection[11];
			projection[14] = vector.w - projection[15];
			return projection;
		}

		public static Vector4 CameraSpacePlane(Camera camera, Vector3 position, Vector3 normal, float clipPlaneOffset, float sideSign)
		{
			Vector3 point = position + normal * clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
			Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(rhs.x, rhs.y, rhs.z, -Vector3.Dot(lhs, rhs));
		}
	}
}
