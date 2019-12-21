using System;
using UnityEngine;

public static class LuxWaterUtils
{
	public static void GetGersterWavesDescription(ref LuxWaterUtils.GersterWavesDescription Description, Material WaterMaterial)
	{
		Description.intensity = WaterMaterial.GetVector("_GerstnerVertexIntensity");
		Description.steepness = WaterMaterial.GetVector("_GSteepness");
		Description.amp = WaterMaterial.GetVector("_GAmplitude");
		Description.freq = WaterMaterial.GetVector("_GFinalFrequency");
		Description.speed = WaterMaterial.GetVector("_GFinalSpeed");
		Description.dirAB = WaterMaterial.GetVector("_GDirectionAB");
		Description.dirCD = WaterMaterial.GetVector("_GDirectionCD");
	}

	public static Vector3 GetGestnerDisplacement(Vector3 WorldPosition, LuxWaterUtils.GersterWavesDescription Description, float TimeOffset)
	{
		Vector2 vector;
		vector.x = WorldPosition.x;
		vector.y = WorldPosition.z;
		Vector4 vector2;
		vector2.x = Description.steepness.x * Description.amp.x * Description.dirAB.x;
		vector2.y = Description.steepness.x * Description.amp.x * Description.dirAB.y;
		vector2.z = Description.steepness.y * Description.amp.y * Description.dirAB.z;
		vector2.w = Description.steepness.y * Description.amp.y * Description.dirAB.w;
		Vector4 vector3;
		vector3.x = Description.steepness.z * Description.amp.z * Description.dirCD.x;
		vector3.y = Description.steepness.z * Description.amp.z * Description.dirCD.y;
		vector3.z = Description.steepness.w * Description.amp.w * Description.dirCD.z;
		vector3.w = Description.steepness.w * Description.amp.w * Description.dirCD.w;
		Vector4 vector4;
		vector4.x = Description.freq.x * (Description.dirAB.x * vector.x + Description.dirAB.y * vector.y);
		vector4.y = Description.freq.y * (Description.dirAB.z * vector.x + Description.dirAB.w * vector.y);
		vector4.z = Description.freq.z * (Description.dirCD.x * vector.x + Description.dirCD.y * vector.y);
		vector4.w = Description.freq.w * (Description.dirCD.z * vector.x + Description.dirCD.w * vector.y);
		float num = Time.timeSinceLevelLoad + TimeOffset;
		Vector4 vector5;
		vector5.x = num * Description.speed.x;
		vector5.y = num * Description.speed.y;
		vector5.z = num * Description.speed.z;
		vector5.w = num * Description.speed.w;
		vector4.x += vector5.x;
		vector4.y += vector5.y;
		vector4.z += vector5.z;
		vector4.w += vector5.w;
		Vector4 vector6;
		vector6.x = (float)Math.Cos((double)vector4.x);
		vector6.y = (float)Math.Cos((double)vector4.y);
		vector6.z = (float)Math.Cos((double)vector4.z);
		vector6.w = (float)Math.Cos((double)vector4.w);
		Vector4 vector7;
		vector7.x = (float)Math.Sin((double)vector4.x);
		vector7.y = (float)Math.Sin((double)vector4.y);
		vector7.z = (float)Math.Sin((double)vector4.z);
		vector7.w = (float)Math.Sin((double)vector4.w);
		Vector3 result;
		result.x = (vector6.x * vector2.x + vector6.y * vector2.z + vector6.z * vector3.x + vector6.w * vector3.z) * Description.intensity.x;
		result.z = (vector6.x * vector2.y + vector6.y * vector2.w + vector6.z * vector3.y + vector6.w * vector3.w) * Description.intensity.z;
		result.y = (vector7.x * Description.amp.x + vector7.y * Description.amp.y + vector7.z * Description.amp.z + vector7.w * Description.amp.w) * Description.intensity.y;
		return result;
	}

	public struct GersterWavesDescription
	{
		public Vector3 intensity;

		public Vector4 steepness;

		public Vector4 amp;

		public Vector4 freq;

		public Vector4 speed;

		public Vector4 dirAB;

		public Vector4 dirCD;
	}
}
