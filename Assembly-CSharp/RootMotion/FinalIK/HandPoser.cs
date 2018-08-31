using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class HandPoser : Poser
	{
		public override void AutoMapping()
		{
			if (this.poseRoot == null)
			{
				this.poseChildren = new Transform[0];
			}
			else
			{
				this.poseChildren = this.poseRoot.GetComponentsInChildren<Transform>();
			}
			this._poseRoot = this.poseRoot;
		}

		protected override void InitiatePoser()
		{
			this.children = base.GetComponentsInChildren<Transform>();
			this.StoreDefaultState();
		}

		protected override void FixPoserTransforms()
		{
			for (int i = 0; i < this.children.Length; i++)
			{
				this.children[i].localPosition = this.defaultLocalPositions[i];
				this.children[i].localRotation = this.defaultLocalRotations[i];
			}
		}

		protected override void UpdatePoser()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			if (this.localPositionWeight <= 0f && this.localRotationWeight <= 0f)
			{
				return;
			}
			if (this._poseRoot != this.poseRoot)
			{
				this.AutoMapping();
			}
			if (this.poseRoot == null)
			{
				return;
			}
			if (this.children.Length != this.poseChildren.Length)
			{
				Warning.Log("Number of children does not match with the pose", base.transform, false);
				return;
			}
			float t = this.localRotationWeight * this.weight;
			float t2 = this.localPositionWeight * this.weight;
			for (int i = 0; i < this.children.Length; i++)
			{
				if (this.children[i] != base.transform)
				{
					this.children[i].localRotation = Quaternion.Lerp(this.children[i].localRotation, this.poseChildren[i].localRotation, t);
					this.children[i].localPosition = Vector3.Lerp(this.children[i].localPosition, this.poseChildren[i].localPosition, t2);
				}
			}
		}

		private void StoreDefaultState()
		{
			this.defaultLocalPositions = new Vector3[this.children.Length];
			this.defaultLocalRotations = new Quaternion[this.children.Length];
			for (int i = 0; i < this.children.Length; i++)
			{
				this.defaultLocalPositions[i] = this.children[i].localPosition;
				this.defaultLocalRotations[i] = this.children[i].localRotation;
			}
		}

		private Transform _poseRoot;

		private Transform[] children;

		private Transform[] poseChildren;

		private Vector3[] defaultLocalPositions;

		private Quaternion[] defaultLocalRotations;
	}
}
