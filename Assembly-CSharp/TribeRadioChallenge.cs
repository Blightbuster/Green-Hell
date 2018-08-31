using System;
using AIs;
using UnityEngine;

public class TribeRadioChallenge : Challenge
{
	public override void Activate(GameObject parent)
	{
		this.m_Radio = null;
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			GameObject gameObject = parent.transform.GetChild(i).gameObject;
			if (gameObject.name == "Radio")
			{
				this.m_Radio = gameObject.GetComponent<Trigger>();
			}
			else
			{
				HumanAIGroup component = gameObject.GetComponent<HumanAIGroup>();
				if (component)
				{
					this.m_Group = component;
					this.m_Group.Initialize();
				}
			}
		}
		DebugUtils.Assert(this.m_Group != null && this.m_Radio != null, "Missing Radio in challenge objects!", true, DebugUtils.AssertType.Info);
		base.Activate(parent);
	}

	public override void Update()
	{
		base.Update();
		if (!this.m_Group.m_Active)
		{
			float num = 0f;
			HumanAI closestMember = this.m_Group.GetClosestMember(out num);
			if (num <= 50f)
			{
				this.m_Group.Activate();
			}
		}
		if (this.m_Radio.WasTriggered())
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		string text = localization.Get("HUDTribeRadioChallenge_Radio");
		text += " ";
		int num = 0;
		int num2 = 0;
		Player.Get().GetGPSCoordinates(this.m_Radio.transform.position, out num, out num2);
		string text2 = text;
		text = string.Concat(new string[]
		{
			text2,
			num.ToString(),
			"'W ",
			num2.ToString(),
			"'S"
		});
		return text + "\n";
	}

	private Trigger m_Radio;

	private HumanAIGroup m_Group;
}
