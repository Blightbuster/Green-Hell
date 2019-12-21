using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMemberListElement : MonoBehaviour
{
	public void SetName(string name)
	{
		if (name == null || name.Length == 0)
		{
			this.m_MemberName.text = GreenHellGame.Instance.GetLocalization().Get("MenuIngame_CoopEmptySlot", true);
			return;
		}
		this.m_MemberName.text = name;
	}

	[SerializeField]
	private Text m_MemberName;
}
