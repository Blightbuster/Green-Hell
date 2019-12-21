using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Forge : Construction, IProcessor, IItemSlotParent, ITriggerThrough, IFireObject
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Sound = base.gameObject.GetComponent<AudioSource>();
		this.m_BurningDuration = this.m_BurningLength;
		base.RegisterConstantUpdateItem();
	}

	protected override void Start()
	{
		base.Start();
		if (this.m_State == Forge.State.None)
		{
			this.SetState(Forge.State.WaitingForItem);
		}
		HUDProcess.Get().RegisterProcess(this, "forge_charcoal", this, true);
		FirecampGroupsManager.Get().OnCreateFirecamp(this);
	}

	public override bool IsForge()
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
		Item x = (Item)trigger;
		if (x == this)
		{
			return (this.m_BurningLength - this.m_BurningDuration) / this.m_BurningLength;
		}
		if (x == this.m_Item)
		{
			Forge.State state = this.m_State;
			if (state == Forge.State.MakingOre)
			{
				return 1f - (this.m_MakingOreLength - this.m_ForgingDuration) / this.m_MakingOreLength;
			}
			if (state == Forge.State.MakingForm)
			{
				return 1f - (this.m_MakingFormLength - this.m_ForgingDuration) / this.m_MakingFormLength;
			}
		}
		return 0f;
	}

	private void SetState(Forge.State state)
	{
		if (this.m_State == state)
		{
			return;
		}
		this.m_State = state;
		this.OnEnterState();
	}

	private void OnEnterState()
	{
		switch (this.m_State)
		{
		case Forge.State.WaitingForItem:
			this.m_Item = null;
			this.m_ItemSlot.gameObject.SetActive(true);
			this.m_ForgingDuration = 0f;
			return;
		case Forge.State.MakingOre:
		case Forge.State.MakingForm:
			this.m_ItemSlot.gameObject.SetActive(false);
			HUDProcess.Get().RegisterProcess(this.m_Item, this.GetProcessIconName(), this, true);
			return;
		case Forge.State.Ore:
			HUDProcess.Get().UnregisterProcess(this.m_Item);
			this.m_Item.m_CurrentSlot.RemoveItem();
			UnityEngine.Object.Destroy(this.m_Item.gameObject);
			this.m_ItemSlot.gameObject.SetActive(false);
			ItemsManager.Get().CreateItem(ItemID.iron_ore_melted, true, this.m_ItemSlot.transform);
			this.SetState(Forge.State.WaitingForItem);
			return;
		case Forge.State.Form:
			HUDProcess.Get().UnregisterProcess(this.m_Item);
			this.m_Item.m_CurrentSlot.RemoveItem();
			UnityEngine.Object.Destroy(this.m_Item.gameObject);
			this.m_ItemSlot.gameObject.SetActive(false);
			ItemsManager.Get().CreateItem(this.m_Item.GetInfoID().ToString() + "_baked", true, this.m_ItemSlot.transform);
			this.SetState(Forge.State.WaitingForItem);
			return;
		default:
			return;
		}
	}

	public override void ConstantUpdate()
	{
		base.ConstantUpdate();
		float num = Time.deltaTime;
		bool flag = this.GetProcessProgress(this) < 1f;
		if (this.m_CharcoalSlot.gameObject.activeSelf != flag)
		{
			this.m_CharcoalSlot.gameObject.SetActive(flag);
		}
		if (this.m_Burning)
		{
			if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
			{
				num = Player.GetSleepTimeFactor();
			}
			this.m_BurningDuration += num;
			if (this.m_BurningDuration >= this.m_BurningLength)
			{
				this.m_BurningDuration = this.m_BurningLength;
				this.Extinguish();
			}
		}
		if (this.m_Burning)
		{
			Forge.State state = this.m_State;
			if (state != Forge.State.MakingOre)
			{
				if (state == Forge.State.MakingForm)
				{
					if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
					{
						num = Player.GetSleepTimeFactor();
					}
					this.m_ForgingDuration += num;
					if (this.m_ForgingDuration >= this.m_MakingFormLength)
					{
						this.SetState(Forge.State.Form);
						return;
					}
				}
			}
			else
			{
				if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
				{
					num = Player.GetSleepTimeFactor();
				}
				this.m_ForgingDuration += num;
				if (this.m_ForgingDuration >= this.m_MakingOreLength)
				{
					this.SetState(Forge.State.Ore);
				}
			}
		}
	}

	public bool IsBurning()
	{
		return this.m_Burning;
	}

	public void Ignite()
	{
		this.m_Burning = true;
		this.m_Effects.SetActive(true);
		this.m_Sound.Play();
	}

	private void Extinguish()
	{
		this.m_Burning = false;
		this.m_Effects.SetActive(false);
		this.m_Sound.Stop();
	}

	public bool CanInsertItem(Item item)
	{
		return true;
	}

	public void OnInsertItem(ItemSlot slot)
	{
		if (slot == this.m_CharcoalSlot)
		{
			this.m_BurningDuration -= slot.m_Item.m_Info.m_AddForgeBurningTime;
			if (this.m_BurningDuration < 0f)
			{
				this.m_BurningDuration = 0f;
			}
			Item item = slot.m_Item;
			slot.RemoveItem(item, false);
			UnityEngine.Object.Destroy(item.gameObject);
			return;
		}
		if (slot == this.m_ItemSlot)
		{
			this.m_Item = slot.m_Item;
			this.m_Item.m_CantSave = true;
			this.SetState((slot.m_Item.m_Info.m_Type == ItemType.Form) ? Forge.State.MakingForm : Forge.State.MakingOre);
		}
	}

	public void OnRemoveItem(ItemSlot slot)
	{
	}

	public override bool ShowAdditionalInfo()
	{
		if (this.m_Burning)
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
		return !flag && !this.m_Burning;
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Req_Additional_Info", true);
	}

	public override string GetIconName()
	{
		if (this.ShowAdditionalInfo())
		{
			return "ember_required";
		}
		return base.GetIconName();
	}

	public override bool CanTrigger()
	{
		if (this.m_Burning)
		{
			return false;
		}
		if (this.m_CantTriggerDuringDialog && DialogsManager.Get().IsAnyDialogPlaying())
		{
			return false;
		}
		if (this.GetProcessProgress(this) <= 0f)
		{
			return false;
		}
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
		return this.ShowAdditionalInfo();
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
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
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
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
			}
		}
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("ForgeState" + index, (int)this.m_State);
		SaveGame.SaveVal("ForgeBurning" + index, this.m_Burning);
		SaveGame.SaveVal("ForgeBurningDur" + index, this.m_BurningDuration);
		SaveGame.SaveVal("ForgeForgingBurningDur" + index, this.m_ForgingDuration);
		SaveGame.SaveVal("ForgeItem" + index, this.m_Item != null);
		if (this.m_Item)
		{
			SaveGame.SaveVal("ForgeItemID" + index, (int)this.m_Item.GetInfoID());
		}
	}

	public override void Load(int index)
	{
		base.Load(index);
		if (SaveGame.LoadBVal("ForgeItem" + index))
		{
			ItemID item_id = (ItemID)SaveGame.LoadIVal("ForgeItemID" + index);
			this.m_ItemSlot.InsertItem(ItemsManager.Get().CreateItem(item_id, true, this.m_ItemSlot.transform));
		}
		this.m_State = Forge.State.None;
		this.SetState((Forge.State)SaveGame.LoadIVal("ForgeState" + index));
		this.m_BurningDuration = SaveGame.LoadFVal("ForgeBurningDur" + index);
		this.m_ForgingDuration = SaveGame.LoadFVal("ForgeForgingBurningDur" + index);
		if (SaveGame.LoadBVal("ForgeBurning" + index))
		{
			this.Ignite();
		}
	}

	public override void DestroyMe(bool check_connected = true)
	{
		base.DestroyMe(check_connected);
		ItemInfo info = ItemsManager.Get().GetInfo(ItemID.Charcoal);
		int num = Mathf.FloorToInt((this.m_BurningLength - this.m_BurningDuration) / info.m_AddForgeBurningTime);
		for (int i = 0; i < num; i++)
		{
			ItemsManager.Get().CreateItem(ItemID.Charcoal, true, base.transform.position + UnityEngine.Random.insideUnitSphere * 0.2f, base.transform.rotation);
		}
		if (this.m_ItemSlot.m_Item)
		{
			HUDProcess.Get().UnregisterProcess(this.m_ItemSlot.m_Item);
			this.m_ItemSlot.RemoveItem();
		}
	}

	public override string GetTriggerInfoLocalized()
	{
		if (!Player.Get().GetCurrentItem(Hand.Right))
		{
			return GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Ember_Required", true);
		}
		return base.GetTriggerInfoLocalized();
	}

	private string GetProcessIconName()
	{
		Forge.State state = this.m_State;
		if (state == Forge.State.MakingOre || state == Forge.State.MakingForm)
		{
			return "forge_melt";
		}
		return "Default_Pickup";
	}

	private Forge.State m_State;

	public GameObject m_Effects;

	public float m_MakingOreLength = 10f;

	public float m_MakingFormLength = 10f;

	private float m_ForgingDuration;

	private Item m_Item;

	public ItemSlot m_ItemSlot;

	public ItemSlot m_CharcoalSlot;

	private bool m_Burning;

	private float m_BurningDuration;

	public float m_BurningLength = 2f;

	private AudioSource m_Sound;

	private enum State
	{
		None,
		WaitingForItem,
		MakingOre,
		Ore,
		MakingForm,
		Form
	}
}
