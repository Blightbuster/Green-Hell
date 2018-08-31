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

	private void Start()
	{
		this.m_Screens.Clear();
		MainMenuScreen[] componentsInChildren = base.GetComponentsInChildren<MainMenuScreen>();
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
		MainMenuManager.s_TerrainLoaded = true;
	}

	public void SetActiveScreen(Type screen_type, bool with_fade = true)
	{
		if (with_fade)
		{
			this.m_ScreenToShow = screen_type;
			FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
			fadeSystem.FadeOut(FadeType.All, new VDelegate(this.OnFadOutEnd), 4f, null);
		}
		else
		{
			this.SetActiveScreenInternal(screen_type);
		}
	}

	private void SetActiveScreenInternal(Type screen_type)
	{
		for (int i = 0; i < this.m_Screens.Count; i++)
		{
			MainMenuScreen mainMenuScreen = this.m_Screens[i];
			Type type = mainMenuScreen.GetType();
			if (type == screen_type)
			{
				this.m_Screens[i].gameObject.SetActive(true);
				this.m_Screens[i].OnShow();
			}
			else
			{
				this.m_Screens[i].gameObject.SetActive(false);
				this.m_Screens[i].OnHide();
			}
		}
	}

	private void OnFadOutEnd()
	{
		this.SetActiveScreenInternal(this.m_ScreenToShow);
		FadeSystem fadeSystem = GreenHellGame.GetFadeSystem();
		fadeSystem.FadeIn(FadeType.All, null, 4f);
	}

	public void HideAllScreens()
	{
		for (int i = 0; i < this.m_Screens.Count; i++)
		{
			this.m_Screens[i].gameObject.SetActive(false);
			this.m_Screens[i].OnHide();
		}
	}

	private static MainMenuManager s_Instance;

	private List<MainMenuScreen> m_Screens = new List<MainMenuScreen>();

	private static bool s_TerrainLoaded;

	private Type m_ScreenToShow;
}
