using System;
using UnityEngine.UI;

public class HUDCutscene : HUDBase
{
	protected override void Awake()
	{
		base.Awake();
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return MainLevel.Instance.m_CutscenePlaying;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.m_Text.text = GreenHellGame.Instance.GetLocalization().Get(MainLevel.Instance.m_CutsceneName);
	}

	public Text m_Text;
}
