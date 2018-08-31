using System;

public class HUDBiohazardMask : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return Player.Get().m_ShowBiohazardMask;
	}
}
