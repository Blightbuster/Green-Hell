using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class CureMachine : Trigger, ITriggerOwner, IAnimationEventsReceiver, IItemSlotParent, ITriggerThrough
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Animator = base.gameObject.GetComponent<Animator>();
		this.m_BloodTrigger.SetOwner(this);
		this.m_ItemSlot.gameObject.SetActive(false);
		this.m_ItemSlot.m_ActivityUpdate = false;
		this.m_CureSlot.gameObject.SetActive(false);
		this.m_CureSlot.m_ActivityUpdate = false;
		this.m_AudioSource = new GameObject("audio")
		{
			transform = 
			{
				parent = base.transform,
				position = Vector3.zero
			}
		}.AddComponent<AudioSource>();
		this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.AI);
		this.m_AudioSource.spatialBlend = 1f;
		this.m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_AudioSource.minDistance = 2f;
		this.m_AudioSource.maxDistance = 30f;
		this.m_AudioSource.spatialize = true;
		this.m_AudioSource.loop = true;
		this.m_AudioSource.clip = Resources.Load<AudioClip>("Sounds/Story/kjurmachine_sfx_processing_result_succes_loop");
		ScenarioManager.Get().CreateIntVariable("CureMachineState", (int)this.m_State);
	}

	protected override void Start()
	{
		base.Start();
		base.gameObject.GetComponent<AnimationEventsReceiver>().Initialize(null);
		this.SetState(CureMachine.State.WaitingForBlood);
	}

	public bool CanTrigger(Trigger trigger)
	{
		return trigger.enabled;
	}

	public void OnExecute(Trigger trigger, TriggerAction.TYPE action)
	{
		if (trigger == this.m_BloodTrigger)
		{
			this.SetState(CureMachine.State.BloodInserted);
		}
	}

	public void GetActions(Trigger trigger, List<TriggerAction.TYPE> actions)
	{
		if (trigger == this.m_BloodTrigger)
		{
			actions.Add(TriggerAction.TYPE.Insert);
			return;
		}
		actions.Add(TriggerAction.TYPE.Use);
	}

	public string GetTriggerInfoLocalized(Trigger trigger)
	{
		return string.Empty;
	}

	public string GetIconName(Trigger trigger)
	{
		return string.Empty;
	}

	public void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.CureMachineRClapOpenEnd)
		{
			if (this.m_State != CureMachine.State.Success)
			{
				this.SetState(CureMachine.State.WaitingForItem);
				return;
			}
		}
		else
		{
			if (id == AnimEventID.CureMachineRClapCloseEnd)
			{
				this.SetState(CureMachine.State.Processing);
				return;
			}
			if (id == AnimEventID.CureMachineProcessingEnd && this.m_ItemSlot.m_Item)
			{
				if (this.m_ItemSlot.m_Item.GetInfoID() == this.m_ProperItemID)
				{
					this.SetState(CureMachine.State.Success);
					return;
				}
				this.SetState(CureMachine.State.Fail);
			}
		}
	}

	public bool IsActive()
	{
		return base.enabled;
	}

	public bool ForceReceiveAnimEvent()
	{
		return false;
	}

	public bool CanInsertItem(Item item)
	{
		return (this.m_AvailableItems.Count <= 0 || this.m_AvailableItems.Contains(item.m_Info.m_ID)) && this.m_ItemSlot.gameObject.activeSelf;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (this.m_State == CureMachine.State.WaitingForItem)
		{
			this.SetState(CureMachine.State.Processing);
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
		if (this.m_State == CureMachine.State.Success)
		{
			slot.gameObject.SetActive(false);
			if (slot == this.m_CureSlot)
			{
				base.enabled = false;
			}
		}
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && base.enabled && this.m_State == CureMachine.State.Fail;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		actions.Add(TriggerAction.TYPE.Use);
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		this.m_Animator.SetTrigger(this.m_RClapOpenHash);
		if (this.m_State == CureMachine.State.Fail)
		{
			this.SetState(CureMachine.State.Reset);
		}
	}

	private void SetState(CureMachine.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnEnterState();
		ScenarioManager.Get().SetIntVariable("CureMachineState", (int)state);
	}

	private void OnEnterState()
	{
		switch (this.m_State)
		{
		case CureMachine.State.WaitingForBlood:
			this.m_Animator.CrossFadeInFixedTime(this.m_LClapOpenIdleHash, 0f, 0);
			this.m_Animator.CrossFadeInFixedTime(this.m_LClapOpenIdleHash, 0f, 1);
			this.m_BloodTrigger.gameObject.SetActive(true);
			this.m_BloodTrigger.enabled = true;
			this.m_ItemSlot.gameObject.SetActive(false);
			return;
		case CureMachine.State.BloodInserted:
			this.m_Animator.SetTrigger(this.m_LClapCloseHash);
			this.m_BloodTrigger.enabled = false;
			ScenarioManager.Get().SetBoolVariable("UsedBloodInAnalizer", true);
			for (int i = 0; i < this.m_BloodTrigger.m_RequiredItems.Count; i++)
			{
				Item item = InventoryBackpack.Get().FindItem(this.m_BloodTrigger.m_RequiredItems[i]);
				if (item)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			return;
		case CureMachine.State.WaitingForItem:
			this.m_ItemSlot.gameObject.SetActive(true);
			this.m_ItemSlot.Activate();
			return;
		case CureMachine.State.Processing:
			this.m_Animator.SetTrigger(this.m_RClapCloseHash);
			this.m_ItemSlot.gameObject.SetActive(false);
			return;
		case CureMachine.State.Fail:
			if (this.m_ItemSlot.m_Item)
			{
				UnityEngine.Object.Destroy(this.m_ItemSlot.m_Item.gameObject);
			}
			this.m_Animator.SetTrigger(this.m_FailHash);
			return;
		case CureMachine.State.Success:
		{
			if (this.m_ItemSlot.m_Item)
			{
				UnityEngine.Object.Destroy(this.m_ItemSlot.m_Item.gameObject);
			}
			this.m_Animator.SetTrigger(this.m_SuccessHash);
			this.m_BloodTrigger.gameObject.SetActive(false);
			Item item2 = ItemsManager.Get().CreateItem(this.m_ResultItemID, true, this.m_CureSlot.transform);
			this.m_CureSlot.InsertItem(item2);
			this.m_CureSlot.gameObject.SetActive(true);
			this.m_AudioSource.Play();
			return;
		}
		default:
			return;
		}
	}

	public override void Save()
	{
		base.Save();
		SaveGame.SaveVal("CureMashineState" + base.name, (int)this.m_State);
		SaveGame.SaveVal("CureMashineStateEnabled" + base.name, base.enabled);
	}

	public override void Load()
	{
		base.Load();
		this.m_ItemSlot.gameObject.SetActive(false);
		this.m_ItemSlot.m_ActivityUpdate = false;
		this.m_CureSlot.gameObject.SetActive(false);
		this.m_CureSlot.m_ActivityUpdate = false;
		this.m_AudioSource.Stop();
		base.enabled = SaveGame.LoadBVal("CureMashineStateEnabled" + base.name);
		this.m_State = (CureMachine.State)SaveGame.LoadIVal("CureMashineState" + base.name);
		switch (this.m_State)
		{
		case CureMachine.State.None:
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Lab_Analyzer_Open_Idle"), 0f, 0);
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Screen_Wait_Idle"), 0f, 1);
			break;
		case CureMachine.State.WaitingForBlood:
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("L_Clap_Open_Idle"), 0f, 0);
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Screen_Wait_Idle"), 0f, 1);
			break;
		case CureMachine.State.BloodInserted:
		case CureMachine.State.WaitingForItem:
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("R_Clap_Open"), 0f, 0);
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Screen_InsertSample"), 0f, 1);
			break;
		case CureMachine.State.Processing:
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Lab_Processing_loop"), 0f, 0);
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Screen_Processing"), 0f, 1);
			break;
		case CureMachine.State.Fail:
		case CureMachine.State.Reset:
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Lab_Processing_loop"), 0f, 0, 15f);
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Screen_Fail_Idle"), 0f, 1);
			break;
		case CureMachine.State.Success:
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("L_Clap_Open"), 0f, 0);
			this.m_Animator.CrossFadeInFixedTime(Animator.StringToHash("Screen_Succes_Idle"), 0f, 1);
			this.m_AudioSource.Play();
			break;
		}
		switch (this.m_State)
		{
		case CureMachine.State.WaitingForBlood:
			this.m_BloodTrigger.gameObject.SetActive(true);
			this.m_BloodTrigger.enabled = true;
			this.m_ItemSlot.gameObject.SetActive(false);
			return;
		case CureMachine.State.BloodInserted:
			this.m_BloodTrigger.enabled = false;
			return;
		case CureMachine.State.WaitingForItem:
			this.m_BloodTrigger.enabled = false;
			this.m_ItemSlot.gameObject.SetActive(true);
			this.m_ItemSlot.Activate();
			return;
		case CureMachine.State.Processing:
			this.m_BloodTrigger.enabled = false;
			this.m_ItemSlot.gameObject.SetActive(false);
			return;
		case CureMachine.State.Fail:
		case CureMachine.State.Reset:
			this.m_BloodTrigger.enabled = false;
			return;
		case CureMachine.State.Success:
			this.m_BloodTrigger.gameObject.SetActive(false);
			this.m_CureSlot.gameObject.SetActive(true);
			return;
		default:
			return;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_AudioSource.Stop();
	}

	private CureMachine.State m_State;

	private Animator m_Animator;

	public Trigger m_BloodTrigger;

	public Trigger m_DestroyTrigger;

	public ItemSlot m_ItemSlot;

	public ItemID m_ProperItemID = ItemID.None;

	public ItemID m_ResultItemID = ItemID.None;

	private int m_LClapOpenIdleHash = Animator.StringToHash("L_Clap_Open_Idle");

	private int m_LClapCloseHash = Animator.StringToHash("LClapClose");

	private int m_RClapOpenHash = Animator.StringToHash("RClapOpen");

	private int m_RClapCloseHash = Animator.StringToHash("RClapClose");

	private int m_ProcessingHash = Animator.StringToHash("Lab_Processing_loop");

	private int m_DestroyingHash = Animator.StringToHash("Destroying");

	private int m_FailHash = Animator.StringToHash("Fail");

	private int m_SuccessHash = Animator.StringToHash("Success");

	public ItemSlot m_CureSlot;

	private AudioSource m_AudioSource;

	private List<ItemID> m_AvailableItems = new List<ItemID>
	{
		ItemID.Stone,
		ItemID.Dryed_Liane,
		ItemID.Molineria_leaf,
		ItemID.Wood_Resin,
		ItemID.Bird_feather,
		ItemID.Tobacco_Leaf,
		ItemID.Dryed_Tobacco_Leaf,
		ItemID.Quassia_Amara_flowers,
		ItemID.Ficus_leaf,
		ItemID.Stingray_sting,
		ItemID.Anthill_powder,
		ItemID.Campfire_ash,
		ItemID.Dry_leaf,
		ItemID.CaneToad_Body,
		ItemID.PoisonDartFrog_Body,
		ItemID.ParrotMacaw_Body,
		ItemID.ParrotMacaw_yellow_Body,
		ItemID.Mouse_Body,
		ItemID.Ants,
		ItemID.banisteriopsis_scraps,
		ItemID.Larva,
		ItemID.Larva_Coocked,
		ItemID.Maggots,
		ItemID.Maggots_Cooked,
		ItemID.Egg,
		ItemID.Egg_Boiled,
		ItemID.Egg_Spoiled,
		ItemID.Coconut_flesh,
		ItemID.Coconut_flesh_Cooked,
		ItemID.Coconut_flesh_Spoiled,
		ItemID.Brazil_nut,
		ItemID.Brazil_nut_Spoiled,
		ItemID.Banana,
		ItemID.Banana_Spoiled,
		ItemID.Palm_heart,
		ItemID.Palm_Heart_dryed,
		ItemID.Palm_heart_Spoiled,
		ItemID.Raffia_nut,
		ItemID.Raffia_nut_Spoiled,
		ItemID.Cocona_fruit,
		ItemID.Cocona_fruit_Spoiled,
		ItemID.Phallus_indusiatus,
		ItemID.Phallus_indusiatus_Dryed,
		ItemID.Phallus_indusiatus_Spoiled,
		ItemID.Gerronema_viridilucens,
		ItemID.Gerronema_viridilucens_dryed,
		ItemID.Gerronema_viridilucens_Spoiled,
		ItemID.Gerronema_retiarium,
		ItemID.Gerronema_retiarium_dryed,
		ItemID.Gerronema_retiarium_Spoiled,
		ItemID.indigo_blue_leptonia,
		ItemID.indigo_blue_leptonia_dryed,
		ItemID.indigo_blue_leptonia_Spoiled,
		ItemID.copa_hongo,
		ItemID.copa_hongo_dryed,
		ItemID.copa_hongo_Spoiled,
		ItemID.Plantain_lily_leaf,
		ItemID.Charcoal,
		ItemID.Honeycomb,
		ItemID.coca_leafs,
		ItemID.lily_flower,
		ItemID.monstera_deliciosa_flower,
		ItemID.monstera_deliciosa_fruit,
		ItemID.Albahaca_Flower,
		ItemID.Albahaca_Leaf,
		ItemID.Guanabana_Fruit,
		ItemID.Guanabana_Fruit_Spoiled,
		ItemID.hura_crepitans,
		ItemID.hura_crepitans_Spoiled,
		ItemID.psychotria_viridis,
		ItemID.geoglossum_viride,
		ItemID.geoglossum_viride_Dryed,
		ItemID.geoglossum_viride_Spoiled,
		ItemID.marasmius_haematocephalus,
		ItemID.marasmius_haematocephalus_Dryed,
		ItemID.marasmius_haematocephalus_Spoiled,
		ItemID.Pirahnia_Meat_Raw,
		ItemID.Pirahnia_Meat_Cooked,
		ItemID.Pirahnia_Meat_Smoked,
		ItemID.Pirahnia_Meat_Dryed,
		ItemID.Pirahnia_Meat_Spoiled,
		ItemID.Pirahnia_Meat_Burned,
		ItemID.Peacock_Bass_Meat_Raw,
		ItemID.Peacock_Bass_Meat_Cooked,
		ItemID.Peacock_Bass_Meat_Smoked,
		ItemID.Peacock_Bass_Meat_Dryed,
		ItemID.Peacock_Bass_Meat_Spoiled,
		ItemID.Peacock_Bass_Meat_Burned,
		ItemID.Arowana_Meat_Raw,
		ItemID.Arowana_Meat_Cooked,
		ItemID.Arowana_Meat_Smoked,
		ItemID.Arowana_Meat_Dryed,
		ItemID.Arowana_Meat_Spoiled,
		ItemID.Arowana_Meat_Burned,
		ItemID.Stingray_Meat_Raw,
		ItemID.Stingray_Meat_Cooked,
		ItemID.Stingray_Meat_Smoked,
		ItemID.Stingray_Meat_Dryed,
		ItemID.Stingray_Meat_Spoiled,
		ItemID.Stingray_Meat_Burned,
		ItemID.Rattlesnake_Meat_Raw,
		ItemID.RattlesnakeMeat_Cooked,
		ItemID.Rattlesnake_Meat_Smoked,
		ItemID.Rattlesnake_Meat_Dryed,
		ItemID.Rattlesnake_Meat_Spoiled,
		ItemID.Rattlesnake_Meat_Burned,
		ItemID.Macaw_Meat_Raw,
		ItemID.Macaw_Meat_Cooked,
		ItemID.Macaw_Meat_Smoked,
		ItemID.Macaw_Meat_Dryed,
		ItemID.Macaw_Meat_Spoiled,
		ItemID.Macaw_Meat_Burned,
		ItemID.Cane_Toad_Meat_Raw,
		ItemID.Cane_Toad_Meat_Cooked,
		ItemID.Cane_Toad_Meat_Smoked,
		ItemID.Cane_Toad_Meat_Dryed,
		ItemID.Cane_Toad_Meat_Spoiled,
		ItemID.Cane_Toad_Meat_Burned,
		ItemID.Mouse_Meat_Raw,
		ItemID.Mouse_Meat_Cooked,
		ItemID.Mouse_Meat_Smoked,
		ItemID.Mouse_Meat_Dryed,
		ItemID.Mouse_Meat_Spoiled,
		ItemID.Mouse_Meat_Burned,
		ItemID.Toucan_Meat_Raw,
		ItemID.Toucan_Meat_Cooked,
		ItemID.Toucan_Meat_Smoked,
		ItemID.Toucan_Meat_Dryed,
		ItemID.Toucan_Meat_Spoiled,
		ItemID.Toucan_Meat_Burned,
		ItemID.Poison_Dart_Frog_Meat_Raw,
		ItemID.Poison_Dart_Frog_Meat_Cooked,
		ItemID.Poison_Dart_Frog_Meat_Smoked,
		ItemID.Poison_Dart_Frog_Meat_Dryed,
		ItemID.Poison_Dart_Frog_Meat_Spoiled,
		ItemID.Poison_Dart_Frog_Meat_Burned,
		ItemID.Green_Iguana_Meat_Raw,
		ItemID.Green_Iguana_Meat_Cooked,
		ItemID.Green_Iguana_Meat_Smoked,
		ItemID.Green_Iguana_Meat_Dryed,
		ItemID.Green_Iguana_Meat_Spoiled,
		ItemID.Green_Iguana_Meat_Burned,
		ItemID.Caiman_Lizard_Meat_Raw,
		ItemID.Caiman_Lizard_Meat_Cooked,
		ItemID.Caiman_Lizard_Meat_Smoked,
		ItemID.Caiman_Lizard_Meat_Dryed,
		ItemID.Caiman_Lizard_Meat_Spoiled,
		ItemID.Caiman_Lizard_Meat_Burned,
		ItemID.Peccary_Meat_Raw,
		ItemID.Peccary_Meat_Cooked,
		ItemID.Peccary_Meat_Smoked,
		ItemID.Peccary_Meat_Dryed,
		ItemID.Peccary_Meat_Spoiled,
		ItemID.Peccary_Meat_Burned,
		ItemID.Capybara_Meat_Raw,
		ItemID.Capybara_Meat_Cooked,
		ItemID.Capybara_Meat_Smoked,
		ItemID.Capybara_Meat_Dryed,
		ItemID.Capybara_Meat_Spoiled,
		ItemID.Capybara_Meat_Burned,
		ItemID.Tapir_Meat_Raw,
		ItemID.Tapir_Meat_Cooked,
		ItemID.Tapir_Meat_Smoked,
		ItemID.Tapir_Meat_Dryed,
		ItemID.Tapir_Meat_Spoiled,
		ItemID.Tapir_Meat_Burned,
		ItemID.Mud_Turtle_Meat_Raw,
		ItemID.Mud_Turtle_Meat_Cooked,
		ItemID.Mud_Turtle_Meat_Smoked,
		ItemID.Mud_Turtle_Meat_Dryed,
		ItemID.Mud_Turtle_Meat_Spoiled,
		ItemID.Mud_Turtle_Meat_Burned,
		ItemID.Red_Footed_Tortoise_Meat_Raw,
		ItemID.Red_Footed_Tortoise_Meat_Cooked,
		ItemID.Red_Footed_Tortoise_Meat_Smoked,
		ItemID.Red_Footed_Tortoise_Meat_Dryed,
		ItemID.Red_Footed_Tortoise_Meat_Spoiled,
		ItemID.Red_Footed_Tortoise_Meat_Burned,
		ItemID.Armadilo_Meat_Raw,
		ItemID.Armadilo_Meat_Cooked,
		ItemID.Armadilo_Meat_Smoked,
		ItemID.Armadilo_Meat_Dryed,
		ItemID.Armadilo_Meat_Spoiled,
		ItemID.Armadilo_Meat_Burned,
		ItemID.Armadillo_Three_Banded_Meat_Raw,
		ItemID.Armadillo_Three_Banded_Meat_Cooked,
		ItemID.Armadillo_Three_Banded_Meat_Smoked,
		ItemID.Armadillo_Three_Banded_Meat_Dryed,
		ItemID.Armadillo_Three_Banded_Meat_Spoiled,
		ItemID.Armadillo_Three_Banded_Meat_Burned,
		ItemID.Human_Meat_Raw,
		ItemID.Human_Meat_Cooked,
		ItemID.Human_Meat_Smoked,
		ItemID.Human_Meat_Dryed,
		ItemID.Human_Meat_Spoiled,
		ItemID.Human_Meat_Burned,
		ItemID.Puma_Meat_Raw,
		ItemID.Puma_Meat_Cooked,
		ItemID.Puma_Meat_Smoked,
		ItemID.Puma_Meat_Dryed,
		ItemID.Puma_Meat_Spoiled,
		ItemID.Puma_Meat_Burned,
		ItemID.Jaguar_Meat_Raw,
		ItemID.Jaguar_Meat_Cooked,
		ItemID.Jaguar_Meat_Smoked,
		ItemID.Jaguar_Meat_Dryed,
		ItemID.Jaguar_Meat_Spoiled,
		ItemID.Jaguar_Meat_Burned,
		ItemID.GoliathBirdEater_Body,
		ItemID.GoliathBirdEater_Cooked,
		ItemID.GoliathBirdEater_Spoiled,
		ItemID.GoliathBirdEater_Burned,
		ItemID.BrasilianWanderingSpider_Body,
		ItemID.BrasilianWanderingSpider_Cooked,
		ItemID.BrasilianWanderingSpider_Spoiled,
		ItemID.BrasilianWanderingSpider_Burned,
		ItemID.Goliath_birdeater_ash,
		ItemID.Scorpion_Body,
		ItemID.Scorpion_Cooked,
		ItemID.Scorpion_Spoiled,
		ItemID.Scorpion_Burned,
		ItemID.AngelFish_Body,
		ItemID.AngelFish_Cooked,
		ItemID.AngelFish_Smoked,
		ItemID.AngelFish_Dryed,
		ItemID.AngelFish_Spoiled,
		ItemID.AngelFish_Burned,
		ItemID.DiscusFish_Body,
		ItemID.DiscusFish_Cooked,
		ItemID.DiscusFish_Smoked,
		ItemID.DiscusFish_Dryed,
		ItemID.DiscusFish_Spoiled,
		ItemID.DiscusFish_Burned,
		ItemID.Snail_Raw,
		ItemID.Snail_Cooked,
		ItemID.Snail_Smoked,
		ItemID.Snail_Spoiled,
		ItemID.Snail_Burned,
		ItemID.Caiman_Meat_Raw,
		ItemID.Caiman_Meat_Cooked,
		ItemID.Caiman_Meat_Smoked,
		ItemID.Caiman_Meat_Dryed,
		ItemID.Caiman_Meat_Spoiled,
		ItemID.Caiman_Meat_Burned,
		ItemID.Shrimp_Raw,
		ItemID.Shrimp_Coocked,
		ItemID.Shrimp_Smoked,
		ItemID.Shrimp_Dryed,
		ItemID.Shrimp_Spoiled,
		ItemID.Shrimp_Burned,
		ItemID.PoisonDartFrog_Alive
	};

	private enum State
	{
		None,
		WaitingForBlood,
		BloodInserted,
		WaitingForItem,
		Processing,
		Fail,
		Success,
		Reset
	}
}
