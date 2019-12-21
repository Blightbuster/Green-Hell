using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDebugSkills : MenuDebugScreen
{
	protected void Start()
	{
		Transform transform = base.transform.Find("Slots");
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(false);
		}
		foreach (Skill skill in SkillsManager.Get().m_Skills)
		{
			GameObject gameObject = transform.GetChild(this.m_Skills.Count).gameObject;
			gameObject.SetActive(true);
			MenuDebugSkills.SkillUISlot skillUISlot = default(MenuDebugSkills.SkillUISlot);
			skillUISlot.slot = gameObject;
			skillUISlot.skill = skill;
			skillUISlot.name = gameObject.transform.Find("Name").gameObject.GetComponent<Text>();
			skillUISlot.name.text = skill.m_Name;
			skillUISlot.level = gameObject.transform.Find("Level").gameObject.GetComponent<Text>();
			skillUISlot.next_level = gameObject.transform.Find("NextLevel").gameObject.GetComponent<Text>();
			skillUISlot.value = gameObject.transform.Find("Value").gameObject.GetComponent<Text>();
			skillUISlot.value_slider = gameObject.transform.Find("Slider").gameObject.GetComponent<Slider>();
			skillUISlot.value_slider.minValue = 0f;
			skillUISlot.value_slider.maxValue = 100f;
			skillUISlot.check_box = gameObject.transform.Find("Toggle").gameObject.GetComponent<Toggle>();
			this.m_Skills.Add(skillUISlot);
		}
		this.Setup();
	}

	public override void OnShow()
	{
		base.OnShow();
		this.Setup();
	}

	private void Setup()
	{
		foreach (MenuDebugSkills.SkillUISlot skillUISlot in this.m_Skills)
		{
			skillUISlot.level.text = "Level: " + skillUISlot.skill.m_Level.ToString() + "/" + skillUISlot.skill.m_LevelsCount.ToString();
			skillUISlot.value.text = string.Concat(new object[]
			{
				"Value: ",
				skillUISlot.skill.m_Value.ToString(),
				"/",
				Skill.s_MaxValue
			});
			skillUISlot.value_slider.value = skillUISlot.skill.m_Value;
			SkillCurveKey key = skillUISlot.skill.m_Progress.GetKey(skillUISlot.skill.m_Value);
			skillUISlot.next_level.text = "Next level: " + key.m_Data.y.ToString();
		}
	}

	protected override void Update()
	{
		base.Update();
		foreach (MenuDebugSkills.SkillUISlot skillUISlot in this.m_Skills)
		{
			skillUISlot.skill.m_Value = (float)((int)skillUISlot.value_slider.value);
			skillUISlot.skill.m_Level = skillUISlot.skill.m_Progress.GetLevel(skillUISlot.skill.m_Value);
		}
		this.Setup();
	}

	private List<MenuDebugSkills.SkillUISlot> m_Skills = new List<MenuDebugSkills.SkillUISlot>();

	private struct SkillUISlot
	{
		public GameObject slot;

		public Skill skill;

		public Text name;

		public Text level;

		public Slider value_slider;

		public Text next_level;

		public Text value;

		public Toggle check_box;
	}
}
