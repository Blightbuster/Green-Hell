using System;
using UnityEngine;

public class NotepadData : MonoBehaviour
{
	public virtual void Init()
	{
	}

	public virtual bool ShouldShow()
	{
		return true;
	}

	public void Save(string name)
	{
		SaveGame.SaveVal(name, this.m_WasActive);
	}

	public void Load(string name)
	{
		this.m_WasActive = SaveGame.LoadBVal(name);
	}

	public bool m_WasActive;
}
