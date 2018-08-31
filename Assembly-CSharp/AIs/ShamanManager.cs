using System;
using CJTools;
using UnityEngine;
using UnityEngine.AI;

namespace AIs
{
	public class ShamanManager : MonoBehaviour
	{
		public static ShamanManager Get()
		{
			return ShamanManager.s_Instance;
		}

		private void Awake()
		{
			ShamanManager.s_Instance = this;
			base.enabled = false;
		}

		private void Start()
		{
			this.m_Sky = MainLevel.Instance.m_TODSky;
			this.m_Rays = Camera.main.GetComponent<TOD_Rays>();
			this.m_AmplifyColor = Camera.main.GetComponent<AmplifyColorEffect>();
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSource.loop = true;
			this.m_AudioSource.clip = this.m_Music;
		}

		private bool SpawnShaman()
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(15f, 8f, (float)this.m_ShamansCount, 0f, (float)this.m_SpawnShamansCount);
			float num = this.m_LastAngle + UnityEngine.Random.Range(0f, 270f);
			if (num >= 360f)
			{
				num -= 360f;
			}
			Vector3 vector = Player.Get().transform.position + Quaternion.AngleAxis(UnityEngine.Random.Range(45f, 315f), Vector3.up) * Player.Get().transform.forward * proportionalClamp;
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(vector, out navMeshHit, 2f, -1))
			{
				this.m_Shaman = UnityEngine.Object.Instantiate<GameObject>(this.m_ShamanPrefab, vector, Quaternion.identity);
				this.m_ShamansCount++;
				this.m_LastAngle = num;
				this.m_LastSpawnShamanTime = Time.time;
				return true;
			}
			return false;
		}

		private void SetState(ShamanManager.State state)
		{
			if (this.m_State == state)
			{
				DebugUtils.Assert(DebugUtils.AssertType.Info);
				return;
			}
			this.m_State = state;
			this.OnEnterState();
		}

		private void OnEnterState()
		{
			this.m_ShamansCount = 0;
			this.m_EnterStateTime = Time.time;
			if (this.m_State == ShamanManager.State.None)
			{
				this.m_AudioSource.Stop();
				this.m_Sky.Day.LightIntensity = this.m_DefaultDayLightIntensity;
				this.m_Sky.Night.LightIntensity = this.m_DefaultNightLightIntensity;
				this.m_Sky.Atmosphere.Brightness = this.m_DefaultAtmosphereBrightness;
				this.m_Sky.Clouds.Brightness = this.m_DefaultCloudsBrightness;
				this.m_Sky.Clouds.Attenuation = this.m_DefaultCloudsAttenuation;
				this.m_Sky.Clouds.Coverage = this.m_DefaultCloudsCoverage;
				this.m_Rays.Intensity = this.m_DefaultRaysIntensity;
				this.m_AmplifyColor.BlendAmount = this.m_DefaulBlendAmount;
			}
			else if (this.m_State == ShamanManager.State.Attack)
			{
				this.m_DefaultDayLightIntensity = this.m_Sky.Day.LightIntensity;
				this.m_DefaultNightLightIntensity = this.m_Sky.Night.LightIntensity;
				this.m_DefaultAtmosphereBrightness = this.m_Sky.Atmosphere.Brightness;
				this.m_DefaultCloudsBrightness = this.m_Sky.Clouds.Brightness;
				this.m_DefaultCloudsAttenuation = this.m_Sky.Clouds.Attenuation;
				this.m_DefaultCloudsCoverage = this.m_Sky.Clouds.Coverage;
				this.m_DefaultRaysIntensity = this.m_Rays.Intensity;
				this.m_DefaulBlendAmount = this.m_AmplifyColor.BlendAmount;
				this.m_AudioSource.volume = 0f;
				this.m_AudioSource.Play();
				this.m_SpawnShamanInterval = this.m_EnterAttackDuration;
				this.m_LastKillShamanTime = Time.time;
				this.m_LastSpawnShamanTime = Time.time;
				this.m_LastAngle = UnityEngine.Random.Range(0f, 359f);
			}
		}

		public void OnKillShaman()
		{
			this.m_LastKillShamanTime = Time.time;
			if (this.m_ShamansCount < this.m_SpawnShamansCount)
			{
				if (this.m_SpawnVersion == ShamanManager.SpawnVersion.Kill)
				{
					this.m_SpawnShamanInterval = UnityEngine.Random.Range(this.m_SpawnShamanMinIntervalVKill, this.m_SpawnShamanMaxIntervalVKill);
				}
			}
			else
			{
				this.SetState(ShamanManager.State.Finish);
			}
		}

		private void Update()
		{
			if (!GreenHellGame.ROADSHOW_DEMO && GreenHellGame.DEBUG && Input.GetKeyDown(KeyCode.Z))
			{
				this.SetState(ShamanManager.State.Attack);
			}
			this.UpdateAttack();
			this.UpdateFinish();
			this.UpdateEffects();
		}

		private void UpdateAttack()
		{
			if (this.m_State != ShamanManager.State.Attack)
			{
				return;
			}
			if (this.m_SpawnVersion == ShamanManager.SpawnVersion.Kill)
			{
				if (!this.m_Shaman && Time.time - this.m_LastKillShamanTime >= this.m_SpawnShamanInterval)
				{
					this.SpawnShaman();
				}
			}
			else if (this.m_SpawnVersion == ShamanManager.SpawnVersion.Time && Time.time - this.m_LastSpawnShamanTime >= this.m_SpawnShamanInterval && this.SpawnShaman())
			{
				this.m_SpawnShamanInterval = UnityEngine.Random.Range(this.m_SpawnShamanMinIntervalVTime, this.m_SpawnShamanMaxIntervalVTime);
			}
		}

		private void UpdateFinish()
		{
			if (this.m_State != ShamanManager.State.Finish)
			{
				return;
			}
			if (Time.time - this.m_EnterStateTime > this.m_ExitAttackDuration)
			{
				this.SetState(ShamanManager.State.None);
			}
		}

		private void UpdateEffects()
		{
			if (this.m_State == ShamanManager.State.None)
			{
				return;
			}
			if (this.m_State == ShamanManager.State.Attack && Time.time - this.m_EnterStateTime <= this.m_EnterAttackDuration)
			{
				float b = Time.time - this.m_EnterStateTime;
				this.m_Sky.Day.LightIntensity = CJTools.Math.GetProportionalClamp(this.m_DefaultDayLightIntensity, 0.8f, b, 0f, this.m_EnterAttackDuration);
				this.m_Sky.Night.LightIntensity = CJTools.Math.GetProportionalClamp(this.m_DefaultNightLightIntensity, 0.8f, b, 0f, this.m_EnterAttackDuration);
				this.m_Rays.Intensity = this.m_DefaultRaysIntensity * CJTools.Math.GetProportionalClamp(1f, 0f, b, 0f, this.m_EnterAttackDuration);
				this.m_AmplifyColor.BlendAmount = CJTools.Math.GetProportionalClamp(0f, 1f, b, 0f, this.m_EnterAttackDuration);
				this.m_Sky.Atmosphere.Brightness = CJTools.Math.GetProportionalClamp(this.m_DefaultAtmosphereBrightness, 0.7f, b, 0f, this.m_EnterAttackDuration);
				this.m_Sky.Clouds.Brightness = CJTools.Math.GetProportionalClamp(this.m_DefaultCloudsBrightness, 0.5f, b, 0f, this.m_EnterAttackDuration);
				this.m_Sky.Clouds.Attenuation = CJTools.Math.GetProportionalClamp(this.m_DefaultCloudsAttenuation, 0.7f, b, 0f, this.m_EnterAttackDuration);
				this.m_Sky.Clouds.Coverage = CJTools.Math.GetProportionalClamp(this.m_DefaultCloudsCoverage, 0.8f, b, 0f, this.m_EnterAttackDuration);
				this.m_AudioSource.volume = CJTools.Math.GetProportionalClamp(0f, 1f, b, 0f, this.m_EnterAttackDuration);
			}
			else if (this.m_State == ShamanManager.State.Finish && Time.time - this.m_EnterStateTime <= this.m_ExitAttackDuration)
			{
				float b2 = Time.time - this.m_EnterStateTime;
				this.m_Sky.Day.LightIntensity = CJTools.Math.GetProportionalClamp(0.8f, this.m_DefaultDayLightIntensity, b2, 0f, this.m_ExitAttackDuration);
				this.m_Sky.Night.LightIntensity = CJTools.Math.GetProportionalClamp(0.8f, this.m_DefaultNightLightIntensity, b2, 0f, this.m_ExitAttackDuration);
				this.m_Rays.Intensity = this.m_DefaultRaysIntensity * CJTools.Math.GetProportionalClamp(0f, 1f, b2, 0f, this.m_ExitAttackDuration);
				this.m_AmplifyColor.BlendAmount = CJTools.Math.GetProportionalClamp(1f, 0f, b2, 0f, this.m_ExitAttackDuration);
				this.m_Sky.Atmosphere.Brightness = CJTools.Math.GetProportionalClamp(0.7f, this.m_DefaultAtmosphereBrightness, b2, 0f, this.m_ExitAttackDuration);
				this.m_Sky.Clouds.Brightness = CJTools.Math.GetProportionalClamp(0.5f, this.m_DefaultCloudsBrightness, b2, 0f, this.m_ExitAttackDuration);
				this.m_Sky.Clouds.Attenuation = CJTools.Math.GetProportionalClamp(0.7f, this.m_DefaultCloudsAttenuation, b2, 0f, this.m_ExitAttackDuration);
				this.m_Sky.Clouds.Coverage = CJTools.Math.GetProportionalClamp(0.8f, this.m_DefaultCloudsCoverage, b2, 0f, this.m_ExitAttackDuration);
				this.m_AudioSource.volume = CJTools.Math.GetProportionalClamp(1f, 0f, b2, 0f, this.m_ExitAttackDuration);
			}
		}

		public GameObject m_ShamanPrefab;

		private ShamanManager.State m_State;

		private float m_EnterStateTime;

		private TOD_Sky m_Sky;

		private AmplifyColorEffect m_AmplifyColor;

		private TOD_Rays m_Rays;

		private GameObject m_Shaman;

		public int m_SpawnShamansCount;

		private int m_ShamansCount;

		private AudioSource m_AudioSource;

		private float m_DefaultDayLightIntensity;

		private float m_DefaultNightLightIntensity;

		private float m_DefaultAtmosphereBrightness;

		private float m_DefaultCloudsBrightness;

		private float m_DefaultCloudsAttenuation;

		private float m_DefaultCloudsCoverage;

		private float m_DefaultRaysIntensity;

		private float m_DefaulBlendAmount;

		private float m_LastSpawnShamanTime;

		private float m_LastKillShamanTime;

		private float m_SpawnShamanInterval;

		public float m_SpawnShamanMinIntervalVKill = 1f;

		public float m_SpawnShamanMaxIntervalVKill = 3f;

		public float m_SpawnShamanMinIntervalVTime = 5f;

		public float m_SpawnShamanMaxIntervalVTime = 8f;

		public float m_EnterAttackDuration = 3f;

		public float m_ExitAttackDuration = 3f;

		private float m_LastAngle;

		public ShamanManager.SpawnVersion m_SpawnVersion = ShamanManager.SpawnVersion.Time;

		public ShamanManager.AttackVersion m_AttackVersion;

		public AudioClip m_Music;

		private static ShamanManager s_Instance;

		private enum State
		{
			None,
			Attack,
			Finish
		}

		public enum SpawnVersion
		{
			Kill,
			Time
		}

		public enum AttackVersion
		{
			Slow,
			Fast
		}
	}
}
