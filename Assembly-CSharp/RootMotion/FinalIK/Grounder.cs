using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public abstract class Grounder : MonoBehaviour
	{
		public abstract void ResetPosition();

		protected Vector3 GetSpineOffsetTarget()
		{
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < this.solver.legs.Length; i++)
			{
				vector += this.GetLegSpineBendVector(this.solver.legs[i]);
			}
			return vector;
		}

		protected void LogWarning(string message)
		{
			Warning.Log(message, base.transform, false);
		}

		private Vector3 GetLegSpineBendVector(Grounding.Leg leg)
		{
			Vector3 legSpineTangent = this.GetLegSpineTangent(leg);
			float d = (Vector3.Dot(this.solver.root.forward, legSpineTangent.normalized) + 1f) * 0.5f;
			float magnitude = (leg.IKPosition - leg.transform.position).magnitude;
			return legSpineTangent * magnitude * d;
		}

		private Vector3 GetLegSpineTangent(Grounding.Leg leg)
		{
			Vector3 result = leg.transform.position - this.solver.root.position;
			if (!this.solver.rotateSolver || this.solver.root.up == Vector3.up)
			{
				return new Vector3(result.x, 0f, result.z);
			}
			Vector3 up = this.solver.root.up;
			Vector3.OrthoNormalize(ref up, ref result);
			return result;
		}

		protected abstract void OpenUserManual();

		protected abstract void OpenScriptReference();

		[Tooltip("The master weight. Use this to fade in/out the grounding effect.")]
		[Range(0f, 1f)]
		public float weight = 1f;

		[Tooltip("The Grounding solver. Not to confuse with IK solvers.")]
		public Grounding solver = new Grounding();

		public Grounder.GrounderDelegate OnPreGrounder;

		public Grounder.GrounderDelegate OnPostGrounder;

		protected bool initiated;

		public delegate void GrounderDelegate();
	}
}
