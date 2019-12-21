using System;
using UnityEngine.UI;

public class MainMenuOptions : MenuScreen
{
	public void OnGame()
	{
		if (this.m_ActiveMenuOption.m_Button == this.m_Game)
		{
			MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsGame), true);
		}
	}

	public void OnAudio()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsAudio), true);
	}

	public void OnControls()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsControls), true);
	}

	public void OnGraphics()
	{
		if (this.m_ActiveMenuOption.m_Button == this.m_Graphics)
		{
			MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsGraphics), true);
		}
	}

	public Button m_Graphics;

	public Button m_Game;

	public Button m_Controls;

	public Button m_Audio;

	public Button m_BackButton;
}
