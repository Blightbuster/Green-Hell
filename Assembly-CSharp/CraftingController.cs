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
		base.m_ControllerType = PlayerControllerType.Crafting;
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
		this.m_AudioSource.Stop();
		if (this.m_InProgress)
		{
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
			this.m_InProgress = false;
		}
		foreach (Item item in this.m_Items)
		{
			if (item && item.gameObject)
			{
				item.gameObject.SetActive(true);
				Inventory3DManager.Get().DropItem(item);
			}
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
	}

	public override void OnInputAction(InputActionData action_data)
	{
		InputsManager.InputAction action = action_data.m_Action;
	}

	public void StartCrafting(List<Item> items)
	{
		this.m_Items.Clear();
		foreach (Item item in items)
		{
			this.m_Items.Add(item);
			item.gameObject.SetActive(false);
		}
		CraftingManager.Get().RemoveAllItems(true);
		this.m_Animator.SetBool(this.m_CraftHash, true);
		if (this.m_Player.GetCurrentItem(Hand.Right))
		{
			this.m_Player.GetCurrentItem(Hand.Right).gameObject.SetActive(false);
		}
		if (this.m_Player.GetCurrentItem(Hand.Left))
		{
			this.m_Player.GetCurrentItem(Hand.Left).gameObject.SetActive(false);
		}
		Inventory3DManager.Get().Deactivate();
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
		this.m_InProgress = true;
		this.PlayCraftingSound();
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		base.OnAnimEvent(id);
		if (id == AnimEventID.CraftingEnd)
		{
			Inventory3DManager.Get().Activate();
			CraftingManager.Get().Craft(this.m_Items);
			this.m_Items.Clear();
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

	public bool BlockInventoryInputs()
	{
		return base.enabled && this.m_Animator.GetBool(this.m_CraftHash);
	}

	private int m_CraftHash = Animator.StringToHash("Craft");

	[HideInInspector]
	public bool m_InProgress;

	private static CraftingController s_Instance;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_CraftingClips = new List<AudioClip>();

	[HideInInspector]
	public List<Item> m_Items = new List<Item>();
}
