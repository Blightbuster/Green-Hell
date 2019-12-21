using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class SaveGameMenuSlot : Selectable
{
	[HideInInspector]
	public SaveGameInfo m_SaveInfo { get; private set; }

	public bool IsHighlighted()
	{
		return base.currentSelectionState == Selectable.SelectionState.Highlighted;
	}

	protected override void Awake()
	{
		this.m_InitialColor = this.m_Header.color;
	}

	public void SetSaveInfo(SaveGameInfo? save_info)
	{
		SaveGameInfo saveInfo;
		if (save_info != null)
		{
			this.m_SaveInfo = save_info.Value;
			GreenHellGame.Instance.GetLocalization();
			this.m_Header.enabled = true;
			this.m_Text.enabled = true;
			this.m_Header.text = string.Empty;
			this.m_Text.text = string.Empty;
			Text header = this.m_Header;
			saveInfo = this.m_SaveInfo;
			header.text = saveInfo.GetInfoText();
			Text text = this.m_Text;
			saveInfo = this.m_SaveInfo;
			text.text = saveInfo.date.ToString();
			this.m_GameMode = this.m_SaveInfo.game_mode;
			this.m_Empty = false;
			this.SetupScreenshot(this.m_SaveInfo.file_name);
			if (this.m_SaveInfo.game_version < GreenHellGame.s_GameVersionReleaseCandidate)
			{
				this.m_SaveVersionInfo.enabled = true;
				this.m_SaveVersionInfo.text = this.m_SaveInfo.game_version.ToStringOfficial();
				this.m_Header.color = this.m_SaveVersionInfo.color;
			}
			else
			{
				this.m_SaveVersionInfo.enabled = false;
				this.m_Header.color = this.m_InitialColor;
			}
			this.m_Coop.enabled = false;
			return;
		}
		saveInfo = default(SaveGameInfo);
		this.m_SaveInfo = saveInfo;
		this.m_Header.enabled = true;
		this.m_Text.enabled = false;
		this.m_Header.text = GreenHellGame.Instance.GetLocalization().Get("MenuSaveGameSlotEmpty", true);
		this.m_Empty = true;
		this.m_SaveVersionInfo.enabled = false;
		this.m_Header.color = this.m_InitialColor;
		this.m_Coop.enabled = false;
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (state == Selectable.SelectionState.Highlighted)
		{
			this.m_HL.color = this.m_HL_HighlightColor;
			UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Focus);
			return;
		}
		if (state == Selectable.SelectionState.Pressed)
		{
			UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
			return;
		}
		this.m_HL.color = this.m_HL_DefaultColor;
	}

	public void SetupScreenshot(string slot_name)
	{
		if (this.m_Screenshot)
		{
			SaveGame.LoadScreenshot(slot_name, this.m_Screenshot);
			if (this.m_Screenshot.texture)
			{
				this.m_Screenshot.color = Color.white;
				return;
			}
			this.m_Screenshot.color = Color.black;
		}
	}

	public void ReleseScreenshot()
	{
		if (this.m_Screenshot && this.m_Screenshot.texture)
		{
			this.m_Screenshot.texture = null;
			this.m_Screenshot.color = Color.black;
		}
	}

	public Text m_Header;

	public Text m_Text;

	public Text m_SaveVersionInfo;

	public RawImage m_HL;

	public RawImage m_Screenshot;

	public RawImage m_Coop;

	private Color m_HL_DefaultColor = new Color32(0, 0, 0, 90);

	private Color m_HL_HighlightColor = new Color32(75, 75, 75, 90);

	[HideInInspector]
	public bool m_Empty = true;

	[HideInInspector]
	public GameMode m_GameMode;

	private Color m_InitialColor;
}
