using System;
using UnityEngine.UI;

public class MenuOptions : MenuScreen
{
	public override bool IsMenuButtonEnabled(Button b)
	{
		return base.IsMenuButtonEnabled(b);
	}

	public override void OnBack()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuInGame));
	}

	public void OnGame()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MainMenuOptionsGame));
	}

	public void OnAudio()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MainMenuOptionsAudio));
	}

	public void OnGraphics()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MainMenuOptionsGraphics));
	}

	public void OnControls()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MainMenuOptionsControls));
	}

	public Button m_Graphics;

	public Button m_Game;

	public Button m_Controls;

	public Button m_Audio;

	public Text m_AudioText;

	public Text m_GraphicsText;

	public Text m_ControlsText;

	public Text m_GameText;

	public Button m_BackButton;

	public Text m_BackText;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;
}
