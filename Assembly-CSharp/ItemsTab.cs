﻿using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class ItemsTab : NotepadTab
{
	public override void Init()
	{
		base.Init();
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
			this.m_ItemsElelements.Add(gameObject);
			NotepadData component = gameObject.GetComponent<NotepadData>();
			if (component)
			{
				component.Init();
			}
		}
		int num = 0;
		for (int j = 0; j < this.m_ItemsElelements.Count; j++)
		{
			Text[] componentsDeepChild = General.GetComponentsDeepChild<Text>(this.m_ItemsElelements[j]);
			for (int k = 0; k < componentsDeepChild.Length; k++)
			{
				componentsDeepChild[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsDeepChild[k].name, true);
			}
			PageNum component2 = this.m_ItemsElelements[j].GetComponent<PageNum>();
			if (component2.m_PageNum + 1 > num)
			{
				num = component2.m_PageNum + 1;
			}
		}
		this.m_NumActivePages = num * 2;
	}

	private void OnEnable()
	{
		if (this.m_LastUnlockedItem != ItemID.None)
		{
			this.ShowPageForLastUnlockedItem();
		}
		this.UpdateActivePage();
		MenuNotepad.Get().UpdatePrevNextButtons();
	}

	private void OnDisable()
	{
		this.m_LastUnlockedItem = ItemID.None;
	}

	private void UpdateActivePage()
	{
		int num = 0;
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			PageNum component = this.m_ItemsElelements[i].GetComponent<PageNum>();
			NotepadData component2 = this.m_ItemsElelements[i].gameObject.GetComponent<NotepadData>();
			if (component.m_PageNum == this.m_CurrentPage && (component2 == null || component2.ShouldShow()))
			{
				this.m_ItemsElelements[i].SetActive(true);
				if (component2 != null)
				{
					component2.m_WasActive = true;
				}
				num++;
			}
			else
			{
				this.m_ItemsElelements[i].SetActive(false);
			}
		}
		this.m_NumActiveElementsOnPage = num;
	}

	public override void UpdatePages()
	{
		this.UpdateActivePage();
	}

	public void SetCurrentPageToItem(ItemID id)
	{
		this.m_LastUnlockedItem = id;
	}

	private void ShowPageForLastUnlockedItem()
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			PageNum component = this.m_ItemsElelements[i].GetComponent<PageNum>();
			NotepadItemData component2 = this.m_ItemsElelements[i].gameObject.GetComponent<NotepadItemData>();
			if (!(component == null) && !(component2 == null) && component2.m_ItemID == this.m_LastUnlockedItem.ToString())
			{
				this.m_CurrentPage = component.m_PageNum;
			}
		}
	}

	public override bool ShouldShowNoEntries()
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			NotepadData component = this.m_ItemsElelements[i].gameObject.GetComponent<NotepadData>();
			if (component == null || component.ShouldShow())
			{
				return false;
			}
		}
		return true;
	}

	public override int GetNewEntriesCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			NotepadData component = this.m_ItemsElelements[i].gameObject.GetComponent<NotepadData>();
			if (component != null && component.ShouldShow() && !component.m_WasActive)
			{
				num++;
			}
		}
		return num;
	}

	public override void Save(string name)
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			this.m_ItemsElelements[i].GetComponent<NotepadData>().Save(name + i);
		}
	}

	public override void Load(string name)
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			this.m_ItemsElelements[i].GetComponent<NotepadData>().Load(name + i);
		}
	}

	private List<GameObject> m_ItemsElelements = new List<GameObject>();

	private ItemID m_LastUnlockedItem = ItemID.None;
}
