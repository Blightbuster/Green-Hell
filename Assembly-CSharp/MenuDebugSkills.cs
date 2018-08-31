using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDebugSkills : MenuScreen
{
	protected override void Start()
	{
		base.Start();
		int num = 7;
		int num2 = 0;
		float num3 = this.m_StartY;
		float x = -100f;
		SkillsManager skillsManager = SkillsManager.Get();
		foreach (Skill skill in skillsManager.m_Skills)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_SkillSlotPrefab);
			gameObject.transform.parent = base.transform;
			MenuDebugSkills.SkillUISlot item = default(MenuDebugSkills.SkillUISlot);
			Vector3 zero = Vector3.zero;
			zero.x = x;
			zero.y = num3;
			num3 -= this.m_YDiff;
			num2++;
			if (num2 >= num)
			{
				num3 = this.m_StartY;
				num2 = 0;
				x = 100f;
			}
			gameObject.transform.localPosition = zero;
			item.slot = gameObject;
			item.skill = skill;
			item.name = gameObject.transform.Find("Name").gameObject.GetComponent<Text>();
			item.name.text = skill.m_Name;
			item.level = gameObject.transform.Find("Level").gameObject.GetComponent<Text>();
			item.next_level = gameObject.transform.Find("NextLevel").gameObject.GetComponent<Text>();
			item.value = gameObject.transform.Find("Value").gameObject.GetComponent<Text>();
			item.value_slider = gameObject.transform.Find("Slider").gameObject.GetComponent<Slider>();
			item.value_slider.minValue = 0f;
			item.value_slider.maxValue = 100f;
			item.check_box = gameObject.transform.Find("Toggle").gameObject.GetComponent<Toggle>();
			this.m_Skills.Add(item);
		}
		this.Setup();
	}

	protected override void OnShow()
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

	public GameObject m_SkillSlotPrefab;

	private float m_StartY = 290f;

	private float m_YDiff = 90f;

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
