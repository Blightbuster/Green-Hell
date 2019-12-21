using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager Get()
	{
		return MainMenuManager.s_Instance;
	}

	private void Awake()
	{
		MainMenuManager.s_Instance = this;
	}

	private void OnDestroy()
	{
		MainMenuManager.s_Instance = null;
	}

	private void Start()
	{
		this.m_Screens.Clear();
		MenuScreen[] componentsInChildren = base.GetComponentsInChildren<MenuScreen>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			this.m_Screens.Add(componentsInChildren[i]);
		}
		this.SetActiveScreen(typeof(MainMenu), false);
		if (!MainMenuManager.s_TerrainLoaded)
		{
			SceneManager.LoadScene("Terrain", LoadSceneMode.Additive);
		}
	}

	private void Update()
	{
		if (MainMenuManager.s_TerrainLoaded)
		{
			return;
		}
		Scene sceneByName = SceneManager.GetSceneByName("Terrain");
		if (!sceneByName.isLoaded)
		{
			return;
		}
		GameObject[] rootGameObjects = sceneByName.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			UnityEngine.Object.DontDestroyOnLoad(rootGameObjects[i]);
		}
		SceneManager.UnloadSceneAsync(sceneByName);
		MainMenuManager.s_TerrainLoaded = true;
	}

	public void SetActiveScreen(Type screen_type, bool with_fade = true)
	{
		if (with_fade)
		{
			this.BlockUIInputs();
			this.m_ScreenToShow = screen_type;
			GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.OnFadOutEnd), 0.3f, null);
		}
		else
		{
			this.SetActiveScreenInternal(screen_type);
		}
		if (this.m_PreviousScreens.Contains(screen_type))
		{
			while (this.m_PreviousScreens.Peek() != screen_type)
			{
				this.m_PreviousScreens.Pop();
			}
			return;
		}
		this.m_PreviousScreens.Push(screen_type);
	}

	public Type GetActiveScreen()
	{
		for (int i = 0; i < this.m_Screens.Count; i++)
		{
			MenuScreen menuScreen = this.m_Screens[i];
			if (menuScreen != null && menuScreen.gameObject.activeSelf)
			{
				return menuScreen.GetType();
			}
		}
		return null;
	}

	private void SetActiveScreenInternal(Type screen_type)
	{
		for (int i = 0; i < this.m_Screens.Count; i++)
		{
			if (this.m_Screens[i].GetType() == screen_type)
			{
				this.m_Screens[i].Show();
			}
			else
			{
				this.m_Screens[i].Hide();
			}
		}
	}

	private void OnFadOutEnd()
	{
		this.SetActiveScreenInternal(this.m_ScreenToShow);
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 0.3f);
		this.UnblockUIInputs();
	}

	public void HideAllScreens()
	{
		for (int i = 0; i < this.m_Screens.Count; i++)
		{
			this.m_Screens[i].gameObject.SetActive(false);
			this.m_Screens[i].OnHide();
		}
	}

	public void ShowPreviousScreen()
	{
		if (this.IsUIInputsBlocked())
		{
			return;
		}
		if (this.m_PreviousScreens.Count == 1)
		{
			return;
		}
		while (this.m_PreviousScreens.Count > 0)
		{
			Type type = this.m_PreviousScreens.Pop();
			if (type != this.GetActiveScreen())
			{
				this.SetActiveScreen(type, true);
				return;
			}
		}
	}

	public bool IsChangingScreens()
	{
		if (this.m_ScreenToShow == null)
		{
			return false;
		}
		for (int i = 0; i < this.m_Screens.Count; i++)
		{
			MenuScreen menuScreen = this.m_Screens[i];
			if (this.m_ScreenToShow == menuScreen.GetType())
			{
				return menuScreen.gameObject.activeSelf;
			}
		}
		return false;
	}

	public bool IsUIInputsBlocked()
	{
		return this.m_CanvasGroup && !this.m_CanvasGroup.blocksRaycasts;
	}

	public void BlockUIInputs()
	{
		if (this.m_CanvasGroup)
		{
			this.m_CanvasGroup.blocksRaycasts = false;
		}
	}

	public void UnblockUIInputs()
	{
		if (this.m_CanvasGroup)
		{
			this.m_CanvasGroup.blocksRaycasts = true;
		}
	}

	public void CallStartGameFadeSequence()
	{
		base.Invoke("OnPreStartGame", 0.3f);
	}

	private void OnPreStartGame()
	{
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.OnStartGame), 2f, null);
		CursorManager.Get().ShowCursor(false, false);
	}

	private void OnStartGame()
	{
		LoadingScreen.Get().Show(LoadingScreenState.StartGame);
		GreenHellGame.Instance.m_FromSave = false;
		Music.Get().Stop(1f, 0);
		MainMenuManager.Get().HideAllScreens();
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 2f);
		base.Invoke("OnStartGameDelayed", 1f);
	}

	public void CallLoadGameFadeSequence()
	{
		base.Invoke("OnPreLoadGame", 0.3f);
	}

	private void OnPreLoadGame()
	{
		GreenHellGame.GetFadeSystem().FadeOut(FadeType.All, new VDelegate(this.OnLoadGame), 2f, null);
		CursorManager.Get().ShowCursor(false, false);
	}

	private void OnLoadGame()
	{
		LoadingScreen.Get().Show(LoadingScreenState.StartGame);
		GreenHellGame.Instance.m_FromSave = true;
		Music.Get().Stop(1f, 0);
		this.HideAllScreens();
		GreenHellGame.GetFadeSystem().FadeIn(FadeType.All, null, 2f);
		base.Invoke("OnStartGameDelayed", 1f);
	}

	private void OnStartGameDelayed()
	{
		Music.Get().PlayByName("loading_screen", false, 1f, 0);
		Music.Get().m_Source[0].loop = true;
		GreenHellGame.Instance.StartGame();
	}

	public void SetupController()
	{
		foreach (MenuScreen menuScreen in this.m_Screens)
		{
			menuScreen.SetupController();
		}
	}

	private static MainMenuManager s_Instance;

	private List<MenuScreen> m_Screens = new List<MenuScreen>();

	public CanvasGroup m_CanvasGroup;

	private Stack<Type> m_PreviousScreens = new Stack<Type>();

	private static bool s_TerrainLoaded;

	private Type m_ScreenToShow;
}
