using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDMessage
{
	public float m_StartTime;

	public GameObject m_HudElem;

	public Text m_TextComponent;

	public RawImage m_BGComponent;

	public Image m_IconComponent;

	public Vector3 m_TargetTextPos = Vector3.zero;

	public Vector3 m_TargetBGPos = Vector3.zero;

	public Vector3 m_TargetIconPos = Vector3.zero;
}
