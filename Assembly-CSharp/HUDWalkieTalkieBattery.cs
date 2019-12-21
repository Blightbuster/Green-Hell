using System;
using Enums;
using UnityEngine.UI;

public class HUDWalkieTalkieBattery : HUDBase
{
	public static HUDWalkieTalkieBattery Get()
	{
		return HUDWalkieTalkieBattery.s_Instance;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDWalkieTalkieBattery.s_Instance = this;
	}

	protected override bool ShouldShow()
	{
		return GreenHellGame.Instance.m_GameMode == GameMode.Story && (!ChallengesManager.Get() || !ChallengesManager.Get().IsChallengeActive()) && !HUDReadableItem.Get().isActiveAndEnabled && !ScenarioManager.Get().IsBoolVariableTrue("PlayerMechGameEnding") && (!ScenarioManager.Get().IsDreamOrPreDream() && !Inventory3DManager.Get().IsActive()) && !CutscenesManager.Get().IsCutscenePlaying();
	}

	protected override void Update()
	{
		base.Update();
		this.m_WTMask.fillAmount = PlayerWalkieTalkieModule.Get().GetBatteryLevel();
	}

	public Image m_WTMask;

	private static HUDWalkieTalkieBattery s_Instance;
}
