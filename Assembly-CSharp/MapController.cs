using System;
using CJTools;
using Enums;
using UnityEngine;

public class MapController : PlayerController
{
	public static MapController Get()
	{
		return MapController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.Map;
		MapController.s_Instance = this;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.CreateMapObject();
		MenuNotepad.Get().gameObject.SetActive(true);
		this.m_Animator.SetBool(this.m_MapHash, true);
		this.PositionMap();
		this.m_PrevNotepadTab = MenuNotepad.Get().m_ActiveTab;
		if (MenuNotepad.Get().m_ActiveTab != MenuNotepad.MenuNotepadTab.MapTab)
		{
			MenuNotepad.Get().SetActiveTab(MenuNotepad.MenuNotepadTab.MapTab, false);
		}
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		this.m_Animator.SetBool(this.m_ZoomHash, false);
		HUDItem.Get().Deactivate();
		this.m_CanDisable = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.DestroyMapObject();
		MenuNotepad.Get().SetActiveTab(this.m_PrevNotepadTab, false);
		this.m_Animator.SetBool(this.m_MapHash, false);
		HUDMap.Get().Deactivate();
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
		}
		this.m_Animator.SetBool(this.m_ZoomHash, false);
		if (MenuNotepad.Get() != null)
		{
			MenuNotepad.Get().gameObject.SetActive(false);
		}
		Player.Get().OnHideMap();
	}

	private void CreateMapObject()
	{
		GameObject original = Resources.Load("Prefabs/TempPrefabs/Items/Item/map") as GameObject;
		this.m_Map = UnityEngine.Object.Instantiate<GameObject>(original);
		MenuNotepad.Get().m_NextMap = this.m_Map.transform.FindDeepChild("next_map").GetComponent<Collider>();
		MenuNotepad.Get().m_PrevMap = this.m_Map.transform.FindDeepChild("prev_map").GetComponent<Collider>();
		if (MenuNotepad.Get().m_NextMap)
		{
			Physics.IgnoreCollision(MenuNotepad.Get().m_NextMap, Player.Get().m_Collider);
		}
		if (MenuNotepad.Get().m_PrevMap)
		{
			Physics.IgnoreCollision(MenuNotepad.Get().m_PrevMap, Player.Get().m_Collider);
		}
		this.m_MapHolder = this.m_Map.transform.FindDeepChild("Holder");
		this.m_Map.gameObject.SetActive(false);
		this.m_MapPages[0] = this.m_Map.transform.FindDeepChild("map_page1").gameObject;
		this.m_MapPages[1] = this.m_Map.transform.FindDeepChild("map_page2").gameObject;
		this.m_MapPages[2] = this.m_Map.transform.FindDeepChild("map_page3").gameObject;
	}

	private void DestroyMapObject()
	{
		UnityEngine.Object.Destroy(this.m_Map);
		this.m_Map = null;
		this.m_MapHolder = null;
	}

	public void SetActivePage(int page)
	{
		for (int i = 0; i < 3; i++)
		{
			if (i == page)
			{
				this.m_MapPages[i].SetActive(true);
			}
			else
			{
				this.m_MapPages[i].SetActive(false);
			}
		}
	}

	public override void ControllerUpdate()
	{
		base.ControllerUpdate();
		bool flag = (GreenHellGame.IsPCControllerActive() && InputsManager.Get().IsActionActive(InputsManager.InputAction.RMB)) || (GreenHellGame.IsPadControllerActive() && InputsManager.Get().IsActionActive(InputsManager.InputAction.ZoomMap));
		if (flag != this.m_Animator.GetBool(this.m_ZoomHash))
		{
			this.m_Animator.SetBool(this.m_ZoomHash, flag);
		}
		if (this.m_CursorVisible && WatchController.Get().IsActive())
		{
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
			return;
		}
		if (!this.m_CursorVisible && !WatchController.Get().IsActive() && GreenHellGame.IsPCControllerActive())
		{
			CursorManager.Get().ShowCursor(true, true);
			this.m_CursorVisible = true;
		}
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.PositionMap();
	}

	private void PositionMap()
	{
		if (!this.m_Map)
		{
			return;
		}
		Transform rhand = this.m_Player.GetRHand();
		Rigidbody component = this.m_Map.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		Collider component2 = this.m_Map.GetComponent<Collider>();
		if (component2 != null)
		{
			component2.isTrigger = true;
			Physics.IgnoreCollision(this.m_Map.GetComponent<Collider>(), Player.Get().m_Collider);
		}
		Quaternion rhs = Quaternion.Inverse(this.m_MapHolder.localRotation);
		Vector3 b = this.m_MapHolder.parent.position - this.m_MapHolder.position;
		this.m_Map.transform.rotation = rhand.rotation;
		this.m_Map.transform.rotation *= rhs;
		this.m_Map.transform.position = rhand.position;
		this.m_Map.transform.position += b;
		this.m_Map.transform.parent = rhand;
	}

	public bool CanDisable()
	{
		return this.m_CanDisable;
	}

	public void Hide()
	{
		this.m_Animator.SetBool(this.m_MapHash, false);
		HUDMap.Get().Deactivate();
		if (this.m_CursorVisible)
		{
			CursorManager.Get().ShowCursor(false, false);
			this.m_CursorVisible = false;
		}
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.ShowMap)
		{
			this.m_Map.gameObject.SetActive(true);
			return;
		}
		if (id == AnimEventID.ShowMapEnd)
		{
			this.m_CanDisable = true;
			if (!this.m_CursorVisible && GreenHellGame.IsPCControllerActive())
			{
				CursorManager.Get().ShowCursor(true, false);
				this.m_CursorVisible = true;
			}
			HUDMap.Get().Activate();
			return;
		}
		if (id == AnimEventID.HideMapEnd)
		{
			this.Stop();
			return;
		}
		base.OnAnimEvent(id);
	}

	private int m_MapHash = Animator.StringToHash("Map");

	private int m_ZoomHash = Animator.StringToHash("MapZoom");

	private static MapController s_Instance;

	private GameObject m_Map;

	private Transform m_MapHolder;

	private bool m_CanDisable;

	private MenuNotepad.MenuNotepadTab m_PrevNotepadTab = MenuNotepad.MenuNotepadTab.ItemsTab;

	private bool m_CursorVisible;

	private const int m_NumMapPages = 3;

	private GameObject[] m_MapPages = new GameObject[3];
}
