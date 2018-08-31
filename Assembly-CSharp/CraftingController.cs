using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class CraftingController : PlayerController
{
	public static CraftingController Get()
	{
		return CraftingController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		CraftingController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Crafting;
		this.SetupAudio();
	}

	private void SetupAudio()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		AudioClip item = (AudioClip)Resources.Load("Sounds/Crafting/crafting_generic_01");
		this.m_CraftingClips.Add(item);
		item = (AudioClip)Resources.Load("Sounds/Crafting/crafting_generic_02");
		this.m_CraftingClips.Add(item);
		item = (AudioClip)Resources.Load("Sounds/Crafting/crafting_generic_03");
		this.m_CraftingClips.Add(item);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_InProgress = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_CraftHash, false);
		this.m_InProgress = false;
		this.m_AudioSource.Stop();
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if (action != InputsManager.InputAction.Drop)
		{
		}
	}

	public void StartCrafting()
	{
		this.m_Animator.SetBool(this.m_CraftHash, true);
		if (this.m_Player.GetCurrentItem(Hand.Right))
		{
			this.m_Player.GetCurrentItem(Hand.Right).gameObject.SetActive(false);
		}
		if (this.m_Player.GetCurrentItem(Hand.Left))
		{
			this.m_Player.GetCurrentItem(Hand.Left).gameObject.SetActive(false);
		}
		Inventory3DManager.Get().m_InputsBlocked = true;
		Inventory3DManager.Get().m_InventoryImage.enabled = false;
		InventoryBackpack.Get().m_Backpack.gameObject.SetActive(false);
		CursorManager.Get().ShowCursor(false);
		this.m_InProgress = true;
		this.PlayCraftingSound();
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.CraftingEnd)
		{
			Inventory3DManager.Get().m_InventoryImage.enabled = true;
			InventoryBackpack.Get().m_Backpack.gameObject.SetActive(true);
			CursorManager.Get().ShowCursor(true);
			Inventory3DManager.Get().m_InputsBlocked = false;
			CraftingManager.Get().Craft();
			this.m_Animator.SetBool(this.m_CraftHash, false);
			this.m_AudioSource.Stop();
			this.m_InProgress = false;
			if (this.m_Player.GetCurrentItem(Hand.Right))
			{
				this.m_Player.GetCurrentItem(Hand.Right).gameObject.SetActive(true);
			}
			if (this.m_Player.GetCurrentItem(Hand.Left))
			{
				this.m_Player.GetCurrentItem(Hand.Left).gameObject.SetActive(true);
			}
		}
	}

	private void PlayCraftingSound()
	{
		this.m_AudioSource.PlayOneShot(this.m_CraftingClips[UnityEngine.Random.Range(0, this.m_CraftingClips.Count)]);
	}

	private int m_CraftHash = Animator.StringToHash("Craft");

	[HideInInspector]
	public bool m_InProgress;

	private static CraftingController s_Instance;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_CraftingClips = new List<AudioClip>();
}
