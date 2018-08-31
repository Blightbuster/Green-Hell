using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsTab : NotepadTab
{
	private void Awake()
	{
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
			this.m_SkillsElement.Add(gameObject);
		}
		int num = 0;
		for (int j = 0; j < this.m_SkillsElement.Count; j++)
		{
			PageNum component = this.m_SkillsElement[j].GetComponent<PageNum>();
			if (component.m_PageNum + 1 > num)
			{
				num = component.m_PageNum + 1;
			}
		}
		this.m_NumActivePages = num * 2;
	}

	private void Start()
	{
		this.SetupDatas();
		this.UpdateTexts();
	}

	private void SetupDatas()
	{
		foreach (Skill skill in SkillsManager.Get().m_Skills)
		{
			SkillsTab.SkillData value = default(SkillsTab.SkillData);
			Transform transform = base.transform.Find(skill.m_Name);
			if (!transform)
			{
				Debug.LogWarning("Missing skill object - " + skill.m_Name);
			}
			else
			{
				value.parent = transform.gameObject;
				value.text = value.parent.transform.Find("Text").GetComponent<Text>();
				this.m_SkillDatas.Add(skill, value);
			}
		}
	}

	private void UpdateTexts()
	{
		foreach (Skill skill in this.m_SkillDatas.Keys)
		{
			this.m_SkillDatas[skill].text.text = Mathf.FloorToInt(skill.m_Value) + "/100";
		}
	}

	private void OnEnable()
	{
		this.UpdateActivePage();
		MenuNotepad.Get().UpdatePrevNextButtons();
		this.UpdateTexts();
	}

	public override void UpdatePages()
	{
		this.UpdateActivePage();
	}

	private void UpdateActivePage()
	{
		int num = 0;
		for (int i = 0; i < this.m_SkillsElement.Count; i++)
		{
			PageNum component = this.m_SkillsElement[i].GetComponent<PageNum>();
			if (component.m_PageNum == this.m_CurrentPage)
			{
				this.m_SkillsElement[i].SetActive(true);
				num++;
			}
			else
			{
				this.m_SkillsElement[i].SetActive(false);
			}
		}
		this.m_NumActiveElementsOnPage = num;
		this.UpdateTexts();
	}

	private Dictionary<Skill, SkillsTab.SkillData> m_SkillDatas = new Dictionary<Skill, SkillsTab.SkillData>();

	private List<GameObject> m_SkillsElement = new List<GameObject>();

	private struct SkillData
	{
		public GameObject parent;

		public Text text;
	}
}
