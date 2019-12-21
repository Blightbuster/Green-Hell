using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISelectButton : Selectable, IUIChangeableOption
{
	protected override void Awake()
	{
		base.Awake();
		if (this.m_OnOff)
		{
			this.AddOption("Off", "UISelectButton_Off");
			this.AddOption("On", "UISelectButton_On");
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		this.m_IsSelected = true;
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		this.m_IsSelected = false;
	}

	private void Update()
	{
		if (this.m_IsSelected)
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				this.PressLeftArrow();
				return;
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				this.PressRightArrow();
			}
		}
	}

	public void PressLeftArrow()
	{
		if (this.m_LeftArrow.interactable)
		{
			this.OnLeftButtonPressed();
		}
	}

	public void PressRightArrow()
	{
		if (this.m_RightArrow.interactable)
		{
			this.OnRightButtonPressed();
		}
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		switch (state)
		{
		case Selectable.SelectionState.Normal:
			this.m_Title.color = base.colors.normalColor;
			this.m_Text.color = base.colors.normalColor;
			return;
		case Selectable.SelectionState.Highlighted:
			this.m_Title.color = base.colors.highlightedColor;
			this.m_Text.color = base.colors.highlightedColor;
			return;
		case Selectable.SelectionState.Pressed:
			this.m_Title.color = base.colors.pressedColor;
			this.m_Text.color = base.colors.pressedColor;
			return;
		case Selectable.SelectionState.Disabled:
			this.m_Title.color = base.colors.disabledColor;
			this.m_Text.color = base.colors.disabledColor;
			return;
		default:
			return;
		}
	}

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

	public void AddOption(string option, string unlocalized_text)
	{
		this.m_Options[option] = unlocalized_text;
		this.UpdateEverything();
	}

	public void ClearOptions()
	{
		this.m_Options.Clear();
		this.m_SelectionIdx = 0;
		this.m_PrevValue = 0;
		this.UpdateEverything();
	}

	public bool HasOption(string option)
	{
		string text;
		return this.m_Options.TryGetValue(option, out text);
	}

	public bool SetByOption(string option)
	{
		for (int i = 0; i < this.m_Options.Keys.Count; i++)
		{
			if (this.m_Options.Keys.ElementAt(i) == option)
			{
				this.m_SelectionIdx = i;
				this.UpdateEverything();
				return true;
			}
		}
		return false;
	}

	public void SetOptionText(string option, string unlocalized_text)
	{
		for (int i = 0; i < this.m_Options.Keys.Count; i++)
		{
			if (this.m_Options.Keys.ElementAt(i) == option)
			{
				this.m_Options[option] = unlocalized_text;
				return;
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.UpdateEverything();
	}

	public string GetSelectedOption()
	{
		if (this.m_Options.Count == 0)
		{
			return "Unknown";
		}
		return this.m_Options.Keys.ElementAt(this.m_SelectionIdx);
	}

	private void UpdateEverything()
	{
		if (this.m_Options.Count == 0)
		{
			return;
		}
		this.m_Text.text = (this.m_LocalizeOptions ? GreenHellGame.Instance.GetLocalization().Get(this.m_Options.Values.ElementAt(this.m_SelectionIdx), true) : this.m_Options.Values.ElementAt(this.m_SelectionIdx));
		if ((this.m_SelectionIdx > 0 || this.m_Cyclic) && base.interactable)
		{
			this.m_LeftArrow.interactable = true;
		}
		else
		{
			this.m_LeftArrow.interactable = false;
		}
		if ((this.m_SelectionIdx < this.m_Options.Count - 1 || this.m_Cyclic) && base.interactable)
		{
			this.m_RightArrow.interactable = true;
			return;
		}
		this.m_RightArrow.interactable = false;
	}

	public void OnLeftButtonPressed()
	{
		this.m_SelectionIdx--;
		if (this.m_Cyclic && this.m_SelectionIdx < 0)
		{
			this.m_SelectionIdx = this.m_Options.Count - 1;
		}
		this.m_SelectionIdx = Mathf.Clamp(this.m_SelectionIdx, 0, this.m_Options.Count - 1);
		this.UpdateEverything();
		MenuBase target = this.m_Target;
		if (target != null)
		{
			target.OnSelectionChanged(this, this.m_Options.Keys.ElementAt(this.m_SelectionIdx));
		}
		this.PlaySound();
	}

	public void OnRightButtonPressed()
	{
		this.m_SelectionIdx++;
		if (this.m_Cyclic && this.m_SelectionIdx >= this.m_Options.Count)
		{
			this.m_SelectionIdx = 0;
		}
		this.m_SelectionIdx = Mathf.Clamp(this.m_SelectionIdx, 0, this.m_Options.Count - 1);
		this.UpdateEverything();
		MenuBase target = this.m_Target;
		if (target != null)
		{
			target.OnSelectionChanged(this, this.m_Options.Keys.ElementAt(this.m_SelectionIdx));
		}
		this.PlaySound();
	}

	public void Refresh()
	{
		this.UpdateEverything();
	}

	public bool SetSelectedOptionBoolValue(bool value)
	{
		return this.SetByOption(value ? "On" : "Off");
	}

	public bool GetSelectedOptionBoolValue()
	{
		string selectedOption = this.GetSelectedOption();
		if (selectedOption != null)
		{
			if (selectedOption == "On" || selectedOption == "Yes")
			{
				return true;
			}
			if (selectedOption == "Off" || selectedOption == "No")
			{
				return false;
			}
			DebugUtils.Assert(false, "Wrong option value: " + selectedOption + "!", true, DebugUtils.AssertType.Info);
		}
		else
		{
			DebugUtils.Assert(false, "Option not found!", true, DebugUtils.AssertType.Info);
		}
		return false;
	}

	public void FillOptionsFromEnum<T>(string localization_prefix = "")
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		this.m_Options.Clear();
		foreach (object obj in Enum.GetValues(typeof(T)))
		{
			T t = (T)((object)obj);
			string text = t.ToString();
			this.AddOption(text, localization_prefix + text);
		}
		if (this.m_SelectionIdx > this.m_Options.Count)
		{
			this.m_SelectionIdx = 0;
		}
	}

	public bool SetSelectedOptionEnumValue<T>(T value) where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		return this.SetByOption(EnumUtils<T>.GetName(value));
	}

	public T GetSelectedOptionEnumValue<T>() where T : struct, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		string selectedOption = this.GetSelectedOption();
		if (selectedOption != null)
		{
			return EnumUtils<T>.GetValue(selectedOption);
		}
		DebugUtils.Assert(false, "Option not found!", true, DebugUtils.AssertType.Info);
		return default(T);
	}

	public bool DidValueChange()
	{
		return this.m_PrevValue != this.m_SelectionIdx;
	}

	public void StoreValue()
	{
		this.m_PrevValue = this.m_SelectionIdx;
	}

	public void RevertValue()
	{
		this.m_SelectionIdx = this.m_PrevValue;
		this.UpdateEverything();
	}

	public void PlaySound()
	{
		UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
	}

	public UISelectButtonArrow m_LeftArrow;

	public UISelectButtonArrow m_RightArrow;

	public Text m_Title;

	public Text m_Text;

	private Color m_Color = Color.white;

	[HideInInspector]
	public int m_SelectionIdx;

	private int m_PrevValue;

	public MenuBase m_Target;

	public bool m_Cyclic;

	public bool m_OnOff;

	public const string VAL_ON = "On";

	public const string VAL_YES = "Yes";

	public const string VAL_OFF = "Off";

	public const string VAL_NO = "No";

	public const string VAL_ALWAYS = "Always";

	public bool m_LocalizeOptions = true;

	private Dictionary<string, string> m_Options = new Dictionary<string, string>();

	private bool m_IsSelected;
}
