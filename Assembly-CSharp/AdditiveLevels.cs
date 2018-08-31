using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveLevels : MonoBehaviour
{
	public static AdditiveLevels Get()
	{
		return AdditiveLevels.s_Instance;
	}

	private void Awake()
	{
		AdditiveLevels.s_Instance = this;
	}

	public void LoadScenes()
	{
		this.AddScene("LA");
		this.AddScene("LD");
		this.AddScene("3DMenu");
		this.AddScene("Terrain");
		this.AddScene("HUD");
		this.AddScene("InGameMenu");
		this.AddScene("Notepad");
		this.AddScene("Optimization");
	}

	private void OnEnable()
	{
		base.hideFlags = HideFlags.HideAndDontSave;
	}

	private void AddScene(string name)
	{
		int sceneCount = SceneManager.sceneCount;
		for (int i = 0; i < sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.name == name && sceneAt.isLoaded)
			{
				return;
			}
		}
		SceneManager.LoadScene(name, LoadSceneMode.Additive);
	}

	private static AdditiveLevels s_Instance;
}
