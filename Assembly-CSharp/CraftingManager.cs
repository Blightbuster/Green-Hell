using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
	public static CraftingManager Get()
	{
		return CraftingManager.s_Instance;
	}

	private void Awake()
	{
		CraftingManager.s_Instance = this;
		this.m_Table = base.transform.Find("Table").gameObject;
		this.m_Table.SetActive(false);
		this.m_TableCollider = this.m_Table.GetComponent<Collider>();
		base.gameObject.SetActive(false);
	}

	private void Start()
	{
		Collider componentDeepChild = General.GetComponentDeepChild<Collider>(Player.Get().gameObject);
		Physics.IgnoreCollision(this.m_TableCollider, componentDeepChild, true);
		if (this.m_AvailableItems.Count == 0)
		{
			this.InitializeAvailableItems();
		}
	}

	private void InitializeAvailableItems()
	{
		Dictionary<int, ItemInfo> allInfos = ItemsManager.Get().GetAllInfos();
		foreach (ItemInfo itemInfo in allInfos.Values)
		{
			if (!itemInfo.IsConstruction() && itemInfo.m_Components.Count > 0)
			{
				this.m_AvailableItems.Add(itemInfo);
			}
		}
	}

	public void Activate()
	{
		if (base.gameObject.activeSelf)
		{
			return;
		}
		if (this.m_AvailableItems.Count == 0)
		{
			this.InitializeAvailableItems();
		}
		this.EnableTable();
		Player.Get().StartController(PlayerControllerType.Crafting);
		base.gameObject.SetActive(true);
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Activate();
		}
	}

	public bool IsActive()
	{
		return base.gameObject && base.gameObject.activeSelf;
	}

	private void EnableTable()
	{
		this.m_Table.SetActive(true);
	}

	public void Deactivate()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			Item item = this.m_Items[i];
			item.StaticPhxRequestRemove();
			item.m_OnCraftingTable = false;
			InventoryBackpack.Get().InsertItem(item, null, null, true, true, true, true, true);
		}
		this.m_Items.Clear();
		this.m_Results.Clear();
		this.m_PossibleResults.Clear();
		this.m_Table.SetActive(false);
		base.gameObject.SetActive(false);
		Player.Get().StopController(PlayerControllerType.Crafting);
		if (Player.Get().m_ControllerToStart != PlayerControllerType.Unknown)
		{
			Player.Get().StartControllerInternal();
		}
		this.UpdateHints();
	}

	public bool ContainsItem(Item item)
	{
		return this.m_Items.Contains(item);
	}

	public void AddItem(Item item, bool setup_pos)
	{
		item.m_OnCraftingTable = true;
		item.StaticPhxRequestAdd();
		this.m_Items.Add(item);
		item.UpdateScale(false);
		item.UpdatePhx();
		if (setup_pos)
		{
			item.gameObject.transform.rotation = Quaternion.identity;
			Vector3 position = this.m_Table.transform.position + item.transform.up * item.m_BoxCollider.size.y * item.transform.localScale.y * 0.5f;
			item.gameObject.transform.position = position;
			Matrix4x4 identity = Matrix4x4.identity;
			Vector3 vector = -this.m_TableCollider.transform.forward;
			identity.SetColumn(1, vector);
			Vector3 v = Vector3.Cross(this.m_TableCollider.transform.up, vector);
			identity.SetColumn(0, v);
			identity.SetColumn(2, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(0)));
			identity.SetColumn(0, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(2)));
			Quaternion rotation = CJTools.Math.QuaternionFromMatrix(identity);
			item.gameObject.transform.rotation = rotation;
			if (item.m_Info.m_InventoryRotated)
			{
				item.transform.RotateAround(item.m_BoxCollider.bounds.center, item.transform.up, 90f);
			}
		}
		this.CheckResult();
		HUDCrafting.Get().Setup();
	}

	public void RemoveItem(Item item)
	{
		item.m_OnCraftingTable = false;
		item.StaticPhxRequestRemove();
		this.m_Items.Remove(item);
		this.CheckResult();
		HUDCrafting.Get().Setup();
	}

	private void CheckResult()
	{
		this.m_Results.Clear();
		this.m_PossibleResults.Clear();
		if (this.m_Items.Count == 0)
		{
			return;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (Item item in this.m_Items)
		{
			ItemID infoID = item.GetInfoID();
			if (dictionary.ContainsKey((int)infoID))
			{
				Dictionary<int, int> dictionary2;
				int key;
				dictionary[(int)infoID] = ((dictionary2 = dictionary)[key = (int)infoID] = dictionary2[key] + 1);
			}
			else
			{
				dictionary.Add((int)infoID, 1);
			}
		}
		foreach (ItemInfo itemInfo in this.m_AvailableItems)
		{
			if (!ItemsManager.Get().m_CraftingLockedItems.Contains(itemInfo.m_ID))
			{
				bool flag = true;
				Dictionary<int, int> components = itemInfo.m_Components;
				using (Dictionary<int, int>.KeyCollection.Enumerator enumerator3 = dictionary.Keys.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						ItemID key2 = (ItemID)enumerator3.Current;
						if (!components.ContainsKey((int)key2) || dictionary[(int)key2] > components[(int)key2])
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					int num = 0;
					using (Dictionary<int, int>.KeyCollection.Enumerator enumerator4 = components.Keys.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							ItemID key3 = (ItemID)enumerator4.Current;
							num += components[(int)key3];
						}
					}
					if (num == this.m_Items.Count)
					{
						this.m_Results.Add(itemInfo);
						this.m_PossibleResults.Add(itemInfo);
					}
					else
					{
						this.m_PossibleResults.Add(itemInfo);
					}
				}
			}
		}
	}

	public void StartCrafting(ItemID item_id)
	{
		this.m_Result = item_id;
		CraftingController.Get().StartCrafting();
	}

	public void Craft()
	{
		if (this.m_Result == ItemID.None)
		{
			DebugUtils.Assert("ERROR - Missing result ItemID! Can't craft item!", true, DebugUtils.AssertType.Info);
			return;
		}
		Item item = this.CreateItem(this.m_Result);
		ItemsManager.Get().OnCrafted(this.m_Result);
		this.m_Result = ItemID.None;
		InventoryBackpack.InsertResult insertResult = InventoryBackpack.Get().InsertItem(item, null, null, true, false, true, true, true);
		if (insertResult != InventoryBackpack.InsertResult.Ok)
		{
			this.AddItem(item, true);
		}
		else
		{
			item.OnTake();
		}
		InventoryBackpack.Get().SetupPocket(item.m_Info.m_BackpackPocket);
		Inventory3DManager.Get().SetNewCraftedItem(item);
		this.CheckResult();
		HUDCrafting.Get().Setup();
	}

	private Item CreateItem(ItemID item_id)
	{
		while (this.m_Items.Count > 0)
		{
			UnityEngine.Object.Destroy(this.m_Items[0].gameObject);
			this.m_Items.RemoveAt(0);
		}
		Item item = ItemsManager.Get().CreateItem(item_id, false, Vector3.zero, Quaternion.identity);
		this.CalcHealth(item);
		Player.Get().AddKnownItem(item_id);
		EventsManager.OnEvent(Enums.Event.Craft, 1, (int)item_id);
		Skill.Get<CraftingSkill>().OnSkillAction();
		ItemsManager.Get().OnCreateItem(item_id);
		return item;
	}

	private void CalcHealth(Item item)
	{
		float playerHealthMul = Skill.Get<CraftingSkill>().GetPlayerHealthMul();
		float itemHealthMul = Skill.Get<CraftingSkill>().GetItemHealthMul(item);
		float num = Mathf.Clamp01(playerHealthMul + itemHealthMul + Skill.Get<CraftingSkill>().m_InitialHealthMul);
		item.m_Info.m_Health = item.m_Info.m_MaxHealth * num;
	}

	private void Update()
	{
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Activate();
		}
		this.UpdateHints();
	}

	private void UpdateHints()
	{
		CraftingManager.CraftingHints craftingHints = CraftingManager.CraftingHints.None;
		if (base.gameObject.activeSelf && !CraftingController.Get().m_InProgress)
		{
			if (this.m_Items.Count == 0)
			{
				craftingHints = CraftingManager.CraftingHints.Empty;
			}
			else if (this.m_PossibleResults.Count > 0)
			{
				craftingHints = CraftingManager.CraftingHints.Combine;
			}
			else
			{
				craftingHints = CraftingManager.CraftingHints.Full;
			}
		}
		if (craftingHints != this.m_CurrentHint)
		{
			if (this.m_CurrentHint != CraftingManager.CraftingHints.None)
			{
				HintsManager.Get().HideHint("Crafting_" + this.m_CurrentHint.ToString());
			}
			this.m_CurrentHint = craftingHints;
			if (this.m_CurrentHint != CraftingManager.CraftingHints.None)
			{
				HintsManager.Get().ShowHint("Crafting_" + this.m_CurrentHint.ToString(), 0f);
			}
		}
	}

	public GameObject m_Table;

	[HideInInspector]
	public Collider m_TableCollider;

	public List<Item> m_Items = new List<Item>();

	private List<ItemInfo> m_AvailableItems = new List<ItemInfo>();

	public List<ItemInfo> m_Results = new List<ItemInfo>();

	public List<ItemInfo> m_PossibleResults = new List<ItemInfo>();

	private ItemID m_Result = ItemID.None;

	private CraftingManager.CraftingHints m_CurrentHint;

	public static CraftingManager s_Instance;

	private enum CraftingHints
	{
		None,
		Empty,
		Combine,
		Full
	}
}
