using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIs
{
	public class BodyRotationModule : AIModule
	{
		public override void OnLateUpdate()
		{
			base.OnLateUpdate();
			this.UpdateBones();
		}

		private void UpdateBones()
		{
			Vector3 position = this.m_ReferenceBone.position;
			Vector3 position2 = Player.Get().GetHeadTransform().transform.position;
			Vector3 v = Vector3.RotateTowards(base.transform.forward, (position2 - position).normalized, 0.0174532924f * this.m_MaxAngle, 0f);
			float num = Mathf.Min(0.02f, Time.deltaTime);
			float num2 = 0f;
			if (this.m_RotateToPlayer)
			{
				float num3 = v.AngleSigned(base.transform.forward, Vector3.up);
				num2 = num3 / (float)this.m_Bones.Count;
			}
			this.m_BodyAngle += (num2 - this.m_BodyAngle) * num * 20f;
			for (int i = 0; i < this.m_Bones.Count; i++)
			{
				this.m_Bones[i].transform.Rotate(Vector3.up, this.m_BodyAngle);
			}
			num2 = 0f;
			if (this.m_LookAtPlayer)
			{
				float num4 = (v.y - base.transform.forward.y) * 90f;
				num4 = Mathf.Clamp(num4, -this.m_MaxHeadAngle, this.m_MaxHeadAngle);
				num2 = num4 / (float)this.m_HeadBones.Count;
			}
			this.m_HeadAngle += (num2 - this.m_HeadAngle) * num * 10f;
			for (int j = 0; j < this.m_HeadBones.Count; j++)
			{
				this.m_HeadBones[j].transform.Rotate(Vector3.forward, this.m_HeadAngle);
			}
		}

		public Transform m_ReferenceBone;

		public List<Transform> m_Bones;

		public List<Transform> m_HeadBones;

		[Range(0f, 360f)]
		public float m_MaxAngle;

		[Range(0f, 90f)]
		public float m_MaxHeadAngle;

		private float m_BodyAngle;

		private float m_HeadAngle;

		[HideInInspector]
		public bool m_LookAtPlayer;

		[HideInInspector]
		public bool m_RotateToPlayer;
	}
}
