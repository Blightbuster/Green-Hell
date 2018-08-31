using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDLiquidSource : HUDBase
{
	public static HUDLiquidSource Get()
	{
		return HUDLiquidSource.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDLiquidSource.s_Instance = this;
		this.m_SelectionGet = this.m_ColliderGet.gameObject.GetComponent<RawImage>();
	}

	public override void SetupGroups()
	{
		base.SetupGroups();
		base.AddToGroup(HUDManager.HUDGroup.Inventory3D);
	}

	protected override bool ShouldShow()
	{
		return this.m_Active;
	}

	public void Activate(LiquidContainer container, LiquidSource liquid_source)
	{
		this.m_Container = container;
		this.m_Container.StaticPhxRequestAdd();
		this.m_LiquidSource = liquid_source;
		base.transform.position = Input.mousePosition;
		this.UpdateSelection();
		this.m_Active = true;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateSelection();
		this.UpdateInputs();
	}

	private void UpdateSelection()
	{
		this.m_SelectionGet.enabled = this.m_ColliderGet.OverlapPoint(Input.mousePosition);
	}

	private void UpdateInputs()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (this.m_SelectionGet.enabled)
			{
				this.m_Container.Fill(this.m_LiquidSource);
			}
			this.m_Container.StaticPhxRequestRemove();
			this.m_Active = false;
			Inventory3DManager.Get().SetCarriedItem(this.m_Container);
			HUDMessages hudmessages = (HUDMessages)HUDManager.Get().GetHUD(typeof(HUDMessages));
			hudmessages.AddMessage("LiquidType_" + this.m_LiquidSource.m_LiquidType.ToString() + " " + GreenHellGame.Instance.GetLocalization().Get("HUD_Trigger_Taken"), new Color?(Color.white), HUDMessageIcon.None, string.Empty);
		}
	}

	private void Execute(LiquidContainerInfo info0, LiquidContainerInfo info1)
	{
		float amount = info0.m_Amount;
		info0.m_Amount += info1.m_Amount;
		info0.m_Amount = Mathf.Clamp(info0.m_Amount, 0f, info0.m_Capacity);
		float num = info0.m_Amount - amount;
		info1.m_Amount -= num;
	}

	public PolygonCollider2D m_ColliderGet;

	private RawImage m_SelectionGet;

	private LiquidContainer m_Container;

	private LiquidSource m_LiquidSource;

	public bool m_Active;

	private static HUDLiquidSource s_Instance;
}
