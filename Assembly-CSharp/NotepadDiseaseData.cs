using System;
using System.Collections.Generic;
using Enums;

public class NotepadDiseaseData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_DiseaseType.Count; i++)
		{
			if (PlayerDiseasesModule.Get().IsDiseaseUnlocked(this.m_DiseaseType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<ConsumeEffect> m_DiseaseType = new List<ConsumeEffect>();
}
