using System;
using System.Collections.Generic;
using UnityEngine;

public class Challenge
{
	public virtual void Load(Key key)
	{
		this.m_Name = key.GetVariable(0).SValue;
		string str = string.Empty;
		for (int i = 0; i < key.GetKeysCount(); i++)
		{
			Key key2 = key.GetKey(i);
			if (key2.GetName() == "NameID")
			{
				this.m_NameID = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "DescriptionID")
			{
				this.m_DescriptionID = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "Icon")
			{
				str = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "Object")
			{
				this.m_ParentName = key2.GetVariable(0).SValue;
			}
			else if (key2.GetName() == "Duration")
			{
				this.m_Duration = key2.GetVariable(0).FValue;
			}
			else if (key2.GetName() == "Objective")
			{
				this.m_Objectives.Add(key2.GetVariable(0).SValue);
			}
			else if (key2.GetName() == "StartDate")
			{
				this.m_StartDate = new DateTime(key2.GetVariable(0).IValue, key2.GetVariable(1).IValue, key2.GetVariable(2).IValue, key2.GetVariable(3).IValue, key2.GetVariable(4).IValue, 0);
			}
		}
		this.m_EndDate = this.m_StartDate.AddHours((double)this.m_Duration);
		this.m_Icon = Resources.Load<Sprite>("HUD/Challenges/" + str);
		DebugUtils.Assert(this.m_Icon != null, "Missing challenge icon - " + str + ", challenge - " + this.m_Name, true, DebugUtils.AssertType.Info);
	}

	public virtual void Activate(GameObject parent)
	{
		this.m_Parent = parent;
		this.m_Parent.SetActive(true);
		this.m_ObjectiveIndex = 0;
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			GameObject gameObject = parent.transform.GetChild(i).gameObject;
			gameObject.SetActive(true);
			if (gameObject.name == "Start")
			{
				Player.Get().Teleport(gameObject, false);
			}
		}
		if (this.m_Objectives.Count > this.m_ObjectiveIndex)
		{
			ObjectivesManager.Get().ActivateObjective(this.m_Objectives[this.m_ObjectiveIndex], true);
		}
		MainLevel.Instance.m_TODSky.Cycle.DateTime = this.m_StartDate;
		MainLevel.Instance.m_TODSky.Cycle.GameTime = 0f;
		HUDChallengeTimer.Get().Activate(this);
		HUDChallengeInfo.Get().Activate(this);
	}

	public virtual void Update()
	{
		if (MainLevel.Instance.m_TODSky.Cycle.GameTime > this.m_Duration)
		{
			this.Fail();
		}
	}

	protected virtual void Success()
	{
		this.m_CurrentScore = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		if (this.m_BestScore < 0f || this.m_CurrentScore < this.m_BestScore)
		{
			this.m_BestScore = this.m_CurrentScore;
		}
		this.OnFinish(true);
	}

	public virtual void Fail()
	{
		this.m_CurrentScore = MainLevel.Instance.m_TODSky.Cycle.GameTime;
		this.OnFinish(false);
	}

	protected void NextObjective()
	{
		if (this.m_Objectives.Count > this.m_ObjectiveIndex)
		{
			ObjectivesManager.Get().DeactivateObjective(this.m_Objectives[this.m_ObjectiveIndex]);
		}
		this.m_ObjectiveIndex++;
		if (this.m_Objectives.Count > this.m_ObjectiveIndex)
		{
			ObjectivesManager.Get().ActivateObjective(this.m_Objectives[this.m_ObjectiveIndex], true);
		}
	}

	private void OnFinish(bool success)
	{
		if (this.m_Objectives.Count > this.m_ObjectiveIndex)
		{
			ObjectivesManager.Get().DeactivateObjective(this.m_Objectives[this.m_ObjectiveIndex]);
		}
		HUDChallengeTimer.Get().Deactivate();
		HUDChallengeInfo.Get().Deactivate();
		HUDChallengeResult.Get().Activate(success, this);
		Player.Get().BlockMoves();
		Player.Get().BlockRotation();
		ChallengesManager.Get().OnFinishChallenge(success);
	}

	public virtual string GetLocalizedInfo()
	{
		return string.Empty;
	}

	public virtual bool UpdateHUDChallengeInfo()
	{
		return false;
	}

	public float m_BestScore = -1f;

	public float m_CurrentScore;

	public string m_Name = string.Empty;

	public string m_NameID = string.Empty;

	public string m_DescriptionID = string.Empty;

	public string m_ParentName = string.Empty;

	public float m_Duration;

	private List<string> m_Objectives = new List<string>();

	private int m_ObjectiveIndex;

	public Sprite m_Icon;

	public DateTime m_StartDate;

	public DateTime m_EndDate;

	public int m_EndDay;

	public int m_EndMonth;

	public float m_EndHour;

	private GameObject m_Parent;
}
