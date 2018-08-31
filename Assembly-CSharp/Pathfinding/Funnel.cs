using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public class Funnel
	{
		public static List<Funnel.PathPart> SplitIntoParts(Path path)
		{
			List<GraphNode> path2 = path.path;
			List<Funnel.PathPart> list = ListPool<Funnel.PathPart>.Claim();
			if (path2 == null || path2.Count == 0)
			{
				return list;
			}
			for (int i = 0; i < path2.Count; i++)
			{
				if (path2[i] is TriangleMeshNode || path2[i] is GridNodeBase)
				{
					Funnel.PathPart item = default(Funnel.PathPart);
					item.startIndex = i;
					uint graphIndex = path2[i].GraphIndex;
					while (i < path2.Count)
					{
						if (path2[i].GraphIndex != graphIndex && !(path2[i] is NodeLink3Node))
						{
							break;
						}
						i++;
					}
					i--;
					item.endIndex = i;
					if (item.startIndex == 0)
					{
						item.startPoint = path.vectorPath[0];
					}
					else
					{
						item.startPoint = (Vector3)path2[item.startIndex - 1].position;
					}
					if (item.endIndex == path2.Count - 1)
					{
						item.endPoint = path.vectorPath[path.vectorPath.Count - 1];
					}
					else
					{
						item.endPoint = (Vector3)path2[item.endIndex + 1].position;
					}
					list.Add(item);
				}
				else
				{
					if (!(NodeLink2.GetNodeLink(path2[i]) != null))
					{
						throw new Exception("Unsupported node type or null node");
					}
					Funnel.PathPart item2 = default(Funnel.PathPart);
					item2.startIndex = i;
					uint graphIndex2 = path2[i].GraphIndex;
					for (i++; i < path2.Count; i++)
					{
						if (path2[i].GraphIndex != graphIndex2)
						{
							break;
						}
					}
					i--;
					if (i - item2.startIndex != 0)
					{
						if (i - item2.startIndex != 1)
						{
							throw new Exception("NodeLink2 link length greater than two (2) nodes. " + (i - item2.startIndex + 1));
						}
						item2.endIndex = i;
						item2.isLink = true;
						item2.startPoint = (Vector3)path2[item2.startIndex].position;
						item2.endPoint = (Vector3)path2[item2.endIndex].position;
						list.Add(item2);
					}
				}
			}
			return list;
		}

		public static Funnel.FunnelPortals ConstructFunnelPortals(List<GraphNode> nodes, Funnel.PathPart part)
		{
			List<Vector3> list = ListPool<Vector3>.Claim(nodes.Count + 1);
			List<Vector3> list2 = ListPool<Vector3>.Claim(nodes.Count + 1);
			if (nodes == null || nodes.Count == 0)
			{
				return new Funnel.FunnelPortals
				{
					left = list,
					right = list2
				};
			}
			if (part.endIndex < part.startIndex || part.startIndex < 0 || part.endIndex > nodes.Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			list.Add(part.startPoint);
			list2.Add(part.startPoint);
			for (int i = part.startIndex; i < part.endIndex - 1; i++)
			{
				if (!nodes[i].GetPortal(nodes[i + 1], list, list2, false))
				{
					list.Add((Vector3)nodes[i].position);
					list2.Add((Vector3)nodes[i].position);
					list.Add((Vector3)nodes[i + 1].position);
					list2.Add((Vector3)nodes[i + 1].position);
				}
			}
			list.Add(part.endPoint);
			list2.Add(part.endPoint);
			return new Funnel.FunnelPortals
			{
				left = list,
				right = list2
			};
		}

		public static void ShrinkPortals(Funnel.FunnelPortals portals, float shrink)
		{
			if (shrink <= 1E-05f)
			{
				return;
			}
			for (int i = 0; i < portals.left.Count; i++)
			{
				Vector3 a = portals.left[i];
				Vector3 b = portals.right[i];
				float magnitude = (a - b).magnitude;
				if (magnitude > 0f)
				{
					float num = Mathf.Min(shrink / magnitude, 0.4f);
					portals.left[i] = Vector3.Lerp(a, b, num);
					portals.right[i] = Vector3.Lerp(a, b, 1f - num);
				}
			}
		}

		private static bool UnwrapHelper(Vector3 portalStart, Vector3 portalEnd, Vector3 prevPoint, Vector3 nextPoint, ref Quaternion mRot, ref Vector3 mOffset)
		{
			if (VectorMath.IsColinear(portalStart, portalEnd, nextPoint))
			{
				return false;
			}
			Vector3 vector = portalEnd - portalStart;
			float sqrMagnitude = vector.sqrMagnitude;
			prevPoint -= Vector3.Dot(prevPoint - portalStart, vector) / sqrMagnitude * vector;
			nextPoint -= Vector3.Dot(nextPoint - portalStart, vector) / sqrMagnitude * vector;
			Quaternion quaternion = Quaternion.FromToRotation(nextPoint - portalStart, portalStart - prevPoint);
			mOffset += mRot * (portalStart - quaternion * portalStart);
			mRot *= quaternion;
			return true;
		}

		public static void Unwrap(Funnel.FunnelPortals funnel, Vector2[] left, Vector2[] right)
		{
			Vector3 fromDirection = Vector3.Cross(funnel.right[1] - funnel.left[0], funnel.left[1] - funnel.left[0]);
			left[0] = (right[0] = Vector2.zero);
			Vector3 vector = funnel.left[1];
			Vector3 vector2 = funnel.right[1];
			Vector3 prevPoint = funnel.left[0];
			Quaternion rotation = Quaternion.FromToRotation(fromDirection, Vector3.forward);
			Vector3 b = rotation * -funnel.right[0];
			for (int i = 1; i < funnel.left.Count; i++)
			{
				if (Funnel.UnwrapHelper(vector, vector2, prevPoint, funnel.left[i], ref rotation, ref b))
				{
					prevPoint = vector;
					vector = funnel.left[i];
				}
				left[i] = rotation * funnel.left[i] + b;
				if (Funnel.UnwrapHelper(vector, vector2, prevPoint, funnel.right[i], ref rotation, ref b))
				{
					prevPoint = vector2;
					vector2 = funnel.right[i];
				}
				right[i] = rotation * funnel.right[i] + b;
			}
		}

		private static int FixFunnel(ref Vector2[] left, ref Vector2[] right)
		{
			if (left.Length != right.Length)
			{
				throw new ArgumentException("left and right lists must have equal length");
			}
			if (left.Length < 3)
			{
				return -1;
			}
			int num = 0;
			while (left[1] == left[2] && right[1] == right[2])
			{
				left[1] = left[0];
				right[1] = right[0];
				num++;
				if (left.Length - num < 3)
				{
					return -1;
				}
			}
			Vector2 vector = left[num + 2];
			if (vector == left[num + 1])
			{
				vector = right[num + 2];
			}
			while (VectorMath.IsColinear(left[num], left[num + 1], right[num + 1]) || VectorMath.RightOrColinear(left[num + 1], right[num + 1], vector) == VectorMath.RightOrColinear(left[num + 1], right[num + 1], left[num]))
			{
				left[num + 1] = left[num];
				right[num + 1] = right[num];
				num++;
				if (left.Length - num < 3)
				{
					return -1;
				}
				vector = left[num + 2];
				if (vector == left[num + 1])
				{
					vector = right[num + 2];
				}
			}
			if (!VectorMath.RightOrColinear(left[num], left[num + 1], right[num + 1]) && !VectorMath.IsColinear(left[num], left[num + 1], right[num + 1]))
			{
				Vector2[] array = left;
				left = right;
				right = array;
			}
			return num;
		}

		protected static Vector2 ToXZ(Vector3 p)
		{
			return new Vector2(p.x, p.z);
		}

		protected static Vector3 FromXZ(Vector2 p)
		{
			return new Vector3(p.x, 0f, p.y);
		}

		protected static bool RightOrColinear(Vector2 a, Vector2 b)
		{
			return a.x * b.y - b.x * a.y <= 0f;
		}

		protected static bool LeftOrColinear(Vector2 a, Vector2 b)
		{
			return a.x * b.y - b.x * a.y >= 0f;
		}

		public static List<Vector3> Calculate(Funnel.FunnelPortals funnel, bool unwrap, bool splitAtEveryPortal)
		{
			Vector2[] array = new Vector2[funnel.left.Count];
			Vector2[] array2 = new Vector2[funnel.left.Count];
			if (unwrap)
			{
				Funnel.Unwrap(funnel, array, array2);
			}
			else
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Funnel.ToXZ(funnel.left[i]);
					array2[i] = Funnel.ToXZ(funnel.right[i]);
				}
			}
			Vector2[] array3 = array;
			int num = Funnel.FixFunnel(ref array, ref array2);
			List<Vector3> list = funnel.left;
			List<Vector3> list2 = funnel.right;
			if (array3 != array)
			{
				list = funnel.right;
				list2 = funnel.left;
			}
			List<int> list3 = ListPool<int>.Claim();
			if (num == -1)
			{
				list3.Add(0);
				list3.Add(funnel.left.Count - 1);
			}
			else
			{
				bool flag;
				Funnel.Calculate(array, array2, num, list3, int.MaxValue, out flag);
			}
			List<Vector3> list4 = ListPool<Vector3>.Claim(list3.Count);
			Vector2 p = array[0];
			int num2 = 0;
			for (int j = 0; j < list3.Count; j++)
			{
				int num3 = list3[j];
				if (splitAtEveryPortal)
				{
					Vector2 vector = (num3 < 0) ? array2[-num3] : array[num3];
					for (int k = num2 + 1; k < Math.Abs(num3); k++)
					{
						float t = VectorMath.LineIntersectionFactorXZ(Funnel.FromXZ(array[k]), Funnel.FromXZ(array2[k]), Funnel.FromXZ(p), Funnel.FromXZ(vector));
						list4.Add(Vector3.Lerp(list[k], list2[k], t));
					}
					num2 = Mathf.Abs(num3);
					p = vector;
				}
				if (num3 >= 0)
				{
					list4.Add(list[num3]);
				}
				else
				{
					list4.Add(list2[-num3]);
				}
			}
			ListPool<Vector3>.Release(funnel.left);
			ListPool<Vector3>.Release(funnel.right);
			ListPool<int>.Release(list3);
			return list4;
		}

		private static void Calculate(Vector2[] left, Vector2[] right, int startIndex, List<int> funnelPath, int maxCorners, out bool lastCorner)
		{
			if (left.Length != right.Length)
			{
				throw new ArgumentException();
			}
			lastCorner = false;
			int num = startIndex + 1;
			int num2 = startIndex + 1;
			Vector2 vector = left[startIndex];
			Vector2 vector2 = left[num2];
			Vector2 vector3 = right[num];
			funnelPath.Add(startIndex);
			int i = startIndex + 2;
			while (i < left.Length)
			{
				if (funnelPath.Count >= maxCorners)
				{
					return;
				}
				if (funnelPath.Count > 2000)
				{
					Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
					break;
				}
				Vector2 vector4 = left[i];
				Vector2 vector5 = right[i];
				if (!Funnel.LeftOrColinear(vector3 - vector, vector5 - vector))
				{
					goto IL_10A;
				}
				if (vector == vector3 || Funnel.RightOrColinear(vector2 - vector, vector5 - vector))
				{
					vector3 = vector5;
					num = i;
					goto IL_10A;
				}
				vector3 = (vector = vector2);
				funnelPath.Add(i = (num = num2));
				IL_171:
				i++;
				continue;
				IL_10A:
				if (!Funnel.RightOrColinear(vector2 - vector, vector4 - vector))
				{
					goto IL_171;
				}
				if (vector == vector2 || Funnel.LeftOrColinear(vector3 - vector, vector4 - vector))
				{
					vector2 = vector4;
					num2 = i;
					goto IL_171;
				}
				vector2 = (vector = vector3);
				funnelPath.Add(-(i = (num2 = num)));
				goto IL_171;
			}
			lastCorner = true;
			funnelPath.Add(left.Length - 1);
		}

		public struct FunnelPortals
		{
			public List<Vector3> left;

			public List<Vector3> right;
		}

		public struct PathPart
		{
			public int startIndex;

			public int endIndex;

			public Vector3 startPoint;

			public Vector3 endPoint;

			public bool isLink;
		}
	}
}
