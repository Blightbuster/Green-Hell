using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class OffsetModifier : MonoBehaviour
	{
		protected float deltaTime
		{
			get
			{
				return Time.time - this.lastTime;
			}
		}

		protected abstract void OnModifyOffset();

		protected virtual void Start()
		{
			base.StartCoroutine(this.Initiate());
		}

		private IEnumerator Initiate()
		{
			while (this.ik == null)
			{
				yield return null;
			}
			IKSolverFullBodyBiped solver = this.ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.ModifyOffset));
			this.lastTime = Time.time;
			yield break;
		}

		private void ModifyOffset()
		{
			if (!base.enabled)
			{
				return;
			}
			if (this.weight <= 0f)
			{
				return;
			}
			if (this.deltaTime <= 0f)
			{
				return;
			}
			if (this.ik == null)
			{
				return;
			}
			this.weight = Mathf.Clamp(this.weight, 0f, 1f);
			this.OnModifyOffset();
			this.lastTime = Time.time;
		}

		protected void ApplyLimits(OffsetModifier.OffsetLimits[] limits)
		{
			foreach (OffsetModifier.OffsetLimits offsetLimits in limits)
			{
				offsetLimits.Apply(this.ik.solver.GetEffector(offsetLimits.effector), base.transform.rotation);
			}
		}

		protected virtual void OnDestroy()
		{
			if (this.ik != null)
			{
				IKSolverFullBodyBiped solver = this.ik.solver;
				solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(this.ModifyOffset));
			}
		}

		[Tooltip("The master weight")]
		public float weight = 1f;

		[Tooltip("Reference to the FBBIK component")]
		public FullBodyBipedIK ik;

		protected float lastTime;

		[Serializable]
		public class OffsetLimits
		{
			public void Apply(IKEffector e, Quaternion rootRotation)
			{
				Vector3 point = Quaternion.Inverse(rootRotation) * e.positionOffset;
				if (this.spring <= 0f)
				{
					if (this.x)
					{
						point.x = Mathf.Clamp(point.x, this.minX, this.maxX);
					}
					if (this.y)
					{
						point.y = Mathf.Clamp(point.y, this.minY, this.maxY);
					}
					if (this.z)
					{
						point.z = Mathf.Clamp(point.z, this.minZ, this.maxZ);
					}
				}
				else
				{
					if (this.x)
					{
						point.x = this.SpringAxis(point.x, this.minX, this.maxX);
					}
					if (this.y)
					{
						point.y = this.SpringAxis(point.y, this.minY, this.maxY);
					}
					if (this.z)
					{
						point.z = this.SpringAxis(point.z, this.minZ, this.maxZ);
					}
				}
				e.positionOffset = rootRotation * point;
			}

			private float SpringAxis(float value, float min, float max)
			{
				if (value > min && value < max)
				{
					return value;
				}
				if (value < min)
				{
					return this.Spring(value, min, true);
				}
				return this.Spring(value, max, false);
			}

			private float Spring(float value, float limit, bool negative)
			{
				float num = value - limit;
				float num2 = num * this.spring;
				if (negative)
				{
					return value + Mathf.Clamp(-num2, 0f, -num);
				}
				return value - Mathf.Clamp(num2, 0f, num);
			}

			[Tooltip("The effector type (this is just an enum)")]
			public FullBodyBipedEffector effector;

			[Tooltip("Spring force, if zero then this is a hard limit, if not, offset can exceed the limit.")]
			public float spring;

			[Tooltip("Which axes to limit the offset on?")]
			public bool x;

			[Tooltip("Which axes to limit the offset on?")]
			public bool y;

			[Tooltip("Which axes to limit the offset on?")]
			public bool z;

			[Tooltip("The limits")]
			public float minX;

			[Tooltip("The limits")]
			public float maxX;

			[Tooltip("The limits")]
			public float minY;

			[Tooltip("The limits")]
			public float maxY;

			[Tooltip("The limits")]
			public float minZ;

			[Tooltip("The limits")]
			public float maxZ;
		}
	}
}
