using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UISelectButton : MonoBehaviour
{
	public void SetColor(Color color)
	{
		this.m_Color = color;
		this.m_Title.color = this.m_Color;
		this.m_Text.color = this.m_Color;
		this.m_LeftArrow.GetComponentInChildren<Text>().color = color;
		this.m_RightArrow.GetComponentInChildren<Text>().color = color;
	}

	public Color GetColor()
	{
		return this.m_Color;
	}

	public void SetTitle(string title)
	{
		this.m_Title.text = title;
	}

	public void AddOption(string option, string localization)
	{
		this.m_Options[option] = localization;
	}

	public void SetByOption(string option)
	{
		for (int i = 0; i < this.m_Options.Keys.Count; i++)
		{
			if (this.m_Options.Keys.ElementAt(i) == option)
			{
				this.m_SelectionIdx = i;
				this.UpdateEverything();
				return;
			}
		}
	}

	public void SetOptionText(string option, string text)
	{
		for (int i = 0; i < this.m_Options.Keys.Count; i++)
		{
			if (this.m_Options.Keys.ElementAt(i) == option)
			{
				this.m_Options[option] = text;
				return;
			}
		}
	}

	private void OnEnable()
	{
		this.UpdateEverything();
	}

	private void UpdateEverything()
	{
		if (this.m_Options.Count == 0)
		{
			return;
		}
		this.m_Text.text = this.m_Options.Values.ElementAt(this.m_SelectionIdx);
		if (this.m_SelectionIdx == 0 && this.m_SelectionIdx < this.m_Options.Count - 1)
		{
			this.m_LeftArrow.interactable = false;
		}
		else
		{
			this.m_LeftArrow.interactable = true;
		}
		if (this.m_SelectionIdx < this.m_Options.Count - 1)
		{
			this.m_RightArrow.interactable = true;
		}
		else
		{
			this.m_RightArrow.interactable = false;
		}
	}

	public void OnLeftButtonPressed()
	{
		this.m_SelectionIdx--;
		this.m_SelectionIdx = Mathf.Clamp(this.m_SelectionIdx, 0, this.m_Options.Count);
		this.UpdateEverything();
		this.m_Target.OnSelectionChanged(this, this.m_Options.Keys.ElementAt(this.m_SelectionIdx));
	}

	public void OnRightButtonPressed()
	{
		this.m_SelectionIdx++;
		this.m_SelectionIdx = Mathf.Clamp(this.m_SelectionIdx, 0, this.m_Options.Count);
		this.UpdateEverything();
		this.m_Target.OnSelectionChanged(this, this.m_Options.Keys.ElementAt(this.m_SelectionIdx));
	}

	public UISelectButtonArrow m_LeftArrow;

	public UISelectButtonArrow m_RightArrow;

	public Text m_Title;

	public Text m_Text;

	private Color m_Color = Color.white;

	[HideInInspector]
	public int m_SelectionIdx;

	public MenuBase m_Target;

	private Dictionary<string, string> m_Options = new Dictionary<string, string>();
}
