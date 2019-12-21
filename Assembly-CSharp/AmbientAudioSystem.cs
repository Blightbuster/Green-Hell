using System;
using System.Collections.Generic;
using UnityEngine;

public class AmbientAudioSystem : MonoBehaviour
{
	public static AmbientAudioSystem Instance { get; private set; }

	private void Awake()
	{
		DebugUtils.Assert(AmbientAudioSystem.Instance == null, true);
		AmbientAudioSystem.Instance = this;
		this.Initialize();
	}

	private void Update()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		this.UpdateAnimalsSound();
	}

	private void OnDestroy()
	{
		if (this.m_AnimalSoundsAudioSource)
		{
			UnityEngine.Object.Destroy(this.m_AnimalSoundsAudioSource.gameObject);
		}
		AmbientAudioSystem.Instance = null;
	}

	private void Initialize()
	{
		this.m_AmbientSounds = (Resources.Load("Scripts/Audio/AmbientSounds") as AmbientSounds);
		if (this.m_AnimalSoundsAudioSource == null)
		{
			this.m_AnimalSoundsAudioSource = new GameObject("AnimalSoundsAudioSource").AddComponent<AudioSource>();
			this.m_AnimalSoundsAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
			this.m_AnimalSoundsAudioSource.rolloffMode = AudioRolloffMode.Linear;
		}
	}

	private void UpdateAnimalsSound()
	{
		if (this.m_AnimalSoundsAudioSource == null)
		{
			return;
		}
		if (this.m_AmbientMS != null && !this.m_AmbientMS.m_Playing)
		{
			if (this.m_AnimalSoundsAudioSource != null && this.m_AnimalSoundsAudioSource.isPlaying)
			{
				this.m_AnimalSoundsAudioSource.Stop();
			}
			return;
		}
		if (SwimController.Get().IsActive() && SwimController.Get().GetState() == SwimState.Swim)
		{
			this.m_AnimalSoundsAudioSource.volume = 0f;
		}
		else
		{
			this.m_AnimalSoundsAudioSource.volume = 1f;
		}
		if (Time.time < this.m_AnimalSoundNextTime)
		{
			return;
		}
		Vector3 position;
		AmbientSounds.AmbientDefinition ambientDefinition = this.SelectAmbientDefinition(out position);
		if (ambientDefinition != null)
		{
			this.m_AnimalSoundsAudioSource.spatialize = ambientDefinition.m_Spatialize;
			this.m_AnimalSoundsAudioSource.spatialBlend = ambientDefinition.m_SpatialBlend;
			this.m_AnimalSoundsAudioSource.minDistance = ambientDefinition.m_DistanceMin;
			this.m_AnimalSoundsAudioSource.maxDistance = ambientDefinition.m_DistanceMax + this.m_AmbientSounds.m_AudibilityExtraDistance;
			this.m_AnimalSoundsAudioSource.transform.position = position;
			this.m_AnimalSoundsAudioSource.clip = ambientDefinition.m_Clip;
			this.m_AnimalSoundsAudioSource.loop = false;
			this.m_AnimalSoundsAudioSource.volume = 1f;
			this.m_AnimalSoundsAudioSource.Play();
			this.m_AnimalSoundNextTime = Time.time + ambientDefinition.m_Clip.length + UnityEngine.Random.Range(this.m_AmbientSounds.m_AmbientIntervalMin, this.m_AmbientSounds.m_AmbientIntervalMax);
			this.m_LastAmbient = ambientDefinition;
			return;
		}
		this.m_AnimalSoundNextTime = Time.time + UnityEngine.Random.Range(5f, 10f);
	}

	public void StartRainForestAmbienceMultisample()
	{
		if (!this.m_RainforestMSStarted)
		{
			this.m_AmbientMS = MSManager.Get().PlayMultiSample(this, "Rainforest_Ambience", 1f);
			this.m_AmbientMS.m_RainForestAmbient = true;
		}
		this.m_RainforestMSStarted = true;
	}

	public void MSAmbientStart(float fade_in)
	{
		if (this.m_AmbientMS != null)
		{
			this.m_AmbientMS.Play(this, fade_in);
		}
	}

	public void MSAmbientStop(float fade_out)
	{
		if (this.m_AmbientMS != null)
		{
			this.m_AmbientMS.Stop(this, fade_out);
		}
	}

	private AmbientSounds.AmbientDefinition SelectAmbientDefinition(out Vector3 position)
	{
		List<AmbientSounds.AmbientDefinition> list = MainLevel.Instance.IsNight() ? this.m_AmbientSounds.m_AmbientDefinitionsNight : this.m_AmbientSounds.m_AmbientDefinitionsDay;
		int num = UnityEngine.Random.Range(0, list.Count);
		for (int i = num; i < num + list.Count; i++)
		{
			int index = i % list.Count;
			AmbientSounds.AmbientDefinition ambientDefinition = list[index];
			if (ambientDefinition.m_Enabled && ambientDefinition.m_Clip != null && this.m_LastAmbient != ambientDefinition)
			{
				Vector3? position2 = this.GetPosition(ambientDefinition);
				if (position2 != null || !ambientDefinition.m_Spatialize)
				{
					position = (position2 ?? Vector3.zero);
					return ambientDefinition;
				}
			}
		}
		position = Vector3.zero;
		return null;
	}

	private Vector3? GetPosition(AmbientSounds.AmbientDefinition def)
	{
		if (def.m_DistanceMin > 0f)
		{
			switch (def.m_Position)
			{
			case AmbientSounds.EAmbientPositionType.Ground:
			{
				Vector3 randomPos2D = this.GetRandomPos2D(def);
				randomPos2D.y = MainLevel.GetTerrainY(randomPos2D);
				break;
			}
			case AmbientSounds.EAmbientPositionType.Bush:
				return this.GetTagBasedPos(def);
			case AmbientSounds.EAmbientPositionType.HighInAir:
				this.GetRandomPos2D(def).y += UnityEngine.Random.Range(10f, 30f);
				break;
			case AmbientSounds.EAmbientPositionType.TreeTops:
			{
				Vector3? tagBasedPos = this.GetTagBasedPos(def);
				if (tagBasedPos != null)
				{
					Vector3 value = tagBasedPos.Value;
					value.y += this.m_AmbientSounds.m_BigTreeAdditionalY;
					return new Vector3?(value);
				}
				break;
			}
			}
		}
		return null;
	}

	private Vector3 GetRandomPos2D(AmbientSounds.AmbientDefinition def)
	{
		Vector3 vector = UnityEngine.Random.insideUnitCircle.To3D();
		return Player.Get().GetWorldPosition() + vector.normalized * UnityEngine.Random.Range(def.m_DistanceMin, def.m_DistanceMax);
	}

	private Vector3? GetTagBasedPos(AmbientSounds.AmbientDefinition def)
	{
		List<string> list = null;
		if (def.m_Position == AmbientSounds.EAmbientPositionType.Bush)
		{
			list = this.m_AmbientSounds.m_BushTags;
		}
		else if (def.m_Position == AmbientSounds.EAmbientPositionType.TreeTops)
		{
			list = this.m_AmbientSounds.m_BigTreeTags;
		}
		if (list != null)
		{
			UnityEngine.Random.Range(0, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				int index = i % list.Count;
				GameObject[] array = GameObject.FindGameObjectsWithTag(list[index]);
				int num = UnityEngine.Random.Range(0, array.Length);
				for (int j = num; j < num + array.Length; j++)
				{
					GameObject gameObject = array[j % array.Length];
					float num2 = gameObject.transform.position.Distance(Player.Get().GetWorldPosition());
					if (num2 >= def.m_DistanceMin && num2 <= def.m_DistanceMax)
					{
						return new Vector3?(gameObject.transform.position);
					}
				}
			}
		}
		return null;
	}

	private AmbientSounds m_AmbientSounds;

	private AmbientSounds.AmbientDefinition m_LastAmbient;

	private float m_AnimalSoundNextTime;

	private AudioSource m_AnimalSoundsAudioSource;

	private MSMultiSample m_AmbientMS;

	private bool m_RainforestMSStarted;
}
