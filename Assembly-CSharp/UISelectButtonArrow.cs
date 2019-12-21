using System;
using UnityEngine;
using UnityEngine.UI;

public class UISelectButtonArrow : Button
{
	protected override void Awake()
	{
		base.Start();
		this.m_Text = base.gameObject.GetComponentInChildren<Text>();
		if (this.m_SelectButton == null)
		{
			this.m_SelectButton = base.gameObject.transform.parent.GetComponent<UISelectButton>();
		}
		if (base.gameObject.name.Contains("Left"))
		{
			this.m_Dir = UISelectButtonArrowDir.Left;
			return;
		}
		if (base.gameObject.name.Contains("Right"))
		{
			this.m_Dir = UISelectButtonArrowDir.Right;
		}
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		switch (state)
		{
		case Selectable.SelectionState.Normal:
		{
			Color color = base.colors.normalColor;
			if (this.m_Text != null)
			{
				this.m_Text.color = color;
				return;
			}
			break;
		}
		case Selectable.SelectionState.Highlighted:
		{
			Color color = base.colors.highlightedColor;
			if (this.m_Text != null)
			{
				this.m_Text.color = color;
				return;
			}
			break;
		}
		case Selectable.SelectionState.Pressed:
		{
			Color color = base.colors.pressedColor;
			if (this.m_Text != null)
			{
				this.m_Text.color = color;
			}
			if (this.m_Dir == UISelectButtonArrowDir.Left)
			{
				this.m_SelectButton.OnLeftButtonPressed();
				return;
			}
			if (this.m_Dir == UISelectButtonArrowDir.Right)
			{
				this.m_SelectButton.OnRightButtonPressed();
				return;
			}
			break;
		}
		case Selectable.SelectionState.Disabled:
		{
			Color color = base.colors.disabledColor;
			if (this.m_Text != null)
			{
				this.m_Text.color = color;
				return;
			}
			break;
		}
		default:
		{
			Color color = Color.black;
			if (this.m_Text != null)
			{
				this.m_Text.color = color;
			}
			break;
		}
		}
	}

	private Text m_Text;

	public UISelectButton m_SelectButton;

	private UISelectButtonArrowDir m_Dir;
}
