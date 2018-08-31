using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuDebugDialogs : MenuScreen
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

	private void Setup()
	{
		if (!this.m_List)
		{
			return;
		}
		this.m_List.Clear();
		foreach (string text in DialogsManager.Get().m_Dialogs.Keys)
		{
			if (text.ToLower().Contains(this.m_LastField))
			{
				this.m_List.AddElement(text, -1);
			}
		}
		this.m_List.SortAlphabetically();
		this.m_List.SetSelectionIndex(0);
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (!DialogsManager.Get())
		{
			return;
		}
		if (this.m_List.GetSelectionIndex() < 0 || this.m_List.GetSelectionIndex() >= DialogsManager.Get().m_Dialogs.Count)
		{
			DialogsManager.Get().m_DebugDialogName = string.Empty;
		}
		else
		{
			string selectedElementText = this.m_List.GetSelectedElementText();
			if (selectedElementText != string.Empty)
			{
				DialogsManager.Get().m_DebugDialogName = selectedElementText;
			}
		}
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

	private string m_LastField = string.Empty;
}
