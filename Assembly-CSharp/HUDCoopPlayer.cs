using System;
using CJTools;
using UnityEngine;
using UnityEngine.UI;

internal class HUDCoopPlayer : HUDBase
{
	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.All);
	}

	protected override void Awake()
	{
		base.Awake();
		this.m_RectTransform = base.GetComponent<RectTransform>();
	}

	protected override bool ShouldShow()
	{
		return false;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void UpdatePlayer()
	{
		float best_player_val = 0f;
		GameObject best_player = null;
		Vector3 camera_pos = CameraManager.Get().m_MainCamera.transform.position;
		Vector3 camera_fwd = CameraManager.Get().m_MainCamera.transform.forward;
		ReplTools.ForEachLogicalPlayer(delegate(ReplicatedLogicalPlayer player)
		{
			float proportionalClamp = CJTools.Math.GetProportionalClamp(0f, 1f, player.transform.position.Distance(camera_pos), this.m_VisibleDistance, 2f);
			float proportionalClamp2 = CJTools.Math.GetProportionalClamp(0f, 1f, Vector3.Dot(camera_fwd, (this.GetPlayerHeadPos(player.gameObject) - camera_pos).normalized), 0.7f, 1f);
			float num = proportionalClamp * proportionalClamp2;
			if (num > best_player_val)
			{
				best_player_val = num;
				best_player = player.gameObject;
			}
		}, ReplTools.EPeerType.Remote);
		if (best_player != this.m_Player)
		{
			this.m_Player = best_player;
			this.OnPlayerChanged();
		}
	}

	private Vector3 GetPlayerHeadPos(GameObject player)
	{
		return player.GetPlayerComponent<BoneTransformCache>().GetBone("head").transform.position;
	}

	private void UpdatePosition()
	{
		Vector3 playerHeadPos = this.GetPlayerHeadPos(this.m_Player);
		playerHeadPos.y += this.m_YOffset;
		this.m_RectTransform.position = CameraManager.Get().m_MainCamera.WorldToScreenPoint(playerHeadPos);
	}

	private void UpdateIcons()
	{
		this.m_IconsGrp.SetActive(this.m_StatusIcons > (HUDCoopPlayer.EStatusIcon)0);
	}

	private void UpdateHealth()
	{
		ReplicatedPlayerParams playerComponent = this.m_Player.GetPlayerComponent<ReplicatedPlayerParams>();
		this.m_Hp.fillAmount = playerComponent.m_Health / 100f;
		this.m_HpMax.fillAmount = playerComponent.m_MaxHealth / 100f;
	}

	private void OnPlayerChanged()
	{
		if (this.m_Player)
		{
			this.m_PlayerName.text = this.m_Player.ReplGetOwner().GetDisplayName();
		}
	}

	private const string HEAD_BONE_NAME = "head";

	public float m_VisibleDistance = 10f;

	public float m_YOffset = 0.25f;

	public Image m_Hp;

	public Image m_HpMax;

	public Text m_PlayerName;

	public GameObject m_IconsGrp;

	private GameObject m_Player;

	private RectTransform m_RectTransform;

	private HUDCoopPlayer.EStatusIcon m_StatusIcons;

	[Flags]
	private enum EStatusIcon
	{
		Backpack = 1,
		Sleeping = 2,
		Inspection = 4,
		Crafting = 8,
		Map = 16,
		Notebook = 32,
		Watch = 64
	}
}
