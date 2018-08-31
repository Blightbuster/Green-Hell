using System;

public class BodyInspectionControllerInputs
{
	public void Reset()
	{
		this.m_ChooseLimbX = -1f;
		this.m_ChooseLimbY = 1f;
		this.m_LimbRotation = 0.5f;
		this.m_LeftArmMouseY = 0f;
		this.m_RightArmMouseY = 0f;
		this.m_LeftLegMouseY = 0f;
		this.m_RightLegMouseY = 0f;
		this.m_RotateLimb = false;
		this.m_SelectLimb = false;
	}

	public float m_ChooseLimbX;

	public float m_ChooseLimbY = 1f;

	public float m_LimbRotation = 0.5f;

	public float m_LeftArmMouseY;

	public float m_RightArmMouseY;

	public float m_LeftLegMouseY;

	public float m_RightLegMouseY;

	public bool m_SelectItem;

	public bool m_RotateLimb;

	public bool m_SelectLimb;
}
