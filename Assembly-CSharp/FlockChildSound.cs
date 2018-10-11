using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlockChildSound : MonoBehaviour
{
	public void Start()
	{
		this._flockChild = base.GetComponent<FlockChild>();
		this._audio = base.GetComponent<AudioSource>();
		this._audio.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
		base.InvokeRepeating("PlayRandomSound", UnityEngine.Random.value + 1f, 1f);
		if (this._scareSounds.Length > 0)
		{
			base.InvokeRepeating("ScareSound", 1f, 0.01f);
		}
	}

	public void PlayRandomSound()
	{
		if (MainLevel.Instance.IsNight() && this._flockChild.m_TimeOfDay == FlockChild.TimeOfDay.Day)
		{
			return;
		}
		if (!MainLevel.Instance.IsNight() && this._flockChild.m_TimeOfDay == FlockChild.TimeOfDay.Night)
		{
			return;
		}
		if (LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (base.gameObject.activeInHierarchy)
		{
			if (!this._audio.isPlaying && this._flightSounds.Length > 0 && this._flightSoundRandomChance > UnityEngine.Random.value && !this._flockChild._landing)
			{
				this._audio.clip = this._flightSounds[UnityEngine.Random.Range(0, this._flightSounds.Length)];
				this._audio.pitch = UnityEngine.Random.Range(this._pitchMin, this._pitchMax);
				this._audio.volume = UnityEngine.Random.Range(this._volumeMin, this._volumeMax);
				this._audio.Play();
			}
			else if (!this._audio.isPlaying && this._idleSounds.Length > 0 && this._idleSoundRandomChance > UnityEngine.Random.value && this._flockChild._landing)
			{
				this._audio.clip = this._idleSounds[UnityEngine.Random.Range(0, this._idleSounds.Length)];
				this._audio.pitch = UnityEngine.Random.Range(this._pitchMin, this._pitchMax);
				this._audio.volume = UnityEngine.Random.Range(this._volumeMin, this._volumeMax);
				this._audio.Play();
				this._hasLanded = true;
			}
		}
	}

	public void ScareSound()
	{
		if (MainLevel.Instance.IsNight() && this._flockChild.m_TimeOfDay == FlockChild.TimeOfDay.Day)
		{
			return;
		}
		if (!MainLevel.Instance.IsNight() && this._flockChild.m_TimeOfDay == FlockChild.TimeOfDay.Night)
		{
			return;
		}
		if (LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (base.gameObject.activeInHierarchy && this._hasLanded && !this._flockChild._landing && this._idleSoundRandomChance * 2f > UnityEngine.Random.value)
		{
			this._audio.clip = this._scareSounds[UnityEngine.Random.Range(0, this._scareSounds.Length)];
			this._audio.volume = UnityEngine.Random.Range(this._volumeMin, this._volumeMax);
			this._audio.PlayDelayed(UnityEngine.Random.value * 0.2f);
			this._hasLanded = false;
		}
	}

	public AudioClip[] _idleSounds;

	public float _idleSoundRandomChance = 0.05f;

	public AudioClip[] _flightSounds;

	public float _flightSoundRandomChance = 0.05f;

	public AudioClip[] _scareSounds;

	public float _pitchMin = 0.85f;

	public float _pitchMax = 1f;

	public float _volumeMin = 0.6f;

	public float _volumeMax = 0.8f;

	private FlockChild _flockChild;

	private AudioSource _audio;

	private bool _hasLanded;
}
