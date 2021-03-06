﻿using System;
using UnityEngine;

public class Parasite : Item
{
	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && !this.m_InBody && base.CanTrigger();
	}

	public override void OnAddToInventory()
	{
		base.OnAddToInventory();
		Animator component = base.GetComponent<Animator>();
		if (component)
		{
			component.SetBool("Backpack", true);
		}
	}

	public override void OnRemoveFromInventory()
	{
		base.OnRemoveFromInventory();
		Animator component = base.GetComponent<Animator>();
		if (component)
		{
			component.SetBool("Backpack", false);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	[HideInInspector]
	public bool m_InBody;
}
