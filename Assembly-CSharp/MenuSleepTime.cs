using System;
using UnityEngine.UI;

public class MenuSleepTime : MenuScreen
{
	public static MenuSleepTime Get()
	{
		return MenuSleepTime.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		MenuSleepTime.s_Instance = this;
	}

	public void SetRestingPlace(RestingPlace place)
	{
	}

	public override void OnShow()
	{
		base.OnShow();
	}

	protected override void Update()
	{
		base.Update();
		this.m_SleepDuration = (int)this.m_Slider.value;
		this.m_DurationText.text = this.m_SleepDuration.ToString();
	}

	public void OnSleep()
	{
	}

	private void OnFadeIn()
	{
	}

	public Slider m_Slider;

	public Text m_DurationText;

	private int m_SleepDuration;

	private static MenuSleepTime s_Instance;
}
