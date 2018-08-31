using System;
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
				this.m_Create.SetActive(true);
			}
			if (this.m_Find.gameObject.activeSelf)
			{
				this.m_Find.gameObject.SetActive(false);
			}
		}
		else if (ConstructionController.Get() && ConstructionController.Get().GetGhost())
		{
			if (this.m_Create.activeSelf)
			{
				this.m_Create.SetActive(false);
			}
			if (!this.m_Find.gameObject.activeSelf)
			{
				this.m_Find.gameObject.SetActive(true);
			}
			if (ConstructionController.Get().GetGhost().GetProhibitionType() == ConstructionGhost.ProhibitionType.Hard)
			{
				ConstructionGhost.GhostPlacingCondition placingCondition = ConstructionController.Get().GetGhost().m_PlacingCondition;
				if (placingCondition == ConstructionGhost.GhostPlacingCondition.NeedFirecamp)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindFire");
				}
				else if (placingCondition == ConstructionGhost.GhostPlacingCondition.MustBeInWater)
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindWater");
				}
				else
				{
					this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindPlace");
				}
			}
			else if (ConstructionController.Get().GetGhost().GetProhibitionType() == ConstructionGhost.ProhibitionType.Soft)
			{
				this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HIDConstruction_CutPlants");
			}
			else
			{
				this.m_Find.text = GreenHellGame.Instance.GetLocalization().Get("HUDConstruction_FindPlace");
			}
		}
	}

	public GameObject m_Create;

	public Text m_Find;
}
