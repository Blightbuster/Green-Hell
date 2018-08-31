using System;
using System.Collections.Generic;
using Enums;

public class NotepadDiseaseTreatmentData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_DiseaseTreatmentType.Count; i++)
		{
			if (this.m_DiseaseTreatmentType[i] == NotepadKnownDiseaseTreatment.None)
			{
				return true;
			}
			if (PlayerDiseasesModule.Get().IsDiseaseTreatmentUnlocked(this.m_DiseaseTreatmentType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<NotepadKnownDiseaseTreatment> m_DiseaseTreatmentType = new List<NotepadKnownDiseaseTreatment>();
}
