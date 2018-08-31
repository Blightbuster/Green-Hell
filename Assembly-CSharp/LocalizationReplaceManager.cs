using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalizationReplaceManager : MonoBehaviour
{
	private void Awake()
	{
		SceneManager.sceneLoaded += this.OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		List<Text> list = LocalizationReplaceManager.FindObjectsOfTypeAll<Text>(scene);
		string empty = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			Text text = list[i];
			if (text.text.StartsWith("&") & text.text.EndsWith("&"))
			{
				text.text = GreenHellGame.Instance.GetLocalization().Get(text.text.Trim(this.m_Trim));
			}
		}
	}

	public static List<T> FindObjectsOfTypeAll<T>(Scene scene)
	{
		List<T> list = new List<T>();
		foreach (GameObject gameObject in scene.GetRootGameObjects())
		{
			list.AddRange(gameObject.GetComponentsInChildren<T>(true));
		}
		return list;
	}

	private char[] m_Trim = new char[]
	{
		'&'
	};
}
