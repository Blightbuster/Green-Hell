using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptionsGraphics : MenuScreen, IYesNoDialogOwner
{
	protected override void Awake()
	{
		base.Awake();
		this.m_BackText = this.m_BackButton.GetComponentInChildren<Text>();
		this.m_SoftShadows.AddOption("Low", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_Off"));
		this.m_SoftShadows.AddOption("Medium", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_On"));
		this.m_ShadowsBlur.AddOption("Low", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_Low"));
		this.m_ShadowsBlur.AddOption("Medium", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_Medium"));
		this.m_ShadowsBlur.AddOption("High", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_High"));
		this.m_ShadowsBlur.AddOption("VeryHigh", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_VeryHigh"));
		this.m_Quality.AddOption("VeryLow", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_VeryLow"));
		this.m_Quality.AddOption("Low", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_Low"));
		this.m_Quality.AddOption("Medium", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_Medium"));
		this.m_Quality.AddOption("High", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_High"));
		this.m_Quality.AddOption("VeryHigh", GreenHellGame.Instance.GetLocalization().Get("MenuGraphics_VeryHigh"));
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_OptionsChanged = false;
		this.m_Quality.SetByOption(QualitySettings.names[QualitySettings.GetQualityLevel()]);
		this.m_SoftShadows.SetByOption((!GreenHellGame.Instance.m_Settings.m_SoftShadows) ? "Low" : "Medium");
		this.m_ShadowsBlur.SetByOption(GreenHellGame.Instance.m_Settings.m_ShadowsBlur.ToString());
	}

	protected override void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
		this.UpdateButtons();
	}

	private void UpdateButtons()
	{
		Color color = this.m_BackButton.GetComponentInChildren<Text>().color;
		Color color2 = this.m_BackButton.GetComponentInChildren<Text>().color;
		color.a = MainMenuScreen.s_ButtonsAlpha;
		color2.a = MainMenuScreen.s_InactiveButtonsAlpha;
		this.m_BackButton.GetComponentInChildren<Text>().color = color;
		this.m_Quality.SetColor(color);
		this.m_SoftShadows.SetColor(color);
		this.m_ShadowsBlur.SetColor(color);
		Vector2 screenPoint = Input.mousePosition;
		this.m_ActiveButton = null;
		RectTransform component = this.m_BackButton.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, screenPoint))
		{
			this.m_ActiveButton = this.m_BackButton.gameObject;
		}
		component = this.m_BackText.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == this.m_BackButton.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == this.m_BackButton.gameObject)
		{
			color = this.m_BackText.color;
			color.a = 1f;
			this.m_BackText.color = color;
		}
		this.UpdateButton(this.m_Quality);
		this.UpdateButton(this.m_SoftShadows);
		this.UpdateButton(this.m_ShadowsBlur);
		CursorManager.Get().SetCursor((!(this.m_ActiveButton != null)) ? CursorManager.TYPE.Normal : CursorManager.TYPE.MouseOver);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	private void UpdateButton(UISelectButton button)
	{
		RectTransform component = button.GetComponent<RectTransform>();
		if (RectTransformUtility.RectangleContainsScreenPoint(component, Input.mousePosition))
		{
			this.m_ActiveButton = button.gameObject;
		}
		component = button.m_Title.GetComponent<RectTransform>();
		Vector3 localPosition = component.localPosition;
		float num = (!(this.m_ActiveButton == button.gameObject)) ? this.m_ButtonTextStartX : this.m_SelectedButtonX;
		float num2 = Mathf.Ceil(num - localPosition.x) * Time.unscaledDeltaTime * 10f;
		localPosition.x += num2;
		component.localPosition = localPosition;
		if (this.m_ActiveButton == button.gameObject)
		{
			Color color = button.GetColor();
			color.a = 1f;
			button.SetColor(color);
		}
	}

	public override void OnBack()
	{
		if (this.m_OptionsChanged)
		{
			GreenHellGame.GetYesNoDialog().Show(this, DialogWindowType.YesNo, GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_AcceptTitle"), GreenHellGame.Instance.GetLocalization().Get("YNDialog_OptionsGame_Accept"), false);
		}
		else
		{
			this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
		}
	}

	public void OnYesFromDialog()
	{
		this.ApplyOptions();
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	private void ApplyOptions()
	{
		QualitySettings.SetQualityLevel(this.m_Quality.m_SelectionIdx);
		GreenHellGame.Instance.m_Settings.m_SoftShadows = (this.m_SoftShadows.m_SelectionIdx == 1);
		GreenHellGame.Instance.m_Settings.m_ShadowsBlur = (GameSettings.OptionLevel)this.m_ShadowsBlur.m_SelectionIdx;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		GreenHellGame.Instance.m_Settings.ApplySettings();
	}

	public void OnNoFromDialog()
	{
		this.m_MenuInGameManager.ShowScreen(typeof(MenuOptions));
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		this.m_OptionsChanged = true;
	}

	public void OnOkFromDialog()
	{
	}

	public Button m_BackButton;

	public Text m_BackText;

	private GameObject m_ActiveButton;

	public UISelectButton m_Quality;

	public UISelectButton m_SoftShadows;

	public UISelectButton m_ShadowsBlur;

	private float m_ButtonTextStartX;

	private float m_SelectedButtonX = 10f;

	private bool m_OptionsChanged;
}
