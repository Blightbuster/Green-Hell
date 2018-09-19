using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class TwistRelaxer : MonoBehaviour
	{
		public void Relax()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			Vector3 a = this.parent.rotation * this.axisRelativeToParentDefault;
			Vector3 b = this.child.rotation * this.axisRelativeToChildDefault;
			Vector3 point = Vector3.Slerp(a, b, this.parentChildCrossfade);
			Quaternion rotation = Quaternion.LookRotation(base.transform.rotation * this.axis, base.transform.rotation * this.twistAxis);
			point = Quaternion.Inverse(rotation) * point;
			float num = Mathf.Atan2(point.x, point.z) * 57.29578f;
			Quaternion rotation2 = this.child.rotation;
			base.transform.rotation = Quaternion.AngleAxis(num * this.weight, base.transform.rotation * this.twistAxis) * base.transform.rotation;
			this.child.rotation = rotation2;
		}

		private void Start()
		{
			this.parent = base.transform.parent;
			if (base.transform.childCount == 0)
			{
				Debug.LogError("The Transform of a TwistRelaxer has no children. Can not use TwistRelaxer on that bone.");
				return;
			}
			this.child = base.transform.GetChild(0);
			this.twistAxis = base.transform.InverseTransformDirection(this.child.position - base.transform.position);
			this.axis = new Vector3(this.twistAxis.y, this.twistAxis.z, this.twistAxis.x);
			Vector3 point = base.transform.rotation * this.axis;
			this.axisRelativeToParentDefault = Quaternion.Inverse(this.parent.rotation) * point;
			this.axisRelativeToChildDefault = Quaternion.Inverse(this.child.rotation) * point;
		}

		private void LateUpdate()
		{
			this.Relax();
		}

		[Range(0f, 1f)]
		[Tooltip("The weight of relaxing the twist of this Transform")]
		public float weight = 1f;

		[Range(0f, 1f)]
		[Tooltip("If 0.5, this Transform will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
		public float parentChildCrossfade = 0.5f;

		private Vector3 twistAxis = Vector3.right;

		private Vector3 axis = Vector3.forward;

		private Vector3 axisRelativeToParentDefault;

		private Vector3 axisRelativeToChildDefault;

		private Transform parent;

		private Transform child;
	}
}
