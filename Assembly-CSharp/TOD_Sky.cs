using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CJTools;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(TOD_Components))]
[ExecuteInEditMode]
[RequireComponent(typeof(TOD_Resources))]
public class TOD_Sky : MonoBehaviour
{
	private void UpdateScattering()
	{
		float num = -this.Atmosphere.Directionality;
		float num2 = num * num;
		this.kBetaMie.x = 1.5f * ((1f - num2) / (2f + num2));
		this.kBetaMie.y = 1f + num2;
		this.kBetaMie.z = 2f * num;
		float num3 = 0.002f * this.Atmosphere.MieMultiplier;
		float num4 = 0.002f * this.Atmosphere.RayleighMultiplier;
		float x = num4 * 40f * 5.27016449f;
		float y = num4 * 40f * 9.473285f;
		float z = num4 * 40f * 19.6438026f;
		float w = num3 * 40f;
		this.kSun.x = x;
		this.kSun.y = y;
		this.kSun.z = z;
		this.kSun.w = w;
		float x2 = num4 * 4f * 3.14159274f * 5.27016449f;
		float y2 = num4 * 4f * 3.14159274f * 9.473285f;
		float z2 = num4 * 4f * 3.14159274f * 19.6438026f;
		float w2 = num3 * 4f * 3.14159274f;
		this.k4PI.x = x2;
		this.k4PI.y = y2;
		this.k4PI.z = z2;
		this.k4PI.w = w2;
		this.kRadius.x = 1f;
		this.kRadius.y = 1f;
		this.kRadius.z = 1.025f;
		this.kRadius.w = 1.050625f;
		this.kScale.x = 40.00004f;
		this.kScale.y = 0.25f;
		this.kScale.z = 160.000153f;
		this.kScale.w = 0.0001f;
	}

	private void UpdateCelestials()
	{
		float f = 0.0174532924f * this.World.Latitude;
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float longitude = this.World.Longitude;
		float num3 = 1.57079637f;
		int year = this.Cycle.Year;
		int month = this.Cycle.Month;
		int day = this.Cycle.Day;
		float num4 = this.Cycle.Hour - this.World.UTC;
		float num5 = (float)(367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730530) + num4 / 24f;
		float num6 = (float)(367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730530) + 0.5f;
		float num7 = 23.4393f - 3.563E-07f * num5;
		float f2 = 0.0174532924f * num7;
		float num8 = Mathf.Sin(f2);
		float num9 = Mathf.Cos(f2);
		float num10 = 282.9404f + 4.70935E-05f * num6;
		float num11 = 0.016709f - 1.151E-09f * num6;
		float num12 = 356.047f + 0.985600233f * num6;
		float num13 = 0.0174532924f * num12;
		float num14 = Mathf.Sin(num13);
		float num15 = Mathf.Cos(num13);
		float f3 = num13 + num11 * num14 * (1f + num11 * num15);
		float num16 = Mathf.Sin(f3);
		float num17 = Mathf.Cos(f3);
		float num18 = num17 - num11;
		float num19 = Mathf.Sqrt(1f - num11 * num11) * num16;
		float num20 = 57.29578f * Mathf.Atan2(num19, num18);
		float num21 = Mathf.Sqrt(num18 * num18 + num19 * num19);
		float num22 = num20 + num10;
		float f4 = 0.0174532924f * num22;
		float num23 = Mathf.Sin(f4);
		float num24 = Mathf.Cos(f4);
		float num25 = num21 * num24;
		float num26 = num21 * num23;
		float num27 = num25;
		float num28 = num26 * num9;
		float y = num26 * num8;
		float num29 = Mathf.Atan2(num28, num27);
		float num30 = 57.29578f * num29;
		float f5 = Mathf.Atan2(y, Mathf.Sqrt(num27 * num27 + num28 * num28));
		float num31 = Mathf.Sin(f5);
		float num32 = Mathf.Cos(f5);
		float num33 = num20 + num10;
		float num34 = num33 + 180f;
		float num35 = num30 - num34 - longitude;
		float num36 = -6f;
		float f6 = 0.0174532924f * num36;
		float num37 = Mathf.Sin(f6);
		float f7 = (num37 - num * num31) / (num2 * num32);
		float num38 = Mathf.Acos(f7);
		float num39 = 57.29578f * num38;
		this.SunsetTime = (24f + (num35 + num39) / 15f % 24f) % 24f;
		this.SunriseTime = (24f + (num35 - num39) / 15f % 24f) % 24f;
		float num40 = 282.9404f + 4.70935E-05f * num5;
		float num41 = 0.016709f - 1.151E-09f * num5;
		float num42 = 356.047f + 0.985600233f * num5;
		float num43 = 0.0174532924f * num42;
		float num44 = Mathf.Sin(num43);
		float num45 = Mathf.Cos(num43);
		float f8 = num43 + num41 * num44 * (1f + num41 * num45);
		float num46 = Mathf.Sin(f8);
		float num47 = Mathf.Cos(f8);
		float num48 = num47 - num41;
		float num49 = Mathf.Sqrt(1f - num41 * num41) * num46;
		float num50 = 57.29578f * Mathf.Atan2(num49, num48);
		float num51 = Mathf.Sqrt(num48 * num48 + num49 * num49);
		float num52 = num50 + num40;
		float f9 = 0.0174532924f * num52;
		float num53 = Mathf.Sin(f9);
		float num54 = Mathf.Cos(f9);
		float num55 = num51 * num54;
		float num56 = num51 * num53;
		float num57 = num55;
		float num58 = num56 * num9;
		float y2 = num56 * num8;
		float num59 = Mathf.Atan2(num58, num57);
		float f10 = Mathf.Atan2(y2, Mathf.Sqrt(num57 * num57 + num58 * num58));
		float num60 = Mathf.Sin(f10);
		float num61 = Mathf.Cos(f10);
		float num62 = num50 + num40;
		float num63 = num62 + 180f;
		float num64 = num63 + 15f * num4;
		float num65 = 0.0174532924f * (num64 + longitude);
		this.LocalSiderealTime = (num64 + longitude) / 15f;
		float f11 = num65 - num59;
		float num66 = Mathf.Sin(f11);
		float num67 = Mathf.Cos(f11);
		float num68 = num67 * num61;
		float num69 = num66 * num61;
		float num70 = num60;
		float num71 = num68 * num - num70 * num2;
		float num72 = num69;
		float y3 = num68 * num2 + num70 * num;
		float num73 = Mathf.Atan2(num72, num71) + 3.14159274f;
		float num74 = Mathf.Atan2(y3, Mathf.Sqrt(num71 * num71 + num72 * num72));
		float num75 = num3 - num74;
		float num76 = num74;
		float num77 = num73;
		this.SunZenith = 57.29578f * num75;
		this.SunAltitude = 57.29578f * num76;
		this.SunAzimuth = 57.29578f * num77;
		float num120;
		float num121;
		float num122;
		if (this.Moon.Position == TOD_MoonPositionType.Realistic)
		{
			float num78 = 125.1228f - 0.05295381f * num5;
			float num79 = 5.1454f;
			float num80 = 318.0634f + 0.164357319f * num5;
			float num81 = 60.2666f;
			float num82 = 0.0549f;
			float num83 = 115.3654f + 13.0649929f * num5;
			float f12 = 0.0174532924f * num78;
			float num84 = Mathf.Sin(f12);
			float num85 = Mathf.Cos(f12);
			float f13 = 0.0174532924f * num79;
			float num86 = Mathf.Sin(f13);
			float num87 = Mathf.Cos(f13);
			float num88 = 0.0174532924f * num83;
			float num89 = Mathf.Sin(num88);
			float num90 = Mathf.Cos(num88);
			float f14 = num88 + num82 * num89 * (1f + num82 * num90);
			float num91 = Mathf.Sin(f14);
			float num92 = Mathf.Cos(f14);
			float num93 = num81 * (num92 - num82);
			float num94 = num81 * (Mathf.Sqrt(1f - num82 * num82) * num91);
			float num95 = 57.29578f * Mathf.Atan2(num94, num93);
			float num96 = Mathf.Sqrt(num93 * num93 + num94 * num94);
			float num97 = num95 + num80;
			float f15 = 0.0174532924f * num97;
			float num98 = Mathf.Sin(f15);
			float num99 = Mathf.Cos(f15);
			float num100 = num96 * (num85 * num99 - num84 * num98 * num87);
			float num101 = num96 * (num84 * num99 + num85 * num98 * num87);
			float num102 = num96 * (num98 * num86);
			float num103 = num100;
			float num104 = num101;
			float num105 = num102;
			float num106 = num103;
			float num107 = num104 * num9 - num105 * num8;
			float y4 = num104 * num8 + num105 * num9;
			float num108 = Mathf.Atan2(num107, num106);
			float f16 = Mathf.Atan2(y4, Mathf.Sqrt(num106 * num106 + num107 * num107));
			float num109 = Mathf.Sin(f16);
			float num110 = Mathf.Cos(f16);
			float f17 = num65 - num108;
			float num111 = Mathf.Sin(f17);
			float num112 = Mathf.Cos(f17);
			float num113 = num112 * num110;
			float num114 = num111 * num110;
			float num115 = num109;
			float num116 = num113 * num - num115 * num2;
			float num117 = num114;
			float y5 = num113 * num2 + num115 * num;
			float num118 = Mathf.Atan2(num117, num116) + 3.14159274f;
			float num119 = Mathf.Atan2(y5, Mathf.Sqrt(num116 * num116 + num117 * num117));
			num120 = num3 - num119;
			num121 = num119;
			num122 = num118;
		}
		else
		{
			num120 = num75 - 3.14159274f;
			num121 = num76 - 3.14159274f;
			num122 = num77;
		}
		this.MoonZenith = 57.29578f * num120;
		this.MoonAltitude = 57.29578f * num121;
		this.MoonAzimuth = 57.29578f * num122;
		Quaternion quaternion = Quaternion.Euler(90f - this.World.Latitude, 0f, 0f) * Quaternion.Euler(0f, this.World.Longitude, 0f) * Quaternion.Euler(0f, num65 * 57.29578f, 0f);
		if (this.Stars.Position == TOD_StarsPositionType.Rotating)
		{
			this.Components.SpaceTransform.localRotation = quaternion;
			this.Components.StarTransform.localRotation = quaternion;
		}
		else
		{
			this.Components.SpaceTransform.localRotation = Quaternion.identity;
			this.Components.StarTransform.localRotation = Quaternion.identity;
		}
		Vector3 localPosition = this.OrbitalToLocal(num75, num77);
		this.Components.SunTransform.localPosition = localPosition;
		this.Components.SunTransform.LookAt(this.Components.DomeTransform.position, this.Components.SunTransform.up);
		Vector3 localPosition2 = this.OrbitalToLocal(num120, num122);
		Vector3 worldUp = quaternion * -Vector3.right;
		this.Components.MoonTransform.localPosition = localPosition2;
		this.Components.MoonTransform.LookAt(this.Components.DomeTransform.position, worldUp);
		float num123 = 8f * Mathf.Tan(0.008726646f * this.Sun.MeshSize);
		Vector3 localScale = new Vector3(num123, num123, num123);
		this.Components.SunTransform.localScale = localScale;
		float num124 = 4f * Mathf.Tan(0.008726646f * this.Moon.MeshSize);
		Vector3 localScale2 = new Vector3(num124, num124, num124);
		this.Components.MoonTransform.localScale = localScale2;
		if (RainManager.Get() != null)
		{
			float a = (this.Cycle.Hour <= 19f && this.Cycle.Hour >= 7f) ? 0.5f : 0f;
			this.Atmosphere.Fogginess = CJTools.Math.GetProportionalClamp(0f, a, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
			this.Clouds.Coverage = CJTools.Math.GetProportionalClamp(this.m_DefaultCloudsCoverage, 0.8f, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
			this.Clouds.Opacity = CJTools.Math.GetProportionalClamp(this.m_DefaultCloudsOpacity, 0.8f, RainManager.Get().m_WeatherInterpolated, 0f, 1f);
		}
		bool enabled = (1f - this.Atmosphere.Fogginess) * (1f - this.LerpValue) > 0f;
		this.Components.SpaceRenderer.enabled = enabled;
		this.Components.StarRenderer.enabled = enabled;
		bool enabled2 = this.Components.SunTransform.localPosition.y > -num123;
		this.Components.SunRenderer.enabled = enabled2;
		bool enabled3 = this.Components.MoonTransform.localPosition.y > -num124;
		this.Components.MoonRenderer.enabled = enabled3;
		bool enabled4 = true;
		this.Components.AtmosphereRenderer.enabled = enabled4;
		bool enabled5 = this.Components.Rays != null;
		this.Components.ClearRenderer.enabled = enabled5;
		bool enabled6 = this.Clouds.Coverage > 0f && this.Clouds.Opacity > 0f;
		this.Components.CloudRenderer.enabled = enabled6;
		this.LerpValue = Mathf.InverseLerp(105f, 90f, this.SunZenith);
		float time = Mathf.Clamp01(this.SunZenith / 90f);
		float time2 = Mathf.Clamp01((this.SunZenith - 90f) / 90f);
		float num125 = Mathf.Clamp01((this.LerpValue - 0.1f) / 0.9f);
		float num126 = Mathf.Clamp01((0.1f - this.LerpValue) / 0.1f);
		float num127 = Mathf.Clamp01((90f - num120 * 57.29578f) / 5f);
		this.SunVisibility = (1f - this.Atmosphere.Fogginess) * num125;
		this.MoonVisibility = (1f - this.Atmosphere.Fogginess) * num126 * num127;
		this.SunLightColor = TOD_Util.ApplyAlpha(this.Day.LightColor.Evaluate(time));
		this.MoonLightColor = TOD_Util.ApplyAlpha(this.Night.LightColor.Evaluate(time2));
		this.SunRayColor = TOD_Util.ApplyAlpha(this.Day.RayColor.Evaluate(time));
		this.MoonRayColor = TOD_Util.ApplyAlpha(this.Night.RayColor.Evaluate(time2));
		this.SunSkyColor = TOD_Util.ApplyAlpha(this.Day.SkyColor.Evaluate(time));
		this.MoonSkyColor = TOD_Util.ApplyAlpha(this.Night.SkyColor.Evaluate(time2));
		this.SunMeshColor = TOD_Util.ApplyAlpha(this.Day.SunColor.Evaluate(time));
		this.MoonMeshColor = TOD_Util.ApplyAlpha(this.Night.MoonColor.Evaluate(time2));
		this.SunCloudColor = TOD_Util.ApplyAlpha(this.Day.CloudColor.Evaluate(time));
		this.MoonCloudColor = TOD_Util.ApplyAlpha(this.Night.CloudColor.Evaluate(time2));
		Color b = TOD_Util.ApplyAlpha(this.Day.FogColor.Evaluate(time));
		Color a2 = TOD_Util.ApplyAlpha(this.Night.FogColor.Evaluate(time2));
		this.FogColor = Color.Lerp(a2, b, this.LerpValue);
		Color color = TOD_Util.ApplyAlpha(this.Day.AmbientColor.Evaluate(time));
		Color color2 = TOD_Util.ApplyAlpha(this.Night.AmbientColor.Evaluate(time2));
		if (CaveSensor.s_NumSensorsInside != 0)
		{
			this.AmbientColor += (Color.black - this.AmbientColor) * Time.deltaTime;
			if (this.AmbientColor != Color.black)
			{
				this.Ambient.UpdateInterval = 0f;
			}
			else
			{
				this.Ambient.UpdateInterval = 1f;
			}
		}
		else
		{
			Color color3 = Color.Lerp(color2, color, this.LerpValue);
			this.AmbientColor += (color3 - this.AmbientColor) * Time.deltaTime;
			if (this.AmbientColor != color3)
			{
				this.Ambient.UpdateInterval = 0f;
			}
			else
			{
				this.Ambient.UpdateInterval = 1f;
			}
		}
		Color b2 = color;
		Color a3 = color2;
		this.GroundColor = Color.Lerp(a3, b2, this.LerpValue);
		this.MoonHaloColor = TOD_Util.MulRGB(this.MoonSkyColor, this.Moon.HaloBrightness * num127);
		float shadowStrength;
		float intensity;
		Color color4;
		if (this.LerpValue > 0.1f)
		{
			this.IsDay = true;
			this.IsNight = false;
			shadowStrength = this.Day.ShadowStrength;
			intensity = Mathf.Lerp(0f, this.Day.LightIntensity * this.Day.m_SanityLightIntensityMul, this.SunVisibility);
			color4 = this.SunLightColor;
		}
		else
		{
			this.IsDay = false;
			this.IsNight = true;
			shadowStrength = this.Night.ShadowStrength;
			intensity = Mathf.Lerp(0f, this.Night.LightIntensity * this.Night.m_SanityLightIntensityMul, this.MoonVisibility);
			color4 = this.MoonLightColor;
		}
		this.Components.LightSource.color = color4;
		this.Components.LightSource.intensity = intensity;
		this.Components.LightSource.shadowStrength = shadowStrength;
		if (!Application.isPlaying || this.timeSinceLightUpdate >= this.Light.UpdateInterval)
		{
			this.timeSinceLightUpdate = 0f;
			Vector3 localPosition3 = (!this.IsNight) ? this.OrbitalToLocal(Mathf.Min(num75, (1f - this.Light.MinimumHeight) * 3.14159274f / 2f), num77) : this.OrbitalToLocal(Mathf.Min(num120, (1f - this.Light.MinimumHeight) * 3.14159274f / 2f), num122);
			this.Components.LightTransform.localPosition = localPosition3;
			this.Components.LightTransform.LookAt(this.Components.DomeTransform.position);
		}
		else
		{
			this.timeSinceLightUpdate += Time.deltaTime;
		}
		this.SunDirection = -this.Components.SunTransform.forward;
		this.LocalSunDirection = this.Components.DomeTransform.InverseTransformDirection(this.SunDirection);
		this.MoonDirection = -this.Components.MoonTransform.forward;
		this.LocalMoonDirection = this.Components.DomeTransform.InverseTransformDirection(this.MoonDirection);
		this.LightDirection = -this.Components.LightTransform.forward;
		this.LocalLightDirection = this.Components.DomeTransform.InverseTransformDirection(this.LightDirection);
	}

	public static List<TOD_Sky> Instances
	{
		get
		{
			return TOD_Sky.instances;
		}
	}

	public static TOD_Sky Instance
	{
		get
		{
			return (TOD_Sky.instances.Count != 0) ? TOD_Sky.instances[TOD_Sky.instances.Count - 1] : null;
		}
	}

	public bool Initialized { get; private set; }

	public bool Headless
	{
		get
		{
			return Camera.allCamerasCount == 0;
		}
	}

	public TOD_Components Components { get; private set; }

	public TOD_Resources Resources { get; private set; }

	public bool IsDay { get; private set; }

	public bool IsNight { get; private set; }

	public float Radius
	{
		get
		{
			return this.Components.DomeTransform.lossyScale.y;
		}
	}

	public float Diameter
	{
		get
		{
			return this.Components.DomeTransform.lossyScale.y * 2f;
		}
	}

	public float LerpValue { get; private set; }

	public float SunZenith { get; private set; }

	public float SunAltitude { get; private set; }

	public float SunAzimuth { get; private set; }

	public float MoonZenith { get; private set; }

	public float MoonAltitude { get; private set; }

	public float MoonAzimuth { get; private set; }

	public float SunsetTime { get; private set; }

	public float SunriseTime { get; private set; }

	public float LocalSiderealTime { get; private set; }

	public float LightZenith
	{
		get
		{
			return Mathf.Min(this.SunZenith, this.MoonZenith);
		}
	}

	public float LightIntensity
	{
		get
		{
			return this.Components.LightSource.intensity;
		}
	}

	public float SunVisibility { get; private set; }

	public float MoonVisibility { get; private set; }

	public Vector3 SunDirection { get; private set; }

	public Vector3 MoonDirection { get; private set; }

	public Vector3 LightDirection { get; private set; }

	public Vector3 LocalSunDirection { get; private set; }

	public Vector3 LocalMoonDirection { get; private set; }

	public Vector3 LocalLightDirection { get; private set; }

	public Color SunLightColor { get; private set; }

	public Color MoonLightColor { get; private set; }

	public Color LightColor
	{
		get
		{
			return this.Components.LightSource.color;
		}
	}

	public Color SunRayColor { get; private set; }

	public Color MoonRayColor { get; private set; }

	public Color SunSkyColor { get; private set; }

	public Color MoonSkyColor { get; private set; }

	public Color SunMeshColor { get; private set; }

	public Color MoonMeshColor { get; private set; }

	public Color SunCloudColor { get; private set; }

	public Color MoonCloudColor { get; private set; }

	public Color FogColor { get; private set; }

	public Color GroundColor { get; private set; }

	public Color AmbientColor { get; private set; }

	public Color MoonHaloColor { get; private set; }

	public ReflectionProbe Probe { get; private set; }

	public Vector3 OrbitalToUnity(float radius, float theta, float phi)
	{
		float num = Mathf.Sin(theta);
		float num2 = Mathf.Cos(theta);
		float num3 = Mathf.Sin(phi);
		float num4 = Mathf.Cos(phi);
		Vector3 result;
		result.z = radius * num * num4;
		result.y = radius * num2;
		result.x = radius * num * num3;
		return result;
	}

	public Vector3 OrbitalToLocal(float theta, float phi)
	{
		float num = Mathf.Sin(theta);
		float y = Mathf.Cos(theta);
		float num2 = Mathf.Sin(phi);
		float num3 = Mathf.Cos(phi);
		Vector3 result;
		result.z = num * num3;
		result.y = y;
		result.x = num * num2;
		return result;
	}

	public Color SampleAtmosphere(Vector3 direction, bool directLight = true)
	{
		Vector3 dir = this.Components.DomeTransform.InverseTransformDirection(direction);
		Color color = this.ShaderScatteringColor(dir, directLight);
		color = this.TOD_HDR2LDR(color);
		return this.TOD_LINEAR2GAMMA(color);
	}

	public SphericalHarmonicsL2 RenderToSphericalHarmonics()
	{
		SphericalHarmonicsL2 result = default(SphericalHarmonicsL2);
		bool directLight = false;
		Color color = TOD_Util.ChangeSaturation(this.AmbientColor.linear, this.Ambient.Saturation);
		Vector3 vector = new Vector3(0.612372458f, 0.5f, 0.612372458f);
		Vector3 up = Vector3.up;
		Color linear = this.SampleAtmosphere(up, directLight).linear;
		result.AddDirectionalLight(up, linear, 0.428571433f);
		Vector3 direction = new Vector3(-vector.x, vector.y, -vector.z);
		Color color2 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(direction, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(direction, color2, 0.2857143f);
		Vector3 direction2 = new Vector3(vector.x, vector.y, -vector.z);
		Color color3 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(direction2, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(direction2, color3, 0.2857143f);
		Vector3 direction3 = new Vector3(-vector.x, vector.y, vector.z);
		Color color4 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(direction3, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(direction3, color4, 0.2857143f);
		Vector3 direction4 = new Vector3(vector.x, vector.y, vector.z);
		Color color5 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(direction4, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(direction4, color5, 0.2857143f);
		Vector3 left = Vector3.left;
		Color color6 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(left, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(left, color6, 0.142857149f);
		Vector3 right = Vector3.right;
		Color color7 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(right, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(right, color7, 0.142857149f);
		Vector3 back = Vector3.back;
		Color color8 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(back, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(back, color8, 0.142857149f);
		Vector3 forward = Vector3.forward;
		Color color9 = TOD_Util.ChangeSaturation(this.SampleAtmosphere(forward, directLight).linear, this.Ambient.Saturation);
		result.AddDirectionalLight(forward, color9, 0.142857149f);
		Vector3 direction5 = new Vector3(-vector.x, -vector.y, -vector.z);
		result.AddDirectionalLight(direction5, color, 0.2857143f);
		Vector3 direction6 = new Vector3(vector.x, -vector.y, -vector.z);
		result.AddDirectionalLight(direction6, color, 0.2857143f);
		Vector3 direction7 = new Vector3(-vector.x, -vector.y, vector.z);
		result.AddDirectionalLight(direction7, color, 0.2857143f);
		Vector3 direction8 = new Vector3(vector.x, -vector.y, vector.z);
		result.AddDirectionalLight(direction8, color, 0.2857143f);
		Vector3 down = Vector3.down;
		result.AddDirectionalLight(down, color, 0.428571433f);
		return result;
	}

	public void RenderToCubemap(RenderTexture targetTexture = null)
	{
		if (!this.Probe)
		{
			this.Probe = new GameObject().AddComponent<ReflectionProbe>();
			this.Probe.name = base.gameObject.name + " Reflection Probe";
			this.Probe.mode = ReflectionProbeMode.Realtime;
		}
		if (this.probeRenderID < 0 || this.Probe.IsFinishedRendering(this.probeRenderID))
		{
			float maxValue = float.MaxValue;
			this.Probe.transform.position = this.Components.DomeTransform.position;
			this.Probe.size = new Vector3(maxValue, maxValue, maxValue);
			this.Probe.intensity = RenderSettings.reflectionIntensity;
			this.Probe.clearFlags = this.Reflection.ClearFlags;
			this.Probe.cullingMask = this.Reflection.CullingMask;
			this.Probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			this.Probe.timeSlicingMode = this.Reflection.TimeSlicing;
			this.probeRenderID = this.Probe.RenderProbe(targetTexture);
		}
	}

	public Color SampleFogColor(bool directLight = true)
	{
		Vector3 vector = Vector3.forward;
		if (this.Components.Camera != null)
		{
			vector = Quaternion.Euler(0f, this.Components.Camera.transform.rotation.eulerAngles.y, 0f) * vector;
		}
		Color color = this.SampleAtmosphere(Vector3.Lerp(vector, Vector3.up, this.Fog.HeightBias).normalized, directLight);
		return new Color(color.r, color.g, color.b, 1f);
	}

	public Color SampleSkyColor()
	{
		Vector3 sunDirection = this.SunDirection;
		sunDirection.y = Mathf.Abs(sunDirection.y);
		Color color = this.SampleAtmosphere(sunDirection.normalized, false);
		return new Color(color.r, color.g, color.b, 1f);
	}

	public Color SampleEquatorColor()
	{
		Vector3 sunDirection = this.SunDirection;
		sunDirection.y = 0f;
		Color color = this.SampleAtmosphere(sunDirection.normalized, false);
		return new Color(color.r, color.g, color.b, 1f);
	}

	public void UpdateFog()
	{
		TOD_FogType mode = this.Fog.Mode;
		if (mode != TOD_FogType.None)
		{
			if (mode != TOD_FogType.Color)
			{
				if (mode == TOD_FogType.Directional)
				{
					Color fogColor = this.SampleFogColor(true);
					RenderSettings.fogColor = fogColor;
				}
			}
			else
			{
				Color fogColor2 = this.SampleFogColor(false);
				RenderSettings.fogColor = fogColor2;
			}
		}
	}

	public void UpdateAmbient()
	{
		float saturation = this.Ambient.Saturation;
		float num = Mathf.Lerp(this.Night.AmbientMultiplier, this.Day.AmbientMultiplier, this.LerpValue);
		if (this.m_AmbientIntensity == -3.40282347E+38f)
		{
			this.m_AmbientIntensity = num;
		}
		Color color = TOD_Util.ChangeSaturation(this.AmbientColor, this.Ambient.Saturation);
		if (CaveSensor.s_NumSensorsInside != 0)
		{
			this.m_AmbientIntensity -= this.m_AmbientIntensity * Time.deltaTime;
		}
		else
		{
			this.m_AmbientIntensity += (num - this.m_AmbientIntensity) * Time.deltaTime;
		}
		num = this.m_AmbientIntensity;
		TOD_AmbientType mode = this.Ambient.Mode;
		if (mode != TOD_AmbientType.Color)
		{
			if (mode != TOD_AmbientType.Gradient)
			{
				if (mode == TOD_AmbientType.Spherical)
				{
					RenderSettings.ambientMode = AmbientMode.Skybox;
					RenderSettings.ambientLight = color;
					RenderSettings.ambientIntensity = num;
					RenderSettings.ambientProbe = this.RenderToSphericalHarmonics();
				}
			}
			else
			{
				Color ambientEquatorColor = TOD_Util.ChangeSaturation(this.SampleEquatorColor(), saturation);
				Color ambientSkyColor = TOD_Util.ChangeSaturation(this.SampleSkyColor(), saturation);
				RenderSettings.ambientMode = AmbientMode.Trilight;
				RenderSettings.ambientSkyColor = ambientSkyColor;
				RenderSettings.ambientEquatorColor = ambientEquatorColor;
				RenderSettings.ambientGroundColor = color;
				RenderSettings.ambientIntensity = num;
			}
		}
		else
		{
			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientLight = color;
			RenderSettings.ambientIntensity = num;
		}
	}

	public void UpdateReflection()
	{
		TOD_ReflectionType mode = this.Reflection.Mode;
		if (mode == TOD_ReflectionType.Cubemap)
		{
			float reflectionIntensity = Mathf.Lerp(this.Night.ReflectionMultiplier, this.Day.ReflectionMultiplier, this.LerpValue);
			RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
			RenderSettings.reflectionIntensity = reflectionIntensity;
			if (Application.isPlaying)
			{
				this.RenderToCubemap(null);
			}
		}
	}

	public void LoadParameters(string xml)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TOD_Parameters));
		XmlTextReader xmlReader = new XmlTextReader(new StringReader(xml));
		TOD_Parameters tod_Parameters = xmlSerializer.Deserialize(xmlReader) as TOD_Parameters;
		tod_Parameters.ToSky(this);
	}

	private void UpdateQualitySettings()
	{
		if (this.Headless)
		{
			return;
		}
		Mesh mesh = null;
		Mesh mesh2 = null;
		Mesh mesh3 = null;
		Mesh mesh4 = null;
		Mesh mesh5 = null;
		Mesh mesh6 = null;
		TOD_MeshQualityType meshQuality = this.MeshQuality;
		if (meshQuality != TOD_MeshQualityType.Low)
		{
			if (meshQuality != TOD_MeshQualityType.Medium)
			{
				if (meshQuality == TOD_MeshQualityType.High)
				{
					mesh = this.Resources.SkyLOD0;
					mesh2 = this.Resources.SkyLOD0;
					mesh3 = this.Resources.SkyLOD2;
					mesh4 = this.Resources.CloudsLOD0;
					mesh5 = this.Resources.MoonLOD0;
				}
			}
			else
			{
				mesh = this.Resources.SkyLOD1;
				mesh2 = this.Resources.SkyLOD1;
				mesh3 = this.Resources.SkyLOD2;
				mesh4 = this.Resources.CloudsLOD1;
				mesh5 = this.Resources.MoonLOD1;
			}
		}
		else
		{
			mesh = this.Resources.SkyLOD2;
			mesh2 = this.Resources.SkyLOD2;
			mesh3 = this.Resources.SkyLOD2;
			mesh4 = this.Resources.CloudsLOD2;
			mesh5 = this.Resources.MoonLOD2;
		}
		TOD_StarQualityType starQuality = this.StarQuality;
		if (starQuality != TOD_StarQualityType.Low)
		{
			if (starQuality != TOD_StarQualityType.Medium)
			{
				if (starQuality == TOD_StarQualityType.High)
				{
					mesh6 = this.Resources.StarsLOD0;
				}
			}
			else
			{
				mesh6 = this.Resources.StarsLOD1;
			}
		}
		else
		{
			mesh6 = this.Resources.StarsLOD2;
		}
		if (this.Components.SpaceMeshFilter && this.Components.SpaceMeshFilter.sharedMesh != mesh)
		{
			this.Components.SpaceMeshFilter.mesh = mesh;
		}
		if (this.Components.MoonMeshFilter && this.Components.MoonMeshFilter.sharedMesh != mesh5)
		{
			this.Components.MoonMeshFilter.mesh = mesh5;
		}
		if (this.Components.AtmosphereMeshFilter && this.Components.AtmosphereMeshFilter.sharedMesh != mesh2)
		{
			this.Components.AtmosphereMeshFilter.mesh = mesh2;
		}
		if (this.Components.ClearMeshFilter && this.Components.ClearMeshFilter.sharedMesh != mesh3)
		{
			this.Components.ClearMeshFilter.mesh = mesh3;
		}
		if (this.Components.CloudMeshFilter && this.Components.CloudMeshFilter.sharedMesh != mesh4)
		{
			this.Components.CloudMeshFilter.mesh = mesh4;
		}
		if (this.Components.StarMeshFilter && this.Components.StarMeshFilter.sharedMesh != mesh6)
		{
			this.Components.StarMeshFilter.mesh = mesh6;
		}
	}

	private void UpdateRenderSettings()
	{
		if (this.Headless)
		{
			return;
		}
		this.UpdateFog();
		if (!Application.isPlaying || this.timeSinceAmbientUpdate >= this.Ambient.UpdateInterval)
		{
			this.timeSinceAmbientUpdate = 0f;
			this.UpdateAmbient();
		}
		else
		{
			this.timeSinceAmbientUpdate += Time.deltaTime;
		}
		if (!Application.isPlaying || this.timeSinceReflectionUpdate >= this.Reflection.UpdateInterval)
		{
			this.timeSinceReflectionUpdate = 0f;
			this.UpdateReflection();
		}
		else
		{
			this.timeSinceReflectionUpdate += Time.deltaTime;
		}
	}

	private void UpdateShaderKeywords()
	{
		if (this.Headless)
		{
			return;
		}
		TOD_ColorSpaceType colorSpace = this.ColorSpace;
		if (colorSpace != TOD_ColorSpaceType.Auto)
		{
			if (colorSpace != TOD_ColorSpaceType.Linear)
			{
				if (colorSpace == TOD_ColorSpaceType.Gamma)
				{
					Shader.DisableKeyword("TOD_OUTPUT_LINEAR");
				}
			}
			else
			{
				Shader.EnableKeyword("TOD_OUTPUT_LINEAR");
			}
		}
		else if (QualitySettings.activeColorSpace == UnityEngine.ColorSpace.Linear)
		{
			Shader.EnableKeyword("TOD_OUTPUT_LINEAR");
		}
		else
		{
			Shader.DisableKeyword("TOD_OUTPUT_LINEAR");
		}
		TOD_ColorRangeType colorRange = this.ColorRange;
		if (colorRange != TOD_ColorRangeType.Auto)
		{
			if (colorRange != TOD_ColorRangeType.HDR)
			{
				if (colorRange == TOD_ColorRangeType.LDR)
				{
					Shader.DisableKeyword("TOD_OUTPUT_HDR");
				}
			}
			else
			{
				Shader.EnableKeyword("TOD_OUTPUT_HDR");
			}
		}
		else if (this.Components.Camera && this.Components.Camera.HDR)
		{
			Shader.EnableKeyword("TOD_OUTPUT_HDR");
		}
		else
		{
			Shader.DisableKeyword("TOD_OUTPUT_HDR");
		}
		TOD_ColorOutputType colorOutput = this.ColorOutput;
		if (colorOutput != TOD_ColorOutputType.Raw)
		{
			if (colorOutput == TOD_ColorOutputType.Dithered)
			{
				Shader.EnableKeyword("TOD_OUTPUT_DITHERING");
			}
		}
		else
		{
			Shader.DisableKeyword("TOD_OUTPUT_DITHERING");
		}
		TOD_SkyQualityType skyQuality = this.SkyQuality;
		if (skyQuality != TOD_SkyQualityType.PerVertex)
		{
			if (skyQuality == TOD_SkyQualityType.PerPixel)
			{
				Shader.EnableKeyword("TOD_SCATTERING_PER_PIXEL");
			}
		}
		else
		{
			Shader.DisableKeyword("TOD_SCATTERING_PER_PIXEL");
		}
		TOD_CloudQualityType cloudQuality = this.CloudQuality;
		if (cloudQuality != TOD_CloudQualityType.Low)
		{
			if (cloudQuality != TOD_CloudQualityType.Medium)
			{
				if (cloudQuality == TOD_CloudQualityType.High)
				{
					Shader.EnableKeyword("TOD_CLOUDS_DENSITY");
					Shader.EnableKeyword("TOD_CLOUDS_BUMPED");
				}
			}
			else
			{
				Shader.EnableKeyword("TOD_CLOUDS_DENSITY");
				Shader.DisableKeyword("TOD_CLOUDS_BUMPED");
			}
		}
		else
		{
			Shader.DisableKeyword("TOD_CLOUDS_DENSITY");
			Shader.DisableKeyword("TOD_CLOUDS_BUMPED");
		}
	}

	private void UpdateShaderProperties()
	{
		if (this.Headless)
		{
			return;
		}
		Shader.SetGlobalColor(this.Resources.ID_SunLightColor, this.SunLightColor);
		Shader.SetGlobalColor(this.Resources.ID_MoonLightColor, this.MoonLightColor);
		Shader.SetGlobalColor(this.Resources.ID_SunSkyColor, this.SunSkyColor);
		Shader.SetGlobalColor(this.Resources.ID_MoonSkyColor, this.MoonSkyColor);
		Shader.SetGlobalColor(this.Resources.ID_SunMeshColor, this.SunMeshColor);
		Shader.SetGlobalColor(this.Resources.ID_MoonMeshColor, this.MoonMeshColor);
		Shader.SetGlobalColor(this.Resources.ID_SunCloudColor, this.SunCloudColor);
		Shader.SetGlobalColor(this.Resources.ID_MoonCloudColor, this.MoonCloudColor);
		Shader.SetGlobalColor(this.Resources.ID_FogColor, this.FogColor);
		Shader.SetGlobalColor(this.Resources.ID_GroundColor, this.GroundColor);
		Shader.SetGlobalColor(this.Resources.ID_AmbientColor, this.AmbientColor);
		Shader.SetGlobalVector(this.Resources.ID_SunDirection, this.SunDirection);
		Shader.SetGlobalVector(this.Resources.ID_MoonDirection, this.MoonDirection);
		Shader.SetGlobalVector(this.Resources.ID_LightDirection, this.LightDirection);
		Shader.SetGlobalVector(this.Resources.ID_LocalSunDirection, this.LocalSunDirection);
		Shader.SetGlobalVector(this.Resources.ID_LocalMoonDirection, this.LocalMoonDirection);
		Shader.SetGlobalVector(this.Resources.ID_LocalLightDirection, this.LocalLightDirection);
		Shader.SetGlobalFloat(this.Resources.ID_Contrast, this.Atmosphere.Contrast);
		Shader.SetGlobalFloat(this.Resources.ID_Brightness, this.Atmosphere.Brightness);
		Shader.SetGlobalFloat(this.Resources.ID_Fogginess, this.Atmosphere.Fogginess);
		Shader.SetGlobalFloat(this.Resources.ID_Directionality, this.Atmosphere.Directionality);
		Shader.SetGlobalFloat(this.Resources.ID_MoonHaloPower, 1f / this.Moon.HaloSize);
		Shader.SetGlobalColor(this.Resources.ID_MoonHaloColor, this.MoonHaloColor);
		float value = Mathf.Lerp(0.8f, 0f, this.Clouds.Coverage);
		float num = Mathf.Lerp(3f, 9f, this.Clouds.Sharpness);
		float value2 = Mathf.Lerp(0f, 1f, this.Clouds.Attenuation);
		float value3 = Mathf.Lerp(0f, 2f, this.Clouds.Saturation);
		Shader.SetGlobalFloat(this.Resources.ID_CloudOpacity, this.Clouds.Opacity);
		Shader.SetGlobalFloat(this.Resources.ID_CloudCoverage, value);
		Shader.SetGlobalFloat(this.Resources.ID_CloudSharpness, 1f / num);
		Shader.SetGlobalFloat(this.Resources.ID_CloudDensity, num);
		Shader.SetGlobalFloat(this.Resources.ID_CloudAttenuation, value2);
		Shader.SetGlobalFloat(this.Resources.ID_CloudSaturation, value3);
		Shader.SetGlobalFloat(this.Resources.ID_CloudScattering, this.Clouds.Scattering);
		Shader.SetGlobalFloat(this.Resources.ID_CloudBrightness, this.Clouds.Brightness);
		Shader.SetGlobalVector(this.Resources.ID_CloudOffset, this.Components.Animation.OffsetUV);
		Shader.SetGlobalVector(this.Resources.ID_CloudWind, this.Components.Animation.CloudUV);
		Shader.SetGlobalVector(this.Resources.ID_CloudSize, new Vector3(this.Clouds.Size * 2f, this.Clouds.Size, this.Clouds.Size * 2f));
		Shader.SetGlobalFloat(this.Resources.ID_StarSize, this.Stars.Size);
		Shader.SetGlobalFloat(this.Resources.ID_StarBrightness, this.Stars.Brightness);
		Shader.SetGlobalFloat(this.Resources.ID_StarVisibility, (1f - this.Atmosphere.Fogginess) * (1f - this.LerpValue));
		Shader.SetGlobalFloat(this.Resources.ID_SunMeshContrast, 1f / Mathf.Max(0.001f, this.Sun.MeshContrast));
		Shader.SetGlobalFloat(this.Resources.ID_SunMeshBrightness, this.Sun.MeshBrightness * (1f - this.Atmosphere.Fogginess));
		Shader.SetGlobalFloat(this.Resources.ID_MoonMeshContrast, 1f / Mathf.Max(0.001f, this.Moon.MeshContrast));
		Shader.SetGlobalFloat(this.Resources.ID_MoonMeshBrightness, this.Moon.MeshBrightness * (1f - this.Atmosphere.Fogginess));
		Shader.SetGlobalVector(this.Resources.ID_kBetaMie, this.kBetaMie);
		Shader.SetGlobalVector(this.Resources.ID_kSun, this.kSun);
		Shader.SetGlobalVector(this.Resources.ID_k4PI, this.k4PI);
		Shader.SetGlobalVector(this.Resources.ID_kRadius, this.kRadius);
		Shader.SetGlobalVector(this.Resources.ID_kScale, this.kScale);
		Shader.SetGlobalMatrix(this.Resources.ID_World2Sky, this.Components.DomeTransform.worldToLocalMatrix);
		Shader.SetGlobalMatrix(this.Resources.ID_Sky2World, this.Components.DomeTransform.localToWorldMatrix);
	}

	private float ShaderScale(float inCos)
	{
		float num = 1f - inCos;
		return 0.25f * Mathf.Exp(-0.00287f + num * (0.459f + num * (3.83f + num * (-6.8f + num * 5.25f))));
	}

	private float ShaderMiePhase(float eyeCos, float eyeCos2)
	{
		return this.kBetaMie.x * (1f + eyeCos2) / Mathf.Pow(this.kBetaMie.y + this.kBetaMie.z * eyeCos, 1.5f);
	}

	private float ShaderRayleighPhase(float eyeCos2)
	{
		return 0.75f + 0.75f * eyeCos2;
	}

	private Color ShaderNightSkyColor(Vector3 dir)
	{
		dir.y = Mathf.Max(0f, dir.y);
		return this.MoonSkyColor * (1f - 0.75f * dir.y);
	}

	private Color ShaderMoonHaloColor(Vector3 dir)
	{
		return this.MoonHaloColor * Mathf.Pow(Mathf.Max(0f, Vector3.Dot(dir, this.LocalMoonDirection)), 1f / this.Moon.MeshSize);
	}

	private Color TOD_HDR2LDR(Color color)
	{
		return new Color(1f - Mathf.Pow(2f, -this.Atmosphere.Brightness * color.r), 1f - Mathf.Pow(2f, -this.Atmosphere.Brightness * color.g), 1f - Mathf.Pow(2f, -this.Atmosphere.Brightness * color.b), color.a);
	}

	private Color TOD_GAMMA2LINEAR(Color color)
	{
		return new Color(color.r * color.r, color.g * color.g, color.b * color.b, color.a);
	}

	private Color TOD_LINEAR2GAMMA(Color color)
	{
		return new Color(Mathf.Sqrt(color.r), Mathf.Sqrt(color.g), Mathf.Sqrt(color.b), color.a);
	}

	private Color ShaderScatteringColor(Vector3 dir, bool directLight = true)
	{
		dir.y = Mathf.Max(0f, dir.y);
		float x = this.kRadius.x;
		float y = this.kRadius.y;
		float w = this.kRadius.w;
		float x2 = this.kScale.x;
		float z = this.kScale.z;
		float w2 = this.kScale.w;
		float x3 = this.k4PI.x;
		float y2 = this.k4PI.y;
		float z2 = this.k4PI.z;
		float w3 = this.k4PI.w;
		float x4 = this.kSun.x;
		float y3 = this.kSun.y;
		float z3 = this.kSun.z;
		float w4 = this.kSun.w;
		Vector3 vector = new Vector3(0f, x + w2, 0f);
		float num = Mathf.Sqrt(w + y * dir.y * dir.y - y) - x * dir.y;
		float num2 = Mathf.Exp(z * -w2);
		float inCos = Vector3.Dot(dir, vector) / (x + w2);
		float num3 = num2 * this.ShaderScale(inCos);
		float num4 = num / 2f;
		float num5 = num4 * x2;
		Vector3 vector2 = dir * num4;
		Vector3 vector3 = vector + vector2 * 0.5f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		for (int i = 0; i < 2; i++)
		{
			float magnitude = vector3.magnitude;
			float num9 = 1f / magnitude;
			float num10 = Mathf.Exp(z * (x - magnitude));
			float num11 = num10 * num5;
			float inCos2 = Vector3.Dot(dir, vector3) * num9;
			float inCos3 = Vector3.Dot(this.LocalSunDirection, vector3) * num9;
			float num12 = num3 + num10 * (this.ShaderScale(inCos3) - this.ShaderScale(inCos2));
			float num13 = Mathf.Exp(-num12 * (x3 + w3));
			float num14 = Mathf.Exp(-num12 * (y2 + w3));
			float num15 = Mathf.Exp(-num12 * (z2 + w3));
			num6 += num13 * num11;
			num7 += num14 * num11;
			num8 += num15 * num11;
			vector3 += vector2;
		}
		float num16 = this.SunSkyColor.r * num6 * x4;
		float num17 = this.SunSkyColor.g * num7 * y3;
		float num18 = this.SunSkyColor.b * num8 * z3;
		float num19 = this.SunSkyColor.r * num6 * w4;
		float num20 = this.SunSkyColor.g * num7 * w4;
		float num21 = this.SunSkyColor.b * num8 * w4;
		float num22 = 0f;
		float num23 = 0f;
		float num24 = 0f;
		float num25 = Vector3.Dot(this.LocalSunDirection, dir);
		float eyeCos = num25 * num25;
		float num26 = this.ShaderRayleighPhase(eyeCos);
		num22 += num26 * num16;
		num23 += num26 * num17;
		num24 += num26 * num18;
		if (directLight)
		{
			float num27 = this.ShaderMiePhase(num25, eyeCos);
			num22 += num27 * num19;
			num23 += num27 * num20;
			num24 += num27 * num21;
		}
		Color color = this.ShaderNightSkyColor(dir);
		num22 += color.r;
		num23 += color.g;
		num24 += color.b;
		if (directLight)
		{
			Color color2 = this.ShaderMoonHaloColor(dir);
			num22 += color2.r;
			num23 += color2.g;
			num24 += color2.b;
		}
		num22 = Mathf.Lerp(num22, this.FogColor.r, this.Atmosphere.Fogginess);
		num23 = Mathf.Lerp(num23, this.FogColor.g, this.Atmosphere.Fogginess);
		num24 = Mathf.Lerp(num24, this.FogColor.b, this.Atmosphere.Fogginess);
		num22 = Mathf.Pow(num22 * this.Atmosphere.Brightness, this.Atmosphere.Contrast);
		num23 = Mathf.Pow(num23 * this.Atmosphere.Brightness, this.Atmosphere.Contrast);
		num24 = Mathf.Pow(num24 * this.Atmosphere.Brightness, this.Atmosphere.Contrast);
		return new Color(num22, num23, num24, 1f);
	}

	private void Initialize()
	{
		this.Components = base.GetComponent<TOD_Components>();
		this.Components.Initialize();
		this.Resources = base.GetComponent<TOD_Resources>();
		this.Resources.Initialize();
		TOD_Sky.instances.Add(this);
		this.Initialized = true;
		this.m_DefaultCloudsCoverage = this.Clouds.Coverage;
		this.m_DefaultCloudsOpacity = this.Clouds.Opacity;
	}

	private void Cleanup()
	{
		if (this.Probe)
		{
			UnityEngine.Object.Destroy(this.Probe.gameObject);
		}
		TOD_Sky.instances.Remove(this);
		this.Initialized = false;
	}

	protected void OnEnable()
	{
		this.LateUpdate();
	}

	protected void OnDisable()
	{
		this.Cleanup();
	}

	protected void LateUpdate()
	{
		if (!this.Initialized)
		{
			this.Initialize();
		}
		this.UpdateScattering();
		this.UpdateCelestials();
		this.UpdateQualitySettings();
		this.UpdateRenderSettings();
		this.UpdateShaderKeywords();
		this.UpdateShaderProperties();
	}

	protected void OnValidate()
	{
		this.Cycle.DateTime = this.Cycle.DateTime;
	}

	private const float pi = 3.14159274f;

	private const float tau = 6.28318548f;

	private static List<TOD_Sky> instances = new List<TOD_Sky>();

	private int probeRenderID = -1;

	[Tooltip("Auto: Use the player settings.\nLinear: Force linear color space.\nGamma: Force gamma color space.")]
	public TOD_ColorSpaceType ColorSpace;

	[Tooltip("Auto: Use the camera settings.\nHDR: Force high dynamic range.\nLDR: Force low dynamic range.")]
	public TOD_ColorRangeType ColorRange;

	[Tooltip("Raw: Write color without modifications.\nDithered: Add dithering to reduce banding.")]
	public TOD_ColorOutputType ColorOutput = TOD_ColorOutputType.Dithered;

	[Tooltip("Per Vertex: Calculate sky color per vertex.\nPer Pixel: Calculate sky color per pixel.")]
	public TOD_SkyQualityType SkyQuality;

	[Tooltip("Low: Only recommended for very old mobile devices.\nMedium: Simplified cloud shading.\nHigh: Physically based cloud shading.")]
	public TOD_CloudQualityType CloudQuality = TOD_CloudQualityType.High;

	[Tooltip("Low: Only recommended for very old mobile devices.\nMedium: Simplified mesh geometry.\nHigh: Detailed mesh geometry.")]
	public TOD_MeshQualityType MeshQuality = TOD_MeshQualityType.High;

	[Tooltip("Low: Recommended for most mobile devices.\nMedium: Includes most visible stars.\nHigh: Includes all visible stars.")]
	public TOD_StarQualityType StarQuality = TOD_StarQualityType.High;

	public TOD_CycleParameters Cycle;

	public TOD_WorldParameters World;

	public TOD_AtmosphereParameters Atmosphere;

	public TOD_DayParameters Day;

	public TOD_NightParameters Night;

	public TOD_SunParameters Sun;

	public TOD_MoonParameters Moon;

	public TOD_StarParameters Stars;

	public TOD_CloudParameters Clouds;

	public TOD_LightParameters Light;

	public TOD_FogParameters Fog;

	public TOD_AmbientParameters Ambient;

	public TOD_ReflectionParameters Reflection;

	private float m_AmbientIntensity = float.MinValue;

	private float timeSinceLightUpdate = float.MaxValue;

	private float timeSinceAmbientUpdate = float.MaxValue;

	private float timeSinceReflectionUpdate = float.MaxValue;

	private const int TOD_SAMPLES = 2;

	private Vector3 kBetaMie;

	private Vector4 kSun;

	private Vector4 k4PI;

	private Vector4 kRadius;

	private Vector4 kScale;

	private float m_DefaultCloudsCoverage = 0.7f;

	private float m_DefaultCloudsOpacity = 0.4f;
}
