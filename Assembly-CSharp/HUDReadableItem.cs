using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDReadableItem : HUDBase
{
	public static HUDReadableItem Get()
	{
		return HUDReadableItem.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDReadableItem.s_Instance = this;
		this.m_AudioClip = (Resources.Load("Sounds/TempSounds/Notepad/notepad_page_flip") as AudioClip);
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_ReadButton.gameObject.SetActive(false);
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			if (base.gameObject.transform.GetChild(i).gameObject.name == "HUD_" + this.m_ElementName)
			{
				GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
				gameObject.SetActive(true);
				for (int j = 0; j < gameObject.transform.childCount; j++)
				{
					Text[] componentsInChildren = gameObject.GetComponentsInChildren<Text>();
					for (int k = 0; k < componentsInChildren.Length; k++)
					{
						componentsInChildren[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsInChildren[k].name);
					}
				}
			}
			else if (base.gameObject.transform.GetChild(i).gameObject.name == "HUD_" + this.m_ElementName + "_Details")
			{
				this.m_ReadButton.gameObject.SetActive(true);
				this.m_ReadButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Details");
				base.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
			else if (base.gameObject.transform.GetChild(i).gameObject != this.m_ReadButton.gameObject && base.gameObject.transform.GetChild(i).gameObject != this.m_PagePrevButton.gameObject && base.gameObject.transform.GetChild(i).gameObject != this.m_PageNextButton.gameObject)
			{
				base.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
		CursorManager.Get().ShowCursor(true);
		this.m_PagePrevButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_PrevPage");
		this.m_PageNextButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_NextPage");
		this.m_PagePrevButton.gameObject.SetActive(false);
		this.m_PageNextButton.gameObject.SetActive(false);
		this.m_QuitButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Quit");
		this.m_QuitButton.gameObject.SetActive(true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		CursorManager.Get().ShowCursor(false);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	public void Activate(string element_name)
	{
		this.m_Active = true;
		this.m_ElementName = element_name;
		HUDManager.Get().PlaySound(this.m_AudioClip);
	}

	protected override void Update()
	{
		this.UpdateInputs();
	}

	private void UpdateInputs()
	{
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Quit) || Input.GetKeyDown(KeyCode.E))
		{
			this.Quit();
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			this.OnReadButton();
		}
	}

	private void Quit()
	{
		this.m_Active = false;
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
	}

	public void OnQuitButton()
	{
		this.Quit();
	}

	public void OnPagePrevButton()
	{
		this.m_PageNum--;
		this.UpdatePages();
	}

	public void OnPageNextButton()
	{
		this.m_PageNum++;
		this.UpdatePages();
	}

	public void OnReadButton()
	{
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			if (base.gameObject.transform.GetChild(i).gameObject.name == "HUD_" + this.m_ElementName + "_Details")
			{
				if (!this.m_DetailsActive)
				{
					GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
					gameObject.SetActive(true);
					for (int j = 0; j < gameObject.transform.childCount; j++)
					{
						Text[] componentsInChildren = gameObject.GetComponentsInChildren<Text>();
						for (int k = 0; k < componentsInChildren.Length; k++)
						{
							componentsInChildren[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsInChildren[k].name);
						}
					}
					this.m_ReadButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_DetailsHide");
					this.m_DetailsActive = true;
					this.m_DetailsObject = gameObject;
					this.UpdatePages();
				}
				else
				{
					base.gameObject.transform.GetChild(i).gameObject.SetActive(false);
					this.m_ReadButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Details");
					this.m_DetailsActive = false;
					this.m_PagePrevButton.gameObject.SetActive(false);
					this.m_PageNextButton.gameObject.SetActive(false);
				}
			}
		}
	}

	private void UpdatePages()
	{
		int num = 0;
		for (int i = 1; i < this.m_DetailsObject.gameObject.transform.childCount; i++)
		{
			string name = this.m_DetailsObject.gameObject.transform.GetChild(i).name;
			string s = name.Substring(name.Length - 1);
			int num2 = int.Parse(s);
			if (num2 + 1 > num)
			{
				num = num2 + 1;
			}
			if (num2 == this.m_PageNum)
			{
				this.m_DetailsObject.gameObject.transform.GetChild(i).gameObject.SetActive(true);
			}
			else
			{
				this.m_DetailsObject.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		this.m_PagePrevButton.gameObject.SetActive(this.m_PageNum > 0);
		this.m_PageNextButton.gameObject.SetActive(this.m_PageNum < num - 1);
	}

	private bool m_Active;

	private static HUDReadableItem s_Instance;

	private string m_ElementName = string.Empty;

	public Button m_ReadButton;

	public Button m_QuitButton;

	public Button m_PagePrevButton;

	public Button m_PageNextButton;

	private bool m_DetailsActive;

	private int m_PageNum;

	private GameObject m_DetailsObject;

	private AudioClip m_AudioClip;
}
