using System;

public class HUDNotepad : HUDBase
{
	public static HUDNotepad Get()
	{
		return HUDNotepad.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDNotepad.s_Instance = this;
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
		return this.m_Active;
	}

	public void OnQuit()
	{
		NotepadController.Get().Hide();
	}

	private bool m_Active;

	private static HUDNotepad s_Instance;
}
