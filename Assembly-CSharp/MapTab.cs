using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTab : NotepadTab, ISaveLoad
{
	public static MapTab Get()
	{
		return MapTab.s_Instance;
	}

	private MapTab()
	{
		MapTab.s_Instance = this;
	}

	private void Awake()
	{
		this.m_NumActivePages = this.m_MapDatas.Count;
		this.SetPageNum(0);
	}

	public void InitMapsData()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject gameObject = base.transform.GetChild(i).gameObject;
			if (gameObject.name != "Map0" + i.ToString())
			{
				break;
			}
			MapPageData mapPageData = new MapPageData();
			mapPageData.m_Object = gameObject;
			for (int j = 0; j < mapPageData.m_Object.transform.childCount; j++)
			{
				GameObject gameObject2 = mapPageData.m_Object.transform.GetChild(j).gameObject;
				gameObject2.SetActive(false);
				mapPageData.m_Elemets.Add(gameObject2);
			}
			this.m_MapDatas.Add(mapPageData.m_Object.name, mapPageData);
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		Player.Get().BlockMoves();
		this.m_MovesBlocked = true;
		Player.Get().BlockRotation();
		this.m_RotationBlocked = true;
		if (!this.m_MapDatas.Values.ElementAt(this.m_CurrentPage).m_Unlocked)
		{
			this.SetNextPage();
		}
		this.SetupPage();
		HintsManager.Get().ShowHint("MapZoom", 10f);
	}

	public override void OnHide()
	{
		base.OnHide();
		if (this.m_MovesBlocked)
		{
			Player.Get().UnblockMoves();
			this.m_MovesBlocked = false;
		}
		if (this.m_RotationBlocked)
		{
			Player.Get().UnblockRotation();
			this.m_RotationBlocked = false;
		}
	}

	private void SetupPage()
	{
		if (this.m_CurrentPage < 0 || this.m_CurrentPage >= this.m_MapDatas.Count)
		{
			this.m_CurrentPage = 0;
		}
		int num = 0;
		foreach (string key in this.m_MapDatas.Keys)
		{
			this.m_MapDatas[key].m_Object.SetActive(num++ == this.m_CurrentPage);
		}
		if (this.m_CurrentPage == 0)
		{
			MenuNotepad.Get().m_PrevMap.gameObject.SetActive(false);
			MenuNotepad.Get().m_NextMap.gameObject.SetActive(this.GetUnlockedPagesCount() > 1);
		}
		else
		{
			bool active = false;
			for (int i = this.m_CurrentPage - 1; i >= 0; i--)
			{
				if (this.m_MapDatas.Values.ElementAt(i).m_Unlocked)
				{
					active = true;
					break;
				}
			}
			MenuNotepad.Get().m_PrevMap.gameObject.SetActive(active);
			bool active2 = false;
			for (int j = this.m_CurrentPage + 1; j < this.m_NumActivePages; j++)
			{
				if (this.m_MapDatas.Values.ElementAt(j).m_Unlocked)
				{
					active2 = true;
					break;
				}
			}
			MenuNotepad.Get().m_NextMap.gameObject.SetActive(active2);
		}
		MapController.Get().SetActivePage(this.m_CurrentPage);
	}

	public override void SetProperPage(int dir = 0)
	{
	}

	public override void SetNextPage()
	{
		if (!MenuNotepad.Get().m_NextMap.gameObject.activeSelf)
		{
			return;
		}
		if (this.m_CurrentPage < this.m_NumActivePages - 1)
		{
			this.m_CurrentPage++;
			if (!this.m_MapDatas.Values.ElementAt(this.m_CurrentPage).m_Unlocked)
			{
				this.SetNextPage();
				return;
			}
		}
		this.SetupPage();
	}

	public override void SetPrevPage()
	{
		if (!MenuNotepad.Get().m_PrevMap.gameObject.activeSelf)
		{
			return;
		}
		if (this.m_CurrentPage > 0)
		{
			this.m_CurrentPage--;
			if (!this.m_MapDatas.Values.ElementAt(this.m_CurrentPage).m_Unlocked)
			{
				this.SetPrevPage();
				return;
			}
		}
		this.SetupPage();
	}

	public void LockElement(string element_name)
	{
		foreach (string key in this.m_MapDatas.Keys)
		{
			for (int i = 0; i < this.m_MapDatas[key].m_Elemets.Count; i++)
			{
				if (this.m_MapDatas[key].m_Elemets[i].name == element_name)
				{
					this.m_MapDatas[key].m_Elemets[i].SetActive(false);
					break;
				}
			}
		}
	}

	public void UnlockElement(string element_name)
	{
		foreach (string key in this.m_MapDatas.Keys)
		{
			for (int i = 0; i < this.m_MapDatas[key].m_Elemets.Count; i++)
			{
				if (this.m_MapDatas[key].m_Elemets[i].name == element_name)
				{
					this.m_MapDatas[key].m_Elemets[i].SetActive(true);
					MenuNotepad.Get().OnAddMapArea();
					break;
				}
			}
		}
	}

	public void UnlockAll()
	{
	}

	public void LockPage(string page_name)
	{
		this.m_MapDatas[page_name].m_Unlocked = false;
	}

	public void UnlockPage(string page_name)
	{
		this.m_MapDatas[page_name].m_Unlocked = true;
	}

	public string GetCurrentPageName()
	{
		if (this.m_CurrentPage >= 0 && this.m_CurrentPage <= this.m_MapDatas.Count)
		{
			return this.m_MapDatas.Values.ElementAt(this.m_CurrentPage).m_Object.name;
		}
		return string.Empty;
	}

	public int GetUnlockedPagesCount()
	{
		int num = 0;
		foreach (string key in this.m_MapDatas.Keys)
		{
			if (this.m_MapDatas[key].m_Unlocked)
			{
				num++;
			}
		}
		return num;
	}

	public void Save()
	{
		foreach (string key in this.m_MapDatas.Keys)
		{
			foreach (GameObject gameObject in this.m_MapDatas[key].m_Elemets)
			{
				SaveGame.SaveVal(this.m_MapDatas[key].m_Object.name + gameObject.name, gameObject.activeSelf);
			}
			SaveGame.SaveVal(this.m_MapDatas[key].m_Object.name + "Unlocked", this.m_MapDatas[key].m_Unlocked);
		}
	}

	public void Load()
	{
		foreach (string key in this.m_MapDatas.Keys)
		{
			foreach (GameObject gameObject in this.m_MapDatas[key].m_Elemets)
			{
				gameObject.SetActive(SaveGame.LoadBVal(this.m_MapDatas[key].m_Object.name + gameObject.name));
			}
			this.m_MapDatas[key].m_Unlocked = SaveGame.LoadBVal(this.m_MapDatas[key].m_Object.name + "Unlocked");
		}
	}

	private void Update()
	{
		if (this.m_RotationBlocked && WatchController.Get().IsActive())
		{
			Player.Get().UnblockRotation();
			this.m_RotationBlocked = false;
			return;
		}
		if (!this.m_RotationBlocked && !WatchController.Get().IsActive())
		{
			Player.Get().BlockRotation();
			this.m_RotationBlocked = true;
		}
	}

	private Dictionary<string, MapPageData> m_MapDatas = new Dictionary<string, MapPageData>();

	public Transform m_WorldZeroDummy;

	public Transform m_WorldOneDummy;

	private bool m_MovesBlocked;

	private bool m_RotationBlocked;

	private static MapTab s_Instance;
}
