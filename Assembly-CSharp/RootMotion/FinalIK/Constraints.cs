using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class Constraints
	{
		public bool IsValid()
		{
			return this.transform != null;
		}

		public void Initiate(Transform transform)
		{
			this.transform = transform;
			this.position = transform.position;
			this.rotation = transform.eulerAngles;
		}

		public void Update()
		{
			if (!this.IsValid())
			{
				return;
			}
			if (this.target != null)
			{
				this.position = this.target.position;
			}
			this.transform.position += this.positionOffset;
			if (this.positionWeight > 0f)
			{
				this.transform.position = Vector3.Lerp(this.transform.position, this.position, this.positionWeight);
			}
			if (this.target != null)
			{
				this.rotation = this.target.eulerAngles;
			}
			this.transform.rotation = Quaternion.Euler(this.rotationOffset) * this.transform.rotation;
			if (this.rotationWeight > 0f)
			{
				this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(this.rotation), this.rotationWeight);
			}
		}

		public Transform transform;

		public Transform target;

		public Vector3 positionOffset;

		public Vector3 position;

		[Range(0f, 1f)]
		public float positionWeight;

		public Vector3 rotationOffset;

		public Vector3 rotation;

		[Range(0f, 1f)]
		public float rotationWeight;
	}
}
