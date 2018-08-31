using System;
using UnityEngine;
using UnityEngine.UI;

public class UISelectButtonArrow : Button
{
	protected override void Start()
	{
		base.Start();
		this.m_Text = base.gameObject.GetComponentInChildren<Text>();
		this.m_SelectButton = base.gameObject.transform.parent.GetComponent<UISelectButton>();
		if (base.gameObject.name.Contains("Left"))
		{
			this.m_Dir = UISelectButtonArrowDir.Left;
		}
		else if (base.gameObject.name.Contains("Right"))
		{
			this.m_Dir = UISelectButtonArrowDir.Right;
		}
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
			if (this.m_Dir == UISelectButtonArrowDir.Left)
			{
				this.m_SelectButton.OnLeftButtonPressed();
			}
			else if (this.m_Dir == UISelectButtonArrowDir.Right)
			{
				this.m_SelectButton.OnRightButtonPressed();
			}
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

	public UISelectButton m_SelectButton;

	private UISelectButtonArrowDir m_Dir;
}
