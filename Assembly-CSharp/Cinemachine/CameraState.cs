using System;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	public struct CameraState
	{
		public LensSettings Lens { get; set; }

		public Vector3 ReferenceUp { get; set; }

		public Vector3 ReferenceLookAt { get; set; }

		public bool HasLookAt
		{
			get
			{
				return this.ReferenceLookAt == this.ReferenceLookAt;
			}
		}

		public Vector3 RawPosition { get; set; }

		public Quaternion RawOrientation { get; set; }

		internal Vector3 PositionDampingBypass { get; set; }

		public float ShotQuality { get; set; }

		public Vector3 PositionCorrection { get; set; }

		public Quaternion OrientationCorrection { get; set; }

		public Vector3 CorrectedPosition
		{
			get
			{
				return this.RawPosition + this.PositionCorrection;
			}
		}

		public Quaternion CorrectedOrientation
		{
			get
			{
				return this.RawOrientation * this.OrientationCorrection;
			}
		}

		public Vector3 FinalPosition
		{
			get
			{
				return this.RawPosition + this.PositionCorrection;
			}
		}

		public Quaternion FinalOrientation
		{
			get
			{
				if (Mathf.Abs(this.Lens.Dutch) > 0.0001f)
				{
					return this.CorrectedOrientation * Quaternion.AngleAxis(this.Lens.Dutch, Vector3.forward);
				}
				return this.CorrectedOrientation;
			}
		}

		public static CameraState Default
		{
			get
			{
				return new CameraState
				{
					Lens = LensSettings.Default,
					ReferenceUp = Vector3.up,
					ReferenceLookAt = CameraState.kNoPoint,
					RawPosition = Vector3.zero,
					RawOrientation = Quaternion.identity,
					ShotQuality = 1f,
					PositionCorrection = Vector3.zero,
					OrientationCorrection = Quaternion.identity,
					PositionDampingBypass = Vector3.zero
				};
			}
		}

		public int NumCustomBlendables { get; private set; }

		public CameraState.CustomBlendable GetCustomBlendable(int index)
		{
			switch (index)
			{
			case 0:
				return this.mCustom0;
			case 1:
				return this.mCustom1;
			case 2:
				return this.mCustom2;
			case 3:
				return this.mCustom3;
			default:
				index -= 4;
				if (this.m_CustomOverflow != null && index < this.m_CustomOverflow.Count)
				{
					return this.m_CustomOverflow[index];
				}
				return new CameraState.CustomBlendable(null, 0f);
			}
		}

		private int FindCustomBlendable(UnityEngine.Object custom)
		{
			if (this.mCustom0.m_Custom == custom)
			{
				return 0;
			}
			if (this.mCustom1.m_Custom == custom)
			{
				return 1;
			}
			if (this.mCustom2.m_Custom == custom)
			{
				return 2;
			}
			if (this.mCustom3.m_Custom == custom)
			{
				return 3;
			}
			if (this.m_CustomOverflow != null)
			{
				for (int i = 0; i < this.m_CustomOverflow.Count; i++)
				{
					if (this.m_CustomOverflow[i].m_Custom == custom)
					{
						return i + 4;
					}
				}
			}
			return -1;
		}

		public void AddCustomBlendable(CameraState.CustomBlendable b)
		{
			int num = this.FindCustomBlendable(b.m_Custom);
			if (num >= 0)
			{
				b.m_Weight += this.GetCustomBlendable(num).m_Weight;
			}
			else
			{
				num = this.NumCustomBlendables;
				this.NumCustomBlendables = num + 1;
			}
			switch (num)
			{
			case 0:
				this.mCustom0 = b;
				return;
			case 1:
				this.mCustom1 = b;
				return;
			case 2:
				this.mCustom2 = b;
				return;
			case 3:
				this.mCustom3 = b;
				return;
			default:
				if (this.m_CustomOverflow == null)
				{
					this.m_CustomOverflow = new List<CameraState.CustomBlendable>();
				}
				this.m_CustomOverflow.Add(b);
				return;
			}
		}

		public static CameraState Lerp(CameraState stateA, CameraState stateB, float t)
		{
			t = Mathf.Clamp01(t);
			float t2 = t;
			CameraState result = default(CameraState);
			result.Lens = LensSettings.Lerp(stateA.Lens, stateB.Lens, t);
			result.ReferenceUp = Vector3.Slerp(stateA.ReferenceUp, stateB.ReferenceUp, t);
			result.RawPosition = Vector3.Lerp(stateA.RawPosition, stateB.RawPosition, t);
			result.ShotQuality = Mathf.Lerp(stateA.ShotQuality, stateB.ShotQuality, t);
			result.PositionCorrection = Vector3.Lerp(stateA.PositionCorrection, stateB.PositionCorrection, t);
			result.OrientationCorrection = Quaternion.Slerp(stateA.OrientationCorrection, stateB.OrientationCorrection, t);
			Vector3 vector = Vector3.zero;
			if (!stateA.HasLookAt || !stateB.HasLookAt)
			{
				result.ReferenceLookAt = CameraState.kNoPoint;
			}
			else
			{
				float fieldOfView = stateA.Lens.FieldOfView;
				float fieldOfView2 = stateB.Lens.FieldOfView;
				if (!result.Lens.Orthographic && !Mathf.Approximately(fieldOfView, fieldOfView2))
				{
					LensSettings lens = result.Lens;
					lens.FieldOfView = result.InterpolateFOV(fieldOfView, fieldOfView2, Mathf.Max((stateA.ReferenceLookAt - stateA.CorrectedPosition).magnitude, stateA.Lens.NearClipPlane), Mathf.Max((stateB.ReferenceLookAt - stateB.CorrectedPosition).magnitude, stateB.Lens.NearClipPlane), t);
					result.Lens = lens;
					t2 = Mathf.Abs((lens.FieldOfView - fieldOfView) / (fieldOfView2 - fieldOfView));
				}
				result.ReferenceLookAt = Vector3.Lerp(stateA.ReferenceLookAt, stateB.ReferenceLookAt, t2);
				if (Quaternion.Angle(stateA.RawOrientation, stateB.RawOrientation) > 0.0001f)
				{
					vector = result.ReferenceLookAt - result.CorrectedPosition;
				}
			}
			if (vector.AlmostZero())
			{
				result.RawOrientation = UnityQuaternionExtensions.SlerpWithReferenceUp(stateA.RawOrientation, stateB.RawOrientation, t, result.ReferenceUp);
			}
			else
			{
				vector = vector.normalized;
				if ((vector - result.ReferenceUp).AlmostZero() || (vector + result.ReferenceUp).AlmostZero())
				{
					result.RawOrientation = UnityQuaternionExtensions.SlerpWithReferenceUp(stateA.RawOrientation, stateB.RawOrientation, t, result.ReferenceUp);
				}
				else
				{
					result.RawOrientation = Quaternion.LookRotation(vector, result.ReferenceUp);
					Vector2 a = -stateA.RawOrientation.GetCameraRotationToTarget(stateA.ReferenceLookAt - stateA.CorrectedPosition, stateA.ReferenceUp);
					Vector2 b = -stateB.RawOrientation.GetCameraRotationToTarget(stateB.ReferenceLookAt - stateB.CorrectedPosition, stateB.ReferenceUp);
					result.RawOrientation = result.RawOrientation.ApplyCameraRotation(Vector2.Lerp(a, b, t2), result.ReferenceUp);
				}
			}
			for (int i = 0; i < stateA.NumCustomBlendables; i++)
			{
				CameraState.CustomBlendable customBlendable = stateA.GetCustomBlendable(i);
				customBlendable.m_Weight *= 1f - t;
				if (customBlendable.m_Weight > 0.0001f)
				{
					result.AddCustomBlendable(customBlendable);
				}
			}
			for (int j = 0; j < stateB.NumCustomBlendables; j++)
			{
				CameraState.CustomBlendable customBlendable2 = stateB.GetCustomBlendable(j);
				customBlendable2.m_Weight *= t;
				if (customBlendable2.m_Weight > 0.0001f)
				{
					result.AddCustomBlendable(customBlendable2);
				}
			}
			return result;
		}

		private float InterpolateFOV(float fovA, float fovB, float dA, float dB, float t)
		{
			float a = dA * 2f * Mathf.Tan(fovA * 0.0174532924f / 2f);
			float b = dB * 2f * Mathf.Tan(fovB * 0.0174532924f / 2f);
			float num = Mathf.Lerp(a, b, t);
			float value = 179f;
			float num2 = Mathf.Lerp(dA, dB, t);
			if (num2 > 0.0001f)
			{
				value = 2f * Mathf.Atan(num / (2f * num2)) * 57.29578f;
			}
			return Mathf.Clamp(value, Mathf.Min(fovA, fovB), Mathf.Max(fovA, fovB));
		}

		public static Vector3 kNoPoint = new Vector3(float.NaN, float.NaN, float.NaN);

		private CameraState.CustomBlendable mCustom0;

		private CameraState.CustomBlendable mCustom1;

		private CameraState.CustomBlendable mCustom2;

		private CameraState.CustomBlendable mCustom3;

		private List<CameraState.CustomBlendable> m_CustomOverflow;

		public struct CustomBlendable
		{
			public CustomBlendable(UnityEngine.Object custom, float weight)
			{
				this.m_Custom = custom;
				this.m_Weight = weight;
			}

			public UnityEngine.Object m_Custom;

			public float m_Weight;
		}
	}
}
