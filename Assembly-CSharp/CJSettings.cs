using System;
using UnityEngine;

public class CJSettings : ScriptableObject
{
	public static CJSettings Instance
	{
		get
		{
			if (CJSettings.s_Instance == null)
			{
				CJSettings.s_Instance = (Resources.Load("Scripts/CJSettings") as CJSettings);
			}
			return CJSettings.s_Instance;
		}
	}

	public bool m_UseLocalStorage;

	private static CJSettings s_Instance;
}
