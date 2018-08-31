using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBigInfo : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override bool ShouldShow()
	{
		return this.m_Infos.Count > 0;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Infos.Count == 0)
		{
			return;
		}
		HUDBigInfoData hudbigInfoData = this.m_Infos[0];
		this.m_Header.text = hudbigInfoData.m_Header;
		this.m_Text.text = hudbigInfoData.m_Text;
		if (Time.time - HUDBigInfoData.s_Duration > hudbigInfoData.m_ShowTime)
		{
			if (this.m_Infos.Count > 1)
			{
				this.m_Infos[1].m_ShowTime = Time.time;
			}
			this.m_Infos.RemoveAt(0);
		}
	}

	public void AddInfo(HUDBigInfoData data)
	{
		this.m_Infos.Add(data);
	}

	private List<HUDBigInfoData> m_Infos = new List<HUDBigInfoData>();

	public Text m_Header;

	public Text m_Text;
}
