using System;

public class NotepadStoryObjectiveData : NotepadData
{
	public override bool ShouldShow()
	{
		return StoryObjectivesManager.Get().IsStoryObjectiveUnlocked(base.gameObject.name);
	}
}
