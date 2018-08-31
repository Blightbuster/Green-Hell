using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[DocumentationSorting(21f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	public class CinemachineDollyCart : MonoBehaviour
	{
		private void FixedUpdate()
		{
			if (this.m_UpdateMethod == CinemachineDollyCart.UpdateMethod.FixedUpdate)
			{
				this.SetCartPosition(this.m_Position += this.m_Speed * Time.deltaTime);
			}
		}

		private void Update()
		{
			if (!Application.isPlaying)
			{
				this.SetCartPosition(this.m_Position);
			}
			else if (this.m_UpdateMethod == CinemachineDollyCart.UpdateMethod.Update)
			{
				this.SetCartPosition(this.m_Position += this.m_Speed * Time.deltaTime);
			}
		}

		private void SetCartPosition(float distanceAlongPath)
		{
			if (this.m_Path != null)
			{
				this.m_Position = this.m_Path.NormalizeUnit(distanceAlongPath, this.m_PositionUnits);
				base.transform.position = this.m_Path.EvaluatePositionAtUnit(this.m_Position, this.m_PositionUnits);
				base.transform.rotation = this.m_Path.EvaluateOrientationAtUnit(this.m_Position, this.m_PositionUnits);
			}
		}

		[Tooltip("The path to follow")]
		public CinemachinePathBase m_Path;

		[Tooltip("When to move the cart, if Velocity is non-zero")]
		public CinemachineDollyCart.UpdateMethod m_UpdateMethod;

		[Tooltip("How to interpret the Path Position.  If set to Path Units, values are as follows: 0 represents the first waypoint on the path, 1 is the second, and so on.  Values in-between are points on the path in between the waypoints.  If set to Distance, then Path Position represents distance along the path.")]
		public CinemachinePathBase.PositionUnits m_PositionUnits = CinemachinePathBase.PositionUnits.Distance;

		[Tooltip("Move the cart with this speed along the path.  The value is interpreted according to the Position Units setting.")]
		[FormerlySerializedAs("m_Velocity")]
		public float m_Speed;

		[FormerlySerializedAs("m_CurrentDistance")]
		[Tooltip("The position along the path at which the cart will be placed.  This can be animated directly or, if the velocity is non-zero, will be updated automatically.  The value is interpreted according to the Position Units setting.")]
		public float m_Position;

		public enum UpdateMethod
		{
			Update,
			FixedUpdate
		}
	}
}
