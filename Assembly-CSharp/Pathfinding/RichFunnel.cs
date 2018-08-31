using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public class RichFunnel : RichPathPart
	{
		public RichFunnel()
		{
			this.left = ListPool<Vector3>.Claim();
			this.right = ListPool<Vector3>.Claim();
			this.nodes = new List<TriangleMeshNode>();
			this.graph = null;
		}

		public RichFunnel Initialize(RichPath path, NavGraph graph)
		{
			if (graph == null)
			{
				throw new ArgumentNullException("graph");
			}
			if (this.graph != null)
			{
				throw new InvalidOperationException("Trying to initialize an already initialized object. " + graph);
			}
			this.graph = graph;
			this.path = path;
			return this;
		}

		public override void OnEnterPool()
		{
			this.left.Clear();
			this.right.Clear();
			this.nodes.Clear();
			this.graph = null;
			this.currentNode = 0;
			this.checkForDestroyedNodesCounter = 0;
		}

		public TriangleMeshNode CurrentNode
		{
			get
			{
				TriangleMeshNode triangleMeshNode = this.nodes[this.currentNode];
				if (!triangleMeshNode.Destroyed)
				{
					return triangleMeshNode;
				}
				return null;
			}
		}

		public void BuildFunnelCorridor(List<GraphNode> nodes, int start, int end)
		{
			this.exactStart = (nodes[start] as MeshNode).ClosestPointOnNode(this.exactStart);
			this.exactEnd = (nodes[end] as MeshNode).ClosestPointOnNode(this.exactEnd);
			this.left.Clear();
			this.right.Clear();
			this.left.Add(this.exactStart);
			this.right.Add(this.exactStart);
			this.nodes.Clear();
			IRaycastableGraph raycastableGraph = this.graph as IRaycastableGraph;
			if (raycastableGraph != null && this.funnelSimplification)
			{
				List<GraphNode> list = ListPool<GraphNode>.Claim(end - start);
				this.SimplifyPath(raycastableGraph, nodes, start, end, list, this.exactStart, this.exactEnd);
				if (this.nodes.Capacity < list.Count)
				{
					this.nodes.Capacity = list.Count;
				}
				for (int i = 0; i < list.Count; i++)
				{
					TriangleMeshNode triangleMeshNode = list[i] as TriangleMeshNode;
					if (triangleMeshNode != null)
					{
						this.nodes.Add(triangleMeshNode);
					}
				}
				ListPool<GraphNode>.Release(list);
			}
			else
			{
				if (this.nodes.Capacity < end - start)
				{
					this.nodes.Capacity = end - start;
				}
				for (int j = start; j <= end; j++)
				{
					TriangleMeshNode triangleMeshNode2 = nodes[j] as TriangleMeshNode;
					if (triangleMeshNode2 != null)
					{
						this.nodes.Add(triangleMeshNode2);
					}
				}
			}
			for (int k = 0; k < this.nodes.Count - 1; k++)
			{
				this.nodes[k].GetPortal(this.nodes[k + 1], this.left, this.right, false);
			}
			this.left.Add(this.exactEnd);
			this.right.Add(this.exactEnd);
		}

		private void SimplifyPath(IRaycastableGraph graph, List<GraphNode> nodes, int start, int end, List<GraphNode> result, Vector3 startPoint, Vector3 endPoint)
		{
			if (graph == null)
			{
				throw new ArgumentNullException("graph");
			}
			if (start > end)
			{
				throw new ArgumentException("start >= end");
			}
			int num = start;
			int num2 = 0;
			while (num2++ <= 1000)
			{
				if (start == end)
				{
					result.Add(nodes[end]);
					return;
				}
				int count = result.Count;
				int i = end + 1;
				int num3 = start + 1;
				bool flag = false;
				while (i > num3 + 1)
				{
					int num4 = (i + num3) / 2;
					Vector3 start2 = (start != num) ? ((Vector3)nodes[start].position) : startPoint;
					Vector3 end2 = (num4 != end) ? ((Vector3)nodes[num4].position) : endPoint;
					GraphHitInfo graphHitInfo;
					if (graph.Linecast(start2, end2, nodes[start], out graphHitInfo))
					{
						i = num4;
					}
					else
					{
						flag = true;
						num3 = num4;
					}
				}
				if (!flag)
				{
					result.Add(nodes[start]);
					start = num3;
				}
				else
				{
					Vector3 start3 = (start != num) ? ((Vector3)nodes[start].position) : startPoint;
					Vector3 end3 = (num3 != end) ? ((Vector3)nodes[num3].position) : endPoint;
					GraphHitInfo graphHitInfo2;
					graph.Linecast(start3, end3, nodes[start], out graphHitInfo2, result);
					long num5 = 0L;
					long num6 = 0L;
					for (int j = start; j <= num3; j++)
					{
						num5 += (long)((ulong)nodes[j].Penalty + (ulong)((long)((!(this.path.seeker != null)) ? 0 : this.path.seeker.tagPenalties[(int)((UIntPtr)nodes[j].Tag)])));
					}
					for (int k = count; k < result.Count; k++)
					{
						num6 += (long)((ulong)result[k].Penalty + (ulong)((long)((!(this.path.seeker != null)) ? 0 : this.path.seeker.tagPenalties[(int)((UIntPtr)result[k].Tag)])));
					}
					if ((double)num5 * 1.4 * (double)(num3 - start + 1) < (double)(num6 * (long)(result.Count - count)) || result[result.Count - 1] != nodes[num3])
					{
						result.RemoveRange(count, result.Count - count);
						result.Add(nodes[start]);
						start++;
					}
					else
					{
						result.RemoveAt(result.Count - 1);
						start = num3;
					}
				}
			}
			Debug.LogError("Was the path really long or have we got cought in an infinite loop?");
		}

		private void UpdateFunnelCorridor(int splitIndex, TriangleMeshNode prefix)
		{
			if (splitIndex > 0)
			{
				this.nodes.RemoveRange(0, splitIndex - 1);
				this.nodes[0] = prefix;
			}
			else
			{
				this.nodes.Insert(0, prefix);
			}
			this.left.Clear();
			this.right.Clear();
			this.left.Add(this.exactStart);
			this.right.Add(this.exactStart);
			for (int i = 0; i < this.nodes.Count - 1; i++)
			{
				this.nodes[i].GetPortal(this.nodes[i + 1], this.left, this.right, false);
			}
			this.left.Add(this.exactEnd);
			this.right.Add(this.exactEnd);
		}

		private bool CheckForDestroyedNodes()
		{
			int i = 0;
			int count = this.nodes.Count;
			while (i < count)
			{
				if (this.nodes[i].Destroyed)
				{
					return true;
				}
				i++;
			}
			return false;
		}

		public float DistanceToEndOfPath
		{
			get
			{
				TriangleMeshNode triangleMeshNode = this.CurrentNode;
				Vector3 b = (triangleMeshNode == null) ? this.currentPosition : triangleMeshNode.ClosestPointOnNode(this.currentPosition);
				return (this.exactEnd - b).magnitude;
			}
		}

		public Vector3 ClampToNavmesh(Vector3 position)
		{
			if (this.path.transform != null)
			{
				position = this.path.transform.InverseTransform(position);
			}
			this.ClampToNavmeshInternal(ref position);
			if (this.path.transform != null)
			{
				position = this.path.transform.Transform(position);
			}
			return position;
		}

		public Vector3 Update(Vector3 position, List<Vector3> buffer, int numCorners, out bool lastCorner, out bool requiresRepath)
		{
			if (this.path.transform != null)
			{
				position = this.path.transform.InverseTransform(position);
			}
			lastCorner = false;
			requiresRepath = false;
			if (this.checkForDestroyedNodesCounter >= 10)
			{
				this.checkForDestroyedNodesCounter = 0;
				requiresRepath |= this.CheckForDestroyedNodes();
			}
			else
			{
				this.checkForDestroyedNodesCounter++;
			}
			bool flag = this.ClampToNavmeshInternal(ref position);
			this.currentPosition = position;
			if (flag)
			{
				requiresRepath = true;
				lastCorner = false;
				buffer.Add(position);
			}
			else if (!this.FindNextCorners(position, this.currentNode, buffer, numCorners, out lastCorner))
			{
				Debug.LogError("Failed to find next corners in the path");
				buffer.Add(position);
			}
			if (this.path.transform != null)
			{
				for (int i = 0; i < buffer.Count; i++)
				{
					buffer[i] = this.path.transform.Transform(buffer[i]);
				}
				position = this.path.transform.Transform(position);
			}
			return position;
		}

		private bool ClampToNavmeshInternal(ref Vector3 position)
		{
			if (this.nodes[this.currentNode].Destroyed)
			{
				return true;
			}
			Int3 p = (Int3)position;
			if (this.nodes[this.currentNode].ContainsPoint(p))
			{
				return false;
			}
			int i = this.currentNode + 1;
			int num = Math.Min(this.currentNode + 3, this.nodes.Count);
			while (i < num)
			{
				if (this.nodes[i].Destroyed)
				{
					return true;
				}
				if (this.nodes[i].ContainsPoint(p))
				{
					this.currentNode = i;
					return false;
				}
				i++;
			}
			int j = this.currentNode - 1;
			int num2 = Math.Max(this.currentNode - 3, 0);
			while (j > num2)
			{
				if (this.nodes[j].Destroyed)
				{
					return true;
				}
				if (this.nodes[j].ContainsPoint(p))
				{
					this.currentNode = j;
					return false;
				}
				j--;
			}
			return this.ClampToNavmeshInternalFull(ref position);
		}

		private bool ClampToNavmeshInternalFull(ref Vector3 position)
		{
			int index = 0;
			float closestDist = float.PositiveInfinity;
			bool closestIsInPath = false;
			TriangleMeshNode closestNode = null;
			this.checkForDestroyedNodesCounter = 0;
			int i = 0;
			int count = this.nodes.Count;
			while (i < count)
			{
				if (this.nodes[i].Destroyed)
				{
					return true;
				}
				Vector3 a = this.nodes[i].ClosestPointOnNode(position);
				float sqrMagnitude = (a - position).sqrMagnitude;
				if (sqrMagnitude < closestDist)
				{
					closestDist = sqrMagnitude;
					index = i;
					closestNode = this.nodes[i];
					closestIsInPath = true;
				}
				i++;
			}
			Vector3 posCopy = position;
			int containingIndex = this.nodes.Count - 1;
			int closestIsNeighbourOf = 0;
			Action<GraphNode> action = delegate(GraphNode node)
			{
				if ((containingIndex <= 0 || node != this.nodes[containingIndex - 1]) && (containingIndex >= this.nodes.Count - 1 || node != this.nodes[containingIndex + 1]))
				{
					TriangleMeshNode triangleMeshNode = node as TriangleMeshNode;
					if (triangleMeshNode != null)
					{
						Vector3 a2 = triangleMeshNode.ClosestPointOnNode(posCopy);
						float sqrMagnitude2 = (a2 - posCopy).sqrMagnitude;
						if (sqrMagnitude2 < closestDist)
						{
							closestDist = sqrMagnitude2;
							closestIsNeighbourOf = containingIndex;
							closestNode = triangleMeshNode;
							closestIsInPath = false;
						}
					}
				}
			};
			while (containingIndex >= 0)
			{
				this.nodes[containingIndex].GetConnections(action);
				containingIndex--;
			}
			if (closestIsInPath)
			{
				this.currentNode = index;
				position = this.nodes[index].ClosestPointOnNodeXZ(position);
			}
			else
			{
				position = closestNode.ClosestPointOnNodeXZ(position);
				this.exactStart = position;
				this.UpdateFunnelCorridor(closestIsNeighbourOf, closestNode);
				this.currentNode = 0;
			}
			return false;
		}

		public void FindWalls(List<Vector3> wallBuffer, float range)
		{
			this.FindWalls(this.currentNode, wallBuffer, this.currentPosition, range);
		}

		private void FindWalls(int nodeIndex, List<Vector3> wallBuffer, Vector3 position, float range)
		{
			if (range <= 0f)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			range *= range;
			position.y = 0f;
			int num = 0;
			while (!flag || !flag2)
			{
				if (num >= 0 || !flag)
				{
					if (num <= 0 || !flag2)
					{
						if (num < 0 && nodeIndex + num < 0)
						{
							flag = true;
						}
						else if (num > 0 && nodeIndex + num >= this.nodes.Count)
						{
							flag2 = true;
						}
						else
						{
							TriangleMeshNode triangleMeshNode = (nodeIndex + num - 1 >= 0) ? this.nodes[nodeIndex + num - 1] : null;
							TriangleMeshNode triangleMeshNode2 = this.nodes[nodeIndex + num];
							TriangleMeshNode triangleMeshNode3 = (nodeIndex + num + 1 < this.nodes.Count) ? this.nodes[nodeIndex + num + 1] : null;
							if (triangleMeshNode2.Destroyed)
							{
								break;
							}
							if ((triangleMeshNode2.ClosestPointOnNodeXZ(position) - position).sqrMagnitude > range)
							{
								if (num < 0)
								{
									flag = true;
								}
								else
								{
									flag2 = true;
								}
							}
							else
							{
								for (int i = 0; i < 3; i++)
								{
									this.triBuffer[i] = 0;
								}
								for (int j = 0; j < triangleMeshNode2.connections.Length; j++)
								{
									TriangleMeshNode triangleMeshNode4 = triangleMeshNode2.connections[j].node as TriangleMeshNode;
									if (triangleMeshNode4 != null)
									{
										int num2 = -1;
										for (int k = 0; k < 3; k++)
										{
											for (int l = 0; l < 3; l++)
											{
												if (triangleMeshNode2.GetVertex(k) == triangleMeshNode4.GetVertex((l + 1) % 3) && triangleMeshNode2.GetVertex((k + 1) % 3) == triangleMeshNode4.GetVertex(l))
												{
													num2 = k;
													k = 3;
													break;
												}
											}
										}
										if (num2 != -1)
										{
											this.triBuffer[num2] = ((triangleMeshNode4 != triangleMeshNode && triangleMeshNode4 != triangleMeshNode3) ? 1 : 2);
										}
									}
								}
								for (int m = 0; m < 3; m++)
								{
									if (this.triBuffer[m] == 0)
									{
										wallBuffer.Add((Vector3)triangleMeshNode2.GetVertex(m));
										wallBuffer.Add((Vector3)triangleMeshNode2.GetVertex((m + 1) % 3));
									}
								}
							}
						}
					}
				}
				num = ((num >= 0) ? (-num - 1) : (-num));
			}
			if (this.path.transform != null)
			{
				for (int n = 0; n < wallBuffer.Count; n++)
				{
					wallBuffer[n] = this.path.transform.Transform(wallBuffer[n]);
				}
			}
		}

		private bool FindNextCorners(Vector3 origin, int startIndex, List<Vector3> funnelPath, int numCorners, out bool lastCorner)
		{
			lastCorner = false;
			if (this.left == null)
			{
				throw new Exception("left list is null");
			}
			if (this.right == null)
			{
				throw new Exception("right list is null");
			}
			if (funnelPath == null)
			{
				throw new ArgumentNullException("funnelPath");
			}
			if (this.left.Count != this.right.Count)
			{
				throw new ArgumentException("left and right lists must have equal length");
			}
			int count = this.left.Count;
			if (count == 0)
			{
				throw new ArgumentException("no diagonals");
			}
			if (count - startIndex < 3)
			{
				funnelPath.Add(this.left[count - 1]);
				lastCorner = true;
				return true;
			}
			while (this.left[startIndex + 1] == this.left[startIndex + 2] && this.right[startIndex + 1] == this.right[startIndex + 2])
			{
				startIndex++;
				if (count - startIndex <= 3)
				{
					return false;
				}
			}
			Vector3 vector = this.left[startIndex + 2];
			if (vector == this.left[startIndex + 1])
			{
				vector = this.right[startIndex + 2];
			}
			while (VectorMath.IsColinearXZ(origin, this.left[startIndex + 1], this.right[startIndex + 1]) || VectorMath.RightOrColinearXZ(this.left[startIndex + 1], this.right[startIndex + 1], vector) == VectorMath.RightOrColinearXZ(this.left[startIndex + 1], this.right[startIndex + 1], origin))
			{
				startIndex++;
				if (count - startIndex < 3)
				{
					funnelPath.Add(this.left[count - 1]);
					lastCorner = true;
					return true;
				}
				vector = this.left[startIndex + 2];
				if (vector == this.left[startIndex + 1])
				{
					vector = this.right[startIndex + 2];
				}
			}
			Vector3 vector2 = origin;
			Vector3 vector3 = this.left[startIndex + 1];
			Vector3 vector4 = this.right[startIndex + 1];
			int num = startIndex + 1;
			int num2 = startIndex + 1;
			int i = startIndex + 2;
			while (i < count)
			{
				if (funnelPath.Count >= numCorners)
				{
					return true;
				}
				if (funnelPath.Count > 2000)
				{
					Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
					break;
				}
				Vector3 vector5 = this.left[i];
				Vector3 vector6 = this.right[i];
				if (VectorMath.SignedTriangleAreaTimes2XZ(vector2, vector4, vector6) < 0f)
				{
					goto IL_2FB;
				}
				if (vector2 == vector4 || VectorMath.SignedTriangleAreaTimes2XZ(vector2, vector3, vector6) <= 0f)
				{
					vector4 = vector6;
					num = i;
					goto IL_2FB;
				}
				funnelPath.Add(vector3);
				vector2 = vector3;
				int num3 = num2;
				vector3 = vector2;
				vector4 = vector2;
				num2 = num3;
				num = num3;
				i = num3;
				IL_35F:
				i++;
				continue;
				IL_2FB:
				if (VectorMath.SignedTriangleAreaTimes2XZ(vector2, vector3, vector5) > 0f)
				{
					goto IL_35F;
				}
				if (vector2 == vector3 || VectorMath.SignedTriangleAreaTimes2XZ(vector2, vector4, vector5) >= 0f)
				{
					vector3 = vector5;
					num2 = i;
					goto IL_35F;
				}
				funnelPath.Add(vector4);
				vector2 = vector4;
				num3 = num;
				vector3 = vector2;
				vector4 = vector2;
				num2 = num3;
				num = num3;
				i = num3;
				goto IL_35F;
			}
			lastCorner = true;
			funnelPath.Add(this.left[count - 1]);
			return true;
		}

		private readonly List<Vector3> left;

		private readonly List<Vector3> right;

		private List<TriangleMeshNode> nodes;

		public Vector3 exactStart;

		public Vector3 exactEnd;

		private NavGraph graph;

		private int currentNode;

		private Vector3 currentPosition;

		private int checkForDestroyedNodesCounter;

		private RichPath path;

		private int[] triBuffer = new int[3];

		public bool funnelSimplification = true;
	}
}
