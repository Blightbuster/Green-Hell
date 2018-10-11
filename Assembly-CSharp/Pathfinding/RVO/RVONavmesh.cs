using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.RVO
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_navmesh.php")]
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Navmesh")]
	public class RVONavmesh : GraphModifier
	{
		public override void OnPostCacheLoad()
		{
			this.OnLatePostScan();
		}

		public override void OnGraphsPostUpdate()
		{
			this.OnLatePostScan();
		}

		public override void OnLatePostScan()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.RemoveObstacles();
			NavGraph[] graphs = AstarPath.active.graphs;
			RVOSimulator active = RVOSimulator.active;
			if (active == null)
			{
				throw new NullReferenceException("No RVOSimulator could be found in the scene. Please add one to any GameObject");
			}
			this.lastSim = active.GetSimulator();
			for (int i = 0; i < graphs.Length; i++)
			{
				RecastGraph recastGraph = graphs[i] as RecastGraph;
				INavmesh navmesh = graphs[i] as INavmesh;
				GridGraph gridGraph = graphs[i] as GridGraph;
				if (recastGraph != null)
				{
					foreach (NavmeshTile ng in recastGraph.GetTiles())
					{
						this.AddGraphObstacles(this.lastSim, ng);
					}
				}
				else if (navmesh != null)
				{
					this.AddGraphObstacles(this.lastSim, navmesh);
				}
				else if (gridGraph != null)
				{
					this.AddGraphObstacles(this.lastSim, gridGraph);
				}
			}
		}

		public void RemoveObstacles()
		{
			if (this.lastSim != null)
			{
				for (int i = 0; i < this.obstacles.Count; i++)
				{
					this.lastSim.RemoveObstacle(this.obstacles[i]);
				}
				this.lastSim = null;
			}
			this.obstacles.Clear();
		}

		private void AddGraphObstacles(Simulator sim, GridGraph grid)
		{
			RVONavmesh.FindAllContours(grid, delegate(Vector3[] vertices, bool cycle)
			{
				this.obstacles.Add(sim.AddObstacle(vertices, this.wallHeight, true));
			}, null);
		}

		private static void FindAllContours(GridGraph grid, Action<Vector3[], bool> callback, GridNodeBase[] nodes = null)
		{
			if (grid is LayerGridGraph)
			{
				nodes = (nodes ?? (grid as LayerGridGraph).nodes);
			}
			nodes = (nodes ?? grid.nodes);
			int[] neighbourXOffsets = grid.neighbourXOffsets;
			int[] neighbourZOffsets = grid.neighbourZOffsets;
			int[] array;
			if (grid.neighbours == NumNeighbours.Six)
			{
				array = GridGraph.hexagonNeighbourIndices;
			}
			else
			{
				RuntimeHelpers.InitializeArray(array = new int[4], fieldof(<PrivateImplementationDetails>.$field-02E4414E7DFA0F3AA2387EE8EA7AB31431CB406A).FieldHandle);
			}
			int[] array2 = array;
			float num = (grid.neighbours != NumNeighbours.Six) ? 0.5f : 0.333333343f;
			if (nodes != null)
			{
				Dictionary<Int3, int> dictionary = new Dictionary<Int3, int>();
				Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
				HashSet<int> hashSet = new HashSet<int>();
				List<Vector3> vertices = ListPool<Vector3>.Claim();
				foreach (GridNodeBase gridNodeBase in nodes)
				{
					if (gridNodeBase != null && gridNodeBase.Walkable && !gridNodeBase.HasConnectionsToAllEightNeighbours)
					{
						for (int j = 0; j < array2.Length; j++)
						{
							int num2 = array2[j];
							if (gridNodeBase.GetNeighbourAlongDirection(num2) == null)
							{
								int num3 = array2[(j - 1 + array2.Length) % array2.Length];
								int num4 = array2[(j + 1) % array2.Length];
								Vector3 vector = new Vector3((float)gridNodeBase.XCoordinateInGrid + 0.5f, 0f, (float)gridNodeBase.ZCoordinateInGrid + 0.5f);
								vector.x += (float)neighbourXOffsets[num2] * num;
								vector.z += (float)neighbourZOffsets[num2] * num;
								vector.y = grid.transform.InverseTransform((Vector3)gridNodeBase.position).y;
								Vector3 vector3;
								Vector3 vector2 = vector3 = vector;
								vector3.x += (float)neighbourXOffsets[num3] * num;
								vector3.z += (float)neighbourZOffsets[num3] * num;
								vector2.x += (float)neighbourXOffsets[num4] * num;
								vector2.z += (float)neighbourZOffsets[num4] * num;
								Int3 key = (Int3)vector3;
								Int3 key2 = (Int3)vector2;
								int key3;
								if (dictionary.TryGetValue(key, out key3))
								{
									dictionary.Remove(key);
								}
								else
								{
									int count = vertices.Count;
									dictionary[key] = count;
									key3 = count;
									vertices.Add(vector3);
								}
								int num5;
								if (dictionary.TryGetValue(key2, out num5))
								{
									dictionary.Remove(key2);
								}
								else
								{
									int count = vertices.Count;
									dictionary[key2] = count;
									num5 = count;
									vertices.Add(vector2);
								}
								dictionary2.Add(key3, num5);
								hashSet.Add(num5);
							}
						}
					}
				}
				GraphTransform transform = grid.transform;
				List<Vector3> vertexBuffer = ListPool<Vector3>.Claim();
				RVONavmesh.CompressContour(dictionary2, hashSet, delegate(List<int> chain, bool cycle)
				{
					vertexBuffer.Clear();
					Vector3 vector4 = vertices[chain[0]];
					vertexBuffer.Add(vector4);
					for (int k = 1; k < chain.Count - 1; k++)
					{
						Vector3 vector5 = vertices[chain[k]];
						Vector3 vector6 = vector5 - vector4;
						Vector3 vector7 = vertices[chain[k + 1]] - vector4;
						if (((Mathf.Abs(vector6.x) > 0.01f || Mathf.Abs(vector7.x) > 0.01f) && (Mathf.Abs(vector6.z) > 0.01f || Mathf.Abs(vector7.z) > 0.01f)) || Mathf.Abs(vector6.y) > 0.01f || Mathf.Abs(vector7.y) > 0.01f)
						{
							vertexBuffer.Add(vector5);
						}
						vector4 = vector5;
					}
					vertexBuffer.Add(vertices[chain[chain.Count - 1]]);
					Vector3[] array3 = vertexBuffer.ToArray();
					transform.Transform(array3);
					callback(array3, cycle);
				});
				ListPool<Vector3>.Release(vertexBuffer);
				ListPool<Vector3>.Release(vertices);
			}
		}

		private void AddGraphObstacles(Simulator sim, INavmesh ng)
		{
			int[] uses = new int[3];
			Dictionary<int, int> outline = new Dictionary<int, int>();
			Dictionary<int, Int3> vertexPositions = new Dictionary<int, Int3>();
			HashSet<int> hasInEdge = new HashSet<int>();
			ng.GetNodes(delegate(GraphNode _node)
			{
				TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
				uses[0] = (uses[1] = (uses[2] = 0));
				if (triangleMeshNode != null)
				{
					for (int i = 0; i < triangleMeshNode.connections.Length; i++)
					{
						TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[i].node as TriangleMeshNode;
						if (triangleMeshNode2 != null)
						{
							int num = triangleMeshNode.SharedEdge(triangleMeshNode2);
							if (num != -1)
							{
								uses[num] = 1;
							}
						}
					}
					for (int j = 0; j < 3; j++)
					{
						if (uses[j] == 0)
						{
							int i2 = j;
							int i3 = (j + 1) % triangleMeshNode.GetVertexCount();
							outline[triangleMeshNode.GetVertexIndex(i2)] = triangleMeshNode.GetVertexIndex(i3);
							hasInEdge.Add(triangleMeshNode.GetVertexIndex(i3));
							vertexPositions[triangleMeshNode.GetVertexIndex(i2)] = triangleMeshNode.GetVertex(i2);
							vertexPositions[triangleMeshNode.GetVertexIndex(i3)] = triangleMeshNode.GetVertex(i3);
						}
					}
				}
			});
			List<Vector3> vertices = ListPool<Vector3>.Claim();
			RVONavmesh.CompressContour(outline, hasInEdge, delegate(List<int> chain, bool cycle)
			{
				for (int i = 0; i < chain.Count; i++)
				{
					vertices.Add((Vector3)vertexPositions[chain[i]]);
				}
				this.obstacles.Add(sim.AddObstacle(vertices.ToArray(), this.wallHeight, cycle));
				vertices.Clear();
			});
			ListPool<Vector3>.Release(vertices);
		}

		private static void CompressContour(Dictionary<int, int> outline, HashSet<int> hasInEdge, Action<List<int>, bool> results)
		{
			List<int> list = ListPool<int>.Claim();
			List<int> list2 = ListPool<int>.Claim();
			list2.AddRange(outline.Keys);
			for (int i = 0; i <= 1; i++)
			{
				bool flag = i == 1;
				for (int j = 0; j < list2.Count; j++)
				{
					int num = list2[j];
					if (flag || !hasInEdge.Contains(num))
					{
						int num2 = num;
						list.Clear();
						list.Add(num2);
						while (outline.ContainsKey(num2))
						{
							int num3 = outline[num2];
							outline.Remove(num2);
							list.Add(num3);
							if (num3 == num)
							{
								break;
							}
							num2 = num3;
						}
						if (list.Count > 1)
						{
							results(list, flag);
						}
					}
				}
			}
			ListPool<int>.Release(list2);
			ListPool<int>.Release(list);
		}

		public float wallHeight = 5f;

		private readonly List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		private Simulator lastSim;
	}
}
