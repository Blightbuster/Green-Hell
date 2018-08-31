using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuInGameManager : MonoBehaviour
{
	public static MenuInGameManager Get()
	{
		return MenuInGameManager.s_Instance;
	}

	private void Awake()
	{
		MenuInGameManager.s_Instance = this;
	}

	private void Start()
	{
		this.Init();
	}

	private void Init()
	{
		GameObject gameObject = GameObject.Find("InGameMenu");
		DebugUtils.Assert(gameObject, true);
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			GameObject gameObject2 = gameObject.transform.GetChild(i).gameObject;
			MenuScreen component = gameObject2.GetComponent<MenuScreen>();
			if (component)
			{
				component.m_MenuInGameManager = this;
				component.Hide();
				this.m_Screens[component.GetType()] = component;
			}
		}
		this.m_OutlineCamera = GameObject.Find("OutlineCamera").GetComponent<Camera>();
		this.m_Initialized = true;
	}

	public MenuScreen GetMenu(Type screen_type)
	{
		MenuScreen result = null;
		this.m_Screens.TryGetValue(screen_type, out result);
		return result;
	}

	private void Update()
	{
		this.UpdateInputs();
	}

	private bool CanShowMenuInGame()
	{
		return !GreenHellGame.ROADSHOW_DEMO && !this.m_CurrentScreen && !Inventory3DManager.Get().gameObject.activeSelf && !NotepadController.Get().enabled && !MapController.Get().enabled && !BodyInspectionController.Get().enabled && !BodyInspectionMiniGameController.Get().enabled && (!HUDEndDemo.Get() || !HUDEndDemo.Get().enabled) && !CutscenesManager.Get().IsCutscenePlaying() && !HUDStartSurvivalSplash.Get().m_Active;
	}

	private void UpdateInputs()
	{
		if (MainLevel.Instance.IsMoviePlaying())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape) && this.CanShowMenuInGame())
		{
			this.ShowScreen(typeof(MenuInGame));
		}
		else if (this.m_CurrentScreen)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (this.m_CurrentScreen.GetType() == typeof(MenuInGame))
				{
					this.HideMenu();
				}
				else
				{
					this.m_CurrentScreen.OnBack();
				}
			}
			else if (Input.GetKeyDown(KeyCode.Tab))
			{
				this.HideMenu();
			}
		}
		else if (!MainLevel.Instance.IsPause() && Input.anyKeyDown)
		{
			foreach (Type type in this.m_Screens.Keys)
			{
				MenuScreen menuScreen = this.m_Screens[type];
				if (menuScreen.GetShortcutKey() != KeyCode.None && Input.GetKeyDown(menuScreen.GetShortcutKey()))
				{
					this.ShowScreen(type);
					break;
				}
			}
		}
		if (GreenHellGame.DEBUG && !Input.GetKey(KeyCode.LeftControl))
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				this.ShowScreen(typeof(MenuDebugItem));
			}
			else if (Input.GetKeyDown(KeyCode.F2))
			{
				this.ShowScreen(typeof(MenuDebugScenario));
			}
			else if (Input.GetKeyDown(KeyCode.F3))
			{
				this.ShowScreen(typeof(MenuDebugSpawners));
			}
			else if (Input.GetKeyDown(KeyCode.F4))
			{
				this.ShowScreen(typeof(MenuDebugSkills));
			}
			else if (Input.GetKeyDown(KeyCode.F5))
			{
				this.ShowScreen(typeof(MenuDebugAI));
			}
			else if (Input.GetKeyDown(KeyCode.F6))
			{
				this.ShowScreen(typeof(MenuDebugWounds));
			}
			else if (Input.GetKeyDown(KeyCode.F7))
			{
				this.ShowScreen(typeof(MenuDebugDialogs));
			}
			else if (Input.GetKeyDown(KeyCode.F9))
			{
				this.ShowScreen(typeof(MenuDebugLog));
			}
		}
	}

	public void ShowPrevoiusScreen()
	{
		if (this.m_CurrentScreen)
		{
			if (this.m_PrevoiusScreen)
			{
				this.ShowScreen(this.m_PrevoiusScreen.GetType());
			}
			else
			{
				this.HideMenu();
			}
		}
	}

	public void ShowScreen(Type screen_type)
	{
		if (LoadingScreen.Get().m_Active)
		{
			return;
		}
		if (!this.m_Initialized)
		{
			this.Init();
		}
		MenuScreen menuScreen = null;
		this.m_Screens.TryGetValue(screen_type, out menuScreen);
		if (!menuScreen)
		{
			DebugUtils.Assert("[MenuInGameManager::ShowScreen] Can't find screen " + screen_type.ToString() + "!", true, DebugUtils.AssertType.Info);
			return;
		}
		if (menuScreen == this.m_CurrentScreen)
		{
			return;
		}
		PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 1f);
		this.m_OutlineCamera.enabled = false;
		this.m_PrevoiusScreen = this.m_CurrentScreen;
		if (this.m_CurrentScreen)
		{
			this.m_CurrentScreen.Hide();
		}
		else
		{
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
			CursorManager.Get().ShowCursor(true);
			if (menuScreen.ShouldPauseGame())
			{
				MainLevel.Instance.Pause(true);
			}
		}
		menuScreen.Show();
		this.m_CurrentScreen = menuScreen;
	}

	public void HideMenu()
	{
		if (this.m_CurrentScreen)
		{
			this.m_CurrentScreen.Hide();
			if (this.m_CurrentScreen.ShouldPauseGame())
			{
				MainLevel.Instance.Pause(false);
			}
		}
		this.m_CurrentScreen = null;
		this.m_PrevoiusScreen = null;
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		CursorManager.Get().ShowCursor(false);
		this.m_OutlineCamera.enabled = true;
		PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 0f);
	}

	public bool IsAnyScreenVisible()
	{
		return this.m_CurrentScreen != null;
	}

	private Dictionary<Type, MenuScreen> m_Screens = new Dictionary<Type, MenuScreen>();

	private MenuScreen m_CurrentScreen;

	private MenuScreen m_PrevoiusScreen;

	private Camera m_OutlineCamera;

	private static MenuInGameManager s_Instance;

	private bool m_Initialized;
}
