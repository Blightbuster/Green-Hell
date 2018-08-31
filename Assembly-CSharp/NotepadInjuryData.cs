using System;
using System.Collections.Generic;
using Enums;

public class NotepadInjuryData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_InjuryType.Count; i++)
		{
			if (PlayerInjuryModule.Get().IsInjuryUnlocked(this.m_InjuryType[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<InjuryType> m_InjuryType = new List<InjuryType>();
}
