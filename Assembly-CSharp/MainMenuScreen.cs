using System;

public class MainMenuScreen : MenuBase
{
	public virtual void OnShow()
	{
	}

	public virtual void OnHide()
	{
	}

	public static float s_ButtonsAlpha = 1f;

	public static float s_InactiveButtonsAlpha = 0.3f;

	public static bool s_DebugUnlock;
}
