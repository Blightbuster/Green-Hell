using System;
using UnityEngine;

public class ConstructionController : PlayerController
{
	public static ConstructionController Get()
	{
		return ConstructionController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		ConstructionController.s_Instance = this;
		this.m_ControllerType = PlayerControllerType.Construction;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.CreateGhostItem();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (this.m_Ghost && this.m_Ghost.m_State == ConstructionGhost.GhostState.Dragging)
		{
			UnityEngine.Object.Destroy(this.m_Ghost.gameObject);
		}
	}

	public void SetupPrefab(ItemInfo info)
	{
		string text = info.m_ID.ToString() + "Ghost";
		this.m_GhostPrefab = GreenHellGame.Instance.GetPrefab(text);
		if (!this.m_GhostPrefab)
		{
			DebugUtils.Assert("[ConstructionController:SetupPrefab] ERROR - Can't find ghost prefab - " + text, true, DebugUtils.AssertType.Info);
		}
	}

	public override void OnInputAction(InputsManager.InputAction action)
	{
		if (this.m_Player.GetRotationBlocked())
		{
			return;
		}
		if (action == InputsManager.InputAction.CreateConstruction && this.CanCreateConstruction())
		{
			this.m_Ghost.SetState(ConstructionGhost.GhostState.Building);
			this.Stop();
		}
		else if (action == InputsManager.InputAction.Quit || action == InputsManager.InputAction.AdditionalQuit)
		{
			if (!this.m_Ghost.m_IsBeingDestroyed)
			{
				UnityEngine.Object.Destroy(this.m_Ghost.gameObject);
			}
			this.Stop();
		}
	}

	private void CreateGhostItem()
	{
		DebugUtils.Assert(this.m_GhostPrefab != null, true);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_GhostPrefab, this.m_Player.transform.position, Quaternion.identity);
		gameObject.name = this.m_GhostPrefab.name;
		this.m_Ghost = gameObject.GetComponent<ConstructionGhost>();
		this.m_Ghost.SetState(ConstructionGhost.GhostState.Dragging);
	}

	public bool CanCreateConstruction()
	{
		return this.m_Ghost != null && this.m_Ghost.GetState() == ConstructionGhost.GhostState.Dragging && this.m_Ghost.CanBePlaced();
	}

	public ConstructionGhost GetGhost()
	{
		return this.m_Ghost;
	}

	private ConstructionGhost m_Ghost;

	private GameObject m_GhostPrefab;

	private static ConstructionController s_Instance;
}
