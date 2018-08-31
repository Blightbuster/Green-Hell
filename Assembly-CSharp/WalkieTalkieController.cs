using System;
using Enums;
using UnityEngine;

public class WalkieTalkieController : PlayerController
{
	protected override void Awake()
	{
		base.Awake();
		this.m_ControllerType = PlayerControllerType.WalkieTalkie;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.m_Animator.SetBool(this.m_BWalkieTalkie, true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.m_Animator.SetBool(this.m_BWalkieTalkie, false);
		Item currentItem = this.m_Player.GetCurrentItem(Hand.Right);
		InventoryBackpack.Get().InsertItem(currentItem, null, null, true, true, true, true, true);
		this.m_Player.SetWantedItem(Hand.Right, this.m_RHandItemToRestore, true);
	}

	private int m_BWalkieTalkie = Animator.StringToHash("WalkieTalkie");

	public Item m_RHandItemToRestore;
}
