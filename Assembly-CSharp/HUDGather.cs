using System;
using UnityEngine.UI;

public class HUDGather : HUDBase
{
	public static HUDGather Get()
	{
		return HUDGather.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDGather.s_Instance = this;
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Game);
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override void Start()
	{
		base.Start();
		this.m_Text.text = string.Empty;
	}

	protected override bool ShouldShow()
	{
		return this.m_Show;
	}

	public void Setup()
	{
	}

	public Text m_Text;

	private bool m_Show;

	private static HUDGather s_Instance;
}
