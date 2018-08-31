using System;
using System.Collections.Generic;
using UnityEngine;

public class MapTab : NotepadTab, ISaveLoad
{
	private MapTab()
	{
		MapTab.s_Instance = this;
	}

	public static MapTab Get()
	{
		return MapTab.s_Instance;
	}

	private void Awake()
	{
		this.m_NumActivePages = this.m_MapsData.Count;
		this.SetPageNum(0);
	}

	public void InitMapsData()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject gameObject = base.transform.GetChild(i).gameObject;
			List<GameObject> list = new List<GameObject>();
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				GameObject gameObject2 = gameObject.transform.GetChild(j).gameObject;
				gameObject2.SetActive(false);
				list.Add(gameObject2);
			}
			this.m_MapsData.Add(gameObject, list);
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		Player.Get().BlockMoves();
		this.m_MovesBlocked = true;
		Player.Get().BlockRotation();
		this.m_RotationBlocked = true;
		this.SetupPage();
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
		if (this.m_CurrentPage < 0 || this.m_CurrentPage >= this.m_MapsData.Count)
		{
			this.m_CurrentPage = 0;
		}
		int num = 0;
		foreach (GameObject gameObject in this.m_MapsData.Keys)
		{
			gameObject.SetActive(num++ == this.m_CurrentPage);
		}
		if (this.m_CurrentPage == 0)
		{
			MenuNotepad.Get().m_PrevMap.gameObject.SetActive(false);
			MenuNotepad.Get().m_NextMap.gameObject.SetActive(this.m_NumActivePages > 1);
		}
		else
		{
			MenuNotepad.Get().m_PrevMap.gameObject.SetActive(true);
			MenuNotepad.Get().m_NextMap.gameObject.SetActive(this.m_CurrentPage + 1 < this.m_NumActivePages);
		}
	}

	public override void SetProperPage(int dir = 0)
	{
	}

	public override void SetNextPage()
	{
		base.SetNextPage();
		this.SetupPage();
	}

	public override void SetPrevPage()
	{
		base.SetPrevPage();
		this.SetupPage();
	}

	public void LockElement(string element_name)
	{
		foreach (GameObject key in this.m_MapsData.Keys)
		{
			for (int i = 0; i < this.m_MapsData[key].Count; i++)
			{
				if (this.m_MapsData[key][i].name == element_name)
				{
					this.m_MapsData[key][i].SetActive(false);
					break;
				}
			}
		}
	}

	public void UnlockElement(string element_name)
	{
		foreach (GameObject key in this.m_MapsData.Keys)
		{
			for (int i = 0; i < this.m_MapsData[key].Count; i++)
			{
				if (this.m_MapsData[key][i].name == element_name)
				{
					this.m_MapsData[key][i].SetActive(true);
					break;
				}
			}
		}
	}

	public void Save()
	{
		foreach (GameObject gameObject in this.m_MapsData.Keys)
		{
			foreach (GameObject gameObject2 in this.m_MapsData[gameObject])
			{
				SaveGame.SaveVal(gameObject.name + gameObject2.name, gameObject2.activeSelf);
			}
		}
	}

	public void Load()
	{
		foreach (GameObject gameObject in this.m_MapsData.Keys)
		{
			foreach (GameObject gameObject2 in this.m_MapsData[gameObject])
			{
				gameObject2.SetActive(SaveGame.LoadBVal(gameObject.name + gameObject2.name));
			}
		}
	}

	private void Update()
	{
		if (this.m_RotationBlocked && WatchController.Get().IsActive())
		{
			Player.Get().UnblockRotation();
			this.m_RotationBlocked = false;
		}
		else if (!this.m_RotationBlocked && !WatchController.Get().IsActive())
		{
			Player.Get().BlockRotation();
			this.m_RotationBlocked = true;
		}
	}

	private Dictionary<GameObject, List<GameObject>> m_MapsData = new Dictionary<GameObject, List<GameObject>>();

	public Transform m_WorldZeroDummy;

	public Transform m_WorldOneDummy;

	private bool m_MovesBlocked;

	private bool m_RotationBlocked;

	private static MapTab s_Instance;
}
