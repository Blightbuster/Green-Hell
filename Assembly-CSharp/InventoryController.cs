using System;
using UnityEngine;

public class InventoryController : PlayerController
{
	public static InventoryController Get()
	{
		return InventoryController.s_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		InventoryController.s_Instance = this;
		base.m_ControllerType = PlayerControllerType.Inventory;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_BInventory, true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_BInventory, false);
	}

	private int m_BInventory = Animator.StringToHash("Inventory");

	private static InventoryController s_Instance;
}
