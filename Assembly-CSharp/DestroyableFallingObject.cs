using System;
using UnityEngine;

public class DestroyableFallingObject : DestroyableObject
{
	private void OnEnable()
	{
		this.m_StartTime = Time.time;
	}

	private void Start()
	{
		this.PlayFallSound();
	}

	private void Update()
	{
		if (this.m_CheckAngle && base.gameObject.transform.up.y < 0.1f)
		{
			this.PlayDestroySound();
			DamageInfo damage_info = new DamageInfo();
			base.DestroyMe(damage_info, string.Empty);
			return;
		}
		if (Time.time - this.m_StartTime > this.m_MaxTimeToDestroy)
		{
			this.PlayDestroySound();
			DamageInfo damage_info2 = new DamageInfo();
			base.DestroyMe(damage_info2, string.Empty);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.GetType() == typeof(TerrainCollider))
		{
			this.PlayDestroySound();
			DamageInfo damage_info = new DamageInfo();
			base.DestroyMe(damage_info, string.Empty);
		}
	}

	private void PlayFallSound()
	{
		AudioClip destroyableFallSound = ItemsManager.Get().GetDestroyableFallSound(this.m_Item.m_Info.m_DestroyableFallSoundHash);
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		if (destroyableFallSound)
		{
			this.m_AudioSource.spatialBlend = 1f;
			this.m_AudioSource.PlayOneShot(destroyableFallSound);
		}
	}

	private void PlayDestroySound()
	{
		AudioClip destroyableDestroySound = ItemsManager.Get().GetDestroyableDestroySound(this.m_Item.m_Info.m_DestroyableDestroySoundHash);
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Enviro);
		}
		if (destroyableDestroySound)
		{
			this.m_AudioSource.spatialBlend = 1f;
			this.m_AudioSource.PlayOneShot(destroyableDestroySound);
		}
	}

	public bool m_CheckAngle = true;

	public float m_MaxTimeToDestroy = 3f;

	private float m_StartTime = float.MaxValue;

	private AudioSource m_AudioSource;
}
