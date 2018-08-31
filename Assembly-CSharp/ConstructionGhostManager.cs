using System;
using System.Collections.Generic;
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
		while (this.m_AllGhosts.Count > 0)
		{
			UnityEngine.Object.Destroy(this.m_AllGhosts[0].gameObject);
		}
		this.m_AllGhosts.Clear();
	}

	private void Update()
	{
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
		for (int i = 0; i < this.m_AllGhosts.Count; i++)
		{
			UnityEngine.Object.Destroy(this.m_AllGhosts[i].gameObject);
		}
		this.m_AllGhosts.Clear();
		int num = SaveGame.LoadIVal("GhostCount");
		for (int j = 0; j < num; j++)
		{
			string text = SaveGame.LoadSVal("Ghost" + j);
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
				component.Load(j);
			}
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

	private List<ConstructionGhost> m_AllGhosts = new List<ConstructionGhost>();

	private static ConstructionGhostManager s_Instance;
}
