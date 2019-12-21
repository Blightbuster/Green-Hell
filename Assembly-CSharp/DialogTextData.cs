using System;

public class DialogTextData
{
	public DialogTextData()
	{
	}

	public DialogTextData(string text, float time)
	{
		this.m_Text = text;
		this.m_Time = time;
	}

	public string m_Text = string.Empty;

	public float m_Time;
}
