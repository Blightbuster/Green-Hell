using System;
using UnityEngine;

public class UIAudioPlayer : MonoBehaviour
{
	public static UIAudioPlayer Instance
	{
		get
		{
			if (UIAudioPlayer.s_Singleton == null)
			{
				GameObject gameObject = Resources.Load("Prefabs/Systems/UIAudioPlayer") as GameObject;
				if (gameObject)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
					gameObject2.name = "UIAudioPlayer";
					UIAudioPlayer.s_Singleton = gameObject2.GetComponent<UIAudioPlayer>();
				}
			}
			return UIAudioPlayer.s_Singleton;
		}
	}

	private void Awake()
	{
		UIAudioPlayer.s_Singleton = this;
		this.m_Sources = base.GetComponents<AudioSource>();
		foreach (AudioSource audioSource in this.m_Sources)
		{
			audioSource.ignoreListenerPause = true;
			audioSource.ignoreListenerVolume = true;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public static void Play(AudioClip clip)
	{
		UIAudioPlayer instance = UIAudioPlayer.Instance;
		if (instance != null && instance.CanPlaySound())
		{
			AudioSource source = UIAudioPlayer.Instance.GetSource();
			if (source == null)
			{
				return;
			}
			source.PlayOneShot(clip);
		}
	}

	public static void Play(UIAudioPlayer.UISoundType type)
	{
		if (type >= UIAudioPlayer.UISoundType._Count)
		{
			return;
		}
		UIAudioPlayer instance = UIAudioPlayer.Instance;
		if (instance != null && instance.CanPlaySound())
		{
			AudioClip audioClip = UIAudioPlayer.Instance.m_StandardSounds[(int)type];
			if (audioClip != null)
			{
				AudioSource source = UIAudioPlayer.Instance.GetSource();
				if (source == null)
				{
					return;
				}
				source.PlayOneShot(audioClip);
			}
		}
	}

	private AudioSource GetSource()
	{
		foreach (AudioSource audioSource in this.m_Sources)
		{
			if (!audioSource.isPlaying)
			{
				return audioSource;
			}
		}
		Debug.LogWarning("No free audio source for UI!");
		return null;
	}

	private bool CanPlaySound()
	{
		return !MainMenuManager.Get() || !MainMenuManager.Get().isActiveAndEnabled || (!FadeSystem.Get().m_FadingIn && !FadeSystem.Get().m_FadingOut);
	}

	private AudioSource[] m_Sources;

	private static UIAudioPlayer s_Singleton;

	public AudioClip[] m_StandardSounds = new AudioClip[2];

	public enum UISoundType
	{
		Focus,
		Click,
		_Count
	}
}
