using System;
using UnityEngine;

public class InputActionData
{
	public InputActionData()
	{
	}

	public InputActionData(InputActionData src)
	{
		this.m_Action = src.m_Action;
		this.m_TriggerAction = src.m_TriggerAction;
		this.m_Hold = src.m_Hold;
		this.m_PressTime = src.m_PressTime;
		this.m_Type = src.m_Type;
		this.m_KeyCode = src.m_KeyCode;
	}

	public InputsManager.InputAction m_Action = InputsManager.InputAction.None;

	public TriggerAction.TYPE m_TriggerAction = TriggerAction.TYPE.None;

	public float m_Hold;

	public float m_PressTime;

	public InputsManager.InputActionType m_Type;

	public KeyCode m_KeyCode = KeyCode.Space;
}
