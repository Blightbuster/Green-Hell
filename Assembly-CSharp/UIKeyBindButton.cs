using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyBindButton : MonoBehaviour
{
	private void Update()
	{
		this.UpdateState();
		this.UpdateFrameColor();
		if (this.m_State == UIKeyBindButtonState.Selected)
		{
			this.UpdateInputs();
		}
	}

	private void UpdateState()
	{
		RectTransform component = this.m_Text.GetComponent<RectTransform>();
		bool flag = RectTransformUtility.RectangleContainsScreenPoint(component, Input.mousePosition);
		if (flag)
		{
			if (this.m_State == UIKeyBindButtonState.Unfocused)
			{
				this.m_State = UIKeyBindButtonState.Focused;
			}
			else if (this.m_State == UIKeyBindButtonState.Focused && InputsManager.Get().IsActionActive(InputsManager.InputAction.LMB))
			{
				this.m_State = UIKeyBindButtonState.Selected;
			}
		}
		else if (InputsManager.Get().IsActionActive(InputsManager.InputAction.LMB))
		{
			this.m_State = UIKeyBindButtonState.Unfocused;
		}
		else if (this.m_State == UIKeyBindButtonState.Focused)
		{
			this.m_State = UIKeyBindButtonState.Unfocused;
		}
	}

	private void UpdateFrameColor()
	{
		if (this.m_State == UIKeyBindButtonState.Unfocused)
		{
			this.m_Frame.color = UIKeyBindButton.s_UnfocusedColor;
		}
		else if (this.m_State == UIKeyBindButtonState.Focused)
		{
			this.m_Frame.color = UIKeyBindButton.s_FocusedColor;
		}
		else if (this.m_State == UIKeyBindButtonState.Selected)
		{
			this.m_Frame.color = UIKeyBindButton.s_SelectedColor;
		}
	}

	private void UpdateInputs()
	{
		IEnumerator enumerator = Enum.GetValues(typeof(KeyCode)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				KeyCode key = (KeyCode)obj;
				if (!this.SkipCode(key) && Input.GetKey(key))
				{
					this.OnKeyPressed(key);
					break;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	private bool SkipCode(KeyCode key)
	{
		return key == KeyCode.Mouse0 || key == KeyCode.Mouse1 || key == KeyCode.Mouse3 || key == KeyCode.Mouse4 || key == KeyCode.Mouse5 || key == KeyCode.Mouse6;
	}

	private void OnKeyPressed(KeyCode key)
	{
		this.m_KeyCode = key;
		this.m_Text.text = KeyCodeToString.GetString(key);
		this.m_State = UIKeyBindButtonState.Focused;
	}

	public void SetInputAction(InputsManager.InputAction action)
	{
		this.m_Actions.Clear();
		this.m_Actions.Add(action);
	}

	public void SetInputActions(List<InputsManager.InputAction> actions)
	{
		this.m_Actions.Clear();
		this.m_Actions = new List<InputsManager.InputAction>(actions);
	}

	public void SetTriggerActions(List<TriggerAction.TYPE> actions)
	{
		this.m_TriggerActions.Clear();
		this.m_TriggerActions = new List<TriggerAction.TYPE>(actions);
	}

	public List<TriggerAction.TYPE> GetTriggerActions()
	{
		return this.m_TriggerActions;
	}

	public InputsManager.InputAction GetInputAction()
	{
		return this.m_Actions[0];
	}

	public List<InputsManager.InputAction> GetInputActions()
	{
		return this.m_Actions;
	}

	public void SetKeyCode(KeyCode key)
	{
		this.m_KeyCode = key;
		this.m_Text.text = KeyCodeToString.GetString(key);
	}

	public KeyCode GetKeyCode()
	{
		return this.m_KeyCode;
	}

	public Text m_Title;

	public Text m_Text;

	public RawImage m_Frame;

	private static Color s_UnfocusedColor = Color.clear;

	private static Color s_FocusedColor = Color.yellow;

	private static Color s_SelectedColor = Color.green;

	private KeyCode m_KeyCode = KeyCode.Space;

	private List<InputsManager.InputAction> m_Actions = new List<InputsManager.InputAction>();

	private List<TriggerAction.TYPE> m_TriggerActions = new List<TriggerAction.TYPE>();

	private UIKeyBindButtonState m_State;
}
