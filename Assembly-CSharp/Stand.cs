﻿using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Stand : Construction
{
	protected override void Start()
	{
		base.Start();
		this.m_StoredItemId = (ItemID)Enum.Parse(typeof(ItemID), this.m_StoredInfoName);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (ItemsManager.Get().IsHeavyObject(this.m_StoredItemId))
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if (currentItem != null && currentItem.m_Info.m_ID == this.m_StoredItemId && this.m_NumItems < this.m_Vis.Count)
			{
				actions.Add(TriggerAction.TYPE.InsertToStand);
			}
			if ((currentItem == null && this.m_NumItems > 0) || (currentItem != null && (!ItemsManager.Get().IsHeavyObject(currentItem.m_Info.m_ID) || currentItem.m_Info.m_ID == this.m_StoredItemId) && this.m_NumItems > 0))
			{
				actions.Add(TriggerAction.TYPE.Expand);
			}
		}
		else
		{
			if (this.m_NumItems < this.m_Vis.Count && InventoryBackpack.Get().Contains(this.m_StoredItemId))
			{
				actions.Add(TriggerAction.TYPE.InsertToStand);
			}
			if (this.m_NumItems > 0)
			{
				actions.Add(TriggerAction.TYPE.Expand);
			}
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		if (action == TriggerAction.TYPE.InsertToStand)
		{
			this.m_NumItems++;
			this.PlayInsertSound();
			this.UpdateVis();
			if (ItemsManager.Get().IsHeavyObject(this.m_StoredItemId))
			{
				this.RemoveItemFromHand();
			}
			else
			{
				this.RemoveItemFromBackpack();
			}
		}
		else if (action == TriggerAction.TYPE.Expand)
		{
			base.OnExecute(action);
		}
	}

	private void UpdateVis()
	{
		for (int i = 0; i < this.m_Vis.Count; i++)
		{
			if (i < this.m_NumItems)
			{
				this.m_Vis[i].SetActive(true);
			}
			else
			{
				this.m_Vis[i].SetActive(false);
			}
		}
	}

	public override bool CanTrigger()
	{
		if (ItemsManager.Get().IsHeavyObject(this.m_StoredItemId))
		{
			Item currentItem = Player.Get().GetCurrentItem(Hand.Right);
			if ((currentItem == null && this.m_NumItems > 0) || (currentItem != null && (!ItemsManager.Get().IsHeavyObject(currentItem.m_Info.m_ID) || currentItem.m_Info.m_ID == this.m_StoredItemId)))
			{
				return true;
			}
		}
		else
		{
			if (this.m_NumItems < this.m_Vis.Count - 1 && InventoryBackpack.Get().Contains(this.m_StoredItemId))
			{
				return true;
			}
			if (this.m_NumItems > 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool AddItemToBackpack()
	{
		Item item = ItemsManager.Get().CreateItem(this.m_StoredItemId, true, Vector3.zero, Quaternion.identity);
		InventoryBackpack.InsertResult insertResult = InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		bool flag = InventoryBackpack.InsertResult.Ok == insertResult;
		if (!flag)
		{
			UnityEngine.Object.Destroy(item.gameObject);
			this.m_NumItems++;
			this.UpdateVis();
		}
		else
		{
			this.PlayInsertSound();
			base.AddItemsCountMessage(item);
		}
		return flag;
	}

	private void RemoveItemFromBackpack()
	{
		Item item = InventoryBackpack.Get().FindItem(this.m_StoredItemId);
		if (item == null)
		{
			return;
		}
		if (CraftingManager.Get().gameObject.activeSelf && CraftingManager.Get().ContainsItem(item))
		{
			CraftingManager.Get().RemoveItem(item);
		}
		if (!item.m_CurrentSlot && item.m_InventorySlot && item.m_InventorySlot.m_Items.Count > 0)
		{
			item.m_InventorySlot.RemoveItem(item, false);
		}
		else if (item.m_CurrentSlot && item.m_CurrentSlot.m_InventoryStackSlot)
		{
			item.m_CurrentSlot.RemoveItem(item, false);
		}
		if (InventoryBackpack.Get().m_EquippedItem == item)
		{
			InventoryBackpack.Get().m_EquippedItem = null;
		}
		InventoryBackpack.Get().RemoveItem(item, false);
		UnityEngine.Object.Destroy(item.gameObject);
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("NumItems" + index, this.m_NumItems);
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_NumItems = SaveGame.LoadIVal("NumItems" + index);
	}

	public override void SetupAfterLoad(int index)
	{
		base.SetupAfterLoad(index);
		this.UpdateVis();
	}

	private void RemoveItemFromHand()
	{
		HeavyObject heavyObject = (HeavyObject)Player.Get().GetCurrentItem(Hand.Right);
		if (heavyObject.m_Attached.Count > 0)
		{
			heavyObject.DetachHeavyObject(heavyObject.m_Attached.Count - 1, true);
		}
		else
		{
			Player.Get().DropItem(heavyObject);
			UnityEngine.Object.Destroy(heavyObject.gameObject);
		}
	}

	private bool AddItemToHand()
	{
		HeavyObject heavyObject = (HeavyObject)ItemsManager.Get().CreateItem(this.m_StoredItemId, false, Vector3.zero, Quaternion.identity);
		bool flag = heavyObject.Take();
		if (!flag)
		{
			UnityEngine.Object.Destroy(heavyObject.gameObject);
			this.m_NumItems++;
			this.UpdateVis();
		}
		else
		{
			this.PlayInsertSound();
		}
		return flag;
	}

	public int GetNumitems()
	{
		return this.m_NumItems;
	}

	public override bool Take()
	{
		this.m_NumItems--;
		this.UpdateVis();
		if (ItemsManager.Get().IsHeavyObject(this.m_StoredItemId))
		{
			this.AddItemToHand();
		}
		else
		{
			this.AddItemToBackpack();
		}
		return true;
	}

	public override bool Take3()
	{
		for (int i = 0; i < 3; i++)
		{
			this.m_NumItems--;
			this.UpdateVis();
			bool flag;
			if (ItemsManager.Get().IsHeavyObject(this.m_StoredItemId))
			{
				flag = this.AddItemToHand();
			}
			else
			{
				flag = this.AddItemToBackpack();
			}
			if (!flag)
			{
				break;
			}
		}
		return true;
	}

	public override bool TakeAll()
	{
		while (this.m_NumItems > 0)
		{
			this.m_NumItems--;
			this.UpdateVis();
			bool flag;
			if (ItemsManager.Get().IsHeavyObject(this.m_StoredItemId))
			{
				flag = this.AddItemToHand();
			}
			else
			{
				flag = this.AddItemToBackpack();
			}
			if (!flag)
			{
				break;
			}
		}
		return true;
	}

	public override string GetTriggerInfoLocalized()
	{
		return string.Concat(new object[]
		{
			GreenHellGame.Instance.GetLocalization().Get(this.GetName()),
			" ",
			this.m_NumItems.ToString(),
			" / ",
			this.m_Vis.Count
		});
	}

	public override void DestroyMe()
	{
		base.DestroyMe();
		for (int i = 0; i < this.m_NumItems; i++)
		{
			ItemsManager.Get().CreateItem(this.m_StoredItemId, true, this.m_Vis[i].transform);
		}
	}

	private void PlayInsertSound()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		AudioClip audioClip;
		if (this.m_StoredItemId == ItemID.Log)
		{
			audioClip = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_log_0" + UnityEngine.Random.Range(1, 6).ToString());
		}
		else if (this.m_StoredItemId == ItemID.Long_Stick)
		{
			audioClip = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_long_stick_0" + UnityEngine.Random.Range(1, 3).ToString());
		}
		else
		{
			audioClip = (AudioClip)Resources.Load("Sounds/Constructions/construction_insert_branches_0" + UnityEngine.Random.Range(1, 7).ToString());
		}
		if (audioClip != null)
		{
			this.m_AudioSource.clip = audioClip;
			this.m_AudioSource.Play();
		}
	}

	public string m_StoredInfoName = string.Empty;

	private ItemID m_StoredItemId = ItemID.None;

	private int m_NumItems;

	public List<GameObject> m_Vis = new List<GameObject>();

	private AudioSource m_AudioSource;
}
