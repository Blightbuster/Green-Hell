using System;
using UnityEngine.UI;

public class HUDChallengeInfo : HUDBase
{
	public static HUDChallengeInfo Get()
	{
		return HUDChallengeInfo.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDChallengeInfo.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	public void Activate(Challenge challenge)
	{
		this.m_Active = true;
		this.m_InfoText.text = challenge.GetLocalizedInfo();
		this.m_Challenge = challenge;
	}

	public void Deactivate()
	{
		this.m_Active = false;
	}

	protected override void Update()
	{
		base.Update();
		if (this.m_Active && this.m_Challenge != null && this.m_Challenge.UpdateHUDChallengeInfo())
		{
			this.m_InfoText.text = this.m_Challenge.GetLocalizedInfo();
		}
	}

	private bool m_Active;

	public Text m_InfoText;

	private Challenge m_Challenge;

	private static HUDChallengeInfo s_Instance;
}
