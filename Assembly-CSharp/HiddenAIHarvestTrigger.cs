using System;
using System.Collections.Generic;
using AIs;

public class HiddenAIHarvestTrigger : Trigger
{
	protected override void Start()
	{
		base.Start();
		this.m_AI = base.gameObject.GetComponent<AI>();
		this.m_Name = this.m_AI.m_ID.ToString();
	}

	public override bool CanTrigger()
	{
		return this.m_AI && this.m_AI.m_GoalsModule && this.m_AI.m_GoalsModule.m_CurrentAction != null && this.m_AI.m_GoalsModule.m_CurrentAction.GetType() == typeof(Hide);
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (!this.RequiresToolToHarvest() || Player.Get().HasBlade())
		{
			actions.Add(TriggerAction.TYPE.Harvest);
		}
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
		base.OnExecute(action);
		if (action == TriggerAction.TYPE.Harvest)
		{
			this.m_AI.m_HealthModule.Die();
			DeadBody component = base.gameObject.GetComponent<DeadBody>();
			component.OnExecute(action);
		}
	}

	public override string GetName()
	{
		return this.m_Name;
	}

	public override bool ShowAdditionalInfo()
	{
		return this.RequiresToolToHarvest() && !Player.Get().HasBlade();
	}

	public override string GetAdditionalInfoLocalized()
	{
		return GreenHellGame.Instance.GetLocalization().Get("HUDAddInfo_BladeRequired");
	}

	public override bool RequiresToolToHarvest()
	{
		return true;
	}

	public override string GetIconName()
	{
		return "HUDTrigger_Harvest";
	}

	private AI m_AI;

	private string m_Name = string.Empty;
}
