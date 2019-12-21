using System;
using System.Collections.Generic;
using Pathfinding.Recast;
using Pathfinding.Serialization;
using Pathfinding.Util;
using Pathfinding.Voxels;
using UnityEngine;

namespace Pathfinding
{
	[JsonOptIn]
	public class RecastGraph : NavmeshBase, IUpdatableGraph
	{
		public override float TileWorldSizeX
		{
			get
			{
				return (float)this.tileSizeX * this.cellSize;
			}
		}

		public override float TileWorldSizeZ
		{
			get
			{
				return (float)this.tileSizeZ * this.cellSize;
			}
		}

		protected override float MaxTileConnectionEdgeDistance
		{
			get
			{
				return this.walkableClimb;
			}
		}

		[Obsolete("Obsolete since this is not accurate when the graph is rotated (rotation was not supported when this property was created)")]
		public Bounds forcedBounds
		{
			get
			{
				return new Bounds(this.forcedBoundsCenter, this.forcedBoundsSize);
			}
		}

		[Obsolete("Use node.ClosestPointOnNode instead")]
		public Vector3 ClosestPointOnNode(TriangleMeshNode node, Vector3 pos)
		{
			return node.ClosestPointOnNode(pos);
		}

		[Obsolete("Use node.ContainsPoint instead")]
		public bool ContainsPoint(TriangleMeshNode node, Vector3 pos)
		{
			return node.ContainsPoint((Int3)pos);
		}

		public void SnapForceBoundsToScene()
		{
			List<RasterizationMesh> list = this.CollectMeshes(new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)));
			if (list.Count == 0)
			{
				return;
			}
			Bounds bounds = list[0].bounds;
			for (int i = 1; i < list.Count; i++)
			{
				bounds.Encapsulate(list[i].bounds);
				list[i].Pool();
			}
			this.forcedBoundsCenter = bounds.center;
			this.forcedBoundsSize = bounds.size;
		}

		GraphUpdateThreading IUpdatableGraph.CanUpdateAsync(GraphUpdateObject o)
		{
			if (!o.updatePhysics)
			{
				return GraphUpdateThreading.SeparateThread;
			}
			return (GraphUpdateThreading)7;
		}

		void IUpdatableGraph.UpdateAreaInit(GraphUpdateObject o)
		{
			if (!o.updatePhysics)
			{
				return;
			}
			RelevantGraphSurface.UpdateAllPositions();
			IntRect touchingTiles = base.GetTouchingTiles(o.bounds);
			Bounds tileBounds = base.GetTileBounds(touchingTiles);
			tileBounds.Expand(new Vector3(1f, 0f, 1f) * this.TileBorderSizeInWorldUnits * 2f);
			List<RasterizationMesh> inputMeshes = this.CollectMeshes(tileBounds);
			if (this.globalVox == null)
			{
				this.globalVox = new Voxelize(this.CellHeight, this.cellSize, this.walkableClimb, this.walkableHeight, this.maxSlope, this.maxEdgeLength);
			}
			this.globalVox.inputMeshes = inputMeshes;
		}

		void IUpdatableGraph.UpdateArea(GraphUpdateObject guo)
		{
			IntRect touchingTiles = base.GetTouchingTiles(guo.bounds);
			if (!guo.updatePhysics)
			{
				for (int i = touchingTiles.ymin; i <= touchingTiles.ymax; i++)
				{
					for (int j = touchingTiles.xmin; j <= touchingTiles.xmax; j++)
					{
						NavmeshTile graph = this.tiles[i * this.tileXCount + j];
						NavMeshGraph.UpdateArea(guo, graph);
					}
				}
				return;
			}
			Voxelize voxelize = this.globalVox;
			if (voxelize == null)
			{
				throw new InvalidOperationException("No Voxelizer object. UpdateAreaInit should have been called before this function.");
			}
			for (int k = touchingTiles.xmin; k <= touchingTiles.xmax; k++)
			{
				for (int l = touchingTiles.ymin; l <= touchingTiles.ymax; l++)
				{
					this.stagingTiles.Add(this.BuildTileMesh(voxelize, k, l, 0));
				}
			}
			uint graphIndex = (uint)AstarPath.active.data.GetGraphIndex(this);
			for (int m = 0; m < this.stagingTiles.Count; m++)
			{
				GraphNode[] nodes = this.stagingTiles[m].nodes;
				GraphNode[] array = nodes;
				for (int n = 0; n < array.Length; n++)
				{
					array[n].GraphIndex = graphIndex;
				}
			}
			for (int num = 0; num < voxelize.inputMeshes.Count; num++)
			{
				voxelize.inputMeshes[num].Pool();
			}
			ListPool<RasterizationMesh>.Release(voxelize.inputMeshes);
			voxelize.inputMeshes = null;
		}

		void IUpdatableGraph.UpdateAreaPost(GraphUpdateObject guo)
		{
			for (int i = 0; i < this.stagingTiles.Count; i++)
			{
				NavmeshTile navmeshTile = this.stagingTiles[i];
				int num = navmeshTile.x + navmeshTile.z * this.tileXCount;
				NavmeshTile navmeshTile2 = this.tiles[num];
				for (int j = 0; j < navmeshTile2.nodes.Length; j++)
				{
					navmeshTile2.nodes[j].Destroy();
				}
				this.tiles[num] = navmeshTile;
			}
			for (int k = 0; k < this.stagingTiles.Count; k++)
			{
				NavmeshTile tile = this.stagingTiles[k];
				base.ConnectTileWithNeighbours(tile, false);
			}
			if (this.OnRecalculatedTiles != null)
			{
				this.OnRecalculatedTiles(this.stagingTiles.ToArray());
			}
			this.stagingTiles.Clear();
		}

		public override IEnumerable<Progress> ScanInternal()
		{
			TriangleMeshNode.SetNavmeshHolder(AstarPath.active.data.GetGraphIndex(this), this);
			if (!Application.isPlaying)
			{
				RelevantGraphSurface.FindAllGraphSurfaces();
			}
			RelevantGraphSurface.UpdateAllPositions();
			foreach (Progress progress in this.ScanAllTiles())
			{
				yield return progress;
			}
			IEnumerator<Progress> enumerator = null;
			yield break;
			yield break;
			yield break;
		}

		public override GraphTransform CalculateTransform()
		{
			return new GraphTransform(Matrix4x4.TRS(this.forcedBoundsCenter, Quaternion.Euler(this.rotation), Vector3.one) * Matrix4x4.TRS(-this.forcedBoundsSize * 0.5f, Quaternion.identity, Vector3.one));
		}

		private void InitializeTileInfo()
		{
			int num = (int)(this.forcedBoundsSize.x / this.cellSize + 0.5f);
			int num2 = (int)(this.forcedBoundsSize.z / this.cellSize + 0.5f);
			if (!this.useTiles)
			{
				this.tileSizeX = num;
				this.tileSizeZ = num2;
			}
			else
			{
				this.tileSizeX = this.editorTileSize;
				this.tileSizeZ = this.editorTileSize;
			}
			this.tileXCount = (num + this.tileSizeX - 1) / this.tileSizeX;
			this.tileZCount = (num2 + this.tileSizeZ - 1) / this.tileSizeZ;
			if (this.tileXCount * this.tileZCount > 524288)
			{
				throw new Exception(string.Concat(new object[]
				{
					"Too many tiles (",
					this.tileXCount * this.tileZCount,
					") maximum is ",
					524288,
					"\nTry disabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* inspector."
				}));
			}
			this.tiles = new NavmeshTile[this.tileXCount * this.tileZCount];
		}

		private List<RasterizationMesh>[] PutMeshesIntoTileBuckets(List<RasterizationMesh> meshes)
		{
			List<RasterizationMesh>[] array = new List<RasterizationMesh>[this.tiles.Length];
			Vector3 amount = new Vector3(1f, 0f, 1f) * this.TileBorderSizeInWorldUnits * 2f;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ListPool<RasterizationMesh>.Claim();
			}
			for (int j = 0; j < meshes.Count; j++)
			{
				RasterizationMesh rasterizationMesh = meshes[j];
				Bounds bounds = rasterizationMesh.bounds;
				bounds.Expand(amount);
				IntRect touchingTiles = base.GetTouchingTiles(bounds);
				for (int k = touchingTiles.ymin; k <= touchingTiles.ymax; k++)
				{
					for (int l = touchingTiles.xmin; l <= touchingTiles.xmax; l++)
					{
						array[l + k * this.tileXCount].Add(rasterizationMesh);
					}
				}
			}
			return array;
		}

		protected IEnumerable<Progress> ScanAllTiles()
		{
			RecastGraph.<>c__DisplayClass48_0 <>c__DisplayClass48_ = new RecastGraph.<>c__DisplayClass48_0();
			<>c__DisplayClass48_.<>4__this = this;
			this.transform = this.CalculateTransform();
			this.InitializeTileInfo();
			if (this.scanEmptyGraph)
			{
				base.FillWithEmptyTiles();
				yield break;
			}
			this.walkableClimb = Mathf.Min(this.walkableClimb, this.walkableHeight);
			yield return new Progress(0f, "Finding Meshes");
			Bounds bounds = this.transform.Transform(new Bounds(this.forcedBoundsSize * 0.5f, this.forcedBoundsSize));
			List<RasterizationMesh> meshes = this.CollectMeshes(bounds);
			<>c__DisplayClass48_.buckets = this.PutMeshesIntoTileBuckets(meshes);
			Queue<Int2> tileQueue = new Queue<Int2>();
			for (int i = 0; i < this.tileZCount; i++)
			{
				for (int j = 0; j < this.tileXCount; j++)
				{
					tileQueue.Enqueue(new Int2(j, i));
				}
			}
			ParallelWorkQueue<Int2> parallelWorkQueue = new ParallelWorkQueue<Int2>(tileQueue);
			<>c__DisplayClass48_.voxelizers = new Voxelize[parallelWorkQueue.threadCount];
			for (int k = 0; k < <>c__DisplayClass48_.voxelizers.Length; k++)
			{
				<>c__DisplayClass48_.voxelizers[k] = new Voxelize(this.CellHeight, this.cellSize, this.walkableClimb, this.walkableHeight, this.maxSlope, this.maxEdgeLength);
			}
			parallelWorkQueue.action = delegate(Int2 tile, int threadIndex)
			{
				<>c__DisplayClass48_.voxelizers[threadIndex].inputMeshes = <>c__DisplayClass48_.buckets[tile.x + tile.y * <>c__DisplayClass48_.<>4__this.tileXCount];
				<>c__DisplayClass48_.<>4__this.tiles[tile.x + tile.y * <>c__DisplayClass48_.<>4__this.tileXCount] = <>c__DisplayClass48_.<>4__this.BuildTileMesh(<>c__DisplayClass48_.voxelizers[threadIndex], tile.x, tile.y, threadIndex);
			};
			int timeoutMillis = Application.isPlaying ? 1 : 200;
			foreach (int num in parallelWorkQueue.Run(timeoutMillis))
			{
				yield return new Progress(Mathf.Lerp(0.1f, 0.9f, (float)num / (float)this.tiles.Length), string.Concat(new object[]
				{
					"Calculated Tiles: ",
					num,
					"/",
					this.tiles.Length
				}));
			}
			IEnumerator<int> enumerator = null;
			yield return new Progress(0.9f, "Assigning Graph Indices");
			<>c__DisplayClass48_.graphIndex = (uint)AstarPath.active.data.GetGraphIndex(this);
			this.GetNodes(delegate(GraphNode node)
			{
				node.GraphIndex = <>c__DisplayClass48_.graphIndex;
			});
			int num3;
			for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum = num3 + 1)
			{
				RecastGraph.<>c__DisplayClass48_1 <>c__DisplayClass48_2 = new RecastGraph.<>c__DisplayClass48_1();
				<>c__DisplayClass48_2.CS$<>8__locals1 = <>c__DisplayClass48_;
				<>c__DisplayClass48_2.direction = 0;
				while (<>c__DisplayClass48_2.direction <= 1)
				{
					for (int l = 0; l < this.tiles.Length; l++)
					{
						if ((this.tiles[l].x + this.tiles[l].z) % 2 == coordinateSum)
						{
							tileQueue.Enqueue(new Int2(this.tiles[l].x, this.tiles[l].z));
						}
					}
					parallelWorkQueue = new ParallelWorkQueue<Int2>(tileQueue);
					parallelWorkQueue.action = delegate(Int2 tile, int threadIndex)
					{
						if (<>c__DisplayClass48_2.direction == 0 && tile.x < <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tileXCount - 1)
						{
							<>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.ConnectTiles(<>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tiles[tile.x + tile.y * <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tileXCount], <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tiles[tile.x + 1 + tile.y * <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tileXCount]);
						}
						if (<>c__DisplayClass48_2.direction == 1 && tile.y < <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tileZCount - 1)
						{
							<>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.ConnectTiles(<>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tiles[tile.x + tile.y * <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tileXCount], <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tiles[tile.x + (tile.y + 1) * <>c__DisplayClass48_2.CS$<>8__locals1.<>4__this.tileXCount]);
						}
					};
					int numTilesInQueue = tileQueue.Count;
					foreach (int num2 in parallelWorkQueue.Run(timeoutMillis))
					{
						yield return new Progress(0.95f, string.Concat(new object[]
						{
							"Connected Tiles ",
							numTilesInQueue - num2,
							"/",
							numTilesInQueue,
							" (Phase ",
							<>c__DisplayClass48_2.direction + 1 + 2 * coordinateSum,
							" of 4)"
						}));
					}
					enumerator = null;
					num3 = <>c__DisplayClass48_2.direction;
					<>c__DisplayClass48_2.direction = num3 + 1;
				}
				<>c__DisplayClass48_2 = null;
				num3 = coordinateSum;
			}
			for (int m = 0; m < meshes.Count; m++)
			{
				meshes[m].Pool();
			}
			ListPool<RasterizationMesh>.Release(meshes);
			if (this.OnRecalculatedTiles != null)
			{
				this.OnRecalculatedTiles(this.tiles.Clone() as NavmeshTile[]);
			}
			yield break;
			yield break;
			yield break;
		}

		private List<RasterizationMesh> CollectMeshes(Bounds bounds)
		{
			List<RasterizationMesh> list = ListPool<RasterizationMesh>.Claim();
			RecastMeshGatherer recastMeshGatherer = new RecastMeshGatherer(bounds, this.terrainSampleSize, this.mask, this.tagMask, this.colliderRasterizeDetail);
			if (this.rasterizeMeshes)
			{
				recastMeshGatherer.CollectSceneMeshes(list);
			}
			recastMeshGatherer.CollectRecastMeshObjs(list);
			if (this.rasterizeTerrain)
			{
				float desiredChunkSize = this.cellSize * (float)Math.Max(this.tileSizeX, this.tileSizeZ);
				recastMeshGatherer.CollectTerrainMeshes(this.rasterizeTrees, desiredChunkSize, list);
			}
			if (this.rasterizeColliders)
			{
				recastMeshGatherer.CollectColliderMeshes(list);
			}
			if (list.Count == 0)
			{
				Debug.LogWarning("No MeshFilters were found contained in the layers specified by the 'mask' variables");
			}
			return list;
		}

		private float CellHeight
		{
			get
			{
				return Mathf.Max(this.forcedBoundsSize.y / 64000f, 0.001f);
			}
		}

		private int CharacterRadiusInVoxels
		{
			get
			{
				return Mathf.CeilToInt(this.characterRadius / this.cellSize - 0.1f);
			}
		}

		private int TileBorderSizeInVoxels
		{
			get
			{
				return this.CharacterRadiusInVoxels + 3;
			}
		}

		private float TileBorderSizeInWorldUnits
		{
			get
			{
				return (float)this.TileBorderSizeInVoxels * this.cellSize;
			}
		}

		private Bounds CalculateTileBoundsWithBorder(int x, int z)
		{
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3((float)x * this.TileWorldSizeX, 0f, (float)z * this.TileWorldSizeZ), new Vector3((float)(x + 1) * this.TileWorldSizeX, this.forcedBoundsSize.y, (float)(z + 1) * this.TileWorldSizeZ));
			result.Expand(new Vector3(1f, 0f, 1f) * this.TileBorderSizeInWorldUnits * 2f);
			return result;
		}

		protected NavmeshTile BuildTileMesh(Voxelize vox, int x, int z, int threadIndex = 0)
		{
			vox.borderSize = this.TileBorderSizeInVoxels;
			vox.forcedBounds = this.CalculateTileBoundsWithBorder(x, z);
			vox.width = this.tileSizeX + vox.borderSize * 2;
			vox.depth = this.tileSizeZ + vox.borderSize * 2;
			if (!this.useTiles && this.relevantGraphSurfaceMode == RecastGraph.RelevantGraphSurfaceMode.OnlyForCompletelyInsideTile)
			{
				vox.relevantGraphSurfaceMode = RecastGraph.RelevantGraphSurfaceMode.RequireForAll;
			}
			else
			{
				vox.relevantGraphSurfaceMode = this.relevantGraphSurfaceMode;
			}
			vox.minRegionSize = Mathf.RoundToInt(this.minRegionSize / (this.cellSize * this.cellSize));
			vox.Init();
			vox.VoxelizeInput(this.transform, this.CalculateTileBoundsWithBorder(x, z));
			vox.FilterLedges(vox.voxelWalkableHeight, vox.voxelWalkableClimb, vox.cellSize, vox.cellHeight);
			vox.FilterLowHeightSpans(vox.voxelWalkableHeight, vox.cellSize, vox.cellHeight);
			vox.BuildCompactField();
			vox.BuildVoxelConnections();
			vox.ErodeWalkableArea(this.CharacterRadiusInVoxels);
			vox.BuildDistanceField();
			vox.BuildRegions();
			VoxelContourSet cset = new VoxelContourSet();
			vox.BuildContours(this.contourMaxError, 1, cset, 5);
			VoxelMesh voxelMesh;
			vox.BuildPolyMesh(cset, 3, out voxelMesh);
			for (int i = 0; i < voxelMesh.verts.Length; i++)
			{
				voxelMesh.verts[i] *= 1000;
			}
			vox.transformVoxel2Graph.Transform(voxelMesh.verts);
			return this.CreateTile(vox, voxelMesh, x, z, threadIndex);
		}

		private NavmeshTile CreateTile(Voxelize vox, VoxelMesh mesh, int x, int z, int threadIndex)
		{
			if (mesh.tris == null)
			{
				throw new ArgumentNullException("mesh.tris");
			}
			if (mesh.verts == null)
			{
				throw new ArgumentNullException("mesh.verts");
			}
			if (mesh.tris.Length % 3 != 0)
			{
				throw new ArgumentException("Indices array's length must be a multiple of 3 (mesh.tris)");
			}
			if (mesh.verts.Length >= 4095)
			{
				if (this.tileXCount * this.tileZCount == 1)
				{
					throw new ArgumentException("Too many vertices per tile (more than " + 4095 + ").\n<b>Try enabling tiling in the recast graph settings.</b>\n");
				}
				throw new ArgumentException("Too many vertices per tile (more than " + 4095 + ").\n<b>Try reducing tile size or enabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector</b>");
			}
			else
			{
				NavmeshTile navmeshTile = new NavmeshTile
				{
					x = x,
					z = z,
					w = 1,
					d = 1,
					tris = mesh.tris,
					bbTree = new BBTree()
				};
				navmeshTile.vertsInGraphSpace = Utility.RemoveDuplicateVertices(mesh.verts, navmeshTile.tris);
				navmeshTile.verts = (Int3[])navmeshTile.vertsInGraphSpace.Clone();
				this.transform.Transform(navmeshTile.verts);
				uint num = (uint)(this.active.data.graphs.Length + threadIndex);
				if (num > 255u)
				{
					throw new Exception("Graph limit reached. Multithreaded recast calculations cannot be done because a few scratch graph indices are required.");
				}
				int num2 = x + z * this.tileXCount;
				num2 <<= 12;
				TriangleMeshNode.SetNavmeshHolder((int)num, navmeshTile);
				AstarPath active = this.active;
				lock (active)
				{
					navmeshTile.nodes = base.CreateNodes(navmeshTile.tris, num2, num);
				}
				navmeshTile.bbTree.RebuildFrom(navmeshTile.nodes);
				NavmeshBase.CreateNodeConnections(navmeshTile.nodes);
				TriangleMeshNode.SetNavmeshHolder((int)num, null);
				return navmeshTile;
			}
		}

		public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			base.DeserializeSettingsCompatibility(ctx);
			this.characterRadius = ctx.reader.ReadSingle();
			this.contourMaxError = ctx.reader.ReadSingle();
			this.cellSize = ctx.reader.ReadSingle();
			ctx.reader.ReadSingle();
			this.walkableHeight = ctx.reader.ReadSingle();
			this.maxSlope = ctx.reader.ReadSingle();
			this.maxEdgeLength = ctx.reader.ReadSingle();
			this.editorTileSize = ctx.reader.ReadInt32();
			this.tileSizeX = ctx.reader.ReadInt32();
			this.nearestSearchOnlyXZ = ctx.reader.ReadBoolean();
			this.useTiles = ctx.reader.ReadBoolean();
			this.relevantGraphSurfaceMode = (RecastGraph.RelevantGraphSurfaceMode)ctx.reader.ReadInt32();
			this.rasterizeColliders = ctx.reader.ReadBoolean();
			this.rasterizeMeshes = ctx.reader.ReadBoolean();
			this.rasterizeTerrain = ctx.reader.ReadBoolean();
			this.rasterizeTrees = ctx.reader.ReadBoolean();
			this.colliderRasterizeDetail = ctx.reader.ReadSingle();
			this.forcedBoundsCenter = ctx.DeserializeVector3();
			this.forcedBoundsSize = ctx.DeserializeVector3();
			this.mask = ctx.reader.ReadInt32();
			int num = ctx.reader.ReadInt32();
			this.tagMask = new List<string>(num);
			for (int i = 0; i < num; i++)
			{
				this.tagMask.Add(ctx.reader.ReadString());
			}
			this.showMeshOutline = ctx.reader.ReadBoolean();
			this.showNodeConnections = ctx.reader.ReadBoolean();
			this.terrainSampleSize = ctx.reader.ReadInt32();
			this.walkableClimb = ctx.DeserializeFloat(this.walkableClimb);
			this.minRegionSize = ctx.DeserializeFloat(this.minRegionSize);
			this.tileSizeZ = ctx.DeserializeInt(this.tileSizeX);
			this.showMeshSurface = ctx.reader.ReadBoolean();
		}

		[JsonMember]
		public float characterRadius = 1.5f;

		[JsonMember]
		public float contourMaxError = 2f;

		[JsonMember]
		public float cellSize = 0.5f;

		[JsonMember]
		public float walkableHeight = 2f;

		[JsonMember]
		public float walkableClimb = 0.5f;

		[JsonMember]
		public float maxSlope = 30f;

		[JsonMember]
		public float maxEdgeLength = 20f;

		[JsonMember]
		public float minRegionSize = 3f;

		[JsonMember]
		public int editorTileSize = 128;

		[JsonMember]
		public int tileSizeX = 128;

		[JsonMember]
		public int tileSizeZ = 128;

		[JsonMember]
		public bool useTiles;

		public bool scanEmptyGraph;

		[JsonMember]
		public RecastGraph.RelevantGraphSurfaceMode relevantGraphSurfaceMode;

		[JsonMember]
		public bool rasterizeColliders;

		[JsonMember]
		public bool rasterizeMeshes = true;

		[JsonMember]
		public bool rasterizeTerrain = true;

		[JsonMember]
		public bool rasterizeTrees = true;

		[JsonMember]
		public float colliderRasterizeDetail = 10f;

		[JsonMember]
		public LayerMask mask = -1;

		[JsonMember]
		public List<string> tagMask = new List<string>();

		[JsonMember]
		public int terrainSampleSize = 3;

		[JsonMember]
		public Vector3 rotation;

		[JsonMember]
		public Vector3 forcedBoundsCenter;

		private Voxelize globalVox;

		public const int BorderVertexMask = 1;

		public const int BorderVertexOffset = 31;

		private List<NavmeshTile> stagingTiles = new List<NavmeshTile>();

		public enum RelevantGraphSurfaceMode
		{
			DoNotRequire,
			OnlyForCompletelyInsideTile,
			RequireForAll
		}
	}
}
