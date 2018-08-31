using System;
using Pathfinding.Util;

namespace Pathfinding
{
	public class NavmeshTile : INavmesh, INavmeshHolder
	{
		public void GetTileCoordinates(int tileIndex, out int x, out int z)
		{
			x = this.x;
			z = this.z;
		}

		public int GetVertexArrayIndex(int index)
		{
			return index & 4095;
		}

		public Int3 GetVertex(int index)
		{
			int num = index & 4095;
			return this.verts[num];
		}

		public Int3 GetVertexInGraphSpace(int index)
		{
			return this.vertsInGraphSpace[index & 4095];
		}

		public void GetNodes(Action<GraphNode> action)
		{
			if (this.nodes == null)
			{
				return;
			}
			for (int i = 0; i < this.nodes.Length; i++)
			{
				action(this.nodes[i]);
			}
		}

		internal void Destroy()
		{
			if (this.nodes.Length > 0)
			{
				int tileIndex = NavmeshBase.GetTileIndex(this.nodes[0].GetVertexIndex(0));
				uint graphIndex = this.nodes[0].GraphIndex;
				for (int i = 0; i < this.nodes.Length; i++)
				{
					TriangleMeshNode triangleMeshNode = this.nodes[i];
					if (triangleMeshNode.connections != null)
					{
						for (int j = 0; j < triangleMeshNode.connections.Length; j++)
						{
							TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j].node as TriangleMeshNode;
							if (triangleMeshNode2 != null && triangleMeshNode2.GraphIndex == graphIndex && NavmeshBase.GetTileIndex(triangleMeshNode2.GetVertexIndex(0)) == tileIndex)
							{
								triangleMeshNode.connections[j].node = null;
							}
						}
					}
				}
				for (int k = 0; k < this.nodes.Length; k++)
				{
					this.nodes[k].Destroy();
				}
			}
			this.nodes = null;
			ObjectPool<BBTree>.Release(ref this.bbTree);
		}

		public int[] tris;

		public Int3[] verts;

		public Int3[] vertsInGraphSpace;

		public int x;

		public int z;

		public int w;

		public int d;

		public TriangleMeshNode[] nodes;

		public BBTree bbTree;

		public bool flag;
	}
}
