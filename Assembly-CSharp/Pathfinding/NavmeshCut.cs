using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinding
{
	[AddComponentMenu("Pathfinding/Navmesh/Navmesh Cut")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_navmesh_cut.php")]
	public class NavmeshCut : NavmeshClipper
	{
		protected override void Awake()
		{
			base.Awake();
			this.tr = base.transform;
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			this.lastPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			this.lastRotation = this.tr.rotation;
		}

		public override void ForceUpdate()
		{
			this.lastPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		}

		public override bool RequiresUpdate()
		{
			return (this.tr.position - this.lastPosition).sqrMagnitude > this.updateDistance * this.updateDistance || (this.useRotationAndScale && Quaternion.Angle(this.lastRotation, this.tr.rotation) > this.updateRotationDistance);
		}

		public virtual void UsedForCut()
		{
		}

		internal override void NotifyUpdated()
		{
			this.lastPosition = this.tr.position;
			if (this.useRotationAndScale)
			{
				this.lastRotation = this.tr.rotation;
			}
		}

		private void CalculateMeshContour()
		{
			if (this.mesh == null)
			{
				return;
			}
			NavmeshCut.edges.Clear();
			NavmeshCut.pointers.Clear();
			Vector3[] vertices = this.mesh.vertices;
			int[] triangles = this.mesh.triangles;
			for (int i = 0; i < triangles.Length; i += 3)
			{
				if (VectorMath.IsClockwiseXZ(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]))
				{
					int num = triangles[i];
					triangles[i] = triangles[i + 2];
					triangles[i + 2] = num;
				}
				NavmeshCut.edges[new Int2(triangles[i], triangles[i + 1])] = i;
				NavmeshCut.edges[new Int2(triangles[i + 1], triangles[i + 2])] = i;
				NavmeshCut.edges[new Int2(triangles[i + 2], triangles[i])] = i;
			}
			for (int j = 0; j < triangles.Length; j += 3)
			{
				for (int k = 0; k < 3; k++)
				{
					if (!NavmeshCut.edges.ContainsKey(new Int2(triangles[j + (k + 1) % 3], triangles[j + k % 3])))
					{
						NavmeshCut.pointers[triangles[j + k % 3]] = triangles[j + (k + 1) % 3];
					}
				}
			}
			List<Vector3[]> list = new List<Vector3[]>();
			List<Vector3> list2 = ListPool<Vector3>.Claim();
			for (int l = 0; l < vertices.Length; l++)
			{
				if (NavmeshCut.pointers.ContainsKey(l))
				{
					list2.Clear();
					int num2 = l;
					do
					{
						int num3 = NavmeshCut.pointers[num2];
						if (num3 == -1)
						{
							break;
						}
						NavmeshCut.pointers[num2] = -1;
						list2.Add(vertices[num2]);
						num2 = num3;
						if (num2 == -1)
						{
							goto Block_9;
						}
					}
					while (num2 != l);
					IL_1E4:
					if (list2.Count > 0)
					{
						list.Add(list2.ToArray());
						goto IL_1F9;
					}
					goto IL_1F9;
					Block_9:
					Debug.LogError("Invalid Mesh '" + this.mesh.name + " in " + base.gameObject.name);
					goto IL_1E4;
				}
				IL_1F9:;
			}
			ListPool<Vector3>.Release(list2);
			this.contours = list.ToArray();
		}

		internal override Rect GetBounds(GraphTransform inverseTranform)
		{
			List<List<Vector3>> list = ListPool<List<Vector3>>.Claim();
			this.GetContour(list);
			Rect result = default(Rect);
			for (int i = 0; i < list.Count; i++)
			{
				List<Vector3> list2 = list[i];
				for (int j = 0; j < list2.Count; j++)
				{
					Vector3 vector = inverseTranform.InverseTransform(list2[j]);
					if (j == 0)
					{
						result = new Rect(vector.x, vector.z, 0f, 0f);
					}
					else
					{
						result.xMax = Math.Max(result.xMax, vector.x);
						result.yMax = Math.Max(result.yMax, vector.z);
						result.xMin = Math.Min(result.xMin, vector.x);
						result.yMin = Math.Min(result.yMin, vector.z);
					}
				}
			}
			ListPool<List<Vector3>>.Release(list);
			return result;
		}

		public void GetContour(List<List<Vector3>> buffer)
		{
			if (this.circleResolution < 3)
			{
				this.circleResolution = 3;
			}
			switch (this.type)
			{
			case NavmeshCut.MeshType.Rectangle:
			{
				List<Vector3> list = ListPool<Vector3>.Claim();
				list.Add(new Vector3(-this.rectangleSize.x, 0f, -this.rectangleSize.y) * 0.5f);
				list.Add(new Vector3(this.rectangleSize.x, 0f, -this.rectangleSize.y) * 0.5f);
				list.Add(new Vector3(this.rectangleSize.x, 0f, this.rectangleSize.y) * 0.5f);
				list.Add(new Vector3(-this.rectangleSize.x, 0f, this.rectangleSize.y) * 0.5f);
				bool reverse = this.rectangleSize.x < 0f ^ this.rectangleSize.y < 0f;
				this.TransformBuffer(list, reverse);
				buffer.Add(list);
				return;
			}
			case NavmeshCut.MeshType.Circle:
			{
				List<Vector3> list = ListPool<Vector3>.Claim(this.circleResolution);
				for (int i = 0; i < this.circleResolution; i++)
				{
					list.Add(new Vector3(Mathf.Cos((float)(i * 2) * 3.14159274f / (float)this.circleResolution), 0f, Mathf.Sin((float)(i * 2) * 3.14159274f / (float)this.circleResolution)) * this.circleRadius);
				}
				bool reverse = this.circleRadius < 0f;
				this.TransformBuffer(list, reverse);
				buffer.Add(list);
				return;
			}
			case NavmeshCut.MeshType.CustomMesh:
				if (this.mesh != this.lastMesh || this.contours == null)
				{
					this.CalculateMeshContour();
					this.lastMesh = this.mesh;
				}
				if (this.contours != null)
				{
					bool reverse = this.meshScale < 0f;
					for (int j = 0; j < this.contours.Length; j++)
					{
						Vector3[] array = this.contours[j];
						List<Vector3> list = ListPool<Vector3>.Claim(array.Length);
						for (int k = 0; k < array.Length; k++)
						{
							list.Add(array[k] * this.meshScale);
						}
						this.TransformBuffer(list, reverse);
						buffer.Add(list);
					}
				}
				return;
			default:
				return;
			}
		}

		private void TransformBuffer(List<Vector3> buffer, bool reverse)
		{
			Vector3 vector = this.center;
			if (this.useRotationAndScale)
			{
				Matrix4x4 localToWorldMatrix = this.tr.localToWorldMatrix;
				for (int i = 0; i < buffer.Count; i++)
				{
					buffer[i] = localToWorldMatrix.MultiplyPoint3x4(buffer[i] + vector);
				}
				reverse ^= VectorMath.ReversesFaceOrientationsXZ(localToWorldMatrix);
			}
			else
			{
				vector += this.tr.position;
				for (int j = 0; j < buffer.Count; j++)
				{
					int index = j;
					buffer[index] += vector;
				}
			}
			if (reverse)
			{
				buffer.Reverse();
			}
		}

		public void OnDrawGizmos()
		{
			if (this.tr == null)
			{
				this.tr = base.transform;
			}
			List<List<Vector3>> list = ListPool<List<Vector3>>.Claim();
			this.GetContour(list);
			Gizmos.color = NavmeshCut.GizmoColor;
			for (int i = 0; i < list.Count; i++)
			{
				List<Vector3> list2 = list[i];
				for (int j = 0; j < list2.Count; j++)
				{
					Vector3 from = list2[j];
					Vector3 to = list2[(j + 1) % list2.Count];
					Gizmos.DrawLine(from, to);
				}
			}
			ListPool<List<Vector3>>.Release(list);
		}

		internal float GetY(GraphTransform transform)
		{
			return transform.InverseTransform(this.useRotationAndScale ? this.tr.TransformPoint(this.center) : (this.tr.position + this.center)).y;
		}

		public void OnDrawGizmosSelected()
		{
			List<List<Vector3>> list = ListPool<List<Vector3>>.Claim();
			this.GetContour(list);
			Color color = Color.Lerp(NavmeshCut.GizmoColor, Color.white, 0.5f);
			color.a *= 0.5f;
			Gizmos.color = color;
			NavmeshBase navmeshBase = (AstarPath.active != null) ? (AstarPath.active.data.recastGraph ?? AstarPath.active.data.navmesh) : null;
			GraphTransform graphTransform = (navmeshBase != null) ? navmeshBase.transform : GraphTransform.identityTransform;
			float y = this.GetY(graphTransform);
			float y2 = y - this.height * 0.5f;
			float y3 = y + this.height * 0.5f;
			for (int i = 0; i < list.Count; i++)
			{
				List<Vector3> list2 = list[i];
				for (int j = 0; j < list2.Count; j++)
				{
					Vector3 vector = graphTransform.InverseTransform(list2[j]);
					Vector3 vector2 = graphTransform.InverseTransform(list2[(j + 1) % list2.Count]);
					Vector3 p = vector;
					Vector3 p2 = vector2;
					Vector3 p3 = vector;
					Vector3 p4 = vector2;
					p.y = (p2.y = y2);
					p3.y = (p4.y = y3);
					Gizmos.DrawLine(graphTransform.Transform(p), graphTransform.Transform(p2));
					Gizmos.DrawLine(graphTransform.Transform(p3), graphTransform.Transform(p4));
					Gizmos.DrawLine(graphTransform.Transform(p), graphTransform.Transform(p3));
				}
			}
			ListPool<List<Vector3>>.Release(list);
		}

		[Tooltip("Shape of the cut")]
		public NavmeshCut.MeshType type;

		[Tooltip("The contour(s) of the mesh will be extracted. This mesh should only be a 2D surface, not a volume (see documentation).")]
		public Mesh mesh;

		public Vector2 rectangleSize = new Vector2(1f, 1f);

		public float circleRadius = 1f;

		public int circleResolution = 6;

		public float height = 1f;

		[Tooltip("Scale of the custom mesh")]
		public float meshScale = 1f;

		public Vector3 center;

		[Tooltip("Distance between positions to require an update of the navmesh\nA smaller distance gives better accuracy, but requires more updates when moving the object over time, so it is often slower.")]
		public float updateDistance = 0.4f;

		[Tooltip("Only makes a split in the navmesh, but does not remove the geometry to make a hole")]
		public bool isDual;

		public bool cutsAddedGeom = true;

		[Tooltip("How many degrees rotation that is required for an update to the navmesh. Should be between 0 and 180.")]
		public float updateRotationDistance = 10f;

		[Tooltip("Includes rotation in calculations. This is slower since a lot more matrix multiplications are needed but gives more flexibility.")]
		[FormerlySerializedAs("useRotation")]
		public bool useRotationAndScale;

		private Vector3[][] contours;

		protected Transform tr;

		private Mesh lastMesh;

		private Vector3 lastPosition;

		private Quaternion lastRotation;

		private static readonly Dictionary<Int2, int> edges = new Dictionary<Int2, int>();

		private static readonly Dictionary<int, int> pointers = new Dictionary<int, int>();

		public static readonly Color GizmoColor = new Color(0.145098045f, 0.721568644f, 0.9372549f);

		public enum MeshType
		{
			Rectangle,
			Circle,
			CustomMesh
		}
	}
}
