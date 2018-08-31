using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

namespace AIs
{
	public class VisModule : AIModule
	{
		private void Awake()
		{
			Transform transform = base.transform.FindDeepChild("WaterSplash");
			if (transform)
			{
				this.m_WaterSplashFX = transform.gameObject;
				this.m_WaterSplashFX.SetActive(false);
			}
		}

		public override void Initialize()
		{
			base.Initialize();
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_AudioSource.spatialBlend = 1f;
			this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
			this.m_AudioSource.maxDistance = 12f;
			this.m_AudioSource.spatialize = true;
			if (this.m_AI.m_ID == AI.AIID.BlackCaiman)
			{
				this.m_AttackClip = Resources.Load<AudioClip>("Sounds/AI/BlackCaiman/caiman_attack");
			}
			this.m_DamageAudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_DamageAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
			this.m_DamageAudioSource.spatialBlend = 1f;
			this.m_DamageAudioSource.rolloffMode = AudioRolloffMode.Linear;
			this.m_DamageAudioSource.maxDistance = 12f;
			this.m_DamageAudioSource.spatialize = true;
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			AIManager.BloodFXType key = (!info.m_DamageItem) ? AIManager.BloodFXType.Blunt : info.m_DamageItem.m_Info.m_BloodFXType;
			List<string> list = AIManager.Get().m_BloodFXNames[(int)key];
			if (list.Count == 0)
			{
				DebugUtils.Assert("Missing blood fxes!", true, DebugUtils.AssertType.Info);
				return;
			}
			string text = list[UnityEngine.Random.Range(0, list.Count)];
			text += ((!this.m_AI.m_Params.m_Human && !this.m_AI.m_Params.m_BigAnimal) ? "_S" : "_M");
			Vector3 vector = Vector3.zero;
			if (this.m_AI.m_RagdollBones.Count > 0)
			{
				float num = float.MaxValue;
				foreach (Collider collider in this.m_AI.m_RagdollBones.Keys)
				{
					Vector3 vector2 = collider.ClosestPoint(info.m_Position);
					float num2 = vector2.Distance(info.m_Position);
					if (num2 < num)
					{
						vector = vector2;
						Transform transform = collider.transform;
						num = num2;
					}
				}
			}
			else
			{
				vector = base.transform.position;
			}
			ParticlesManager.Get().Spawn(text, vector, Quaternion.LookRotation((Camera.main.transform.position - Camera.main.transform.right - vector).normalized), null);
			this.m_DamageAudioSource.Stop();
			this.m_DamageAudioSource.clip = AIManager.Get().m_FleshHitSounds[UnityEngine.Random.Range(0, AIManager.Get().m_FleshHitSounds.Count)];
			this.m_DamageAudioSource.Play();
		}

		public void OnStartAttack()
		{
			if (this.m_AttackClip)
			{
				this.m_AudioSource.clip = this.m_AttackClip;
				this.m_AudioSource.Play();
			}
		}

		public override void Update()
		{
			base.Update();
			int qualityLevel = QualitySettings.GetQualityLevel();
			if (qualityLevel == 4 && this.m_WaterSplashFX)
			{
				if (this.m_AI.IsDead() || this.m_AI.transform.position.Distance(Player.Get().transform.position) > 10f)
				{
					this.m_WaterSplashFX.SetActive(false);
					this.m_WaterSplashFX = null;
				}
				else if (!this.m_WaterSplashFX.activeSelf && this.m_AI.IsInWater() && this.m_AI.m_Animator.deltaPosition.magnitude > 0.01f)
				{
					this.m_WaterSplashFX.SetActive(true);
				}
				else if (this.m_WaterSplashFX.activeSelf && (!this.m_AI.IsInWater() || this.m_AI.m_Animator.deltaPosition.magnitude < 0.01f))
				{
					this.m_WaterSplashFX.SetActive(false);
				}
			}
		}

		private void OnDisable()
		{
			if (this.m_WaterSplashFX)
			{
				this.m_WaterSplashFX.SetActive(false);
			}
		}

		public AudioClip m_AttackClip;

		private AudioSource m_AudioSource;

		private AudioSource m_DamageAudioSource;

		private GameObject m_WaterSplashFX;
	}
}
