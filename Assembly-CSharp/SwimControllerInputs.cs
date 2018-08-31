using System;

internal class SwimControllerInputs
{
	public void Reset()
	{
		this.m_Horizontal = 0f;
		this.m_Vertical = 0f;
		this.m_MouseX = 0f;
		this.m_MouseY = 0f;
	}

	public float m_Horizontal;

	public float m_Vertical;

	public float m_MouseX;

	public float m_MouseY;
}
