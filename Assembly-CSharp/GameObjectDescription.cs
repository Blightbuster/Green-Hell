using System;
using UnityEngine;

[Serializable]
public class GameObjectDescription : MonoBehaviour
{
	public string GetDescription()
	{
		return this.m_Description;
	}

	public void SetDescription(string desc)
	{
		this.m_Description = desc;
	}

	[SerializeField]
	[HideInInspector]
	private string m_Description = string.Empty;
}
