using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuDifficultyLevel : MainMenuScreen
{
	private void Update()
	{
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_Easy.GetComponentInChildren<Text>().color;
		Color color2 = this.m_Easy.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		this.m_Easy.GetComponentInChildren<Text>().color = color;
		this.m_Normal.GetComponentInChildren<Text>().color = color;
		this.m_Hard.GetComponentInChildren<Text>().color = color;
		this.m_BackButton.GetComponentInChildren<Text>().color = color;
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_Easy.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Easy;
		}
		component = this.m_Normal.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Normal;
		}
		component = this.m_Hard.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Hard;
		}
		component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton;
		}
		component = this.m_Easy.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		Vector3 position = component.position;
		float num = (!(this.m_ActiveButton == this.m_Easy)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX;
		float num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		num = ((!(this.m_ActiveButton == this.m_Easy)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Easy)
		{
			color = this.m_Easy.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Easy.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Normal.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num = ((!(this.m_ActiveButton == this.m_Normal)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Normal)
		{
			color = this.m_Normal.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Normal.GetComponentInChildren<Text>().color = color;
		}
		component = this.m_Hard.GetComponentInChildren<Text>().GetComponent<RectTransform>();
		position = component.position;
		num = ((!(this.m_ActiveButton == this.m_Hard)) ? MainMenu.s_ButtonTextStartX : MainMenu.s_SelectedButtonX);
		num2 = Mathf.Ceil(num - position.x) * Time.deltaTime * 10f;
		position.x += num2;
		component.position = position;
		if (this.m_ActiveButton == this.m_Hard)
		{
			color = this.m_Hard.GetComponentInChildren<Text>().color;
			color.a = 1f;
			this.m_Hard.GetComponentInChildren<Text>().color = color;
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

	public void OnEasy()
	{
		GreenHellGame.Instance.m_GameDifficulty = GameDifficulty.Easy;
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnNormal()
	{
		GreenHellGame.Instance.m_GameDifficulty = GameDifficulty.Normal;
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnHard()
	{
		GreenHellGame.Instance.m_GameDifficulty = GameDifficulty.Hard;
		MainMenuManager.Get().SetActiveScreen(typeof(SaveGameMenu), true);
	}

	public void OnBack()
	{
		MainMenuManager.Get().SetActiveScreen(typeof(MainMenu), true);
	}

	public Button m_Easy;

	public Button m_Normal;

	public Button m_Hard;

	private Button m_ActiveButton;

	public Button m_BackButton;
}
