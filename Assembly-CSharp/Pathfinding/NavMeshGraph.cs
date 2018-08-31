using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	[JsonOptIn]
	public class NavMeshGraph : NavmeshBase, IUpdatableGraph
	{
		public override float TileWorldSizeX
		{
			get
			{
				return this.forcedBoundsSize.x;
			}
		}

		public override float TileWorldSizeZ
		{
			get
			{
				return this.forcedBoundsSize.z;
			}
		}

		protected override float MaxTileConnectionEdgeDistance
		{
			get
			{
				return 0f;
			}
		}

		public override GraphTransform CalculateTransform()
		{
			return new GraphTransform(Matrix4x4.TRS(this.offset, Quaternion.Euler(this.rotation), Vector3.one) * Matrix4x4.TRS((!(this.sourceMesh != null)) ? Vector3.zero : (this.sourceMesh.bounds.min * this.scale), Quaternion.identity, Vector3.one));
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

		public void UpdateArea(GraphUpdateObject o)
		{
			NavMeshGraph.UpdateArea(o, this);
		}

		public static void UpdateArea(GraphUpdateObject o, INavmesh graph)
		{
			Bounds bounds = o.bounds;
			Rect rect = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
			IntRect irect = new IntRect(Mathf.FloorToInt(bounds.min.x * 1000f), Mathf.FloorToInt(bounds.min.z * 1000f), Mathf.FloorToInt(bounds.max.x * 1000f), Mathf.FloorToInt(bounds.max.z * 1000f));
			Int3 a = new Int3(irect.xmin, 0, irect.ymin);
			Int3 b = new Int3(irect.xmin, 0, irect.ymax);
			Int3 c = new Int3(irect.xmax, 0, irect.ymin);
			Int3 d = new Int3(irect.xmax, 0, irect.ymax);
			int ymin = ((Int3)bounds.min).y;
			int ymax = ((Int3)bounds.max).y;
			graph.GetNodes(delegate(GraphNode _node)
			{
				TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
				bool flag = false;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				for (int i = 0; i < 3; i++)
				{
					Int3 vertex = triangleMeshNode.GetVertex(i);
					Vector3 vector = (Vector3)vertex;
					if (irect.Contains(vertex.x, vertex.z))
					{
						flag = true;
						break;
					}
					if (vector.x < rect.xMin)
					{
						num++;
					}
					if (vector.x > rect.xMax)
					{
						num2++;
					}
					if (vector.z < rect.yMin)
					{
						num3++;
					}
					if (vector.z > rect.yMax)
					{
						num4++;
					}
				}
				if (!flag && (num == 3 || num2 == 3 || num3 == 3 || num4 == 3))
				{
					return;
				}
				for (int j = 0; j < 3; j++)
				{
					int i2 = (j <= 1) ? (j + 1) : 0;
					Int3 vertex2 = triangleMeshNode.GetVertex(j);
					Int3 vertex3 = triangleMeshNode.GetVertex(i2);
					if (VectorMath.SegmentsIntersectXZ(a, b, vertex2, vertex3))
					{
						flag = true;
						break;
					}
					if (VectorMath.SegmentsIntersectXZ(a, c, vertex2, vertex3))
					{
						flag = true;
						break;
					}
					if (VectorMath.SegmentsIntersectXZ(c, d, vertex2, vertex3))
					{
						flag = true;
						break;
					}
					if (VectorMath.SegmentsIntersectXZ(d, b, vertex2, vertex3))
					{
						flag = true;
						break;
					}
				}
				if (flag || triangleMeshNode.ContainsPoint(a) || triangleMeshNode.ContainsPoint(b) || triangleMeshNode.ContainsPoint(c) || triangleMeshNode.ContainsPoint(d))
				{
					flag = true;
				}
				if (!flag)
				{
					return;
				}
				int num5 = 0;
				int num6 = 0;
				for (int k = 0; k < 3; k++)
				{
					Int3 vertex4 = triangleMeshNode.GetVertex(k);
					if (vertex4.y < ymin)
					{
						num6++;
					}
					if (vertex4.y > ymax)
					{
						num5++;
					}
				}
				if (num6 == 3 || num5 == 3)
				{
					return;
				}
				o.WillUpdateNode(triangleMeshNode);
				o.Apply(triangleMeshNode);
			});
		}

		[Obsolete("Set the mesh to ObjImporter.ImportFile(...) and scan the graph the normal way instead")]
		public void ScanInternal(string objMeshPath)
		{
			Mesh x = ObjImporter.ImportFile(objMeshPath);
			if (x == null)
			{
				Debug.LogError("Couldn't read .obj file at '" + objMeshPath + "'");
				return;
			}
			this.sourceMesh = x;
			IEnumerator<Progress> enumerator = this.ScanInternal().GetEnumerator();
			while (enumerator.MoveNext())
			{
			}
		}

		public override IEnumerable<Progress> ScanInternal()
		{
			this.transform = this.CalculateTransform();
			this.tileZCount = (this.tileXCount = 1);
			this.tiles = new NavmeshTile[this.tileZCount * this.tileXCount];
			TriangleMeshNode.SetNavmeshHolder(AstarPath.active.data.GetGraphIndex(this), this);
			if (this.sourceMesh == null)
			{
				base.FillWithEmptyTiles();
				yield break;
			}
			yield return new Progress(0f, "Transforming Vertices");
			this.forcedBoundsSize = this.sourceMesh.bounds.size * this.scale;
			Vector3[] vectorVertices = this.sourceMesh.vertices;
			List<Int3> intVertices = ListPool<Int3>.Claim(vectorVertices.Length);
			Matrix4x4 matrix = Matrix4x4.TRS(-this.sourceMesh.bounds.min * this.scale, Quaternion.identity, Vector3.one * this.scale);
			for (int i = 0; i < vectorVertices.Length; i++)
			{
				intVertices.Add((Int3)matrix.MultiplyPoint3x4(vectorVertices[i]));
			}
			yield return new Progress(0.1f, "Compressing Vertices");
			Int3[] compressedVertices = null;
			int[] compressedTriangles = null;
			Polygon.CompressMesh(intVertices, new List<int>(this.sourceMesh.triangles), out compressedVertices, out compressedTriangles);
			ListPool<Int3>.Release(intVertices);
			yield return new Progress(0.2f, "Building Nodes");
			base.ReplaceTile(0, 0, compressedVertices, compressedTriangles);
			if (this.OnRecalculatedTiles != null)
			{
				this.OnRecalculatedTiles(this.tiles.Clone() as NavmeshTile[]);
			}
			yield break;
		}

		public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			base.DeserializeSettingsCompatibility(ctx);
			this.sourceMesh = (ctx.DeserializeUnityObject() as Mesh);
			this.offset = ctx.DeserializeVector3();
			this.rotation = ctx.DeserializeVector3();
			this.scale = ctx.reader.ReadSingle();
			this.nearestSearchOnlyXZ = !ctx.reader.ReadBoolean();
		}

		[JsonMember]
		public Mesh sourceMesh;

		[JsonMember]
		public Vector3 offset;

		[JsonMember]
		public Vector3 rotation;

		[JsonMember]
		public float scale = 1f;
	}
}
