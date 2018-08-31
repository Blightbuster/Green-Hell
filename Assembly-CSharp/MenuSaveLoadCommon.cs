using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuSaveLoadCommon : MenuScreen
{
	public static bool IsAnySave()
	{
		for (int i = 0; i < MenuSaveLoadCommon.s_MaxSlots; i++)
		{
			string str = "Slot" + i.ToString() + ".sav";
			if (File.Exists(Application.persistentDataPath + "/" + str))
			{
				return true;
			}
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_BackButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("Menu_Back");
		this.FillSlots();
	}

	private void FillSlots()
	{
		this.m_Slots.Clear();
		for (int i = 0; i < MenuSaveLoadCommon.s_MaxSlots; i++)
		{
			SaveGameMenuSlot component = base.transform.FindDeepChild("Slot" + i.ToString()).GetComponent<SaveGameMenuSlot>();
			DebugUtils.Assert(component, true);
			this.m_Slots.Add(component);
		}
		for (int j = 0; j < MenuSaveLoadCommon.s_MaxSlots; j++)
		{
			string str = "Slot" + j.ToString() + ".sav";
			bool flag = File.Exists(Application.persistentDataPath + "/" + str);
			if (flag)
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				FileStream fileStream = File.Open(Application.persistentDataPath + "/" + str, FileMode.Open);
				GameVersion gameVersion = new GameVersion((GameVersion)binaryFormatter.Deserialize(fileStream));
				GameMode gameMode = (GameMode)binaryFormatter.Deserialize(fileStream);
				long dateData = (long)binaryFormatter.Deserialize(fileStream);
				int num = (int)binaryFormatter.Deserialize(fileStream) + 1;
				int num2 = (int)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				Localization localization = GreenHellGame.Instance.GetLocalization();
				this.m_Slots[j].m_Header.enabled = true;
				this.m_Slots[j].m_Text.enabled = true;
				this.m_Slots[j].m_Header.text = string.Empty;
				this.m_Slots[j].m_Text.text = string.Empty;
				if (gameMode == GameMode.Story)
				{
					this.m_Slots[j].m_Header.text = localization.Get("GameMode_Story") + " ";
				}
				else if (gameMode == GameMode.Survival)
				{
					this.m_Slots[j].m_Header.text = localization.Get("GameMode_Survival") + " ";
				}
				GameDifficulty gameDifficulty = (GameDifficulty)num2;
				if (gameDifficulty != GameDifficulty.Easy)
				{
					if (gameDifficulty != GameDifficulty.Normal)
					{
						if (gameDifficulty == GameDifficulty.Hard)
						{
							Text header = this.m_Slots[j].m_Header;
							header.text += localization.Get("DifficultyLevel_Hard");
						}
					}
					else
					{
						Text header2 = this.m_Slots[j].m_Header;
						header2.text += localization.Get("DifficultyLevel_Normal");
					}
				}
				else
				{
					Text header3 = this.m_Slots[j].m_Header;
					header3.text += localization.Get("DifficultyLevel_Easy");
				}
				Text header4 = this.m_Slots[j].m_Header;
				header4.text += " ";
				Text header5 = this.m_Slots[j].m_Header;
				header5.text += localization.Get("SaveGame_Day");
				Text header6 = this.m_Slots[j].m_Header;
				header6.text += ": ";
				Text header7 = this.m_Slots[j].m_Header;
				header7.text += num.ToString();
				DateTime dateTime = DateTime.FromBinary(dateData);
				this.m_Slots[j].m_Text.text = dateTime.ToString();
				this.m_Slots[j].m_GameMode = gameMode;
				this.m_Slots[j].m_Empty = false;
			}
			else
			{
				this.m_Slots[j].m_Header.enabled = true;
				this.m_Slots[j].m_Text.enabled = false;
				this.m_Slots[j].m_Header.text = GreenHellGame.Instance.GetLocalization().Get("MenuSaveGameSlotEmpty");
				this.m_Slots[j].m_Empty = true;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateButtons();
		this.UpdateSlots();
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null) && this.m_HLSlotIdx < 0) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
	}

	private void UpdateButtons()
	{
		this.m_ActiveButton = null;
		Vector2 screenPoint = Input.mousePosition;
		RectTransform component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton;
		}
		Color color = this.m_BackText.color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		this.m_BackText.color = color;
		component = this.m_BackText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_BackButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_BackButton)
		{
			color = this.m_BackText.color;
			color.a = 1f;
			this.m_BackText.color = color;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	protected virtual void UpdateSlots()
	{
		Vector3 mousePosition = Input.mousePosition;
		this.m_HLSlotIdx = -1;
		for (int i = 0; i < this.m_Slots.Count; i++)
		{
			RectTransform component = this.m_Slots[i].GetComponent<RectTransform>();
			if (RectTransformUtility.RectangleContainsScreenPoint(component, mousePosition))
			{
				this.m_HLSlotIdx = i;
			}
		}
		if (Input.GetMouseButton(0) && this.m_HLSlotIdx >= 0 && !LoadingScreen.Get().m_Active)
		{
			this.OnSlotSelected(this.m_HLSlotIdx);
		}
	}

	protected virtual void OnSlotSelected(int slot_idx)
	{
	}

	public override void OnBack()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuInGame));
	}

	public static int s_MaxSlots = 4;

	protected Button m_ActiveButton;

	public Button m_BackButton;

	protected Text m_BackText;

	protected List<SaveGameMenuSlot> m_Slots = new List<SaveGameMenuSlot>();

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	private int m_HLSlotIdx = -1;
}
