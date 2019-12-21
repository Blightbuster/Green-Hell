using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class StoryObjectivesTab : NotepadTab
{
	public override void Init()
	{
		base.Init();
		this.m_LeftDummyPosition.Set(-103.4f, 72.2f, 1920f);
		this.m_RightDummyPosition.Set(23.6f, 72.2f, 1920f);
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			GameObject gameObject = base.gameObject.transform.GetChild(i).gameObject;
			if (!(gameObject.name == "LeftPageDummy") && !(gameObject.name == "RightPageDummy"))
			{
				this.m_ItemsElelements.Add(gameObject);
				NotepadData component = gameObject.GetComponent<NotepadData>();
				if (component)
				{
					component.Init();
				}
			}
		}
		for (int j = 0; j < this.m_ItemsElelements.Count; j++)
		{
			Text[] componentsDeepChild = General.GetComponentsDeepChild<Text>(this.m_ItemsElelements[j]);
			for (int k = 0; k < componentsDeepChild.Length; k++)
			{
				componentsDeepChild[k].text = GreenHellGame.Instance.GetLocalization().Get(componentsDeepChild[k].name, true);
			}
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftShift))
		{
			this.DebugUnlockStoryEvents();
		}
		this.UpdateActivePage();
	}

	private void DebugUnlockStoryEvents()
	{
		this.m_DebugStoryObjectives.Clear();
		this.m_DebugStoryObjectives.Add("NB_OBJ_FindHelp");
		this.m_DebugStoryObjectives.Add("NB_OBJ_CheckVillage");
		this.m_DebugStoryObjectives.Add("NB_OBJ_MakeAyuhaska");
		this.m_DebugStoryObjectives.Add("NB_OBJ_Battery");
		this.m_DebugStoryObjectives.Add("NB_OBJ_Rock");
		this.m_DebugStoryObjectives.Add("NB_OBJ_PlaceA");
		this.m_DebugStoryObjectives.Add("NB_OBJ_PlaceB");
		this.m_DebugStoryObjectives.Add("NB_OBJ_PlaceC");
		this.m_DebugStoryObjectives.Add("NB_OBJ_FindGrabGear");
		this.m_DebugStoryObjectives.Add("NB_OBJ_FindFuel");
		this.m_DebugStoryObjectives.Add("NB_OBJ_CheckWhereElevator");
		this.m_DebugStoryObjectives.Add("NB_OBJ_CheckTheGoldMine");
		this.m_DebugStoryObjectives.Add("NB_OBJ_ClimbToStoneRing");
		this.m_DebugStoryObjectives.Add("NB_OBJ_2ndDream");
		this.m_DebugStoryObjectives.Add("NB_OBJ_CheckAirport");
		this.m_DebugStoryObjectives.Add("NB_OBJ_FindMia");
		this.m_DebugStoryObjectives.Add("NB_OBJ_W8AtAirport");
		this.m_DebugStoryObjectives.Add("NB_OBJ_Cenote");
		this.m_DebugStoryObjectives.Add("NB_OBJ_3rdDream");
		this.m_DebugStoryObjectives.Add("NB_OBJ_FindCure");
		this.m_DebugStoryObjectives.Add("NB_OBJ_FindLambda");
		int i = 0;
		while (i < this.m_DebugStoryObjectives.Count)
		{
			int index = UnityEngine.Random.Range(0, this.m_DebugStoryObjectives.Count);
			StoryObjectivesManager.Get().ActivateObjective(this.m_DebugStoryObjectives[index], false);
			if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
			{
				StoryObjectivesManager.Get().DeactivateObjective(this.m_DebugStoryObjectives[index]);
			}
			this.m_DebugStoryObjectives.RemoveAt(index);
		}
	}

	private void UpdateActivePage()
	{
		float num = 0f;
		bool flag = false;
		int numActiveElementsOnPage = 0;
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			this.m_ItemsElelements[i].SetActive(false);
		}
		for (int j = 0; j < StoryObjectivesManager.Get().m_CompletedObjectives.Count; j++)
		{
			NotepadData dataByname = this.GetDataByname(StoryObjectivesManager.Get().m_CompletedObjectives[j].m_Name);
			if (dataByname)
			{
				this.SetupElement(dataByname, ref flag, ref num, 0.7f, ref numActiveElementsOnPage, 14);
			}
		}
		for (int k = 0; k < StoryObjectivesManager.Get().m_ActiveObjectives.Count; k++)
		{
			NotepadData dataByname2 = this.GetDataByname(StoryObjectivesManager.Get().m_ActiveObjectives[k].m_Name);
			if (dataByname2)
			{
				this.SetupElement(dataByname2, ref flag, ref num, 0.7f, ref numActiveElementsOnPage, 14);
			}
		}
		this.m_NumActiveElementsOnPage = numActiveElementsOnPage;
	}

	private void SetupElement(NotepadData data, ref bool right_page, ref float shift_y, float height_scale, ref int num_elems, int num_elems_in_page)
	{
		if (data.ShouldShow())
		{
			data.gameObject.SetActive(true);
			if (data.gameObject.transform.childCount > 0)
			{
				RectTransform component = data.gameObject.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
				if (component)
				{
					Vector3 v = component.anchoredPosition;
					v.x = (right_page ? this.m_RightDummyPosition.x : this.m_LeftDummyPosition.x);
					v.y = (right_page ? this.m_RightDummyPosition.y : this.m_LeftDummyPosition.y);
					v.y += shift_y;
					v.z = this.m_LeftDummyPosition.z;
					component.anchoredPosition = v;
					shift_y -= 7f;
					num_elems++;
					if (num_elems >= num_elems_in_page)
					{
						if (!right_page)
						{
							shift_y = 0f;
						}
						right_page = true;
					}
				}
			}
			if (data != null)
			{
				data.m_WasActive = true;
				if (StoryObjectivesManager.Get().IsStoryObjectiveCompleted(data.name))
				{
					this.StrikeThroughChildren(data.gameObject.transform);
					return;
				}
			}
		}
		else
		{
			data.gameObject.SetActive(false);
		}
	}

	private NotepadData GetDataByname(string elem_name)
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			if (this.m_ItemsElelements[i].name == elem_name)
			{
				return this.m_ItemsElelements[i].gameObject.GetComponent<NotepadData>();
			}
		}
		return null;
	}

	private void StrikeThroughChildren(Transform trans)
	{
		for (int i = 0; i < trans.childCount; i++)
		{
			Text component = trans.GetChild(i).gameObject.GetComponent<Text>();
			if (component && !component.CompareTag("StrikeThrough"))
			{
				component.tag = "StrikeThrough";
				component.text = General.StrikeThrough(component.text);
			}
		}
	}

	public override int GetNewEntriesCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			NotepadData component = this.m_ItemsElelements[i].gameObject.GetComponent<NotepadData>();
			if (component != null && component.ShouldShow() && !component.m_WasActive)
			{
				num++;
			}
		}
		return num;
	}

	public override void Save(string name)
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			this.m_ItemsElelements[i].GetComponent<NotepadData>().Save(name + i);
		}
	}

	public override void Load(string name)
	{
		for (int i = 0; i < this.m_ItemsElelements.Count; i++)
		{
			Text[] componentsDeepChild = General.GetComponentsDeepChild<Text>(this.m_ItemsElelements[i]);
			for (int j = 0; j < componentsDeepChild.Length; j++)
			{
				componentsDeepChild[j].text = GreenHellGame.Instance.GetLocalization().Get(componentsDeepChild[j].name, true);
				componentsDeepChild[j].tag = "Untagged";
			}
		}
		for (int k = 0; k < this.m_ItemsElelements.Count; k++)
		{
			this.m_ItemsElelements[k].GetComponent<NotepadData>().Load(name + k);
		}
	}

	private List<GameObject> m_ItemsElelements = new List<GameObject>();

	private Vector3 m_LeftDummyPosition = Vector3.zero;

	private Vector3 m_RightDummyPosition = Vector3.zero;

	private List<string> m_DebugStoryObjectives = new List<string>();
}
