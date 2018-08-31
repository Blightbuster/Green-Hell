using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

public class HUDPlanner : HUDBase, IUIListExMouseDownReceiver
{
	public static HUDPlanner Get()
	{
		return HUDPlanner.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDPlanner.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override bool ShouldShow()
	{
		return !(NotepadController.Get() == null) && NotepadController.Get().IsActive() && MenuNotepad.Get().m_ActiveTab == MenuNotepad.MenuNotepadTab.PlannerTab;
	}

	protected override void OnShow()
	{
		base.OnShow();
		PlannerMode plannerMode = this.m_PlannerMode;
		if (plannerMode != PlannerMode.Planning)
		{
			if (plannerMode == PlannerMode.ReadOnly || plannerMode == PlannerMode.Summary)
			{
				this.m_List.gameObject.SetActive(false);
			}
		}
		else
		{
			this.m_List.gameObject.SetActive(true);
			this.FillList();
			this.m_List.m_MouseDownReceiver = this;
		}
		this.m_YesNoDialog.SetActive(false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (this.m_GoToSleep)
		{
			SleepController.Get().StartSleeping(this.m_RestingPlace, true);
			Player.Get().StartController(PlayerControllerType.Sleep);
			this.m_GoToSleep = false;
		}
		if (this.m_PlannerMode == PlannerMode.Summary)
		{
			for (int i = 0; i < Player.Get().GetComponent<PlayerPlannerModule>().m_PlannedTasks.Count; i++)
			{
				Player.Get().GetComponent<PlayerPlannerModule>().m_PlannedTasks[i].m_Fullfiled = false;
			}
			Player.Get().GetComponent<PlayerPlannerModule>().m_PlannedTasks.Clear();
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdatePointerOverList();
		if (this.m_DraggedObject != null)
		{
			if (Input.GetMouseButton(0))
			{
				this.m_DraggedObject.ui_element.transform.position = Input.mousePosition;
			}
			else if (this.m_PointerOverList)
			{
				UnityEngine.Object.Destroy(this.m_DraggedObject.ui_element);
				this.m_DraggedObject = null;
			}
			else
			{
				PlayerPlannerModule component = Player.Get().GetComponent<PlayerPlannerModule>();
				component.AddPlannedTask(component.m_AllTasks[this.m_DraggedObject.data]);
				this.m_List.m_Elements[this.m_DraggedObject.idx].show_delete_button = true;
				this.m_List.UpdateElements();
				UnityEngine.Object.Destroy(this.m_DraggedObject.ui_element);
				this.m_DraggedObject = null;
			}
		}
	}

	private void UpdatePointerOverList()
	{
		RectTransform component = this.m_List.GetComponent<RectTransform>();
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(component, Input.mousePosition, null, out zero))
		{
			zero2.x = CJTools.Math.GetProportional(0f, 1f, zero.x, -component.sizeDelta.x * 0.5f, component.sizeDelta.x * 0.5f);
			zero2.y = CJTools.Math.GetProportional(0f, 1f, zero.y, -component.sizeDelta.y * 0.5f, component.sizeDelta.y * 0.5f);
		}
		if (zero2.x > 0f && zero2.x < 1f && zero2.y > 0f && zero2.y < 1f)
		{
			this.m_PointerOverList = true;
		}
		else
		{
			this.m_PointerOverList = false;
		}
		Debug.Log("HUDPlanner::UpdatePointerOverList m_PointerOverList " + this.m_PointerOverList.ToString());
	}

	private void FillList()
	{
		this.m_List.Clear();
		PlayerPlannerModule component = Player.Get().GetComponent<PlayerPlannerModule>();
		for (int i = 0; i < component.m_AllTasks.Count; i++)
		{
			if (component.m_AllTasks[i].m_ShowInList)
			{
				if (component.m_PlannedTasks.Contains(component.m_AllTasks[i]))
				{
					this.m_List.AddElement(component.m_AllTasks[i].m_LocalizedText, i, true);
				}
				else
				{
					this.m_List.AddElement(component.m_AllTasks[i].m_LocalizedText, i, false);
				}
			}
		}
	}

	public void OnUIListExMouseDown(UIListEx list)
	{
	}

	public void OnUIListExMouseDownElement(UIListExElement elem)
	{
		PlayerPlannerModule component = Player.Get().GetComponent<PlayerPlannerModule>();
		for (int i = 0; i < component.m_PlannedTasks.Count; i++)
		{
			string localizedText = component.m_PlannedTasks[i].m_LocalizedText;
			if (localizedText == elem.ui_element.GetComponentInChildren<Text>().text)
			{
				return;
			}
		}
		GameObject ui_element = elem.ui_element;
		this.m_DraggedObject = new UIListExElement();
		this.m_DraggedObject.ui_element = UnityEngine.Object.Instantiate<GameObject>(ui_element);
		this.m_DraggedObject.data = elem.data;
		this.m_DraggedObject.idx = elem.idx;
		this.m_DraggedObject.text = elem.text;
		this.m_DraggedObject.ui_element.GetComponentInChildren<Text>().text = elem.ui_element.GetComponentInChildren<Text>().text;
		this.m_DraggedObject.ui_element.transform.parent = base.gameObject.transform;
		this.m_List.UpdateElements();
	}

	public void OnUIListExMouseDownDeleteButton(UIListExElement elem)
	{
		PlayerPlannerModule component = Player.Get().GetComponent<PlayerPlannerModule>();
		component.DeletePlannedTask(component.m_AllTasks[elem.data]);
	}

	public void OnOKButton()
	{
		this.ShowYesNoDialog();
	}

	public void ShowYesNoDialog()
	{
		this.m_YesNoDialog.SetActive(true);
		if (Player.Get().GetComponent<PlayerPlannerModule>().m_PlannedTasks.Count == 0)
		{
			this.m_YesNoDialogText.text = GreenHellGame.Instance.GetLocalization().Get("HUDPlannerYesNoDialogEmpty");
		}
		else
		{
			this.m_YesNoDialogText.text = GreenHellGame.Instance.GetLocalization().Get("HUDPlannerYesNoDialog");
		}
		this.m_YesText.text = GreenHellGame.Instance.GetLocalization().Get("Yes");
		this.m_NoText.text = GreenHellGame.Instance.GetLocalization().Get("No");
	}

	public void OnYesButton()
	{
		this.m_YesNoDialog.SetActive(false);
		Player.Get().StopController(PlayerControllerType.Notepad);
	}

	public void OnNoButton()
	{
		this.m_YesNoDialog.SetActive(false);
	}

	public UIListEx m_List;

	private UIListExElement m_DraggedObject;

	public bool m_PointerOverList;

	public PlannerMode m_PlannerMode = PlannerMode.ReadOnly;

	public static HUDPlanner s_Instance;

	public GameObject m_YesNoDialog;

	public Text m_YesNoDialogText;

	public Text m_YesText;

	public Text m_NoText;

	[HideInInspector]
	public bool m_GoToSleep;

	[HideInInspector]
	public int m_SleepDuration;

	[HideInInspector]
	public RestingPlace m_RestingPlace;
}
