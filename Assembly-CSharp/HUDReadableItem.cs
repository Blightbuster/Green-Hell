using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDReadableItem : HUDBase, IInputsReceiver
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
		this.m_DetailsActive = false;
		this.m_PageNum = 0;
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
						componentsInChildren[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsInChildren[k].name, true);
					}
				}
			}
			else if (base.gameObject.transform.GetChild(i).gameObject.name == "HUD_" + this.m_ElementName + "_Details")
			{
				this.m_ReadButton.gameObject.SetActive(true);
				this.m_ReadButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Details", true);
				base.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
			else if (base.gameObject.transform.GetChild(i).gameObject != this.m_ReadButton.gameObject && base.gameObject.transform.GetChild(i).gameObject != this.m_PagePrevButton.gameObject && base.gameObject.transform.GetChild(i).gameObject != this.m_PageNextButton.gameObject)
			{
				base.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
		this.m_ScrollValue = 1f;
		if (GreenHellGame.IsPCControllerActive())
		{
			CursorManager.Get().ShowCursor(true, true);
			this.m_CursorVisible = true;
		}
		this.m_PagePrevButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_PrevPage", true);
		this.m_PageNextButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_NextPage", true);
		this.m_PagePrevButton.gameObject.SetActive(false);
		this.m_PageNextButton.gameObject.SetActive(false);
		this.m_QuitButton.gameObject.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Quit", true);
		this.m_QuitButton.gameObject.SetActive(true);
		this.m_ReadButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Details", true);
		this.m_ReadButtonPad.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_DetailsPad", true);
		this.m_PadScrollIcon.SetActive(false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
		}
		if (this.m_ActiveItem)
		{
			this.m_ActiveItem.m_WasReadedAndOff = true;
		}
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	public void Activate(string element_name, ReadableItem ri)
	{
		this.m_Active = true;
		this.m_ElementName = element_name;
		HUDManager.Get().PlaySound(this.m_AudioClip);
		this.m_ActiveItem = ri;
	}

	protected override void Update()
	{
		this.m_PadScrollIcon.SetActive(GreenHellGame.IsPadControllerActive() && this.m_DetailsActive && this.m_DetailsScrollbar != null);
		this.UpdateInputs();
	}

	private void UpdateInputs()
	{
		if (InputsManager.Get().IsActionActive(InputsManager.InputAction.Quit) || InputsManager.Get().IsActionActive(InputsManager.InputAction.AdditionalQuit))
		{
			this.Deactivate();
		}
		if (GreenHellGame.IsPadControllerActive() && this.m_DetailsScrollbar)
		{
			if (InputsManager.Get().IsActionActive(InputsManager.InputAction.LSForward))
			{
				this.m_ScrollValue += Time.deltaTime * this.m_PadScrollSpeed;
			}
			else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.LSBackward))
			{
				this.m_ScrollValue -= Time.deltaTime * this.m_PadScrollSpeed;
			}
			this.m_ScrollValue = Mathf.Clamp01(this.m_ScrollValue);
		}
		if (GreenHellGame.IsPadControllerActive() && this.m_DetailsScrollbar)
		{
			this.m_DetailsScrollbar.value = this.m_ScrollValue;
		}
	}

	public void Deactivate()
	{
		if (this.m_Active)
		{
			this.m_Active = false;
			Player.Get().UnblockMoves();
			Player.Get().UnblockRotation();
		}
	}

	public void OnQuitButton()
	{
		this.Deactivate();
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
							componentsInChildren[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsInChildren[k].name, true);
						}
					}
					this.m_ReadButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_DetailsHide", true);
					this.m_ReadButtonPad.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_DetailsHidePad", true);
					this.m_DetailsActive = true;
					this.m_DetailsObject = gameObject;
					this.m_DetailsScrollbar = gameObject.GetComponentInChildren<Scrollbar>();
					if (this.m_DetailsScrollbar)
					{
						this.m_DetailsScrollbar.value = 1f;
					}
					this.m_ScrollValue = 1f;
					this.UpdatePages();
				}
				else
				{
					base.gameObject.transform.GetChild(i).gameObject.SetActive(false);
					this.m_ReadButton.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_Details", true);
					this.m_ReadButtonPad.GetComponentInChildren<Text>().text = GreenHellGame.Instance.GetLocalization().Get("HUD_ReadableItem_DetailsPad", true);
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
			int num2 = -1;
			try
			{
				num2 = int.Parse(s);
			}
			catch
			{
			}
			if (num2 >= 0)
			{
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
		}
		this.m_PagePrevButton.gameObject.SetActive(this.m_PageNum > 0);
		this.m_PageNextButton.gameObject.SetActive(this.m_PageNum < num - 1);
	}

	public void OnInputAction(InputActionData action_data)
	{
		if (action_data.m_Action == InputsManager.InputAction.ReadCollectable)
		{
			this.OnReadButton();
		}
	}

	public bool CanReceiveAction()
	{
		return base.enabled;
	}

	public bool CanReceiveActionPaused()
	{
		return false;
	}

	private bool m_Active;

	private static HUDReadableItem s_Instance;

	private string m_ElementName = string.Empty;

	public Button m_ReadButton;

	public GameObject m_ReadButtonPad;

	public Button m_QuitButton;

	public Button m_PagePrevButton;

	public Button m_PageNextButton;

	private bool m_DetailsActive;

	private int m_PageNum;

	private GameObject m_DetailsObject;

	private Scrollbar m_DetailsScrollbar;

	private AudioClip m_AudioClip;

	private ReadableItem m_ActiveItem;

	private bool m_CursorVisible;

	public float m_PadScrollSpeed = 3f;

	private float m_ScrollValue = 1f;

	public GameObject m_PadScrollIcon;
}
