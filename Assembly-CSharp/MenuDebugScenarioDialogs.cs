using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuDebugScenarioDialogs : MenuDebugScreen
{
	public override void OnShow()
	{
		base.OnShow();
		this.m_List.SetFocus(true);
		this.m_Field.text = string.Empty;
		EventSystem.current.SetSelectedGameObject(this.m_Field.gameObject, null);
		this.m_Field.OnPointerClick(new PointerEventData(EventSystem.current));
		this.m_LastField = string.Empty;
		this.Setup();
	}

	public override void OnHide()
	{
		base.OnHide();
		if (!DialogsManager.Get())
		{
			return;
		}
		if (this.m_List.GetSelectionIndex() > 0)
		{
			string selectedElementText = this.m_List.GetSelectedElementText();
			if (selectedElementText != string.Empty)
			{
				DialogsManager.Get().StartDialog(selectedElementText);
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
		this.m_List.AddElement("NONE", -1);
		foreach (string element in DialogsManager.Get().m_ScenarioDialogs.Keys)
		{
			this.m_List.AddElement(element, -1);
		}
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

	private string m_LastField = string.Empty;
}
