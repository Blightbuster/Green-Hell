using System;

internal class FPPControllerInputs
{
	public void Reset(bool reset_duck = true)
	{
		this.m_Horizontal = 0f;
		this.m_Vertical = 0f;
		this.m_MouseX = 0f;
		this.m_MouseY = 0f;
		this.m_Jump = false;
		if (reset_duck)
		{
			this.m_Duck = false;
		}
		this.m_Sprint = false;
	}

	public float m_Horizontal;

	public float m_Vertical;

	public float m_MouseX;

	public float m_MouseY;

	public bool m_Jump;

	public bool m_Duck;

	public bool m_Sprint;
}
