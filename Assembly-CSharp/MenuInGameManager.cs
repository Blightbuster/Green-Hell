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

	private void OnDestroy()
	{
		MenuInGameManager.s_Instance = null;
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
			MenuScreen component = gameObject.transform.GetChild(i).gameObject.GetComponent<MenuScreen>();
			if (component)
			{
				component.m_MenuInGameManager = this;
				component.m_IsIngame = true;
				component.Hide();
				this.m_Screens[component.GetType()] = component;
			}
		}
		this.m_CanvasGroup = gameObject.GetComponent<CanvasGroup>();
		this.m_OutlineCamera = GameObject.Find("OutlineCamera").GetComponent<Camera>();
		this.m_Initialized = true;
	}

	public MenuScreen GetMenu(Type screen_type)
	{
		MenuScreen result = null;
		this.m_Screens.TryGetValue(screen_type, out result);
		return result;
	}

	public bool IsCursorVisible()
	{
		return this.m_CursorVisible;
	}

	public void SetCursorVisible(bool visible)
	{
		this.m_CursorVisible = visible;
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
		bool flag = Input.GetKeyDown(KeyCode.Escape) || (GreenHellGame.IsPadControllerActive() && Input.GetKeyDown(InputHelpers.PadButton.Start.KeyFromPad()));
		if (flag && this.CanShowMenuInGame())
		{
			this.ShowScreen(typeof(MenuInGame));
		}
		else if (this.m_CurrentScreen)
		{
			if (flag)
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
			else if (Input.GetKeyDown(KeyCode.Tab) && this.m_CurrentScreen.GetType() == typeof(MenuInGame))
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
				if (!Input.GetKey(KeyCode.RightControl))
				{
					this.ShowScreen(typeof(MenuDebugItem));
					return;
				}
				this.ShowScreen(typeof(MenuDebugSounds));
				return;
			}
			else if (Input.GetKeyDown(KeyCode.F2))
			{
				if (!Input.GetKey(KeyCode.RightControl))
				{
					this.ShowScreen(typeof(MenuDebugScenario));
					return;
				}
				this.ShowScreen(typeof(MenuDebugCamera));
				return;
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.F3))
				{
					this.ShowScreen(typeof(MenuDebugSpawners));
					return;
				}
				if (Input.GetKeyDown(KeyCode.F4))
				{
					this.ShowScreen(typeof(MenuDebugSkills));
					return;
				}
				if (Input.GetKeyDown(KeyCode.F5))
				{
					this.ShowScreen(typeof(MenuDebugAI));
					return;
				}
				if (Input.GetKeyDown(KeyCode.F6))
				{
					this.ShowScreen(typeof(MenuDebugWounds));
					return;
				}
				if (Input.GetKeyDown(KeyCode.F7))
				{
					this.ShowScreen(typeof(MenuDebugDialogs));
					return;
				}
				if (Input.GetKeyDown(KeyCode.F9))
				{
					this.ShowScreen(typeof(MenuDebugLog));
					return;
				}
				if (Input.GetKeyDown(KeyCode.F11))
				{
					this.ShowScreen(typeof(MenuDebugScenarioDialogs));
				}
			}
		}
	}

	public void ShowPrevoiusScreen()
	{
		if (this.m_CurrentScreen)
		{
			while (this.m_PreviousScreens.Count > 0)
			{
				Type type = this.m_PreviousScreens.Pop();
				if (type != this.m_CurrentScreen.GetType())
				{
					this.ShowScreen(type);
					return;
				}
			}
			this.HideMenu();
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
		if (this.m_PreviousScreens.Contains(screen_type))
		{
			while (this.m_PreviousScreens.Peek() != screen_type)
			{
				this.m_PreviousScreens.Pop();
			}
		}
		else
		{
			this.m_PreviousScreens.Push(screen_type);
		}
		if (this.m_CurrentScreen)
		{
			this.m_CurrentScreen.Hide();
		}
		else
		{
			Player.Get().BlockMoves();
			Player.Get().BlockRotation();
			if (GreenHellGame.IsPCControllerActive() || menuScreen.IsDebugScreen())
			{
				CursorManager.Get().ShowCursor(true, false);
				this.m_CursorVisible = true;
			}
			if (menuScreen.ShouldPauseGame())
			{
				MainLevel.Instance.Pause(true);
			}
		}
		menuScreen.Show();
		this.m_CurrentScreen = menuScreen;
		if (GreenHellGame.IsPadControllerActive() && screen_type == typeof(MenuInGame))
		{
			foreach (Type type in this.m_Screens.Keys)
			{
				if (screen_type != type)
				{
					this.m_Screens[type].m_ActiveMenuOption = null;
				}
			}
		}
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
		foreach (Type key in this.m_Screens.Keys)
		{
			this.m_Screens[key].m_ActiveMenuOption = null;
		}
		this.m_CurrentScreen = null;
		this.m_PreviousScreens.Clear();
		Player.Get().UnblockMoves();
		Player.Get().UnblockRotation();
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
		}
		this.m_OutlineCamera.enabled = true;
		PostProcessManager.Get().SetWeight(PostProcessManager.Effect.InGameMenu, 0f);
		InputsManager.Get().m_OmitMouseUp = true;
	}

	public bool IsAnyScreenVisible()
	{
		return this.m_CurrentScreen != null;
	}

	public bool IsScreenVisible(Type type)
	{
		return this.m_CurrentScreen != null && this.m_CurrentScreen.GetType() == type;
	}

	public void ScenarioShowScreen(string menu_name)
	{
		Type type = Type.GetType(menu_name);
		this.ShowScreen(type);
	}

	public void SetupController()
	{
		foreach (MenuScreen menuScreen in this.m_Screens.Values)
		{
			menuScreen.SetupController();
		}
	}

	private Dictionary<Type, MenuScreen> m_Screens = new Dictionary<Type, MenuScreen>();

	private MenuScreen m_CurrentScreen;

	private Stack<Type> m_PreviousScreens = new Stack<Type>();

	private Camera m_OutlineCamera;

	private static MenuInGameManager s_Instance;

	[HideInInspector]
	public CanvasGroup m_CanvasGroup;

	[HideInInspector]
	private bool m_CursorVisible;

	private bool m_Initialized;
}
