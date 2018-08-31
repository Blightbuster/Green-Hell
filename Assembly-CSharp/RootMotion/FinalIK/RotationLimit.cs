using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class RotationLimit : MonoBehaviour
	{
		public void SetDefaultLocalRotation()
		{
			this.defaultLocalRotation = base.transform.localRotation;
			this.defaultLocalRotationSet = true;
		}

		public Quaternion GetLimitedLocalRotation(Quaternion localRotation, out bool changed)
		{
			if (!this.initiated)
			{
				this.Awake();
			}
			Quaternion quaternion = Quaternion.Inverse(this.defaultLocalRotation) * localRotation;
			Quaternion quaternion2 = this.LimitRotation(quaternion);
			changed = (quaternion2 != quaternion);
			if (!changed)
			{
				return localRotation;
			}
			return this.defaultLocalRotation * quaternion2;
		}

		public bool Apply()
		{
			bool result = false;
			base.transform.localRotation = this.GetLimitedLocalRotation(base.transform.localRotation, out result);
			return result;
		}

		public void Disable()
		{
			if (this.initiated)
			{
				base.enabled = false;
				return;
			}
			this.Awake();
			base.enabled = false;
		}

		public Vector3 secondaryAxis
		{
			get
			{
				return new Vector3(this.axis.y, this.axis.z, this.axis.x);
			}
		}

		public Vector3 crossAxis
		{
			get
			{
				return Vector3.Cross(this.axis, this.secondaryAxis);
			}
		}

		protected abstract Quaternion LimitRotation(Quaternion rotation);

		private void Awake()
		{
			if (!this.defaultLocalRotationSet)
			{
				this.SetDefaultLocalRotation();
			}
			if (this.axis == Vector3.zero)
			{
				Debug.LogError("Axis is Vector3.zero.");
			}
			this.initiated = true;
		}

		private void LateUpdate()
		{
			this.Apply();
		}

		public void LogWarning(string message)
		{
			Warning.Log(message, base.transform, false);
		}

		protected static Quaternion Limit1DOF(Quaternion rotation, Vector3 axis)
		{
			return Quaternion.FromToRotation(rotation * axis, axis) * rotation;
		}

		protected static Quaternion LimitTwist(Quaternion rotation, Vector3 axis, Vector3 orthoAxis, float twistLimit)
		{
			twistLimit = Mathf.Clamp(twistLimit, 0f, 180f);
			if (twistLimit >= 180f)
			{
				return rotation;
			}
			Vector3 vector = rotation * axis;
			Vector3 toDirection = orthoAxis;
			Vector3.OrthoNormalize(ref vector, ref toDirection);
			Vector3 fromDirection = rotation * orthoAxis;
			Vector3.OrthoNormalize(ref vector, ref fromDirection);
			Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection) * rotation;
			if (twistLimit <= 0f)
			{
				return quaternion;
			}
			return Quaternion.RotateTowards(quaternion, rotation, twistLimit);
		}

		protected static float GetOrthogonalAngle(Vector3 v1, Vector3 v2, Vector3 normal)
		{
			Vector3.OrthoNormalize(ref normal, ref v1);
			Vector3.OrthoNormalize(ref normal, ref v2);
			return Vector3.Angle(v1, v2);
		}

		public Vector3 axis = Vector3.forward;

		[HideInInspector]
		public Quaternion defaultLocalRotation;

		private bool initiated;

		private bool applicationQuit;

		private bool defaultLocalRotationSet;
	}
}
