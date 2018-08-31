using System;

public class HUDBodyInspectionMinigame : HUDBase
{
	protected override void OnShow()
	{
		base.OnShow();
		if (this.m_BIMController == null)
		{
			this.m_BIMController = BodyInspectionMiniGameController.Get();
		}
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return !(BodyInspectionMiniGameController.Get() == null) && BodyInspectionMiniGameController.Get().enabled;
	}

	private BodyInspectionMiniGameController m_BIMController;
}
