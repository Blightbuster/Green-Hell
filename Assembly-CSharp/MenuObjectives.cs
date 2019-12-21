using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class MenuObjectives : MenuScreen
{
	public override void OnShow()
	{
		base.OnShow();
		this.Setup();
	}

	private void Setup()
	{
		this.m_ContentActive.text = string.Empty;
		this.m_ContentCompleted.text = string.Empty;
		List<Objective> activeObjectives = ObjectivesManager.Get().m_ActiveObjectives;
		List<Objective> completedObjectives = ObjectivesManager.Get().m_CompletedObjectives;
		Text contentActive = this.m_ContentActive;
		contentActive.text += "<color=orange>";
		Text contentActive2 = this.m_ContentActive;
		contentActive2.text += GreenHellGame.Instance.GetLocalization().Get("MenuObjectives_ActiveObjectives", true);
		Text contentActive3 = this.m_ContentActive;
		contentActive3.text += "</color>";
		Text contentActive4 = this.m_ContentActive;
		contentActive4.text += "\n";
		Text contentCompleted = this.m_ContentCompleted;
		contentCompleted.text += "<color=orange>";
		Text contentCompleted2 = this.m_ContentCompleted;
		contentCompleted2.text += GreenHellGame.Instance.GetLocalization().Get("MenuObjectives_CompletedObjectives", true);
		Text contentCompleted3 = this.m_ContentCompleted;
		contentCompleted3.text += "</color>";
		Text contentCompleted4 = this.m_ContentCompleted;
		contentCompleted4.text += "\n";
		for (int i = 0; i < activeObjectives.Count; i++)
		{
			Objective objective = activeObjectives[i];
			Text contentActive5 = this.m_ContentActive;
			contentActive5.text += "<color=white>";
			Text contentActive6 = this.m_ContentActive;
			contentActive6.text += GreenHellGame.Instance.GetLocalization().Get(objective.m_TextID, true);
			Text contentActive7 = this.m_ContentActive;
			contentActive7.text += "</color>";
			Text contentActive8 = this.m_ContentActive;
			contentActive8.text += "\n";
		}
		for (int j = 0; j < completedObjectives.Count; j++)
		{
			Objective objective2 = completedObjectives[j];
			Text contentCompleted5 = this.m_ContentCompleted;
			contentCompleted5.text += "<color=green>";
			Text contentCompleted6 = this.m_ContentCompleted;
			contentCompleted6.text += GreenHellGame.Instance.GetLocalization().Get(objective2.m_Name, true);
			Text contentCompleted7 = this.m_ContentCompleted;
			contentCompleted7.text += "</color>";
			Text contentCompleted8 = this.m_ContentCompleted;
			contentCompleted8.text += "\n";
		}
	}

	public Text m_ContentActive;

	public Text m_ContentCompleted;
}
