using System;
using UnityEngine;

public class BoatChallenge : Challenge, IGhostObserver
{
	public override void Load(Key key)
	{
		base.Load(key);
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "BoatGhost")
			{
				this.m_BoatGhostName = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "SuppliesGhost")
			{
				this.m_SuppliesGhostName = key2.GetVariable(0).SValue;
			}
		}
	}

	public override void Activate(GameObject parent)
	{
		this.m_Boat = null;
		this.m_Supplies = null;
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			GameObject gameObject = parent.transform.GetChild(i).gameObject;
			if (gameObject.name == this.m_BoatGhostName)
			{
				ConstructionGhost component = gameObject.GetComponent<ConstructionGhost>();
				this.m_Boat = component;
				this.m_Boat.RegisterObserver(this);
			}
			else if (gameObject.name == this.m_SuppliesGhostName)
			{
				ConstructionGhost component2 = gameObject.GetComponent<ConstructionGhost>();
				component2.gameObject.SetActive(false);
				this.m_Supplies = component2;
				this.m_Supplies.RegisterObserver(this);
			}
		}
		DebugUtils.Assert(this.m_Boat != null && this.m_Supplies != null, "Missing Boat ghost in challenge objects!", true, DebugUtils.AssertType.Info);
		base.Activate(parent);
		this.m_Supplies.gameObject.SetActive(false);
	}

	public void OnCreateConstruction(ConstructionGhost ghost, Item created_item)
	{
		if (ghost == this.m_Boat)
		{
			this.m_Supplies.gameObject.SetActive(true);
			base.NextObjective();
			return;
		}
		if (ghost == this.m_Supplies)
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		string text = GreenHellGame.Instance.GetLocalization().Get("HUDBoatChallenge_Boat", true);
		text += " ";
		int num = 0;
		int num2 = 0;
		Player.Get().GetGPSCoordinates(this.m_Boat.transform.position, out num, out num2);
		text = string.Concat(new string[]
		{
			text,
			num.ToString(),
			"'W ",
			num2.ToString(),
			"'S"
		});
		return text + "\n";
	}

	private ConstructionGhost m_Boat;

	private ConstructionGhost m_Supplies;

	private string m_BoatGhostName = string.Empty;

	private string m_SuppliesGhostName = string.Empty;
}
