using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptions : MenuScreen
{
	protected override void Awake()
	{
		base.Awake();
		this.m_AudioText = this.m_Audio.GetComponentInChildren<Text>();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
		this.m_GraphicsText = this.m_Graphics.GetComponentInChildren<Text>();
	}

	protected override void Update()
	{
		base.Update();
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
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Graphics;
		}
		component = this.m_Audio.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_Audio;
		}
		component = this.m_Game.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint) && !this.m_EarlyAccess)
		{
			this.m_ActiveButton = this.m_Game;
		}
		component = this.m_Controls.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint) && !this.m_EarlyAccess)
		{
			this.m_ActiveButton = this.m_Controls;
		}
		component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton;
		}
		component = this.m_AudioText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_Audio)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_Audio)
		{
			color = this.m_AudioText.color;
			color.a = 1f;
			this.m_AudioText.color = color;
		}
		component = this.m_GraphicsText.GetComponent<RectTransform>();
		localPosition = component.localPosition;
		num = ((!(this.m_ActiveButton == this.m_Graphics)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
		num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_Graphics)
		{
			color = this.m_GraphicsText.color;
			color.a = 1f;
			this.m_GraphicsText.color = color;
		}
		if (this.m_EarlyAccess)
		{
			this.m_Game.GetComponentInChildren<Text>().color = color2;
		}
		if (this.m_EarlyAccess)
		{
			this.m_Controls.GetComponentInChildren<Text>().color = color2;
		}
		component = this.m_BackText.GetComponent<RectTransform>();
		localPosition = component.localPosition;
		num = ((!(this.m_ActiveButton == this.m_BackButton)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX);
		num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_BackButton)
		{
			color = this.m_BackText.color;
			color.a = 1f;
			this.m_BackText.color = color;
		}
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	public override void OnBack()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuInGame));
	}

	public void OnGame()
	{
	}

	public void OnAudio()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptionsAudio));
	}

	public void OnGraphics()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptionsGraphics));
	}

	public Button m_Graphics;

	public Button m_Game;

	public Button m_Controls;

	public Button m_Audio;

	public Text m_AudioText;

	public Text m_GraphicsText;

	private Button m_ActiveButton;

	public Button m_BackButton;

	public Text m_BackText;

	private bool m_EarlyAccess = GreenHellGame.s_GameVersion <= GreenHellGame.s_GameVersionEarlyAcces;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;
}
