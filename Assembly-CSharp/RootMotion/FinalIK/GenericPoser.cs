using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class GenericPoser : Poser
	{
		[ContextMenu("Auto-Mapping")]
		public override void AutoMapping()
		{
			if (this.poseRoot == null)
			{
				this.maps = new GenericPoser.Map[0];
				return;
			}
			this.maps = new GenericPoser.Map[0];
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
			Transform[] componentsInChildren2 = this.poseRoot.GetComponentsInChildren<Transform>();
			for (int i = 1; i < componentsInChildren.Length; i++)
			{
				Transform targetNamed = this.GetTargetNamed(componentsInChildren[i].name, componentsInChildren2);
				if (targetNamed != null)
				{
					Array.Resize<GenericPoser.Map>(ref this.maps, this.maps.Length + 1);
					this.maps[this.maps.Length - 1] = new GenericPoser.Map(componentsInChildren[i], targetNamed);
				}
			}
			this.StoreDefaultState();
		}

		protected override void InitiatePoser()
		{
			this.StoreDefaultState();
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
			if (this.poseRoot == null)
			{
				return;
			}
			float localRotationWeight = this.localRotationWeight * this.weight;
			float localPositionWeight = this.localPositionWeight * this.weight;
			for (int i = 0; i < this.maps.Length; i++)
			{
				this.maps[i].Update(localRotationWeight, localPositionWeight);
			}
		}

		protected override void FixPoserTransforms()
		{
			for (int i = 0; i < this.maps.Length; i++)
			{
				this.maps[i].FixTransform();
			}
		}

		private void StoreDefaultState()
		{
			for (int i = 0; i < this.maps.Length; i++)
			{
				this.maps[i].StoreDefaultState();
			}
		}

		private Transform GetTargetNamed(string tName, Transform[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == tName)
				{
					return array[i];
				}
			}
			return null;
		}

		public GenericPoser.Map[] maps;

		[Serializable]
		public class Map
		{
			public Map(Transform bone, Transform target)
			{
				this.bone = bone;
				this.target = target;
				this.StoreDefaultState();
			}

			public void StoreDefaultState()
			{
				this.defaultLocalPosition = this.bone.localPosition;
				this.defaultLocalRotation = this.bone.localRotation;
			}

			public void FixTransform()
			{
				this.bone.localPosition = this.defaultLocalPosition;
				this.bone.localRotation = this.defaultLocalRotation;
			}

			public void Update(float localRotationWeight, float localPositionWeight)
			{
				this.bone.localRotation = Quaternion.Lerp(this.bone.localRotation, this.target.localRotation, localRotationWeight);
				this.bone.localPosition = Vector3.Lerp(this.bone.localPosition, this.target.localPosition, localPositionWeight);
			}

			public Transform bone;

			public Transform target;

			private Vector3 defaultLocalPosition;

			private Quaternion defaultLocalRotation;
		}
	}
}
