using System;
using System.Collections.Generic;
using UnityEngine;

public class CampChallenge : Challenge, IGhostObserver
{
	public override void Activate(GameObject parent)
	{
		this.m_Ghosts.Clear();
		this.m_Constructions.Clear();
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			GameObject gameObject = parent.transform.GetChild(i).gameObject;
			ConstructionGhost component = gameObject.GetComponent<ConstructionGhost>();
			if (component)
			{
				component.RegisterObserver(this);
				this.m_Ghosts.Add(component);
			}
		}
		this.m_Count = this.m_Ghosts.Count;
		base.Activate(parent);
	}

	public void OnCreateConstruction(ConstructionGhost ghost, Item created_item)
	{
		foreach (ConstructionGhost y in this.m_Ghosts)
		{
			if (ghost == y)
			{
				Construction component = created_item.gameObject.GetComponent<Construction>();
				DebugUtils.Assert(component, true);
				this.m_Constructions.Add(component);
				break;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (this.m_Constructions.Count == this.m_Count)
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		string text = localization.Get("HUDCampChallenge_Camp");
		text += " ";
		float num = 0f;
		float num2 = float.MaxValue;
		float num3 = 0f;
		float num4 = float.MaxValue;
		for (int i = 0; i < this.m_Ghosts.Count; i++)
		{
			Vector3 position = this.m_Ghosts[i].transform.position;
			if (position.x > num3)
			{
				num3 = position.x;
			}
			if (position.x < num4)
			{
				num4 = position.x;
			}
			if (position.z > num)
			{
				num = position.z;
			}
			if (position.z < num2)
			{
				num2 = position.z;
			}
		}
		Vector3 zero = Vector3.zero;
		zero.x = num4 + (num3 - num4) * 0.5f;
		zero.z = num2 + (num - num2) * 0.5f;
		int num5 = 0;
		int num6 = 0;
		Player.Get().GetGPSCoordinates(zero, out num5, out num6);
		string text2 = text;
		text = string.Concat(new string[]
		{
			text2,
			num5.ToString(),
			"'W ",
			num6.ToString(),
			"'S"
		});
		text += "\n";
		for (int j = 0; j < this.m_Ghosts.Count; j++)
		{
			text += localization.Get(this.m_Ghosts[j].m_ResultItemID.ToString());
			text += "\n";
		}
		return text;
	}

	private List<ConstructionGhost> m_Ghosts = new List<ConstructionGhost>();

	private List<Construction> m_Constructions = new List<Construction>();

	private int m_Count;
}
