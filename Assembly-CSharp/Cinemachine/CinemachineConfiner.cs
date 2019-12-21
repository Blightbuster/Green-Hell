using System;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(22f, DocumentationSortingAttribute.Level.UserRef)]
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[SaveDuringPlay]
	public class CinemachineConfiner : CinemachineExtension
	{
		public bool CameraWasDisplaced(CinemachineVirtualCameraBase vcam)
		{
			return base.GetExtraState<CinemachineConfiner.VcamExtraState>(vcam).confinerDisplacement > 0f;
		}

		private void OnValidate()
		{
			this.m_Damping = Mathf.Max(0f, this.m_Damping);
		}

		public bool IsValid
		{
			get
			{
				return (this.m_ConfineMode == CinemachineConfiner.Mode.Confine3D && this.m_BoundingVolume != null) || (this.m_ConfineMode == CinemachineConfiner.Mode.Confine2D && this.m_BoundingShape2D != null);
			}
		}

		protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			if (this.IsValid && stage == CinemachineCore.Stage.Body)
			{
				Vector3 vector;
				if (this.m_ConfineScreenEdges && state.Lens.Orthographic)
				{
					vector = this.ConfineScreenEdges(vcam, ref state);
				}
				else
				{
					vector = this.ConfinePoint(state.CorrectedPosition);
				}
				CinemachineConfiner.VcamExtraState extraState = base.GetExtraState<CinemachineConfiner.VcamExtraState>(vcam);
				if (this.m_Damping > 0f && deltaTime >= 0f)
				{
					Vector3 vector2 = vector - extraState.m_previousDisplacement;
					vector2 = Damper.Damp(vector2, this.m_Damping, deltaTime);
					vector = extraState.m_previousDisplacement + vector2;
				}
				extraState.m_previousDisplacement = vector;
				state.PositionCorrection += vector;
				extraState.confinerDisplacement = vector.magnitude;
			}
		}

		public void InvalidatePathCache()
		{
			this.m_pathCache = null;
		}

		private bool ValidatePathCache()
		{
			Type left = (this.m_BoundingShape2D == null) ? null : this.m_BoundingShape2D.GetType();
			if (left == typeof(PolygonCollider2D))
			{
				PolygonCollider2D polygonCollider2D = this.m_BoundingShape2D as PolygonCollider2D;
				if (this.m_pathCache == null || this.m_pathCache.Count != polygonCollider2D.pathCount)
				{
					this.m_pathCache = new List<List<Vector2>>();
					for (int i = 0; i < polygonCollider2D.pathCount; i++)
					{
						Vector2[] path = polygonCollider2D.GetPath(i);
						List<Vector2> list = new List<Vector2>();
						for (int j = 0; j < path.Length; j++)
						{
							list.Add(path[j]);
						}
						this.m_pathCache.Add(list);
					}
				}
				return true;
			}
			if (left == typeof(CompositeCollider2D))
			{
				CompositeCollider2D compositeCollider2D = this.m_BoundingShape2D as CompositeCollider2D;
				if (this.m_pathCache == null || this.m_pathCache.Count != compositeCollider2D.pathCount)
				{
					this.m_pathCache = new List<List<Vector2>>();
					Vector2[] array = new Vector2[compositeCollider2D.pointCount];
					for (int k = 0; k < compositeCollider2D.pathCount; k++)
					{
						int path2 = compositeCollider2D.GetPath(k, array);
						List<Vector2> list2 = new List<Vector2>();
						for (int l = 0; l < path2; l++)
						{
							list2.Add(array[l]);
						}
						this.m_pathCache.Add(list2);
					}
				}
				return true;
			}
			this.InvalidatePathCache();
			return false;
		}

		private Vector3 ConfinePoint(Vector3 camPos)
		{
			if (this.m_ConfineMode == CinemachineConfiner.Mode.Confine3D)
			{
				return this.m_BoundingVolume.ClosestPoint(camPos) - camPos;
			}
			if (this.m_BoundingShape2D.OverlapPoint(camPos))
			{
				return Vector3.zero;
			}
			if (!this.ValidatePathCache())
			{
				return Vector3.zero;
			}
			Vector2 vector = camPos;
			Vector2 a = vector;
			float num = float.MaxValue;
			for (int i = 0; i < this.m_pathCache.Count; i++)
			{
				int count = this.m_pathCache[i].Count;
				if (count > 0)
				{
					Vector2 vector2 = this.m_BoundingShape2D.transform.TransformPoint(this.m_pathCache[i][count - 1]);
					for (int j = 0; j < count; j++)
					{
						Vector2 vector3 = this.m_BoundingShape2D.transform.TransformPoint(this.m_pathCache[i][j]);
						Vector2 vector4 = Vector2.Lerp(vector2, vector3, vector.ClosestPointOnSegment(vector2, vector3));
						float num2 = Vector2.SqrMagnitude(vector - vector4);
						if (num2 < num)
						{
							num = num2;
							a = vector4;
						}
						vector2 = vector3;
					}
				}
			}
			return a - vector;
		}

		private Vector3 ConfineScreenEdges(CinemachineVirtualCameraBase vcam, ref CameraState state)
		{
			Quaternion rotation = Quaternion.Inverse(state.CorrectedOrientation);
			float orthographicSize = state.Lens.OrthographicSize;
			float d = orthographicSize * state.Lens.Aspect;
			Vector3 b = rotation * Vector3.right * d;
			Vector3 b2 = rotation * Vector3.up * orthographicSize;
			Vector3 vector = Vector3.zero;
			Vector3 a = state.CorrectedPosition;
			for (int i = 0; i < 12; i++)
			{
				Vector3 vector2 = this.ConfinePoint(a - b2 - b);
				if (vector2.AlmostZero())
				{
					vector2 = this.ConfinePoint(a - b2 + b);
				}
				if (vector2.AlmostZero())
				{
					vector2 = this.ConfinePoint(a + b2 - b);
				}
				if (vector2.AlmostZero())
				{
					vector2 = this.ConfinePoint(a + b2 + b);
				}
				if (vector2.AlmostZero())
				{
					break;
				}
				vector += vector2;
				a += vector2;
			}
			return vector;
		}

		[Tooltip("The confiner can operate using a 2D bounding shape or a 3D bounding volume")]
		public CinemachineConfiner.Mode m_ConfineMode;

		[Tooltip("The volume within which the camera is to be contained")]
		public Collider m_BoundingVolume;

		[Tooltip("The 2D shape within which the camera is to be contained")]
		public Collider2D m_BoundingShape2D;

		[Tooltip("If camera is orthographic, screen edges will be confined to the volume.  If not checked, then only the camera center will be confined")]
		public bool m_ConfineScreenEdges = true;

		[Tooltip("How gradually to return the camera to the bounding volume if it goes beyond the borders.  Higher numbers are more gradual.")]
		[Range(0f, 10f)]
		public float m_Damping;

		private List<List<Vector2>> m_pathCache;

		public enum Mode
		{
			Confine2D,
			Confine3D
		}

		private class VcamExtraState
		{
			public Vector3 m_previousDisplacement;

			public float confinerDisplacement;
		}
	}
}
