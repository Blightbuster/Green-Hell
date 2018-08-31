using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public class GradientContainer : ScriptableObject
	{
		[FormerlySerializedAs("gradient")]
		public Gradient Gradient;
	}
}
