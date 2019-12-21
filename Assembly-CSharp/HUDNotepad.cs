using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDNotepad : HUDBase
{
	public static HUDNotepad Get()
	{
		return HUDNotepad.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDNotepad.s_Instance = this;
		this.m_ItemsCounter = this.CreateCounter();
		this.m_StoryCounter = this.CreateCounter();
		this.m_SkillsCounter = this.CreateCounter();
		this.m_ConstructionsCounter = this.CreateCounter();
		this.m_TrapsCounter = this.CreateCounter();
		this.m_MudCounter = this.CreateCounter();
		this.m_FirecampCounter = this.CreateCounter();
		this.m_WaterConstructionsCounter = this.CreateCounter();
		this.m_HealingItemsCounter = this.CreateCounter();
		this.m_PlantsCounter = this.CreateCounter();
		this.m_CustomConstructionsCounter = this.CreateCounter();
		this.m_StoryObjectivesCounter = this.CreateCounter();
	}

	private HUDNotepad.Counter CreateCounter()
	{
		HUDNotepad.Counter counter = default(HUDNotepad.Counter);
		counter.obj = UnityEngine.Object.Instantiate<GameObject>(this.m_CounterPrefab, base.transform);
		counter.text = counter.obj.transform.Find("Count").GetComponent<Text>();
		this.m_AllCounters.Add(counter);
		return counter;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Show(false);
	}

	public void Activate()
	{
		this.m_Active = true;
	}

	public void Deactivate()
	{
		this.m_Active = false;
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.UpdateCounters();
	}

	protected override void OnHide()
	{
		base.OnHide();
		foreach (HUDNotepad.Counter counter in this.m_AllCounters)
		{
			counter.obj.SetActive(false);
		}
	}

	public void OnCreateNotepad(Notepad notepad)
	{
		this.m_ItemsCounter.bookmark = notepad.m_ItemsTabCollider;
		this.m_StoryCounter.bookmark = notepad.m_StoryTabCollider;
		this.m_SkillsCounter.bookmark = notepad.m_SkillsTabCollider;
		this.m_ConstructionsCounter.bookmark = notepad.m_ConstructionsTabCollider;
		this.m_TrapsCounter.bookmark = notepad.m_TrapsTabCollider;
		this.m_FirecampCounter.bookmark = notepad.m_FirecampTabCollider;
		this.m_WaterConstructionsCounter.bookmark = notepad.m_WaterConstructionsTabCollider;
		this.m_HealingItemsCounter.bookmark = notepad.m_HealingItemsTabCollider;
		this.m_PlantsCounter.bookmark = notepad.m_PlantsTabCollider;
		this.m_CustomConstructionsCounter.bookmark = notepad.m_CustomConstructionsTabCollider;
		this.m_MudCounter.bookmark = notepad.m_MudBuildingsTabCollider;
		this.m_StoryObjectivesCounter.bookmark = notepad.m_StoryObjectivesTabCollider;
		this.m_NotepadCreated = true;
	}

	public void OnQuit()
	{
		NotepadController.Get().Hide();
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateCounters();
	}

	private void UpdateCounters()
	{
		if (!this.m_NotepadCreated)
		{
			return;
		}
		this.UpdateCounter(ref this.m_ItemsCounter, Vector3.right * this.m_ItemsCounter.bookmark.size.x * 0.5f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.ItemsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_StoryCounter, Vector3.right * this.m_StoryCounter.bookmark.size.x * 0.5f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.StoryTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_SkillsCounter, Vector3.right * this.m_SkillsCounter.bookmark.size.x * 0.5f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.SkillsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_ConstructionsCounter, Vector3.left * this.m_ConstructionsCounter.bookmark.size.x * 0.8f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.ConstructionsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_TrapsCounter, Vector3.left * this.m_TrapsCounter.bookmark.size.x * 0.7f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.TrapsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_FirecampCounter, Vector3.left * this.m_FirecampCounter.bookmark.size.x * 0.7f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.FirecampTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_WaterConstructionsCounter, Vector3.left * this.m_WaterConstructionsCounter.bookmark.size.x * 0.7f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.WaterConstructionsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_HealingItemsCounter, Vector3.right * this.m_HealingItemsCounter.bookmark.size.x * 0.5f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.HealingItemsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_PlantsCounter, Vector3.right * this.m_PlantsCounter.bookmark.size.x * 0.5f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.PlantsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_CustomConstructionsCounter, Vector3.left * this.m_CustomConstructionsCounter.bookmark.size.x * 0.6f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.CustomConstructionsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_MudCounter, Vector3.left * this.m_MudCounter.bookmark.size.x * 0.6f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.MudBuildingsTab].GetNewEntriesCount());
		this.UpdateCounter(ref this.m_StoryObjectivesCounter, Vector3.right * this.m_StoryObjectivesCounter.bookmark.size.x * 0.5f, MenuNotepad.Get().m_Tabs[MenuNotepad.MenuNotepadTab.StoryObjectivesTab].GetNewEntriesCount());
	}

	private void UpdateCounter(ref HUDNotepad.Counter counter, Vector3 shift, int count)
	{
		if (count == 0)
		{
			counter.obj.SetActive(false);
			return;
		}
		counter.obj.transform.position = Camera.main.WorldToScreenPoint(counter.bookmark.transform.TransformPoint(counter.bookmark.center + shift));
		counter.text.text = count.ToString();
		counter.obj.SetActive(true);
	}

	private bool m_Active;

	private HUDNotepad.Counter m_ItemsCounter;

	private HUDNotepad.Counter m_StoryCounter;

	private HUDNotepad.Counter m_SkillsCounter;

	private HUDNotepad.Counter m_ConstructionsCounter;

	private HUDNotepad.Counter m_TrapsCounter;

	private HUDNotepad.Counter m_MudCounter;

	private HUDNotepad.Counter m_FirecampCounter;

	private HUDNotepad.Counter m_WaterConstructionsCounter;

	private HUDNotepad.Counter m_HealingItemsCounter;

	private HUDNotepad.Counter m_PlantsCounter;

	private HUDNotepad.Counter m_CustomConstructionsCounter;

	private HUDNotepad.Counter m_StoryObjectivesCounter;

	private List<HUDNotepad.Counter> m_AllCounters = new List<HUDNotepad.Counter>();

	public GameObject m_CounterPrefab;

	private bool m_NotepadCreated;

	private static HUDNotepad s_Instance;

	private struct Counter
	{
		public GameObject obj;

		public Text text;

		public BoxCollider bookmark;
	}
}
