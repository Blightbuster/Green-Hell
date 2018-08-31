using System;
using System.Diagnostics;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public class BBTree : IAstarPooledObject
	{
		public Rect Size
		{
			get
			{
				if (this.count == 0)
				{
					return new Rect(0f, 0f, 0f, 0f);
				}
				IntRect rect = this.tree[0].rect;
				return Rect.MinMaxRect((float)rect.xmin * 0.001f, (float)rect.ymin * 0.001f, (float)rect.xmax * 0.001f, (float)rect.ymax * 0.001f);
			}
		}

		public void Clear()
		{
			this.count = 0;
			this.leafNodes = 0;
			if (this.tree != null)
			{
				ArrayPool<BBTree.BBTreeBox>.Release(ref this.tree, false);
			}
			if (this.nodeLookup != null)
			{
				for (int i = 0; i < this.nodeLookup.Length; i++)
				{
					this.nodeLookup[i] = null;
				}
				ArrayPool<TriangleMeshNode>.Release(ref this.nodeLookup, false);
			}
			this.tree = ArrayPool<BBTree.BBTreeBox>.Claim(0);
			this.nodeLookup = ArrayPool<TriangleMeshNode>.Claim(0);
		}

		void IAstarPooledObject.OnEnterPool()
		{
			this.Clear();
		}

		private void EnsureCapacity(int c)
		{
			if (c > this.tree.Length)
			{
				BBTree.BBTreeBox[] array = ArrayPool<BBTree.BBTreeBox>.Claim(c);
				this.tree.CopyTo(array, 0);
				ArrayPool<BBTree.BBTreeBox>.Release(ref this.tree, false);
				this.tree = array;
			}
		}

		private void EnsureNodeCapacity(int c)
		{
			if (c > this.nodeLookup.Length)
			{
				TriangleMeshNode[] array = ArrayPool<TriangleMeshNode>.Claim(c);
				this.nodeLookup.CopyTo(array, 0);
				ArrayPool<TriangleMeshNode>.Release(ref this.nodeLookup, false);
				this.nodeLookup = array;
			}
		}

		private int GetBox(IntRect rect)
		{
			if (this.count >= this.tree.Length)
			{
				this.EnsureCapacity(this.count + 1);
			}
			this.tree[this.count] = new BBTree.BBTreeBox(rect);
			this.count++;
			return this.count - 1;
		}

		public void RebuildFrom(TriangleMeshNode[] nodes)
		{
			this.Clear();
			if (nodes.Length == 0)
			{
				return;
			}
			this.EnsureCapacity(Mathf.CeilToInt((float)nodes.Length * 2.1f));
			this.EnsureNodeCapacity(Mathf.CeilToInt((float)nodes.Length * 1.1f));
			int[] array = ArrayPool<int>.Claim(nodes.Length);
			for (int i = 0; i < nodes.Length; i++)
			{
				array[i] = i;
			}
			IntRect[] array2 = ArrayPool<IntRect>.Claim(nodes.Length);
			for (int j = 0; j < nodes.Length; j++)
			{
				Int3 @int;
				Int3 int2;
				Int3 int3;
				nodes[j].GetVertices(out @int, out int2, out int3);
				IntRect intRect = new IntRect(@int.x, @int.z, @int.x, @int.z);
				intRect = intRect.ExpandToContain(int2.x, int2.z);
				intRect = intRect.ExpandToContain(int3.x, int3.z);
				array2[j] = intRect;
			}
			this.RebuildFromInternal(nodes, array, array2, 0, nodes.Length, false);
			ArrayPool<int>.Release(ref array, false);
			ArrayPool<IntRect>.Release(ref array2, false);
		}

		private static int SplitByX(TriangleMeshNode[] nodes, int[] permutation, int from, int to, int divider)
		{
			int num = to;
			for (int i = from; i < num; i++)
			{
				if (nodes[permutation[i]].position.x > divider)
				{
					num--;
					int num2 = permutation[num];
					permutation[num] = permutation[i];
					permutation[i] = num2;
					i--;
				}
			}
			return num;
		}

		private static int SplitByZ(TriangleMeshNode[] nodes, int[] permutation, int from, int to, int divider)
		{
			int num = to;
			for (int i = from; i < num; i++)
			{
				if (nodes[permutation[i]].position.z > divider)
				{
					num--;
					int num2 = permutation[num];
					permutation[num] = permutation[i];
					permutation[i] = num2;
					i--;
				}
			}
			return num;
		}

		private int RebuildFromInternal(TriangleMeshNode[] nodes, int[] permutation, IntRect[] nodeBounds, int from, int to, bool odd)
		{
			IntRect rect = BBTree.NodeBounds(permutation, nodeBounds, from, to);
			int box = this.GetBox(rect);
			if (to - from <= 4)
			{
				int num = this.tree[box].nodeOffset = this.leafNodes * 4;
				this.EnsureNodeCapacity(num + 4);
				this.leafNodes++;
				for (int i = 0; i < 4; i++)
				{
					this.nodeLookup[num + i] = ((i >= to - from) ? null : nodes[permutation[from + i]]);
				}
				return box;
			}
			int num2;
			if (odd)
			{
				int divider = (rect.xmin + rect.xmax) / 2;
				num2 = BBTree.SplitByX(nodes, permutation, from, to, divider);
			}
			else
			{
				int divider2 = (rect.ymin + rect.ymax) / 2;
				num2 = BBTree.SplitByZ(nodes, permutation, from, to, divider2);
			}
			if (num2 == from || num2 == to)
			{
				if (!odd)
				{
					int divider3 = (rect.xmin + rect.xmax) / 2;
					num2 = BBTree.SplitByX(nodes, permutation, from, to, divider3);
				}
				else
				{
					int divider4 = (rect.ymin + rect.ymax) / 2;
					num2 = BBTree.SplitByZ(nodes, permutation, from, to, divider4);
				}
				if (num2 == from || num2 == to)
				{
					num2 = (from + to) / 2;
				}
			}
			this.tree[box].left = this.RebuildFromInternal(nodes, permutation, nodeBounds, from, num2, !odd);
			this.tree[box].right = this.RebuildFromInternal(nodes, permutation, nodeBounds, num2, to, !odd);
			return box;
		}

		private static IntRect NodeBounds(int[] permutation, IntRect[] nodeBounds, int from, int to)
		{
			IntRect result = nodeBounds[permutation[from]];
			for (int i = from + 1; i < to; i++)
			{
				IntRect intRect = nodeBounds[permutation[i]];
				result.xmin = Math.Min(result.xmin, intRect.xmin);
				result.ymin = Math.Min(result.ymin, intRect.ymin);
				result.xmax = Math.Max(result.xmax, intRect.xmax);
				result.ymax = Math.Max(result.ymax, intRect.ymax);
			}
			return result;
		}

		[Conditional("ASTARDEBUG")]
		private static void DrawDebugRect(IntRect rect)
		{
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmin, 0f, (float)rect.ymin), new Vector3((float)rect.xmax, 0f, (float)rect.ymin), Color.white);
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmin, 0f, (float)rect.ymax), new Vector3((float)rect.xmax, 0f, (float)rect.ymax), Color.white);
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmin, 0f, (float)rect.ymin), new Vector3((float)rect.xmin, 0f, (float)rect.ymax), Color.white);
			UnityEngine.Debug.DrawLine(new Vector3((float)rect.xmax, 0f, (float)rect.ymin), new Vector3((float)rect.xmax, 0f, (float)rect.ymax), Color.white);
		}

		[Conditional("ASTARDEBUG")]
		private static void DrawDebugNode(TriangleMeshNode node, float yoffset, Color color)
		{
			UnityEngine.Debug.DrawLine((Vector3)node.GetVertex(1) + Vector3.up * yoffset, (Vector3)node.GetVertex(2) + Vector3.up * yoffset, color);
			UnityEngine.Debug.DrawLine((Vector3)node.GetVertex(0) + Vector3.up * yoffset, (Vector3)node.GetVertex(1) + Vector3.up * yoffset, color);
			UnityEngine.Debug.DrawLine((Vector3)node.GetVertex(2) + Vector3.up * yoffset, (Vector3)node.GetVertex(0) + Vector3.up * yoffset, color);
		}

		public NNInfoInternal QueryClosest(Vector3 p, NNConstraint constraint, out float distance)
		{
			distance = float.PositiveInfinity;
			return this.QueryClosest(p, constraint, ref distance, new NNInfoInternal(null));
		}

		public NNInfoInternal QueryClosestXZ(Vector3 p, NNConstraint constraint, ref float distance, NNInfoInternal previous)
		{
			float num = distance * distance;
			float num2 = num;
			if (this.count > 0 && BBTree.SquaredRectPointDistance(this.tree[0].rect, p) < num)
			{
				this.SearchBoxClosestXZ(0, p, ref num, constraint, ref previous);
				if (num < num2)
				{
					distance = Mathf.Sqrt(num);
				}
			}
			return previous;
		}

		private void SearchBoxClosestXZ(int boxi, Vector3 p, ref float closestSqrDist, NNConstraint constraint, ref NNInfoInternal nnInfo)
		{
			BBTree.BBTreeBox bbtreeBox = this.tree[boxi];
			if (bbtreeBox.IsLeaf)
			{
				TriangleMeshNode[] array = this.nodeLookup;
				int num = 0;
				while (num < 4 && array[bbtreeBox.nodeOffset + num] != null)
				{
					TriangleMeshNode triangleMeshNode = array[bbtreeBox.nodeOffset + num];
					if (constraint == null || constraint.Suitable(triangleMeshNode))
					{
						Vector3 constClampedPosition = triangleMeshNode.ClosestPointOnNodeXZ(p);
						float num2 = (constClampedPosition.x - p.x) * (constClampedPosition.x - p.x) + (constClampedPosition.z - p.z) * (constClampedPosition.z - p.z);
						if (nnInfo.constrainedNode == null || num2 < closestSqrDist - 1E-06f || (num2 <= closestSqrDist + 1E-06f && Mathf.Abs(constClampedPosition.y - p.y) < Mathf.Abs(nnInfo.constClampedPosition.y - p.y)))
						{
							nnInfo.constrainedNode = triangleMeshNode;
							nnInfo.constClampedPosition = constClampedPosition;
							closestSqrDist = num2;
						}
					}
					num++;
				}
			}
			else
			{
				int left = bbtreeBox.left;
				int right = bbtreeBox.right;
				float num3;
				float num4;
				this.GetOrderedChildren(ref left, ref right, out num3, out num4, p);
				if (num3 <= closestSqrDist)
				{
					this.SearchBoxClosestXZ(left, p, ref closestSqrDist, constraint, ref nnInfo);
				}
				if (num4 <= closestSqrDist)
				{
					this.SearchBoxClosestXZ(right, p, ref closestSqrDist, constraint, ref nnInfo);
				}
			}
		}

		public NNInfoInternal QueryClosest(Vector3 p, NNConstraint constraint, ref float distance, NNInfoInternal previous)
		{
			float num = distance * distance;
			float num2 = num;
			if (this.count > 0 && BBTree.SquaredRectPointDistance(this.tree[0].rect, p) < num)
			{
				this.SearchBoxClosest(0, p, ref num, constraint, ref previous);
				if (num < num2)
				{
					distance = Mathf.Sqrt(num);
				}
			}
			return previous;
		}

		private void SearchBoxClosest(int boxi, Vector3 p, ref float closestSqrDist, NNConstraint constraint, ref NNInfoInternal nnInfo)
		{
			BBTree.BBTreeBox bbtreeBox = this.tree[boxi];
			if (bbtreeBox.IsLeaf)
			{
				TriangleMeshNode[] array = this.nodeLookup;
				int num = 0;
				while (num < 4 && array[bbtreeBox.nodeOffset + num] != null)
				{
					TriangleMeshNode triangleMeshNode = array[bbtreeBox.nodeOffset + num];
					Vector3 vector = triangleMeshNode.ClosestPointOnNode(p);
					float sqrMagnitude = (vector - p).sqrMagnitude;
					if (sqrMagnitude < closestSqrDist)
					{
						if (constraint == null || constraint.Suitable(triangleMeshNode))
						{
							nnInfo.constrainedNode = triangleMeshNode;
							nnInfo.constClampedPosition = vector;
							closestSqrDist = sqrMagnitude;
						}
					}
					num++;
				}
			}
			else
			{
				int left = bbtreeBox.left;
				int right = bbtreeBox.right;
				float num2;
				float num3;
				this.GetOrderedChildren(ref left, ref right, out num2, out num3, p);
				if (num2 < closestSqrDist)
				{
					this.SearchBoxClosest(left, p, ref closestSqrDist, constraint, ref nnInfo);
				}
				if (num3 < closestSqrDist)
				{
					this.SearchBoxClosest(right, p, ref closestSqrDist, constraint, ref nnInfo);
				}
			}
		}

		private void GetOrderedChildren(ref int first, ref int second, out float firstDist, out float secondDist, Vector3 p)
		{
			firstDist = BBTree.SquaredRectPointDistance(this.tree[first].rect, p);
			secondDist = BBTree.SquaredRectPointDistance(this.tree[second].rect, p);
			if (secondDist < firstDist)
			{
				int num = first;
				first = second;
				second = num;
				float num2 = firstDist;
				firstDist = secondDist;
				secondDist = num2;
			}
		}

		public TriangleMeshNode QueryInside(Vector3 p, NNConstraint constraint)
		{
			return (this.count == 0 || !this.tree[0].Contains(p)) ? null : this.SearchBoxInside(0, p, constraint);
		}

		private TriangleMeshNode SearchBoxInside(int boxi, Vector3 p, NNConstraint constraint)
		{
			BBTree.BBTreeBox bbtreeBox = this.tree[boxi];
			if (bbtreeBox.IsLeaf)
			{
				TriangleMeshNode[] array = this.nodeLookup;
				int num = 0;
				while (num < 4 && array[bbtreeBox.nodeOffset + num] != null)
				{
					TriangleMeshNode triangleMeshNode = array[bbtreeBox.nodeOffset + num];
					if (triangleMeshNode.ContainsPoint((Int3)p))
					{
						if (constraint == null || constraint.Suitable(triangleMeshNode))
						{
							return triangleMeshNode;
						}
					}
					num++;
				}
			}
			else
			{
				if (this.tree[bbtreeBox.left].Contains(p))
				{
					TriangleMeshNode triangleMeshNode2 = this.SearchBoxInside(bbtreeBox.left, p, constraint);
					if (triangleMeshNode2 != null)
					{
						return triangleMeshNode2;
					}
				}
				if (this.tree[bbtreeBox.right].Contains(p))
				{
					TriangleMeshNode triangleMeshNode3 = this.SearchBoxInside(bbtreeBox.right, p, constraint);
					if (triangleMeshNode3 != null)
					{
						return triangleMeshNode3;
					}
				}
			}
			return null;
		}

		public void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
			if (this.count == 0)
			{
				return;
			}
			this.OnDrawGizmos(0, 0);
		}

		private void OnDrawGizmos(int boxi, int depth)
		{
			BBTree.BBTreeBox bbtreeBox = this.tree[boxi];
			Vector3 a = (Vector3)new Int3(bbtreeBox.rect.xmin, 0, bbtreeBox.rect.ymin);
			Vector3 vector = (Vector3)new Int3(bbtreeBox.rect.xmax, 0, bbtreeBox.rect.ymax);
			Vector3 vector2 = (a + vector) * 0.5f;
			Vector3 size = (vector - vector2) * 2f;
			size = new Vector3(size.x, 1f, size.z);
			vector2.y += (float)(depth * 2);
			Gizmos.color = AstarMath.IntToColor(depth, 1f);
			Gizmos.DrawCube(vector2, size);
			if (!bbtreeBox.IsLeaf)
			{
				this.OnDrawGizmos(bbtreeBox.left, depth + 1);
				this.OnDrawGizmos(bbtreeBox.right, depth + 1);
			}
		}

		private static bool NodeIntersectsCircle(TriangleMeshNode node, Vector3 p, float radius)
		{
			return float.IsPositiveInfinity(radius) || (p - node.ClosestPointOnNode(p)).sqrMagnitude < radius * radius;
		}

		private static bool RectIntersectsCircle(IntRect r, Vector3 p, float radius)
		{
			if (float.IsPositiveInfinity(radius))
			{
				return true;
			}
			Vector3 vector = p;
			p.x = Math.Max(p.x, (float)r.xmin * 0.001f);
			p.x = Math.Min(p.x, (float)r.xmax * 0.001f);
			p.z = Math.Max(p.z, (float)r.ymin * 0.001f);
			p.z = Math.Min(p.z, (float)r.ymax * 0.001f);
			return (p.x - vector.x) * (p.x - vector.x) + (p.z - vector.z) * (p.z - vector.z) < radius * radius;
		}

		private static float SquaredRectPointDistance(IntRect r, Vector3 p)
		{
			Vector3 vector = p;
			p.x = Math.Max(p.x, (float)r.xmin * 0.001f);
			p.x = Math.Min(p.x, (float)r.xmax * 0.001f);
			p.z = Math.Max(p.z, (float)r.ymin * 0.001f);
			p.z = Math.Min(p.z, (float)r.ymax * 0.001f);
			return (p.x - vector.x) * (p.x - vector.x) + (p.z - vector.z) * (p.z - vector.z);
		}

		private BBTree.BBTreeBox[] tree;

		private TriangleMeshNode[] nodeLookup;

		private int count;

		private int leafNodes;

		private const int MaximumLeafSize = 4;

		private struct BBTreeBox
		{
			public BBTreeBox(IntRect rect)
			{
				this.nodeOffset = -1;
				this.rect = rect;
				this.left = (this.right = -1);
			}

			public BBTreeBox(int nodeOffset, IntRect rect)
			{
				this.nodeOffset = nodeOffset;
				this.rect = rect;
				this.left = (this.right = -1);
			}

			public bool IsLeaf
			{
				get
				{
					return this.nodeOffset >= 0;
				}
			}

			public bool Contains(Vector3 point)
			{
				Int3 @int = (Int3)point;
				return this.rect.Contains(@int.x, @int.z);
			}

			public IntRect rect;

			public int nodeOffset;

			public int left;

			public int right;
		}
	}
}
