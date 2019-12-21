using System;
using Enums;
using UnityEngine;
using UnityEngine.UI;

public class HUDConstruction : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override bool ShouldShow()
	{
		if (!ConstructionController.Get().IsActive())
		{
			return false;
		}
		ConstructionGhost ghost = ConstructionController.Get().GetGhost();
		return ghost != null && ghost.GetState() == ConstructionGhost.GhostState.Dragging;
	}

	protected override void OnShow()
	{
		base.OnShow();
		this.SetupText(ConstructionController.Get().CanCreateConstruction());
	}

	protected override void Update()
	{
		base.Update();
		this.SetupText(ConstructionController.Get().CanCreateConstruction());
	}

	private void SetupText(bool can_create)
	{
		if (can_create)
		{
			if (!this.m_Create.activeSelf)
			{
				this.m_Create.SetActive(GreenHellGame.IsPCControllerActive());
			}
			if (!this.m_CreatePad.activeSelf)
			{
				this.m_CreatePad.SetActive(GreenHellGame.IsPadControllerActive());
			}
			if (this.m_Find.gameObject.activeSelf)
			{
				this.m_Find.gameObject.SetActive(false);
				return;
			}
		}
		else if (ConstructionController.Get() && ConstructionController.Get().GetGhost())
		{
			if (this.m_Create.activeSelf)
			{
				this.m_Create.SetActive(false);
			}
			if (this.m_CreatePad.activeSelf)
			{
				this.m_CreatePad.SetActive(false);
			}
			if (!this.m_Find.gameObject.activeSelf)
			{
				this.m_Find.gameObject.SetActive(true);
			}
			this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindPlace", true);
			if (ConstructionController.Get().GetGhost().GetProhibitionType() == ConstructionGhost.ProhibitionType.Depth)
			{
				this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindPlace", true);
				return;
			}
			if (ConstructionController.Get().GetGhost().GetProhibitionType() == ConstructionGhost.ProhibitionType.Hard)
			{
				ConstructionGhost.GhostPlacingCondition placingCondition = ConstructionController.Get().GetGhost().m_PlacingCondition;
				if (placingCondition == ConstructionGhost.GhostPlacingCondition.NeedFirecamp)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindFire", true);
					return;
				}
				if (placingCondition == ConstructionGhost.GhostPlacingCondition.MustBeInWater)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindWater", true);
					return;
				}
				if (placingCondition != ConstructionGhost.GhostPlacingCondition.IsSnapped)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindPlace", true);
					return;
				}
				ItemID resultItemID = ConstructionController.Get().GetGhost().m_ResultItemID;
				if (resultItemID == ItemID.mud_wall || resultItemID == ItemID.mud_doorway || resultItemID == ItemID.mud_window_wall || resultItemID == ItemID.mud_wall_fireside)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_MudSnap", true);
					return;
				}
				if (resultItemID == ItemID.building_wall || resultItemID == ItemID.building_bamboo_wall || resultItemID == ItemID.building_shed || resultItemID == ItemID.building_bamboo_shed || resultItemID == ItemID.wooden_doorway || resultItemID == ItemID.bamboo_doorway || resultItemID == ItemID.building_banana_leaf_roof || resultItemID == ItemID.mud_ceiling || resultItemID == ItemID.building_roof)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_Snap", true);
					return;
				}
				if (resultItemID == ItemID.building_shed_roof || resultItemID == ItemID.building_banana_shed_roof || resultItemID == ItemID.mud_shed_wall || resultItemID == ItemID.mud_shed_ceiling)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_ShedSnap", true);
					return;
				}
			}
			else
			{
				if (ConstructionController.Get().GetGhost().GetProhibitionType() == ConstructionGhost.ProhibitionType.Soft)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HIDConstruction_CutPlants", true);
					return;
				}
				this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindPlace", true);
			}
		}
	}

	public GameObject m_Create;

	public GameObject m_CreatePad;

	public Text m_Find;
}
