using System;
using Cinemachine.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[DocumentationSorting(6f, DocumentationSortingAttribute.Level.UserRef)]
	[RequireComponent(typeof(CinemachinePipeline))]
	[SaveDuringPlay]
	[AddComponentMenu("")]
	public class CinemachineOrbitalTransposer : CinemachineTransposer
	{
		protected override void OnValidate()
		{
			if (this.m_LegacyRadius != 3.40282347E+38f && this.m_LegacyHeightOffset != 3.40282347E+38f && this.m_LegacyHeadingBias != 3.40282347E+38f)
			{
				this.m_FollowOffset = new Vector3(0f, this.m_LegacyHeightOffset, -this.m_LegacyRadius);
				this.m_LegacyHeightOffset = (this.m_LegacyRadius = float.MaxValue);
				this.m_Heading.m_HeadingBias = this.m_LegacyHeadingBias;
				this.m_XAxis.m_MaxSpeed = this.m_XAxis.m_MaxSpeed / 10f;
				this.m_XAxis.m_AccelTime = this.m_XAxis.m_AccelTime / 10f;
				this.m_XAxis.m_DecelTime = this.m_XAxis.m_DecelTime / 10f;
				this.m_LegacyHeadingBias = float.MaxValue;
				this.m_RecenterToTargetHeading.LegacyUpgrade(ref this.m_Heading.m_HeadingDefinition, ref this.m_Heading.m_VelocityFilterStrength);
			}
			this.m_XAxis.Validate();
			this.m_RecenterToTargetHeading.Validate();
			base.OnValidate();
		}

		public float UpdateHeading(float deltaTime, Vector3 up, ref AxisState axis)
		{
			if (deltaTime >= 0f || CinemachineCore.Instance.IsLive(base.VirtualCamera))
			{
				bool flag = false;
				flag |= axis.Update(deltaTime);
				if (flag)
				{
					this.mLastHeadingAxisInputTime = Time.time;
					this.mHeadingRecenteringVelocity = 0f;
				}
			}
			float targetHeading = this.GetTargetHeading(axis.Value, base.GetReferenceOrientation(up), deltaTime);
			if (deltaTime < 0f)
			{
				this.mHeadingRecenteringVelocity = 0f;
				if (this.m_RecenterToTargetHeading.m_enabled)
				{
					axis.Value = targetHeading;
				}
			}
			else if (this.m_BindingMode != CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp && this.m_RecenterToTargetHeading.m_enabled && Time.time > this.mLastHeadingAxisInputTime + this.m_RecenterToTargetHeading.m_RecenterWaitTime)
			{
				float num = this.m_RecenterToTargetHeading.m_RecenteringTime / 3f;
				if (num <= deltaTime)
				{
					axis.Value = targetHeading;
				}
				else
				{
					float f = Mathf.DeltaAngle(axis.Value, targetHeading);
					float num2 = Mathf.Abs(f);
					if (num2 < 0.0001f)
					{
						axis.Value = targetHeading;
						this.mHeadingRecenteringVelocity = 0f;
					}
					else
					{
						float num3 = deltaTime / num;
						float num4 = Mathf.Sign(f) * Mathf.Min(num2, num2 * num3);
						float num5 = num4 - this.mHeadingRecenteringVelocity;
						if ((num4 < 0f && num5 < 0f) || (num4 > 0f && num5 > 0f))
						{
							num4 = this.mHeadingRecenteringVelocity + num4 * num3;
						}
						axis.Value += num4;
						this.mHeadingRecenteringVelocity = num4;
					}
				}
			}
			float value = axis.Value;
			if (this.m_BindingMode == CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
			{
				axis.Value = 0f;
			}
			return value;
		}

		private void OnEnable()
		{
			this.m_XAxis.SetThresholds(0f, 360f, true);
			this.PreviousTarget = null;
			this.mLastTargetPosition = Vector3.zero;
		}

		private Transform PreviousTarget { get; set; }

		public override void MutateCameraState(ref CameraState curState, float deltaTime)
		{
			base.InitPrevFrameStateInfo(ref curState, deltaTime);
			if (base.FollowTarget != this.PreviousTarget)
			{
				this.PreviousTarget = base.FollowTarget;
				this.mTargetRigidBody = ((!(this.PreviousTarget == null)) ? this.PreviousTarget.GetComponent<Rigidbody>() : null);
				this.mLastTargetPosition = ((!(this.PreviousTarget == null)) ? this.PreviousTarget.position : Vector3.zero);
				this.mHeadingTracker = null;
			}
			float num = this.HeadingUpdater(this, deltaTime, curState.ReferenceUp);
			if (this.IsValid)
			{
				this.mLastTargetPosition = base.FollowTarget.position;
				if (this.m_BindingMode != CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
				{
					num += this.m_Heading.m_HeadingBias;
				}
				Quaternion quaternion = Quaternion.AngleAxis(num, curState.ReferenceUp);
				Vector3 effectiveOffset = base.EffectiveOffset;
				Vector3 a;
				Quaternion quaternion2;
				base.TrackTarget(deltaTime, curState.ReferenceUp, quaternion * effectiveOffset, out a, out quaternion2);
				curState.ReferenceUp = quaternion2 * Vector3.up;
				if (deltaTime >= 0f)
				{
					Vector3 vector = quaternion * effectiveOffset - this.mHeadingPrevFrame * this.mOffsetPrevFrame;
					vector = quaternion2 * vector;
					curState.PositionDampingBypass = vector;
				}
				quaternion2 *= quaternion;
				curState.RawPosition = a + quaternion2 * effectiveOffset;
				this.mHeadingPrevFrame = ((this.m_BindingMode != CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp) ? quaternion : Quaternion.identity);
				this.mOffsetPrevFrame = effectiveOffset;
			}
		}

		public override void OnPositionDragged(Vector3 delta)
		{
			Quaternion referenceOrientation = base.GetReferenceOrientation(base.VcamState.ReferenceUp);
			Vector3 b = Quaternion.Inverse(referenceOrientation) * delta;
			b.x = 0f;
			this.m_FollowOffset += b;
			this.m_FollowOffset = base.EffectiveOffset;
		}

		private static string GetFullName(GameObject current)
		{
			if (current == null)
			{
				return string.Empty;
			}
			if (current.transform.parent == null)
			{
				return "/" + current.name;
			}
			return CinemachineOrbitalTransposer.GetFullName(current.transform.parent.gameObject) + "/" + current.name;
		}

		private float GetTargetHeading(float currentHeading, Quaternion targetOrientation, float deltaTime)
		{
			if (this.m_BindingMode == CinemachineTransposer.BindingMode.SimpleFollowWithWorldUp)
			{
				return 0f;
			}
			if (base.FollowTarget == null)
			{
				return currentHeading;
			}
			if (this.m_Heading.m_HeadingDefinition == CinemachineOrbitalTransposer.Heading.HeadingDefinition.Velocity && this.mTargetRigidBody == null)
			{
				Debug.Log(string.Format("Attempted to use HeadingDerivationMode.Velocity to calculate heading for {0}. No RigidBody was present on '{1}'. Defaulting to position delta", CinemachineOrbitalTransposer.GetFullName(base.VirtualCamera.VirtualCameraGameObject), base.FollowTarget));
				this.m_Heading.m_HeadingDefinition = CinemachineOrbitalTransposer.Heading.HeadingDefinition.PositionDelta;
			}
			Vector3 vector = Vector3.zero;
			switch (this.m_Heading.m_HeadingDefinition)
			{
			case CinemachineOrbitalTransposer.Heading.HeadingDefinition.PositionDelta:
				vector = base.FollowTarget.position - this.mLastTargetPosition;
				goto IL_E9;
			case CinemachineOrbitalTransposer.Heading.HeadingDefinition.Velocity:
				vector = this.mTargetRigidBody.velocity;
				goto IL_E9;
			case CinemachineOrbitalTransposer.Heading.HeadingDefinition.TargetForward:
				vector = base.FollowTarget.forward;
				goto IL_E9;
			}
			return 0f;
			IL_E9:
			int num = this.m_Heading.m_VelocityFilterStrength * 5;
			if (this.mHeadingTracker == null || this.mHeadingTracker.FilterSize != num)
			{
				this.mHeadingTracker = new CinemachineOrbitalTransposer.HeadingTracker(num);
			}
			this.mHeadingTracker.DecayHistory();
			Vector3 vector2 = targetOrientation * Vector3.up;
			vector = vector.ProjectOntoPlane(vector2);
			if (!vector.AlmostZero())
			{
				this.mHeadingTracker.Add(vector);
			}
			vector = this.mHeadingTracker.GetReliableHeading();
			if (!vector.AlmostZero())
			{
				return UnityVectorExtensions.SignedAngle(targetOrientation * Vector3.forward, vector, vector2);
			}
			return currentHeading;
		}

		[Tooltip("The definition of Forward.  Camera will follow behind.")]
		[Space]
		public CinemachineOrbitalTransposer.Heading m_Heading = new CinemachineOrbitalTransposer.Heading(CinemachineOrbitalTransposer.Heading.HeadingDefinition.TargetForward, 4, 0f);

		[Tooltip("Automatic heading recentering.  The settings here defines how the camera will reposition itself in the absence of player input.")]
		public CinemachineOrbitalTransposer.Recentering m_RecenterToTargetHeading = new CinemachineOrbitalTransposer.Recentering(true, 1f, 2f);

		[Tooltip("Heading Control.  The settings here control the behaviour of the camera in response to the player's input.")]
		public AxisState m_XAxis = new AxisState(300f, 2f, 1f, 0f, "Mouse X", true);

		[FormerlySerializedAs("m_Radius")]
		[HideInInspector]
		[SerializeField]
		private float m_LegacyRadius = float.MaxValue;

		[SerializeField]
		[FormerlySerializedAs("m_HeightOffset")]
		[HideInInspector]
		private float m_LegacyHeightOffset = float.MaxValue;

		[FormerlySerializedAs("m_HeadingBias")]
		[HideInInspector]
		[SerializeField]
		private float m_LegacyHeadingBias = float.MaxValue;

		[NoSaveDuringPlay]
		[HideInInspector]
		public bool m_HeadingIsSlave;

		internal CinemachineOrbitalTransposer.UpdateHeadingDelegate HeadingUpdater = (CinemachineOrbitalTransposer orbital, float deltaTime, Vector3 up) => orbital.UpdateHeading(deltaTime, up, ref orbital.m_XAxis);

		private float mLastHeadingAxisInputTime;

		private float mHeadingRecenteringVelocity;

		private Vector3 mLastTargetPosition = Vector3.zero;

		private CinemachineOrbitalTransposer.HeadingTracker mHeadingTracker;

		private Rigidbody mTargetRigidBody;

		private Quaternion mHeadingPrevFrame = Quaternion.identity;

		private Vector3 mOffsetPrevFrame = Vector3.zero;

		[DocumentationSorting(6.2f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct Heading
		{
			public Heading(CinemachineOrbitalTransposer.Heading.HeadingDefinition def, int filterStrength, float bias)
			{
				this.m_HeadingDefinition = def;
				this.m_VelocityFilterStrength = filterStrength;
				this.m_HeadingBias = bias;
			}

			[Tooltip("How 'forward' is defined.  The camera will be placed by default behind the target.  PositionDelta will consider 'forward' to be the direction in which the target is moving.")]
			public CinemachineOrbitalTransposer.Heading.HeadingDefinition m_HeadingDefinition;

			[Tooltip("Size of the velocity sampling window for target heading filter.  This filters out irregularities in the target's movement.  Used only if deriving heading from target's movement (PositionDelta or Velocity)")]
			[Range(0f, 10f)]
			public int m_VelocityFilterStrength;

			[Tooltip("Where the camera is placed when the X-axis value is zero.  This is a rotation in degrees around the Y axis.  When this value is 0, the camera will be placed behind the target.  Nonzero offsets will rotate the zero position around the target.")]
			[Range(-180f, 180f)]
			public float m_HeadingBias;

			[DocumentationSorting(6.21f, DocumentationSortingAttribute.Level.UserRef)]
			public enum HeadingDefinition
			{
				PositionDelta,
				Velocity,
				TargetForward,
				WorldForward
			}
		}

		[DocumentationSorting(6.5f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct Recentering
		{
			public Recentering(bool enabled, float recenterWaitTime, float recenteringSpeed)
			{
				this.m_enabled = enabled;
				this.m_RecenterWaitTime = recenterWaitTime;
				this.m_RecenteringTime = recenteringSpeed;
				this.m_LegacyHeadingDefinition = (this.m_LegacyVelocityFilterStrength = -1);
			}

			public void Validate()
			{
				this.m_RecenterWaitTime = Mathf.Max(0f, this.m_RecenterWaitTime);
				this.m_RecenteringTime = Mathf.Max(0f, this.m_RecenteringTime);
			}

			internal bool LegacyUpgrade(ref CinemachineOrbitalTransposer.Heading.HeadingDefinition heading, ref int velocityFilter)
			{
				if (this.m_LegacyHeadingDefinition != -1 && this.m_LegacyVelocityFilterStrength != -1)
				{
					heading = (CinemachineOrbitalTransposer.Heading.HeadingDefinition)this.m_LegacyHeadingDefinition;
					velocityFilter = this.m_LegacyVelocityFilterStrength;
					this.m_LegacyHeadingDefinition = (this.m_LegacyVelocityFilterStrength = -1);
					return true;
				}
				return false;
			}

			[Tooltip("If checked, will enable automatic recentering of the camera based on the heading definition. If unchecked, recenting is disabled.")]
			public bool m_enabled;

			[Tooltip("If no input has been detected, the camera will wait this long in seconds before moving its heading to the zero position.")]
			public float m_RecenterWaitTime;

			[Tooltip("Maximum angular speed of recentering.  Will accelerate into and decelerate out of this.")]
			public float m_RecenteringTime;

			[FormerlySerializedAs("m_HeadingDefinition")]
			[SerializeField]
			[HideInInspector]
			private int m_LegacyHeadingDefinition;

			[FormerlySerializedAs("m_VelocityFilterStrength")]
			[SerializeField]
			[HideInInspector]
			private int m_LegacyVelocityFilterStrength;
		}

		internal delegate float UpdateHeadingDelegate(CinemachineOrbitalTransposer orbital, float deltaTime, Vector3 up);

		private class HeadingTracker
		{
			public HeadingTracker(int filterSize)
			{
				this.mHistory = new CinemachineOrbitalTransposer.HeadingTracker.Item[filterSize];
				float num = (float)filterSize / 5f;
				CinemachineOrbitalTransposer.HeadingTracker.mDecayExponent = -Mathf.Log(2f) / num;
				this.ClearHistory();
			}

			public int FilterSize
			{
				get
				{
					return this.mHistory.Length;
				}
			}

			private void ClearHistory()
			{
				this.mTop = (this.mBottom = (this.mCount = 0));
				this.mWeightSum = 0f;
				this.mHeadingSum = Vector3.zero;
			}

			private static float Decay(float time)
			{
				return Mathf.Exp(time * CinemachineOrbitalTransposer.HeadingTracker.mDecayExponent);
			}

			public void Add(Vector3 velocity)
			{
				if (this.FilterSize == 0)
				{
					this.mLastGoodHeading = velocity;
					return;
				}
				float magnitude = velocity.magnitude;
				if (magnitude > 0.0001f)
				{
					CinemachineOrbitalTransposer.HeadingTracker.Item item = default(CinemachineOrbitalTransposer.HeadingTracker.Item);
					item.velocity = velocity;
					item.weight = magnitude;
					item.time = Time.time;
					if (this.mCount == this.FilterSize)
					{
						this.PopBottom();
					}
					this.mCount++;
					this.mHistory[this.mTop] = item;
					if (++this.mTop == this.FilterSize)
					{
						this.mTop = 0;
					}
					this.mWeightSum *= CinemachineOrbitalTransposer.HeadingTracker.Decay(item.time - this.mWeightTime);
					this.mWeightTime = item.time;
					this.mWeightSum += magnitude;
					this.mHeadingSum += item.velocity;
				}
			}

			private void PopBottom()
			{
				if (this.mCount > 0)
				{
					float time = Time.time;
					CinemachineOrbitalTransposer.HeadingTracker.Item item = this.mHistory[this.mBottom];
					if (++this.mBottom == this.FilterSize)
					{
						this.mBottom = 0;
					}
					this.mCount--;
					float num = CinemachineOrbitalTransposer.HeadingTracker.Decay(time - item.time);
					this.mWeightSum -= item.weight * num;
					this.mHeadingSum -= item.velocity * num;
					if (this.mWeightSum <= 0.0001f || this.mCount == 0)
					{
						this.ClearHistory();
					}
				}
			}

			public void DecayHistory()
			{
				float time = Time.time;
				float num = CinemachineOrbitalTransposer.HeadingTracker.Decay(time - this.mWeightTime);
				this.mWeightSum *= num;
				this.mWeightTime = time;
				if (this.mWeightSum < 0.0001f)
				{
					this.ClearHistory();
				}
				else
				{
					this.mHeadingSum *= num;
				}
			}

			public Vector3 GetReliableHeading()
			{
				if (this.mWeightSum > 0.0001f && (this.mCount == this.mHistory.Length || this.mLastGoodHeading.AlmostZero()))
				{
					Vector3 v = this.mHeadingSum / this.mWeightSum;
					if (!v.AlmostZero())
					{
						this.mLastGoodHeading = v.normalized;
					}
				}
				return this.mLastGoodHeading;
			}

			private CinemachineOrbitalTransposer.HeadingTracker.Item[] mHistory;

			private int mTop;

			private int mBottom;

			private int mCount;

			private Vector3 mHeadingSum;

			private float mWeightSum;

			private float mWeightTime;

			private Vector3 mLastGoodHeading = Vector3.zero;

			private static float mDecayExponent;

			private struct Item
			{
				public Vector3 velocity;

				public float weight;

				public float time;
			}
		}
	}
}
