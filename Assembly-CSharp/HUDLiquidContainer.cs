using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDLiquidContainer : HUDBase
{
	public static HUDLiquidContainer Get()
	{
		return HUDLiquidContainer.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		HUDLiquidContainer.s_Instance = this;
		this.m_SelectionPour = this.m_ColliderPour.gameObject.GetComponent<RawImage>();
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

	protected override void OnShow()
	{
		base.OnShow();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	public bool Activate(LiquidContainer container0, LiquidContainer container1)
	{
		LiquidContainerInfo liquidContainerInfo = (LiquidContainerInfo)container0.m_Info;
		LiquidContainerInfo liquidContainerInfo2 = (LiquidContainerInfo)container1.m_Info;
		if (liquidContainerInfo == null || liquidContainerInfo2 == null)
		{
			return false;
		}
		if (liquidContainerInfo.m_LiquidType != liquidContainerInfo2.m_LiquidType && liquidContainerInfo.m_Amount != 0f && liquidContainerInfo2.m_Amount != 0f)
		{
			return false;
		}
		this.m_Container0 = container0;
		this.m_Container0.StaticPhxRequestAdd();
		this.m_Container1 = container1;
		base.transform.position = Input.mousePosition;
		this.UpdateSelection();
		this.m_Active = true;
		return true;
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateSelection();
		this.UpdateInputs();
	}

	private void UpdateSelection()
	{
		if (this.m_ColliderPour.OverlapPoint(Input.mousePosition))
		{
			this.m_SelectionPour.enabled = true;
			this.m_SelectionGet.enabled = false;
		}
		else if (this.m_ColliderGet.OverlapPoint(Input.mousePosition))
		{
			this.m_SelectionPour.enabled = false;
			this.m_SelectionGet.enabled = true;
		}
		else
		{
			this.m_SelectionPour.enabled = false;
			this.m_SelectionGet.enabled = false;
		}
	}

	private void UpdateInputs()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (this.m_SelectionGet.enabled)
			{
				this.m_Container0.Fill(this.m_Container1);
			}
			else if (this.m_SelectionPour.enabled)
			{
				this.m_Container1.Fill(this.m_Container0);
			}
			this.m_Container0.StaticPhxRequestRemove();
			this.m_Active = false;
			Inventory3DManager.Get().SetCarriedItem(this.m_Container0);
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

	public PolygonCollider2D m_ColliderPour;

	private RawImage m_SelectionPour;

	public PolygonCollider2D m_ColliderGet;

	private RawImage m_SelectionGet;

	private LiquidContainer m_Container0;

	private LiquidContainer m_Container1;

	public bool m_Active;

	private static HUDLiquidContainer s_Instance;
}
