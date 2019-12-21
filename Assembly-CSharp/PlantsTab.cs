using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class PlantsTab : NotepadTab
{
	public override void Init()
	{
		base.Init();
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
			this.m_PlantsElements.Add(gameObject);
		}
		int num = 0;
		for (int j = 0; j < this.m_PlantsElements.Count; j++)
		{
			Text[] componentsDeepChild = General.GetComponentsDeepChild<Text>(this.m_PlantsElements[j]);
			for (int k = 0; k < componentsDeepChild.Length; k++)
			{
				componentsDeepChild[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsDeepChild[k].name, true);
			}
			PageNum component = this.m_PlantsElements[j].GetComponent<PageNum>();
			if (component.m_PageNum + 1 > num)
			{
				num = component.m_PageNum + 1;
			}
		}
		this.m_NumActivePages = num * 2;
	}

	private void OnEnable()
	{
		this.UpdateActivePage();
		MenuNotepad.Get().UpdatePrevNextButtons();
	}

	public override void UpdatePages()
	{
		this.UpdateActivePage();
	}

	private void UpdateActivePage()
	{
		int num = 0;
		for (int i = 0; i < this.m_PlantsElements.Count; i++)
		{
			PageNum component = this.m_PlantsElements[i].GetComponent<PageNum>();
			NotepadData component2 = this.m_PlantsElements[i].gameObject.GetComponent<NotepadData>();
			if (component.m_PageNum == this.m_CurrentPage && (component2 == null || component2.ShouldShow()))
			{
				this.m_PlantsElements[i].SetActive(true);
				if (component2 != null)
				{
					component2.m_WasActive = true;
				}
				this.UpdateSubElements(this.m_PlantsElements[i].transform);
				num++;
			}
			else
			{
				this.m_PlantsElements[i].SetActive(false);
			}
		}
		this.m_NumActiveElementsOnPage = num;
	}

	private void UpdateSubElements(Transform trans)
	{
		for (int i = 0; i < trans.childCount; i++)
		{
			Transform child = trans.GetChild(i);
			NotepadData component = child.GetComponent<NotepadData>();
			if (component == null || component.ShouldShow())
			{
				if (component != null)
				{
					component.m_WasActive = true;
				}
				child.gameObject.SetActive(true);
			}
			else
			{
				child.gameObject.SetActive(false);
			}
		}
	}

	public override int GetNewEntriesCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_PlantsElements.Count; i++)
		{
			NotepadData component = this.m_PlantsElements[i].gameObject.GetComponent<NotepadData>();
			if (component != null && component.ShouldShow() && !component.m_WasActive)
			{
				num++;
			}
		}
		return num;
	}

	public override void Save(string name)
	{
		for (int i = 0; i < this.m_PlantsElements.Count; i++)
		{
			this.m_PlantsElements[i].GetComponent<NotepadData>().Save(name + i);
		}
	}

	public override void Load(string name)
	{
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionMasterHotfix2)
		{
			for (int i = 0; i < this.m_PlantsElements.Count; i++)
			{
				this.m_PlantsElements[i].GetComponent<NotepadData>().Load(name + i);
			}
		}
	}

	private List<GameObject> m_PlantsElements = new List<GameObject>();
}
