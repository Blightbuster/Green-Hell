using System;
using UnityEngine;

public class HUDBigInfoData
{
	public HUDBigInfoData()
	{
		this.m_ShowTime = Time.time;
	}

	public string m_Header = string.Empty;

	public string m_TextureName = string.Empty;

	public string m_Text = string.Empty;

	public float m_ShowTime = float.MaxValue;

	public static float s_Duration = 6f;
}
