using System;
using System.Collections.Generic;
using System.IO;
using CJTools;
using UnityEngine;

public class RainManager : ReplicatedBehaviour, ISaveLoad
{
	public event Action OnRainBegin;

	public event Action OnRainEnd;

	public static RainManager Get()
	{
		return RainManager.s_Instance;
	}

	private void Awake()
	{
		RainManager.s_Instance = this;
	}

	private void Start()
	{
		this.ParseScript();
		RainCone[] array = (RainCone[])Resources.FindObjectsOfTypeAll(typeof(RainCone));
		if (array.Length != 1)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
		}
		this.m_RainCone = array[0].gameObject;
		this.m_Material = this.m_RainCone.GetComponent<MeshRenderer>().material;
		this.m_ShaderPropertyId = Shader.PropertyToID("_Intensity");
		this.m_ConeStartRotation = this.m_RainCone.transform.rotation;
		this.m_RTP = Terrain.activeTerrain.GetComponent<ReliefTerrain>();
		DebugUtils.Assert(this.m_RTP, true);
		this.m_ScenarioRain = new RainData();
		this.m_ScenarioRain.m_RainDuration = float.MaxValue;
		this.m_ScenarioRain.m_RainInterval = 14f;
		this.m_ScenarioNoRain = new RainData();
		this.m_ScenarioNoRain.m_RainDuration = 0f;
		this.m_ScenarioNoRain.m_RainInterval = float.MaxValue;
		this.LoadDensityData();
		this.SetupAudio();
	}

	private void SetupAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		this.m_ThunderClips.Clear();
		for (int i = 1; i < 10; i++)
		{
			this.m_ThunderClips.Add((AudioClip)Resources.Load("Sounds/Ambience/amb_thunder_distant_0" + i.ToString()));
		}
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Weather/Rain.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Rain")
			{
				RainData rainData = new RainData();
				rainData.m_RainDuration = key.GetVariable(0).FValue;
				rainData.m_RainInterval = key.GetVariable(1).FValue;
				this.m_RainData.Add(rainData);
			}
			else if (key.GetName() == "RainCollectorFillPerSecondRain")
			{
				this.m_RainCollectorFillPerSecondRain = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "RainCollectorFillPerSecondNoRain")
			{
				if (GreenHellGame.ROADSHOW_DEMO)
				{
					this.m_RainCollectorFillPerSecondNoRain = 10f;
				}
				else
				{
					this.m_RainCollectorFillPerSecondNoRain = key.GetVariable(0).FValue;
				}
			}
			else if (key.GetName() == "Period")
			{
				RainPeriod rainPeriod = new RainPeriod();
				rainPeriod.m_Type = (RainPeriodType)Enum.Parse(typeof(RainPeriodType), key.GetVariable(0).SValue);
				rainPeriod.m_Duration = key.GetVariable(1).FValue;
				this.m_Periods.Add(rainPeriod);
			}
		}
	}

	private void RainManagerUpdate()
	{
		if (GreenHellGame.TWITCH_DEMO)
		{
			if (this.m_DebugEnabled)
			{
				this.m_WeatherInterpolated += Time.deltaTime * 0.15f;
			}
			else
			{
				this.m_WeatherInterpolated -= Time.deltaTime * 0.15f;
			}
			this.m_WeatherInterpolated = Mathf.Clamp01(this.m_WeatherInterpolated);
			if (this.m_WeatherInterpolated >= 1f)
			{
				this.m_Wetness += Time.deltaTime * 0.5f;
			}
			else
			{
				this.m_Wetness -= Time.deltaTime * 0.2f;
			}
			this.m_Wetness = Mathf.Clamp01(this.m_Wetness);
			this.SetShaderProperty();
			this.SendRainMessages();
			this.UpdateTerrainWetness();
			return;
		}
		if (this.m_DebugEnabled)
		{
			if (this.m_DebugRainEnabled)
			{
				this.m_WeatherInterpolated += Time.deltaTime * 0.15f;
			}
			else
			{
				this.m_WeatherInterpolated -= Time.deltaTime * 0.15f;
			}
			this.m_WeatherInterpolated = Mathf.Clamp01(this.m_WeatherInterpolated);
			if (this.m_WeatherInterpolated >= 1f)
			{
				this.m_Wetness += Time.deltaTime * 0.5f;
			}
			else
			{
				this.m_Wetness -= Time.deltaTime * 0.2f;
			}
			this.m_Wetness = Mathf.Clamp01(this.m_Wetness);
			this.SetShaderProperty();
			this.SendRainMessages();
			this.UpdateTerrainWetness();
			return;
		}
		if (this.m_CurrentPeriod < 0)
		{
			this.m_CurrentPeriod = 0;
			if (MainLevel.Instance.GetCurrentTimeMinutes() < 0.1f)
			{
				MainLevel.Instance.UpdateCurentTimeInMinutes();
			}
			this.m_PeriodStartTime = MainLevel.Instance.GetCurrentTimeMinutes();
		}
		if (MainLevel.Instance.GetCurrentTimeMinutes() > this.m_PeriodStartTime + this.m_Periods[this.m_CurrentPeriod].m_Duration)
		{
			this.m_CurrentPeriod++;
			if (this.m_CurrentPeriod >= this.m_Periods.Count)
			{
				this.m_CurrentPeriod = 0;
			}
			this.m_PeriodStartTime = MainLevel.Instance.GetCurrentTimeMinutes();
		}
		if ((this.m_Periods[this.m_CurrentPeriod].m_Type == RainPeriodType.Dry && this.m_CurrentRainData != this.m_ScenarioRain) || (this.m_Periods[this.m_CurrentPeriod].m_Type == RainPeriodType.Wet && this.m_CurrentRainData != this.m_ScenarioNoRain))
		{
			if (this.m_Periods[this.m_CurrentPeriod].m_Type == RainPeriodType.Wet)
			{
				this.m_WeatherInterpolated += Time.deltaTime * 0.15f;
			}
			else
			{
				this.m_WeatherInterpolated -= Time.deltaTime * 0.15f;
			}
			this.m_WeatherInterpolated = Mathf.Clamp01(this.m_WeatherInterpolated);
			if (this.m_WeatherInterpolated >= 1f)
			{
				this.m_Wetness += Time.deltaTime * 0.5f;
			}
			else
			{
				this.m_Wetness -= Time.deltaTime * 0.2f;
			}
			this.m_Wetness = Mathf.Clamp01(this.m_Wetness);
			this.SetShaderProperty();
			this.SendRainMessages();
			this.UpdateTerrainWetness();
			return;
		}
		float currentTimeMinutes = MainLevel.Instance.GetCurrentTimeMinutes();
		if (this.m_CurrentRainData == null)
		{
			this.m_CurrentDataIndex = 0;
			this.m_CurrentRainData = this.m_RainData[this.m_CurrentDataIndex];
			this.m_CurrentRainData.m_ExecutionTime = currentTimeMinutes;
		}
		if (this.m_CurrentRainData.m_RainInterval + this.m_CurrentRainData.m_RainDuration + this.m_CurrentRainData.m_ExecutionTime < currentTimeMinutes)
		{
			this.m_CurrentDataIndex++;
			if (this.m_CurrentDataIndex >= this.m_RainData.Count)
			{
				this.m_CurrentDataIndex = 0;
			}
			this.m_CurrentRainData = this.m_RainData[this.m_CurrentDataIndex];
			this.m_CurrentRainData.m_ExecutionTime = currentTimeMinutes;
		}
		else if (this.m_CurrentRainData.m_RainInterval + this.m_CurrentRainData.m_ExecutionTime < currentTimeMinutes)
		{
			this.m_LastRainTime = currentTimeMinutes;
			this.m_WeatherInterpolated = 1f;
		}
		else if (this.m_LastRainTime + 14f > currentTimeMinutes)
		{
			this.m_WeatherInterpolated = CJTools.Math.GetProportionalClamp(1f, 0f, currentTimeMinutes, this.m_LastRainTime, this.m_LastRainTime + 14f);
		}
		else
		{
			this.m_WeatherInterpolated = CJTools.Math.GetProportionalClamp(0f, 1f, currentTimeMinutes, this.m_CurrentRainData.m_ExecutionTime + this.m_CurrentRainData.m_RainInterval - 14f, this.m_CurrentRainData.m_ExecutionTime + this.m_CurrentRainData.m_RainInterval);
		}
		if (this.m_WeatherInterpolated >= 1f)
		{
			this.m_Wetness += Time.deltaTime * 0.5f;
		}
		else
		{
			this.m_Wetness -= Time.deltaTime * 0.2f;
		}
		this.m_Wetness = Mathf.Clamp01(this.m_Wetness);
		this.SetShaderProperty();
		this.SendRainMessages();
		this.UpdateTerrainWetness();
		if (this.m_LastWeatherInterpolated == 0f && this.m_WeatherInterpolated > 0f && this.m_CurrentRainData.m_RainDuration > 8f)
		{
			this.PlayThunderSound();
		}
		this.m_LastWeatherInterpolated = this.m_WeatherInterpolated;
	}

	private void UpdateRainConeActivity()
	{
		if (Player.Get().IsCameraUnderwater())
		{
			this.m_RainCone.SetActive(false);
			return;
		}
		this.m_RainCone.SetActive(true);
	}

	private void PlayThunderSound()
	{
		this.m_AudioSource.PlayOneShot(this.m_ThunderClips[UnityEngine.Random.Range(0, this.m_ThunderClips.Count)]);
	}

	private void UpdateTerrainWetness()
	{
		if (this.m_HasRainBegun)
		{
			this.m_RTP.globalSettingsHolder.TERRAIN_RainIntensity = this.m_WeatherInterpolated;
			this.m_RTP.globalSettingsHolder.TERRAIN_DropletsSpeed = 60f;
			this.m_RTP.globalSettingsHolder.TERRAIN_RippleScale = 0.7f;
			this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness = this.m_RTP.globalSettingsHolder.TERRAIN_WetnessAttackCurve.Evaluate(Time.time - this.m_RainStartTime);
		}
		else if (this.m_HasRainEnded)
		{
			this.m_RTP.globalSettingsHolder.TERRAIN_RainIntensity = 0f;
			this.m_RTP.globalSettingsHolder.TERRAIN_DropletsSpeed = 9f;
			this.m_RTP.globalSettingsHolder.TERRAIN_RippleScale = 1.6f;
			if (this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness > this.m_RTP.globalSettingsHolder.TERRAIN_WetnessReleaseFastValue)
			{
				this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness = CJTools.Math.GetProportionalClamp(this.m_RainEndGlobalWetness, this.m_RTP.globalSettingsHolder.TERRAIN_WetnessReleaseFastValue, Time.time, this.m_RainEndTime, this.m_RainEndTime + this.m_RTP.globalSettingsHolder.TERRAIN_WetnessReleaseFastTime);
				this.m_WetnessReleaseSlow = false;
			}
			if (this.m_RainEndTime > 1f && !this.m_WetnessReleaseSlow && this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness <= this.m_RTP.globalSettingsHolder.TERRAIN_WetnessReleaseFastValue)
			{
				this.m_WetnessReleaseSlow = true;
				this.m_WetnessReleaseSlowStartTime = Time.time;
			}
			if (this.m_WetnessReleaseSlow)
			{
				this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness = CJTools.Math.GetProportionalClamp(this.m_RTP.globalSettingsHolder.TERRAIN_WetnessReleaseFastValue, 0f, Time.time, this.m_WetnessReleaseSlowStartTime, this.m_WetnessReleaseSlowStartTime + this.m_RTP.globalSettingsHolder.TERRAIN_WetnessReleaseSlowTime);
			}
		}
		Shader.SetGlobalFloat(this.m_ShaderTerrainGlobalWeness, this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness);
		Shader.SetGlobalFloat(this.m_ShaderTerrainRainIntensity, this.m_RTP.globalSettingsHolder.TERRAIN_RainIntensity);
	}

	private void SendRainMessages()
	{
		if (!this.m_HasRainBegun && this.m_WeatherInterpolated >= 0.05f)
		{
			if (this.OnRainBegin != null)
			{
				this.OnRainBegin();
			}
			this.m_HasRainBegun = true;
			this.m_HasRainEnded = false;
			this.m_RainStartTime = Time.time;
		}
		if (!this.m_HasRainEnded && this.m_WeatherInterpolated < 0.05f)
		{
			if (this.OnRainEnd != null)
			{
				this.OnRainEnd();
			}
			this.m_HasRainEnded = true;
			this.m_HasRainBegun = false;
			this.m_RainEndTime = Time.time;
			this.m_RainEndGlobalWetness = this.m_RTP.globalSettingsHolder.TERRAIN_GlobalWetness;
		}
	}

	private void SetShaderProperty()
	{
		this.m_Material.SetFloat(this.m_ShaderPropertyId, this.m_WeatherInterpolated);
		Shader.SetGlobalFloat(this.m_ShaderWetnessGlobal, this.m_Wetness);
	}

	private void FillRainCollectors()
	{
		float num = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			num = Player.GetSleepTimeFactor();
		}
		float fill_amount;
		if (this.m_WeatherInterpolated >= 0.95f)
		{
			fill_amount = this.m_RainCollectorFillPerSecondRain * num;
		}
		else
		{
			fill_amount = this.m_RainCollectorFillPerSecondNoRain * num;
		}
		for (int i = 0; i < RainCollector.s_AllRainCollectors.Count; i++)
		{
			RainCollector.s_AllRainCollectors[i].Pour(fill_amount);
		}
	}

	public bool IsInRainCutter(Vector3 point)
	{
		using (List<RainCutter>.Enumerator enumerator = RainCutter.s_AllRainCutters.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.CheckInside(point))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsInRainCutterTent(Vector3 point)
	{
		foreach (RainCutter rainCutter in RainCutter.s_AllRainCutters)
		{
			if (rainCutter.m_Type == RainCutterType.Tent && rainCutter.CheckInside(point))
			{
				return true;
			}
		}
		return false;
	}

	public void ToggleDebug()
	{
		this.m_DebugEnabled = !this.m_DebugEnabled;
	}

	public void TogglePeriodDebug()
	{
		this.m_DebugPeriodEnabled = !this.m_DebugPeriodEnabled;
	}

	public void ToggleRain()
	{
		this.m_DebugRainEnabled = !this.m_DebugRainEnabled;
	}

	private void Update()
	{
		this.RainManagerUpdate();
		this.RotateCone();
		this.UpdatePlayerSanity();
		this.FillRainCollectors();
		this.UpdateWind();
		this.UpdateAreaDensity();
		this.UpdateCameraInRainCutter();
		this.UpdateRainConeActivity();
	}

	private void UpdateCameraInRainCutter()
	{
		if (this.m_CameraMain == null)
		{
			this.m_CameraMain = Camera.main;
		}
		Camera cameraMain = this.m_CameraMain;
		if (cameraMain == null)
		{
			this.m_Material.SetFloat(this.m_ShaderInRainCutter, 0f);
			return;
		}
		RainCutter rainCutter = null;
		float num = 0f;
		foreach (RainCutter rainCutter2 in RainCutter.s_AllRainCutters)
		{
			float insideValue = rainCutter2.GetInsideValue(cameraMain.transform.position);
			if (num < insideValue)
			{
				num = insideValue;
				rainCutter = rainCutter2;
			}
		}
		if (rainCutter != null)
		{
			this.m_Material.SetMatrix(this.m_ShaderRainCutterMtx, rainCutter.transform.worldToLocalMatrix);
		}
		this.m_Material.SetFloat(this.m_ShaderInRainCutter, num);
	}

	private void UpdatePlayerSanity()
	{
		if (this.IsRain() && !this.IsInRainCutter(Player.Get().transform.position))
		{
			this.m_PlayerWetDuration += Time.deltaTime;
		}
		else
		{
			this.m_PlayerWetDuration = 0f;
		}
		if (this.m_PlayerWetDuration >= PlayerSanityModule.Get().GetEventInterval(PlayerSanityModule.SanityEventType.Rain))
		{
			PlayerSanityModule.Get().OnEvent(PlayerSanityModule.SanityEventType.Rain, 1);
			this.m_PlayerWetDuration = 0f;
		}
	}

	private void RotateCone()
	{
		this.m_ConeRotationAngle += this.m_ConeRotationSpeed * Time.deltaTime;
		Quaternion rhs = Quaternion.Euler(0f, 0f, this.m_ConeRotationAngle);
		this.m_RainCone.transform.rotation = this.m_ConeStartRotation * rhs;
	}

	private void UpdateWind()
	{
		if (Time.time > this.m_WindRandomNextTime)
		{
			this.m_WantedWind = UnityEngine.Random.Range(0f, 1f);
			this.m_WindRandomNextTime = Time.time + UnityEngine.Random.Range(this.m_WindRandomMin, this.m_WindRandomMax);
		}
		float num = 1f;
		float num2 = 0.3f;
		float num3;
		if (this.m_WantedWind > this.m_Wind)
		{
			num3 = num;
		}
		else
		{
			num3 = num2;
		}
		this.m_Wind += (this.m_WantedWind - this.m_Wind) * Time.deltaTime * num3;
	}

	private void UpdateAreaDensity()
	{
		float num = 1f;
		float num2 = 0.3f;
		AreaDensity areaDensity = this.GetAreaDensity();
		if (areaDensity == AreaDensity.High)
		{
			this.m_WantedAreaDensity = 1f;
		}
		else if (areaDensity == AreaDensity.Medium)
		{
			this.m_WantedAreaDensity = 0.5f;
		}
		else
		{
			this.m_WantedAreaDensity = 0f;
		}
		float num3;
		if (this.m_WantedAreaDensity > this.m_AreaDensity)
		{
			num3 = num;
		}
		else
		{
			num3 = num2;
		}
		this.m_AreaDensity += (this.m_WantedAreaDensity - this.m_AreaDensity) * Time.deltaTime * num3;
	}

	private AreaDensity GetAreaDensity()
	{
		Vector2 vector = this.CalculateUV(Player.Get().gameObject.transform.position);
		int num = (int)(vector.x * (float)this.m_TextureWidth);
		int num2 = (int)(vector.y * (float)this.m_TextureHeight);
		if (num < 0 || num2 < 0 || num >= this.m_TextureWidth || num2 >= this.m_TextureHeight)
		{
			return AreaDensity.Low;
		}
		return this.m_AreaDenityData[num, num2];
	}

	private Vector2 CalculateUV(Vector3 pos)
	{
		Vector2 result;
		result.x = (pos.x - Terrain.activeTerrain.GetPosition().x) / Terrain.activeTerrain.terrainData.size.x;
		result.y = (pos.z - Terrain.activeTerrain.GetPosition().z) / Terrain.activeTerrain.terrainData.size.z;
		return result;
	}

	public void ScenarioStartRain()
	{
		this.m_CurrentRainData = this.m_ScenarioRain;
		this.m_CurrentRainData.m_ExecutionTime = MainLevel.Instance.GetCurrentTimeMinutes();
	}

	public void ScenarioStopRain()
	{
		this.m_CurrentRainData = this.m_RainData[this.m_CurrentDataIndex];
		this.m_CurrentRainData.m_ExecutionTime = MainLevel.Instance.GetCurrentTimeMinutes();
	}

	public void ScenarioBlockRain()
	{
		this.m_CurrentRainData = this.m_ScenarioNoRain;
		this.m_CurrentRainData.m_ExecutionTime = MainLevel.Instance.GetCurrentTimeMinutes();
		this.m_LastRainTime = float.MinValue;
	}

	public void ScenarioUnblockRain()
	{
		this.ScenarioStopRain();
	}

	private void LoadDensityData()
	{
		int num = RainManager.m_DenityTexturePath.IndexOf("/", 2);
		int num2 = RainManager.m_DenityTexturePath.IndexOf(".");
		Stream stream = new MemoryStream((Resources.Load(RainManager.m_DenityTexturePath.Substring(num + 1, num2 - num - 1)) as TextAsset).bytes);
		if ((byte)stream.ReadByte() != 68)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		if ((byte)stream.ReadByte() != 65)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		if ((byte)stream.ReadByte() != 80)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		int num3 = BitConverter.ToInt32(array, 0);
		stream.Read(array, 0, 4);
		int num4 = BitConverter.ToInt32(array, 0);
		this.m_AreaDenityData = new AreaDensity[num3, num4];
		this.m_TextureWidth = num3;
		this.m_TextureHeight = num4;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				float num5 = (float)((byte)stream.ReadByte());
				num5 /= 255f;
				float num6 = (float)((byte)stream.ReadByte());
				num6 /= 255f;
				float num7 = (float)((byte)stream.ReadByte());
				num7 /= 255f;
				float num8 = (float)((byte)stream.ReadByte());
				num8 /= 255f;
				Color color;
				color.r = num5;
				color.g = num6;
				color.b = num7;
				color.a = num8;
				AreaDensity areaDensity;
				if (color.r > 0.95f)
				{
					areaDensity = AreaDensity.High;
				}
				else if (color.g > 0.95f)
				{
					areaDensity = AreaDensity.Medium;
				}
				else if (color.b > 0.95f)
				{
					areaDensity = AreaDensity.Low;
				}
				else
				{
					areaDensity = AreaDensity.None;
				}
				this.m_AreaDenityData[i, j] = areaDensity;
			}
		}
		stream.Close();
	}

	public bool IsRain()
	{
		return this.m_WeatherInterpolated > this.m_RainTreshold;
	}

	public void Save()
	{
		SaveGame.SaveVal("WeatherInterpolated", this.m_WeatherInterpolated);
		SaveGame.SaveVal("CurrentWeatherDataIndex", this.m_CurrentDataIndex);
		SaveGame.SaveVal("WeatherExecutionTime", this.m_CurrentRainData.m_ExecutionTime);
		SaveGame.SaveVal("CurrentPeriod", this.m_CurrentPeriod);
		SaveGame.SaveVal("PeriodStartTime", this.m_PeriodStartTime);
	}

	public void Load()
	{
		this.m_WeatherInterpolated = SaveGame.LoadFVal("WeatherInterpolated");
		this.m_CurrentDataIndex = SaveGame.LoadIVal("CurrentWeatherDataIndex");
		this.m_CurrentRainData = this.m_RainData[this.m_CurrentDataIndex];
		this.m_CurrentRainData.m_ExecutionTime = SaveGame.LoadFVal("WeatherExecutionTime");
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate5)
		{
			this.m_CurrentPeriod = SaveGame.LoadIVal("CurrentPeriod");
			this.m_PeriodStartTime = SaveGame.LoadFVal("PeriodStartTime");
		}
	}

	public override void OnReplicationPrepare()
	{
		this.ReplSetDirty();
	}

	public override void OnReplicationSerialize(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_WeatherInterpolated);
		writer.Write(this.m_CurrentDataIndex);
		writer.Write((this.m_CurrentRainData != null) ? this.m_CurrentRainData.m_ExecutionTime : 0f);
		writer.Write(this.m_CurrentPeriod);
		writer.Write(this.m_PeriodStartTime);
		writer.Write(this.m_LastRainTime);
	}

	public override void OnReplicationDeserialize(P2PNetworkReader reader, bool initial_state)
	{
		this.m_WeatherInterpolated = reader.ReadFloat();
		this.m_CurrentDataIndex = reader.ReadInt32();
		if (this.m_RainData.Count > this.m_CurrentDataIndex)
		{
			this.m_CurrentRainData = this.m_RainData[this.m_CurrentDataIndex];
		}
		if (this.m_CurrentRainData != null)
		{
			this.m_CurrentRainData.m_ExecutionTime = reader.ReadFloat();
		}
		else
		{
			reader.ReadFloat();
		}
		this.m_CurrentPeriod = reader.ReadInt32();
		this.m_PeriodStartTime = reader.ReadFloat();
		this.m_LastRainTime = reader.ReadFloat();
	}

	public GameObject m_RainCone;

	private List<RainData> m_RainData = new List<RainData>();

	private RainData m_CurrentRainData;

	private int m_CurrentDataIndex;

	public bool m_DebugEnabled;

	public bool m_DebugRainEnabled;

	public bool m_DebugPeriodEnabled;

	private float m_RainCollectorFillPerSecondRain = 5f;

	private float m_RainCollectorFillPerSecondNoRain = 0.5f;

	private float m_PlayerWetDuration;

	private static RainManager s_Instance = null;

	public float m_WeatherInterpolated;

	private float m_LastRainTime = float.MinValue;

	private int m_ShaderPropertyId = -1;

	private Material m_Material;

	public float m_ConeRotationSpeed = 0.7f;

	private float m_ConeRotationAngle;

	private Quaternion m_ConeStartRotation = Quaternion.identity;

	private ReliefTerrain m_RTP;

	private int m_ShaderWetnessGlobal = Shader.PropertyToID("_Wetness");

	private int m_ShaderTerrainGlobalWeness = Shader.PropertyToID("TERRAIN_GlobalWetness");

	private int m_ShaderTerrainRainIntensity = Shader.PropertyToID("TERRAIN_RainIntensity");

	private int m_ShaderInRainCutter = Shader.PropertyToID("_InRainCutter");

	private int m_ShaderRainCutterMtx = Shader.PropertyToID("_RainCutterMtx");

	private bool m_HasRainBegun;

	private bool m_HasRainEnded;

	private RainData m_ScenarioRain;

	private RainData m_ScenarioNoRain;

	private const float BLEND_TIME = 14f;

	private static string m_DenityTexturePath = "/Resources/Systems/DensityArea/DensityArea.bytes";

	private AreaDensity[,] m_AreaDenityData;

	private int m_TextureWidth;

	private int m_TextureHeight;

	private float m_RainTreshold = 0.5f;

	private AudioSource m_AudioSource;

	private List<RainPeriod> m_Periods = new List<RainPeriod>();

	private float m_PeriodStartTime = float.MinValue;

	private int m_CurrentPeriod = -1;

	private List<AudioClip> m_ThunderClips = new List<AudioClip>();

	private float m_LastWeatherInterpolated;

	private float m_RainStartTime = -1f;

	private float m_RainEndTime = -1f;

	private float m_RainEndGlobalWetness;

	private bool m_WetnessReleaseSlow;

	private float m_WetnessReleaseSlowStartTime = -1f;

	private float m_Wetness;

	private List<BoxCollider> m_BoxColliderTemp = new List<BoxCollider>();

	private Camera m_CameraMain;

	private float m_WantedAreaDensity;

	public float m_AreaDensity;

	public float m_Wind;

	public float m_WantedWind;

	private float m_WindRandomMin = 35f;

	private float m_WindRandomMax = 85f;

	private float m_WindRandomNextTime;
}
