using System;

public class HUDMap : HUDBase
{
	public static HUDMap Get()
	{
		return HUDMap.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDMap.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	public void Activate()
	{
		this.m_Active = true;
	}

	public void Deactivate()
	{
		this.m_Active = false;
	}

	protected override bool ShouldShow()
	{
		return this.IsActive();
	}

	public bool IsActive()
	{
		return this.m_Active;
	}

	public void OnQuit()
	{
		MapController.Get().Hide();
	}

	private bool m_Active;

	private static HUDMap s_Instance;
}
