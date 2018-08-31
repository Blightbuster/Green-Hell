using System;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
	public static CustomCursor Get()
	{
		return CustomCursor.s_Instance;
	}

	private void Awake()
	{
		CustomCursor.s_Instance = this;
	}

	public void Show(bool show)
	{
		this.m_Visible = show;
	}

	private void OnGUI()
	{
		if (this.m_Visible)
		{
			GUI.DrawTexture(new Rect(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y, 32f, 32f), this.m_Texture);
		}
	}

	[HideInInspector]
	public bool m_Visible;

	[HideInInspector]
	public Texture2D m_Texture;

	private static CustomCursor s_Instance;
}
