using System;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableFallingObject : DestroyableObject
{
	private void OnEnable()
	{
		this.m_StartTime = Time.time;
	}

	public void AddAngularVelocityOnStart(Vector3 ang_vel, float duration)
	{
		this.m_AddAngularVelOnStart = true;
		this.m_AngularVelocityOnStart = ang_vel;
		this.m_AddAngularVelocityOnStartDuration = duration;
	}

	private void FixedUpdate()
	{
		if (this.m_RigidBody == null)
		{
			base.GetComponents<Rigidbody>(this.m_TempList);
			if (this.m_TempList.Count > 0)
			{
				this.m_RigidBody = this.m_TempList[0];
			}
		}
		if (this.m_RigidBody != null && this.m_AddAngularVelOnStart && Time.time - this.m_StartTime < this.m_AddAngularVelocityOnStartDuration)
		{
			this.m_RigidBody.isKinematic = false;
			this.m_RigidBody.AddTorque(this.m_AngularVelocityOnStart * 1f, ForceMode.Acceleration);
			DebugRender.DrawLine(base.transform.position, base.transform.position + this.m_AngularVelocityOnStart, default(Color), 0f);
		}
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
			base.DestroyMe(damage_info, "");
			return;
		}
		if (Time.time - this.m_StartTime > this.m_MaxTimeToDestroy)
		{
			this.PlayDestroySound();
			DamageInfo damage_info2 = new DamageInfo();
			base.DestroyMe(damage_info2, "");
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.GetType() == typeof(TerrainCollider))
		{
			this.PlayDestroySound();
			DamageInfo damage_info = new DamageInfo();
			base.DestroyMe(damage_info, "");
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

	[HideInInspector]
	public bool m_AddAngularVelOnStart;

	[HideInInspector]
	public Vector3 m_AngularVelocityOnStart = Vector3.zero;

	[HideInInspector]
	public float m_AddAngularVelocityOnStartDuration;

	private Rigidbody m_RigidBody;

	private List<Rigidbody> m_TempList = new List<Rigidbody>();
}
