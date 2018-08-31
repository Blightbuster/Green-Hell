using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ConstructionsTab : ItemsTab, IInputsReceiver
{
	private void Start()
	{
		InputsManager.Get().RegisterReceiver(this);
	}

	public override void OnShow()
	{
		base.OnShow();
		NotepadItemMask[] componentsInChildren = base.GetComponentsInChildren<NotepadItemMask>(true);
		this.m_Masks.Clear();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.m_Masks.Add(componentsInChildren[i]);
		}
		this.m_CanvasRecTransform = base.gameObject.transform.parent.GetComponent<RectTransform>();
	}

	private void Update()
	{
		this.UpdateMasksUnderCursor();
	}

	public bool CanReceiveAction()
	{
		return NotepadController.Get().IsActive() && MenuNotepad.Get().m_ActiveTab == this.m_Tab;
	}

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (action == InputsManager.InputAction.LMB)
		{
			for (int i = 0; i < this.m_Masks.Count; i++)
			{
				if (this.m_Masks[i].gameObject.activeSelf)
				{
					ConstructionController component = Player.Get().GetComponent<ConstructionController>();
					if (!component.IsActive())
					{
						string itemID = this.m_Masks[i].gameObject.transform.parent.GetComponent<NotepadConstructionData>().m_ItemID;
						ItemID id = (ItemID)Enum.Parse(typeof(ItemID), itemID);
						ItemInfo info = ItemsManager.Get().GetInfo(id);
						component.SetupPrefab(info);
						Player.Get().StartController(PlayerControllerType.Construction);
					}
					break;
				}
			}
		}
	}

	private void UpdateMasksUnderCursor()
	{
		Vector2 notepadCanvasCursorPos = MenuNotepad.Get().m_NotepadCanvasCursorPos;
		notepadCanvasCursorPos.x *= this.m_CanvasRecTransform.rect.width;
		notepadCanvasCursorPos.y *= this.m_CanvasRecTransform.rect.height;
		notepadCanvasCursorPos.x -= this.m_CanvasRecTransform.rect.width * 0.5f;
		notepadCanvasCursorPos.y -= this.m_CanvasRecTransform.rect.height * 0.5f;
		notepadCanvasCursorPos.x *= this.m_CanvasRecTransform.localScale.x;
		notepadCanvasCursorPos.y *= this.m_CanvasRecTransform.localScale.y;
		bool mouseOverConstruction = false;
		for (int i = 0; i < this.m_Masks.Count; i++)
		{
			RectTransform component = this.m_Masks[i].gameObject.GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, notepadCanvasCursorPos) && this.m_Masks[i].gameObject.transform.parent.gameObject.activeSelf)
			{
				this.m_Masks[i].gameObject.SetActive(true);
				mouseOverConstruction = true;
			}
			else
			{
				this.m_Masks[i].gameObject.SetActive(false);
			}
		}
		MenuNotepad.Get().m_MouseOverConstruction = mouseOverConstruction;
	}

	private List<NotepadItemMask> m_Masks = new List<NotepadItemMask>();

	private RectTransform m_CanvasRecTransform;
}
