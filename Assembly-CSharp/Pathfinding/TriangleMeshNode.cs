using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding
{
	public class TriangleMeshNode : MeshNode
	{
		public TriangleMeshNode(AstarPath astar) : base(astar)
		{
		}

		public static INavmeshHolder GetNavmeshHolder(uint graphIndex)
		{
			return TriangleMeshNode._navmeshHolders[(int)graphIndex];
		}

		public static void SetNavmeshHolder(int graphIndex, INavmeshHolder graph)
		{
			if (TriangleMeshNode._navmeshHolders.Length <= graphIndex)
			{
				object obj = TriangleMeshNode.lockObject;
				lock (obj)
				{
					if (TriangleMeshNode._navmeshHolders.Length <= graphIndex)
					{
						INavmeshHolder[] array = new INavmeshHolder[graphIndex + 1];
						for (int i = 0; i < TriangleMeshNode._navmeshHolders.Length; i++)
						{
							array[i] = TriangleMeshNode._navmeshHolders[i];
						}
						TriangleMeshNode._navmeshHolders = array;
					}
				}
			}
			TriangleMeshNode._navmeshHolders[graphIndex] = graph;
		}

		public void UpdatePositionFromVertices()
		{
			Int3 lhs;
			Int3 rhs;
			Int3 rhs2;
			this.GetVertices(out lhs, out rhs, out rhs2);
			this.position = (lhs + rhs + rhs2) * 0.333333f;
		}

		public int GetVertexIndex(int i)
		{
			return (i != 0) ? ((i != 1) ? this.v2 : this.v1) : this.v0;
		}

		public int GetVertexArrayIndex(int i)
		{
			return TriangleMeshNode.GetNavmeshHolder(base.GraphIndex).GetVertexArrayIndex((i != 0) ? ((i != 1) ? this.v2 : this.v1) : this.v0);
		}

		public void GetVertices(out Int3 v0, out Int3 v1, out Int3 v2)
		{
			INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(base.GraphIndex);
			v0 = navmeshHolder.GetVertex(this.v0);
			v1 = navmeshHolder.GetVertex(this.v1);
			v2 = navmeshHolder.GetVertex(this.v2);
		}

		public override Int3 GetVertex(int i)
		{
			return TriangleMeshNode.GetNavmeshHolder(base.GraphIndex).GetVertex(this.GetVertexIndex(i));
		}

		public Int3 GetVertexInGraphSpace(int i)
		{
			return TriangleMeshNode.GetNavmeshHolder(base.GraphIndex).GetVertexInGraphSpace(this.GetVertexIndex(i));
		}

		public override int GetVertexCount()
		{
			return 3;
		}

		public override Vector3 ClosestPointOnNode(Vector3 p)
		{
			Int3 ob;
			Int3 ob2;
			Int3 ob3;
			this.GetVertices(out ob, out ob2, out ob3);
			return Polygon.ClosestPointOnTriangle((Vector3)ob, (Vector3)ob2, (Vector3)ob3, p);
		}

		public override Vector3 ClosestPointOnNodeXZ(Vector3 p)
		{
			Int3 ob;
			Int3 ob2;
			Int3 ob3;
			this.GetVertices(out ob, out ob2, out ob3);
			return Polygon.ClosestPointOnTriangleXZ((Vector3)ob, (Vector3)ob2, (Vector3)ob3, p);
		}

		public override bool ContainsPoint(Int3 p)
		{
			Int3 @int;
			Int3 int2;
			Int3 int3;
			this.GetVertices(out @int, out int2, out int3);
			return (long)(int2.x - @int.x) * (long)(p.z - @int.z) - (long)(p.x - @int.x) * (long)(int2.z - @int.z) <= 0L && (long)(int3.x - int2.x) * (long)(p.z - int2.z) - (long)(p.x - int2.x) * (long)(int3.z - int2.z) <= 0L && (long)(@int.x - int3.x) * (long)(p.z - int3.z) - (long)(p.x - int3.x) * (long)(@int.z - int3.z) <= 0L;
		}

		public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
		{
			base.UpdateG(path, pathNode);
			handler.heap.Add(pathNode);
			if (this.connections == null)
			{
				return;
			}
			for (int i = 0; i < this.connections.Length; i++)
			{
				GraphNode node = this.connections[i].node;
				PathNode pathNode2 = handler.GetPathNode(node);
				if (pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
				{
					node.UpdateRecursiveG(path, pathNode2, handler);
				}
			}
		}

		public override void Open(Path path, PathNode pathNode, PathHandler handler)
		{
			if (this.connections == null)
			{
				return;
			}
			bool flag = pathNode.flag2;
			for (int i = this.connections.Length - 1; i >= 0; i--)
			{
				Connection connection = this.connections[i];
				GraphNode node = connection.node;
				if (path.CanTraverse(connection.node))
				{
					PathNode pathNode2 = handler.GetPathNode(connection.node);
					if (pathNode2 != pathNode.parent)
					{
						uint num = connection.cost;
						if (flag || pathNode2.flag2)
						{
							num = path.GetConnectionSpecialCost(this, connection.node, num);
						}
						if (pathNode2.pathID != handler.PathID)
						{
							pathNode2.node = connection.node;
							pathNode2.parent = pathNode;
							pathNode2.pathID = handler.PathID;
							pathNode2.cost = num;
							pathNode2.H = path.CalculateHScore(node);
							node.UpdateG(path, pathNode2);
							handler.heap.Add(pathNode2);
						}
						else if (pathNode.G + num + path.GetTraversalCost(node) < pathNode2.G)
						{
							pathNode2.cost = num;
							pathNode2.parent = pathNode;
							node.UpdateRecursiveG(path, pathNode2, handler);
						}
						else if (pathNode2.G + num + path.GetTraversalCost(this) < pathNode.G && node.ContainsConnection(this))
						{
							pathNode.parent = pathNode2;
							pathNode.cost = num;
							this.UpdateRecursiveG(path, pathNode, handler);
						}
					}
				}
			}
		}

		public int SharedEdge(GraphNode other)
		{
			int result;
			int num;
			this.GetPortal(other, null, null, false, out result, out num);
			return result;
		}

		public override bool GetPortal(GraphNode _other, List<Vector3> left, List<Vector3> right, bool backwards)
		{
			int num;
			int num2;
			return this.GetPortal(_other, left, right, backwards, out num, out num2);
		}

		public bool GetPortal(GraphNode _other, List<Vector3> left, List<Vector3> right, bool backwards, out int aIndex, out int bIndex)
		{
			aIndex = -1;
			bIndex = -1;
			if (_other.GraphIndex != base.GraphIndex)
			{
				return false;
			}
			TriangleMeshNode triangleMeshNode = _other as TriangleMeshNode;
			int num = this.GetVertexIndex(0) >> 12 & 524287;
			int num2 = triangleMeshNode.GetVertexIndex(0) >> 12 & 524287;
			if (num != num2 && TriangleMeshNode.GetNavmeshHolder(base.GraphIndex) is RecastGraph)
			{
				for (int i = 0; i < this.connections.Length; i++)
				{
					if (this.connections[i].node.GraphIndex != base.GraphIndex)
					{
						NodeLink3Node nodeLink3Node = this.connections[i].node as NodeLink3Node;
						if (nodeLink3Node != null && nodeLink3Node.GetOther(this) == triangleMeshNode && left != null)
						{
							nodeLink3Node.GetPortal(triangleMeshNode, left, right, false);
							return true;
						}
					}
				}
				INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(base.GraphIndex);
				int num3;
				int num4;
				navmeshHolder.GetTileCoordinates(num, out num3, out num4);
				int num5;
				int num6;
				navmeshHolder.GetTileCoordinates(num2, out num5, out num6);
				int num7;
				if (Math.Abs(num3 - num5) == 1)
				{
					num7 = 0;
				}
				else
				{
					if (Math.Abs(num4 - num6) != 1)
					{
						throw new Exception(string.Concat(new object[]
						{
							"Tiles not adjacent (",
							num3,
							", ",
							num4,
							") (",
							num5,
							", ",
							num6,
							")"
						}));
					}
					num7 = 2;
				}
				int vertexCount = this.GetVertexCount();
				int vertexCount2 = triangleMeshNode.GetVertexCount();
				int num8 = -1;
				int num9 = -1;
				for (int j = 0; j < vertexCount; j++)
				{
					int num10 = this.GetVertex(j)[num7];
					for (int k = 0; k < vertexCount2; k++)
					{
						if (num10 == triangleMeshNode.GetVertex((k + 1) % vertexCount2)[num7] && this.GetVertex((j + 1) % vertexCount)[num7] == triangleMeshNode.GetVertex(k)[num7])
						{
							num8 = j;
							num9 = k;
							j = vertexCount;
							break;
						}
					}
				}
				aIndex = num8;
				bIndex = num9;
				if (num8 != -1)
				{
					Int3 vertex = this.GetVertex(num8);
					Int3 vertex2 = this.GetVertex((num8 + 1) % vertexCount);
					int i2 = (num7 != 2) ? 2 : 0;
					int num11 = Math.Min(vertex[i2], vertex2[i2]);
					int num12 = Math.Max(vertex[i2], vertex2[i2]);
					num11 = Math.Max(num11, Math.Min(triangleMeshNode.GetVertex(num9)[i2], triangleMeshNode.GetVertex((num9 + 1) % vertexCount2)[i2]));
					num12 = Math.Min(num12, Math.Max(triangleMeshNode.GetVertex(num9)[i2], triangleMeshNode.GetVertex((num9 + 1) % vertexCount2)[i2]));
					if (vertex[i2] < vertex2[i2])
					{
						vertex[i2] = num11;
						vertex2[i2] = num12;
					}
					else
					{
						vertex[i2] = num12;
						vertex2[i2] = num11;
					}
					if (left != null)
					{
						left.Add((Vector3)vertex);
						right.Add((Vector3)vertex2);
					}
					return true;
				}
			}
			else if (!backwards)
			{
				int num13 = -1;
				int num14 = -1;
				int vertexCount3 = this.GetVertexCount();
				int vertexCount4 = triangleMeshNode.GetVertexCount();
				for (int l = 0; l < vertexCount3; l++)
				{
					int vertexIndex = this.GetVertexIndex(l);
					for (int m = 0; m < vertexCount4; m++)
					{
						if (vertexIndex == triangleMeshNode.GetVertexIndex((m + 1) % vertexCount4) && this.GetVertexIndex((l + 1) % vertexCount3) == triangleMeshNode.GetVertexIndex(m))
						{
							num13 = l;
							num14 = m;
							l = vertexCount3;
							break;
						}
					}
				}
				aIndex = num13;
				bIndex = num14;
				if (num13 == -1)
				{
					for (int n = 0; n < this.connections.Length; n++)
					{
						if (this.connections[n].node.GraphIndex != base.GraphIndex)
						{
							NodeLink3Node nodeLink3Node2 = this.connections[n].node as NodeLink3Node;
							if (nodeLink3Node2 != null && nodeLink3Node2.GetOther(this) == triangleMeshNode && left != null)
							{
								nodeLink3Node2.GetPortal(triangleMeshNode, left, right, false);
								return true;
							}
						}
					}
					return false;
				}
				if (left != null)
				{
					left.Add((Vector3)this.GetVertex(num13));
					right.Add((Vector3)this.GetVertex((num13 + 1) % vertexCount3));
				}
			}
			return true;
		}

		public override float SurfaceArea()
		{
			INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(base.GraphIndex);
			return (float)Math.Abs(VectorMath.SignedTriangleAreaTimes2XZ(navmeshHolder.GetVertex(this.v0), navmeshHolder.GetVertex(this.v1), navmeshHolder.GetVertex(this.v2))) * 0.5f;
		}

		public override Vector3 RandomPointOnSurface()
		{
			float value;
			float value2;
			do
			{
				value = UnityEngine.Random.value;
				value2 = UnityEngine.Random.value;
			}
			while (value + value2 > 1f);
			INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(base.GraphIndex);
			return (Vector3)(navmeshHolder.GetVertex(this.v1) - navmeshHolder.GetVertex(this.v0)) * value + (Vector3)(navmeshHolder.GetVertex(this.v2) - navmeshHolder.GetVertex(this.v0)) * value2 + (Vector3)navmeshHolder.GetVertex(this.v0);
		}

		public override void SerializeNode(GraphSerializationContext ctx)
		{
			base.SerializeNode(ctx);
			ctx.writer.Write(this.v0);
			ctx.writer.Write(this.v1);
			ctx.writer.Write(this.v2);
		}

		public override void DeserializeNode(GraphSerializationContext ctx)
		{
			base.DeserializeNode(ctx);
			this.v0 = ctx.reader.ReadInt32();
			this.v1 = ctx.reader.ReadInt32();
			this.v2 = ctx.reader.ReadInt32();
		}

		public int v0;

		public int v1;

		public int v2;

		protected static INavmeshHolder[] _navmeshHolders = new INavmeshHolder[0];

		protected static readonly object lockObject = new object();
	}
}
