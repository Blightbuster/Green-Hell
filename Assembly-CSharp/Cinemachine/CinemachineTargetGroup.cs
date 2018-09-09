using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[ExecuteInEditMode]
	[DocumentationSorting(19f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("Cinemachine/CinemachineTargetGroup")]
	[SaveDuringPlay]
	public class CinemachineTargetGroup : MonoBehaviour
	{
		public Bounds BoundingBox
		{
			get
			{
				float num;
				Vector3 vector = this.CalculateAveragePosition(out num);
				bool flag = false;
				Bounds result = new Bounds(vector, new Vector3(this.m_lastRadius * 2f, this.m_lastRadius * 2f, this.m_lastRadius * 2f));
				if (num > 0.0001f)
				{
					for (int i = 0; i < this.m_Targets.Length; i++)
					{
						if (this.m_Targets[i].target != null)
						{
							float num2 = this.m_Targets[i].weight;
							if (num2 < num - 0.0001f)
							{
								num2 /= num;
							}
							else
							{
								num2 = 1f;
							}
							float num3 = this.m_Targets[i].radius * 2f * num2;
							Vector3 center = Vector3.Lerp(vector, this.m_Targets[i].target.position, num2);
							Bounds bounds = new Bounds(center, new Vector3(num3, num3, num3));
							if (!flag)
							{
								result = bounds;
							}
							else
							{
								result.Encapsulate(bounds);
							}
							flag = true;
						}
					}
				}
				Vector3 extents = result.extents;
				this.m_lastRadius = Mathf.Max(extents.x, Mathf.Max(extents.y, extents.z));
				return result;
			}
		}

		public bool IsEmpty
		{
			get
			{
				for (int i = 0; i < this.m_Targets.Length; i++)
				{
					if (this.m_Targets[i].target != null && this.m_Targets[i].weight > 0.0001f)
					{
						return false;
					}
				}
				return true;
			}
		}

		public Bounds GetViewSpaceBoundingBox(Matrix4x4 mView)
		{
			Matrix4x4 inverse = mView.inverse;
			float num;
			Vector3 vector = inverse.MultiplyPoint3x4(this.CalculateAveragePosition(out num));
			bool flag = false;
			Bounds result = new Bounds(vector, new Vector3(this.m_lastRadius * 2f, this.m_lastRadius * 2f, this.m_lastRadius * 2f));
			if (num > 0.0001f)
			{
				for (int i = 0; i < this.m_Targets.Length; i++)
				{
					if (this.m_Targets[i].target != null)
					{
						float num2 = this.m_Targets[i].weight;
						if (num2 < num - 0.0001f)
						{
							num2 /= num;
						}
						else
						{
							num2 = 1f;
						}
						float num3 = this.m_Targets[i].radius * 2f;
						Vector4 v = inverse.MultiplyPoint3x4(this.m_Targets[i].target.position);
						v = Vector3.Lerp(vector, v, num2);
						Bounds bounds = new Bounds(v, new Vector3(num3, num3, num3));
						if (!flag)
						{
							result = bounds;
						}
						else
						{
							result.Encapsulate(bounds);
						}
						flag = true;
					}
				}
			}
			Vector3 extents = result.extents;
			this.m_lastRadius = Mathf.Max(extents.x, Mathf.Max(extents.y, extents.z));
			return result;
		}

		private Vector3 CalculateAveragePosition(out float averageWeight)
		{
			Vector3 vector = Vector3.zero;
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < this.m_Targets.Length; i++)
			{
				if (this.m_Targets[i].target != null && this.m_Targets[i].weight > 0.0001f)
				{
					num2++;
					num += this.m_Targets[i].weight;
					vector += this.m_Targets[i].target.position * this.m_Targets[i].weight;
				}
			}
			if (num > 0.0001f)
			{
				vector /= num;
			}
			if (num2 == 0)
			{
				averageWeight = 0f;
				return base.transform.position;
			}
			averageWeight = num / (float)num2;
			return vector;
		}

		private Quaternion CalculateAverageOrientation()
		{
			Quaternion identity = Quaternion.identity;
			for (int i = 0; i < this.m_Targets.Length; i++)
			{
				if (this.m_Targets[i].target != null)
				{
					float weight = this.m_Targets[i].weight;
					Quaternion rotation = this.m_Targets[i].target.rotation;
					identity = new Quaternion(identity.x + rotation.x * weight, identity.y + rotation.y * weight, identity.z + rotation.z * weight, identity.w + rotation.w * weight);
				}
			}
			return identity.Normalized();
		}

		private void OnValidate()
		{
			for (int i = 0; i < this.m_Targets.Length; i++)
			{
				if (this.m_Targets[i].weight < 0f)
				{
					this.m_Targets[i].weight = 0f;
				}
				if (this.m_Targets[i].radius < 0f)
				{
					this.m_Targets[i].radius = 0f;
				}
			}
		}

		private void FixedUpdate()
		{
			if (this.m_UpdateMethod == CinemachineTargetGroup.UpdateMethod.FixedUpdate)
			{
				this.UpdateTransform();
			}
		}

		private void Update()
		{
			if (!Application.isPlaying || this.m_UpdateMethod == CinemachineTargetGroup.UpdateMethod.Update)
			{
				this.UpdateTransform();
			}
		}

		private void LateUpdate()
		{
			if (this.m_UpdateMethod == CinemachineTargetGroup.UpdateMethod.LateUpdate)
			{
				this.UpdateTransform();
			}
		}

		private void UpdateTransform()
		{
			if (this.IsEmpty)
			{
				return;
			}
			CinemachineTargetGroup.PositionMode positionMode = this.m_PositionMode;
			if (positionMode != CinemachineTargetGroup.PositionMode.GroupCenter)
			{
				if (positionMode == CinemachineTargetGroup.PositionMode.GroupAverage)
				{
					float num;
					base.transform.position = this.CalculateAveragePosition(out num);
				}
			}
			else
			{
				base.transform.position = this.BoundingBox.center;
			}
			CinemachineTargetGroup.RotationMode rotationMode = this.m_RotationMode;
			if (rotationMode != CinemachineTargetGroup.RotationMode.Manual)
			{
				if (rotationMode == CinemachineTargetGroup.RotationMode.GroupAverage)
				{
					base.transform.rotation = this.CalculateAverageOrientation();
				}
			}
		}

		[Tooltip("How the group's position is calculated.  Select GroupCenter for the center of the bounding box, and GroupAverage for a weighted average of the positions of the members.")]
		public CinemachineTargetGroup.PositionMode m_PositionMode;

		[Tooltip("How the group's rotation is calculated.  Select Manual to use the value in the group's transform, and GroupAverage for a weighted average of the orientations of the members.")]
		public CinemachineTargetGroup.RotationMode m_RotationMode;

		[Tooltip("When to update the group's transform based on the position of the group members")]
		public CinemachineTargetGroup.UpdateMethod m_UpdateMethod = CinemachineTargetGroup.UpdateMethod.LateUpdate;

		[NoSaveDuringPlay]
		[Tooltip("The target objects, together with their weights and radii, that will contribute to the group's average position, orientation, and size.")]
		public CinemachineTargetGroup.Target[] m_Targets = new CinemachineTargetGroup.Target[0];

		private float m_lastRadius;

		[DocumentationSorting(19.1f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct Target
		{
			[Tooltip("The target objects.  This object's position and orientation will contribute to the group's average position and orientation, in accordance with its weight")]
			public Transform target;

			[Tooltip("How much weight to give the target when averaging.  Cannot be negative")]
			public float weight;

			[Tooltip("The radius of the target, used for calculating the bounding box.  Cannot be negative")]
			public float radius;
		}

		[DocumentationSorting(19.2f, DocumentationSortingAttribute.Level.UserRef)]
		public enum PositionMode
		{
			GroupCenter,
			GroupAverage
		}

		[DocumentationSorting(19.3f, DocumentationSortingAttribute.Level.UserRef)]
		public enum RotationMode
		{
			Manual,
			GroupAverage
		}

		public enum UpdateMethod
		{
			Update,
			FixedUpdate,
			LateUpdate
		}
	}
}
