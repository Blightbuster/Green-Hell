using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cinemachine
{
	[DocumentationSorting(6.4f, DocumentationSortingAttribute.Level.UserRef)]
	[Serializable]
	public struct AxisState
	{
		public AxisState(float maxSpeed, float accelTime, float decelTime, float val, string name, bool invert)
		{
			this.m_MaxSpeed = maxSpeed;
			this.m_AccelTime = accelTime;
			this.m_DecelTime = decelTime;
			this.Value = val;
			this.m_InputAxisName = name;
			this.m_InputAxisValue = 0f;
			this.m_InvertAxis = invert;
			this.mCurrentSpeed = 0f;
			this.mMinValue = 0f;
			this.mMaxValue = 0f;
			this.mWrapAround = false;
		}

		public void Validate()
		{
			this.m_MaxSpeed = Mathf.Max(0f, this.m_MaxSpeed);
			this.m_AccelTime = Mathf.Max(0f, this.m_AccelTime);
			this.m_DecelTime = Mathf.Max(0f, this.m_DecelTime);
		}

		public void SetThresholds(float minValue, float maxValue, bool wrapAround)
		{
			this.mMinValue = minValue;
			this.mMaxValue = maxValue;
			this.mWrapAround = wrapAround;
		}

		public bool Update(float deltaTime)
		{
			if (!string.IsNullOrEmpty(this.m_InputAxisName))
			{
				try
				{
					this.m_InputAxisValue = CinemachineCore.GetInputAxis(this.m_InputAxisName);
				}
				catch (ArgumentException ex)
				{
					Debug.LogError(ex.ToString());
				}
			}
			float num = this.m_InputAxisValue;
			if (this.m_InvertAxis)
			{
				num *= -1f;
			}
			if (this.m_MaxSpeed > 0.0001f)
			{
				float num2 = num * this.m_MaxSpeed;
				if (Mathf.Abs(num2) < 0.0001f || (Mathf.Sign(this.mCurrentSpeed) == Mathf.Sign(num2) && Mathf.Abs(num2) < Mathf.Abs(this.mCurrentSpeed)))
				{
					float num3 = Mathf.Min(Mathf.Abs(num2 - this.mCurrentSpeed) / Mathf.Max(0.0001f, this.m_DecelTime) * deltaTime, Mathf.Abs(this.mCurrentSpeed));
					this.mCurrentSpeed -= Mathf.Sign(this.mCurrentSpeed) * num3;
				}
				else
				{
					float num4 = Mathf.Abs(num2 - this.mCurrentSpeed) / Mathf.Max(0.0001f, this.m_AccelTime);
					this.mCurrentSpeed += Mathf.Sign(num2) * num4 * deltaTime;
					if (Mathf.Sign(this.mCurrentSpeed) == Mathf.Sign(num2) && Mathf.Abs(this.mCurrentSpeed) > Mathf.Abs(num2))
					{
						this.mCurrentSpeed = num2;
					}
				}
			}
			float maxSpeed = this.GetMaxSpeed();
			this.mCurrentSpeed = Mathf.Clamp(this.mCurrentSpeed, -maxSpeed, maxSpeed);
			this.Value += this.mCurrentSpeed * deltaTime;
			if (this.Value > this.mMaxValue || this.Value < this.mMinValue)
			{
				if (this.mWrapAround)
				{
					if (this.Value > this.mMaxValue)
					{
						this.Value = this.mMinValue + (this.Value - this.mMaxValue);
					}
					else
					{
						this.Value = this.mMaxValue + (this.Value - this.mMinValue);
					}
				}
				else
				{
					this.Value = Mathf.Clamp(this.Value, this.mMinValue, this.mMaxValue);
					this.mCurrentSpeed = 0f;
				}
			}
			return Mathf.Abs(num) > 0.0001f;
		}

		private float GetMaxSpeed()
		{
			float num = this.mMaxValue - this.mMinValue;
			if (!this.mWrapAround && num > 0f)
			{
				float num2 = num / 10f;
				if (this.mCurrentSpeed > 0f && this.mMaxValue - this.Value < num2)
				{
					float t = (this.mMaxValue - this.Value) / num2;
					return Mathf.Lerp(0f, this.m_MaxSpeed, t);
				}
				if (this.mCurrentSpeed < 0f && this.Value - this.mMinValue < num2)
				{
					float t2 = (this.Value - this.mMinValue) / num2;
					return Mathf.Lerp(0f, this.m_MaxSpeed, t2);
				}
			}
			return this.m_MaxSpeed;
		}

		[NoSaveDuringPlay]
		[Tooltip("The current value of the axis.")]
		public float Value;

		[Tooltip("The maximum speed of this axis in units/second")]
		public float m_MaxSpeed;

		[Tooltip("The amount of time in seconds it takes to accelerate to MaxSpeed with the supplied Axis at its maximum value")]
		public float m_AccelTime;

		[Tooltip("The amount of time in seconds it takes to decelerate the axis to zero if the supplied axis is in a neutral position")]
		public float m_DecelTime;

		[FormerlySerializedAs("m_AxisName")]
		[Tooltip("The name of this axis as specified in Unity Input manager. Setting to an empty string will disable the automatic updating of this axis")]
		public string m_InputAxisName;

		[NoSaveDuringPlay]
		[Tooltip("The value of the input axis.  A value of 0 means no input.  You can drive this directly from a custom input system, or you can set the Axis Name and have the value driven by the internal Input Manager")]
		public float m_InputAxisValue;

		[NoSaveDuringPlay]
		[Tooltip("If checked, then the raw value of the input axis will be inverted before it is used")]
		public bool m_InvertAxis;

		private float mCurrentSpeed;

		private float mMinValue;

		private float mMaxValue;

		private bool mWrapAround;

		private const float Epsilon = 0.0001f;
	}
}
