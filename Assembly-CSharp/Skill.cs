using System;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
	public static T Get<T>() where T : Skill
	{
		return (T)((object)Skill.s_Instances[typeof(T)]);
	}

	public virtual void Initialize(string name)
	{
		Skill.s_Instances.Add(base.GetType(), this);
		this.m_Name = name;
		this.RegisterCurve(this.m_Progress, "ProgressCurve");
	}

	protected void RegisterCurve(SkillCurve curve, string key)
	{
		DebugUtils.Assert(curve != null, "[PlayerSkill:RegisterCurve] Error, curve == null!", true, DebugUtils.AssertType.Info);
		this.m_Curves.Add(curve, key);
	}

	public virtual void Load(Key key)
	{
		foreach (SkillCurve skillCurve in this.m_Curves.Keys)
		{
			if (key.GetName() == this.m_Curves[skillCurve])
			{
				skillCurve.Load(key);
			}
		}
		this.m_LevelsCount = this.m_Progress.m_Curve.Count;
	}

	public void Save()
	{
		SaveGame.SaveVal("Skill" + base.GetType().ToString(), this.m_Value);
	}

	public void Load()
	{
		this.m_Value = SaveGame.LoadFVal("Skill" + base.GetType().ToString());
	}

	public void OnSkillAction()
	{
		if (this.m_Value >= Skill.s_MaxValue)
		{
			return;
		}
		int num = Mathf.FloorToInt(this.m_Value);
		this.m_Value += this.m_Progress.Progress(this.m_Value);
		this.m_Value = Mathf.Clamp(this.m_Value, 0f, Skill.s_MaxValue);
		Localization localization = GreenHellGame.Instance.GetLocalization();
		int num2 = Mathf.FloorToInt(this.m_Value);
		if (num2 > num && HUDMessages.Get())
		{
			HUDMessages.Get().AddMessage(localization.Get("Skill_" + this.m_Name, true) + " +" + (num2 - num).ToString(), null, HUDMessageIcon.None, "", null);
		}
	}

	public static float s_MaxValue = 100f;

	[Range(0f, 100f)]
	public float m_Value;

	public int m_Level;

	public int m_LevelsCount;

	public SkillCurve m_Progress = new SkillCurve();

	public string m_Name = string.Empty;

	public Dictionary<SkillCurve, string> m_Curves = new Dictionary<SkillCurve, string>();

	public static Dictionary<Type, Skill> s_Instances = new Dictionary<Type, Skill>();
}
