using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
	public static SkillsManager Get()
	{
		return SkillsManager.s_Instance;
	}

	private void Awake()
	{
		SkillsManager.s_Instance = this;
	}

	private void Start()
	{
		this.LoadScript();
	}

	public void LoadScript()
	{
		Skill.s_Instances.Clear();
		this.m_StringToSkillType.Clear();
		TextAssetParser textAssetParser = new TextAssetParser(this.m_SkillsScript);
		for (int i = 0; i < textAssetParser.GetKeysCount(); i++)
		{
			Key key = textAssetParser.GetKey(i);
			if (key.GetName() == "Skill")
			{
				string svalue = key.GetVariable(0).SValue;
				Type type = Type.GetType(svalue + "Skill");
				Skill skill = Activator.CreateInstance(type) as Skill;
				skill.Initialize(svalue);
				for (int j = 0; j < key.GetKeysCount(); j++)
				{
					skill.Load(key.GetKey(j));
				}
				this.m_Skills.Add(skill);
				this.m_StringToSkillType.Add(svalue, type);
			}
		}
	}

	public bool SkillGreater(string name, float value)
	{
		Type key = this.m_StringToSkillType[name];
		Skill skill = Skill.s_Instances[key];
		return skill.m_Value > value;
	}

	public bool SkillGreaterOrEqual(string name, float value)
	{
		Type key = this.m_StringToSkillType[name];
		Skill skill = Skill.s_Instances[key];
		return skill.m_Value >= value;
	}

	public void Save()
	{
		foreach (Skill skill in this.m_Skills)
		{
			skill.Save();
		}
	}

	public void Load()
	{
		foreach (Skill skill in this.m_Skills)
		{
			skill.Load();
		}
	}

	public TextAsset m_SkillsScript;

	public List<Skill> m_Skills = new List<Skill>();

	private static SkillsManager s_Instance;

	private Dictionary<string, Type> m_StringToSkillType = new Dictionary<string, Type>();
}
