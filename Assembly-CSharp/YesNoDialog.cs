using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class YesNoDialog : MonoBehaviour
{
	private void Start()
	{
		this.m_DW.m_LabelText = base.gameObject.FindChild("Txt_Label").GetComponent<Text>();
		this.m_DW.m_DescriptionText = base.gameObject.FindChild("Txt_Description").GetComponent<Text>();
		this.m_DW.m_Button1Text = base.gameObject.FindChild("Button1").FindChild("Text").GetComponent<Text>();
		this.m_DW.m_Button2Text = base.gameObject.FindChild("Button2").FindChild("Text").GetComponent<Text>();
		this.m_DW.m_Button3Text = base.gameObject.FindChild("Button3").FindChild("Text").GetComponent<Text>();
		this.m_DW.m_Button1 = base.gameObject.FindChild("Button1").GetComponent<Button>();
		this.m_DW.m_Button2 = base.gameObject.FindChild("Button2").GetComponent<Button>();
		this.m_DW.m_Button3 = base.gameObject.FindChild("Button3").GetComponent<Button>();
		this.m_DW.m_Button1Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_Yes");
		this.m_DW.m_Button2Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_No");
		this.m_DW.m_Button3Text.text = GreenHellGame.Instance.GetLocalization().Get("MenuYesNoDialog_OK");
		base.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (this.m_DialogType == DialogWindowType.YesNo)
			{
				this.NoButtonClicked();
			}
			if (this.m_DialogType == DialogWindowType.Ok)
			{
				this.OkButtonClicked();
			}
		}
	}

	public void Show(IYesNoDialogOwner owner, DialogWindowType type, string label = "label", string description = "description", bool change_effect = true)
	{
		this.m_DialogType = type;
		this.m_DW.m_LabelText.text = label;
		this.m_DW.m_DescriptionText.text = description;
		if (type == DialogWindowType.YesNo)
		{
			this.m_DW.m_Button1.gameObject.SetActive(true);
			this.m_DW.m_Button2.gameObject.SetActive(true);
			this.m_DW.m_Button3.gameObject.SetActive(false);
		}
		if (type == DialogWindowType.Ok)
		{
			this.m_DW.m_Button1.gameObject.SetActive(false);
			this.m_DW.m_Button2.gameObject.SetActive(false);
			this.m_DW.m_Button3.gameObject.SetActive(true);
		}
		base.gameObject.SetActive(true);
		this.m_Screen = owner;
		this.m_MainCanvasObject = GameObject.Find("MainMenuCanvas");
		if (this.m_MainCanvasObject != null)
		{
			this.m_MainCanvas = this.m_MainCanvasObject.GetComponent<Canvas>();
			this.m_MainCanvas.enabled = false;
		}
		this.m_ChangeEffect = change_effect;
		if (this.m_ChangeEffect)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 1f);
		}
		if (this.m_Screen is MainMenuScreen)
		{
			Time.timeScale = 0.5f;
		}
	}

	private void Close()
	{
		base.gameObject.SetActive(false);
		this.m_MainCanvasObject = GameObject.Find("MainMenuCanvas");
		if (this.m_MainCanvasObject != null)
		{
			this.m_MainCanvas = this.m_MainCanvasObject.GetComponent<Canvas>();
			this.m_MainCanvas.enabled = true;
		}
		if (this.m_ChangeEffect)
		{
			PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 0f);
		}
		if (this.m_Screen is MainMenuScreen)
		{
			Time.timeScale = 1f;
		}
	}

	public void YesButtonClicked()
	{
		this.m_Screen.OnYesFromDialog();
		this.Close();
	}

	public void NoButtonClicked()
	{
		this.m_Screen.OnNoFromDialog();
		this.Close();
	}

	public void OkButtonClicked()
	{
		this.m_Screen.OnOkFromDialog();
		this.Close();
	}

	private DialogueWindow m_DW = new DialogueWindow();

	private IYesNoDialogOwner m_Screen;

	private Canvas m_MainCanvas;

	private GameObject m_MainCanvasObject;

	private DialogWindowType m_DialogType;

	private bool m_ChangeEffect = true;
}
