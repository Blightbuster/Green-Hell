using System;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater
{
	[ExecuteInEditMode]
	public class LuxWater_PlanarReflection : MonoBehaviour
	{
		private void OnEnable()
		{
			base.gameObject.layer = LayerMask.NameToLayer("Water");
			if (base.GetComponent<Renderer>() != null)
			{
				this.m_SharedMaterial = base.GetComponent<Renderer>().sharedMaterial;
			}
		}

		private void OnDisable()
		{
			if (this.m_ReflectionCamera != null)
			{
				UnityEngine.Object.DestroyImmediate(this.m_ReflectionCamera.targetTexture);
				UnityEngine.Object.DestroyImmediate(this.m_ReflectionCamera);
			}
			if (this.m_HelperCameras != null)
			{
				this.m_HelperCameras.Clear();
			}
		}

		private Camera CreateReflectionCameraFor(Camera cam)
		{
			string name = base.gameObject.name + "Reflection" + cam.name;
			GameObject gameObject = GameObject.Find(name);
			if (!gameObject)
			{
				gameObject = new GameObject(name, new Type[]
				{
					typeof(Camera)
				});
				gameObject.hideFlags = HideFlags.HideAndDontSave;
			}
			if (!gameObject.GetComponent(typeof(Camera)))
			{
				gameObject.AddComponent(typeof(Camera));
			}
			Camera component = gameObject.GetComponent<Camera>();
			component.backgroundColor = this.clearColor;
			component.clearFlags = (this.reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Color);
			this.SetStandardCameraParameter(component, this.reflectionMask);
			if (!component.targetTexture)
			{
				component.targetTexture = this.CreateTextureFor(cam);
			}
			return component;
		}

		private void SetStandardCameraParameter(Camera cam, LayerMask mask)
		{
			cam.cullingMask = (mask & ~(1 << LayerMask.NameToLayer("Water")));
			cam.backgroundColor = Color.black;
			cam.enabled = false;
		}

		private RenderTexture CreateTextureFor(Camera cam)
		{
			int width = Mathf.FloorToInt((float)(cam.pixelWidth / (int)this.Resolution));
			int height = Mathf.FloorToInt((float)(cam.pixelHeight / (int)this.Resolution));
			return new RenderTexture(width, height, 24)
			{
				hideFlags = HideFlags.DontSave
			};
		}

		public void RenderHelpCameras(Camera currentCam)
		{
			if (this.m_HelperCameras == null)
			{
				this.m_HelperCameras = new Dictionary<Camera, bool>();
			}
			if (!this.m_HelperCameras.ContainsKey(currentCam))
			{
				this.m_HelperCameras.Add(currentCam, false);
			}
			if (this.m_HelperCameras[currentCam])
			{
				return;
			}
			if (currentCam.name.Contains("Reflection Probes"))
			{
				return;
			}
			if (!this.m_ReflectionCamera)
			{
				this.m_ReflectionCamera = this.CreateReflectionCameraFor(currentCam);
			}
			this.RenderReflectionFor(currentCam, this.m_ReflectionCamera);
			this.m_HelperCameras[currentCam] = true;
		}

		public void LateUpdate()
		{
			if (this.m_HelperCameras != null)
			{
				this.m_HelperCameras.Clear();
			}
		}

		public void WaterTileBeingRendered(Transform tr, Camera currentCam)
		{
			this.RenderHelpCameras(currentCam);
			if (this.m_ReflectionCamera && this.m_SharedMaterial)
			{
				this.m_SharedMaterial.SetTexture(this.reflectionSampler, this.m_ReflectionCamera.targetTexture);
			}
		}

		public void OnWillRenderObject()
		{
			this.WaterTileBeingRendered(base.transform, Camera.current);
		}

		private void RenderReflectionFor(Camera cam, Camera reflectCamera)
		{
			if (!reflectCamera)
			{
				return;
			}
			if (this.m_SharedMaterial && !this.m_SharedMaterial.HasProperty(this.reflectionSampler))
			{
				return;
			}
			reflectCamera.cullingMask = (this.reflectionMask & ~(1 << LayerMask.NameToLayer("Water")));
			this.SaneCameraSettings(reflectCamera);
			reflectCamera.backgroundColor = this.clearColor;
			reflectCamera.clearFlags = (this.reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Color);
			GL.invertCulling = true;
			Transform transform = base.transform;
			Vector3 eulerAngles = cam.transform.eulerAngles;
			reflectCamera.transform.eulerAngles = new Vector3(-eulerAngles.x, eulerAngles.y, eulerAngles.z);
			reflectCamera.transform.position = cam.transform.position;
			Vector3 position = transform.transform.position;
			position.y = transform.position.y + this.WaterSurfaceOffset;
			Vector3 up = transform.transform.up;
			float w = -Vector3.Dot(up, position) - this.clipPlaneOffset;
			Vector4 plane = new Vector4(up.x, up.y, up.z, w);
			Matrix4x4 matrix4x = Matrix4x4.zero;
			matrix4x = LuxWater_PlanarReflection.CalculateReflectionMatrix(matrix4x, plane);
			this.m_Oldpos = cam.transform.position;
			Vector3 position2 = matrix4x.MultiplyPoint(this.m_Oldpos);
			reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * matrix4x;
			Vector4 clipPlane = this.CameraSpacePlane(reflectCamera, position, up, 1f);
			Matrix4x4 matrix4x2 = cam.projectionMatrix;
			matrix4x2 = LuxWater_PlanarReflection.CalculateObliqueMatrix(matrix4x2, clipPlane);
			reflectCamera.projectionMatrix = matrix4x2;
			reflectCamera.transform.position = position2;
			Vector3 eulerAngles2 = cam.transform.eulerAngles;
			reflectCamera.transform.eulerAngles = new Vector3(-eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
			int pixelLightCount = QualitySettings.pixelLightCount;
			if (this.disablePixelLights)
			{
				QualitySettings.pixelLightCount = 0;
			}
			float num = QualitySettings.shadowDistance;
			int shadowCascades = QualitySettings.shadowCascades;
			if (!this.renderShadows)
			{
				QualitySettings.shadowDistance = 0f;
			}
			else if (this.shadowDistance > 0f)
			{
				QualitySettings.shadowDistance = this.shadowDistance;
			}
			QualitySettings.shadowCascades = (int)this.ShadowCascades;
			reflectCamera.Render();
			GL.invertCulling = false;
			if (this.disablePixelLights)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			if (!this.renderShadows || this.shadowDistance > 0f)
			{
				QualitySettings.shadowDistance = num;
			}
			QualitySettings.shadowCascades = shadowCascades;
			if (this.isMaster)
			{
				for (int i = 0; i < this.WaterMaterials.Length; i++)
				{
					this.WaterMaterials[i].SetTexture(this.reflectionSampler, reflectCamera.targetTexture);
				}
			}
		}

		private void SaneCameraSettings(Camera helperCam)
		{
			helperCam.depthTextureMode = DepthTextureMode.None;
			helperCam.backgroundColor = Color.black;
			helperCam.clearFlags = CameraClearFlags.Color;
			helperCam.renderingPath = RenderingPath.Forward;
		}

		private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
		{
			Vector4 b = projection.inverse * new Vector4(LuxWater_PlanarReflection.Sgn(clipPlane.x), LuxWater_PlanarReflection.Sgn(clipPlane.y), 1f, 1f);
			Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
			projection[2] = vector.x - projection[3];
			projection[6] = vector.y - projection[7];
			projection[10] = vector.z - projection[11];
			projection[14] = vector.w - projection[15];
			return projection;
		}

		private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
			reflectionMat.m01 = -2f * plane[0] * plane[1];
			reflectionMat.m02 = -2f * plane[0] * plane[2];
			reflectionMat.m03 = -2f * plane[3] * plane[0];
			reflectionMat.m10 = -2f * plane[1] * plane[0];
			reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
			reflectionMat.m12 = -2f * plane[1] * plane[2];
			reflectionMat.m13 = -2f * plane[3] * plane[1];
			reflectionMat.m20 = -2f * plane[2] * plane[0];
			reflectionMat.m21 = -2f * plane[2] * plane[1];
			reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
			reflectionMat.m23 = -2f * plane[3] * plane[2];
			reflectionMat.m30 = 0f;
			reflectionMat.m31 = 0f;
			reflectionMat.m32 = 0f;
			reflectionMat.m33 = 1f;
			return reflectionMat;
		}

		private static float Sgn(float a)
		{
			if (a > 0f)
			{
				return 1f;
			}
			if (a < 0f)
			{
				return -1f;
			}
			return 0f;
		}

		private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 point = pos + normal * this.clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
			Vector3 vector = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(vector.x, vector.y, vector.z, -Vector3.Dot(lhs, vector));
		}

		public bool UpdateSceneView = true;

		[Space(5f)]
		public bool isMaster;

		public Material[] WaterMaterials;

		[Space(5f)]
		public LayerMask reflectionMask = -1;

		public LuxWater_PlanarReflection.ReflectionResolution Resolution = LuxWater_PlanarReflection.ReflectionResolution.Half;

		public Color clearColor = Color.black;

		public bool reflectSkybox = true;

		[Space(5f)]
		public bool disablePixelLights;

		[Space(5f)]
		public bool renderShadows = true;

		public float shadowDistance;

		public LuxWater_PlanarReflection.NumberOfShadowCascades ShadowCascades = LuxWater_PlanarReflection.NumberOfShadowCascades.One;

		[Space(5f)]
		public float WaterSurfaceOffset;

		public float clipPlaneOffset = 0.07f;

		private string reflectionSampler = "_LuxWater_ReflectionTex";

		private Vector3 m_Oldpos;

		private Camera m_ReflectionCamera;

		private Material m_SharedMaterial;

		private Dictionary<Camera, bool> m_HelperCameras;

		private RenderTexture m_reflectionMap;

		public enum ReflectionResolution
		{
			Full = 1,
			Half,
			Quarter = 4,
			Eighth = 8
		}

		public enum NumberOfShadowCascades
		{
			One = 1,
			Two,
			Four = 4
		}
	}
}
