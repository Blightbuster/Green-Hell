using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CJButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	private void Awake()
	{
		this.m_Image = base.gameObject.GetComponent<Image>();
		this.m_Normal = this.m_Image.sprite;
	}

	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		this.m_Image.sprite = this.m_Highlighted;
		this.m_IsOver = true;
	}

	public void OnPointerExit(PointerEventData pointerEventData)
	{
		this.m_Image.sprite = this.m_Normal;
		this.m_IsOver = false;
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		if (this.m_IsOver)
		{
			this.m_Image.sprite = this.m_Pressed;
		}
	}

	public void OnPointerUp(PointerEventData pointerEventData)
	{
		if (this.m_IsOver)
		{
			this.m_Image.sprite = this.m_Highlighted;
		}
	}

	private Sprite m_Normal;

	public Sprite m_Highlighted;

	public Sprite m_Pressed;

	private Image m_Image;

	[HideInInspector]
	public bool m_IsOver;
}
