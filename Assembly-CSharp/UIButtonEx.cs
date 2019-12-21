using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonEx : Button, IPointerEnterHandler, IEventSystemHandler, IPointerClickHandler
{
	protected override void Awake()
	{
		base.Start();
		this.m_Text = base.gameObject.GetComponentInChildren<Text>();
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		if (this.m_Text == null)
		{
			return;
		}
		switch (state)
		{
		case Selectable.SelectionState.Normal:
			this.m_Text.color = base.colors.normalColor;
			return;
		case Selectable.SelectionState.Highlighted:
			this.m_Text.color = base.colors.highlightedColor;
			return;
		case Selectable.SelectionState.Pressed:
			this.m_Text.color = base.colors.pressedColor;
			return;
		case Selectable.SelectionState.Disabled:
			this.m_Text.color = base.colors.disabledColor;
			return;
		default:
			this.m_Text.color = Color.black;
			return;
		}
	}

	private bool CanPlaySound()
	{
		return base.interactable;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (this.CanPlaySound() && this.m_FocusClip)
		{
			UIAudioPlayer.Play(this.m_FocusClip);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);
		if (this.CanPlaySound() && this.m_ClickClip)
		{
			UIAudioPlayer.Play(this.m_ClickClip);
		}
	}

	private Text m_Text;

	public bool m_MoveWhenFocused = true;

	public AudioClip m_FocusClip;

	public AudioClip m_ClickClip;
}
