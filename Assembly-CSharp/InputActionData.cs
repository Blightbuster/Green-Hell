using System;
using UnityEngine;

public class InputActionData
{
	public InputActionData ShallowCopy()
	{
		return (InputActionData)base.MemberwiseClone();
	}

	public InputsManager.InputAction m_Action = InputsManager.InputAction.None;

	public TriggerAction.TYPE m_TriggerAction = TriggerAction.TYPE.None;

	public float m_Hold;

	public float m_StartHoldTime;

	public float m_LastPressTime;

	public float m_LastReleaseTime;

	public InputsManager.InputActionType m_Type;

	public KeyCode m_KeyCode;

	public KeyCode m_Ps4KeyCode;

	public bool m_Analog;

	public string m_AxisName;

	public Sprite m_PadIcon;

	public bool m_Inverted;

	public float m_Value;

	public float m_AxisValue;

	public Vector2 m_Histeresis;

	public ControllerType m_ControllerType;
}
