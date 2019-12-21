using System;

public class PlayerCutsceneController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.PlayerCutscene;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Player.m_UseGravity = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Player.m_UseGravity = true;
	}
}
