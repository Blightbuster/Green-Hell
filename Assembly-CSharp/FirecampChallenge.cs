using System;
using System.Collections.Generic;
using UnityEngine;

public class FirecampChallenge : Challenge, IGhostObserver
{
	public override void Activate(GameObject parent)
	{
		this.m_Ghosts.Clear();
		this.m_Firecamps.Clear();
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			ConstructionGhost component = parent.transform.GetChild(i).gameObject.GetComponent<ConstructionGhost>();
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
				Firecamp component = created_item.gameObject.GetComponent<Firecamp>();
				DebugUtils.Assert(component, true);
				this.m_Firecamps.Add(component);
				break;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (this.m_Firecamps.Count < this.m_Count)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < this.m_Firecamps.Count; i++)
		{
			if (!this.m_Firecamps[i].m_Burning)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		string text = string.Empty;
		Localization localization = GreenHellGame.Instance.GetLocalization();
		for (int i = 0; i < this.m_Ghosts.Count; i++)
		{
			text += localization.Get("HUDFirecampChallenge_Firecamp", true);
			text = text + " " + (i + 1).ToString();
			text += " ";
			int num = 0;
			int num2 = 0;
			Player.Get().GetGPSCoordinates(this.m_Ghosts[i].transform.position, out num, out num2);
			text = string.Concat(new string[]
			{
				text,
				num.ToString(),
				"'W ",
				num2.ToString(),
				"'S"
			});
			text += "\n";
		}
		return text;
	}

	private List<ConstructionGhost> m_Ghosts = new List<ConstructionGhost>();

	private List<Firecamp> m_Firecamps = new List<Firecamp>();

	private int m_Count;
}
