using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Object")]
	[HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
	public class InteractionObject : MonoBehaviour
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

		public float length { get; private set; }

		public InteractionSystem lastUsedInteractionSystem { get; private set; }

		public void Initiate()
		{
			for (int i = 0; i < this.weightCurves.Length; i++)
			{
				if (this.weightCurves[i].curve.length > 0)
				{
					float time = this.weightCurves[i].curve.keys[this.weightCurves[i].curve.length - 1].time;
					this.length = Mathf.Clamp(this.length, time, this.length);
				}
			}
			for (int j = 0; j < this.events.Length; j++)
			{
				this.length = Mathf.Clamp(this.length, this.events[j].time, this.length);
			}
			this.targets = this.targetsRoot.GetComponentsInChildren<InteractionTarget>();
		}

		public Transform lookAtTarget
		{
			get
			{
				if (this.otherLookAtTarget != null)
				{
					return this.otherLookAtTarget;
				}
				return base.transform;
			}
		}

		public InteractionTarget GetTarget(FullBodyBipedEffector effectorType, InteractionSystem interactionSystem)
		{
			if (interactionSystem.tag == string.Empty || interactionSystem.tag == string.Empty)
			{
				foreach (InteractionTarget interactionTarget in this.targets)
				{
					if (interactionTarget.effectorType == effectorType)
					{
						return interactionTarget;
					}
				}
				return null;
			}
			foreach (InteractionTarget interactionTarget2 in this.targets)
			{
				if (interactionTarget2.effectorType == effectorType && interactionTarget2.tag == interactionSystem.tag)
				{
					return interactionTarget2;
				}
			}
			return null;
		}

		public bool CurveUsed(InteractionObject.WeightCurve.Type type)
		{
			foreach (InteractionObject.WeightCurve weightCurve in this.weightCurves)
			{
				if (weightCurve.type == type)
				{
					return true;
				}
			}
			foreach (InteractionObject.Multiplier multiplier in this.multipliers)
			{
				if (multiplier.result == type)
				{
					return true;
				}
			}
			return false;
		}

		public InteractionTarget[] GetTargets()
		{
			return this.targets;
		}

		public Transform GetTarget(FullBodyBipedEffector effectorType, string tag)
		{
			if (tag == string.Empty || tag == string.Empty)
			{
				return this.GetTarget(effectorType);
			}
			for (int i = 0; i < this.targets.Length; i++)
			{
				if (this.targets[i].effectorType == effectorType && this.targets[i].tag == tag)
				{
					return this.targets[i].transform;
				}
			}
			return base.transform;
		}

		public void OnStartInteraction(InteractionSystem interactionSystem)
		{
			this.lastUsedInteractionSystem = interactionSystem;
		}

		public void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionTarget target, float timer, float weight)
		{
			for (int i = 0; i < this.weightCurves.Length; i++)
			{
				float num = (!(target == null)) ? target.GetValue(this.weightCurves[i].type) : 1f;
				this.Apply(solver, effector, this.weightCurves[i].type, this.weightCurves[i].GetValue(timer), weight * num);
			}
			for (int j = 0; j < this.multipliers.Length; j++)
			{
				if (this.multipliers[j].curve == this.multipliers[j].result && !Warning.logged)
				{
					Warning.Log("InteractionObject Multiplier 'Curve' " + this.multipliers[j].curve.ToString() + "and 'Result' are the same.", base.transform, false);
				}
				int weightCurveIndex = this.GetWeightCurveIndex(this.multipliers[j].curve);
				if (weightCurveIndex != -1)
				{
					float num2 = (!(target == null)) ? target.GetValue(this.multipliers[j].result) : 1f;
					this.Apply(solver, effector, this.multipliers[j].result, this.multipliers[j].GetValue(this.weightCurves[weightCurveIndex], timer), weight * num2);
				}
				else if (!Warning.logged)
				{
					Warning.Log("InteractionObject Multiplier curve " + this.multipliers[j].curve.ToString() + "does not exist.", base.transform, false);
				}
			}
		}

		public float GetValue(InteractionObject.WeightCurve.Type weightCurveType, InteractionTarget target, float timer)
		{
			int weightCurveIndex = this.GetWeightCurveIndex(weightCurveType);
			if (weightCurveIndex != -1)
			{
				float num = (!(target == null)) ? target.GetValue(weightCurveType) : 1f;
				return this.weightCurves[weightCurveIndex].GetValue(timer) * num;
			}
			for (int i = 0; i < this.multipliers.Length; i++)
			{
				if (this.multipliers[i].result == weightCurveType)
				{
					int weightCurveIndex2 = this.GetWeightCurveIndex(this.multipliers[i].curve);
					if (weightCurveIndex2 != -1)
					{
						float num2 = (!(target == null)) ? target.GetValue(this.multipliers[i].result) : 1f;
						return this.multipliers[i].GetValue(this.weightCurves[weightCurveIndex2], timer) * num2;
					}
				}
			}
			return 0f;
		}

		public Transform targetsRoot
		{
			get
			{
				if (this.otherTargetsRoot != null)
				{
					return this.otherTargetsRoot;
				}
				return base.transform;
			}
		}

		private void Awake()
		{
			this.Initiate();
		}

		private void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionObject.WeightCurve.Type type, float value, float weight)
		{
			switch (type)
			{
			case InteractionObject.WeightCurve.Type.PositionWeight:
				solver.GetEffector(effector).positionWeight = Mathf.Lerp(solver.GetEffector(effector).positionWeight, value, weight);
				return;
			case InteractionObject.WeightCurve.Type.RotationWeight:
				solver.GetEffector(effector).rotationWeight = Mathf.Lerp(solver.GetEffector(effector).rotationWeight, value, weight);
				return;
			case InteractionObject.WeightCurve.Type.PositionOffsetX:
				solver.GetEffector(effector).position += ((!(this.positionOffsetSpace != null)) ? solver.GetRoot().rotation : this.positionOffsetSpace.rotation) * Vector3.right * value * weight;
				return;
			case InteractionObject.WeightCurve.Type.PositionOffsetY:
				solver.GetEffector(effector).position += ((!(this.positionOffsetSpace != null)) ? solver.GetRoot().rotation : this.positionOffsetSpace.rotation) * Vector3.up * value * weight;
				return;
			case InteractionObject.WeightCurve.Type.PositionOffsetZ:
				solver.GetEffector(effector).position += ((!(this.positionOffsetSpace != null)) ? solver.GetRoot().rotation : this.positionOffsetSpace.rotation) * Vector3.forward * value * weight;
				return;
			case InteractionObject.WeightCurve.Type.Pull:
				solver.GetChain(effector).pull = Mathf.Lerp(solver.GetChain(effector).pull, value, weight);
				return;
			case InteractionObject.WeightCurve.Type.Reach:
				solver.GetChain(effector).reach = Mathf.Lerp(solver.GetChain(effector).reach, value, weight);
				return;
			case InteractionObject.WeightCurve.Type.Push:
				solver.GetChain(effector).push = Mathf.Lerp(solver.GetChain(effector).push, value, weight);
				return;
			case InteractionObject.WeightCurve.Type.PushParent:
				solver.GetChain(effector).pushParent = Mathf.Lerp(solver.GetChain(effector).pushParent, value, weight);
				return;
			}
		}

		private Transform GetTarget(FullBodyBipedEffector effectorType)
		{
			for (int i = 0; i < this.targets.Length; i++)
			{
				if (this.targets[i].effectorType == effectorType)
				{
					return this.targets[i].transform;
				}
			}
			return base.transform;
		}

		private int GetWeightCurveIndex(InteractionObject.WeightCurve.Type weightCurveType)
		{
			for (int i = 0; i < this.weightCurves.Length; i++)
			{
				if (this.weightCurves[i].type == weightCurveType)
				{
					return i;
				}
			}
			return -1;
		}

		private int GetMultiplierIndex(InteractionObject.WeightCurve.Type weightCurveType)
		{
			for (int i = 0; i < this.multipliers.Length; i++)
			{
				if (this.multipliers[i].result == weightCurveType)
				{
					return i;
				}
			}
			return -1;
		}

		[ContextMenu("User Manual")]
		private void OpenUserManual()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
		}

		[ContextMenu("Scrpt Reference")]
		private void OpenScriptReference()
		{
			Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_object.html");
		}

		[Tooltip("If the Interaction System has a 'Look At' LookAtIK component assigned, will use it to make the character look at the specified Transform. If unassigned, will look at this GameObject.")]
		public Transform otherLookAtTarget;

		[Tooltip("The root Transform of the InteractionTargets. If null, will use this GameObject. GetComponentsInChildren<InteractionTarget>() will be used at initiation to find all InteractionTargets associated with this InteractionObject.")]
		public Transform otherTargetsRoot;

		[Tooltip("If assigned, all PositionOffset channels will be applied in the rotation space of this Transform. If not, they will be in the rotation space of the character.")]
		public Transform positionOffsetSpace;

		public InteractionObject.WeightCurve[] weightCurves;

		public InteractionObject.Multiplier[] multipliers;

		public InteractionObject.InteractionEvent[] events;

		private InteractionTarget[] targets = new InteractionTarget[0];

		[Serializable]
		public class InteractionEvent
		{
			public void Activate(Transform t)
			{
				foreach (InteractionObject.AnimatorEvent animatorEvent in this.animations)
				{
					animatorEvent.Activate(this.pickUp);
				}
				foreach (InteractionObject.Message message in this.messages)
				{
					message.Send(t);
				}
			}

			[Tooltip("The time of the event since interaction start.")]
			public float time;

			[Tooltip("If true, the interaction will be paused on this event. The interaction can be resumed by InteractionSystem.ResumeInteraction() or InteractionSystem.ResumeAll;")]
			public bool pause;

			[Tooltip("If true, the object will be parented to the effector bone on this event. Note that picking up like this can be done by only a single effector at a time. If you wish to pick up an object with both hands, see the Interaction PickUp2Handed demo scene.")]
			public bool pickUp;

			[Tooltip("The animations called on this event.")]
			public InteractionObject.AnimatorEvent[] animations;

			[Tooltip("The messages sent on this event using GameObject.SendMessage().")]
			public InteractionObject.Message[] messages;
		}

		[Serializable]
		public class Message
		{
			public void Send(Transform t)
			{
				if (this.recipient == null)
				{
					return;
				}
				if (this.function == string.Empty || this.function == string.Empty)
				{
					return;
				}
				this.recipient.SendMessage(this.function, t, SendMessageOptions.RequireReceiver);
			}

			[Tooltip("The name of the function called.")]
			public string function;

			[Tooltip("The recipient game object.")]
			public GameObject recipient;

			private const string empty = "";
		}

		[Serializable]
		public class AnimatorEvent
		{
			public void Activate(bool pickUp)
			{
				if (this.animator != null)
				{
					if (pickUp)
					{
						this.animator.applyRootMotion = false;
					}
					this.Activate(this.animator);
				}
				if (this.animation != null)
				{
					this.Activate(this.animation);
				}
			}

			private void Activate(Animator animator)
			{
				if (this.animationState == string.Empty)
				{
					return;
				}
				if (this.resetNormalizedTime)
				{
					animator.CrossFade(this.animationState, this.crossfadeTime, this.layer, 0f);
				}
				else
				{
					animator.CrossFade(this.animationState, this.crossfadeTime, this.layer);
				}
			}

			private void Activate(Animation animation)
			{
				if (this.animationState == string.Empty)
				{
					return;
				}
				if (this.resetNormalizedTime)
				{
					animation[this.animationState].normalizedTime = 0f;
				}
				animation[this.animationState].layer = this.layer;
				animation.CrossFade(this.animationState, this.crossfadeTime);
			}

			[Tooltip("The Animator component that will receive the AnimatorEvents.")]
			public Animator animator;

			[Tooltip("The Animation component that will receive the AnimatorEvents (Legacy).")]
			public Animation animation;

			[Tooltip("The name of the animation state.")]
			public string animationState;

			[Tooltip("The crossfading time.")]
			public float crossfadeTime = 0.3f;

			[Tooltip("The layer of the animation state (if using Legacy, the animation state will be forced to this layer).")]
			public int layer;

			[Tooltip("Should the animation always start from 0 normalized time?")]
			public bool resetNormalizedTime;

			private const string empty = "";
		}

		[Serializable]
		public class WeightCurve
		{
			public float GetValue(float timer)
			{
				return this.curve.Evaluate(timer);
			}

			[Tooltip("The type of the curve (InteractionObject.WeightCurve.Type).")]
			public InteractionObject.WeightCurve.Type type;

			[Tooltip("The weight curve.")]
			public AnimationCurve curve;

			[Serializable]
			public enum Type
			{
				PositionWeight,
				RotationWeight,
				PositionOffsetX,
				PositionOffsetY,
				PositionOffsetZ,
				Pull,
				Reach,
				RotateBoneWeight,
				Push,
				PushParent,
				PoserWeight
			}
		}

		[Serializable]
		public class Multiplier
		{
			public float GetValue(InteractionObject.WeightCurve weightCurve, float timer)
			{
				return weightCurve.GetValue(timer) * this.multiplier;
			}

			[Tooltip("The curve type to multiply.")]
			public InteractionObject.WeightCurve.Type curve;

			[Tooltip("The multiplier of the curve's value.")]
			public float multiplier = 1f;

			[Tooltip("The resulting value will be applied to this channel.")]
			public InteractionObject.WeightCurve.Type result;
		}
	}
}
