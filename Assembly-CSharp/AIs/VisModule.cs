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

		public override void Initialize(Being being)
		{
			base.Initialize(being);
			if (this.m_AI.m_ID == AI.AIID.Thug)
			{
				this.AttachMask();
			}
		}

		private void AttachMask()
		{
			GameObject prefab;
			if (UnityEngine.Random.Range(0, 2) == 0)
			{
				prefab = GreenHellGame.Instance.GetPrefab("tribal_mask_a");
			}
			else
			{
				prefab = GreenHellGame.Instance.GetPrefab("tribal_mask_b");
			}
			this.m_MaskObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
			Transform headTransform = this.m_AI.GetHeadTransform();
			this.m_MaskObject.transform.position = headTransform.position;
			Matrix4x4 identity = Matrix4x4.identity;
			identity.SetColumn(0, this.m_AI.transform.right);
			identity.SetColumn(1, this.m_AI.transform.up);
			identity.SetColumn(2, this.m_AI.transform.forward);
			Quaternion rotation = CJTools.Math.QuaternionFromMatrix(identity);
			this.m_MaskObject.transform.rotation = rotation;
			this.m_MaskObject.transform.localScale = headTransform.transform.lossyScale;
			this.m_MaskObject.transform.SetParent(headTransform, true);
		}

		public override void OnTakeDamage(DamageInfo info)
		{
			base.OnTakeDamage(info);
			this.SpawnBlood(info);
			if (this.m_AI.m_ID != AI.AIID.RedFootedTortoise)
			{
				this.PlayDamageSound();
			}
		}

		private void SpawnBlood(DamageInfo info)
		{
			if (AI.IsTurtle(this.m_AI.m_ID) || this.m_AI.m_ID == AI.AIID.ArmadilloThreeBanded)
			{
				return;
			}
			AIManager.BloodFXType key = info.m_DamageItem ? info.m_DamageItem.m_Info.m_BloodFXType : AIManager.BloodFXType.Blunt;
			List<string> list = AIManager.Get().m_BloodFXNames[(int)key];
			if (list.Count == 0)
			{
				DebugUtils.Assert("Missing blood fxes!", true, DebugUtils.AssertType.Info);
				return;
			}
			string text = list[UnityEngine.Random.Range(0, list.Count)];
			text += ((this.m_AI.m_Params.m_Human || this.m_AI.m_Params.m_BigAnimal) ? "_M" : "_S");
			Vector3 vector = Vector3.zero;
			RagdollBone closestRagdollBone = this.m_AI.GetClosestRagdollBone(info.m_Position);
			if (closestRagdollBone)
			{
				vector = closestRagdollBone.transform.position;
			}
			else
			{
				vector = base.transform.position;
			}
			Vector3 forward = (info.m_Damager && info.m_Damager.IsPlayer()) ? (Camera.main.transform.position - Camera.main.transform.right - vector).normalized : (-info.m_HitDir);
			ParticlesManager.Get().Spawn(text, vector, Quaternion.LookRotation(forward), Vector3.zero, null, -1f, false);
		}

		public void OnStartAttack()
		{
			if (this.m_AI.m_SoundModule != null)
			{
				this.m_AI.m_SoundModule.PlayAttackSound();
			}
		}

		public override void Update()
		{
			base.Update();
			if (QualitySettings.GetQualityLevel() == 4 && this.m_WaterSplashFX)
			{
				if (this.m_AI.IsDead() || this.m_AI.transform.position.Distance(Player.Get().transform.position) > 10f)
				{
					this.m_WaterSplashFX.SetActive(false);
					this.m_WaterSplashFX = null;
					return;
				}
				if (!this.m_WaterSplashFX.activeSelf && this.m_AI.IsInWater() && this.m_AI.m_Animator.deltaPosition.magnitude > 0.01f)
				{
					this.m_WaterSplashFX.SetActive(true);
					return;
				}
				if (this.m_WaterSplashFX.activeSelf && (!this.m_AI.IsInWater() || this.m_AI.m_Animator.deltaPosition.magnitude < 0.01f))
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

		private void PlayDamageSound()
		{
			if (this.m_DamageAudioSource == null)
			{
				this.m_DamageAudioSource = base.gameObject.AddComponent<AudioSource>();
				this.m_DamageAudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
				this.m_DamageAudioSource.spatialBlend = 1f;
				this.m_DamageAudioSource.rolloffMode = AudioRolloffMode.Linear;
				this.m_DamageAudioSource.maxDistance = 12f;
				this.m_DamageAudioSource.spatialize = true;
			}
			this.m_DamageAudioSource.Stop();
			this.m_DamageAudioSource.clip = AIManager.Get().m_FleshHitSounds[UnityEngine.Random.Range(0, AIManager.Get().m_FleshHitSounds.Count)];
			this.m_DamageAudioSource.Play();
		}

		private AudioSource m_DamageAudioSource;

		private GameObject m_WaterSplashFX;

		private GameObject m_MaskObject;
	}
}
