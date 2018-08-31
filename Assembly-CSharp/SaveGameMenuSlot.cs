using System;
using Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveGameMenuSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		this.m_HL.color = this.m_HL_HighlightColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.m_HL.color = this.m_HL_DefaultColor;
	}

	public Text m_Header;

	public Text m_Text;

	public RawImage m_HL;

	private Color m_HL_DefaultColor = new Color32(0, 0, 0, 90);

	private Color m_HL_HighlightColor = new Color32(75, 75, 75, 90);

	public bool m_Empty = true;

	public GameMode m_GameMode;
}
