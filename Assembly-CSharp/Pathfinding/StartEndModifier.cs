using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	[Serializable]
	public class StartEndModifier : PathModifier
	{
		public override int Order
		{
			get
			{
				return 0;
			}
		}

		public override void Apply(Path _p)
		{
			ABPath abpath = _p as ABPath;
			if (abpath == null || abpath.vectorPath.Count == 0)
			{
				return;
			}
			if (abpath.vectorPath.Count == 1 && !this.addPoints)
			{
				abpath.vectorPath.Add(abpath.vectorPath[0]);
			}
			bool flag;
			Vector3 vector = this.Snap(abpath, this.exactStartPoint, true, out flag);
			bool flag2;
			Vector3 vector2 = this.Snap(abpath, this.exactEndPoint, false, out flag2);
			if ((flag || this.addPoints) && this.exactStartPoint != StartEndModifier.Exactness.SnapToNode)
			{
				abpath.vectorPath.Insert(0, vector);
			}
			else
			{
				abpath.vectorPath[0] = vector;
			}
			if ((flag2 || this.addPoints) && this.exactEndPoint != StartEndModifier.Exactness.SnapToNode)
			{
				abpath.vectorPath.Add(vector2);
				return;
			}
			abpath.vectorPath[abpath.vectorPath.Count - 1] = vector2;
		}

		private Vector3 Snap(ABPath path, StartEndModifier.Exactness mode, bool start, out bool forceAddPoint)
		{
			int num = start ? 0 : (path.path.Count - 1);
			GraphNode graphNode = path.path[num];
			Vector3 vector = (Vector3)graphNode.position;
			forceAddPoint = false;
			switch (mode)
			{
			case StartEndModifier.Exactness.SnapToNode:
				return vector;
			case StartEndModifier.Exactness.Original:
			case StartEndModifier.Exactness.Interpolate:
			case StartEndModifier.Exactness.NodeConnection:
			{
				Vector3 vector2;
				if (start)
				{
					vector2 = ((this.adjustStartPoint != null) ? this.adjustStartPoint() : path.originalStartPoint);
				}
				else
				{
					vector2 = path.originalEndPoint;
				}
				switch (mode)
				{
				case StartEndModifier.Exactness.Original:
					return this.GetClampedPoint(vector, vector2, graphNode);
				case StartEndModifier.Exactness.Interpolate:
				{
					Vector3 clampedPoint = this.GetClampedPoint(vector, vector2, graphNode);
					GraphNode graphNode2 = path.path[Mathf.Clamp(num + (start ? 1 : -1), 0, path.path.Count - 1)];
					return VectorMath.ClosestPointOnSegment(vector, (Vector3)graphNode2.position, clampedPoint);
				}
				case StartEndModifier.Exactness.NodeConnection:
				{
					this.connectionBuffer = (this.connectionBuffer ?? new List<GraphNode>());
					Action<GraphNode> action;
					if ((action = this.connectionBufferAddDelegate) == null)
					{
						action = new Action<GraphNode>(this.connectionBuffer.Add);
					}
					this.connectionBufferAddDelegate = action;
					GraphNode graphNode2 = path.path[Mathf.Clamp(num + (start ? 1 : -1), 0, path.path.Count - 1)];
					graphNode.GetConnections(this.connectionBufferAddDelegate);
					Vector3 result = vector;
					float num2 = float.PositiveInfinity;
					for (int i = this.connectionBuffer.Count - 1; i >= 0; i--)
					{
						GraphNode graphNode3 = this.connectionBuffer[i];
						Vector3 vector3 = VectorMath.ClosestPointOnSegment(vector, (Vector3)graphNode3.position, vector2);
						float sqrMagnitude = (vector3 - vector2).sqrMagnitude;
						if (sqrMagnitude < num2)
						{
							result = vector3;
							num2 = sqrMagnitude;
							forceAddPoint = (graphNode3 != graphNode2);
						}
					}
					this.connectionBuffer.Clear();
					return result;
				}
				}
				throw new ArgumentException("Cannot reach this point, but the compiler is not smart enough to realize that.");
			}
			case StartEndModifier.Exactness.ClosestOnNode:
				return this.GetClampedPoint(vector, start ? path.startPoint : path.endPoint, graphNode);
			default:
				throw new ArgumentException("Invalid mode");
			}
		}

		public Vector3 GetClampedPoint(Vector3 from, Vector3 to, GraphNode hint)
		{
			Vector3 vector = to;
			RaycastHit raycastHit;
			if (this.useRaycasting && Physics.Linecast(from, to, out raycastHit, this.mask))
			{
				vector = raycastHit.point;
			}
			if (this.useGraphRaycasting && hint != null)
			{
				IRaycastableGraph raycastableGraph = AstarData.GetGraph(hint) as IRaycastableGraph;
				GraphHitInfo graphHitInfo;
				if (raycastableGraph != null && raycastableGraph.Linecast(from, vector, hint, out graphHitInfo))
				{
					vector = graphHitInfo.point;
				}
			}
			return vector;
		}

		public bool addPoints;

		public StartEndModifier.Exactness exactStartPoint = StartEndModifier.Exactness.ClosestOnNode;

		public StartEndModifier.Exactness exactEndPoint = StartEndModifier.Exactness.ClosestOnNode;

		public Func<Vector3> adjustStartPoint;

		public bool useRaycasting;

		public LayerMask mask = -1;

		public bool useGraphRaycasting;

		private List<GraphNode> connectionBuffer;

		private Action<GraphNode> connectionBufferAddDelegate;

		public enum Exactness
		{
			SnapToNode,
			Original,
			Interpolate,
			ClosestOnNode,
			NodeConnection
		}
	}
}
