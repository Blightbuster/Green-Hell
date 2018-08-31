using System;
using UnityEngine;

public class FireTool : MonoBehaviour, IItemSlotParent
{
	private void Awake()
	{
		this.m_KindlingSlot.gameObject.SetActive(false);
		this.m_KindlingSlot.m_ActivityUpdate = false;
		this.m_Animator = base.GetComponent<Animator>();
		this.m_NormalVis = base.transform.Find("BackpackVis").gameObject;
		this.m_NormalVis.SetActive(true);
		this.m_AnimationVis = base.transform.Find("Animation").gameObject;
		this.m_AnimationVis.SetActive(false);
		this.m_Light = this.m_AnimationVis.transform.Find("FireDummy").GetComponent<Light>();
		this.m_Particle = this.m_AnimationVis.transform.Find("FireDummy").GetComponent<ParticleSystem>();
		this.m_Emission = this.m_Particle.emission;
		this.m_FireParticle = this.m_AnimationVis.transform.Find("Fire").gameObject.GetComponent<ParticleSystem>();
		this.m_FireParticle.emission.enabled = false;
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public void OnInsertItem(ItemSlot slot)
	{
		slot.m_Item.transform.parent = base.transform;
		slot.Deactivate();
		slot.gameObject.SetActive(false);
		this.m_Particle.Play();
		MakeFireController.Get().OnAddKindling(slot.m_Item);
	}

	public ItemSlot m_KindlingSlot;

	private ParticleSystem m_FireParticle;

	[HideInInspector]
	public ParticleSystem m_Particle;

	[HideInInspector]
	public ParticleSystem.EmissionModule m_Emission;

	[HideInInspector]
	public Light m_Light;

	[HideInInspector]
	public Animator m_Animator;

	[Tooltip("Stamina Consumption per second")]
	public float m_StaminaConsumption;

	[HideInInspector]
	public GameObject m_NormalVis;

	[HideInInspector]
	public GameObject m_AnimationVis;
}
