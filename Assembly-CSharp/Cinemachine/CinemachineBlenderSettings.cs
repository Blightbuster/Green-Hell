using System;
using UnityEngine;

namespace Cinemachine
{
	[DocumentationSorting(10f, DocumentationSortingAttribute.Level.UserRef)]
	[Serializable]
	public sealed class CinemachineBlenderSettings : ScriptableObject
	{
		public AnimationCurve GetBlendCurveForVirtualCameras(string fromCameraName, string toCameraName, AnimationCurve defaultCurve)
		{
			AnimationCurve animationCurve = null;
			AnimationCurve animationCurve2 = null;
			if (this.m_CustomBlends != null)
			{
				for (int i = 0; i < this.m_CustomBlends.Length; i++)
				{
					CinemachineBlenderSettings.CustomBlend customBlend = this.m_CustomBlends[i];
					if (customBlend.m_From == fromCameraName && customBlend.m_To == toCameraName)
					{
						return customBlend.m_Blend.BlendCurve;
					}
					if (customBlend.m_From == "**ANY CAMERA**")
					{
						if (!string.IsNullOrEmpty(toCameraName) && customBlend.m_To == toCameraName)
						{
							animationCurve = customBlend.m_Blend.BlendCurve;
						}
						else if (customBlend.m_To == "**ANY CAMERA**")
						{
							defaultCurve = customBlend.m_Blend.BlendCurve;
						}
					}
					else if (customBlend.m_To == "**ANY CAMERA**" && !string.IsNullOrEmpty(fromCameraName) && customBlend.m_From == fromCameraName)
					{
						animationCurve2 = customBlend.m_Blend.BlendCurve;
					}
				}
			}
			if (animationCurve != null)
			{
				return animationCurve;
			}
			if (animationCurve2 != null)
			{
				return animationCurve2;
			}
			return defaultCurve;
		}

		[Tooltip("The array containing explicitly defined blends between two Virtual Cameras")]
		public CinemachineBlenderSettings.CustomBlend[] m_CustomBlends;

		public const string kBlendFromAnyCameraLabel = "**ANY CAMERA**";

		[DocumentationSorting(10.1f, DocumentationSortingAttribute.Level.UserRef)]
		[Serializable]
		public struct CustomBlend
		{
			[Tooltip("When blending from this camera")]
			public string m_From;

			[Tooltip("When blending to this camera")]
			public string m_To;

			[Tooltip("Blend curve definition")]
			public CinemachineBlendDefinition m_Blend;
		}
	}
}
