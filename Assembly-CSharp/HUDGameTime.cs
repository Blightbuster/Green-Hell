using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDGameTime : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory);
	}

	protected override void Start()
	{
		base.Start();
		this.m_TODSky = MainLevel.Instance.m_TODSky;
	}

	protected override bool ShouldShow()
	{
		return !Player.Get().m_DreamActive && Player.Get().IsWatchControllerActive() && Player.Get().GetComponent<WatchController>().m_Mode == WatchMode.Hour;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateGameTimeText();
		this.UpdateDayTimeText();
	}

	private void UpdateGameTimeText()
	{
		if (!this.m_GameTimeText)
		{
			return;
		}
		float time = Time.time;
		string str = string.Format("{0}:{1:00}", (int)time / 60, (int)time % 60);
		this.m_GameTimeText.text = "Game Time " + str;
	}

	private void UpdateDayTimeText()
	{
		if (!this.m_DayTimeText)
		{
			return;
		}
		int num = (int)this.m_TODSky.Cycle.Hour;
		int num2 = (int)((this.m_TODSky.Cycle.Hour - (float)num) * 60f);
		string str = string.Format("{0}:{1:00}", num, num2);
		this.m_DayTimeText.text = "Day Time " + str;
	}

	public Text m_GameTimeText;

	public Text m_DayTimeText;

	private TOD_Sky m_TODSky;
}
