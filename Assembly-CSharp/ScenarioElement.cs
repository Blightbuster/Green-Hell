using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioElement
{
	public virtual void Setup()
	{
	}

	public ScenarioElement.State GetActionState()
	{
		return this.m_State;
	}

	public virtual void Activate()
	{
		this.m_State = ScenarioElement.State.InProgress;
	}

	public void Complete()
	{
		this.m_State = ScenarioElement.State.Completed;
	}

	public void Reset()
	{
		this.m_State = ScenarioElement.State.None;
	}

	public bool IsState(ScenarioElement.State state)
	{
		return this.m_State == state;
	}

	public void Update()
	{
		if (this.ShouldComplete())
		{
			this.Complete();
		}
	}

	protected virtual bool ShouldComplete()
	{
		return false;
	}

	public virtual bool IsCondition()
	{
		return false;
	}

	public virtual bool IsAction()
	{
		return false;
	}

	public virtual void Save(ScenarioNode node)
	{
		SaveGame.SaveVal(node.m_Name + "_" + this.m_ID, (int)this.m_State);
	}

	public virtual void Load(ScenarioNode node, int index)
	{
		if (node.m_State == ScenarioNode.State.Completed)
		{
			this.m_State = ScenarioElement.State.Completed;
			return;
		}
		if (SaveGame.m_SaveGameVersion >= GreenHellGame.s_GameVersionEarlyAccessUpdate9 && this.m_ID >= 0)
		{
			this.m_State = (ScenarioElement.State)SaveGame.LoadIVal(node.m_Name + "_" + this.m_ID);
			return;
		}
		this.m_State = (ScenarioElement.State)SaveGame.LoadIVal(node.m_Name + "_" + index);
	}

	public virtual void PostLoad()
	{
	}

	public virtual void OnSceneLoaded()
	{
		if (this.m_HasNullObject)
		{
			this.CheckObjects(null);
		}
	}

	public virtual void OnSceneUnload(Scene scene)
	{
		if (this.m_ParamO1 && this.m_ParamO1.scene == scene)
		{
			this.m_ParamO1 = null;
			this.m_HasNullObject = true;
		}
		if (this.m_ParamO2 && this.m_ParamO2.scene == scene)
		{
			this.m_ParamO2 = null;
			this.m_HasNullObject = true;
		}
	}

	public virtual bool CheckObjects(GameObject go = null)
	{
		if (!this.m_HasNullObject)
		{
			return true;
		}
		bool flag = !this.m_IsGO1;
		if (this.m_IsGO1)
		{
			if (go == null)
			{
				this.m_ParamO1 = MainLevel.Instance.GetUniqueObject(this.m_ParamO1Name);
				flag = (this.m_ParamO1 != null);
			}
			else if (go.name == this.m_ParamO1Name)
			{
				this.m_ParamO1 = go;
				flag = true;
			}
		}
		bool flag2 = !this.m_IsGO2;
		if (this.m_IsGO2)
		{
			if (go == null)
			{
				this.m_ParamO2 = MainLevel.Instance.GetUniqueObject(this.m_ParamO2Name);
				flag2 = (this.m_ParamO2 != null);
			}
			else if (go.name == this.m_ParamO2Name)
			{
				this.m_ParamO2 = go;
				flag2 = true;
			}
		}
		this.m_HasNullObject = (!flag || !flag2);
		return !this.m_HasNullObject;
	}

	public ScenarioSyntaxData m_ScenarioSyntaxData;

	public string m_Content = string.Empty;

	public string m_EncodedContent = string.Empty;

	public ScenarioElement.State m_State;

	protected string m_Param0 = string.Empty;

	protected string m_Param1 = string.Empty;

	protected bool m_IsGO1;

	protected bool m_IsGO2;

	protected string m_ParamO1Name;

	protected string m_ParamO2Name;

	protected GameObject m_ParamO1;

	protected GameObject m_ParamO2;

	public bool m_And;

	public ScenarioNode m_Node;

	public int m_ID = -1;

	public bool m_HasNullObject;

	public enum State
	{
		None,
		InProgress,
		Completed
	}
}
