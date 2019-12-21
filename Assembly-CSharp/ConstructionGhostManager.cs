using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ConstructionGhostManager : MonoBehaviour, ISaveLoad
{
	public static ConstructionGhostManager Get()
	{
		return ConstructionGhostManager.s_Instance;
	}

	private void Awake()
	{
		ConstructionGhostManager.s_Instance = this;
	}

	public void RegisterGhost(ConstructionGhost ghost)
	{
		if (!this.m_AllGhosts.Contains(ghost))
		{
			this.m_AllGhosts.Add(ghost);
		}
	}

	public void UnregisterGhost(ConstructionGhost ghost)
	{
		this.m_AllGhosts.Remove(ghost);
	}

	public void ScenarioDeleteAllGhosts()
	{
		int i = 0;
		while (i < this.m_AllGhosts.Count)
		{
			if (this.m_AllGhosts[0].m_ResultItemID != ItemID.Bamboo_Bridge)
			{
				UnityEngine.Object.Destroy(this.m_AllGhosts[i].gameObject);
				this.m_AllGhosts.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private void Update()
	{
		this.UpdateActivity();
		if (!GreenHellGame.DEBUG)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F8))
		{
			for (int i = 0; i < this.m_AllGhosts.Count; i++)
			{
				if (this.m_AllGhosts[i].gameObject.activeSelf)
				{
					this.m_AllGhosts[i].m_DebugBuild = true;
				}
			}
		}
	}

	private void UpdateActivity()
	{
		if (this.m_AllGhosts.Count == 0)
		{
			return;
		}
		if (Time.time - this.m_LastUpdateActivityTime < this.m_UpdateActivityInterval)
		{
			return;
		}
		float num = (float)Mathf.Min(20, this.m_AllGhosts.Count);
		int num2 = 0;
		while ((float)num2 < num)
		{
			if (this.m_CurrentIndex >= this.m_AllGhosts.Count)
			{
				this.m_CurrentIndex = 0;
			}
			ConstructionGhost constructionGhost = this.m_AllGhosts[this.m_CurrentIndex];
			if (constructionGhost.m_Challenge)
			{
				this.m_CurrentIndex++;
			}
			else if (ScenarioManager.Get().IsPreDream())
			{
				if (constructionGhost.gameObject.activeSelf)
				{
					constructionGhost.gameObject.SetActive(false);
				}
				this.m_CurrentIndex++;
			}
			else if (constructionGhost.m_ResultItemID == ItemID.Bamboo_Bridge && GreenHellGame.Instance.m_GameMode == GameMode.Story)
			{
				if (constructionGhost.gameObject.activeSelf)
				{
					constructionGhost.gameObject.SetActive(false);
				}
				this.m_CurrentIndex++;
			}
			else
			{
				bool flag = Player.Get().transform.position.Distance(constructionGhost.transform.position) < this.m_ActivityDist;
				if (constructionGhost.gameObject.activeSelf != flag && !constructionGhost.IsReady())
				{
					constructionGhost.gameObject.SetActive(flag);
				}
				this.m_CurrentIndex++;
			}
			num2++;
		}
		this.m_LastUpdateActivityTime = Time.time;
	}

	public void Save()
	{
		int num = 0;
		for (int i = 0; i < this.m_AllGhosts.Count; i++)
		{
			ConstructionGhost constructionGhost = this.m_AllGhosts[i];
			if (constructionGhost.m_State == ConstructionGhost.GhostState.Building)
			{
				SaveGame.SaveVal("Ghost" + num, constructionGhost.name);
				constructionGhost.Save(num);
				num++;
			}
		}
		SaveGame.SaveVal("GhostCount", num);
	}

	public void Load()
	{
		while (this.m_AllGhosts.Count > 0)
		{
			if (this.m_AllGhosts[0].gameObject)
			{
				UnityEngine.Object.Destroy(this.m_AllGhosts[0].gameObject);
			}
			this.m_AllGhosts.RemoveAt(0);
		}
	}

	public void SetupAfterLoad()
	{
		bool flag = false;
		int num = SaveGame.LoadIVal("GhostCount");
		for (int i = 0; i < num; i++)
		{
			string text = SaveGame.LoadSVal("Ghost" + i);
			GameObject prefab = GreenHellGame.Instance.GetPrefab(text);
			if (!prefab)
			{
				DebugUtils.Assert("[ConstructionGhostManager:Load] ERROR - Can't load prefab - " + text, true, DebugUtils.AssertType.Info);
			}
			else
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
				gameObject.name = prefab.name;
				ConstructionGhost component = gameObject.GetComponent<ConstructionGhost>();
				component.Load(i);
				if (component.m_ResultItemID == ItemID.Bamboo_Bridge)
				{
					flag = true;
				}
			}
		}
		if (!flag && GreenHellGame.Instance.m_GameMode == GameMode.Survival)
		{
			GameObject gameObject2 = null;
			bool flag2 = false;
			foreach (Item item in Item.s_AllItems)
			{
				if (item.GetInfoID() == ItemID.Bamboo_Bridge)
				{
					flag2 = true;
					gameObject2 = item.gameObject;
					break;
				}
			}
			Vector3 zero = Vector3.zero;
			zero.Set(842.39f, 138.569f, 1617.97f);
			Quaternion identity = Quaternion.identity;
			identity.Set(-0.5448493f, -0.450922f, -0.451856f, 0.543723f);
			if (!flag2)
			{
				gameObject2 = UnityEngine.Object.Instantiate<GameObject>(GreenHellGame.Instance.GetPrefab("Bamboo_Bridge"));
				gameObject2.transform.position = zero;
				gameObject2.transform.rotation = identity;
				return;
			}
			gameObject2.transform.position = zero;
			gameObject2.transform.rotation = identity;
		}
	}

	public bool GhostExist(string name)
	{
		for (int i = 0; i < this.m_AllGhosts.Count; i++)
		{
			if (this.m_AllGhosts[i].name == name && this.m_AllGhosts[i].m_State == ConstructionGhost.GhostState.Building)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSettingUpGhost(string name)
	{
		for (int i = 0; i < this.m_AllGhosts.Count; i++)
		{
			if (this.m_AllGhosts[i].name == name && this.m_AllGhosts[i].m_State == ConstructionGhost.GhostState.Dragging)
			{
				return true;
			}
		}
		return false;
	}

	public void OnDestoryConstruction(Construction construction)
	{
		this.m_TempGhostToDestroy.Clear();
		foreach (ConstructionGhost constructionGhost in this.m_AllGhosts)
		{
			if (constructionGhost.m_SelectedSlot)
			{
				ConstructionSlot[] constructionSlots = construction.m_ConstructionSlots;
				for (int i = 0; i < constructionSlots.Length; i++)
				{
					if (constructionSlots[i] == constructionGhost.m_SelectedSlot)
					{
						this.m_TempGhostToDestroy.Add(constructionGhost);
						break;
					}
				}
			}
		}
		foreach (ConstructionGhost constructionGhost2 in this.m_TempGhostToDestroy)
		{
			constructionGhost2.Deconstruct();
		}
		foreach (ConstructionGhost constructionGhost3 in this.m_AllGhosts)
		{
			if (constructionGhost3.m_ConstructionBelow == construction && constructionGhost3.m_State == ConstructionGhost.GhostState.Building)
			{
				constructionGhost3.Deconstruct();
			}
		}
	}

	private List<ConstructionGhost> m_AllGhosts = new List<ConstructionGhost>();

	private float m_LastUpdateActivityTime;

	private float m_UpdateActivityInterval = 1f;

	private static ConstructionGhostManager s_Instance;

	private int m_CurrentIndex;

	private float m_ActivityDist = 60f;

	private List<ConstructionGhost> m_TempGhostToDestroy = new List<ConstructionGhost>();
}
