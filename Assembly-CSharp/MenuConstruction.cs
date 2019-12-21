using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuConstruction : MenuScreen
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Selection = base.transform.Find("IconSelection").GetComponent<RawImage>();
		for (int i = 0; i < 2147483647; i++)
		{
			Transform transform = base.transform.Find("Slot" + i);
			if (!transform)
			{
				break;
			}
			MenuConstructionSlot menuConstructionSlot = new MenuConstructionSlot();
			menuConstructionSlot.parent = transform.gameObject;
			menuConstructionSlot.name = transform.Find("Name").GetComponent<Text>();
			menuConstructionSlot.icon = transform.Find("Icon").GetComponent<RawImage>();
			menuConstructionSlot.components = transform.Find("Components").GetComponent<Text>();
			menuConstructionSlot.info = null;
			this.m_Slots.Add(menuConstructionSlot);
		}
		List<ItemInfo> allInfosOfType = ItemsManager.Get().GetAllInfosOfType(ItemType.Construction);
		for (int j = 0; j < allInfosOfType.Count; j++)
		{
			this.m_Infos.Add((ConstructionInfo)allInfosOfType[j]);
		}
		List<ItemInfo> allInfosOfType2 = ItemsManager.Get().GetAllInfosOfType(ItemType.Trap);
		for (int k = 0; k < allInfosOfType2.Count; k++)
		{
			this.m_Infos.Add((ConstructionInfo)allInfosOfType2[k]);
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		this.Setup();
	}

	private void Setup()
	{
		foreach (MenuConstructionSlot menuConstructionSlot in this.m_Slots)
		{
			menuConstructionSlot.parent.SetActive(false);
		}
		this.m_ActiveSlots.Clear();
		int num = 0;
		foreach (ConstructionInfo constructionInfo in this.m_Infos)
		{
			if (constructionInfo.m_ConstructionType == this.m_ConstructionType)
			{
				if (num >= this.m_Slots.Count)
				{
					DebugUtils.Assert("[MenuConstruction:Setup] Not enough slots for constructions - " + this.m_ConstructionType.ToString(), true, DebugUtils.AssertType.Info);
					break;
				}
				string name = constructionInfo.m_ID.ToString() + "Ghost";
				ConstructionGhost component = GreenHellGame.Instance.GetPrefab(name).GetComponent<ConstructionGhost>();
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				foreach (GhostStep ghostStep in component.m_Steps)
				{
					foreach (GhostSlot ghostSlot in ghostStep.m_Slots)
					{
						if (dictionary.ContainsKey(ghostSlot.m_ItemName))
						{
							Dictionary<string, int> dictionary2 = dictionary;
							string itemName = ghostSlot.m_ItemName;
							int value = dictionary2[itemName] + 1;
							dictionary2[itemName] = value;
						}
						else
						{
							dictionary.Add(ghostSlot.m_ItemName, 1);
						}
					}
				}
				MenuConstructionSlot menuConstructionSlot2 = this.m_Slots[num];
				num++;
				menuConstructionSlot2.parent.SetActive(true);
				menuConstructionSlot2.name.text = constructionInfo.m_ID.ToString();
				menuConstructionSlot2.name.color = this.m_NormalColor;
				menuConstructionSlot2.info = constructionInfo;
				menuConstructionSlot2.components.text = string.Empty;
				menuConstructionSlot2.components.color = this.m_NormalColor;
				foreach (string text in dictionary.Keys)
				{
					Text components = menuConstructionSlot2.components;
					components.text = string.Concat(new object[]
					{
						components.text,
						text,
						"*",
						dictionary[text],
						", "
					});
				}
				this.m_ActiveSlots.Add(menuConstructionSlot2);
			}
		}
	}

	public void OnSlotPress(int slot_index)
	{
		MenuConstructionSlot menuConstructionSlot = this.m_Slots[slot_index];
		Player.Get().GetComponent<ConstructionController>().SetupPrefab(menuConstructionSlot.info);
		Player.Get().StartController(PlayerControllerType.Construction);
		this.m_MenuInGameManager.HideMenu();
	}

	public void SetConstruction(int index)
	{
		this.m_ConstructionType = (ConstructionType)index;
		this.Setup();
	}

	public void SelectIcon(RawImage icon)
	{
		this.m_Selection.rectTransform.position = icon.rectTransform.position;
	}

	public void Close()
	{
		this.m_MenuInGameManager.HideMenu();
	}

	public void FocusSlot(int index)
	{
		for (int i = 0; i < this.m_ActiveSlots.Count; i++)
		{
			MenuConstructionSlot menuConstructionSlot = this.m_ActiveSlots[i];
			menuConstructionSlot.name.color = ((i == index) ? this.m_FocusColor : this.m_NormalColor);
			menuConstructionSlot.components.color = ((i == index) ? this.m_FocusColor : this.m_NormalColor);
		}
	}

	private List<ConstructionInfo> m_Infos = new List<ConstructionInfo>();

	private List<MenuConstructionSlot> m_Slots = new List<MenuConstructionSlot>();

	private List<MenuConstructionSlot> m_ActiveSlots = new List<MenuConstructionSlot>();

	private ConstructionType m_ConstructionType;

	private RawImage m_Selection;

	public Color m_NormalColor = Color.white;

	public Color m_FocusColor = Color.white;
}
