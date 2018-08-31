using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDCrafting : HUDBase
{
	public static HUDCrafting Get()
	{
		return HUDCrafting.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDCrafting.s_Instance = this;
		for (int i = 0; i < 2147483647; i++)
		{
			Transform transform = base.transform.Find("Slot" + i);
			if (!transform)
			{
				break;
			}
			this.m_Slots.Add(transform.GetComponent<RawImage>());
		}
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return CraftingManager.Get().gameObject.activeSelf && Inventory3DManager.Get().m_InventoryImage.enabled;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.Setup();
	}

	public void OnButton(int id)
	{
		CraftingManager.Get().StartCrafting(CraftingManager.Get().m_Results[id].m_ID);
	}

	public void Setup()
	{
		this.SetupInfo();
		this.SetupSlots();
	}

	private void SetupSlots()
	{
		int num = 0;
		for (int i = 0; i < CraftingManager.Get().m_PossibleResults.Count; i++)
		{
			int num2 = 0;
			using (Dictionary<int, int>.KeyCollection.Enumerator enumerator = CraftingManager.Get().m_PossibleResults[i].m_Components.Keys.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemID key = (ItemID)enumerator.Current;
					num2 += CraftingManager.Get().m_PossibleResults[i].m_Components[(int)key];
				}
			}
			num = Mathf.Max(num, num2);
		}
		if (num >= this.m_Slots.Count)
		{
			DebugUtils.Assert("Add more crafting slots!!!", true, DebugUtils.AssertType.Info);
		}
		int count = CraftingManager.Get().m_Items.Count;
		for (int j = 0; j < this.m_Slots.Count; j++)
		{
			if (j < num)
			{
				this.m_Slots[j].gameObject.SetActive(true);
				if (j < count)
				{
					this.m_Slots[j].texture = this.m_IconOccupied;
				}
				else
				{
					this.m_Slots[j].texture = this.m_IconFree;
				}
			}
			else
			{
				this.m_Slots[j].gameObject.SetActive(false);
			}
		}
	}

	private void SetupInfo()
	{
		if (CraftingManager.Get().m_Results.Count == 0)
		{
			this.m_CraftButton.gameObject.SetActive(false);
			this.m_Icon.gameObject.SetActive(false);
			this.m_Text.gameObject.SetActive(false);
			return;
		}
		ItemInfo itemInfo = CraftingManager.Get().m_Results[0];
		string text = itemInfo.m_IconName;
		if (text.Empty())
		{
			text = "Default_Pickup";
		}
		Sprite sprite = null;
		ItemsManager.Get().m_ItemIconsSprites.TryGetValue(text, out sprite);
		this.m_Icon.sprite = sprite;
		this.m_Text.text = GreenHellGame.Instance.GetLocalization().Get(itemInfo.m_ID.ToString());
		this.m_CraftButton.gameObject.SetActive(true);
		this.m_Icon.gameObject.SetActive(true);
		this.m_Text.gameObject.SetActive(true);
	}

	protected override void Update()
	{
	}

	public GameObject m_CraftButton;

	public Image m_Icon;

	public Text m_Text;

	public Texture m_IconFree;

	public Texture m_IconOccupied;

	private List<RawImage> m_Slots = new List<RawImage>();

	private static HUDCrafting s_Instance;
}
