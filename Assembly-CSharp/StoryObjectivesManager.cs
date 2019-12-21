using System;

public class StoryObjectivesManager : ObjectivesManager
{
	public new static StoryObjectivesManager Get()
	{
		return StoryObjectivesManager.s_Instance;
	}

	private void Awake()
	{
		StoryObjectivesManager.s_Instance = this;
	}

	private void Start()
	{
		this.ParseScript();
	}

	private void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Objectives/StoryObjectives.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "Objective")
			{
				Objective item = new Objective(key.GetVariable(0).SValue, key.GetVariable(1).SValue);
				this.m_Objectives.Add(item);
			}
		}
	}

	public bool IsStoryObjectiveUnlocked(string obj_name)
	{
		for (int i = 0; i < this.m_ActiveObjectives.Count; i++)
		{
			Objective objective = this.m_ActiveObjectives[i];
			if (obj_name == objective.m_Name)
			{
				return true;
			}
		}
		for (int j = 0; j < this.m_CompletedObjectives.Count; j++)
		{
			Objective objective2 = this.m_CompletedObjectives[j];
			if (obj_name == objective2.m_Name)
			{
				return true;
			}
		}
		return false;
	}

	protected override void DeactivateObjective(Objective obj)
	{
		if (obj == null)
		{
			DebugUtils.Assert(DebugUtils.AssertType.Info);
			return;
		}
		bool flag = false;
		for (int i = 0; i < this.m_ActiveObjectives.Count; i++)
		{
			if (this.m_ActiveObjectives[i].m_Name == obj.m_Name)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			obj.SetState(ObjectiveState.Completed);
			base.OnObjectiveCompleted(obj);
		}
	}

	public bool IsStoryObjectiveCompleted(string obj_name)
	{
		return base.IsObjectiveCompleted(obj_name);
	}

	public override void Save()
	{
		SaveGame.SaveVal("ActiveStoryObjectivesCount", this.m_ActiveObjectives.Count);
		for (int i = 0; i < this.m_ActiveObjectives.Count; i++)
		{
			SaveGame.SaveVal("ActiveStoryObjective" + i.ToString(), this.m_ActiveObjectives[i].m_Name);
		}
		SaveGame.SaveVal("CompletedStoryObjectivesCount", this.m_CompletedObjectives.Count);
		for (int j = 0; j < this.m_CompletedObjectives.Count; j++)
		{
			SaveGame.SaveVal("CompletedStoryObjective" + j.ToString(), this.m_CompletedObjectives[j].m_Name);
		}
	}

	public override void Load()
	{
		this.m_ActiveObjectives.Clear();
		int num = SaveGame.LoadIVal("ActiveStoryObjectivesCount");
		for (int i = 0; i < num; i++)
		{
			base.ActivateObjective(SaveGame.LoadSVal("ActiveStoryObjective" + i.ToString()), true);
		}
		this.m_CompletedObjectives.Clear();
		int num2 = SaveGame.LoadIVal("CompletedStoryObjectivesCount");
		for (int j = 0; j < num2; j++)
		{
			base.DeactivateObjective(SaveGame.LoadSVal("CompletedStoryObjective" + j.ToString()));
		}
	}

	private static StoryObjectivesManager s_Instance;
}
