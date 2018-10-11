using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class MenuNotepad : MonoBehaviour, ISaveLoad
{
	public static MenuNotepad Get()
	{
		return MenuNotepad.s_Instance;
	}

	private void Awake()
	{
		MenuNotepad.s_Instance = this;
		if (GreenHellGame.ROADSHOW_DEMO)
		{
			this.m_ActiveTab = MenuNotepad.MenuNotepadTab.FirecampTab;
		}
		else
		{
			this.m_ActiveTab = MenuNotepad.MenuNotepadTab.ConstructionsTab;
		}
		this.m_WasTabActivated = new bool[12];
		for (int i = 0; i < 12; i++)
		{
			this.m_WasTabActivated[i] = false;
		}
	}

	private void Start()
	{
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
	}

	private void Initialize()
	{
		List<NotepadTab> componentsDeepChild = General.GetComponentsDeepChild<NotepadTab>(base.gameObject);
		string[] names = Enum.GetNames(typeof(MenuNotepad.MenuNotepadTab));
		for (int i = 0; i < names.Length - 1; i++)
		{
			for (int j = 0; j < componentsDeepChild.Count; j++)
			{
				if (names[i] == componentsDeepChild[j].GetType().ToString())
				{
					MenuNotepad.MenuNotepadTab menuNotepadTab = (MenuNotepad.MenuNotepadTab)i;
					this.m_Tabs[menuNotepadTab] = componentsDeepChild[j];
					this.m_Tabs[menuNotepadTab].m_Tab = menuNotepadTab;
					if (menuNotepadTab == MenuNotepad.MenuNotepadTab.MapTab)
					{
						((MapTab)this.m_Tabs[menuNotepadTab]).InitMapsData();
					}
					break;
				}
			}
		}
		this.m_NoEntries = base.gameObject.transform.FindDeepChild("NoEntries").gameObject;
		this.m_NoEntries.SetActive(false);
		this.m_NoEntries.GetComponent<Text>().text = GreenHellGame.Instance.GetLocalization().Get("NoEntries");
		base.gameObject.SetActive(false);
		this.UpdatePrevNextButtons();
		this.InitializeSounds();
		this.m_Initialized = true;
	}

	private void InitializeSounds()
	{
		if (this.m_AudioSource == null)
		{
			this.m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			this.m_AudioSource.outputAudioMixerGroup = GreenHellGame.Instance.GetAudioMixerGroup(AudioMixerGroupGame.Player);
		}
		AudioClip item = Resources.Load("Sounds/Notepad/notepad_page_turn_01") as AudioClip;
		this.m_FlipPageSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_page_turn_02") as AudioClip);
		this.m_FlipPageSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_page_turn_03") as AudioClip);
		this.m_FlipPageSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_tab_switch_01") as AudioClip);
		this.m_SwitchTabSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_tab_switch_02") as AudioClip);
		this.m_SwitchTabSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_open_01") as AudioClip);
		this.m_ShowSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_open_02") as AudioClip);
		this.m_ShowSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_open_03") as AudioClip);
		this.m_ShowSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_close_01") as AudioClip);
		this.m_HideSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_close_02") as AudioClip);
		this.m_HideSounds.Add(item);
		item = (Resources.Load("Sounds/Notepad/notepad_close_03") as AudioClip);
		this.m_HideSounds.Add(item);
	}

	private void OnEnable()
	{
		if (!this.m_Initialized)
		{
			this.Initialize();
		}
		this.m_NotepadCanvasCursorPos = Vector2.one * 0.5f;
		this.UpdatePrevNextButtons();
		this.SetActiveTab(this.m_ActiveTab, false);
		this.SetProperPage();
		this.SetNoEntries();
		this.PlayShowSound();
	}

	private void OnDisable()
	{
		for (int i = 0; i < 12; i++)
		{
			this.m_Tabs[(MenuNotepad.MenuNotepadTab)i].gameObject.SetActive(false);
			this.m_Tabs[(MenuNotepad.MenuNotepadTab)i].OnHide();
		}
		if (this.m_SheduledTab != MenuNotepad.MenuNotepadTab.None)
		{
			this.SetActiveTab(this.m_SheduledTab, false);
			this.m_SheduledTab = MenuNotepad.MenuNotepadTab.None;
		}
		CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		this.m_MouseOverButton = false;
		this.m_MouseOverConstruction = false;
	}

	public void OnNotepadHide()
	{
		this.PlayHideSound();
	}

	public void AddStoryEvent(string element_name)
	{
		StoryTab storyTab = (StoryTab)this.m_Tabs[MenuNotepad.MenuNotepadTab.StoryTab];
		storyTab.AddStoryEvent(element_name);
		this.OnAddStoryEvent();
	}

	public void RemoveStoryEvent(string element_name)
	{
		StoryTab storyTab = (StoryTab)this.m_Tabs[MenuNotepad.MenuNotepadTab.StoryTab];
		storyTab.RemoveStoryEvent(element_name);
		this.OnRemoveStoryEvent();
	}

	public void OnAddStoryEvent()
	{
		this.UpdatePrevNextButtons();
		HUDInfoLog hudinfoLog = (HUDInfoLog)HUDManager.Get().GetHUD(typeof(HUDInfoLog));
		string title = GreenHellGame.Instance.GetLocalization().Get("MSG_Notepad_Story_NewEntry");
		this.SetActiveTab(MenuNotepad.MenuNotepadTab.StoryTab, true);
		hudinfoLog.AddInfo(title, string.Empty);
		PlayerAudioModule.Get().PlayNotepadEntrySound();
	}

	private void OnRemoveStoryEvent()
	{
		this.UpdatePrevNextButtons();
	}

	public void OnAddMapArea()
	{
		HUDMessages.Get().AddMessage(GreenHellGame.Instance.GetLocalization().Get("MSG_Map_New_Location"), null, HUDMessageIcon.None, string.Empty);
		PlayerAudioModule.Get().PlayNotepadEntrySound();
	}

	public void SetNotepadObject(Notepad notepad)
	{
		this.m_Notepad = notepad;
		if (this.m_Notepad != null)
		{
			this.m_MeshFilter = this.m_Notepad.gameObject.transform.FindDeepChild("notebook 1").GetComponent<MeshFilter>();
		}
		this.m_NotepadMesh = this.m_MeshFilter.mesh;
		this.m_NotepadMeshVertices = this.m_NotepadMesh.vertices;
		this.m_NotepadMeshTriangles = this.m_NotepadMesh.triangles;
		this.m_NotepadMeshUV2 = this.m_NotepadMesh.uv2;
	}

	private void Update()
	{
		this.UpdateInputs();
		this.UpdateCursorOnNotepadPosition();
		this.UpdateCursor();
	}

	private void UpdateCursorOnNotepadPosition()
	{
		if (this.m_Notepad == null)
		{
			return;
		}
		if (!this.m_MeshFilter)
		{
			return;
		}
		this.m_CameraRay.origin = this.m_MeshFilter.gameObject.transform.InverseTransformPoint(Camera.main.ScreenPointToRay(Input.mousePosition).origin);
		this.m_CameraRay.direction = this.m_MeshFilter.gameObject.transform.InverseTransformDirection(Camera.main.ScreenPointToRay(Input.mousePosition).direction);
		Vector3 zero = Vector3.zero;
		float num = float.MaxValue;
		int num2 = -1;
		Vector3 b = Vector3.zero;
		for (int i = 0; i < this.m_NotepadMeshTriangles.Length; i += 3)
		{
			this.m_Triangle.p0 = this.m_NotepadMeshVertices[this.m_NotepadMeshTriangles[i]];
			this.m_Triangle.p1 = this.m_NotepadMeshVertices[this.m_NotepadMeshTriangles[i + 1]];
			this.m_Triangle.p2 = this.m_NotepadMeshVertices[this.m_NotepadMeshTriangles[i + 2]];
			if (this.m_Triangle.Intersect(this.m_CameraRay, out zero))
			{
				float num3 = Vector3.Distance(this.m_CameraRay.origin, zero);
				if (num3 < num)
				{
					num = num3;
					num2 = i;
					this.m_ClosestTriangle.p0 = this.m_Triangle.p0;
					this.m_ClosestTriangle.p1 = this.m_Triangle.p1;
					this.m_ClosestTriangle.p2 = this.m_Triangle.p2;
					b = zero;
				}
			}
		}
		if (num2 < 0)
		{
			this.m_NotepadCanvasCursorPos = Vector2.one * 0.5f;
			this.m_CollisionFound = false;
			return;
		}
		Vector3 vector = this.m_ClosestTriangle.p0 - b;
		Vector3 vector2 = this.m_ClosestTriangle.p1 - b;
		Vector3 vector3 = this.m_ClosestTriangle.p2 - b;
		float magnitude = Vector3.Cross(this.m_ClosestTriangle.p0 - this.m_ClosestTriangle.p1, this.m_ClosestTriangle.p0 - this.m_ClosestTriangle.p2).magnitude;
		float d = Vector3.Cross(vector2, vector3).magnitude / magnitude;
		float d2 = Vector3.Cross(vector3, vector).magnitude / magnitude;
		float d3 = Vector3.Cross(vector, vector2).magnitude / magnitude;
		this.m_NotepadCanvasCursorPos = this.m_NotepadMeshUV2[this.m_NotepadMeshTriangles[num2]] * d + this.m_NotepadMeshUV2[this.m_NotepadMeshTriangles[num2 + 1]] * d2 + this.m_NotepadMeshUV2[this.m_NotepadMeshTriangles[num2 + 2]] * d3;
		this.m_CollisionFound = true;
	}

	private void UpdateInputs()
	{
		if (Camera.main == null)
		{
			return;
		}
		Ray ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / (float)Screen.width, Input.mousePosition.y / (float)Screen.height, 0f));
		RaycastHit[] array = Physics.RaycastAll(ray, 10f);
		this.m_MouseOverButton = false;
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider == this.m_PrevMap)
			{
				num = -1;
			}
			else if (array[i].collider == this.m_NextMap)
			{
				num = 1;
			}
		}
		if (num != 0)
		{
			if (Input.GetMouseButtonUp(0))
			{
				this.OnLMouseUp(MenuNotepad.MenuNotepadTab.MapTab, num);
			}
			this.m_MouseOverButton = true;
			return;
		}
		if (this.m_Notepad == null)
		{
			return;
		}
		MenuNotepad.MenuNotepadTab menuNotepadTab = MenuNotepad.MenuNotepadTab.None;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].collider == this.m_Notepad.m_StoryTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.StoryTab;
			}
			else if (array[j].collider == this.m_Notepad.m_SkillsTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.SkillsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_ItemsTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.ItemsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_ConstructionsTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.ConstructionsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_FirecampTabCollider)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.FirecampTab;
			}
			else if (array[j].collider == this.m_Notepad.m_TrapsTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.TrapsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_PlannerTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftShift))
				{
					HUDPlanner.Get().m_PlannerMode = PlannerMode.Planning;
				}
				else
				{
					HUDPlanner.Get().m_PlannerMode = PlannerMode.ReadOnly;
				}
				menuNotepadTab = MenuNotepad.MenuNotepadTab.PlannerTab;
			}
			else if (array[j].collider == this.m_Notepad.m_WaterConstructionsTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.WaterConstructionsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_HealingItemsTabCollider && !GreenHellGame.ROADSHOW_DEMO)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.HealingItemsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_PlantsTabCollider)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.PlantsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_CustomConstructionsTabCollider)
			{
				menuNotepadTab = MenuNotepad.MenuNotepadTab.CustomConstructionsTab;
			}
			else if (array[j].collider == this.m_Notepad.m_PrevPage || array[j].collider == this.m_PrevMap)
			{
				num = -1;
			}
			else if (array[j].collider == this.m_Notepad.m_NextPage || array[j].collider == this.m_NextMap)
			{
				num = 1;
			}
		}
		if (Input.GetMouseButtonUp(0) && (menuNotepadTab >= MenuNotepad.MenuNotepadTab.StoryTab || num != 0))
		{
			this.OnLMouseUp(menuNotepadTab, num);
		}
		this.m_MouseOverButton = (menuNotepadTab >= MenuNotepad.MenuNotepadTab.StoryTab || num != 0);
	}

	private void OnLMouseUp(MenuNotepad.MenuNotepadTab tab, int page)
	{
		if (tab == MenuNotepad.MenuNotepadTab.StoryTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.StoryTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.SkillsTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.SkillsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.ItemsTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.ItemsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.ConstructionsTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.ConstructionsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.FirecampTab)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.FirecampTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.TrapsTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.TrapsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.PlannerTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftShift))
			{
				HUDPlanner.Get().m_PlannerMode = PlannerMode.Planning;
			}
			else
			{
				HUDPlanner.Get().m_PlannerMode = PlannerMode.ReadOnly;
			}
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.PlannerTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.WaterConstructionsTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.WaterConstructionsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.HealingItemsTab && !GreenHellGame.ROADSHOW_DEMO)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.HealingItemsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.PlantsTab)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.PlantsTab, false);
			return;
		}
		if (tab == MenuNotepad.MenuNotepadTab.CustomConstructionsTab)
		{
			this.SetActiveTab(MenuNotepad.MenuNotepadTab.CustomConstructionsTab, false);
			return;
		}
		if (page == -1)
		{
			this.SetPrevPage();
			return;
		}
		if (page == 1)
		{
			this.SetNextPage();
			return;
		}
	}

	public void SetActiveTab(MenuNotepad.MenuNotepadTab tab, bool only_if_disabled = false)
	{
		if (only_if_disabled && base.enabled && base.gameObject.activeSelf)
		{
			this.m_SheduledTab = tab;
			return;
		}
		if (tab != this.m_ActiveTab)
		{
			this.PlaySwitchTabSound();
		}
		for (int i = 0; i < 12; i++)
		{
			if (tab == (MenuNotepad.MenuNotepadTab)i)
			{
				this.m_Tabs[(MenuNotepad.MenuNotepadTab)i].gameObject.SetActive(true);
				this.m_ActiveTab = tab;
			}
			else
			{
				this.m_Tabs[(MenuNotepad.MenuNotepadTab)i].gameObject.SetActive(false);
				this.m_Tabs[(MenuNotepad.MenuNotepadTab)i].OnHide();
			}
		}
		this.m_Tabs[tab].OnShow();
		this.UpdatePrevNextButtons();
		if (base.enabled && base.gameObject.activeSelf)
		{
			this.m_WasTabActivated[(int)tab] = true;
		}
		if (this.m_Notepad != null)
		{
			this.m_Notepad.SetActiveTab(tab);
		}
		this.OnSetActiveTab();
	}

	private void OnSetActiveTab()
	{
		this.SetProperPage();
		this.SetNoEntries();
	}

	public void UpdatePrevNextButtons()
	{
		if (this.m_Notepad == null)
		{
			return;
		}
		NotepadTab notepadTab = this.m_Tabs[this.m_ActiveTab];
		if (notepadTab.GetCurrentPage() == 0)
		{
			this.m_Notepad.EnablePrevPage(false);
			if (notepadTab.GetNumActivePages() > 2)
			{
				this.m_Notepad.EnableNextPage(true);
			}
			else
			{
				this.m_Notepad.EnableNextPage(false);
			}
		}
		else
		{
			this.m_Notepad.EnablePrevPage(true);
			if (notepadTab.GetNumActivePages() > (notepadTab.GetCurrentPage() + 1) * 2)
			{
				this.m_Notepad.EnableNextPage(true);
			}
			else
			{
				this.m_Notepad.EnableNextPage(false);
			}
		}
	}

	private void SetPrevPage()
	{
		NotepadTab notepadTab = this.m_Tabs[this.m_ActiveTab];
		notepadTab.SetPrevPage();
		this.UpdatePrevNextButtons();
		this.PlayFlipPageSound();
	}

	private void SetNextPage()
	{
		NotepadTab notepadTab = this.m_Tabs[this.m_ActiveTab];
		notepadTab.SetNextPage();
		this.UpdatePrevNextButtons();
		this.PlayFlipPageSound();
	}

	public bool WasTabActivated(string tab_name)
	{
		MenuNotepad.MenuNotepadTab menuNotepadTab = (MenuNotepad.MenuNotepadTab)Enum.Parse(typeof(MenuNotepad.MenuNotepadTab), tab_name);
		return this.m_WasTabActivated[(int)menuNotepadTab];
	}

	public bool IsTabActive(string tab_name)
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			MenuNotepad.MenuNotepadTab menuNotepadTab = (MenuNotepad.MenuNotepadTab)Enum.Parse(typeof(MenuNotepad.MenuNotepadTab), tab_name);
			return this.m_ActiveTab == menuNotepadTab;
		}
		return false;
	}

	public void Save()
	{
		for (int i = 0; i < 12; i++)
		{
			SaveGame.SaveVal("NotepadWasActive" + i, this.m_WasTabActivated[i]);
		}
	}

	public void Load()
	{
		for (int i = 0; i < 12; i++)
		{
			this.m_WasTabActivated[i] = SaveGame.LoadBVal("NotepadWasActive" + i);
		}
	}

	public void SetCurrentPageToItem(ItemID id)
	{
		ItemsTab component = this.m_Tabs[this.m_ActiveTab].GetComponent<ItemsTab>();
		if (component != null)
		{
			component.SetCurrentPageToItem(id);
		}
	}

	private void PlayShowSound()
	{
		if (this.m_ShowSounds.Count > 0)
		{
			AudioClip audioClip = this.m_ShowSounds[UnityEngine.Random.Range(0, this.m_ShowSounds.Count)];
			if (audioClip)
			{
				this.m_AudioSource.PlayOneShot(audioClip);
			}
		}
	}

	private void PlayHideSound()
	{
		if (this.m_HideSounds.Count > 0)
		{
			AudioClip audioClip = this.m_HideSounds[UnityEngine.Random.Range(0, this.m_HideSounds.Count)];
			if (audioClip)
			{
				this.m_AudioSource.PlayOneShot(audioClip);
			}
		}
	}

	private void PlaySwitchTabSound()
	{
		if (this.m_SwitchTabSounds.Count > 0)
		{
			AudioClip audioClip = this.m_SwitchTabSounds[UnityEngine.Random.Range(0, this.m_SwitchTabSounds.Count)];
			if (audioClip)
			{
				this.m_AudioSource.PlayOneShot(audioClip);
			}
		}
	}

	private void PlayFlipPageSound()
	{
		if (this.m_FlipPageSounds.Count > 0)
		{
			AudioClip audioClip = this.m_FlipPageSounds[UnityEngine.Random.Range(0, this.m_FlipPageSounds.Count)];
			if (audioClip)
			{
				this.m_AudioSource.PlayOneShot(audioClip);
			}
		}
	}

	private void SetProperPage()
	{
		this.m_Tabs[this.m_ActiveTab].SetProperPage(0);
		this.UpdatePrevNextButtons();
	}

	private void SetNoEntries()
	{
		if (this.m_Tabs[this.m_ActiveTab].ShouldShowNoEntries())
		{
			this.m_NoEntries.SetActive(true);
		}
		else
		{
			this.m_NoEntries.SetActive(false);
		}
	}

	private void UpdateCursor()
	{
		if (this.m_ActiveTab == MenuNotepad.MenuNotepadTab.MapTab)
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		}
		else if (this.m_MouseOverConstruction)
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.Hammer);
		}
		else if (this.m_MouseOverButton)
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.MouseOver);
		}
		else if (!HUDNewWheel.Get().IsSelected())
		{
			CursorManager.Get().SetCursor(CursorManager.TYPE.Normal);
		}
	}

	private Dictionary<MenuNotepad.MenuNotepadTab, NotepadTab> m_Tabs = new Dictionary<MenuNotepad.MenuNotepadTab, NotepadTab>();

	public static MenuNotepad s_Instance;

	private Notepad m_Notepad;

	[HideInInspector]
	[NonSerialized]
	public MenuNotepad.MenuNotepadTab m_ActiveTab = MenuNotepad.MenuNotepadTab.ConstructionsTab;

	private MenuNotepad.MenuNotepadTab m_SheduledTab = MenuNotepad.MenuNotepadTab.None;

	private bool m_Initialized;

	private MeshFilter m_MeshFilter;

	private Ray m_CameraRay = default(Ray);

	private Triangle m_Triangle = new Triangle();

	private Triangle m_ClosestTriangle = new Triangle();

	private Mesh m_NotepadMesh;

	private Vector3[] m_NotepadMeshVertices;

	private int[] m_NotepadMeshTriangles;

	private Vector2[] m_NotepadMeshUV2;

	public Vector2 m_NotepadCanvasCursorPos = Vector2.zero;

	public bool m_CollisionFound;

	private bool[] m_WasTabActivated;

	private AudioSource m_AudioSource;

	private List<AudioClip> m_ShowSounds = new List<AudioClip>();

	private List<AudioClip> m_HideSounds = new List<AudioClip>();

	private List<AudioClip> m_SwitchTabSounds = new List<AudioClip>();

	private List<AudioClip> m_FlipPageSounds = new List<AudioClip>();

	public GameObject m_NoEntries;

	[HideInInspector]
	public bool m_MouseOverConstruction;

	[HideInInspector]
	public bool m_MouseOverButton;

	[HideInInspector]
	public Collider m_NextMap;

	[HideInInspector]
	public Collider m_PrevMap;

	public enum MenuNotepadTab
	{
		None = -1,
		StoryTab,
		MapTab,
		ItemsTab,
		ConstructionsTab,
		FirecampTab,
		TrapsTab,
		PlannerTab,
		SkillsTab,
		WaterConstructionsTab,
		HealingItemsTab,
		PlantsTab,
		CustomConstructionsTab,
		Count
	}
}
