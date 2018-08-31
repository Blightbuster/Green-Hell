using System;
using UnityEngine;

public class MenuScreen : MenuBase, IInputsReceiver
{
	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnEnable()
	{
	}

	public virtual KeyCode GetShortcutKey()
	{
		return KeyCode.None;
	}

	public bool CanReceiveAction()
	{
		return true;
	}

	public void OnInputAction(InputsManager.InputAction action)
	{
		if (base.gameObject.activeSelf && action == InputsManager.InputAction.Quit)
		{
			this.m_MenuInGameManager.HideMenu();
		}
	}

	public virtual bool ShouldPauseGame()
	{
		return true;
	}

	protected virtual void Update()
	{
	}

	public virtual void OnClose()
	{
		this.m_MenuInGameManager.HideMenu();
	}

	public virtual void OnBack()
	{
		this.m_MenuInGameManager.ShowPrevoiusScreen();
	}

	public virtual void Show()
	{
		base.gameObject.SetActive(true);
		this.OnShow();
		Player.Get().OnMenuScreenShow(base.GetType());
	}

	public virtual void Hide()
	{
		base.gameObject.SetActive(false);
		this.OnHide();
		Player.Get().OnMenuScreenHide();
	}

	protected void SetVisibleHUD(HUDManager.HUDGroup group)
	{
		this.m_VisibleHUD = group;
	}

	protected virtual void OnShow()
	{
		HUDManager.Get().SetActiveGroup(this.m_VisibleHUD);
	}

	protected virtual void OnHide()
	{
		HUDManager.Get().SetActiveGroup(HUDManager.HUDGroup.Game);
	}

	private HUDManager.HUDGroup m_VisibleHUD;

	[HideInInspector]
	public MenuInGameManager m_MenuInGameManager;
}
