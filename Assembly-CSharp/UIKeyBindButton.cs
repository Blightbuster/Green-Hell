using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIKeyBindButton : Selectable
{
	public void SetMenuScreen(MenuScreen screen)
	{
		this.m_Screen = screen;
		DebugUtils.Assert(this.m_Screen, true);
	}

	private void Update()
	{
		this.UpdateFrameColor();
		if (base.currentSelectionState == Selectable.SelectionState.Highlighted && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			this.UpdateInputs();
		}
	}

	protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		if (state == Selectable.SelectionState.Highlighted)
		{
			UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Focus);
			return;
		}
		if (state == Selectable.SelectionState.Pressed)
		{
			UIAudioPlayer.Play(UIAudioPlayer.UISoundType.Click);
			this.Select();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && base.currentSelectionState == Selectable.SelectionState.Highlighted && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			this.OnKeyPressed(KeyCode.Mouse0);
			EventSystem.current.SetSelectedGameObject(null, null);
			return;
		}
		base.OnPointerDown(eventData);
	}

	private void UpdateFrameColor()
	{
		if (base.currentSelectionState == Selectable.SelectionState.Highlighted || base.currentSelectionState == Selectable.SelectionState.Pressed)
		{
			if (EventSystem.current.currentSelectedGameObject == base.gameObject)
			{
				this.m_Frame.color = UIKeyBindButton.s_SelectedColor;
				this.m_Title.color = this.m_HighlightedColor;
				this.m_Text.color = this.m_HighlightedKeyColor;
			}
			else
			{
				this.m_Frame.color = UIKeyBindButton.s_FocusedColor;
				this.m_Title.color = this.m_HighlightedColor;
				this.m_Text.color = this.m_HighlightedKeyColor;
			}
		}
		else
		{
			this.m_Frame.color = UIKeyBindButton.s_UnfocusedColor;
			this.m_Title.color = this.m_NormalColor;
			this.m_Text.color = this.m_NormalKeyColor;
		}
		if (this.m_Blinking)
		{
			Color color = this.s_BlinkingColor;
			color.a = Mathf.Clamp01(Mathf.Sin(this.m_BlinkingLength));
			this.m_BlinkingLength -= Time.unscaledDeltaTime * 8f;
			this.m_Frame.color = color;
			if (this.m_BlinkingLength <= 0f)
			{
				this.StopBlinking();
			}
		}
	}

	private void UpdateInputs()
	{
		if (Input.anyKeyDown)
		{
			foreach (object obj in Enum.GetValues(typeof(KeyCode)))
			{
				KeyCode key = (KeyCode)obj;
				if (!this.SkipCode(key) && Input.GetKey(key))
				{
					this.OnKeyPressed(key);
					EventSystem.current.SetSelectedGameObject(null, null);
					break;
				}
			}
		}
	}

	private bool SkipCode(KeyCode key)
	{
		return key == KeyCode.Escape;
	}

	private void OnKeyPressed(KeyCode key)
	{
		this.m_KeyCode = key;
		this.m_Text.text = KeyCodeToString.GetString(key);
		this.m_State = UIKeyBindButton.UIKeyBindButtonState.Focused;
		this.m_BindingChanged = true;
		this.m_Screen.OnBindingChanged(this);
		this.UpdateImage();
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

	public bool SetupKeyCode(KeyCode key)
	{
		bool result = this.m_KeyCode != key;
		this.m_KeyCode = key;
		this.m_Text.text = KeyCodeToString.GetString(key);
		this.m_BindingChanged = false;
		this.UpdateImage();
		return result;
	}

	public KeyCode GetKeyCode()
	{
		return this.m_KeyCode;
	}

	public void StartBlinking()
	{
		this.m_Blinking = true;
		this.m_BlinkingLength = this.m_BlinkingDuration * 8f;
	}

	private void StopBlinking()
	{
		this.m_Blinking = false;
		this.m_BlinkingLength = 0f;
	}

	private void UpdateImage()
	{
		this.m_LMB.gameObject.SetActive(false);
		this.m_MMB.gameObject.SetActive(false);
		this.m_RMB.gameObject.SetActive(false);
		this.m_Text.gameObject.SetActive(true);
		if (this.m_KeyCode == KeyCode.Mouse0)
		{
			this.m_LMB.gameObject.SetActive(true);
			this.m_Text.gameObject.SetActive(false);
			return;
		}
		if (this.m_KeyCode == KeyCode.Mouse2)
		{
			this.m_MMB.gameObject.SetActive(true);
			this.m_Text.gameObject.SetActive(false);
			return;
		}
		if (this.m_KeyCode == KeyCode.Mouse1)
		{
			this.m_RMB.gameObject.SetActive(true);
			this.m_Text.gameObject.SetActive(false);
		}
	}

	public Text m_Title;

	public Text m_Text;

	public RawImage m_Frame;

	public RawImage m_LMB;

	public RawImage m_MMB;

	public RawImage m_RMB;

	private static Color s_UnfocusedColor = Color.clear;

	private static Color s_FocusedColor = new Color(0.8f, 0.7372549f, 0.160784319f);

	private static Color s_SelectedColor = new Color(0.423529416f, 0.509803951f, 0.219607845f);

	public Color s_BlinkingColor = new Color(1f, 0f, 0f);

	public Color m_NormalColor = new Color(1f, 1f, 1f, 0.3529412f);

	public Color m_HighlightedColor = new Color(1f, 1f, 1f, 0.509803951f);

	public Color m_NormalKeyColor = new Color(1f, 1f, 1f, 0.509803951f);

	public Color m_HighlightedKeyColor = new Color(1f, 1f, 1f, 0.784313738f);

	private KeyCode m_KeyCode = KeyCode.Space;

	public List<InputsManager.InputAction> m_Actions = new List<InputsManager.InputAction>();

	public List<TriggerAction.TYPE> m_TriggerActions = new List<TriggerAction.TYPE>();

	private UIKeyBindButton.UIKeyBindButtonState m_State;

	[HideInInspector]
	public bool m_BindingChanged;

	private MenuScreen m_Screen;

	private float m_BlinkingDuration = 1f;

	private float m_BlinkingLength;

	private const float m_BlinkingSpeed = 8f;

	private bool m_Blinking;

	private bool m_IsSelected;

	private enum UIKeyBindButtonState
	{
		Unfocused,
		Focused,
		Selected
	}
}
