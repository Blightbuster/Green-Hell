using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class CJObject : MonoBehaviour
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

	protected virtual void Update()
	{
	}

	protected virtual void LateUpdate()
	{
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

	public virtual bool IsPlayer()
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
		return GreenHellGame.Instance.GetLocalization().Get(this.GetName());
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
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((!this.IsAI()) ? CJObject.s_DisappearItemFXPrefab : CJObject.s_DisappearAIFXPrefab, base.transform.position, base.transform.rotation);
		DisappearEffect component = gameObject.GetComponent<DisappearEffect>();
		component.Initialize(base.transform);
		PlayerSanityModule.Get().OnObjectDisappear(this, play_disappear_chatter);
	}

	[HideInInspector]
	public bool m_Hallucination;

	[HideInInspector]
	public bool m_HallucinationDisappearing;

	private static GameObject s_DisappearAIFXPrefab;

	private static GameObject s_DisappearItemFXPrefab;

	[HideInInspector]
	public bool m_IsBeingDestroyed;
}
