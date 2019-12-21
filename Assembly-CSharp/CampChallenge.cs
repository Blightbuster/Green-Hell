using System;
using System.Collections.Generic;
using UnityEngine;

public class CampChallenge : Challenge, IGhostObserver, IConstructionObserver
{
	public override void Activate(GameObject parent)
	{
		this.m_Data.Clear();
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			ConstructionGhost component = parent.transform.GetChild(i).gameObject.GetComponent<ConstructionGhost>();
			if (component)
			{
				component.m_DestroyOnCreateConstruction = false;
				component.RegisterObserver(this);
				this.m_Data.Add(component, null);
			}
		}
		this.m_Count = this.m_Data.Count;
		base.Activate(parent);
	}

	public void OnCreateConstruction(ConstructionGhost ghost, Item created_item)
	{
		foreach (ConstructionGhost y in this.m_Data.Keys)
		{
			if (ghost == y)
			{
				Construction component = created_item.gameObject.GetComponent<Construction>();
				DebugUtils.Assert(component, true);
				this.m_Data[ghost] = component;
				HUDChallengeInfo.Get().SetupText(this);
				component.RegisterObserver(this);
				break;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		bool flag = true;
		foreach (ConstructionGhost key in this.m_Data.Keys)
		{
			if (this.m_Data[key] == null)
			{
				flag = false;
			}
		}
		if (flag)
		{
			this.Success();
		}
	}

	public override string GetLocalizedInfo()
	{
		Localization localization = GreenHellGame.Instance.GetLocalization();
		string text = localization.Get("HUDCampChallenge_Camp", true);
		text += " ";
		float num = 0f;
		float num2 = float.MaxValue;
		float num3 = 0f;
		float num4 = float.MaxValue;
		foreach (ConstructionGhost constructionGhost in this.m_Data.Keys)
		{
			if (!(this.m_Data[constructionGhost] != null))
			{
				Vector3 position = constructionGhost.transform.position;
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
		}
		Vector3 zero = Vector3.zero;
		zero.x = num4 + (num3 - num4) * 0.5f;
		zero.z = num2 + (num - num2) * 0.5f;
		int num5 = 0;
		int num6 = 0;
		Player.Get().GetGPSCoordinates(zero, out num5, out num6);
		text = string.Concat(new string[]
		{
			text,
			num5.ToString(),
			"'W ",
			num6.ToString(),
			"'S"
		});
		text += "\n";
		foreach (ConstructionGhost constructionGhost2 in this.m_Data.Keys)
		{
			if (!(this.m_Data[constructionGhost2] != null))
			{
				text += localization.Get(constructionGhost2.m_ResultItemID.ToString(), true);
				text += "\n";
			}
		}
		return text;
	}

	public void OnDestroyConstruction(Construction con)
	{
		foreach (ConstructionGhost constructionGhost in this.m_Data.Keys)
		{
			if (this.m_Data[constructionGhost] == con && !constructionGhost.gameObject.activeSelf)
			{
				constructionGhost.Reset();
				constructionGhost.gameObject.SetActive(true);
				HUDChallengeInfo.Get().SetupText(this);
			}
		}
		con.UnregisterObserver(this);
	}

	private Dictionary<ConstructionGhost, Construction> m_Data = new Dictionary<ConstructionGhost, Construction>();

	private int m_Count;
}
