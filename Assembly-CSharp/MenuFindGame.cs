using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuFindGame : MenuScreen
{
	protected override void Awake()
	{
		base.Awake();
		this.m_StartStopOnlineText = this.m_ButtonStartStopOnline.GetComponentInChildren<Text>();
		this.m_ButtonStartStopOnline.gameObject.SetActive(false);
		this.m_ButtonStartStopOnlineText = this.m_ButtonStartStopOnline.GetComponentInChildren<Text>();
		if (this.m_SearchSessionVisibility)
		{
			this.m_SearchSessionVisibility.AddOption(P2PGameVisibility.Public.ToString(), "MenuFindGame_Search_Public");
			this.m_SearchSessionVisibility.AddOption(P2PGameVisibility.Friends.ToString(), "MenuFindGame_Search_Friends");
		}
		if (this.m_HostedSessionVisibility)
		{
			this.m_HostedSessionVisibility.AddOption(P2PGameVisibility.Singleplayer.ToString(), "GameVisibility_Singleplayer");
			this.m_HostedSessionVisibility.AddOption(P2PGameVisibility.Private.ToString(), "GameVisibility_Private");
			this.m_HostedSessionVisibility.AddOption(P2PGameVisibility.Friends.ToString(), "GameVisibility_Friends");
			this.m_HostedSessionVisibility.AddOption(P2PGameVisibility.Public.ToString(), "GameVisibility_Public");
		}
	}

	public override void OnShow()
	{
		base.OnShow();
		if (ReplTools.IsPlayingAlone())
		{
			P2PTransportLayer.OnLobbyListAcquiredEvent += this.OnLobbyList;
			this.OnRefresh();
			if (this.m_SearchSessionVisibility)
			{
				this.m_SearchSessionVisibility.SetByOption(this.m_WantedVisibility.ToString());
			}
			if (this.m_HostedSessionVisibility)
			{
				this.m_HostedSessionVisibility.SetSelectedOptionEnumValue<P2PGameVisibility>(GreenHellGame.Instance.m_Settings.m_GameVisibility);
			}
		}
		else
		{
			this.SetListElementCount(0);
		}
		this.m_SearchSessionVisibility.gameObject.SetActive(ReplTools.IsPlayingAlone());
		this.m_ButtonRefresh.gameObject.SetActive(ReplTools.IsPlayingAlone());
		this.m_ButtonStartStopOnlineText.text = GreenHellGame.Instance.GetLocalization().Get(ReplTools.AmIMaster() ? "MenuFindGame_StopSession" : "MenuFindGame_LeaveSession", true);
		this.UpdateStatusText();
	}

	public override bool IsMenuButtonEnabled(Button b)
	{
		return (!(b == this.m_ButtonRefresh) || ReplTools.IsPlayingAlone()) && base.IsMenuButtonEnabled(b);
	}

	public override void OnBack()
	{
		P2PTransportLayer.OnLobbyListAcquiredEvent -= this.OnLobbyList;
		GreenHellGame.Instance.m_Settings.SaveSettings();
		base.OnBack();
	}

	public void OnRefresh()
	{
		if (ReplTools.IsPlayingAlone())
		{
			P2PTransportLayer.Instance.RequestLobbyList(this.m_WantedVisibility);
		}
	}

	public void OnStartStopOnline()
	{
		if (P2PSession.Instance.Status == P2PSession.ESessionStatus.Idle)
		{
			P2PSession.Instance.Start(null);
		}
		else
		{
			P2PSession.Instance.Restart();
		}
		this.UpdateStatusText();
	}

	public void OnJoinPressed(P2PLobbyInfo data)
	{
		if (ReplTools.IsPlayingAlone() && data != null)
		{
			P2PTransportLayer.OnLobbyListAcquiredEvent -= this.OnLobbyList;
			P2PSession.Instance.JoinLobby(data.m_Address);
			if (this.m_MenuInGameManager)
			{
				this.m_MenuInGameManager.HideMenu();
				return;
			}
			this.OnBack();
		}
	}

	public void OnLobbyList(List<P2PLobbyInfo> lobbies)
	{
		if (ReplTools.IsPlayingAlone() && this.m_GamesList != null)
		{
			this.SetListElementCount(lobbies.Count);
			for (int i = 0; i < lobbies.Count; i++)
			{
				Transform child = this.m_GamesList.transform.GetChild(i);
				if (child)
				{
					P2PLobbyInfo p2PLobbyInfo = lobbies[i];
					child.GetComponentInChildren<Text>().text = string.Format("{0} [{1}/{2}]", p2PLobbyInfo.m_Name, p2PLobbyInfo.m_MemberCount, p2PLobbyInfo.m_SlotCount);
					MenuFindGameButtonData component = child.GetComponent<MenuFindGameButtonData>();
					component.m_LobbyInfo = p2PLobbyInfo;
					component.m_Parent = base.gameObject;
				}
			}
		}
		this.UpdateStatusText();
	}

	public override void OnSelectionChanged(UISelectButton button, string option)
	{
		if (this.m_SearchSessionVisibility == button)
		{
			this.m_WantedVisibility = this.m_SearchSessionVisibility.GetSelectedOptionEnumValue<P2PGameVisibility>();
			this.OnRefresh();
			return;
		}
		if (this.m_HostedSessionVisibility == button)
		{
			P2PGameVisibility selectedOptionEnumValue = this.m_HostedSessionVisibility.GetSelectedOptionEnumValue<P2PGameVisibility>();
			GreenHellGame.Instance.m_Settings.m_GameVisibility = selectedOptionEnumValue;
			this.UpdateStatusText();
			return;
		}
		base.OnSelectionChanged(button, option);
	}

	private void UpdateStartStopOnlineButtonText()
	{
	}

	private void SetListElementCount(int count)
	{
		if (this.m_GamesList == null)
		{
			return;
		}
		if (this.m_GamesList.transform.childCount > count)
		{
			int childCount = this.m_GamesList.transform.childCount;
			for (int i = 1; i <= childCount - count; i++)
			{
				UnityEngine.Object.Destroy(this.m_GamesList.transform.GetChild(childCount - i).gameObject);
			}
		}
		if (this.m_GameButtonPrefab != null)
		{
			while (this.m_GamesList.transform.childCount < count)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.m_GameButtonPrefab).transform.parent = this.m_GamesList.transform;
			}
		}
	}

	private void UpdateStatusText()
	{
		P2PGameVisibility gameVisibility = GreenHellGame.Instance.m_Settings.m_GameVisibility;
		if (this.m_IsRefreshing)
		{
			this.m_StatusText.text = GreenHellGame.Instance.GetLocalization().Get("MenuFindGame_StatusRefreshing", true);
		}
		else if (gameVisibility == P2PGameVisibility.Singleplayer && ReplTools.IsPlayingAlone())
		{
			this.m_StatusText.text = GreenHellGame.Instance.GetLocalization().Get("MenuFindGame_StatusSingleplayer", true);
		}
		else
		{
			switch (P2PSession.Instance.Status)
			{
			case P2PSession.ESessionStatus.Idle:
				this.m_StatusText.text = GreenHellGame.Instance.GetLocalization().Get("MenuFindGame_StatusIdle", true);
				break;
			case P2PSession.ESessionStatus.Listening:
				this.m_StatusText.text = GreenHellGame.Instance.GetLocalization().Get("MenuFindGame_StatusListening", true);
				break;
			case P2PSession.ESessionStatus.Connecting:
				this.m_StatusText.text = GreenHellGame.Instance.GetLocalization().Get("MenuFindGame_StatusConnecting", true);
				break;
			case P2PSession.ESessionStatus.Connected:
				this.m_StatusText.text = GreenHellGame.Instance.GetLocalization().Get("MenuFindGame_StatusPlaying", true);
				break;
			}
		}
		if (this.m_StartStopOnlineText)
		{
			this.m_ButtonStartStopOnline.gameObject.SetActive(!ReplTools.IsPlayingAlone());
		}
	}

	public GameObject m_GameButtonPrefab;

	public GameObject m_GamesList;

	public Text m_StatusText;

	public UISelectButton m_SearchSessionVisibility;

	public UISelectButton m_HostedSessionVisibility;

	public Button m_ButtonRefresh;

	public Button m_ButtonStartStopOnline;

	private Text m_ButtonStartStopOnlineText;

	[HideInInspector]
	public Text m_StartStopOnlineText;

	private bool m_IsRefreshing;

	private P2PGameVisibility m_WantedVisibility = P2PGameVisibility.Friends;
}
