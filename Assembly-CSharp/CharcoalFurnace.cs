using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class CharcoalFurnace : Construction, IProcessor, IItemSlotParent, IGhostPartParent, ITriggerThrough, IFireObject
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Sound = base.gameObject.GetComponent<AudioSource>();
		this.m_FireSound = this.m_Sound.clip;
		this.m_WoodSlot = base.gameObject.GetComponentInChildren<ItemSlot>();
		base.RegisterConstantUpdateItem();
	}

	protected override void Start()
	{
		base.Start();
		if (this.m_State == CharcoalFurnace.State.None)
		{
			this.SetState(CharcoalFurnace.State.WaitingForWood, false);
		}
		FirecampGroupsManager.Get().OnCreateFirecamp(this);
	}

	public override bool IsCharcoalFurnace()
	{
		return true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (FirecampGroupsManager.Get())
		{
			FirecampGroupsManager.Get().OnDestroyFirecamp(this);
		}
		base.UnregisterConstantUpdateItem();
	}

	public float GetProcessProgress(Trigger trigger)
	{
		if (this.m_State == CharcoalFurnace.State.WaitingForWood)
		{
			return this.m_CoalValue / this.m_RequiredCoalValue;
		}
		return this.m_BurningDuration / this.m_BurningLength;
	}

	public void Ignite()
	{
		this.SetState(CharcoalFurnace.State.Burning, false);
	}

	public bool IsBurning()
	{
		return this.m_State == CharcoalFurnace.State.Burning;
	}

	private void SetState(CharcoalFurnace.State state, bool from_save = false)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnEnterState(from_save);
	}

	private void OnEnterState(bool from_save)
	{
		switch (this.m_State)
		{
		case CharcoalFurnace.State.WaitingForWood:
			if (this.m_Stand)
			{
				UnityEngine.Object.Destroy(this.m_Stand.gameObject);
				this.m_Stand = null;
			}
			this.m_WoodItemIDs.Clear();
			this.m_CoalValue = 0f;
			HUDProcess.Get().RegisterProcess(this, "charcoal_furnace_wood", this, true);
			this.m_WoodSlot.gameObject.SetActive(true);
			this.m_Door.gameObject.SetActive(false);
			this.m_Door.enabled = false;
			break;
		case CharcoalFurnace.State.WaitingForDoor:
			HUDProcess.Get().UnregisterProcess(this);
			this.m_WoodSlot.gameObject.SetActive(false);
			this.m_Door.Reset();
			this.m_Door.gameObject.SetActive(true);
			this.m_Door.enabled = true;
			break;
		case CharcoalFurnace.State.WaitingForFire:
			this.m_Sound.loop = false;
			this.m_Sound.clip = this.m_BuildDoorSound;
			this.m_Sound.Play();
			this.m_WoodSlot.gameObject.SetActive(false);
			this.m_Door.gameObject.SetActive(true);
			this.m_Door.enabled = false;
			break;
		case CharcoalFurnace.State.Burning:
			this.m_WoodSlot.gameObject.SetActive(false);
			this.m_Door.gameObject.SetActive(true);
			this.m_Door.enabled = false;
			HUDProcess.Get().RegisterProcess(this, "furnace_charcoal_hot", this, true);
			this.m_Effects.SetActive(true);
			this.m_BurningDuration = 0f;
			this.m_Sound.clip = this.m_FireSound;
			this.m_Sound.loop = true;
			this.m_Sound.Play();
			break;
		case CharcoalFurnace.State.DestroyDoor:
			this.m_WoodItemIDs.Clear();
			this.m_WoodSlot.gameObject.SetActive(false);
			this.m_Door.gameObject.SetActive(true);
			this.m_Door.enabled = false;
			HUDProcess.Get().UnregisterProcess(this);
			this.m_Effects.SetActive(false);
			this.m_Sound.Stop();
			break;
		case CharcoalFurnace.State.Ready:
			this.m_CoalValue = 0f;
			this.CreateStand(from_save);
			this.m_Stand.enabled = true;
			this.m_WoodSlot.gameObject.SetActive(false);
			this.m_Door.gameObject.SetActive(false);
			this.m_Door.enabled = false;
			this.m_Sound.clip = this.m_RemoveDoorSound;
			this.m_Sound.loop = false;
			this.m_Sound.Play();
			break;
		}
		this.UpdateVis();
	}

	private void CreateStand(bool from_save)
	{
		this.m_Stand = (ItemsManager.Get().CreateItem(ItemID.Charcoal_Stand, true, base.transform) as Stand);
		this.m_Stand.m_CantSave = true;
		this.m_Stand.m_NumItems = this.m_Stand.m_Vis.Count;
		this.m_Stand.UpdateVis();
		this.m_Stand.m_CanInsert = false;
		this.m_Stand.m_DestroyEmpty = true;
		if (!from_save)
		{
			float d = 0.4f;
			ItemsManager.Get().CreateItem(ItemID.Campfire_ash, true, base.transform.position + Vector3.forward * d, base.transform.rotation);
			ItemsManager.Get().CreateItem(ItemID.Campfire_ash, true, base.transform.position + Quaternion.AngleAxis(120f, Vector3.up) * Vector3.forward * d, base.transform.rotation);
			ItemsManager.Get().CreateItem(ItemID.Campfire_ash, true, base.transform.position + Quaternion.AngleAxis(240f, Vector3.up) * Vector3.forward * d, base.transform.rotation);
		}
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		if (this.m_State == CharcoalFurnace.State.Burning)
		{
			float num = Time.deltaTime;
			if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
			{
				num = Player.GetSleepTimeFactor();
			}
			this.m_BurningDuration += num;
			if (this.m_BurningDuration >= this.m_BurningLength)
			{
				this.SetState(CharcoalFurnace.State.DestroyDoor, false);
			}
		}
		else if (this.m_State == CharcoalFurnace.State.Ready && this.m_Stand.m_NumItems == 0)
		{
			this.SetState(CharcoalFurnace.State.WaitingForWood, false);
		}
		if (this.m_DoorLOD1.activeSelf != this.m_Door.gameObject.activeSelf)
		{
			this.m_DoorLOD1.SetActive(this.m_Door.gameObject.activeSelf);
		}
	}

	public bool CanInsertItem(Item item)
	{
		return this.m_State == CharcoalFurnace.State.WaitingForWood;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		this.m_CoalValue += slot.m_Item.m_Info.m_CoalValue;
		this.m_CoalValue = Mathf.Min(this.m_CoalValue, this.m_RequiredCoalValue);
		this.m_WoodItemIDs.Add((int)slot.m_Item.GetInfoID());
		Item item = slot.m_Item;
		slot.RemoveItem(item, false);
		UnityEngine.Object.Destroy(item.gameObject);
		this.UpdateVis();
		if (this.m_CoalValue == this.m_RequiredCoalValue)
		{
			this.SetState(CharcoalFurnace.State.WaitingForDoor, false);
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public Material GetActiveMaterial()
	{
		return this.m_ActiveMaterial;
	}

	public Material GetHighlightedMaterial()
	{
		return this.m_HighlightedMaterial;
	}

	public void OnGhostFulfill(bool from_save)
	{
		this.SetState(CharcoalFurnace.State.WaitingForFire, false);
	}

	private bool ShouldShowEmberRequired()
	{
		if (this.m_State != CharcoalFurnace.State.WaitingForFire)
		{
			return false;
		}
		Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
		bool flag = false;
		if (currentItem)
		{
			ItemID id = currentItem.m_Info.m_ID;
			flag = (id == ItemID.Fire || id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch);
		}
		return !flag;
	}

	public override bool ShowAdditionalInfo()
	{
		return this.ShouldShowEmberRequired();
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Req_Additional_Info", true);
	}

	public override string GetIconName()
	{
		if (this.ShouldShowEmberRequired())
		{
			return "ember_required";
		}
		if (this.m_State == CharcoalFurnace.State.DestroyDoor)
		{
			return "Charcoal_pile";
		}
		return base.GetIconName();
	}

	public override string GetTriggerInfoLocalized()
	{
		if (this.ShouldShowEmberRequired())
		{
			return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Required", true);
		}
		return base.GetTriggerInfoLocalized();
	}

	public override bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (this.m_State == CharcoalFurnace.State.WaitingForFire)
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem)
			{
				ItemID id = currentItem.m_Info.m_ID;
				if (id == ItemID.Fire)
				{
					return true;
				}
				if ((id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch) && ((Torch)currentItem).m_Burning)
				{
					return true;
				}
			}
			if (this.ShouldShowEmberRequired())
			{
				return true;
			}
		}
		else if (this.m_State == CharcoalFurnace.State.DestroyDoor)
		{
			return true;
		}
		return false;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (this.m_State == CharcoalFurnace.State.WaitingForFire)
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (!currentItem)
			{
				return;
			}
			ItemID id = currentItem.m_Info.m_ID;
			if (id == ItemID.Fire)
			{
				actions.Add(TriggerAction.TYPE.Ignite);
			}
			if ((id == ItemID.Torch || id == ItemID.Weak_Torch || id == ItemID.Tobacco_Torch) && ((Torch)currentItem).m_Burning)
			{
				actions.Add(TriggerAction.TYPE.Ignite);
				return;
			}
		}
		else if (this.m_State == CharcoalFurnace.State.DestroyDoor)
		{
			actions.Add(TriggerAction.TYPE.Use);
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Ignite)
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem && currentItem.GetInfoID() == ItemID.Fire)
			{
				ItemController.Get().IgniteFireObject(this);
				return;
			}
			if (action == TriggerAction.TYPE.Ignite)
			{
				Player.Get().m_TorchController.IgniteFireObject(this);
				return;
			}
		}
		else if (action == TriggerAction.TYPE.Use)
		{
			this.SetState(CharcoalFurnace.State.Ready, false);
		}
	}

	private void UpdateVis()
	{
		float num = this.m_CoalValue / this.m_RequiredCoalValue;
		for (int i = 0; i < this.m_Visualisations.Count; i++)
		{
			bool active;
			if (this.m_State == CharcoalFurnace.State.WaitingForWood)
			{
				active = ((float)i / (float)this.m_Visualisations.Count < num);
			}
			else
			{
				active = (this.m_State != CharcoalFurnace.State.Ready);
			}
			this.m_Visualisations[i].SetActive(active);
		}
	}

	public override void DestroyMe(bool check_connected = true)
	{
		base.DestroyMe(check_connected);
		if (this.m_Stand && !this.m_Stand.enabled)
		{
			this.m_Stand.enabled = true;
		}
		foreach (int item_id in this.m_WoodItemIDs)
		{
			if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				ItemsManager.Get().CreateItem((ItemID)item_id, true, base.transform);
			}
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("CharcoalFurnaceState" + index, (int)this.m_State);
		SaveGame.SaveVal("CharcoalFurnaceVal" + index, this.m_CoalValue);
		SaveGame.SaveVal("CharcoalFurnaceBurningDur" + index, this.m_BurningDuration);
		SaveGame.SaveVal("CharcoalFurnaceCount" + index, this.m_WoodItemIDs.Count);
		for (int i = 0; i < this.m_WoodItemIDs.Count; i++)
		{
			SaveGame.SaveVal("CharcoalFurnaceID" + index * 666 + i.ToString(), this.m_WoodItemIDs[i]);
		}
		SaveGame.SaveVal("CharcoalFurnaceStandCount" + index, this.m_Stand ? this.m_Stand.m_NumItems : 0);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_State = CharcoalFurnace.State.None;
		this.SetState((CharcoalFurnace.State)SaveGame.LoadIVal("CharcoalFurnaceState" + index), true);
		if (this.m_Stand)
		{
			this.m_Stand.m_NumItems = SaveGame.LoadIVal("CharcoalFurnaceStandCount" + index);
		}
		this.m_CoalValue = SaveGame.LoadFVal("CharcoalFurnaceVal" + index);
		this.m_WoodItemIDs.Clear();
		int num = SaveGame.LoadIVal("CharcoalFurnaceCount" + index);
		for (int i = 0; i < num; i++)
		{
			this.m_WoodItemIDs.Add(SaveGame.LoadIVal("CharcoalFurnaceID" + index * 666 + i.ToString()));
		}
		this.m_BurningDuration = SaveGame.LoadFVal("CharcoalFurnaceBurningDur" + index);
		this.UpdateVis();
	}

	private CharcoalFurnace.State m_State;

	public float m_RequiredCoalValue = 100f;

	private float m_CoalValue;

	public GhostPart m_Door;

	public GameObject m_DoorLOD1;

	public GameObject m_Effects;

	public Material m_ActiveMaterial;

	public Material m_HighlightedMaterial;

	public float m_BurningLength = 120f;

	private float m_BurningDuration;

	private ItemSlot m_WoodSlot;

	private Stand m_Stand;

	public List<GameObject> m_Visualisations = new List<GameObject>();

	private List<int> m_WoodItemIDs = new List<int>();

	private AudioSource m_Sound;

	private AudioClip m_FireSound;

	public AudioClip m_BuildDoorSound;

	public AudioClip m_RemoveDoorSound;

	private enum State
	{
		None,
		WaitingForWood,
		WaitingForDoor,
		WaitingForFire,
		Burning,
		DestroyDoor,
		Ready
	}
}
