using System;

public class PlayerParams
{
	public PlayerParams()
	{
		this.ParseScript();
	}

	public float m_FistFightNormalDamage { get; protected set; }

	public float m_FistFightHardDamage { get; protected set; }

	public void ParseScript()
	{
		ScriptParser scriptParser = new ScriptParser();
		scriptParser.Parse("Player/Player_params.txt", true);
		for (int i = 0; i < scriptParser.GetKeysCount(); i++)
		{
			Key key = scriptParser.GetKey(i);
			if (key.GetName() == "TriggerCheckRange")
			{
				this.m_TriggerCheckRange = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "TriggerUseRange")
			{
				this.m_TriggerUseRange = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FistFightNormalDamage")
			{
				this.m_FistFightNormalDamage = key.GetVariable(0).FValue;
			}
			else if (key.GetName() == "FistFightHardDamage")
			{
				this.m_FistFightHardDamage = key.GetVariable(0).FValue;
			}
		}
	}

	public float GetTriggerCheckRange()
	{
		return this.m_TriggerCheckRange;
	}

	public float GetTriggerUseRange()
	{
		return this.m_TriggerUseRange;
	}

	private float m_TriggerCheckRange = 2f;

	private float m_TriggerUseRange = 2f;
}
