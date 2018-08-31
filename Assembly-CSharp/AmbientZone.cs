using System;

public class AmbientZone : SensorBase
{
	protected override void OnEnter()
	{
		base.OnEnter();
		this.m_Multisample = MSManager.Get().PlayMultiSample(this.m_MultisampleName, this.m_FadeIn);
	}

	protected override void OnExit()
	{
		base.OnExit();
		MSManager.Get().StopMultiSample(this.m_Multisample, this.m_FadeOut);
	}

	public string m_MultisampleName = string.Empty;

	private MSMultiSample m_Multisample;

	public float m_FadeIn = 1f;

	public float m_FadeOut = 1f;
}
