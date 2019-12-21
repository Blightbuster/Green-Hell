using System;
using UnityEngine;

public class MenuFindGameButtonData : MonoBehaviour
{
	public void OnPressed()
	{
		GameObject parent = this.m_Parent;
		if (parent == null)
		{
			return;
		}
		MenuFindGame component = parent.GetComponent<MenuFindGame>();
		if (component == null)
		{
			return;
		}
		component.OnJoinPressed(this.m_LobbyInfo);
	}

	public GameObject m_Parent;

	public P2PLobbyInfo m_LobbyInfo;
}
