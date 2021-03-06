﻿using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class CJObject : MonoBehaviour, IRelevanceCalculator
{
	protected virtual void Awake()
	{
		if (!CJObject.s_DisappearAIFXPrefab)
		{
			CJObject.s_DisappearAIFXPrefab = (Resources.Load("Prefabs/FX/DisappearAIFX") as GameObject);
		}
		if (!CJObject.s_DisappearItemFXPrefab)
		{
			CJObject.s_DisappearItemFXPrefab = (Resources.Load("Prefabs/FX/DisappearItemFX") as GameObject);
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void Start()
	{
	}

	protected virtual void LateUpdate()
	{
	}

	protected virtual void Update()
	{
		this.UpdateHitShake();
	}

	protected virtual void OnDestroy()
	{
		this.m_IsBeingDestroyed = true;
	}

	public virtual bool TakeDamage(DamageInfo damage_info)
	{
		return true;
	}

	public virtual HitCollisionType GetHitCollisionType()
	{
		return HitCollisionType.Collider;
	}

	public virtual List<OBB> GetColliderBoxes()
	{
		return null;
	}

	public virtual bool IsItem()
	{
		return false;
	}

	public virtual bool IsItemHold()
	{
		return false;
	}

	public virtual bool IsItemReplacer()
	{
		return false;
	}

	public virtual bool IsAI()
	{
		return false;
	}

	public virtual bool IsHumanAI()
	{
		return false;
	}

	public virtual bool IsFish()
	{
		return false;
	}

	public virtual bool IsPlayer()
	{
		return false;
	}

	public virtual bool IsShower()
	{
		return false;
	}

	public virtual bool CanBeImpaledOnSpear()
	{
		return false;
	}

	public virtual void OnImpaleOnSpear()
	{
	}

	public virtual string GetName()
	{
		return base.name;
	}

	public virtual string GetTriggerInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get(this.GetName(), true);
	}

	public virtual bool CanReceiveDamageOfType(DamageType damage_type)
	{
		return false;
	}

	public virtual string GetParticleOnHit()
	{
		return string.Empty;
	}

	public void Disappear(bool play_disappear_chatter = false)
	{
		if (this.m_HallucinationDisappearing)
		{
			return;
		}
		this.m_HallucinationDisappearing = true;
		UnityEngine.Object.Instantiate<GameObject>(this.IsAI() ? CJObject.s_DisappearAIFXPrefab : CJObject.s_DisappearItemFXPrefab, base.transform.position, base.transform.rotation).GetComponent<DisappearEffect>().Initialize(base.transform);
		PlayerSanityModule.Get().OnObjectDisappear(this, play_disappear_chatter);
	}

	public void HitShake()
	{
		if (this.m_HitShake)
		{
			return;
		}
		this.m_StartHitShakeTime = Time.time;
		this.m_HitShake = true;
		this.m_StartHitShakePosition = base.transform.position;
	}

	private void UpdateHitShake()
	{
		if (!this.m_HitShake)
		{
			return;
		}
		base.transform.position = this.m_StartHitShakePosition + Vector3.forward * Mathf.Sign(Time.time * 123f) * 0.01f;
		if (Time.time - this.m_StartHitShakeTime > 0.1f)
		{
			base.transform.position = this.m_StartHitShakePosition;
			this.m_HitShake = false;
		}
	}

	public bool IsSceneObject()
	{
		return this.m_SceneObject;
	}

	public void SetSceneObject(bool set = true)
	{
		this.m_SceneObject = set;
	}

	public virtual bool CanBeRemovedByRelevance(bool on_owner)
	{
		return !this.m_SceneObject;
	}

	public virtual float CalculateRelevance(IPeerWorldRepresentation peer, bool is_owner)
	{
		if (this.m_Hallucination && !peer.IsLocalPeer())
		{
			return 0f;
		}
		return -1f;
	}

	[HideInInspector]
	public bool m_Hallucination;

	[HideInInspector]
	public bool m_HallucinationDisappearing;

	private static GameObject s_DisappearAIFXPrefab;

	private static GameObject s_DisappearItemFXPrefab;

	[HideInInspector]
	public bool m_IsBeingDestroyed;

	[HideInInspector]
	public bool m_HitShake;

	private float m_StartHitShakeTime;

	private Vector3 m_StartHitShakePosition = Vector3.zero;

	[SerializeField]
	public bool m_SceneObject;
}
