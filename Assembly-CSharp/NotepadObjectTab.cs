using System;
using UnityEngine;

internal class NotepadObjectTab
{
	public void On()
	{
		this.m_GameObjectOn.SetActive(true);
		this.m_GameObjectOff.SetActive(false);
	}

	public void Off()
	{
		this.m_GameObjectOn.SetActive(false);
		this.m_GameObjectOff.SetActive(true);
	}

	public MenuNotepad.MenuNotepadTab m_MenuTab = MenuNotepad.MenuNotepadTab.None;

	public GameObject m_GameObjectOn;

	public GameObject m_GameObjectOff;
}
