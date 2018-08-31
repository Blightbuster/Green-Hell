using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Util
{
	public class PathInterpolator
	{
		public virtual Vector3 position
		{
			get
			{
				float t = (this.currentSegmentLength <= 0.0001f) ? 0f : ((this.currentDistance - this.distanceToSegmentStart) / this.currentSegmentLength);
				return Vector3.Lerp(this.path[this.segmentIndex], this.path[this.segmentIndex + 1], t);
			}
		}

		public Vector3 tangent
		{
			get
			{
				return this.path[this.segmentIndex + 1] - this.path[this.segmentIndex];
			}
		}

		public float remainingDistance
		{
			get
			{
				return this.totalDistance - this.distance;
			}
			set
			{
				this.distance = this.totalDistance - value;
			}
		}

		public float distance
		{
			get
			{
				return this.currentDistance;
			}
			set
			{
				this.currentDistance = value;
				while (this.currentDistance < this.distanceToSegmentStart && this.segmentIndex > 0)
				{
					this.PrevSegment();
				}
				while (this.currentDistance > this.distanceToSegmentStart + this.currentSegmentLength && this.segmentIndex < this.path.Count - 2)
				{
					this.NextSegment();
				}
			}
		}

		public int segmentIndex { get; private set; }

		public bool valid
		{
			get
			{
				return this.path != null;
			}
		}

		public void SetPath(List<Vector3> path)
		{
			if (path.Count < 2)
			{
				throw new ArgumentException("Path must have a length of at least 2");
			}
			this.path = path;
			this.currentDistance = 0f;
			this.segmentIndex = 0;
			this.distanceToSegmentStart = 0f;
			this.currentSegmentLength = (path[1] - path[0]).magnitude;
			this.totalDistance = 0f;
			Vector3 b = path[0];
			for (int i = 1; i < path.Count; i++)
			{
				Vector3 vector = path[i];
				this.totalDistance += (vector - b).magnitude;
				b = vector;
			}
		}

		public void MoveToSegment(int index, float fractionAlongSegment)
		{
			if (index < 0 || index >= this.path.Count - 1)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			while (this.segmentIndex > index)
			{
				this.PrevSegment();
			}
			while (this.segmentIndex < index)
			{
				this.NextSegment();
			}
			this.distance = this.distanceToSegmentStart + Mathf.Clamp01(fractionAlongSegment) * this.currentSegmentLength;
		}

		public void MoveToClosestPoint(Vector3 point)
		{
			float num = float.PositiveInfinity;
			float fractionAlongSegment = 0f;
			int index = 0;
			for (int i = 0; i < this.path.Count - 1; i++)
			{
				float num2 = VectorMath.ClosestPointOnLineFactor(this.path[i], this.path[i + 1], point);
				Vector3 b = Vector3.Lerp(this.path[i], this.path[i + 1], num2);
				float sqrMagnitude = (point - b).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					fractionAlongSegment = num2;
					index = i;
				}
			}
			this.MoveToSegment(index, fractionAlongSegment);
		}

		public void MoveToLocallyClosestPoint(Vector3 point, bool allowForwards = true, bool allowBackwards = true)
		{
			while (allowForwards && this.segmentIndex < this.path.Count - 2 && (this.path[this.segmentIndex + 1] - point).sqrMagnitude <= (this.path[this.segmentIndex] - point).sqrMagnitude)
			{
				this.NextSegment();
			}
			while (allowBackwards && this.segmentIndex > 0 && (this.path[this.segmentIndex - 1] - point).sqrMagnitude <= (this.path[this.segmentIndex] - point).sqrMagnitude)
			{
				this.PrevSegment();
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = float.PositiveInfinity;
			float num4 = float.PositiveInfinity;
			if (this.segmentIndex > 0)
			{
				num = VectorMath.ClosestPointOnLineFactor(this.path[this.segmentIndex - 1], this.path[this.segmentIndex], point);
				num3 = (Vector3.Lerp(this.path[this.segmentIndex - 1], this.path[this.segmentIndex], num) - point).sqrMagnitude;
			}
			if (this.segmentIndex < this.path.Count - 1)
			{
				num2 = VectorMath.ClosestPointOnLineFactor(this.path[this.segmentIndex], this.path[this.segmentIndex + 1], point);
				num4 = (Vector3.Lerp(this.path[this.segmentIndex], this.path[this.segmentIndex + 1], num2) - point).sqrMagnitude;
			}
			if (num3 < num4)
			{
				this.MoveToSegment(this.segmentIndex - 1, num);
			}
			else
			{
				this.MoveToSegment(this.segmentIndex, num2);
			}
		}

		public void MoveToCircleIntersection2D(Vector3 circleCenter3D, float radius, IMovementPlane transform)
		{
			Vector2 vector = transform.ToPlane(circleCenter3D);
			while (this.segmentIndex < this.path.Count - 2 && (transform.ToPlane(this.path[this.segmentIndex + 1]) - vector).sqrMagnitude <= radius * radius)
			{
				this.NextSegment();
			}
			float fractionAlongSegment = VectorMath.LineCircleIntersectionFactor(vector, transform.ToPlane(this.path[this.segmentIndex]), transform.ToPlane(this.path[this.segmentIndex + 1]), radius);
			this.MoveToSegment(this.segmentIndex, fractionAlongSegment);
		}

		protected virtual void PrevSegment()
		{
			this.segmentIndex--;
			this.currentSegmentLength = (this.path[this.segmentIndex + 1] - this.path[this.segmentIndex]).magnitude;
			this.distanceToSegmentStart -= this.currentSegmentLength;
		}

		protected virtual void NextSegment()
		{
			this.segmentIndex++;
			this.distanceToSegmentStart += this.currentSegmentLength;
			this.currentSegmentLength = (this.path[this.segmentIndex + 1] - this.path[this.segmentIndex]).magnitude;
		}

		private List<Vector3> path;

		private float distanceToSegmentStart;

		private float currentSegmentLength;

		private float currentDistance;

		private float totalDistance;
	}
}
