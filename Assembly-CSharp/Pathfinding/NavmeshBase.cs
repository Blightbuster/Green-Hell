using System;
using System.Collections.Generic;
using System.IO;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public abstract class NavmeshBase : NavGraph, INavmesh, INavmeshHolder, ITransformedGraph, IRaycastableGraph
	{
		public abstract float TileWorldSizeX { get; }

		public abstract float TileWorldSizeZ { get; }

		protected abstract float MaxTileConnectionEdgeDistance { get; }

		GraphTransform ITransformedGraph.transform
		{
			get
			{
				return this.transform;
			}
		}

		public abstract GraphTransform CalculateTransform();

		public NavmeshTile GetTile(int x, int z)
		{
			return this.tiles[x + z * this.tileXCount];
		}

		public Int3 GetVertex(int index)
		{
			int num = index >> 12 & 524287;
			return this.tiles[num].GetVertex(index);
		}

		public Int3 GetVertexInGraphSpace(int index)
		{
			int num = index >> 12 & 524287;
			return this.tiles[num].GetVertexInGraphSpace(index);
		}

		public static int GetTileIndex(int index)
		{
			return index >> 12 & 524287;
		}

		public int GetVertexArrayIndex(int index)
		{
			return index & 4095;
		}

		public void GetTileCoordinates(int tileIndex, out int x, out int z)
		{
			z = tileIndex / this.tileXCount;
			x = tileIndex - z * this.tileXCount;
		}

		public NavmeshTile[] GetTiles()
		{
			return this.tiles;
		}

		public Bounds GetTileBounds(IntRect rect)
		{
			return this.GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
		}

		public Bounds GetTileBounds(int x, int z, int width = 1, int depth = 1)
		{
			return this.transform.Transform(this.GetTileBoundsInGraphSpace(x, z, width, depth));
		}

		public Bounds GetTileBoundsInGraphSpace(IntRect rect)
		{
			return this.GetTileBoundsInGraphSpace(rect.xmin, rect.ymin, rect.Width, rect.Height);
		}

		public Bounds GetTileBoundsInGraphSpace(int x, int z, int width = 1, int depth = 1)
		{
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3((float)x * this.TileWorldSizeX, 0f, (float)z * this.TileWorldSizeZ), new Vector3((float)(x + width) * this.TileWorldSizeX, this.forcedBoundsSize.y, (float)(z + depth) * this.TileWorldSizeZ));
			return result;
		}

		public Int2 GetTileCoordinates(Vector3 p)
		{
			p = this.transform.InverseTransform(p);
			p.x /= this.TileWorldSizeX;
			p.z /= this.TileWorldSizeZ;
			return new Int2((int)p.x, (int)p.z);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			TriangleMeshNode.SetNavmeshHolder(this.active.data.GetGraphIndex(this), null);
			if (this.tiles != null)
			{
				for (int i = 0; i < this.tiles.Length; i++)
				{
					ObjectPool<BBTree>.Release(ref this.tiles[i].bbTree);
				}
			}
		}

		public override void RelocateNodes(Matrix4x4 deltaMatrix)
		{
			this.RelocateNodes(deltaMatrix * this.transform);
		}

		public void RelocateNodes(GraphTransform newTransform)
		{
			this.transform = newTransform;
			if (this.tiles != null)
			{
				for (int i = 0; i < this.tiles.Length; i++)
				{
					NavmeshTile navmeshTile = this.tiles[i];
					if (navmeshTile != null)
					{
						navmeshTile.vertsInGraphSpace.CopyTo(navmeshTile.verts, 0);
						this.transform.Transform(navmeshTile.verts);
						for (int j = 0; j < navmeshTile.nodes.Length; j++)
						{
							navmeshTile.nodes[j].UpdatePositionFromVertices();
						}
						navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
					}
				}
			}
		}

		protected static NavmeshTile NewEmptyTile(int x, int z)
		{
			return new NavmeshTile
			{
				x = x,
				z = z,
				w = 1,
				d = 1,
				verts = new Int3[0],
				vertsInGraphSpace = new Int3[0],
				tris = new int[0],
				nodes = new TriangleMeshNode[0],
				bbTree = ObjectPool<BBTree>.Claim()
			};
		}

		public override void GetNodes(Action<GraphNode> action)
		{
			if (this.tiles == null)
			{
				return;
			}
			for (int i = 0; i < this.tiles.Length; i++)
			{
				if (this.tiles[i] != null && this.tiles[i].x + this.tiles[i].z * this.tileXCount == i)
				{
					TriangleMeshNode[] nodes = this.tiles[i].nodes;
					if (nodes != null)
					{
						for (int j = 0; j < nodes.Length; j++)
						{
							action(nodes[j]);
						}
					}
				}
			}
		}

		public IntRect GetTouchingTiles(Bounds bounds)
		{
			bounds = this.transform.InverseTransform(bounds);
			return IntRect.Intersection(new IntRect(Mathf.FloorToInt(bounds.min.x / this.TileWorldSizeX), Mathf.FloorToInt(bounds.min.z / this.TileWorldSizeZ), Mathf.FloorToInt(bounds.max.x / this.TileWorldSizeX), Mathf.FloorToInt(bounds.max.z / this.TileWorldSizeZ)), new IntRect(0, 0, this.tileXCount - 1, this.tileZCount - 1));
		}

		public IntRect GetTouchingTilesInGraphSpace(Rect rect)
		{
			return IntRect.Intersection(new IntRect(Mathf.FloorToInt(rect.xMin / this.TileWorldSizeX), Mathf.FloorToInt(rect.yMin / this.TileWorldSizeZ), Mathf.FloorToInt(rect.xMax / this.TileWorldSizeX), Mathf.FloorToInt(rect.yMax / this.TileWorldSizeZ)), new IntRect(0, 0, this.tileXCount - 1, this.tileZCount - 1));
		}

		public IntRect GetTouchingTilesRound(Bounds bounds)
		{
			bounds = this.transform.InverseTransform(bounds);
			return IntRect.Intersection(new IntRect(Mathf.RoundToInt(bounds.min.x / this.TileWorldSizeX), Mathf.RoundToInt(bounds.min.z / this.TileWorldSizeZ), Mathf.RoundToInt(bounds.max.x / this.TileWorldSizeX) - 1, Mathf.RoundToInt(bounds.max.z / this.TileWorldSizeZ) - 1), new IntRect(0, 0, this.tileXCount - 1, this.tileZCount - 1));
		}

		protected void ConnectTileWithNeighbours(NavmeshTile tile, bool onlyUnflagged = false)
		{
			if (tile.w != 1 || tile.d != 1)
			{
				throw new ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
			}
			for (int i = -1; i <= 1; i++)
			{
				int num = tile.z + i;
				if (num >= 0 && num < this.tileZCount)
				{
					for (int j = -1; j <= 1; j++)
					{
						int num2 = tile.x + j;
						if (num2 >= 0 && num2 < this.tileXCount && j == 0 != (i == 0))
						{
							NavmeshTile navmeshTile = this.tiles[num2 + num * this.tileXCount];
							if (!onlyUnflagged || !navmeshTile.flag)
							{
								this.ConnectTiles(navmeshTile, tile);
							}
						}
					}
				}
			}
		}

		protected void RemoveConnectionsFromTile(NavmeshTile tile)
		{
			if (tile.x > 0)
			{
				int num = tile.x - 1;
				for (int i = tile.z; i < tile.z + tile.d; i++)
				{
					this.RemoveConnectionsFromTo(this.tiles[num + i * this.tileXCount], tile);
				}
			}
			if (tile.x + tile.w < this.tileXCount)
			{
				int num2 = tile.x + tile.w;
				for (int j = tile.z; j < tile.z + tile.d; j++)
				{
					this.RemoveConnectionsFromTo(this.tiles[num2 + j * this.tileXCount], tile);
				}
			}
			if (tile.z > 0)
			{
				int num3 = tile.z - 1;
				for (int k = tile.x; k < tile.x + tile.w; k++)
				{
					this.RemoveConnectionsFromTo(this.tiles[k + num3 * this.tileXCount], tile);
				}
			}
			if (tile.z + tile.d < this.tileZCount)
			{
				int num4 = tile.z + tile.d;
				for (int l = tile.x; l < tile.x + tile.w; l++)
				{
					this.RemoveConnectionsFromTo(this.tiles[l + num4 * this.tileXCount], tile);
				}
			}
		}

		protected void RemoveConnectionsFromTo(NavmeshTile a, NavmeshTile b)
		{
			if (a == null || b == null)
			{
				return;
			}
			if (a == b)
			{
				return;
			}
			int num = b.x + b.z * this.tileXCount;
			for (int i = 0; i < a.nodes.Length; i++)
			{
				TriangleMeshNode triangleMeshNode = a.nodes[i];
				if (triangleMeshNode.connections != null)
				{
					for (int j = 0; j < triangleMeshNode.connections.Length; j++)
					{
						TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j].node as TriangleMeshNode;
						if (triangleMeshNode2 != null && (triangleMeshNode2.GetVertexIndex(0) >> 12 & 524287) == num)
						{
							triangleMeshNode.RemoveConnection(triangleMeshNode.connections[j].node);
							j--;
						}
					}
				}
			}
		}

		public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
		{
			return this.GetNearestForce(position, (constraint != null && constraint.distanceXZ) ? NavmeshBase.NNConstraintDistanceXZ : null);
		}

		public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			if (this.tiles == null)
			{
				return default(NNInfoInternal);
			}
			Int2 tileCoordinates = this.GetTileCoordinates(position);
			tileCoordinates.x = Mathf.Clamp(tileCoordinates.x, 0, this.tileXCount - 1);
			tileCoordinates.y = Mathf.Clamp(tileCoordinates.y, 0, this.tileZCount - 1);
			int num = Math.Max(this.tileXCount, this.tileZCount);
			NNInfoInternal nninfoInternal = default(NNInfoInternal);
			float positiveInfinity = float.PositiveInfinity;
			bool flag = this.nearestSearchOnlyXZ || (constraint != null && constraint.distanceXZ);
			int num2 = 0;
			while (num2 < num && positiveInfinity >= (float)(num2 - 2) * Math.Max(this.TileWorldSizeX, this.TileWorldSizeX))
			{
				int num3 = Math.Min(num2 + tileCoordinates.y + 1, this.tileZCount);
				for (int i = Math.Max(-num2 + tileCoordinates.y, 0); i < num3; i++)
				{
					int num4 = Math.Abs(num2 - Math.Abs(i - tileCoordinates.y));
					int num5 = num4;
					do
					{
						int num6 = -num5 + tileCoordinates.x;
						if (num6 >= 0 && num6 < this.tileXCount)
						{
							NavmeshTile navmeshTile = this.tiles[num6 + i * this.tileXCount];
							if (navmeshTile != null)
							{
								if (flag)
								{
									nninfoInternal = navmeshTile.bbTree.QueryClosestXZ(position, constraint, ref positiveInfinity, nninfoInternal);
								}
								else
								{
									nninfoInternal = navmeshTile.bbTree.QueryClosest(position, constraint, ref positiveInfinity, nninfoInternal);
								}
							}
						}
						num5 = -num5;
					}
					while (num5 != num4);
				}
				num2++;
			}
			nninfoInternal.node = nninfoInternal.constrainedNode;
			nninfoInternal.constrainedNode = null;
			nninfoInternal.clampedPosition = nninfoInternal.constClampedPosition;
			return nninfoInternal;
		}

		public GraphNode PointOnNavmesh(Vector3 position, NNConstraint constraint)
		{
			if (this.tiles == null)
			{
				return null;
			}
			Int2 tileCoordinates = this.GetTileCoordinates(position);
			if (tileCoordinates.x < 0 || tileCoordinates.y < 0 || tileCoordinates.x >= this.tileXCount || tileCoordinates.y >= this.tileZCount)
			{
				return null;
			}
			NavmeshTile tile = this.GetTile(tileCoordinates.x, tileCoordinates.y);
			if (tile != null)
			{
				return tile.bbTree.QueryInside(position, constraint);
			}
			return null;
		}

		protected void FillWithEmptyTiles()
		{
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					this.tiles[i * this.tileXCount + j] = NavmeshBase.NewEmptyTile(j, i);
				}
			}
		}

		protected static void CreateNodeConnections(TriangleMeshNode[] nodes)
		{
			List<Connection> list = ListPool<Connection>.Claim();
			Dictionary<Int2, int> dictionary = ObjectPoolSimple<Dictionary<Int2, int>>.Claim();
			dictionary.Clear();
			for (int i = 0; i < nodes.Length; i++)
			{
				TriangleMeshNode triangleMeshNode = nodes[i];
				int vertexCount = triangleMeshNode.GetVertexCount();
				for (int j = 0; j < vertexCount; j++)
				{
					Int2 key = new Int2(triangleMeshNode.GetVertexIndex(j), triangleMeshNode.GetVertexIndex((j + 1) % vertexCount));
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, i);
					}
				}
			}
			foreach (TriangleMeshNode triangleMeshNode2 in nodes)
			{
				list.Clear();
				int vertexCount2 = triangleMeshNode2.GetVertexCount();
				for (int l = 0; l < vertexCount2; l++)
				{
					int vertexIndex = triangleMeshNode2.GetVertexIndex(l);
					int vertexIndex2 = triangleMeshNode2.GetVertexIndex((l + 1) % vertexCount2);
					int num;
					if (dictionary.TryGetValue(new Int2(vertexIndex2, vertexIndex), out num))
					{
						TriangleMeshNode triangleMeshNode3 = nodes[num];
						int vertexCount3 = triangleMeshNode3.GetVertexCount();
						for (int m = 0; m < vertexCount3; m++)
						{
							if (triangleMeshNode3.GetVertexIndex(m) == vertexIndex2 && triangleMeshNode3.GetVertexIndex((m + 1) % vertexCount3) == vertexIndex)
							{
								list.Add(new Connection
								{
									node = triangleMeshNode3,
									cost = (uint)(triangleMeshNode2.position - triangleMeshNode3.position).costMagnitude
								});
								break;
							}
						}
					}
				}
				triangleMeshNode2.connections = list.ToArrayFromPool<Connection>();
			}
			dictionary.Clear();
			ObjectPoolSimple<Dictionary<Int2, int>>.Release(ref dictionary);
			ListPool<Connection>.Release(list);
		}

		protected void ConnectTiles(NavmeshTile tile1, NavmeshTile tile2)
		{
			if (tile1 == null || tile2 == null)
			{
				return;
			}
			if (tile1.nodes == null)
			{
				throw new ArgumentException("tile1 does not contain any nodes");
			}
			if (tile2.nodes == null)
			{
				throw new ArgumentException("tile2 does not contain any nodes");
			}
			int num = Mathf.Clamp(tile2.x, tile1.x, tile1.x + tile1.w - 1);
			int num2 = Mathf.Clamp(tile1.x, tile2.x, tile2.x + tile2.w - 1);
			int num3 = Mathf.Clamp(tile2.z, tile1.z, tile1.z + tile1.d - 1);
			int num4 = Mathf.Clamp(tile1.z, tile2.z, tile2.z + tile2.d - 1);
			int i;
			int i2;
			int num5;
			int num6;
			float num7;
			if (num == num2)
			{
				i = 2;
				i2 = 0;
				num5 = num3;
				num6 = num4;
				num7 = this.TileWorldSizeZ;
			}
			else
			{
				if (num3 != num4)
				{
					throw new ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
				}
				i = 0;
				i2 = 2;
				num5 = num;
				num6 = num2;
				num7 = this.TileWorldSizeX;
			}
			if (Math.Abs(num5 - num6) != 1)
			{
				throw new ArgumentException(string.Concat(new object[]
				{
					"Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '",
					num5,
					"' and '",
					num6,
					"')"
				}));
			}
			int num8 = (int)Math.Round((double)((float)Math.Max(num5, num6) * num7 * 1000f));
			TriangleMeshNode[] nodes = tile1.nodes;
			TriangleMeshNode[] nodes2 = tile2.nodes;
			foreach (TriangleMeshNode triangleMeshNode in nodes)
			{
				int vertexCount = triangleMeshNode.GetVertexCount();
				for (int k = 0; k < vertexCount; k++)
				{
					Int3 vertexInGraphSpace = triangleMeshNode.GetVertexInGraphSpace(k);
					Int3 vertexInGraphSpace2 = triangleMeshNode.GetVertexInGraphSpace((k + 1) % vertexCount);
					if (Math.Abs(vertexInGraphSpace[i] - num8) < 2 && Math.Abs(vertexInGraphSpace2[i] - num8) < 2)
					{
						int num9 = Math.Min(vertexInGraphSpace[i2], vertexInGraphSpace2[i2]);
						int num10 = Math.Max(vertexInGraphSpace[i2], vertexInGraphSpace2[i2]);
						if (num9 != num10)
						{
							foreach (TriangleMeshNode triangleMeshNode2 in nodes2)
							{
								int vertexCount2 = triangleMeshNode2.GetVertexCount();
								for (int m = 0; m < vertexCount2; m++)
								{
									Int3 vertexInGraphSpace3 = triangleMeshNode2.GetVertexInGraphSpace(m);
									Int3 vertexInGraphSpace4 = triangleMeshNode2.GetVertexInGraphSpace((m + 1) % vertexCount);
									if (Math.Abs(vertexInGraphSpace3[i] - num8) < 2 && Math.Abs(vertexInGraphSpace4[i] - num8) < 2)
									{
										int num11 = Math.Min(vertexInGraphSpace3[i2], vertexInGraphSpace4[i2]);
										int num12 = Math.Max(vertexInGraphSpace3[i2], vertexInGraphSpace4[i2]);
										if (num11 != num12 && num10 > num11 && num9 < num12 && ((vertexInGraphSpace == vertexInGraphSpace3 && vertexInGraphSpace2 == vertexInGraphSpace4) || (vertexInGraphSpace == vertexInGraphSpace4 && vertexInGraphSpace2 == vertexInGraphSpace3) || VectorMath.SqrDistanceSegmentSegment((Vector3)vertexInGraphSpace, (Vector3)vertexInGraphSpace2, (Vector3)vertexInGraphSpace3, (Vector3)vertexInGraphSpace4) < this.MaxTileConnectionEdgeDistance * this.MaxTileConnectionEdgeDistance))
										{
											uint costMagnitude = (uint)(triangleMeshNode.position - triangleMeshNode2.position).costMagnitude;
											triangleMeshNode.AddConnection(triangleMeshNode2, costMagnitude);
											triangleMeshNode2.AddConnection(triangleMeshNode, costMagnitude);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public void StartBatchTileUpdate()
		{
			if (this.batchTileUpdate)
			{
				throw new InvalidOperationException("Calling StartBatchLoad when batching is already enabled");
			}
			this.batchTileUpdate = true;
		}

		public void EndBatchTileUpdate()
		{
			if (!this.batchTileUpdate)
			{
				throw new InvalidOperationException("Calling EndBatchLoad when batching not enabled");
			}
			this.batchTileUpdate = false;
			int num = this.tileXCount;
			int num2 = this.tileZCount;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					this.tiles[j + i * this.tileXCount].flag = false;
				}
			}
			for (int k = 0; k < this.batchUpdatedTiles.Count; k++)
			{
				this.tiles[this.batchUpdatedTiles[k]].flag = true;
			}
			for (int l = 0; l < num2; l++)
			{
				for (int m = 0; m < num; m++)
				{
					if (m < num - 1 && (this.tiles[m + l * this.tileXCount].flag || this.tiles[m + 1 + l * this.tileXCount].flag) && this.tiles[m + l * this.tileXCount] != this.tiles[m + 1 + l * this.tileXCount])
					{
						this.ConnectTiles(this.tiles[m + l * this.tileXCount], this.tiles[m + 1 + l * this.tileXCount]);
					}
					if (l < num2 - 1 && (this.tiles[m + l * this.tileXCount].flag || this.tiles[m + (l + 1) * this.tileXCount].flag) && this.tiles[m + l * this.tileXCount] != this.tiles[m + (l + 1) * this.tileXCount])
					{
						this.ConnectTiles(this.tiles[m + l * this.tileXCount], this.tiles[m + (l + 1) * this.tileXCount]);
					}
				}
			}
			this.batchUpdatedTiles.Clear();
		}

		protected void ClearTiles(int x, int z, int w, int d)
		{
			for (int i = z; i < z + d; i++)
			{
				for (int j = x; j < x + w; j++)
				{
					int num = j + i * this.tileXCount;
					NavmeshTile navmeshTile = this.tiles[num];
					if (navmeshTile != null)
					{
						navmeshTile.Destroy();
						for (int k = navmeshTile.z; k < navmeshTile.z + navmeshTile.d; k++)
						{
							for (int l = navmeshTile.x; l < navmeshTile.x + navmeshTile.w; l++)
							{
								NavmeshTile navmeshTile2 = this.tiles[l + k * this.tileXCount];
								if (navmeshTile2 == null || navmeshTile2 != navmeshTile)
								{
									throw new Exception("This should not happen");
								}
								if (k < z || k >= z + d || l < x || l >= x + w)
								{
									this.tiles[l + k * this.tileXCount] = NavmeshBase.NewEmptyTile(l, k);
									if (this.batchTileUpdate)
									{
										this.batchUpdatedTiles.Add(l + k * this.tileXCount);
									}
								}
								else
								{
									this.tiles[l + k * this.tileXCount] = null;
								}
							}
						}
					}
				}
			}
		}

		public void ReplaceTile(int x, int z, Int3[] verts, int[] tris)
		{
			this.ReplaceTile(x, z, 1, 1, verts, tris);
		}

		public void ReplaceTile(int x, int z, int w, int d, Int3[] verts, int[] tris)
		{
			if (x + w > this.tileXCount || z + d > this.tileZCount || x < 0 || z < 0)
			{
				throw new ArgumentException(string.Concat(new object[]
				{
					"Tile is placed at an out of bounds position or extends out of the graph bounds (",
					x,
					", ",
					z,
					" [",
					w,
					", ",
					d,
					"] ",
					this.tileXCount,
					" ",
					this.tileZCount,
					")"
				}));
			}
			if (w < 1 || d < 1)
			{
				throw new ArgumentException(string.Concat(new object[]
				{
					"width and depth must be greater or equal to 1. Was ",
					w,
					", ",
					d
				}));
			}
			if (tris.Length % 3 != 0)
			{
				throw new ArgumentException("Triangle array's length must be a multiple of 3 (tris)");
			}
			if (verts.Length > 65535)
			{
				throw new ArgumentException("Too many vertices per tile (more than 65535). Try using a smaller tile size.");
			}
			this.ClearTiles(x, z, w, d);
			NavmeshTile navmeshTile = new NavmeshTile
			{
				x = x,
				z = z,
				w = w,
				d = d,
				tris = tris,
				bbTree = ObjectPool<BBTree>.Claim()
			};
			if (!Mathf.Approximately((float)x * this.TileWorldSizeX * 1000f, (float)Math.Round((double)((float)x * this.TileWorldSizeX * 1000f))))
			{
				Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
			}
			if (!Mathf.Approximately((float)z * this.TileWorldSizeZ * 1000f, (float)Math.Round((double)((float)z * this.TileWorldSizeZ * 1000f))))
			{
				Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
			}
			Int3 rhs = (Int3)new Vector3((float)x * this.TileWorldSizeX, 0f, (float)z * this.TileWorldSizeZ);
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] += rhs;
			}
			navmeshTile.vertsInGraphSpace = verts;
			navmeshTile.verts = (Int3[])verts.Clone();
			this.transform.Transform(navmeshTile.verts);
			int graphIndex = AstarPath.active.data.graphs.Length;
			TriangleMeshNode.SetNavmeshHolder(graphIndex, navmeshTile);
			int num = x + z * this.tileXCount;
			num <<= 12;
			if (navmeshTile.verts.Length > 4095)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Too many vertices in the tile (",
					navmeshTile.verts.Length,
					" > ",
					4095,
					")\nYou can enable ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector to raise this limit."
				}));
				this.tiles[num] = NavmeshBase.NewEmptyTile(x, z);
				return;
			}
			TriangleMeshNode[] array = navmeshTile.nodes = this.CreateNodes(navmeshTile.tris, num, (uint)graphIndex);
			navmeshTile.bbTree.RebuildFrom(array);
			NavmeshBase.CreateNodeConnections(navmeshTile.nodes);
			for (int j = z; j < z + d; j++)
			{
				for (int k = x; k < x + w; k++)
				{
					this.tiles[k + j * this.tileXCount] = navmeshTile;
				}
			}
			if (this.batchTileUpdate)
			{
				this.batchUpdatedTiles.Add(x + z * this.tileXCount);
			}
			else
			{
				this.ConnectTileWithNeighbours(navmeshTile, false);
			}
			TriangleMeshNode.SetNavmeshHolder(graphIndex, null);
			graphIndex = AstarPath.active.data.GetGraphIndex(this);
			for (int l = 0; l < array.Length; l++)
			{
				array[l].GraphIndex = (uint)graphIndex;
			}
		}

		protected TriangleMeshNode[] CreateNodes(int[] tris, int tileIndex, uint graphIndex)
		{
			TriangleMeshNode[] array = new TriangleMeshNode[tris.Length / 3];
			for (int i = 0; i < array.Length; i++)
			{
				TriangleMeshNode triangleMeshNode = array[i] = new TriangleMeshNode(this.active);
				triangleMeshNode.GraphIndex = graphIndex;
				triangleMeshNode.v0 = (tris[i * 3] | tileIndex);
				triangleMeshNode.v1 = (tris[i * 3 + 1] | tileIndex);
				triangleMeshNode.v2 = (tris[i * 3 + 2] | tileIndex);
				if (!VectorMath.IsClockwiseXZ(triangleMeshNode.GetVertexInGraphSpace(0), triangleMeshNode.GetVertexInGraphSpace(1), triangleMeshNode.GetVertexInGraphSpace(2)))
				{
					int v = triangleMeshNode.v0;
					triangleMeshNode.v0 = triangleMeshNode.v2;
					triangleMeshNode.v2 = v;
				}
				triangleMeshNode.Walkable = true;
				triangleMeshNode.Penalty = this.initialPenalty;
				triangleMeshNode.UpdatePositionFromVertices();
			}
			return array;
		}

		public bool Linecast(Vector3 origin, Vector3 end)
		{
			return this.Linecast(origin, end, base.GetNearest(origin, NNConstraint.None).node);
		}

		public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit)
		{
			return NavmeshBase.Linecast(this, origin, end, hint, out hit, null);
		}

		public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint)
		{
			GraphHitInfo graphHitInfo;
			return NavmeshBase.Linecast(this, origin, end, hint, out graphHitInfo, null);
		}

		public bool Linecast(Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
		{
			return NavmeshBase.Linecast(this, origin, end, hint, out hit, trace);
		}

		public static bool Linecast(INavmesh graph, Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit)
		{
			return NavmeshBase.Linecast(graph, tmp_origin, tmp_end, hint, out hit, null);
		}

		public static bool Linecast(INavmesh graph, Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
		{
			Int3 @int = (Int3)tmp_end;
			Int3 int2 = (Int3)tmp_origin;
			hit = default(GraphHitInfo);
			if (float.IsNaN(tmp_origin.x + tmp_origin.y + tmp_origin.z))
			{
				throw new ArgumentException("origin is NaN");
			}
			if (float.IsNaN(tmp_end.x + tmp_end.y + tmp_end.z))
			{
				throw new ArgumentException("end is NaN");
			}
			TriangleMeshNode triangleMeshNode = hint as TriangleMeshNode;
			if (triangleMeshNode == null)
			{
				triangleMeshNode = ((graph as NavGraph).GetNearest(tmp_origin, NNConstraint.None).node as TriangleMeshNode);
				if (triangleMeshNode == null)
				{
					Debug.LogError("Could not find a valid node to start from");
					hit.point = tmp_origin;
					return true;
				}
			}
			if (int2 == @int)
			{
				hit.node = triangleMeshNode;
				return false;
			}
			int2 = (Int3)triangleMeshNode.ClosestPointOnNode((Vector3)int2);
			hit.origin = (Vector3)int2;
			if (!triangleMeshNode.Walkable)
			{
				hit.point = (Vector3)int2;
				hit.tangentOrigin = (Vector3)int2;
				return true;
			}
			List<Vector3> list = ListPool<Vector3>.Claim();
			List<Vector3> list2 = ListPool<Vector3>.Claim();
			int num = 0;
			for (;;)
			{
				num++;
				if (num > 2000)
				{
					break;
				}
				TriangleMeshNode triangleMeshNode2 = null;
				if (trace != null)
				{
					trace.Add(triangleMeshNode);
				}
				if (triangleMeshNode.ContainsPoint(@int))
				{
					goto Block_9;
				}
				for (int i = 0; i < triangleMeshNode.connections.Length; i++)
				{
					if (triangleMeshNode.connections[i].node.GraphIndex == triangleMeshNode.GraphIndex)
					{
						list.Clear();
						list2.Clear();
						if (triangleMeshNode.GetPortal(triangleMeshNode.connections[i].node, list, list2, false))
						{
							Vector3 vector = list[0];
							Vector3 vector2 = list2[0];
							float num2;
							float num3;
							if ((VectorMath.RightXZ(vector, vector2, hit.origin) || !VectorMath.RightXZ(vector, vector2, tmp_end)) && VectorMath.LineIntersectionFactorXZ(vector, vector2, hit.origin, tmp_end, out num2, out num3) && num3 >= 0f && num2 >= 0f && num2 <= 1f)
							{
								triangleMeshNode2 = (triangleMeshNode.connections[i].node as TriangleMeshNode);
								break;
							}
						}
					}
				}
				if (triangleMeshNode2 == null)
				{
					goto Block_17;
				}
				triangleMeshNode = triangleMeshNode2;
			}
			Debug.LogError("Linecast was stuck in infinite loop. Breaking.");
			ListPool<Vector3>.Release(list);
			ListPool<Vector3>.Release(list2);
			return true;
			Block_9:
			ListPool<Vector3>.Release(list);
			ListPool<Vector3>.Release(list2);
			return false;
			Block_17:
			int vertexCount = triangleMeshNode.GetVertexCount();
			for (int j = 0; j < vertexCount; j++)
			{
				Vector3 vector3 = (Vector3)triangleMeshNode.GetVertex(j);
				Vector3 vector4 = (Vector3)triangleMeshNode.GetVertex((j + 1) % vertexCount);
				float num4;
				float num5;
				if ((VectorMath.RightXZ(vector3, vector4, hit.origin) || !VectorMath.RightXZ(vector3, vector4, tmp_end)) && VectorMath.LineIntersectionFactorXZ(vector3, vector4, hit.origin, tmp_end, out num4, out num5) && num5 >= 0f && num4 >= 0f && num4 <= 1f)
				{
					Vector3 point = vector3 + (vector4 - vector3) * num4;
					hit.point = point;
					hit.node = triangleMeshNode;
					hit.tangent = vector4 - vector3;
					hit.tangentOrigin = vector3;
					ListPool<Vector3>.Release(list);
					ListPool<Vector3>.Release(list2);
					return true;
				}
			}
			Debug.LogWarning("Linecast failing because point not inside node, and line does not hit any edges of it");
			ListPool<Vector3>.Release(list);
			ListPool<Vector3>.Release(list2);
			return false;
		}

		public override void OnDrawGizmos(RetainedGizmos gizmos, bool drawNodes)
		{
			if (!drawNodes)
			{
				return;
			}
			using (GraphGizmoHelper singleFrameGizmoHelper = gizmos.GetSingleFrameGizmoHelper(this.active))
			{
				Bounds bounds = default(Bounds);
				bounds.SetMinMax(Vector3.zero, this.forcedBoundsSize);
				singleFrameGizmoHelper.builder.DrawWireCube(this.CalculateTransform(), bounds, Color.white);
			}
			if (this.tiles != null)
			{
				for (int i = 0; i < this.tiles.Length; i++)
				{
					if (this.tiles[i] != null)
					{
						RetainedGizmos.Hasher hasher = new RetainedGizmos.Hasher(this.active);
						hasher.AddHash(this.showMeshOutline ? 1 : 0);
						hasher.AddHash(this.showMeshSurface ? 1 : 0);
						hasher.AddHash(this.showNodeConnections ? 1 : 0);
						TriangleMeshNode[] nodes = this.tiles[i].nodes;
						for (int j = 0; j < nodes.Length; j++)
						{
							hasher.HashNode(nodes[j]);
						}
						if (!gizmos.Draw(hasher))
						{
							using (GraphGizmoHelper gizmoHelper = gizmos.GetGizmoHelper(this.active, hasher))
							{
								if (this.showMeshSurface || this.showMeshOutline)
								{
									this.CreateNavmeshSurfaceVisualization(this.tiles[i], gizmoHelper);
								}
								if (this.showMeshSurface || this.showMeshOutline)
								{
									NavmeshBase.CreateNavmeshOutlineVisualization(this.tiles[i], gizmoHelper);
								}
								if (this.showNodeConnections)
								{
									for (int k = 0; k < nodes.Length; k++)
									{
										gizmoHelper.DrawConnections(nodes[k]);
									}
								}
							}
						}
						gizmos.Draw(hasher);
					}
				}
			}
			if (this.active.showUnwalkableNodes)
			{
				base.DrawUnwalkableNodes(this.active.unwalkableNodeDebugSize);
			}
		}

		private void CreateNavmeshSurfaceVisualization(NavmeshTile tile, GraphGizmoHelper helper)
		{
			Vector3[] array = ArrayPool<Vector3>.Claim(tile.nodes.Length * 3);
			Color[] array2 = ArrayPool<Color>.Claim(tile.nodes.Length * 3);
			for (int i = 0; i < tile.nodes.Length; i++)
			{
				TriangleMeshNode triangleMeshNode = tile.nodes[i];
				Int3 ob;
				Int3 ob2;
				Int3 ob3;
				triangleMeshNode.GetVertices(out ob, out ob2, out ob3);
				array[i * 3] = (Vector3)ob;
				array[i * 3 + 1] = (Vector3)ob2;
				array[i * 3 + 2] = (Vector3)ob3;
				Color color = helper.NodeColor(triangleMeshNode);
				array2[i * 3] = (array2[i * 3 + 1] = (array2[i * 3 + 2] = color));
			}
			if (this.showMeshSurface)
			{
				helper.DrawTriangles(array, array2, tile.nodes.Length);
			}
			if (this.showMeshOutline)
			{
				helper.DrawWireTriangles(array, array2, tile.nodes.Length);
			}
			ArrayPool<Vector3>.Release(ref array, false);
			ArrayPool<Color>.Release(ref array2, false);
		}

		private static void CreateNavmeshOutlineVisualization(NavmeshTile tile, GraphGizmoHelper helper)
		{
			bool[] array = new bool[3];
			for (int i = 0; i < tile.nodes.Length; i++)
			{
				array[0] = (array[1] = (array[2] = false));
				TriangleMeshNode triangleMeshNode = tile.nodes[i];
				for (int j = 0; j < triangleMeshNode.connections.Length; j++)
				{
					TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j].node as TriangleMeshNode;
					if (triangleMeshNode2 != null && triangleMeshNode2.GraphIndex == triangleMeshNode.GraphIndex)
					{
						for (int k = 0; k < 3; k++)
						{
							for (int l = 0; l < 3; l++)
							{
								if (triangleMeshNode.GetVertexIndex(k) == triangleMeshNode2.GetVertexIndex((l + 1) % 3) && triangleMeshNode.GetVertexIndex((k + 1) % 3) == triangleMeshNode2.GetVertexIndex(l))
								{
									array[k] = true;
									k = 3;
									break;
								}
							}
						}
					}
				}
				Color color = helper.NodeColor(triangleMeshNode);
				for (int m = 0; m < 3; m++)
				{
					if (!array[m])
					{
						helper.builder.DrawLine((Vector3)triangleMeshNode.GetVertex(m), (Vector3)triangleMeshNode.GetVertex((m + 1) % 3), color);
					}
				}
			}
		}

		public override void SerializeExtraInfo(GraphSerializationContext ctx)
		{
			BinaryWriter writer = ctx.writer;
			if (this.tiles == null)
			{
				writer.Write(-1);
				return;
			}
			writer.Write(this.tileXCount);
			writer.Write(this.tileZCount);
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					NavmeshTile navmeshTile = this.tiles[j + i * this.tileXCount];
					if (navmeshTile == null)
					{
						throw new Exception("NULL Tile");
					}
					writer.Write(navmeshTile.x);
					writer.Write(navmeshTile.z);
					if (navmeshTile.x == j && navmeshTile.z == i)
					{
						writer.Write(navmeshTile.w);
						writer.Write(navmeshTile.d);
						writer.Write(navmeshTile.tris.Length);
						for (int k = 0; k < navmeshTile.tris.Length; k++)
						{
							writer.Write(navmeshTile.tris[k]);
						}
						writer.Write(navmeshTile.verts.Length);
						for (int l = 0; l < navmeshTile.verts.Length; l++)
						{
							ctx.SerializeInt3(navmeshTile.verts[l]);
						}
						writer.Write(navmeshTile.vertsInGraphSpace.Length);
						for (int m = 0; m < navmeshTile.vertsInGraphSpace.Length; m++)
						{
							ctx.SerializeInt3(navmeshTile.vertsInGraphSpace[m]);
						}
						writer.Write(navmeshTile.nodes.Length);
						for (int n = 0; n < navmeshTile.nodes.Length; n++)
						{
							navmeshTile.nodes[n].SerializeNode(ctx);
						}
					}
				}
			}
		}

		public override void DeserializeExtraInfo(GraphSerializationContext ctx)
		{
			BinaryReader reader = ctx.reader;
			this.tileXCount = reader.ReadInt32();
			if (this.tileXCount < 0)
			{
				return;
			}
			this.tileZCount = reader.ReadInt32();
			this.transform = this.CalculateTransform();
			this.tiles = new NavmeshTile[this.tileXCount * this.tileZCount];
			TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					int num = j + i * this.tileXCount;
					int num2 = reader.ReadInt32();
					if (num2 < 0)
					{
						throw new Exception("Invalid tile coordinates (x < 0)");
					}
					int num3 = reader.ReadInt32();
					if (num3 < 0)
					{
						throw new Exception("Invalid tile coordinates (z < 0)");
					}
					if (num2 != j || num3 != i)
					{
						this.tiles[num] = this.tiles[num3 * this.tileXCount + num2];
					}
					else
					{
						NavmeshTile navmeshTile = new NavmeshTile();
						navmeshTile.x = num2;
						navmeshTile.z = num3;
						navmeshTile.w = reader.ReadInt32();
						navmeshTile.d = reader.ReadInt32();
						navmeshTile.bbTree = ObjectPool<BBTree>.Claim();
						this.tiles[num] = navmeshTile;
						int num4 = reader.ReadInt32();
						if (num4 % 3 != 0)
						{
							throw new Exception("Corrupt data. Triangle indices count must be divisable by 3. Got " + num4);
						}
						navmeshTile.tris = new int[num4];
						for (int k = 0; k < navmeshTile.tris.Length; k++)
						{
							navmeshTile.tris[k] = reader.ReadInt32();
						}
						navmeshTile.verts = new Int3[reader.ReadInt32()];
						for (int l = 0; l < navmeshTile.verts.Length; l++)
						{
							navmeshTile.verts[l] = ctx.DeserializeInt3();
						}
						if (ctx.meta.version.Major >= 4)
						{
							navmeshTile.vertsInGraphSpace = new Int3[reader.ReadInt32()];
							if (navmeshTile.vertsInGraphSpace.Length != navmeshTile.verts.Length)
							{
								throw new Exception("Corrupt data. Array lengths did not match");
							}
							for (int m = 0; m < navmeshTile.verts.Length; m++)
							{
								navmeshTile.vertsInGraphSpace[m] = ctx.DeserializeInt3();
							}
						}
						else
						{
							navmeshTile.vertsInGraphSpace = new Int3[navmeshTile.verts.Length];
							navmeshTile.verts.CopyTo(navmeshTile.vertsInGraphSpace, 0);
							this.transform.InverseTransform(navmeshTile.vertsInGraphSpace);
						}
						int num5 = reader.ReadInt32();
						navmeshTile.nodes = new TriangleMeshNode[num5];
						num <<= 12;
						for (int n = 0; n < navmeshTile.nodes.Length; n++)
						{
							TriangleMeshNode triangleMeshNode = new TriangleMeshNode(this.active);
							navmeshTile.nodes[n] = triangleMeshNode;
							triangleMeshNode.DeserializeNode(ctx);
							triangleMeshNode.v0 = (navmeshTile.tris[n * 3] | num);
							triangleMeshNode.v1 = (navmeshTile.tris[n * 3 + 1] | num);
							triangleMeshNode.v2 = (navmeshTile.tris[n * 3 + 2] | num);
							triangleMeshNode.UpdatePositionFromVertices();
						}
						navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
					}
				}
			}
		}

		public override void PostDeserialization()
		{
			this.transform = this.CalculateTransform();
		}

		public const int VertexIndexMask = 4095;

		public const int TileIndexMask = 524287;

		public const int TileIndexOffset = 12;

		[JsonMember]
		public Vector3 forcedBoundsSize = new Vector3(100f, 40f, 100f);

		[JsonMember]
		public bool showMeshOutline = true;

		[JsonMember]
		public bool showNodeConnections;

		[JsonMember]
		public bool showMeshSurface;

		public int tileXCount;

		public int tileZCount;

		protected NavmeshTile[] tiles;

		[JsonMember]
		public bool nearestSearchOnlyXZ;

		private bool batchTileUpdate;

		private List<int> batchUpdatedTiles = new List<int>();

		public GraphTransform transform = new GraphTransform(Matrix4x4.identity);

		public Action<NavmeshTile[]> OnRecalculatedTiles;

		private static readonly NNConstraint NNConstraintDistanceXZ = new NNConstraint
		{
			distanceXZ = true
		};
	}
}
