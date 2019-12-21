using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadSaveGameMenuCommon : MenuScreen
{
	public static bool IsAnySave()
	{
		for (int i = 0; i < LoadSaveGameMenuCommon.s_MaxSlots; i++)
		{
			if (GreenHellGame.Instance.FileExistsInRemoteStorage(SaveGame.SLOT_NAME + i.ToString() + ".sav") || GreenHellGame.Instance.FileExistsInRemoteStorage(SaveGame.OLD_SLOT_NAME + i.ToString() + ".sav"))
			{
				return true;
			}
		}
		return false;
	}

	public override void OnShow()
	{
		base.OnShow();
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			current.SetSelectedGameObject(null);
		}
		this.m_BackButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("Menu_Back", true);
		this.FillSlots();
		if (GreenHellGame.IsPadControllerActive())
		{
			this.m_Slots[0].Select();
			this.m_Slots[0].OnSelect(null);
		}
	}

	public override void OnHide()
	{
		base.OnHide();
		foreach (SaveGameMenuSlot saveGameMenuSlot in this.m_Slots)
		{
			saveGameMenuSlot.ReleseScreenshot();
		}
	}

	private void FillSlots()
	{
		this.m_Slots.Clear();
		for (int i = 0; i < LoadSaveGameMenuCommon.s_MaxSlots; i++)
		{
			SaveGameMenuSlot component = base.transform.FindDeepChild("Slot" + i.ToString()).GetComponent<SaveGameMenuSlot>();
			DebugUtils.Assert(component, true);
			this.m_Slots.Add(component);
		}
		for (int j = 0; j < LoadSaveGameMenuCommon.s_MaxSlots; j++)
		{
			this.m_Slots[j].SetSaveInfo(SaveGameInfo.ReadSaveSlot(j));
		}
	}

	public override void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.LSBackward || action_data.m_Action == InputsManager.InputAction.DPadDown)
		{
			for (int i = 0; i < this.m_Slots.Count; i++)
			{
				if ((!this.IsLoadGameMenu() || this.m_Slots[i].m_SaveInfo.loadable) && this.m_Slots[i].IsHighlighted())
				{
					for (int j = i + 1; j < this.m_Slots.Count; j++)
					{
						if (!this.IsLoadGameMenu() || this.m_Slots[j].m_SaveInfo.loadable)
						{
							this.m_Slots[j].Select();
							this.m_Slots[j].OnSelect(null);
							return;
						}
					}
					return;
				}
			}
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.LSForward || action_data.m_Action == InputsManager.InputAction.DPadUp)
		{
			for (int k = 0; k < this.m_Slots.Count; k++)
			{
				if ((!this.IsLoadGameMenu() || this.m_Slots[k].m_SaveInfo.loadable) && this.m_Slots[k].IsHighlighted())
				{
					for (int l = k - 1; l >= 0; l--)
					{
						if (!this.IsLoadGameMenu() || this.m_Slots[l].m_SaveInfo.loadable)
						{
							this.m_Slots[l].Select();
							this.m_Slots[l].OnSelect(null);
							break;
						}
					}
				}
			}
			return;
		}
		if (action_data.m_Action == InputsManager.InputAction.Button_A)
		{
			this.CheckSelection();
			return;
		}
		base.OnInputAction(action_data);
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateSlots();
		if (!GreenHellGame.IsYesNoDialogActive())
		{
			CursorManager.Get().SetCursor(((this.m_ActiveMenuOption != null && this.m_ActiveMenuOption.m_Button != null) || this.m_HLSlotIdx >= 0) ? CursorManager.TYPE.MouseOver : CursorManager.TYPE.Normal);
		}
		if (GreenHellGame.IsPadControllerActive())
		{
			CursorManager.Get().SetCursorPos(Vector2.zero);
		}
	}

	protected virtual bool IsLoadGameMenu()
	{
		return false;
	}

	private void UpdateSlots()
	{
		if (GreenHellGame.GetYesNoDialog().gameObject.activeSelf)
		{
			return;
		}
		if (GreenHellGame.IsPCControllerActive())
		{
			this.CheckSelection();
		}
	}

	private void CheckSelection()
	{
		EventSystem current = EventSystem.current;
		GameObject x = (current != null) ? current.currentSelectedGameObject : null;
		this.m_HLSlotIdx = -1;
		for (int i = 0; i < this.m_Slots.Count; i++)
		{
			if (!this.IsLoadGameMenu() || this.m_Slots[i].m_SaveInfo.loadable)
			{
				if (this.m_Slots[i].IsHighlighted())
				{
					this.m_HLSlotIdx = i;
				}
				if (x == this.m_Slots[i].gameObject)
				{
					this.OnSlotSelected(i);
				}
			}
		}
	}

	protected virtual void OnSlotSelected(int slot_idx)
	{
	}

	protected void EnableSlots(bool enable)
	{
		foreach (SaveGameMenuSlot saveGameMenuSlot in this.m_Slots)
		{
			if (saveGameMenuSlot)
			{
				saveGameMenuSlot.enabled = enable;
			}
		}
	}

	protected string GetSelectedSlotFileName()
	{
		if (this.m_SlotIdx >= 0 && this.m_SlotIdx < this.m_Slots.Count)
		{
			SaveGameMenuSlot saveGameMenuSlot = this.m_Slots[this.m_SlotIdx];
			return ((saveGameMenuSlot != null) ? saveGameMenuSlot.m_SaveInfo.file_name : null) ?? this.GetSlotSaveName(this.m_SlotIdx);
		}
		DebugUtils.Assert("Invalid slot idex to load", true, DebugUtils.AssertType.Info);
		return string.Empty;
	}

	protected string GetSlotSaveName(int slot_idx)
	{
		return SaveGame.SLOT_NAME + slot_idx.ToString() + ".sav";
	}

	public static int s_MaxSlots = 4;

	public Button m_BackButton;

	protected List<SaveGameMenuSlot> m_Slots = new List<SaveGameMenuSlot>();

	protected int m_SlotIdx = -1;

	private int m_HLSlotIdx = -1;
}
