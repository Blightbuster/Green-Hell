using System;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(10.2f, DocumentationSortingAttribute.Level.UserRef)]
	[Serializable]
	public struct CinemachineBlendDefinition
	{
		public CinemachineBlendDefinition(CinemachineBlendDefinition.Style style, float time)
		{
			this.m_Style = style;
			this.m_Time = time;
		}

		public AnimationCurve BlendCurve
		{
			get
			{
				float timeEnd = Mathf.Max(0f, this.m_Time);
				switch (this.m_Style)
				{
				default:
					return new AnimationCurve();
				case CinemachineBlendDefinition.Style.EaseInOut:
					return AnimationCurve.EaseInOut(0f, 0f, timeEnd, 1f);
				case CinemachineBlendDefinition.Style.EaseIn:
				{
					AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, timeEnd, 1f);
					Keyframe[] keys = animationCurve.keys;
					keys[1].inTangent = 0f;
					animationCurve.keys = keys;
					return animationCurve;
				}
				case CinemachineBlendDefinition.Style.EaseOut:
				{
					AnimationCurve animationCurve2 = AnimationCurve.Linear(0f, 0f, timeEnd, 1f);
					Keyframe[] keys2 = animationCurve2.keys;
					keys2[0].outTangent = 0f;
					animationCurve2.keys = keys2;
					return animationCurve2;
				}
				case CinemachineBlendDefinition.Style.HardIn:
				{
					AnimationCurve animationCurve3 = AnimationCurve.Linear(0f, 0f, timeEnd, 1f);
					Keyframe[] keys3 = animationCurve3.keys;
					keys3[0].outTangent = 0f;
					keys3[1].inTangent = 1.5708f;
					animationCurve3.keys = keys3;
					return animationCurve3;
				}
				case CinemachineBlendDefinition.Style.HardOut:
				{
					AnimationCurve animationCurve4 = AnimationCurve.Linear(0f, 0f, timeEnd, 1f);
					Keyframe[] keys4 = animationCurve4.keys;
					keys4[0].outTangent = 1.5708f;
					keys4[1].inTangent = 0f;
					animationCurve4.keys = keys4;
					return animationCurve4;
				}
				case CinemachineBlendDefinition.Style.Linear:
					return AnimationCurve.Linear(0f, 0f, timeEnd, 1f);
				}
			}
		}

		[Tooltip("Shape of the blend curve")]
		public CinemachineBlendDefinition.Style m_Style;

		[Tooltip("Duration of the blend, in seconds")]
		public float m_Time;

		[DocumentationSorting(10.21f, DocumentationSortingAttribute.Level.UserRef)]
		public enum Style
		{
			Cut,
			EaseInOut,
			EaseIn,
			EaseOut,
			HardIn,
			HardOut,
			Linear
		}
	}
}
