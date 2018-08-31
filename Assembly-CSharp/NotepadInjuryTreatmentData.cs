using System;
using System.Collections.Generic;
using Enums;

public class NotepadInjuryTreatmentData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_InjuryTreatmentType.Count; i++)
		{
			if (this.m_InjuryTreatmentType[i] == NotepadKnownInjuryTreatment.None)
			{
				return true;
			}
			if (PlayerInjuryModule.Get().IsInjuryTreatmentUnlocked(this.m_InjuryTreatmentType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<NotepadKnownInjuryTreatment> m_InjuryTreatmentType = new List<NotepadKnownInjuryTreatment>();
}
