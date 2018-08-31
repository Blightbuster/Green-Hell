using System;
using System.Collections.Generic;
using Enums;

public class NotepadInjuryStateData : NotepadData
{
	public override bool ShouldShow()
	{
		for (int i = 0; i < this.m_InjuryState.Count; i++)
		{
			if (PlayerInjuryModule.Get().IsInjuryStateUnlocked(this.m_InjuryState[i]))
			{
				return true;
			}
		}
		return false;
	}

	public List<InjuryState> m_InjuryState = new List<InjuryState>();
}
