using System;
using System.Collections.Generic;
using UnityEngine;

public class InputActionData
{
	public InputsManager.InputAction m_Action = InputsManager.InputAction.None;

	public TriggerAction.TYPE m_TriggerAction = TriggerAction.TYPE.None;

	public float m_Hold;

	public float m_PressTime;

	public InputsManager.InputActionType m_Type;

	public List<KeyCode> m_KeyCodes = new List<KeyCode>();

	public string m_CrossPlatformInput = string.Empty;
}
