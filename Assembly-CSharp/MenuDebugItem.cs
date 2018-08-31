using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuDebugItem : MenuScreen
{
	protected override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
		this.m_Field.text = string.Empty;
		EventSystem.current.SetSelectedGameObject(this.m_Field.gameObject, null);
		this.m_Field.OnPointerClick(new PointerEventData(EventSystem.current));
		this.m_LastField = string.Empty;
		this.Setup();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (!ItemsManager.Get())
		{
			return;
		}
		if (this.m_List.GetSelectionIndex() < 0 || this.m_List.GetSelectionIndex() >= this.m_Items.Count)
		{
			ItemsManager.Get().m_DebugSpawnID = ItemID.None;
		}
		else
		{
			string selectedElementText = this.m_List.GetSelectedElementText();
			if (selectedElementText != string.Empty)
			{
				ItemsManager.Get().m_DebugSpawnID = (ItemID)Enum.Parse(typeof(ItemID), selectedElementText);
			}
		}
	}

	private void Setup()
	{
		if (!this.m_List)
		{
			return;
		}
		this.m_List.Clear();
		this.m_Items = ItemsManager.Get().GetAllInfos();
		using (Dictionary<int, ItemInfo>.KeyCollection.Enumerator enumerator = this.m_Items.Keys.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ItemID itemID = (ItemID)enumerator.Current;
				string text = string.Empty;
				text += itemID.ToString();
				if (text.ToLower().Contains(this.m_LastField))
				{
					this.m_List.AddElement(text, -1);
				}
			}
		}
		this.m_List.SortAlphabetically();
		this.m_List.SetSelectionIndex(0);
	}

	protected override void Update()
	{
		if (this.m_LastField != this.m_Field.text)
		{
			this.m_LastField = this.m_Field.text.ToLower();
			this.Setup();
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			this.OnClose();
		}
	}

	public UIList m_List;

	public InputField m_Field;

	private Dictionary<int, ItemInfo> m_Items = new Dictionary<int, ItemInfo>();

	private string m_LastField = string.Empty;
}
