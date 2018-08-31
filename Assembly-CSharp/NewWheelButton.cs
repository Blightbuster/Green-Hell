using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewWheelButton : Button
{
	protected override void Awake()
	{
		base.Awake();
		this.selection = base.transform.Find("Selection");
		this.text = base.transform.Find("Text");
		this.m_Icon = base.transform.Find("Icon").gameObject.GetComponent<RawImage>();
		this.selection.gameObject.SetActive(false);
		this.text.gameObject.SetActive(false);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		this.selection.gameObject.SetActive(true);
		this.text.gameObject.SetActive(true);
		CursorManager.Get().SetCursor(CursorManager.TYPE.MouseOver);
		this.m_Selected = true;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		this.selection.gameObject.SetActive(false);
		this.text.gameObject.SetActive(false);
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		this.m_Selected = false;
	}

	public void ResetAll()
	{
		this.selection.gameObject.SetActive(false);
		this.text.gameObject.SetActive(false);
		this.m_Selected = false;
	}

	private Transform selection;

	private Transform text;

	[HideInInspector]
	public RawImage m_Icon;

	[HideInInspector]
	public bool m_Selected;
}
