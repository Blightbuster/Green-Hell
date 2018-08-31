using System;
using UnityEngine;

namespace UltimateWater
{
	public interface IWaterDisplacements
	{
		Vector3 GetDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

		Vector2 GetHorizontalDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

		float GetHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

		Vector4 GetForceAndHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

		float MaxVerticalDisplacement { get; }

		float MaxHorizontalDisplacement { get; }
	}
}
