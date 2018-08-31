using System;
using UnityEngine;

public class TOD_Resources : MonoBehaviour
{
	public int ID_SunLightColor { get; private set; }

	public int ID_MoonLightColor { get; private set; }

	public int ID_SunSkyColor { get; private set; }

	public int ID_MoonSkyColor { get; private set; }

	public int ID_SunMeshColor { get; private set; }

	public int ID_MoonMeshColor { get; private set; }

	public int ID_SunCloudColor { get; private set; }

	public int ID_MoonCloudColor { get; private set; }

	public int ID_FogColor { get; private set; }

	public int ID_GroundColor { get; private set; }

	public int ID_AmbientColor { get; private set; }

	public int ID_SunDirection { get; private set; }

	public int ID_MoonDirection { get; private set; }

	public int ID_LightDirection { get; private set; }

	public int ID_LocalSunDirection { get; private set; }

	public int ID_LocalMoonDirection { get; private set; }

	public int ID_LocalLightDirection { get; private set; }

	public int ID_Contrast { get; private set; }

	public int ID_Brightness { get; private set; }

	public int ID_Fogginess { get; private set; }

	public int ID_Directionality { get; private set; }

	public int ID_MoonHaloPower { get; private set; }

	public int ID_MoonHaloColor { get; private set; }

	public int ID_CloudSize { get; private set; }

	public int ID_CloudOpacity { get; private set; }

	public int ID_CloudCoverage { get; private set; }

	public int ID_CloudSharpness { get; private set; }

	public int ID_CloudDensity { get; private set; }

	public int ID_CloudAttenuation { get; private set; }

	public int ID_CloudSaturation { get; private set; }

	public int ID_CloudScattering { get; private set; }

	public int ID_CloudBrightness { get; private set; }

	public int ID_CloudMultiplier { get; private set; }

	public int ID_CloudOffset { get; private set; }

	public int ID_CloudWind { get; private set; }

	public int ID_StarSize { get; private set; }

	public int ID_StarBrightness { get; private set; }

	public int ID_StarVisibility { get; private set; }

	public int ID_SunMeshContrast { get; private set; }

	public int ID_SunMeshBrightness { get; private set; }

	public int ID_MoonMeshContrast { get; private set; }

	public int ID_MoonMeshBrightness { get; private set; }

	public int ID_kBetaMie { get; private set; }

	public int ID_kSun { get; private set; }

	public int ID_k4PI { get; private set; }

	public int ID_kRadius { get; private set; }

	public int ID_kScale { get; private set; }

	public int ID_World2Sky { get; private set; }

	public int ID_Sky2World { get; private set; }

	public void Initialize()
	{
		this.ID_SunLightColor = Shader.PropertyToID("TOD_SunLightColor");
		this.ID_MoonLightColor = Shader.PropertyToID("TOD_MoonLightColor");
		this.ID_SunSkyColor = Shader.PropertyToID("TOD_SunSkyColor");
		this.ID_MoonSkyColor = Shader.PropertyToID("TOD_MoonSkyColor");
		this.ID_SunMeshColor = Shader.PropertyToID("TOD_SunMeshColor");
		this.ID_MoonMeshColor = Shader.PropertyToID("TOD_MoonMeshColor");
		this.ID_SunCloudColor = Shader.PropertyToID("TOD_SunCloudColor");
		this.ID_MoonCloudColor = Shader.PropertyToID("TOD_MoonCloudColor");
		this.ID_FogColor = Shader.PropertyToID("TOD_FogColor");
		this.ID_GroundColor = Shader.PropertyToID("TOD_GroundColor");
		this.ID_AmbientColor = Shader.PropertyToID("TOD_AmbientColor");
		this.ID_SunDirection = Shader.PropertyToID("TOD_SunDirection");
		this.ID_MoonDirection = Shader.PropertyToID("TOD_MoonDirection");
		this.ID_LightDirection = Shader.PropertyToID("TOD_LightDirection");
		this.ID_LocalSunDirection = Shader.PropertyToID("TOD_LocalSunDirection");
		this.ID_LocalMoonDirection = Shader.PropertyToID("TOD_LocalMoonDirection");
		this.ID_LocalLightDirection = Shader.PropertyToID("TOD_LocalLightDirection");
		this.ID_Contrast = Shader.PropertyToID("TOD_Contrast");
		this.ID_Brightness = Shader.PropertyToID("TOD_Brightness");
		this.ID_Fogginess = Shader.PropertyToID("TOD_Fogginess");
		this.ID_Directionality = Shader.PropertyToID("TOD_Directionality");
		this.ID_MoonHaloPower = Shader.PropertyToID("TOD_MoonHaloPower");
		this.ID_MoonHaloColor = Shader.PropertyToID("TOD_MoonHaloColor");
		this.ID_CloudSize = Shader.PropertyToID("TOD_CloudSize");
		this.ID_CloudOpacity = Shader.PropertyToID("TOD_CloudOpacity");
		this.ID_CloudCoverage = Shader.PropertyToID("TOD_CloudCoverage");
		this.ID_CloudSharpness = Shader.PropertyToID("TOD_CloudSharpness");
		this.ID_CloudDensity = Shader.PropertyToID("TOD_CloudDensity");
		this.ID_CloudAttenuation = Shader.PropertyToID("TOD_CloudAttenuation");
		this.ID_CloudSaturation = Shader.PropertyToID("TOD_CloudSaturation");
		this.ID_CloudScattering = Shader.PropertyToID("TOD_CloudScattering");
		this.ID_CloudBrightness = Shader.PropertyToID("TOD_CloudBrightness");
		this.ID_CloudOffset = Shader.PropertyToID("TOD_CloudOffset");
		this.ID_CloudWind = Shader.PropertyToID("TOD_CloudWind");
		this.ID_StarSize = Shader.PropertyToID("TOD_StarSize");
		this.ID_StarBrightness = Shader.PropertyToID("TOD_StarBrightness");
		this.ID_StarVisibility = Shader.PropertyToID("TOD_StarVisibility");
		this.ID_SunMeshContrast = Shader.PropertyToID("TOD_SunMeshContrast");
		this.ID_SunMeshBrightness = Shader.PropertyToID("TOD_SunMeshBrightness");
		this.ID_MoonMeshContrast = Shader.PropertyToID("TOD_MoonMeshContrast");
		this.ID_MoonMeshBrightness = Shader.PropertyToID("TOD_MoonMeshBrightness");
		this.ID_kBetaMie = Shader.PropertyToID("TOD_kBetaMie");
		this.ID_kSun = Shader.PropertyToID("TOD_kSun");
		this.ID_k4PI = Shader.PropertyToID("TOD_k4PI");
		this.ID_kRadius = Shader.PropertyToID("TOD_kRadius");
		this.ID_kScale = Shader.PropertyToID("TOD_kScale");
		this.ID_World2Sky = Shader.PropertyToID("TOD_World2Sky");
		this.ID_Sky2World = Shader.PropertyToID("TOD_Sky2World");
	}

	public Material Skybox;

	public Mesh MoonLOD0;

	public Mesh MoonLOD1;

	public Mesh MoonLOD2;

	public Mesh SkyLOD0;

	public Mesh SkyLOD1;

	public Mesh SkyLOD2;

	public Mesh CloudsLOD0;

	public Mesh CloudsLOD1;

	public Mesh CloudsLOD2;

	public Mesh StarsLOD0;

	public Mesh StarsLOD1;

	public Mesh StarsLOD2;
}
