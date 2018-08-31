using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Util
{
	public class GraphGizmoHelper : IDisposable, IAstarPooledObject
	{
		public GraphGizmoHelper()
		{
			this.drawConnection = new Action<GraphNode>(this.DrawConnection);
		}

		public RetainedGizmos.Hasher hasher { get; private set; }

		public RetainedGizmos.Builder builder { get; private set; }

		public void Init(AstarPath active, RetainedGizmos.Hasher hasher, RetainedGizmos gizmos)
		{
			this.debugData = active.debugPathData;
			this.debugPathID = active.debugPathID;
			this.debugMode = active.debugMode;
			this.debugFloor = active.debugFloor;
			this.debugRoof = active.debugRoof;
			this.gizmos = gizmos;
			this.hasher = hasher;
			this.builder = ObjectPool<RetainedGizmos.Builder>.Claim();
			this.showSearchTree = (active.showSearchTree && this.debugData != null);
		}

		public void OnEnterPool()
		{
			RetainedGizmos.Builder builder = this.builder;
			ObjectPool<RetainedGizmos.Builder>.Release(ref builder);
			this.builder = null;
			this.debugData = null;
		}

		public void DrawConnections(GraphNode node)
		{
			if (this.showSearchTree)
			{
				if (GraphGizmoHelper.InSearchTree(node, this.debugData, this.debugPathID))
				{
					PathNode pathNode = this.debugData.GetPathNode(node);
					if (pathNode.parent != null)
					{
						this.builder.DrawLine((Vector3)node.position, (Vector3)this.debugData.GetPathNode(node).parent.node.position, this.NodeColor(node));
					}
				}
			}
			else
			{
				this.drawConnectionColor = this.NodeColor(node);
				this.drawConnectionStart = (Vector3)node.position;
				node.GetConnections(this.drawConnection);
			}
		}

		private void DrawConnection(GraphNode other)
		{
			this.builder.DrawLine(this.drawConnectionStart, Vector3.Lerp((Vector3)other.position, this.drawConnectionStart, 0.5f), this.drawConnectionColor);
		}

		public Color NodeColor(GraphNode node)
		{
			if (this.showSearchTree && !GraphGizmoHelper.InSearchTree(node, this.debugData, this.debugPathID))
			{
				return Color.clear;
			}
			Color result;
			if (node.Walkable)
			{
				GraphDebugMode graphDebugMode = this.debugMode;
				switch (graphDebugMode)
				{
				case GraphDebugMode.Penalty:
					result = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (node.Penalty - this.debugFloor) / (this.debugRoof - this.debugFloor));
					break;
				case GraphDebugMode.Connections:
					result = AstarColor.NodeConnection;
					break;
				case GraphDebugMode.Tags:
					result = AstarColor.GetAreaColor(node.Tag);
					break;
				default:
					if (graphDebugMode != GraphDebugMode.Areas)
					{
						if (this.debugData == null)
						{
							result = AstarColor.NodeConnection;
						}
						else
						{
							PathNode pathNode = this.debugData.GetPathNode(node);
							float num;
							if (this.debugMode == GraphDebugMode.G)
							{
								num = pathNode.G;
							}
							else if (this.debugMode == GraphDebugMode.H)
							{
								num = pathNode.H;
							}
							else
							{
								num = pathNode.F;
							}
							result = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (num - this.debugFloor) / (this.debugRoof - this.debugFloor));
						}
					}
					else
					{
						result = AstarColor.GetAreaColor(node.Area);
					}
					break;
				}
			}
			else
			{
				result = AstarColor.UnwalkableNode;
			}
			return result;
		}

		public static bool InSearchTree(GraphNode node, PathHandler handler, ushort pathID)
		{
			return handler.GetPathNode(node).pathID == pathID;
		}

		public void DrawWireTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
		{
			this.builder.DrawLine(a, b, color);
			this.builder.DrawLine(b, c, color);
			this.builder.DrawLine(c, a, color);
		}

		public void DrawTriangles(Vector3[] vertices, Color[] colors, int numTriangles)
		{
			List<int> list = ListPool<int>.Claim(numTriangles);
			for (int i = 0; i < numTriangles * 3; i++)
			{
				list.Add(i);
			}
			this.builder.DrawMesh(this.gizmos, vertices, list, colors);
			ListPool<int>.Release(list);
		}

		public void DrawWireTriangles(Vector3[] vertices, Color[] colors, int numTriangles)
		{
			for (int i = 0; i < numTriangles; i++)
			{
				this.DrawWireTriangle(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2], colors[i * 3]);
			}
		}

		public void Submit()
		{
			this.builder.Submit(this.gizmos, this.hasher);
		}

		void IDisposable.Dispose()
		{
			GraphGizmoHelper graphGizmoHelper = this;
			this.Submit();
			ObjectPool<GraphGizmoHelper>.Release(ref graphGizmoHelper);
		}

		private RetainedGizmos gizmos;

		private PathHandler debugData;

		private ushort debugPathID;

		private GraphDebugMode debugMode;

		private bool showSearchTree;

		private float debugFloor;

		private float debugRoof;

		private Vector3 drawConnectionStart;

		private Color drawConnectionColor;

		private readonly Action<GraphNode> drawConnection;
	}
}
