using System;
using System.Collections.Generic;
using Enums;

public class NotepadSymptomTreatmentData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_SymptomTreatmentType.Count; i++)
		{
			if (this.m_SymptomTreatmentType[i] == NotepadKnownSymptomTreatment.None)
			{
				return true;
			}
			if (PlayerDiseasesModule.Get().IsSymptomTreatmentUnlocked(this.m_SymptomTreatmentType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<NotepadKnownSymptomTreatment> m_SymptomTreatmentType = new List<NotepadKnownSymptomTreatment>();
}
