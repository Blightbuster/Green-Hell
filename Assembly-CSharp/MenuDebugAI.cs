using System;
using AIs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuDebugAI : MenuScreen
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
		for (int i = 0; i < 39; i++)
		{
			AI.AIID aiid = (AI.AIID)i;
			string text = aiid.ToString();
			if (text.ToLower().Contains(this.m_LastField))
			{
				this.m_List.AddElement(aiid.ToString(), -1);
			}
		}
		this.m_List.SortAlphabetically();
		this.m_List.SetSelectionIndex(0);
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (!ItemsManager.Get())
		{
			return;
		}
		if (this.m_List.GetSelectionIndex() < 0 || this.m_List.GetSelectionIndex() >= 39)
		{
			AIManager.Get().m_DebugSpawnID = AI.AIID.None;
		}
		else
		{
			string selectedElementText = this.m_List.GetSelectedElementText();
			if (selectedElementText != string.Empty)
			{
				AIManager.Get().m_DebugSpawnID = (AI.AIID)Enum.Parse(typeof(AI.AIID), selectedElementText);
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
