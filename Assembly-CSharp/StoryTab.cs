using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class StoryTab : NotepadTab
{
	private void Start()
	{
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
	}

	private void Initialize()
	{
		this.m_LeftDummy = base.gameObject.transform.FindDeepChild("DummyLeft");
		this.m_RightDummy = base.gameObject.transform.FindDeepChild("DummyRight");
		this.m_Initialized = true;
	}

	public void AddStoryEvent(string element_name)
	{
		Transform transform = base.gameObject.transform.FindDeepChild(element_name);
		if (transform == null)
		{
			DebugUtils.Assert("StoryTab AddStoryEvent - cant find object" + element_name, true, DebugUtils.AssertType.Info);
			return;
		}
		transform.gameObject.SetActive(true);
		if (!this.m_StoryEvents.Contains(transform.gameObject))
		{
			this.m_StoryEvents.Add(transform.gameObject);
		}
		Text[] componentsDeepChild = General.GetComponentsDeepChild<Text>(transform.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].text = GreenHellGame.Instance.GetLocalization().Get(componentsDeepChild[i].name, true);
		}
		this.m_NumActivePages = this.m_StoryEvents.Count;
		this.UpdatePages();
	}

	public void RemoveStoryEvent(string element_name)
	{
		Transform transform = base.gameObject.transform.FindDeepChild(element_name);
		if (transform == null)
		{
			DebugUtils.Assert("StoryTab AddStoryEvent - cant find object" + element_name, true, DebugUtils.AssertType.Info);
			return;
		}
		transform.gameObject.SetActive(false);
		if (this.m_StoryEvents.Contains(transform.gameObject))
		{
			this.m_StoryEvents.Remove(transform.gameObject);
		}
		this.m_NumActivePages = this.m_StoryEvents.Count;
		this.UpdatePages();
	}

	public override void UpdatePages()
	{
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
		int num = 0;
		for (int i = 0; i < this.m_StoryEvents.Count; i++)
		{
			if (i == this.m_CurrentPage * 2)
			{
				this.m_StoryEvents[i].SetActive(true);
				this.m_StoryEvents[i].transform.position = this.m_LeftDummy.transform.position;
				num++;
			}
			else if (i == this.m_CurrentPage * 2 + 1)
			{
				this.m_StoryEvents[i].SetActive(true);
				this.m_StoryEvents[i].transform.position = this.m_RightDummy.transform.position;
				num++;
			}
			else
			{
				this.m_StoryEvents[i].SetActive(false);
			}
			if (this.m_StoryEvents[i].activeSelf)
			{
				NotepadData component = this.m_StoryEvents[i].GetComponent<NotepadData>();
				if (component)
				{
					component.m_WasActive = true;
				}
			}
		}
		this.m_NumActiveElementsOnPage = num;
	}

	public override bool ShouldShowNoEntries()
	{
		return this.m_StoryEvents.Count <= 0;
	}

	public override void Save(string name)
	{
		base.Save(name);
		SaveGame.SaveVal("Count" + name, this.m_StoryEvents.Count);
		for (int i = 0; i < this.m_StoryEvents.Count; i++)
		{
			SaveGame.SaveVal(name + i, this.m_StoryEvents[i].name);
		}
		for (int j = 0; j < this.m_StoryEvents.Count; j++)
		{
			this.m_StoryEvents[j].GetComponent<NotepadData>().Save(name + j + "_");
		}
	}

	public override void Load(string name)
	{
		base.Load(name);
		while (this.m_StoryEvents.Count > 0)
		{
			this.RemoveStoryEvent(this.m_StoryEvents[0].name);
		}
		int num = SaveGame.LoadIVal("Count" + name);
		for (int i = 0; i < num; i++)
		{
			this.AddStoryEvent(SaveGame.LoadSVal(name + i));
		}
		for (int j = 0; j < this.m_StoryEvents.Count; j++)
		{
			this.m_StoryEvents[j].GetComponent<NotepadData>().Load(name + j + "_");
		}
	}

	public override int GetNewEntriesCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_StoryEvents.Count; i++)
		{
			NotepadData component = this.m_StoryEvents[i].GetComponent<NotepadData>();
			if (component != null && !component.m_WasActive)
			{
				num++;
			}
		}
		return num;
	}

	private List<GameObject> m_StoryEvents = new List<GameObject>();

	private Transform m_LeftDummy;

	private Transform m_RightDummy;

	private bool m_Initialized;
}
