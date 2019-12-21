using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDObjective
{
	public HUDObjective()
	{
		this.m_StartTime = Time.time;
	}

	public Objective m_Objective;

	public GameObject m_HudElem;

	public Text m_TextComponent;

	public RawImage m_BG;

	public RawImage m_Icon;

	public Vector3 m_BGTargetPosition = Vector3.zero;

	public Vector3 m_TextTargetPosition = Vector3.zero;

	public Vector3 m_IconTargetPosition = Vector3.zero;

	public float m_StartTime = -1f;
}
