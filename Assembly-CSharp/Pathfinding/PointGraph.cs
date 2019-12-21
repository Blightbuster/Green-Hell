using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[JsonOptIn]
	public class PointGraph : NavGraph, IUpdatableGraph
	{
		public int nodeCount { get; protected set; }

		public override int CountNodes()
		{
			return this.nodeCount;
		}

		public override void GetNodes(Action<GraphNode> action)
		{
			if (this.nodes == null)
			{
				return;
			}
			int nodeCount = this.nodeCount;
			for (int i = 0; i < nodeCount; i++)
			{
				action(this.nodes[i]);
			}
		}

		public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
		{
			return this.GetNearestForce(position, null);
		}

		public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			if (this.nodes == null)
			{
				return default(NNInfoInternal);
			}
			if (this.optimizeForSparseGraph)
			{
				return new NNInfoInternal(this.lookupTree.GetNearest((Int3)position, constraint));
			}
			float num = (constraint == null || constraint.constrainDistance) ? AstarPath.active.maxNearestNodeDistanceSqr : float.PositiveInfinity;
			NNInfoInternal result = new NNInfoInternal(null);
			float num2 = float.PositiveInfinity;
			float num3 = float.PositiveInfinity;
			for (int i = 0; i < this.nodeCount; i++)
			{
				PointNode pointNode = this.nodes[i];
				float sqrMagnitude = (position - (Vector3)pointNode.position).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					result.node = pointNode;
				}
				if (sqrMagnitude < num3 && sqrMagnitude < num && (constraint == null || constraint.Suitable(pointNode)))
				{
					num3 = sqrMagnitude;
					result.constrainedNode = pointNode;
				}
			}
			result.UpdateInfo();
			return result;
		}

		public PointNode AddNode(Int3 position)
		{
			return this.AddNode<PointNode>(new PointNode(this.active), position);
		}

		public T AddNode<T>(T node, Int3 position) where T : PointNode
		{
			if (this.nodes == null || this.nodeCount == this.nodes.Length)
			{
				PointNode[] array = new PointNode[(this.nodes != null) ? Math.Max(this.nodes.Length + 4, this.nodes.Length * 2) : 4];
				for (int i = 0; i < this.nodeCount; i++)
				{
					array[i] = this.nodes[i];
				}
				this.nodes = array;
			}
			node.SetPosition(position);
			node.GraphIndex = this.graphIndex;
			node.Walkable = true;
			this.nodes[this.nodeCount] = node;
			int nodeCount = this.nodeCount;
			this.nodeCount = nodeCount + 1;
			this.AddToLookup(node);
			return node;
		}

		protected static int CountChildren(Transform tr)
		{
			int num = 0;
			foreach (object obj in tr)
			{
				Transform tr2 = (Transform)obj;
				num++;
				num += PointGraph.CountChildren(tr2);
			}
			return num;
		}

		protected void AddChildren(ref int c, Transform tr)
		{
			foreach (object obj in tr)
			{
				Transform transform = (Transform)obj;
				this.nodes[c].SetPosition((Int3)transform.position);
				this.nodes[c].Walkable = true;
				this.nodes[c].gameObject = transform.gameObject;
				c++;
				this.AddChildren(ref c, transform);
			}
		}

		public void RebuildNodeLookup()
		{
			if (!this.optimizeForSparseGraph || this.nodes == null)
			{
				this.lookupTree = new PointKDTree();
				return;
			}
			PointKDTree pointKDTree = this.lookupTree;
			GraphNode[] array = this.nodes;
			pointKDTree.Rebuild(array, 0, this.nodeCount);
		}

		private void AddToLookup(PointNode node)
		{
			this.lookupTree.Add(node);
		}

		public override IEnumerable<Progress> ScanInternal()
		{
			yield return new Progress(0f, "Searching for GameObjects");
			if (this.root == null)
			{
				GameObject[] gos = (this.searchTag != null) ? GameObject.FindGameObjectsWithTag(this.searchTag) : null;
				if (gos == null)
				{
					this.nodes = new PointNode[0];
					this.nodeCount = 0;
					yield break;
				}
				yield return new Progress(0.1f, "Creating nodes");
				this.nodes = new PointNode[gos.Length];
				this.nodeCount = this.nodes.Length;
				for (int j = 0; j < this.nodes.Length; j++)
				{
					this.nodes[j] = new PointNode(this.active);
				}
				for (int k = 0; k < gos.Length; k++)
				{
					this.nodes[k].SetPosition((Int3)gos[k].transform.position);
					this.nodes[k].Walkable = true;
					this.nodes[k].gameObject = gos[k].gameObject;
				}
				gos = null;
			}
			else
			{
				if (!this.recursive)
				{
					this.nodes = new PointNode[this.root.childCount];
					this.nodeCount = this.nodes.Length;
					for (int l = 0; l < this.nodes.Length; l++)
					{
						this.nodes[l] = new PointNode(this.active);
					}
					int num = 0;
					using (IEnumerator enumerator = this.root.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							object obj = enumerator.Current;
							Transform transform = (Transform)obj;
							this.nodes[num].SetPosition((Int3)transform.position);
							this.nodes[num].Walkable = true;
							this.nodes[num].gameObject = transform.gameObject;
							num++;
						}
						goto IL_2B5;
					}
				}
				this.nodes = new PointNode[PointGraph.CountChildren(this.root)];
				this.nodeCount = this.nodes.Length;
				for (int m = 0; m < this.nodes.Length; m++)
				{
					this.nodes[m] = new PointNode(this.active);
				}
				int num2 = 0;
				this.AddChildren(ref num2, this.root);
			}
			IL_2B5:
			if (this.optimizeForSparseGraph)
			{
				yield return new Progress(0.15f, "Building node lookup");
				this.RebuildNodeLookup();
			}
			if (this.maxDistance >= 0f)
			{
				List<Connection> connections = new List<Connection>();
				List<GraphNode> candidateConnections = new List<GraphNode>();
				long maxPossibleSqrRange;
				if (this.maxDistance == 0f && (this.limits.x == 0f || this.limits.y == 0f || this.limits.z == 0f))
				{
					maxPossibleSqrRange = long.MaxValue;
				}
				else
				{
					maxPossibleSqrRange = (long)(Mathf.Max(this.limits.x, Mathf.Max(this.limits.y, Mathf.Max(this.limits.z, this.maxDistance))) * 1000f) + 1L;
					maxPossibleSqrRange *= maxPossibleSqrRange;
				}
				int num6;
				for (int i = 0; i < this.nodes.Length; i = num6 + 1)
				{
					if (i % 512 == 0)
					{
						yield return new Progress(Mathf.Lerp(0.15f, 1f, (float)i / (float)this.nodes.Length), "Connecting nodes");
					}
					connections.Clear();
					PointNode pointNode = this.nodes[i];
					if (this.optimizeForSparseGraph)
					{
						candidateConnections.Clear();
						this.lookupTree.GetInRange(pointNode.position, maxPossibleSqrRange, candidateConnections);
						for (int n = 0; n < candidateConnections.Count; n++)
						{
							PointNode pointNode2 = candidateConnections[n] as PointNode;
							float num3;
							if (pointNode2 != pointNode && this.IsValidConnection(pointNode, pointNode2, out num3))
							{
								connections.Add(new Connection
								{
									node = pointNode2,
									cost = (uint)Mathf.RoundToInt(num3 * 1000f)
								});
							}
						}
					}
					else
					{
						for (int num4 = 0; num4 < this.nodes.Length; num4++)
						{
							if (i != num4)
							{
								PointNode pointNode3 = this.nodes[num4];
								float num5;
								if (this.IsValidConnection(pointNode, pointNode3, out num5))
								{
									connections.Add(new Connection
									{
										node = pointNode3,
										cost = (uint)Mathf.RoundToInt(num5 * 1000f)
									});
								}
							}
						}
					}
					pointNode.connections = connections.ToArray();
					num6 = i;
				}
				connections = null;
				candidateConnections = null;
			}
			yield break;
			yield break;
		}

		public virtual bool IsValidConnection(GraphNode a, GraphNode b, out float dist)
		{
			dist = 0f;
			if (!a.Walkable || !b.Walkable)
			{
				return false;
			}
			Vector3 vector = (Vector3)(b.position - a.position);
			if ((!Mathf.Approximately(this.limits.x, 0f) && Mathf.Abs(vector.x) > this.limits.x) || (!Mathf.Approximately(this.limits.y, 0f) && Mathf.Abs(vector.y) > this.limits.y) || (!Mathf.Approximately(this.limits.z, 0f) && Mathf.Abs(vector.z) > this.limits.z))
			{
				return false;
			}
			dist = vector.magnitude;
			if (this.maxDistance != 0f && dist >= this.maxDistance)
			{
				return false;
			}
			if (!this.raycast)
			{
				return true;
			}
			Ray ray = new Ray((Vector3)a.position, vector);
			Ray ray2 = new Ray((Vector3)b.position, -vector);
			if (this.use2DPhysics)
			{
				if (this.thickRaycast)
				{
					return !Physics2D.CircleCast(ray.origin, this.thickRaycastRadius, ray.direction, dist, this.mask) && !Physics2D.CircleCast(ray2.origin, this.thickRaycastRadius, ray2.direction, dist, this.mask);
				}
				return !Physics2D.Linecast((Vector3)a.position, (Vector3)b.position, this.mask) && !Physics2D.Linecast((Vector3)b.position, (Vector3)a.position, this.mask);
			}
			else
			{
				if (this.thickRaycast)
				{
					return !Physics.SphereCast(ray, this.thickRaycastRadius, dist, this.mask) && !Physics.SphereCast(ray2, this.thickRaycastRadius, dist, this.mask);
				}
				return !Physics.Linecast((Vector3)a.position, (Vector3)b.position, this.mask) && !Physics.Linecast((Vector3)b.position, (Vector3)a.position, this.mask);
			}
		}

		public GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o)
		{
			return GraphUpdateThreading.UnityThread;
		}

		public void UpdateAreaInit(GraphUpdateObject o)
		{
		}

		public void UpdateAreaPost(GraphUpdateObject o)
		{
		}

		public void UpdateArea(GraphUpdateObject guo)
		{
			if (this.nodes == null)
			{
				return;
			}
			for (int i = 0; i < this.nodeCount; i++)
			{
				if (guo.bounds.Contains((Vector3)this.nodes[i].position))
				{
					guo.WillUpdateNode(this.nodes[i]);
					guo.Apply(this.nodes[i]);
				}
			}
			if (guo.updatePhysics)
			{
				Bounds bounds = guo.bounds;
				if (this.thickRaycast)
				{
					bounds.Expand(this.thickRaycastRadius * 2f);
				}
				List<Connection> list = ListPool<Connection>.Claim();
				for (int j = 0; j < this.nodeCount; j++)
				{
					PointNode pointNode = this.nodes[j];
					Vector3 a = (Vector3)pointNode.position;
					List<Connection> list2 = null;
					for (int k = 0; k < this.nodeCount; k++)
					{
						if (k != j)
						{
							Vector3 b = (Vector3)this.nodes[k].position;
							if (VectorMath.SegmentIntersectsBounds(bounds, a, b))
							{
								PointNode pointNode2 = this.nodes[k];
								bool flag = pointNode.ContainsConnection(pointNode2);
								float num;
								bool flag2 = this.IsValidConnection(pointNode, pointNode2, out num);
								if (!flag && flag2)
								{
									if (list2 == null)
									{
										list.Clear();
										list2 = list;
										list2.AddRange(pointNode.connections);
									}
									uint cost = (uint)Mathf.RoundToInt(num * 1000f);
									list2.Add(new Connection
									{
										node = pointNode2,
										cost = cost
									});
								}
								else if (flag && !flag2)
								{
									if (list2 == null)
									{
										list.Clear();
										list2 = list;
										list2.AddRange(pointNode.connections);
									}
									for (int l = 0; l < list2.Count; l++)
									{
										if (list2[l].node == pointNode2)
										{
											list2.RemoveAt(l);
											break;
										}
									}
								}
							}
						}
					}
					if (list2 != null)
					{
						pointNode.connections = list2.ToArray();
					}
				}
				ListPool<Connection>.Release(list);
			}
		}

		public override void PostDeserialization()
		{
			this.RebuildNodeLookup();
		}

		public override void RelocateNodes(Matrix4x4 deltaMatrix)
		{
			base.RelocateNodes(deltaMatrix);
			this.RebuildNodeLookup();
		}

		public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			base.DeserializeSettingsCompatibility(ctx);
			this.root = (ctx.DeserializeUnityObject() as Transform);
			this.searchTag = ctx.reader.ReadString();
			this.maxDistance = ctx.reader.ReadSingle();
			this.limits = ctx.DeserializeVector3();
			this.raycast = ctx.reader.ReadBoolean();
			this.use2DPhysics = ctx.reader.ReadBoolean();
			this.thickRaycast = ctx.reader.ReadBoolean();
			this.thickRaycastRadius = ctx.reader.ReadSingle();
			this.recursive = ctx.reader.ReadBoolean();
			ctx.reader.ReadBoolean();
			this.mask = ctx.reader.ReadInt32();
			this.optimizeForSparseGraph = ctx.reader.ReadBoolean();
			ctx.reader.ReadBoolean();
		}

		public override void SerializeExtraInfo(GraphSerializationContext ctx)
		{
			if (this.nodes == null)
			{
				ctx.writer.Write(-1);
			}
			ctx.writer.Write(this.nodeCount);
			for (int i = 0; i < this.nodeCount; i++)
			{
				if (this.nodes[i] == null)
				{
					ctx.writer.Write(-1);
				}
				else
				{
					ctx.writer.Write(0);
					this.nodes[i].SerializeNode(ctx);
				}
			}
		}

		public override void DeserializeExtraInfo(GraphSerializationContext ctx)
		{
			int num = ctx.reader.ReadInt32();
			if (num == -1)
			{
				this.nodes = null;
				return;
			}
			this.nodes = new PointNode[num];
			this.nodeCount = num;
			for (int i = 0; i < this.nodes.Length; i++)
			{
				if (ctx.reader.ReadInt32() != -1)
				{
					this.nodes[i] = new PointNode(this.active);
					this.nodes[i].DeserializeNode(ctx);
				}
			}
		}

		[JsonMember]
		public Transform root;

		[JsonMember]
		public string searchTag;

		[JsonMember]
		public float maxDistance;

		[JsonMember]
		public Vector3 limits;

		[JsonMember]
		public bool raycast = true;

		[JsonMember]
		public bool use2DPhysics;

		[JsonMember]
		public bool thickRaycast;

		[JsonMember]
		public float thickRaycastRadius = 1f;

		[JsonMember]
		public bool recursive = true;

		[JsonMember]
		public LayerMask mask;

		[JsonMember]
		public bool optimizeForSparseGraph;

		private PointKDTree lookupTree = new PointKDTree();

		public PointNode[] nodes;
	}
}
