using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[Serializable]
	public class InteractionEffector
	{
		public InteractionEffector(FullBodyBipedEffector effectorType)
		{
			this.effectorType = effectorType;
		}

		public FullBodyBipedEffector effectorType { get; private set; }

		public bool isPaused { get; private set; }

		public InteractionObject interactionObject { get; private set; }

		public bool inInteraction
		{
			get
			{
				return this.interactionObject != null;
			}
		}

		public void Initiate(InteractionSystem interactionSystem)
		{
			this.interactionSystem = interactionSystem;
			if (this.effector == null)
			{
				this.effector = interactionSystem.ik.solver.GetEffector(this.effectorType);
				this.poser = this.effector.bone.GetComponent<Poser>();
			}
			this.StoreDefaults();
		}

		private void StoreDefaults()
		{
			this.defaultPositionWeight = this.interactionSystem.ik.solver.GetEffector(this.effectorType).positionWeight;
			this.defaultRotationWeight = this.interactionSystem.ik.solver.GetEffector(this.effectorType).rotationWeight;
			this.defaultPull = this.interactionSystem.ik.solver.GetChain(this.effectorType).pull;
			this.defaultReach = this.interactionSystem.ik.solver.GetChain(this.effectorType).reach;
			this.defaultPush = this.interactionSystem.ik.solver.GetChain(this.effectorType).push;
			this.defaultPushParent = this.interactionSystem.ik.solver.GetChain(this.effectorType).pushParent;
		}

		public bool ResetToDefaults(float speed)
		{
			if (this.inInteraction)
			{
				return false;
			}
			if (this.isPaused)
			{
				return false;
			}
			if (this.defaults)
			{
				return false;
			}
			this.resetTimer = Mathf.Clamp(this.resetTimer -= Time.deltaTime * speed, 0f, 1f);
			if (this.effector.isEndEffector)
			{
				if (this.pullUsed)
				{
					this.interactionSystem.ik.solver.GetChain(this.effectorType).pull = Mathf.Lerp(this.defaultPull, this.interactionSystem.ik.solver.GetChain(this.effectorType).pull, this.resetTimer);
				}
				if (this.reachUsed)
				{
					this.interactionSystem.ik.solver.GetChain(this.effectorType).reach = Mathf.Lerp(this.defaultReach, this.interactionSystem.ik.solver.GetChain(this.effectorType).reach, this.resetTimer);
				}
				if (this.pushUsed)
				{
					this.interactionSystem.ik.solver.GetChain(this.effectorType).push = Mathf.Lerp(this.defaultPush, this.interactionSystem.ik.solver.GetChain(this.effectorType).push, this.resetTimer);
				}
				if (this.pushParentUsed)
				{
					this.interactionSystem.ik.solver.GetChain(this.effectorType).pushParent = Mathf.Lerp(this.defaultPushParent, this.interactionSystem.ik.solver.GetChain(this.effectorType).pushParent, this.resetTimer);
				}
			}
			if (this.positionWeightUsed)
			{
				this.effector.positionWeight = Mathf.Lerp(this.defaultPositionWeight, this.effector.positionWeight, this.resetTimer);
			}
			if (this.rotationWeightUsed)
			{
				this.effector.rotationWeight = Mathf.Lerp(this.defaultRotationWeight, this.effector.rotationWeight, this.resetTimer);
			}
			if (this.resetTimer <= 0f)
			{
				this.pullUsed = false;
				this.reachUsed = false;
				this.pushUsed = false;
				this.pushParentUsed = false;
				this.positionWeightUsed = false;
				this.rotationWeightUsed = false;
				this.defaults = true;
			}
			return true;
		}

		public bool Pause()
		{
			if (!this.inInteraction)
			{
				return false;
			}
			this.isPaused = true;
			this.pausePositionRelative = this.target.InverseTransformPoint(this.effector.position);
			this.pauseRotationRelative = Quaternion.Inverse(this.target.rotation) * this.effector.rotation;
			if (this.interactionSystem.OnInteractionPause != null)
			{
				this.interactionSystem.OnInteractionPause(this.effectorType, this.interactionObject);
			}
			return true;
		}

		public bool Resume()
		{
			if (!this.inInteraction)
			{
				return false;
			}
			this.isPaused = false;
			if (this.interactionSystem.OnInteractionResume != null)
			{
				this.interactionSystem.OnInteractionResume(this.effectorType, this.interactionObject);
			}
			return true;
		}

		public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt)
		{
			if (!this.inInteraction)
			{
				this.effector.position = this.effector.bone.position;
				this.effector.rotation = this.effector.bone.rotation;
			}
			else if (!interrupt)
			{
				return false;
			}
			this.target = interactionObject.GetTarget(this.effectorType, tag);
			if (this.target == null)
			{
				return false;
			}
			this.interactionTarget = this.target.GetComponent<InteractionTarget>();
			this.interactionObject = interactionObject;
			if (this.interactionSystem.OnInteractionStart != null)
			{
				this.interactionSystem.OnInteractionStart(this.effectorType, interactionObject);
			}
			interactionObject.OnStartInteraction(this.interactionSystem);
			this.triggered.Clear();
			for (int i = 0; i < interactionObject.events.Length; i++)
			{
				this.triggered.Add(false);
			}
			if (this.poser != null)
			{
				if (this.poser.poseRoot == null)
				{
					this.poser.weight = 0f;
				}
				if (this.interactionTarget != null)
				{
					this.poser.poseRoot = this.target.transform;
				}
				else
				{
					this.poser.poseRoot = null;
				}
				this.poser.AutoMapping();
			}
			this.positionWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PositionWeight);
			this.rotationWeightUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.RotationWeight);
			this.pullUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Pull);
			this.reachUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Reach);
			this.pushUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.Push);
			this.pushParentUsed = interactionObject.CurveUsed(InteractionObject.WeightCurve.Type.PushParent);
			this.StoreDefaults();
			this.timer = 0f;
			this.weight = 0f;
			this.fadeInSpeed = ((fadeInTime <= 0f) ? 1000f : (1f / fadeInTime));
			this.length = interactionObject.length;
			this.isPaused = false;
			this.pickedUp = false;
			this.pickUpPosition = Vector3.zero;
			this.pickUpRotation = Quaternion.identity;
			if (this.interactionTarget != null)
			{
				this.interactionTarget.RotateTo(this.effector.bone.position);
			}
			this.started = true;
			return true;
		}

		public void Update(Transform root, float speed)
		{
			if (!this.inInteraction)
			{
				if (this.started)
				{
					this.isPaused = false;
					this.pickedUp = false;
					this.defaults = false;
					this.resetTimer = 1f;
					this.started = false;
				}
				return;
			}
			if (this.interactionTarget != null && !this.interactionTarget.rotateOnce)
			{
				this.interactionTarget.RotateTo(this.effector.bone.position);
			}
			if (this.isPaused)
			{
				this.effector.position = this.target.TransformPoint(this.pausePositionRelative);
				this.effector.rotation = this.target.rotation * this.pauseRotationRelative;
				this.interactionObject.Apply(this.interactionSystem.ik.solver, this.effectorType, this.interactionTarget, this.timer, this.weight);
				return;
			}
			this.timer += Time.deltaTime * speed * ((!(this.interactionTarget != null)) ? 1f : this.interactionTarget.interactionSpeedMlp);
			this.weight = Mathf.Clamp(this.weight + Time.deltaTime * this.fadeInSpeed * speed, 0f, 1f);
			bool flag = false;
			bool flag2 = false;
			this.TriggerUntriggeredEvents(true, out flag, out flag2);
			Vector3 b = (!this.pickedUp) ? this.target.position : this.pickUpPosition;
			Quaternion b2 = (!this.pickedUp) ? this.target.rotation : this.pickUpRotation;
			this.effector.position = Vector3.Lerp(this.effector.bone.position, b, this.weight);
			this.effector.rotation = Quaternion.Lerp(this.effector.bone.rotation, b2, this.weight);
			this.interactionObject.Apply(this.interactionSystem.ik.solver, this.effectorType, this.interactionTarget, this.timer, this.weight);
			if (flag)
			{
				this.PickUp(root);
			}
			if (flag2)
			{
				this.Pause();
			}
			float value = this.interactionObject.GetValue(InteractionObject.WeightCurve.Type.PoserWeight, this.interactionTarget, this.timer);
			if (this.poser != null)
			{
				this.poser.weight = Mathf.Lerp(this.poser.weight, value, this.weight);
			}
			else if (value > 0f)
			{
				Warning.Log(string.Concat(new string[]
				{
					"InteractionObject ",
					this.interactionObject.name,
					" has a curve/multipler for Poser Weight, but the bone of effector ",
					this.effectorType.ToString(),
					" has no HandPoser/GenericPoser attached."
				}), this.effector.bone, false);
			}
			if (this.timer >= this.length)
			{
				this.Stop();
			}
		}

		public float progress
		{
			get
			{
				if (!this.inInteraction)
				{
					return 0f;
				}
				if (this.length == 0f)
				{
					return 0f;
				}
				return this.timer / this.length;
			}
		}

		private void TriggerUntriggeredEvents(bool checkTime, out bool pickUp, out bool pause)
		{
			pickUp = false;
			pause = false;
			for (int i = 0; i < this.triggered.Count; i++)
			{
				if (!this.triggered[i] && (!checkTime || this.interactionObject.events[i].time < this.timer))
				{
					this.interactionObject.events[i].Activate(this.effector.bone);
					if (this.interactionObject.events[i].pickUp)
					{
						if (this.timer >= this.interactionObject.events[i].time)
						{
							this.timer = this.interactionObject.events[i].time;
						}
						pickUp = true;
					}
					if (this.interactionObject.events[i].pause)
					{
						if (this.timer >= this.interactionObject.events[i].time)
						{
							this.timer = this.interactionObject.events[i].time;
						}
						pause = true;
					}
					if (this.interactionSystem.OnInteractionEvent != null)
					{
						this.interactionSystem.OnInteractionEvent(this.effectorType, this.interactionObject, this.interactionObject.events[i]);
					}
					this.triggered[i] = true;
				}
			}
		}

		private void PickUp(Transform root)
		{
			this.pickUpPosition = this.effector.position;
			this.pickUpRotation = this.effector.rotation;
			this.pickUpOnPostFBBIK = true;
			this.pickedUp = true;
			Rigidbody component = this.interactionObject.targetsRoot.GetComponent<Rigidbody>();
			if (component != null)
			{
				if (!component.isKinematic)
				{
					component.isKinematic = true;
				}
				if (root.GetComponent<Collider>() != null)
				{
					Collider[] componentsInChildren = this.interactionObject.targetsRoot.GetComponentsInChildren<Collider>();
					foreach (Collider collider in componentsInChildren)
					{
						if (!collider.isTrigger)
						{
							Physics.IgnoreCollision(root.GetComponent<Collider>(), collider);
						}
					}
				}
			}
			if (this.interactionSystem.OnInteractionPickUp != null)
			{
				this.interactionSystem.OnInteractionPickUp(this.effectorType, this.interactionObject);
			}
		}

		public bool Stop()
		{
			if (!this.inInteraction)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			this.TriggerUntriggeredEvents(false, out flag, out flag2);
			if (this.interactionSystem.OnInteractionStop != null)
			{
				this.interactionSystem.OnInteractionStop(this.effectorType, this.interactionObject);
			}
			if (this.interactionTarget != null)
			{
				this.interactionTarget.ResetRotation();
			}
			this.interactionObject = null;
			this.weight = 0f;
			this.timer = 0f;
			this.isPaused = false;
			this.target = null;
			this.defaults = false;
			this.resetTimer = 1f;
			if (this.poser != null && !this.pickedUp)
			{
				this.poser.weight = 0f;
			}
			this.pickedUp = false;
			this.started = false;
			return true;
		}

		public void OnPostFBBIK()
		{
			if (!this.inInteraction)
			{
				return;
			}
			float num = this.interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, this.interactionTarget, this.timer) * this.weight;
			if (num > 0f)
			{
				Quaternion b = (!this.pickedUp) ? this.effector.rotation : this.pickUpRotation;
				Quaternion rhs = Quaternion.Slerp(this.effector.bone.rotation, b, num * num);
				this.effector.bone.localRotation = Quaternion.Inverse(this.effector.bone.parent.rotation) * rhs;
			}
			if (this.pickUpOnPostFBBIK)
			{
				Vector3 position = this.effector.bone.position;
				this.effector.bone.position = this.pickUpPosition;
				this.interactionObject.targetsRoot.parent = this.effector.bone;
				this.effector.bone.position = position;
				this.pickUpOnPostFBBIK = false;
			}
		}

		private Poser poser;

		private IKEffector effector;

		private float timer;

		private float length;

		private float weight;

		private float fadeInSpeed;

		private float defaultPositionWeight;

		private float defaultRotationWeight;

		private float defaultPull;

		private float defaultReach;

		private float defaultPush;

		private float defaultPushParent;

		private float resetTimer;

		private bool positionWeightUsed;

		private bool rotationWeightUsed;

		private bool pullUsed;

		private bool reachUsed;

		private bool pushUsed;

		private bool pushParentUsed;

		private bool pickedUp;

		private bool defaults;

		private bool pickUpOnPostFBBIK;

		private Vector3 pickUpPosition;

		private Vector3 pausePositionRelative;

		private Quaternion pickUpRotation;

		private Quaternion pauseRotationRelative;

		private InteractionTarget interactionTarget;

		private Transform target;

		private List<bool> triggered = new List<bool>();

		private InteractionSystem interactionSystem;

		private bool started;
	}
}
