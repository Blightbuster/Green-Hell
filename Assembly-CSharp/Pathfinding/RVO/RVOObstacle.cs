using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.RVO
{
	public abstract class RVOObstacle : VersionedMonoBehaviour
	{
		protected abstract void CreateObstacles();

		protected abstract bool ExecuteInEditor { get; }

		protected abstract bool LocalCoordinates { get; }

		protected abstract bool StaticObstacle { get; }

		protected abstract float Height { get; }

		protected abstract bool AreGizmosDirty();

		public void OnDrawGizmos()
		{
			this.OnDrawGizmos(false);
		}

		public void OnDrawGizmosSelected()
		{
			this.OnDrawGizmos(true);
		}

		public void OnDrawGizmos(bool selected)
		{
			this.gizmoDrawing = true;
			Gizmos.color = new Color(0.615f, 1f, 0.06f, (!selected) ? 0.7f : 1f);
			MovementPlane movementPlane = (!(RVOSimulator.active != null)) ? MovementPlane.XZ : RVOSimulator.active.movementPlane;
			Vector3 vector = (movementPlane != MovementPlane.XZ) ? (-Vector3.forward) : Vector3.up;
			if (this.gizmoVerts == null || this.AreGizmosDirty() || this._obstacleMode != this.obstacleMode)
			{
				this._obstacleMode = this.obstacleMode;
				if (this.gizmoVerts == null)
				{
					this.gizmoVerts = new List<Vector3[]>();
				}
				else
				{
					this.gizmoVerts.Clear();
				}
				this.CreateObstacles();
			}
			Matrix4x4 matrix = this.GetMatrix();
			for (int i = 0; i < this.gizmoVerts.Count; i++)
			{
				Vector3[] array = this.gizmoVerts[i];
				int j = 0;
				int num = array.Length - 1;
				while (j < array.Length)
				{
					Gizmos.DrawLine(matrix.MultiplyPoint3x4(array[j]), matrix.MultiplyPoint3x4(array[num]));
					num = j++;
				}
				if (selected)
				{
					int k = 0;
					int num2 = array.Length - 1;
					while (k < array.Length)
					{
						Vector3 vector2 = matrix.MultiplyPoint3x4(array[num2]);
						Vector3 vector3 = matrix.MultiplyPoint3x4(array[k]);
						if (movementPlane != MovementPlane.XY)
						{
							Gizmos.DrawLine(vector2 + vector * this.Height, vector3 + vector * this.Height);
							Gizmos.DrawLine(vector2, vector2 + vector * this.Height);
						}
						Vector3 vector4 = (vector2 + vector3) * 0.5f;
						Vector3 normalized = (vector3 - vector2).normalized;
						if (!(normalized == Vector3.zero))
						{
							Vector3 vector5 = Vector3.Cross(vector, normalized);
							Gizmos.DrawLine(vector4, vector4 + vector5);
							Gizmos.DrawLine(vector4 + vector5, vector4 + vector5 * 0.5f + normalized * 0.5f);
							Gizmos.DrawLine(vector4 + vector5, vector4 + vector5 * 0.5f - normalized * 0.5f);
						}
						num2 = k++;
					}
				}
			}
			this.gizmoDrawing = false;
		}

		protected virtual Matrix4x4 GetMatrix()
		{
			return (!this.LocalCoordinates) ? Matrix4x4.identity : base.transform.localToWorldMatrix;
		}

		public void OnDisable()
		{
			if (this.addedObstacles != null)
			{
				if (this.sim == null)
				{
					throw new Exception("This should not happen! Make sure you are not overriding the OnEnable function");
				}
				for (int i = 0; i < this.addedObstacles.Count; i++)
				{
					this.sim.RemoveObstacle(this.addedObstacles[i]);
				}
			}
		}

		public void OnEnable()
		{
			if (this.addedObstacles != null)
			{
				if (this.sim == null)
				{
					throw new Exception("This should not happen! Make sure you are not overriding the OnDisable function");
				}
				for (int i = 0; i < this.addedObstacles.Count; i++)
				{
					ObstacleVertex obstacleVertex = this.addedObstacles[i];
					ObstacleVertex obstacleVertex2 = obstacleVertex;
					do
					{
						obstacleVertex.layer = this.layer;
						obstacleVertex = obstacleVertex.next;
					}
					while (obstacleVertex != obstacleVertex2);
					this.sim.AddObstacle(this.addedObstacles[i]);
				}
			}
		}

		public void Start()
		{
			this.addedObstacles = new List<ObstacleVertex>();
			this.sourceObstacles = new List<Vector3[]>();
			this.prevUpdateMatrix = this.GetMatrix();
			this.CreateObstacles();
		}

		public void Update()
		{
			Matrix4x4 matrix = this.GetMatrix();
			if (matrix != this.prevUpdateMatrix)
			{
				for (int i = 0; i < this.addedObstacles.Count; i++)
				{
					this.sim.UpdateObstacle(this.addedObstacles[i], this.sourceObstacles[i], matrix);
				}
				this.prevUpdateMatrix = matrix;
			}
		}

		protected void FindSimulator()
		{
			if (RVOSimulator.active == null)
			{
				throw new InvalidOperationException("No RVOSimulator could be found in the scene. Please add one to any GameObject");
			}
			this.sim = RVOSimulator.active.GetSimulator();
		}

		protected void AddObstacle(Vector3[] vertices, float height)
		{
			if (vertices == null)
			{
				throw new ArgumentNullException("Vertices Must Not Be Null");
			}
			if (height < 0f)
			{
				throw new ArgumentOutOfRangeException("Height must be non-negative");
			}
			if (vertices.Length < 2)
			{
				throw new ArgumentException("An obstacle must have at least two vertices");
			}
			if (this.sim == null)
			{
				this.FindSimulator();
			}
			if (this.gizmoDrawing)
			{
				Vector3[] array = new Vector3[vertices.Length];
				this.WindCorrectly(vertices);
				Array.Copy(vertices, array, vertices.Length);
				this.gizmoVerts.Add(array);
				return;
			}
			if (vertices.Length == 2)
			{
				this.AddObstacleInternal(vertices, height);
				return;
			}
			this.WindCorrectly(vertices);
			this.AddObstacleInternal(vertices, height);
		}

		private void AddObstacleInternal(Vector3[] vertices, float height)
		{
			this.addedObstacles.Add(this.sim.AddObstacle(vertices, height, this.GetMatrix(), this.layer, true));
			this.sourceObstacles.Add(vertices);
		}

		private void WindCorrectly(Vector3[] vertices)
		{
			int num = 0;
			float num2 = float.PositiveInfinity;
			Matrix4x4 matrix = this.GetMatrix();
			for (int i = 0; i < vertices.Length; i++)
			{
				float x = matrix.MultiplyPoint3x4(vertices[i]).x;
				if (x < num2)
				{
					num = i;
					num2 = x;
				}
			}
			Vector3 a = matrix.MultiplyPoint3x4(vertices[(num - 1 + vertices.Length) % vertices.Length]);
			Vector3 b = matrix.MultiplyPoint3x4(vertices[num]);
			Vector3 c = matrix.MultiplyPoint3x4(vertices[(num + 1) % vertices.Length]);
			MovementPlane movementPlane;
			if (this.sim != null)
			{
				movementPlane = this.sim.movementPlane;
			}
			else if (RVOSimulator.active)
			{
				movementPlane = RVOSimulator.active.movementPlane;
			}
			else
			{
				movementPlane = MovementPlane.XZ;
			}
			if (movementPlane == MovementPlane.XY)
			{
				a.z = a.y;
				b.z = b.y;
				c.z = c.y;
			}
			if (VectorMath.IsClockwiseXZ(a, b, c) != (this.obstacleMode == RVOObstacle.ObstacleVertexWinding.KeepIn))
			{
				Array.Reverse(vertices);
			}
		}

		public RVOObstacle.ObstacleVertexWinding obstacleMode;

		public RVOLayer layer = RVOLayer.DefaultObstacle;

		protected Simulator sim;

		private List<ObstacleVertex> addedObstacles;

		private List<Vector3[]> sourceObstacles;

		private bool gizmoDrawing;

		private List<Vector3[]> gizmoVerts;

		private RVOObstacle.ObstacleVertexWinding _obstacleMode;

		private Matrix4x4 prevUpdateMatrix;

		public enum ObstacleVertexWinding
		{
			KeepOut,
			KeepIn
		}
	}
}
