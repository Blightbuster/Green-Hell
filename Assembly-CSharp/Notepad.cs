using System;
using System.Collections.Generic;
using System.Linq;
using CJTools;
using Enums;
using UnityEngine;

public class Notepad : MonoBehaviour
{
	private void Awake()
	{
		this.m_PrevPageObject = base.gameObject.transform.FindDeepChild("previous_page").gameObject;
		this.m_NextPageObject = base.gameObject.transform.FindDeepChild("next_page").gameObject;
		if (GreenHellGame.TWITCH_DEMO || GreenHellGame.Instance.m_GameMode == GameMode.Survival)
		{
			this.m_StoryTabCollider.gameObject.SetActive(false);
			this.m_PlannerTabCollider.gameObject.SetActive(false);
		}
		NotepadObjectTab notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.StoryTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_StoryTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_StoryTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.StoryTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.ItemsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_ItemsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_ItemsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.ItemsTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.ConstructionsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_ConstructionsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_ConstructionsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.ConstructionsTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.TrapsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_TrapsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_TrapsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.TrapsTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.PlannerTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_PlannerTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_PlannerTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.PlannerTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.FirecampTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_FirecampTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_FirecampTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.FirecampTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.WaterConstructionsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_WaterConstructionsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_WaterConstructionsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.WaterConstructionsTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.HealingItemsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_HealingItemsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_HealingItemsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.HealingItemsTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.SkillsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_SkillsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_SkillsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.SkillsTab] = notepadObjectTab;
		notepadObjectTab = new NotepadObjectTab();
		notepadObjectTab.m_MenuTab = MenuNotepad.MenuNotepadTab.PlantsTab;
		notepadObjectTab.m_GameObjectOn = this.GetOnObject(this.m_PlantsTabCollider.gameObject.transform.parent.gameObject);
		notepadObjectTab.m_GameObjectOff = this.GetOffObject(this.m_PlantsTabCollider.gameObject.transform.parent.gameObject);
		this.m_ObjetcTabs[MenuNotepad.MenuNotepadTab.PlantsTab] = notepadObjectTab;
	}

	private GameObject GetOnObject(GameObject parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			if (parent.transform.GetChild(i).gameObject.name.EndsWith("_on"))
			{
				return parent.transform.GetChild(i).gameObject;
			}
		}
		return null;
	}

	private GameObject GetOffObject(GameObject parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			if (parent.transform.GetChild(i).gameObject.name.EndsWith("_off"))
			{
				return parent.transform.GetChild(i).gameObject;
			}
		}
		return null;
	}

	public void EnablePrevPage(bool enable)
	{
		this.m_PrevPageObject.SetActive(enable);
	}

	public void EnableNextPage(bool enable)
	{
		this.m_NextPageObject.SetActive(enable);
	}

	public void SetActiveTab(MenuNotepad.MenuNotepadTab tab)
	{
		NotepadObjectTab notepadObjectTab = null;
		if (!this.m_ObjetcTabs.TryGetValue(tab, out notepadObjectTab))
		{
			return;
		}
		for (int i = 0; i < this.m_ObjetcTabs.Count; i++)
		{
			if ((!GreenHellGame.TWITCH_DEMO && GreenHellGame.Instance.m_GameMode != GameMode.Survival) || (this.m_ObjetcTabs.Values.ElementAt(i).m_MenuTab != MenuNotepad.MenuNotepadTab.StoryTab && this.m_ObjetcTabs.Values.ElementAt(i).m_MenuTab != MenuNotepad.MenuNotepadTab.PlannerTab))
			{
				if (this.m_ObjetcTabs.Keys.ElementAt(i) == tab)
				{
					this.m_ObjetcTabs.Values.ElementAt(i).m_GameObjectOn.SetActive(true);
					this.m_ObjetcTabs.Values.ElementAt(i).m_GameObjectOff.SetActive(false);
				}
				else
				{
					this.m_ObjetcTabs.Values.ElementAt(i).m_GameObjectOn.SetActive(false);
					this.m_ObjetcTabs.Values.ElementAt(i).m_GameObjectOff.SetActive(true);
				}
			}
		}
	}

	public Collider m_StoryTabCollider;

	public Collider m_SkillsTabCollider;

	public Collider m_ItemsTabCollider;

	public Collider m_ConstructionsTabCollider;

	public Collider m_TrapsTabCollider;

	public Collider m_PlannerTabCollider;

	public Collider m_FirecampTabCollider;

	public Collider m_WaterConstructionsTabCollider;

	public Collider m_HealingItemsTabCollider;

	public Collider m_PlantsTabCollider;

	private Dictionary<MenuNotepad.MenuNotepadTab, NotepadObjectTab> m_ObjetcTabs = new Dictionary<MenuNotepad.MenuNotepadTab, NotepadObjectTab>();

	public Collider m_PrevPage;

	public Collider m_NextPage;

	private GameObject m_PrevPageObject;

	private GameObject m_NextPageObject;
}
