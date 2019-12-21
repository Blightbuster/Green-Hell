using System;
using CJTools;
using Enums;
using UnityEngine;

public class NotepadController : PlayerController
{
	public static NotepadController Get()
	{
		return NotepadController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		base.m_ControllerType = PlayerControllerType.Notepad;
		NotepadController.s_Instance = this;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetInteger(this.m_NotepadHash, 1);
		this.m_Player.BlockRotation();
		this.m_Player.BlockMoves();
		HUDItem.Get().Deactivate();
		if (Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Deactivate();
		}
		this.m_CanDisable = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.Hide();
		this.DestroyNotepadObject();
		if (MenuNotepad.Get() != null)
		{
			MenuNotepad.Get().gameObject.SetActive(false);
		}
		this.m_Player.UnblockRotation();
		this.m_Player.UnblockMoves();
		HUDQuickAccessBar.Get().OnHideNotepad();
		Player.Get().OnHideNotepad();
		this.m_Animator.SetInteger(this.m_NotepadHash, 0);
		HUDNotepad.Get().Deactivate();
		CursorManager.Get().ShowCursor(false, false);
	}

	public bool CanDisable()
	{
		return this.m_CanDisable;
	}

	private void CreateNotepadObject()
	{
		GameObject original = Resources.Load("Prefabs/TempPrefabs/Items/Item/notebook") as GameObject;
		this.m_Notepad = UnityEngine.Object.Instantiate<GameObject>(original);
		this.m_NotepadHolder = this.m_Notepad.transform.FindDeepChild("Holder");
		Notepad component = this.m_Notepad.GetComponent<Notepad>();
		MenuNotepad.Get().SetNotepadObject(component);
		MenuNotepad.Get().gameObject.SetActive(true);
		HUDNotepad.Get().OnCreateNotepad(component);
	}

	private void DestroyNotepadObject()
	{
		UnityEngine.Object.Destroy(this.m_Notepad);
		this.m_Notepad = null;
		this.m_NotepadHolder = null;
	}

	public void Hide()
	{
		this.m_Animator.SetInteger(this.m_NotepadHash, 0);
		MenuNotepad.Get().OnNotepadHide();
		HUDNotepad.Get().Deactivate();
		CursorManager.Get().ShowCursor(false, false);
	}

	public override void ControllerLateUpdate()
	{
		base.ControllerLateUpdate();
		this.PositionNotepad();
	}

	private void PositionNotepad()
	{
		if (!this.m_Notepad)
		{
			return;
		}
		Transform rhand = this.m_Player.GetRHand();
		Rigidbody component = this.m_Notepad.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		Collider component2 = this.m_Notepad.GetComponent<Collider>();
		if (component2 != null)
		{
			component2.isTrigger = true;
		}
		Quaternion rhs = Quaternion.Inverse(this.m_NotepadHolder.localRotation);
		Vector3 b = this.m_NotepadHolder.parent.position - this.m_NotepadHolder.position;
		this.m_Notepad.transform.rotation = rhand.rotation;
		this.m_Notepad.transform.rotation *= rhs;
		this.m_Notepad.transform.position = rhand.position;
		this.m_Notepad.transform.position += b;
	}

	public override void OnAnimEvent(AnimEventID id)
	{
		if (id == AnimEventID.ShowNotebook)
		{
			this.CreateNotepadObject();
			return;
		}
		if (id == AnimEventID.ShowNotebookEnd)
		{
			this.m_CanDisable = true;
			HUDNotepad.Get().Activate();
			CursorManager.Get().ShowCursor(true, true);
			return;
		}
		if (id == AnimEventID.HideNotebookEnd)
		{
			this.Stop();
			return;
		}
		base.OnAnimEvent(id);
	}

	private int m_NotepadHash = Animator.StringToHash("Notepad");

	private GameObject m_Notepad;

	private Transform m_NotepadHolder;

	private static NotepadController s_Instance;

	private bool m_CanDisable;
}
