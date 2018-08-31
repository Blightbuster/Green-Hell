using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Polygonal")]
	[HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
	public class RotationLimitPolygonal : RotationLimit
	{
		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page12.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_polygonal.html");
		}

		[ContextMenu("Support Group")]
		private void SupportGroup()
		{
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		[ContextMenu("Asset Store Thread")]
		private void ASThread()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		public void SetLimitPoints(RotationLimitPolygonal.LimitPoint[] points)
		{
			if (points.Length < 3)
			{
				base.LogWarning("The polygon must have at least 3 Limit Points.");
				return;
			}
			this.points = points;
			this.BuildReachCones();
		}

		protected override Quaternion LimitRotation(Quaternion rotation)
		{
			if (this.reachCones.Length == 0)
			{
				this.Start();
			}
			Quaternion rotation2 = this.LimitSwing(rotation);
			return RotationLimit.LimitTwist(rotation2, this.axis, base.secondaryAxis, this.twistLimit);
		}

		private void Start()
		{
			if (this.points.Length < 3)
			{
				this.ResetToDefault();
			}
			for (int i = 0; i < this.reachCones.Length; i++)
			{
				if (!this.reachCones[i].isValid)
				{
					if (this.smoothIterations <= 0)
					{
						int num;
						if (i < this.reachCones.Length - 1)
						{
							num = i + 1;
						}
						else
						{
							num = 0;
						}
						base.LogWarning(string.Concat(new object[]
						{
							"Reach Cone {point ",
							i,
							", point ",
							num,
							", Origin} has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex."
						}));
					}
					else
					{
						base.LogWarning("One of the Reach Cones in the polygon has negative volume. Make sure Axis vector is in the reachable area and the polygon is convex.");
					}
				}
			}
			this.axis = this.axis.normalized;
		}

		public void ResetToDefault()
		{
			this.points = new RotationLimitPolygonal.LimitPoint[4];
			for (int i = 0; i < this.points.Length; i++)
			{
				this.points[i] = new RotationLimitPolygonal.LimitPoint();
			}
			Quaternion quaternion = Quaternion.AngleAxis(45f, Vector3.right);
			Quaternion quaternion2 = Quaternion.AngleAxis(45f, Vector3.up);
			this.points[0].point = quaternion * quaternion2 * this.axis;
			this.points[1].point = Quaternion.Inverse(quaternion) * quaternion2 * this.axis;
			this.points[2].point = Quaternion.Inverse(quaternion) * Quaternion.Inverse(quaternion2) * this.axis;
			this.points[3].point = quaternion * Quaternion.Inverse(quaternion2) * this.axis;
			this.BuildReachCones();
		}

		public void BuildReachCones()
		{
			this.smoothIterations = Mathf.Clamp(this.smoothIterations, 0, 3);
			this.P = new Vector3[this.points.Length];
			for (int i = 0; i < this.points.Length; i++)
			{
				this.P[i] = this.points[i].point.normalized;
			}
			for (int j = 0; j < this.smoothIterations; j++)
			{
				this.P = this.SmoothPoints();
			}
			this.reachCones = new RotationLimitPolygonal.ReachCone[this.P.Length];
			for (int k = 0; k < this.reachCones.Length - 1; k++)
			{
				this.reachCones[k] = new RotationLimitPolygonal.ReachCone(Vector3.zero, this.axis.normalized, this.P[k], this.P[k + 1]);
			}
			this.reachCones[this.P.Length - 1] = new RotationLimitPolygonal.ReachCone(Vector3.zero, this.axis.normalized, this.P[this.P.Length - 1], this.P[0]);
			for (int l = 0; l < this.reachCones.Length; l++)
			{
				this.reachCones[l].Calculate();
			}
		}

		private Vector3[] SmoothPoints()
		{
			Vector3[] array = new Vector3[this.P.Length * 2];
			float scalar = this.GetScalar(this.P.Length);
			for (int i = 0; i < array.Length; i += 2)
			{
				array[i] = this.PointToTangentPlane(this.P[i / 2], 1f);
			}
			for (int j = 1; j < array.Length; j += 2)
			{
				Vector3 b = Vector3.zero;
				Vector3 vector = Vector3.zero;
				Vector3 b2 = Vector3.zero;
				if (j > 1 && j < array.Length - 2)
				{
					b = array[j - 2];
					b2 = array[j + 1];
				}
				else if (j == 1)
				{
					b = array[array.Length - 2];
					b2 = array[j + 1];
				}
				else if (j == array.Length - 1)
				{
					b = array[j - 2];
					b2 = array[0];
				}
				if (j < array.Length - 1)
				{
					vector = array[j + 1];
				}
				else
				{
					vector = array[0];
				}
				int num = array.Length / this.points.Length;
				array[j] = 0.5f * (array[j - 1] + vector) + scalar * this.points[j / num].tangentWeight * (vector - b) + scalar * this.points[j / num].tangentWeight * (array[j - 1] - b2);
			}
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = this.TangentPointToSphere(array[k], 1f);
			}
			return array;
		}

		private float GetScalar(int k)
		{
			if (k <= 3)
			{
				return 0.1667f;
			}
			if (k == 4)
			{
				return 0.1036f;
			}
			if (k == 5)
			{
				return 0.085f;
			}
			if (k == 6)
			{
				return 0.0773f;
			}
			if (k == 7)
			{
				return 0.07f;
			}
			return 0.0625f;
		}

		private Vector3 PointToTangentPlane(Vector3 p, float r)
		{
			float num = Vector3.Dot(this.axis, p);
			float num2 = 2f * r * r / (r * r + num);
			return num2 * p + (1f - num2) * -this.axis;
		}

		private Vector3 TangentPointToSphere(Vector3 q, float r)
		{
			float num = Vector3.Dot(q - this.axis, q - this.axis);
			float num2 = 4f * r * r / (4f * r * r + num);
			return num2 * q + (1f - num2) * -this.axis;
		}

		private Quaternion LimitSwing(Quaternion rotation)
		{
			if (rotation == Quaternion.identity)
			{
				return rotation;
			}
			Vector3 vector = rotation * this.axis;
			int reachCone = this.GetReachCone(vector);
			if (reachCone == -1)
			{
				if (!Warning.logged)
				{
					base.LogWarning("RotationLimitPolygonal reach cones are invalid.");
				}
				return rotation;
			}
			float num = Vector3.Dot(this.reachCones[reachCone].B, vector);
			if (num > 0f)
			{
				return rotation;
			}
			Vector3 rhs = Vector3.Cross(this.axis, vector);
			vector = Vector3.Cross(-this.reachCones[reachCone].B, rhs);
			Quaternion lhs = Quaternion.FromToRotation(rotation * this.axis, vector);
			return lhs * rotation;
		}

		private int GetReachCone(Vector3 L)
		{
			float num = Vector3.Dot(this.reachCones[0].S, L);
			for (int i = 0; i < this.reachCones.Length; i++)
			{
				float num2 = num;
				if (i < this.reachCones.Length - 1)
				{
					num = Vector3.Dot(this.reachCones[i + 1].S, L);
				}
				else
				{
					num = Vector3.Dot(this.reachCones[0].S, L);
				}
				if (num2 >= 0f && num < 0f)
				{
					return i;
				}
			}
			return -1;
		}

		[Range(0f, 180f)]
		public float twistLimit = 180f;

		[Range(0f, 3f)]
		public int smoothIterations;

		[HideInInspector]
		[SerializeField]
		public RotationLimitPolygonal.LimitPoint[] points;

		[SerializeField]
		[HideInInspector]
		public Vector3[] P;

		[SerializeField]
		[HideInInspector]
		public RotationLimitPolygonal.ReachCone[] reachCones = new RotationLimitPolygonal.ReachCone[0];

		[Serializable]
		public class ReachCone
		{
			public ReachCone(Vector3 _o, Vector3 _a, Vector3 _b, Vector3 _c)
			{
				this.tetrahedron = new Vector3[4];
				this.tetrahedron[0] = _o;
				this.tetrahedron[1] = _a;
				this.tetrahedron[2] = _b;
				this.tetrahedron[3] = _c;
				this.volume = 0f;
				this.S = Vector3.zero;
				this.B = Vector3.zero;
			}

			public Vector3 o
			{
				get
				{
					return this.tetrahedron[0];
				}
			}

			public Vector3 a
			{
				get
				{
					return this.tetrahedron[1];
				}
			}

			public Vector3 b
			{
				get
				{
					return this.tetrahedron[2];
				}
			}

			public Vector3 c
			{
				get
				{
					return this.tetrahedron[3];
				}
			}

			public bool isValid
			{
				get
				{
					return this.volume > 0f;
				}
			}

			public void Calculate()
			{
				Vector3 lhs = Vector3.Cross(this.a, this.b);
				this.volume = Vector3.Dot(lhs, this.c) / 6f;
				this.S = Vector3.Cross(this.a, this.b).normalized;
				this.B = Vector3.Cross(this.b, this.c).normalized;
			}

			public Vector3[] tetrahedron;

			public float volume;

			public Vector3 S;

			public Vector3 B;
		}

		[Serializable]
		public class LimitPoint
		{
			public LimitPoint()
			{
				this.point = Vector3.forward;
				this.tangentWeight = 1f;
			}

			public Vector3 point;

			public float tangentWeight;
		}
	}
}
