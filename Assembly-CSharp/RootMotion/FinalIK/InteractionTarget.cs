using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Target")]
	public class InteractionTarget : MonoBehaviour
	{
		[ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
		private void OpenTutorial1()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
		private void OpenTutorial2()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
		private void OpenTutorial3()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
		}

		[ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
		private void OpenTutorial4()
		{
			Application.OpenURL("https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
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

		public float GetValue(InteractionObject.WeightCurve.Type curveType)
		{
			for (int i = 0; i < this.multipliers.Length; i++)
			{
				if (this.multipliers[i].curve == curveType)
				{
					return this.multipliers[i].multiplier;
				}
			}
			return 1f;
		}

		public void ResetRotation()
		{
			if (this.pivot != null)
			{
				this.pivot.localRotation = this.defaultLocalRotation;
			}
		}

		public void RotateTo(Vector3 position)
		{
			if (this.pivot == null)
			{
				return;
			}
			if (this.pivot != this.lastPivot)
			{
				this.defaultLocalRotation = this.pivot.localRotation;
				this.lastPivot = this.pivot;
			}
			this.pivot.localRotation = this.defaultLocalRotation;
			if (this.twistWeight > 0f)
			{
				Vector3 fromDirection = base.transform.position - this.pivot.position;
				Vector3 vector = this.pivot.rotation * this.twistAxis;
				Vector3 vector2 = vector;
				Vector3.OrthoNormalize(ref vector2, ref fromDirection);
				vector2 = vector;
				Vector3 toDirection = position - this.pivot.position;
				Vector3.OrthoNormalize(ref vector2, ref toDirection);
				Quaternion b = QuaTools.FromToAroundAxis(fromDirection, toDirection, vector);
				this.pivot.rotation = Quaternion.Lerp(Quaternion.identity, b, this.twistWeight) * this.pivot.rotation;
			}
			if (this.swingWeight > 0f)
			{
				Quaternion b2 = Quaternion.FromToRotation(base.transform.position - this.pivot.position, position - this.pivot.position);
				this.pivot.rotation = Quaternion.Lerp(Quaternion.identity, b2, this.swingWeight) * this.pivot.rotation;
			}
		}

		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_target.html");
		}

		[Tooltip("The type of the FBBIK effector.")]
		public FullBodyBipedEffector effectorType;

		[Tooltip("InteractionObject weight curve multipliers for this effector target.")]
		public InteractionTarget.Multiplier[] multipliers;

		[Tooltip("The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.")]
		public float interactionSpeedMlp = 1f;

		[Tooltip("The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.")]
		public Transform pivot;

		[Tooltip("The axis of twisting the interaction target (blue line).")]
		public Vector3 twistAxis = Vector3.up;

		[Tooltip("The weight of twisting the interaction target towards the effector bone in the start of the interaction.")]
		public float twistWeight = 1f;

		[Tooltip("The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.")]
		public float swingWeight;

		[Tooltip("If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.")]
		public bool rotateOnce = true;

		private Quaternion defaultLocalRotation;

		private Transform lastPivot;

		[Serializable]
		public class Multiplier
		{
			[Tooltip("The curve type (InteractionObject.WeightCurve.Type).")]
			public InteractionObject.WeightCurve.Type curve;

			[Tooltip("Multiplier of the curve's value.")]
			public float multiplier;
		}
	}
}
