using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfo : MonoBehaviour
{
	private void Start()
	{
		this.m_Members = base.GetComponentsInChildren<LobbyMemberListElement>();
	}

	private void OnEnable()
	{
		switch (P2PSession.Instance.GetGameVisibility())
		{
		case P2PGameVisibility.Singleplayer:
			this.m_VisibilityText.text = GreenHellGame.Instance.GetLocalization().GetMixed("MenuIngame_GameVisibility", new string[]
			{
				"GameVisibility_Singleplayer"
			});
			return;
		case P2PGameVisibility.Public:
			this.m_VisibilityText.text = GreenHellGame.Instance.GetLocalization().GetMixed("MenuIngame_GameVisibility", new string[]
			{
				"GameVisibility_Public"
			});
			return;
		case P2PGameVisibility.Friends:
			this.m_VisibilityText.text = GreenHellGame.Instance.GetLocalization().GetMixed("MenuIngame_GameVisibility", new string[]
			{
				"GameVisibility_Friends"
			});
			return;
		case P2PGameVisibility.Private:
			this.m_VisibilityText.text = GreenHellGame.Instance.GetLocalization().GetMixed("MenuIngame_GameVisibility", new string[]
			{
				"GameVisibility_Private"
			});
			return;
		default:
			return;
		}
	}

	private void Update()
	{
		if (this.m_LastUpdate < Time.realtimeSinceStartup - 1f)
		{
			this.m_LastUpdate = Time.realtimeSinceStartup;
			ReadOnlyCollection<P2PLobbyMemberInfo> currentLobbyMembers = P2PTransportLayer.Instance.GetCurrentLobbyMembers();
			DebugUtils.Assert(currentLobbyMembers.Count <= this.m_Members.Length, true);
			for (int i = 0; i < this.m_Members.Length; i++)
			{
				if (i < currentLobbyMembers.Count)
				{
					this.m_Members[i].SetName(currentLobbyMembers[i].m_Name);
				}
				else
				{
					this.m_Members[i].SetName(string.Empty);
				}
			}
		}
	}

	private LobbyMemberListElement[] m_Members;

	private float m_LastUpdate = -1f;

	private const float UPDATE_INTERVAL = 1f;

	public Text m_VisibilityText;
}
