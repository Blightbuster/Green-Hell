using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[SaveDuringPlay]
	[DocumentationSorting(18f, DocumentationSortingAttribute.Level.UserRef)]
	[AddComponentMenu("Cinemachine/CinemachinePath")]
	public class CinemachinePath : CinemachinePathBase
	{
		public override float MinPos
		{
			get
			{
				return 0f;
			}
		}

		public override float MaxPos
		{
			get
			{
				int num = this.m_Waypoints.Length - 1;
				if (num < 1)
				{
					return 0f;
				}
				return (float)((!this.m_Looped) ? num : (num + 1));
			}
		}

		public override bool Looped
		{
			get
			{
				return this.m_Looped;
			}
		}

		public override int DistanceCacheSampleStepsPerSegment
		{
			get
			{
				return this.m_Resolution;
			}
		}

		private float GetBoundingIndices(float pos, out int indexA, out int indexB)
		{
			pos = this.NormalizePos(pos);
			int num = Mathf.RoundToInt(pos);
			if (Mathf.Abs(pos - (float)num) < 0.0001f)
			{
				indexA = (indexB = ((num != this.m_Waypoints.Length) ? num : 0));
			}
			else
			{
				indexA = Mathf.FloorToInt(pos);
				if (indexA >= this.m_Waypoints.Length)
				{
					pos -= this.MaxPos;
					indexA = 0;
				}
				indexB = Mathf.CeilToInt(pos);
				if (indexB >= this.m_Waypoints.Length)
				{
					indexB = 0;
				}
			}
			return pos;
		}

		public override Vector3 EvaluatePosition(float pos)
		{
			Vector3 position = default(Vector3);
			if (this.m_Waypoints.Length == 0)
			{
				position = base.transform.position;
			}
			else
			{
				int num;
				int num2;
				pos = this.GetBoundingIndices(pos, out num, out num2);
				if (num == num2)
				{
					position = this.m_Waypoints[num].position;
				}
				else
				{
					CinemachinePath.Waypoint waypoint = this.m_Waypoints[num];
					CinemachinePath.Waypoint waypoint2 = this.m_Waypoints[num2];
					position = SplineHelpers.Bezier3(pos - (float)num, this.m_Waypoints[num].position, waypoint.position + waypoint.tangent, waypoint2.position - waypoint2.tangent, waypoint2.position);
				}
			}
			return base.transform.TransformPoint(position);
		}

		public override Vector3 EvaluateTangent(float pos)
		{
			Vector3 direction = default(Vector3);
			if (this.m_Waypoints.Length == 0)
			{
				direction = base.transform.rotation * Vector3.forward;
			}
			else
			{
				int num;
				int num2;
				pos = this.GetBoundingIndices(pos, out num, out num2);
				if (num == num2)
				{
					direction = this.m_Waypoints[num].tangent;
				}
				else
				{
					CinemachinePath.Waypoint waypoint = this.m_Waypoints[num];
					CinemachinePath.Waypoint waypoint2 = this.m_Waypoints[num2];
					direction = SplineHelpers.BezierTangent3(pos - (float)num, this.m_Waypoints[num].position, waypoint.position + waypoint.tangent, waypoint2.position - waypoint2.tangent, waypoint2.position);
				}
			}
			return base.transform.TransformDirection(direction);
		}

		public override Quaternion EvaluateOrientation(float pos)
		{
			Quaternion result = base.transform.rotation;
			if (this.m_Waypoints.Length > 0)
			{
				int num;
				int num2;
				pos = this.GetBoundingIndices(pos, out num, out num2);
				float angle;
				if (num == num2)
				{
					angle = this.m_Waypoints[num].roll;
				}
				else
				{
					float num3 = this.m_Waypoints[num].roll;
					float num4 = this.m_Waypoints[num2].roll;
					if (num2 == 0)
					{
						num3 %= 360f;
						num4 %= 360f;
					}
					angle = Mathf.Lerp(num3, num4, pos - (float)num);
				}
				Vector3 vector = this.EvaluateTangent(pos);
				if (!vector.AlmostZero())
				{
					Vector3 upwards = base.transform.rotation * Vector3.up;
					Quaternion lhs = Quaternion.LookRotation(vector, upwards);
					result = lhs * Quaternion.AngleAxis(angle, Vector3.forward);
				}
			}
			return result;
		}

		private void OnValidate()
		{
			this.InvalidateDistanceCache();
		}

		[Tooltip("If checked, then the path ends are joined to form a continuous loop.")]
		public bool m_Looped;

		[Tooltip("The waypoints that define the path.  They will be interpolated using a bezier curve.")]
		public CinemachinePath.Waypoint[] m_Waypoints = new CinemachinePath.Waypoint[0];

		[DocumentationSorting(18.2f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct Waypoint
		{
			[Tooltip("Position in path-local space")]
			public Vector3 position;

			[Tooltip("Offset from the position, which defines the tangent of the curve at the waypoint.  The length of the tangent encodes the strength of the bezier handle.  The same handle is used symmetrically on both sides of the waypoint, to ensure smoothness.")]
			public Vector3 tangent;

			[Tooltip("Defines the roll of the path at this waypoint.  The other orientation axes are inferred from the tangent and world up.")]
			public float roll;
		}
	}
}
