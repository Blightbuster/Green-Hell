using System;
using System.Collections.Generic;
using Enums;

public class NotepadSymptomData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_SymptomType.Count; i++)
		{
			if (PlayerDiseasesModule.Get().IsSymptomUnlocked(this.m_SymptomType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<Enums.DiseaseSymptom> m_SymptomType = new List<Enums.DiseaseSymptom>();
}
