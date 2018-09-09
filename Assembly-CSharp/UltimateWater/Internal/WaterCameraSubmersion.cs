using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateWater.Internal
{
	[Serializable]
	public class WaterCameraSubmersion
	{
		public SubmersionState State
		{
			get
			{
				Water containingWater = this._Camera._ContainingWater;
				if (containingWater == null)
				{
					return SubmersionState.None;
				}
				if (!containingWater.Volume.Boundless)
				{
					return SubmersionState.Partial;
				}
				return this.Evaluate();
			}
		}

		public void OnEnable(WaterCamera camera)
		{
			this._Camera = camera;
		}

		public void OnDisable()
		{
		}

		public void OnDrawGizmos()
		{
			Camera cameraComponent = this._Camera.CameraComponent;
			float num = WaterCameraSubmersion.CalculateNearPlaneHeight(cameraComponent);
			WaterCameraSubmersion.CreatePlanePoints(cameraComponent, this._Subdivisions, this._Points);
			Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
			for (int i = 0; i < this._Points.Count; i++)
			{
				Gizmos.DrawSphere(this._Points[i], num * this._Radius);
			}
		}

		public void Create()
		{
			if (this._Samples != null)
			{
				this.Destroy();
			}
			Water containingWater = this._Camera._ContainingWater;
			if (containingWater != null)
			{
				Camera cameraComponent = this._Camera.CameraComponent;
				this._Samples = new List<WaterSample>();
				WaterCameraSubmersion.CreatePlanePoints(cameraComponent, this._Subdivisions, this._Points);
				for (int i = 0; i < this._Points.Count; i++)
				{
					WaterSample waterSample = new WaterSample(containingWater, WaterSample.DisplacementMode.Height, 0.4f);
					waterSample.Start(this._Points[i]);
					this._Samples.Add(waterSample);
				}
			}
		}

		public void Destroy()
		{
			if (this._Samples != null)
			{
				foreach (WaterSample waterSample in this._Samples)
				{
					waterSample.Stop();
				}
				this._Samples.Clear();
				this._Samples = null;
			}
			this._Points.Clear();
		}

		public void OnValidate()
		{
			if (this._Subdivisions < 0)
			{
				this._Subdivisions = 0;
			}
			if (this._Radius < 0f)
			{
				this._Radius = 0f;
			}
		}

		private SubmersionState Evaluate()
		{
			int num = 0;
			float num2 = WaterCameraSubmersion.CalculateNearPlaneHeight(this._Camera.CameraComponent);
			float num3 = num2 * this._Radius;
			WaterCameraSubmersion.CreatePlanePoints(this._Camera.CameraComponent, this._Subdivisions, this._Points);
			for (int i = 0; i < this._Samples.Count; i++)
			{
				Vector3 origin = this._Points[i];
				Vector3 andReset = this._Samples[i].GetAndReset(origin, WaterSample.ComputationsMode.Normal);
				if (origin.y + num3 <= andReset.y)
				{
					num++;
				}
				if (origin.y + num3 >= andReset.y && origin.y - num3 <= andReset.y)
				{
					return SubmersionState.Partial;
				}
			}
			if (num == this._Samples.Count)
			{
				return SubmersionState.Full;
			}
			if (num == 0)
			{
				return SubmersionState.None;
			}
			return SubmersionState.Partial;
		}

		private static void CreatePlanePoints(Camera camera, int subdivisions, List<Vector3> result)
		{
			result.Clear();
			float num = WaterCameraSubmersion.CalculateNearPlaneHeight(camera);
			float num2 = num * camera.aspect;
			Vector3 nearPlaneCenter = WaterCameraSubmersion.GetNearPlaneCenter(camera);
			if (subdivisions == 0)
			{
				result.Add(nearPlaneCenter);
				return;
			}
			subdivisions--;
			Vector3 b = 0.5f * num * camera.transform.up;
			Vector3 b2 = 0.5f * num2 * camera.transform.right;
			Vector3 vector = nearPlaneCenter - b - b2;
			Vector3 vector2 = nearPlaneCenter + b - b2;
			Vector3 vector3 = nearPlaneCenter + b + b2;
			Vector3 vector4 = nearPlaneCenter - b + b2;
			result.Add(vector);
			result.Add(vector2);
			result.Add(vector3);
			result.Add(vector4);
			for (int i = 0; i < subdivisions; i++)
			{
				float t = ((float)i + 1f) / ((float)subdivisions + 1f);
				result.Add(Vector3.Lerp(vector, vector2, t));
				result.Add(Vector3.Lerp(vector2, vector3, t));
				result.Add(Vector3.Lerp(vector3, vector4, t));
				result.Add(Vector3.Lerp(vector4, vector, t));
			}
		}

		private static Vector3 GetNearPlaneCenter(Camera camera)
		{
			return camera.transform.position + camera.transform.forward * camera.nearClipPlane;
		}

		private static float CalculateNearPlaneHeight(Camera camera)
		{
			return 2f * camera.nearClipPlane * Mathf.Tan(0.5f * camera.fieldOfView * 0.0174532924f);
		}

		[Range(0f, 2f)]
		[SerializeField]
		private float _Radius = 1f;

		[SerializeField]
		private int _Subdivisions;

		private WaterCamera _Camera;

		private List<WaterSample> _Samples;

		private readonly List<Vector3> _Points = new List<Vector3>();
	}
}
