using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class DirtSickness : Disease
{
	public DirtSickness()
	{
		this.m_Type = ConsumeEffect.DirtSickness;
	}

	public override void Check()
	{
		base.Check();
		this.UpdateLevel();
		if (this.m_Level >= 1)
		{
			PlayerDiseasesModule.Get().RequestDisease(ConsumeEffect.DirtSickness, 0f, 0);
		}
	}

	public override void Update()
	{
		base.Update();
		this.UpdateLevel();
		if (this.m_Level < 1)
		{
			this.Deactivate();
		}
	}

	private void UpdateLevel()
	{
		this.m_Level = Mathf.FloorToInt(PlayerConditionModule.Get().m_Dirtiness / (PlayerConditionModule.Get().m_MaxDirtiness / (float)this.m_MaxLevel));
	}

	protected override void CheckAutoHeal()
	{
	}

	public override void OnEat(ConsumableInfo info)
	{
		base.OnEat(info);
		float num = UnityEngine.Random.Range(0f, 1f);
		Disease disease = PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.DirtSickness);
		if (num < (float)disease.m_Level * 0.1f)
		{
			ParasiteSickness parasiteSickness = (ParasiteSickness)PlayerDiseasesModule.Get().GetDisease(ConsumeEffect.ParasiteSickness);
			if (parasiteSickness.IsActive())
			{
				parasiteSickness.IncreaseLevel(1);
			}
			else
			{
				PlayerDiseasesModule.Get().RequestDisease(ConsumeEffect.ParasiteSickness, 0f, 1);
			}
			HUDMessages.Get().AddMessage("+1 " + GreenHellGame.Instance.GetLocalization().Get("HUDMessage_ParasiteFromDirt", true), new Color?(Color.white), HUDMessageIcon.ParasiteSickness, string.Empty, this.m_IconIndexesTemp);
		}
	}

	protected override bool CanApplyConsumeEffect(ConsumableInfo info)
	{
		return true;
	}

	private List<int> m_IconIndexesTemp = new List<int>(new int[1]);
}
