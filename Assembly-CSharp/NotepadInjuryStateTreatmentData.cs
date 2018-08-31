using System;
using System.Collections.Generic;
using Enums;

public class NotepadInjuryStateTreatmentData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_InjuryStateTreatmentType.Count; i++)
		{
			if (this.m_InjuryStateTreatmentType[i] == NotepadKnownInjuryStateTreatment.None)
			{
				return true;
			}
			if (PlayerInjuryModule.Get().IsInjuryStateTreatmentUnlocked(this.m_InjuryStateTreatmentType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<NotepadKnownInjuryStateTreatment> m_InjuryStateTreatmentType = new List<NotepadKnownInjuryStateTreatment>();
}
