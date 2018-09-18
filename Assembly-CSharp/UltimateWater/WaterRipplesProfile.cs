using System;
using UnityEngine;

namespace UltimateWater
{
	public class WaterRipplesProfile : ScriptableObject
	{
		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			WaterSimulationArea[] array = UnityEngine.Object.FindObjectsOfType<WaterSimulationArea>();
			foreach (WaterSimulationArea waterSimulationArea in array)
			{
				waterSimulationArea.UpdateShaderVariables();
			}
		}

		[Header("Settings")]
		[Range(0f, 1f)]
		[Tooltip("How fast wave amplitude decreases with time")]
		public float Damping = 0.3f;

		[Range(0f, 1f)]
		[Tooltip("How fast the waves spread")]
		public float Propagation = 1f;

		[Tooltip("Force inflicted by interacting objects")]
		public float Gain = 30f;

		[Tooltip("Wave amplitude decrease with depth")]
		public float HeightGain = 2f;

		[Tooltip("Wave amplitude decrease offset")]
		public float HeightOffset = 2f;

		[Tooltip("Wave height multiplier")]
		public float Amplitude = 1f;

		[Tooltip("How much smoothing is applied between iterations")]
		[Range(0f, 1f)]
		[Header("Smooth")]
		public float Sigma;

		[Tooltip("How strong are wave normals")]
		[Header("Normals")]
		public float Multiplier = 1f;

		[Tooltip("How wide is sampling distance for normal calculations")]
		public float Spread = 0.001f;
	}
}
