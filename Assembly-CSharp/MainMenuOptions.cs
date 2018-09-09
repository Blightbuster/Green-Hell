using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuOptions : MainMenuScreen
{
	private void Update()
	{
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_Graphics.GetComponentInChildren<Text>().color;
		Color color2 = this.m_Graphics.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		color2.a = MainMenuScreen.s_InactiveButtonsAlpha;
		this.m_Graphics.GetComponentInChildren<Text>().color = color;
		this.m_Game.GetComponentInChildren<Text>().color = color;
		this.m_Controls.GetComponentInChildren<Text>().color = color;
		this.m_Audio.GetComponentInChildren<Text>().color = color;
		this.m_BackButton.GetComponentInChildren<Text>().color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_Graphics.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint) && !this.m_EarlyAccess)
		{
			this.m_ActiveButton = this.m_Graphics;
		}
		component = this.m_Audio.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Audio;
		}
		component = this.m_Game.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Game;
		}
		component = this.m_Controls.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Controls;
		}
		component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton;
		}
		component = this.m_Graphics.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		Vector3 position = component.position;
		float num = (!(this.m_ActiveButton == this.m_Graphics)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX;
		float num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		num = ((!(this.m_ActiveButton == this.m_Graphics)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Graphics)
		{
			color = this.m_Graphics.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Graphics.GetComponentInChildren<Text>().color = color;
		}
		if (this.m_EarlyAccess)
		{
			this.m_Graphics.GetComponentInChildren<Text>().color = color2;
		}
		component = this.m_Audio.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num = ((!(this.m_ActiveButton == this.m_Audio)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		num = ((!(this.m_ActiveButton == this.m_Audio)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Audio)
		{
			color = this.m_Audio.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Audio.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Game.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num = ((!(this.m_ActiveButton == this.m_Game)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Game)
		{
			color = this.m_Game.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Game.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Controls.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num = ((!(this.m_ActiveButton == this.m_Controls)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Controls)
		{
			color = this.m_Controls.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Controls.GetComponentInChildren<Text>().color = color;
		}
		if (this.m_ActiveButton == this.m_BackButton)
		{
			color = this.m_BackButton.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_BackButton.GetComponentInChildren<Text>().color = color;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public void OnGame()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsGame), true);
	}

	public void OnAudio()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsAudio), true);
	}

	public void OnControls()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenuOptionsControls), true);
	}

	public void OnBack()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenu), true);
	}

	public Button m_Graphics;

	public Button m_Game;

	public Button m_Controls;

	public Button m_Audio;

	private Button m_ActiveButton;

	public Button m_BackButton;

	private bool m_EarlyAccess = GreenHellGame.s_GameVersion <= GreenHellGame.s_GameVersionEarlyAccessUpdate2;
}
