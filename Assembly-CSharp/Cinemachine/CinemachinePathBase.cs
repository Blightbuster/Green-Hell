using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	public abstract class CinemachinePathBase : MonoBehaviour
	{
		public abstract float MinPos { get; }

		public abstract float MaxPos { get; }

		public abstract bool Looped { get; }

		public virtual float NormalizePos(float pos)
		{
			if (this.MaxPos == 0f)
			{
				return 0f;
			}
			if (this.Looped)
			{
				pos %= this.MaxPos;
				if (pos < 0f)
				{
					pos += this.MaxPos;
				}
				return pos;
			}
			return Mathf.Clamp(pos, 0f, this.MaxPos);
		}

		public abstract Vector3 EvaluatePosition(float pos);

		public abstract Vector3 EvaluateTangent(float pos);

		public abstract Quaternion EvaluateOrientation(float pos);

		public virtual float FindClosestPoint(Vector3 p, int startSegment, int searchRadius, int stepsPerSegment)
		{
			float num = this.MinPos;
			float num2 = this.MaxPos;
			if (searchRadius >= 0)
			{
				int num3 = Mathf.FloorToInt(Mathf.Min((float)searchRadius, (num2 - num) / 2f));
				num = (float)(startSegment - num3);
				num2 = (float)(startSegment + num3 + 1);
				if (!this.Looped)
				{
					num = Mathf.Max(num, this.MinPos);
					num2 = Mathf.Max(num2, this.MaxPos);
				}
			}
			stepsPerSegment = Mathf.RoundToInt(Mathf.Clamp((float)stepsPerSegment, 1f, 100f));
			float num4 = 1f / (float)stepsPerSegment;
			float num5 = (float)startSegment;
			float num6 = float.MaxValue;
			int num7 = (stepsPerSegment == 1) ? 1 : 3;
			for (int i = 0; i < num7; i++)
			{
				Vector3 vector = this.EvaluatePosition(num);
				for (float num8 = num + num4; num8 <= num2; num8 += num4)
				{
					Vector3 vector2 = this.EvaluatePosition(num8);
					float num9 = p.ClosestPointOnSegment(vector, vector2);
					float num10 = Vector3.SqrMagnitude(p - Vector3.Lerp(vector, vector2, num9));
					if (num10 < num6)
					{
						num6 = num10;
						num5 = num8 - (1f - num9) * num4;
					}
					vector = vector2;
				}
				num = num5 - num4;
				num2 = num5 + num4;
				num4 /= (float)stepsPerSegment;
			}
			return num5;
		}

		public float MinUnit(CinemachinePathBase.PositionUnits units)
		{
			if (units != CinemachinePathBase.PositionUnits.Distance)
			{
				return this.MinPos;
			}
			return 0f;
		}

		public float MaxUnit(CinemachinePathBase.PositionUnits units)
		{
			if (units != CinemachinePathBase.PositionUnits.Distance)
			{
				return this.MaxPos;
			}
			return this.PathLength;
		}

		public virtual float NormalizeUnit(float pos, CinemachinePathBase.PositionUnits units)
		{
			if (units == CinemachinePathBase.PositionUnits.Distance)
			{
				return this.NormalizePathDistance(pos);
			}
			return this.NormalizePos(pos);
		}

		public Vector3 EvaluatePositionAtUnit(float pos, CinemachinePathBase.PositionUnits units)
		{
			if (units == CinemachinePathBase.PositionUnits.Distance)
			{
				pos = this.GetPathPositionFromDistance(pos);
			}
			return this.EvaluatePosition(pos);
		}

		public Vector3 EvaluateTangentAtUnit(float pos, CinemachinePathBase.PositionUnits units)
		{
			if (units == CinemachinePathBase.PositionUnits.Distance)
			{
				pos = this.GetPathPositionFromDistance(pos);
			}
			return this.EvaluateTangent(pos);
		}

		public Quaternion EvaluateOrientationAtUnit(float pos, CinemachinePathBase.PositionUnits units)
		{
			if (units == CinemachinePathBase.PositionUnits.Distance)
			{
				pos = this.GetPathPositionFromDistance(pos);
			}
			return this.EvaluateOrientation(pos);
		}

		public abstract int DistanceCacheSampleStepsPerSegment { get; }

		public virtual void InvalidateDistanceCache()
		{
			this.m_DistanceToPos = null;
			this.m_PosToDistance = null;
			this.m_CachedSampleSteps = 0;
			this.m_PathLength = 0f;
		}

		public bool DistanceCacheIsValid()
		{
			return this.MaxPos == this.MinPos || (this.m_DistanceToPos != null && this.m_PosToDistance != null && this.m_CachedSampleSteps == this.DistanceCacheSampleStepsPerSegment && this.m_CachedSampleSteps > 0);
		}

		public float PathLength
		{
			get
			{
				if (this.DistanceCacheSampleStepsPerSegment < 1)
				{
					return 0f;
				}
				if (!this.DistanceCacheIsValid())
				{
					this.ResamplePath(this.DistanceCacheSampleStepsPerSegment);
				}
				return this.m_PathLength;
			}
		}

		public float NormalizePathDistance(float distance)
		{
			float pathLength = this.PathLength;
			if (pathLength < 1E-05f)
			{
				return 0f;
			}
			if (this.Looped)
			{
				distance %= pathLength;
				if (distance < 0f)
				{
					distance += pathLength;
				}
			}
			return Mathf.Clamp(distance, 0f, pathLength);
		}

		public float GetPathPositionFromDistance(float distance)
		{
			if (this.DistanceCacheSampleStepsPerSegment < 1 || this.PathLength < 0.0001f)
			{
				return this.MinPos;
			}
			distance = this.NormalizePathDistance(distance);
			float num = distance / this.m_cachedDistanceStepSize;
			int num2 = Mathf.FloorToInt(num);
			if (num2 >= this.m_DistanceToPos.Length - 1)
			{
				return this.MaxPos;
			}
			float t = num - (float)num2;
			return this.MinPos + Mathf.Lerp(this.m_DistanceToPos[num2], this.m_DistanceToPos[num2 + 1], t);
		}

		public float GetPathDistanceFromPosition(float pos)
		{
			if (this.DistanceCacheSampleStepsPerSegment < 1 || this.PathLength < 0.0001f)
			{
				return 0f;
			}
			pos = this.NormalizePos(pos);
			float num = pos / this.m_cachedPosStepSize;
			int num2 = Mathf.FloorToInt(num);
			if (num2 >= this.m_PosToDistance.Length - 1)
			{
				return this.m_PathLength;
			}
			float t = num - (float)num2;
			return Mathf.Lerp(this.m_PosToDistance[num2], this.m_PosToDistance[num2 + 1], t);
		}

		private void ResamplePath(int stepsPerSegment)
		{
			this.InvalidateDistanceCache();
			float minPos = this.MinPos;
			float maxPos = this.MaxPos;
			float num = 1f / (float)Mathf.Max(1, stepsPerSegment);
			int num2 = Mathf.RoundToInt((maxPos - minPos) / num) + 1;
			this.m_PosToDistance = new float[num2];
			this.m_CachedSampleSteps = stepsPerSegment;
			this.m_cachedPosStepSize = num;
			Vector3 a = this.EvaluatePosition(0f);
			this.m_PosToDistance[0] = 0f;
			float num3 = minPos;
			for (int i = 1; i < num2; i++)
			{
				num3 += num;
				Vector3 vector = this.EvaluatePosition(num3);
				float num4 = Vector3.Distance(a, vector);
				this.m_PathLength += num4;
				a = vector;
				this.m_PosToDistance[i] = this.m_PathLength;
			}
			this.m_DistanceToPos = new float[num2];
			this.m_DistanceToPos[0] = 0f;
			if (num2 > 1)
			{
				num = this.m_PathLength / (float)(num2 - 1);
				this.m_cachedDistanceStepSize = num;
				float num5 = 0f;
				int num6 = 1;
				for (int j = 1; j < num2; j++)
				{
					num5 += num;
					float num7 = this.m_PosToDistance[num6];
					while (num7 < num5 && num6 < num2 - 1)
					{
						num7 = this.m_PosToDistance[++num6];
					}
					float num8 = this.m_PosToDistance[num6 - 1];
					float num9 = num7 - num8;
					float num10 = (num5 - num8) / num9;
					this.m_DistanceToPos[j] = this.m_cachedPosStepSize * (num10 + (float)num6 - 1f);
				}
			}
		}

		[Tooltip("Path samples per waypoint.  This is used for calculating path distances.")]
		[Range(1f, 100f)]
		public int m_Resolution = 20;

		[Tooltip("The settings that control how the path will appear in the editor scene view.")]
		public CinemachinePathBase.Appearance m_Appearance = new CinemachinePathBase.Appearance();

		private float[] m_DistanceToPos;

		private float[] m_PosToDistance;

		private int m_CachedSampleSteps;

		private float m_PathLength;

		private float m_cachedPosStepSize;

		private float m_cachedDistanceStepSize;

		[DocumentationSorting(18.1f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public class Appearance
		{
			[Tooltip("The color of the path itself when it is active in the editor")]
			public Color pathColor = Color.green;

			[Tooltip("The color of the path itself when it is inactive in the editor")]
			public Color inactivePathColor = Color.gray;

			[Tooltip("The width of the railroad-tracks that are drawn to represent the path")]
			[Range(0f, 10f)]
			public float width = 0.2f;
		}

		public enum PositionUnits
		{
			PathUnits,
			Distance
		}
	}
}
