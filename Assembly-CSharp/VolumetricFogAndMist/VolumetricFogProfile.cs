using System;
using UnityEngine;

namespace VolumetricFogAndMist
{
	[CreateAssetMenu(fileName = "VolumetricFogProfile", menuName = "Volumetric Fog Profile", order = 100)]
	public class VolumetricFogProfile : ScriptableObject
	{
		public void Load(VolumetricFog fog)
		{
			fog.density = this.density;
			fog.noiseStrength = this.noiseStrength;
			fog.height = this.height;
			fog.baselineHeight = this.baselineHeight;
			fog.distance = this.distance;
			fog.distanceFallOff = this.distanceFallOff;
			fog.maxFogLength = this.maxFogLength;
			fog.maxFogLengthFallOff = this.maxFogLengthFallOff;
			fog.baselineRelativeToCamera = this.baselineRelativeToCamera;
			fog.baselineRelativeToCameraDelay = this.baselineRelativeToCameraDelay;
			fog.noiseScale = this.noiseScale;
			fog.noiseSparse = this.noiseSparse;
			fog.useXYPlane = this.useXYPlane;
			fog.sunCopyColor = this.sunCopyColor;
			fog.alpha = this.alpha;
			fog.color = this.color;
			fog.specularColor = this.specularColor;
			fog.specularThreshold = this.specularThreshold;
			fog.specularIntensity = this.specularIntensity;
			fog.lightDirection = this.lightDirection;
			fog.lightIntensity = this.lightIntensity;
			fog.lightColor = this.lightColor;
			fog.speed = this.speed;
			fog.windDirection = this.windDirection;
			fog.turbulenceStrength = this.turbulenceStrength;
			fog.useRealTime = this.useRealTime;
			fog.skyColor = this.skyColor;
			fog.skyHaze = this.skyHaze;
			fog.skySpeed = this.skySpeed;
			fog.skyNoiseStrength = this.skyNoiseStrength;
			fog.skyAlpha = this.skyAlpha;
			fog.stepping = this.stepping;
			fog.steppingNear = this.steppingNear;
			fog.dithering = this.dithering;
			fog.ditherStrength = this.ditherStrength;
		}

		public void Save(VolumetricFog fog)
		{
			this.density = fog.density;
			this.noiseStrength = fog.noiseStrength;
			this.height = fog.height;
			this.baselineHeight = fog.baselineHeight;
			this.distance = fog.distance;
			this.distanceFallOff = fog.distanceFallOff;
			this.maxFogLength = fog.maxFogLength;
			this.maxFogLengthFallOff = fog.maxFogLengthFallOff;
			this.baselineRelativeToCamera = fog.baselineRelativeToCamera;
			this.baselineRelativeToCameraDelay = fog.baselineRelativeToCameraDelay;
			this.noiseScale = fog.noiseScale;
			this.noiseSparse = fog.noiseSparse;
			this.useXYPlane = fog.useXYPlane;
			this.sunCopyColor = fog.sunCopyColor;
			this.alpha = fog.alpha;
			this.color = fog.color;
			this.specularColor = fog.specularColor;
			this.specularThreshold = fog.specularThreshold;
			this.specularIntensity = fog.specularIntensity;
			this.lightDirection = fog.lightDirection;
			this.lightIntensity = fog.lightIntensity;
			this.lightColor = fog.lightColor;
			this.speed = fog.speed;
			this.windDirection = fog.windDirection;
			this.turbulenceStrength = fog.turbulenceStrength;
			this.useRealTime = fog.useRealTime;
			this.skyColor = fog.skyColor;
			this.skyHaze = fog.skyHaze;
			this.skySpeed = fog.skySpeed;
			this.skyNoiseStrength = fog.skyNoiseStrength;
			this.skyAlpha = fog.skyAlpha;
			this.stepping = fog.stepping;
			this.steppingNear = fog.steppingNear;
			this.dithering = fog.dithering;
			this.ditherStrength = fog.ditherStrength;
		}

		public static void Lerp(VolumetricFogProfile profile1, VolumetricFogProfile profile2, float t, VolumetricFog fog)
		{
			if (t < 0f)
			{
				t = 0f;
			}
			else if (t > 1f)
			{
				t = 1f;
			}
			fog.density = profile1.density * (1f - t) + profile2.density * t;
			fog.noiseStrength = profile1.noiseStrength * (1f - t) + profile2.noiseStrength * t;
			fog.height = profile1.height * (1f - t) + profile2.height * t;
			fog.baselineHeight = profile1.baselineHeight * (1f - t) + profile2.baselineHeight * t;
			fog.distance = profile1.baselineHeight * (1f - t) + profile2.distance * t;
			fog.distanceFallOff = profile1.distanceFallOff * (1f - t) + profile2.distanceFallOff * t;
			fog.maxFogLength = profile1.maxFogLength * (1f - t) + profile2.maxFogLength * t;
			fog.maxFogLengthFallOff = profile1.maxFogLengthFallOff * (1f - t) + profile2.maxFogLengthFallOff * t;
			fog.baselineRelativeToCamera = ((t >= 0.5f) ? profile2.baselineRelativeToCamera : profile1.baselineRelativeToCamera);
			fog.baselineRelativeToCameraDelay = profile1.baselineRelativeToCameraDelay * (1f - t) + profile2.baselineRelativeToCameraDelay * t;
			fog.noiseScale = profile1.noiseScale * (1f - t) + profile2.noiseScale * t;
			fog.noiseSparse = profile1.noiseSparse * (1f - t) + profile2.noiseSparse * t;
			fog.sunCopyColor = ((t >= 0.5f) ? profile2.sunCopyColor : profile1.sunCopyColor);
			fog.alpha = profile1.alpha * (1f - t) + profile2.alpha * t;
			fog.color = profile1.color * (1f - t) + profile2.color * t;
			fog.specularColor = profile1.specularColor * (1f - t) + profile2.color * t;
			fog.specularThreshold = profile1.specularThreshold * (1f - t) + profile2.specularThreshold * t;
			fog.specularIntensity = profile1.specularIntensity * (1f - t) + profile2.specularIntensity * t;
			fog.lightDirection = profile1.lightDirection * (1f - t) + profile2.lightDirection * t;
			fog.lightIntensity = profile1.lightIntensity * (1f - t) + profile2.lightIntensity * t;
			fog.lightColor = profile1.lightColor * (1f - t) + profile2.lightColor * t;
			fog.speed = profile1.speed * (1f - t) + profile2.speed * t;
			fog.windDirection = profile1.windDirection * (1f - t) + profile2.windDirection * t;
			fog.turbulenceStrength = profile1.turbulenceStrength * (1f - t) + profile2.turbulenceStrength * t;
			fog.skyColor = profile1.skyColor * (1f - t) + profile2.skyColor * t;
			fog.skyHaze = profile1.skyHaze * (1f - t) + profile2.skyHaze * t;
			fog.skySpeed = profile1.skySpeed * (1f - t) + profile2.skySpeed * t;
			fog.skyNoiseStrength = profile1.skyNoiseStrength * (1f - t) + profile2.skyNoiseStrength * t;
			fog.skyAlpha = profile1.skyAlpha * (1f - t) + profile2.skyAlpha * t;
			fog.stepping = profile1.stepping * (1f - t) + profile2.stepping * t;
			fog.steppingNear = profile1.steppingNear * (1f - t) + profile2.steppingNear * t;
			fog.dithering = ((t >= 0.5f) ? profile2.dithering : profile1.dithering);
			fog.ditherStrength = profile1.ditherStrength * (1f - t) + profile2.ditherStrength * t;
		}

		public LIGHTING_MODEL lightingModel;

		public bool sunCopyColor = true;

		[Range(0f, 1.25f)]
		public float density = 1f;

		[Range(0f, 1f)]
		public float noiseStrength = 0.8f;

		[Range(0f, 500f)]
		public float height = 4f;

		public float baselineHeight;

		[Range(0f, 1000f)]
		public float distance;

		[Range(0f, 5f)]
		public float distanceFallOff;

		[Range(0f, 2000f)]
		public float maxFogLength = 1000f;

		[Range(0f, 1f)]
		public float maxFogLengthFallOff;

		public bool baselineRelativeToCamera;

		[Range(0f, 1f)]
		public float baselineRelativeToCameraDelay;

		[Range(0.2f, 10f)]
		public float noiseScale = 1f;

		[Range(-0.3f, 1f)]
		public float noiseSparse;

		[Range(0f, 1.05f)]
		public float alpha = 1f;

		public Color color = new Color(0.89f, 0.89f, 0.89f, 1f);

		public Color specularColor = new Color(1f, 1f, 0.8f, 1f);

		[Range(0f, 1f)]
		public float specularThreshold = 0.6f;

		[Range(0f, 1f)]
		public float specularIntensity = 0.2f;

		public Vector3 lightDirection = new Vector3(1f, 0f, -1f);

		[Range(-1f, 3f)]
		public float lightIntensity = 0.2f;

		public Color lightColor = Color.white;

		[Range(0f, 1f)]
		public float speed = 0.01f;

		public bool useRealTime;

		public Vector3 windDirection = new Vector3(-1f, 0f, 0f);

		[Range(0f, 10f)]
		public float turbulenceStrength;

		public bool useXYPlane;

		public Color skyColor = new Color(0.89f, 0.89f, 0.89f, 1f);

		[Range(0f, 1000f)]
		public float skyHaze = 50f;

		[Range(0f, 1f)]
		public float skySpeed = 0.3f;

		[Range(0f, 1f)]
		public float skyNoiseStrength = 0.1f;

		[Range(0f, 1f)]
		public float skyAlpha = 1f;

		public float stepping = 12f;

		public float steppingNear = 1f;

		public bool dithering;

		public float ditherStrength = 0.75f;
	}
}
