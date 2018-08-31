using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class HarvestingSmallAnimalController : PlayerController
{
	public static HarvestingSmallAnimalController Get()
	{
		return HarvestingSmallAnimalController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HarvestingSmallAnimalController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.HarvestingSmallAnimal;
		this.SetupAudio();
	}

	private void SetupAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		AudioClip item = (AudioClip)Resources.Load("Sounds/Harvesting/harvesting_generic_01");
		this.m_HarvestingClips.Add(item);
		item = (AudioClip)Resources.Load("Sounds/Harvesting/harvesting_generic_02");
		this.m_HarvestingClips.Add(item);
		item = (AudioClip)Resources.Load("Sounds/Harvesting/harvesting_generic_03");
		this.m_HarvestingClips.Add(item);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Item.enabled = false;
		this.m_InInventory = Inventory3DManager.Get().gameObject.activeSelf;
		if (this.m_InInventory)
		{
			Inventory3DManager.Get().m_InputsBlocked = true;
			Inventory3DManager.Get().Deactivate();
		}
		this.m_Animator.SetBool(this.m_HarvestingHash, true);
		this.PlayHarvestingSound();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_HarvestingHash, false);
		this.m_AudioSource.Stop();
		if (this.m_InInventory)
		{
			Inventory3DManager.Get().m_InputsBlocked = false;
			Player.Get().m_OpenBackpackSheduled = true;
		}
		if (this.m_Item != null)
		{
			this.m_Item.enabled = true;
		}
		LookController.Get().m_LookDev.y = 0f;
		LookController.Get().m_WantedLookDev.y = 0f;
	}

	public void SetItem(Item item)
	{
		this.m_Item = item;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.HarvestingEnd)
		{
			this.m_Item.Harvest();
			this.m_Item = null;
			if (this.m_InInventory)
			{
				Inventory3DManager.Get().Activate();
				Inventory3DManager.Get().m_InputsBlocked = false;
			}
			this.Stop();
		}
	}

	private void PlayHarvestingSound()
	{
		this.m_AudioSource.PlayOneShot(this.m_HarvestingClips[UnityEngine.Random.Range(0, this.m_HarvestingClips.Count)]);
	}

	private int m_HarvestingHash = Animator.StringToHash("Craft");

	private Item m_Item;

	private bool m_InInventory;

	private static HarvestingSmallAnimalController s_Instance;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_HarvestingClips = new List<AudioClip>();
}
