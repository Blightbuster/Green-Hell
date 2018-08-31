using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDCameraPosition : HUDBase
{
	public static HUDCameraPosition Get()
	{
		return HUDCameraPosition.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDCameraPosition.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	protected override void Update()
	{
		base.Update();
		if (Camera.main == null)
		{
			return;
		}
		this.m_Text.text = Camera.main.transform.position.ToString();
	}

	[HideInInspector]
	public bool m_Active;

	public Text m_Text;

	private static HUDCameraPosition s_Instance;
}
