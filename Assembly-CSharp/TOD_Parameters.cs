using System;

[Serializable]
public class TOD_Parameters
{
	public TOD_Parameters()
	{
	}

	public TOD_Parameters(TOD_Sky sky)
	{
		this.Cycle = sky.Cycle;
		this.World = sky.World;
		this.Atmosphere = sky.Atmosphere;
		this.Day = sky.Day;
		this.Night = sky.Night;
		this.Sun = sky.Sun;
		this.Moon = sky.Moon;
		this.Light = sky.Light;
		this.Stars = sky.Stars;
		this.Clouds = sky.Clouds;
		this.Fog = sky.Fog;
		this.Ambient = sky.Ambient;
		this.Reflection = sky.Reflection;
	}

	public void ToSky(TOD_Sky sky)
	{
		sky.Cycle = this.Cycle;
		sky.World = this.World;
		sky.Atmosphere = this.Atmosphere;
		sky.Day = this.Day;
		sky.Night = this.Night;
		sky.Sun = this.Sun;
		sky.Moon = this.Moon;
		sky.Light = this.Light;
		sky.Stars = this.Stars;
		sky.Clouds = this.Clouds;
		sky.Fog = this.Fog;
		sky.Ambient = this.Ambient;
		sky.Reflection = this.Reflection;
	}

	public TOD_CycleParameters Cycle;

	public TOD_WorldParameters World;

	public TOD_AtmosphereParameters Atmosphere;

	public TOD_DayParameters Day;

	public TOD_NightParameters Night;

	public TOD_SunParameters Sun;

	public TOD_MoonParameters Moon;

	public TOD_LightParameters Light;

	public TOD_StarParameters Stars;

	public TOD_CloudParameters Clouds;

	public TOD_FogParameters Fog;

	public TOD_AmbientParameters Ambient;

	public TOD_ReflectionParameters Reflection;
}
