using System;

public class MenuDebugScreen : MenuScreen
{
	public override bool IsDebugScreen()
	{
		return true;
	}

	public override void OnBack()
	{
		if (this.m_MenuInGameManager)
		{
			this.m_MenuInGameManager.HideMenu();
			return;
		}
		base.OnBack();
	}
}
