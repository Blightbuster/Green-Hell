using System;
using UnityEngine;

public class NotepadTab : MonoBehaviour
{
	public virtual void Init()
	{
	}

	public virtual void OnShow()
	{
	}

	public virtual void OnHide()
	{
	}

	public virtual int GetNumActivePages()
	{
		return this.m_NumActivePages;
	}

	public virtual int GetCurrentPage()
	{
		return this.m_CurrentPage;
	}

	public virtual void UpdatePages()
	{
	}

	public virtual void SetPageNum(int page_num)
	{
		if (page_num < this.m_NumActivePages - 1 && page_num >= 0)
		{
			this.m_CurrentPage = page_num;
			this.UpdatePages();
		}
	}

	public virtual void SetPrevPage()
	{
		this.SetProperPage(-1);
	}

	public virtual void SetNextPage()
	{
		this.SetProperPage(1);
	}

	public virtual void SetProperPage(int dir = 0)
	{
		int currentPage = this.m_CurrentPage;
		if (this.m_NumActiveElementsOnPage == 0 || dir != 0)
		{
			if (dir >= 0)
			{
				for (int i = currentPage + dir; i < this.m_NumActivePages; i++)
				{
					this.SetPageNum(i);
					if (this.m_NumActiveElementsOnPage > 0)
					{
						return;
					}
				}
			}
			if (dir <= 0)
			{
				for (int j = currentPage - 1; j >= 0; j--)
				{
					this.SetPageNum(j);
					if (this.m_NumActiveElementsOnPage > 0)
					{
						return;
					}
				}
			}
			this.SetPageNum(currentPage);
		}
	}

	public virtual bool ShouldShowNoEntries()
	{
		return false;
	}

	public virtual void Save(string name)
	{
	}

	public virtual void Load(string name)
	{
	}

	public virtual int GetNewEntriesCount()
	{
		return 0;
	}

	protected int m_CurrentPage;

	protected int m_NumActivePages;

	[HideInInspector]
	public int m_NumActiveElementsOnPage;

	public MenuNotepad.MenuNotepadTab m_Tab = MenuNotepad.MenuNotepadTab.None;
}
