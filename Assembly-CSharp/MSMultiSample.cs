using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class MSMultiSample
{
	public void Load(Key key)
	{
		this.m_Name = key.GetVariable(0).SValue;
		this.m_Params.Clear();
		this.m_Samples.Clear();
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "Param")
			{
				MSCurveHash mscurveHash = new MSCurveHash();
				mscurveHash.m_Name = key2.GetVariable(0).SValue;
				mscurveHash.m_Hash = Animator.StringToHash(mscurveHash.m_Name);
				this.m_Params.Add(mscurveHash);
			}
			else if (key2.GetName() == "Sample")
			{
				MSSample mssample = this.AddSample();
				mssample.Load(key2);
			}
		}
	}

	public void Save(Key key)
	{
		CJVariable cjvariable = key.AddVariable();
		cjvariable.SValue = this.m_Name;
		for (int i = 0; i < this.m_Params.Count; i++)
		{
			Key key2 = key.AddKey("Param");
			cjvariable = key2.AddVariable();
			cjvariable.SValue = this.m_Params[i].m_Name;
		}
		for (int j = 0; j < this.m_Samples.Count; j++)
		{
			Key key3 = key.AddKey("Sample");
			this.m_Samples[j].Save(key3);
		}
	}

	public void Reload()
	{
		for (int i = 0; i < this.m_Samples.Count; i++)
		{
			this.m_Samples[i].Reload();
		}
	}

	public MSSample AddSample()
	{
		MSSample mssample = new MSSample();
		mssample.m_MultiSample = this;
		this.m_Samples.Add(mssample);
		return mssample;
	}

	public void Play(float fade_in = 0f)
	{
		this.m_PlayTime = Time.time;
		this.m_FadeInTime = fade_in;
		this.m_Playing = true;
		for (int i = 0; i < this.m_Samples.Count; i++)
		{
			MSSample mssample = this.m_Samples[i];
			if (mssample != null)
			{
				mssample.Play();
			}
		}
	}

	public void Stop(float fade_out)
	{
		this.m_StopTime = fade_out;
		this.m_FadeOutTime = fade_out;
	}

	public void Stop()
	{
		this.m_Playing = false;
		for (int i = 0; i < this.m_Samples.Count; i++)
		{
			MSSample mssample = this.m_Samples[i];
			if (mssample != null)
			{
				mssample.Stop();
			}
		}
		this.m_StopTime = float.MaxValue;
	}

	public void Update()
	{
		if (Time.time > this.m_StopTime + this.m_FadeOutTime)
		{
			this.Stop();
			return;
		}
		this.UpdateParams();
		float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, Time.time, this.m_PlayTime, this.m_PlayTime + this.m_FadeInTime);
		if (Time.time > this.m_StopTime)
		{
			proportionalClamp = CJTools.Math.GetProportionalClamp(1f, 0f, Time.time, this.m_StopTime, this.m_StopTime - this.m_FadeOutTime);
		}
		for (int i = 0; i < this.m_Samples.Count; i++)
		{
			MSSample mssample = this.m_Samples[i];
			if (mssample != null)
			{
				mssample.Update(proportionalClamp);
			}
		}
	}

	private void UpdateParams()
	{
		for (int i = 0; i < this.m_Params.Count; i++)
		{
			int hash = this.m_Params[i].m_Hash;
			if (hash == MSMultiSample.s_DayTimeVolHash)
			{
				float value = MainLevel.Instance.m_TODSky.Cycle.Hour / 24f;
				this.m_ParamsMap[hash] = value;
			}
			else if (hash == MSMultiSample.s_WaterVolHash)
			{
				float value2 = (!SwimController.Get().IsActive() || SwimController.Get().m_State != SwimState.Dive) ? 0f : 1f;
				this.m_ParamsMap[hash] = value2;
			}
			else if (hash == MSMultiSample.s_RainVolHash)
			{
				float weatherInterpolated = RainManager.Get().m_WeatherInterpolated;
				this.m_ParamsMap[hash] = weatherInterpolated;
			}
			else if (hash == MSMultiSample.s_WindVolHash)
			{
				float wind = RainManager.Get().m_Wind;
				this.m_ParamsMap[hash] = wind;
			}
			else if (hash == MSMultiSample.s_WindPitchHash)
			{
				float wind2 = RainManager.Get().m_Wind;
				this.m_ParamsMap[hash] = wind2;
			}
			else if (hash == MSMultiSample.s_AreaDensityVolHash)
			{
				float areaDensity = RainManager.Get().m_AreaDensity;
				this.m_ParamsMap[hash] = areaDensity;
			}
		}
	}

	public static string s_Script = "Scripts/MultiSample/MultiSamples";

	public static string s_DefaultName = "Multisample_Name";

	public string m_Name = string.Empty;

	public List<MSSample> m_Samples = new List<MSSample>();

	public List<MSCurveHash> m_Params = new List<MSCurveHash>();

	public Dictionary<int, float> m_ParamsMap = new Dictionary<int, float>();

	public bool m_Playing;

	private float m_PlayTime;

	private float m_FadeInTime;

	private float m_StopTime = float.MaxValue;

	private float m_FadeOutTime;

	private static int s_DayTimeVolHash = Animator.StringToHash("DayTime_Vol");

	private static int s_WaterVolHash = Animator.StringToHash("Water_Vol");

	private static int s_RainVolHash = Animator.StringToHash("Rain_Vol");

	private static int s_WindVolHash = Animator.StringToHash("Wind_Vol");

	private static int s_WindPitchHash = Animator.StringToHash("Wind_Pitch");

	private static int s_AreaDensityVolHash = Animator.StringToHash("AreaDensity_Vol");
}
