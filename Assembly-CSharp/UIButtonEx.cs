using System;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonEx : Button
{
	protected override void Start()
	{
		base.Start();
		this.m_Text = base.gameObject.GetComponentInChildren<Text>();
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		Color color;
		switch (state)
		{
		case Selectable.SelectionState.Normal:
			color = base.colors.normalColor;
			break;
		case Selectable.SelectionState.Highlighted:
			color = base.colors.highlightedColor;
			break;
		case Selectable.SelectionState.Pressed:
			color = base.colors.pressedColor;
			break;
		case Selectable.SelectionState.Disabled:
			color = base.colors.disabledColor;
			break;
		default:
			color = Color.black;
			break;
		}
		if (this.m_Text != null)
		{
			this.m_Text.color = color;
		}
	}

	private Text m_Text;
}
