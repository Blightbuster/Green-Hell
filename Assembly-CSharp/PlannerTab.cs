using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlannerTab : NotepadTab
{
	public static PlannerTab Get()
	{
		return PlannerTab.s_Instance;
	}

	private void Awake()
	{
		PlannerTab.s_Instance = this;
	}

	private void OnEnable()
	{
		this.m_FramesToUpdate = 3;
	}

	public void OnPlannedTaskAdded()
	{
		this.RefillList();
	}

	public void OnPlannedTaskDeleted()
	{
		this.RefillList();
	}

	private void Update()
	{
		if (this.m_FramesToUpdate == 0)
		{
			this.RefillList();
		}
		if (this.m_FramesToUpdate > 0)
		{
			this.m_FramesToUpdate--;
		}
	}

	private void RefillList()
	{
		this.DeleteAllTasks();
		PlayerPlannerModule component = Player.Get().GetComponent<PlayerPlannerModule>();
		for (int i = 0; i < component.m_PlannedTasks.Count; i++)
		{
			UIListExElement uilistExElement = new UIListExElement(this.m_PrefabToInstantiate, base.gameObject);
			uilistExElement.text = component.m_PlannedTasks[i].m_LocalizedText;
			uilistExElement.ui_element.GetComponentInChildren<Text>().text = component.m_PlannedTasks[i].m_LocalizedText;
			if (component.m_PlannedTasks[i].m_Fullfiled)
			{
				uilistExElement.ui_element.GetComponentInChildren<Text>().color = this.m_FullfiledColor;
			}
			else
			{
				uilistExElement.ui_element.GetComponentInChildren<Text>().color = this.m_NormalColor;
			}
			this.m_Tasks.Add(uilistExElement);
		}
		this.UpdateElements();
	}

	private void UpdateElements()
	{
		Vector3 localPosition = this.m_DummyPos.transform.localPosition;
		for (int i = 0; i < this.m_Tasks.Count; i++)
		{
			this.m_Tasks[i].ui_element.transform.localPosition = localPosition;
			localPosition.y -= this.m_Tasks[i].ui_element.GetComponent<RectTransform>().rect.height;
		}
	}

	private void DeleteAllTasks()
	{
		for (int i = 0; i < this.m_Tasks.Count; i++)
		{
			UnityEngine.Object.Destroy(this.m_Tasks[i].ui_element);
		}
		this.m_Tasks.Clear();
	}

	private List<UIListExElement> m_Tasks = new List<UIListExElement>();

	public GameObject m_PrefabToInstantiate;

	public GameObject m_DummyPos;

	public static PlannerTab s_Instance;

	public Color m_NormalColor = Color.black;

	public Color m_FullfiledColor = Color.green;

	private int m_FramesToUpdate = 3;
}
