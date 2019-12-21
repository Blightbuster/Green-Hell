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
				this.AddSample().Load(key2);
			}
		}
	}

	public void Save(Key key)
	{
		key.AddVariable().SValue = this.m_Name;
		for (int i = 0; i < this.m_Params.Count; i++)
		{
			key.AddKey("Param").AddVariable().SValue = this.m_Params[i].m_Name;
		}
		for (int j = 0; j < this.m_Samples.Count; j++)
		{
			Key key2 = key.AddKey("Sample");
			this.m_Samples[j].Save(key2);
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

	public void Play(Component requester, float fade_in = 0f)
	{
		this.m_Requesters.Add(requester);
		if (requester is IMSMultisampleController)
		{
			this.m_RequesterControllers.Add(requester as IMSMultisampleController);
		}
		if (this.m_State == MSState.FadeIn || this.m_State == MSState.Playing)
		{
			return;
		}
		if (this.m_FirstTimeUse)
		{
			this.m_VolumeMul = 0f;
			this.m_FirstTimeUse = false;
		}
		this.m_VolumeMulOnStartFade = this.m_VolumeMul;
		this.m_State = MSState.FadeIn;
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

	public void Stop(Component requester, float fade_out)
	{
		this.m_Requesters.Remove(requester);
		if (requester is IMSMultisampleController)
		{
			this.m_RequesterControllers.Remove(requester as IMSMultisampleController);
		}
		if (this.m_Requesters.Count > 0)
		{
			if (this.m_RequesterControllers.Count == 0)
			{
				this.SetSpatialBlend(0f, null);
			}
			return;
		}
		this.m_VolumeMulOnStartFade = this.m_VolumeMul;
		this.m_StopTime = Time.time;
		this.m_FadeOutTime = fade_out;
		this.m_State = MSState.FadeOut;
		if (fade_out == 0f)
		{
			this.Stop();
		}
	}

	public void ForceStop()
	{
		this.m_Requesters.Clear();
		this.m_RequesterControllers.Clear();
		this.m_VolumeMulOnStartFade = this.m_VolumeMul;
		this.m_StopTime = Time.time;
		this.m_FadeOutTime = 0f;
		this.m_State = MSState.FadeOut;
		this.Stop();
	}

	private void Stop()
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
		this.m_State = MSState.None;
		this.m_ChangeRainforestAmbientVolume = false;
		this.m_RainforestAmbientVolume = 1f;
	}

	public void Update()
	{
		if (this.m_State == MSState.FadeOut && Time.time > this.m_StopTime + this.m_FadeOutTime)
		{
			this.Stop();
			return;
		}
		this.UpdateParams();
		this.UpdateSpatialBlend();
		float num = 1f;
		if (this.m_RainForestAmbient)
		{
			num = MSMultiSample.s_RainforestWantedAmbientVolume;
		}
		else if (this.m_State == MSState.FadeIn)
		{
			if (this.m_ChangeRainforestAmbientVolume)
			{
				Mathf.Clamp01((Time.time - this.m_PlayTime) / this.m_PlayTime + this.m_FadeInTime);
				MSMultiSample.s_RainforestWantedAmbientVolume = CJTools.Math.GetProportionalClamp(1f, this.m_RainforestAmbientVolume, Time.time, this.m_PlayTime, this.m_PlayTime + this.m_FadeInTime);
			}
			num = CJTools.Math.GetProportionalClamp(this.m_VolumeMulOnStartFade, 1f, Time.time, this.m_PlayTime, this.m_PlayTime + this.m_FadeInTime);
			if (num >= 1f)
			{
				this.m_State = MSState.Playing;
			}
		}
		else if (this.m_State == MSState.FadeOut)
		{
			if (this.m_ChangeRainforestAmbientVolume)
			{
				Mathf.Clamp01((Time.time - this.m_StopTime) / this.m_StopTime + this.m_FadeInTime);
				MSMultiSample.s_RainforestWantedAmbientVolume = CJTools.Math.GetProportionalClamp(this.m_RainforestAmbientVolume, 1f, Time.time, this.m_StopTime, this.m_StopTime + this.m_FadeOutTime);
			}
			num = CJTools.Math.GetProportionalClamp(this.m_VolumeMulOnStartFade, 0f, Time.time, this.m_StopTime, this.m_StopTime + this.m_FadeOutTime);
		}
		else if (this.m_State == MSState.Playing && this.m_ChangeRainforestAmbientVolume)
		{
			MSMultiSample.s_RainforestWantedAmbientVolume = this.m_RainforestAmbientVolume;
		}
		this.m_VolumeMul = num;
		for (int i = 0; i < this.m_Samples.Count; i++)
		{
			MSSample mssample = this.m_Samples[i];
			if (mssample != null)
			{
				mssample.Update(num);
			}
		}
	}

	private void UpdateSpatialBlend()
	{
		Vector3? source_position = null;
		float num = 0f;
		float num2 = float.MaxValue;
		bool flag = true;
		foreach (IMSMultisampleController imsmultisampleController in this.m_RequesterControllers)
		{
			if (imsmultisampleController != null)
			{
				float num3;
				Vector3? vector;
				float num4;
				imsmultisampleController.UpdateSpatialBlend(out num3, out vector, out num4);
				if (flag || num > num3 || (num == num3 && num4 < num2))
				{
					num = num3;
					source_position = vector;
					num2 = num4;
					flag = false;
				}
			}
		}
		this.SetSpatialBlend(num, source_position);
	}

	private void SetSpatialBlend(float spatial_blend, Vector3? source_position = null)
	{
		for (int i = 0; i < this.m_Samples.Count; i++)
		{
			this.m_Samples[i].SetSpatialBlend(spatial_blend, source_position);
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
				float value2 = Player.Get().IsCameraUnderwater() ? 1f : 0f;
				this.m_ParamsMap[hash] = value2;
			}
			else if (hash == MSMultiSample.s_RainVolHash)
			{
				float value3 = RainManager.Get().m_WeatherInterpolated * MainLevel.Instance.m_RainVolume;
				this.m_ParamsMap[hash] = value3;
			}
			else if (hash == MSMultiSample.s_RainTentVolHash)
			{
				float num = (RainManager.Get().IsInRainCutterTent(Player.Get().GetLEyeTransform().position) && RainManager.Get().m_WeatherInterpolated > 0.9f) ? 1f : 0f;
				float num2 = 0f;
				if (!this.m_ParamsMap.TryGetValue(hash, out num2))
				{
					this.m_ParamsMap[hash] = 0f;
				}
				else
				{
					Dictionary<int, float> paramsMap = this.m_ParamsMap;
					int key = hash;
					paramsMap[key] += (num - this.m_ParamsMap[hash]) * Time.deltaTime;
					this.m_ParamsMap[hash] = Mathf.Clamp01(this.m_ParamsMap[hash]);
				}
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

	private MSState m_State;

	public bool m_ChangeRainforestAmbientVolume;

	public float m_RainforestAmbientVolume = 1f;

	private List<Component> m_Requesters = new List<Component>();

	private List<IMSMultisampleController> m_RequesterControllers = new List<IMSMultisampleController>();

	private float m_PlayTime;

	private float m_FadeInTime;

	private bool m_FirstTimeUse = true;

	private float m_StopTime = float.MaxValue;

	private float m_FadeOutTime;

	private float m_VolumeMul = 1f;

	private float m_VolumeMulOnStartFade = 1f;

	public bool m_RainForestAmbient;

	public static float s_RainforestWantedAmbientVolume = 1f;

	private static int s_DayTimeVolHash = Animator.StringToHash("DayTime_Vol");

	private static int s_WaterVolHash = Animator.StringToHash("Water_Vol");

	private static int s_RainVolHash = Animator.StringToHash("Rain_Vol");

	private static int s_WindVolHash = Animator.StringToHash("Wind_Vol");

	private static int s_WindPitchHash = Animator.StringToHash("Wind_Pitch");

	private static int s_AreaDensityVolHash = Animator.StringToHash("AreaDensity_Vol");

	private static int s_RainTentVolHash = Animator.StringToHash("Rain_Tent_Vol");
}
