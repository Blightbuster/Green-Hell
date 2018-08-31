using System;
using CJTools;
using UnityEngine;

public class AnimalBody : Item
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.m_InInventory)
		{
			Animator componentDeepChild = General.GetComponentDeepChild<Animator>(base.gameObject);
			if (componentDeepChild == null)
			{
				return;
			}
			int num = Animator.StringToHash("Backpack");
			foreach (AnimatorControllerParameter animatorControllerParameter in componentDeepChild.parameters)
			{
				if (animatorControllerParameter.nameHash == num)
				{
					componentDeepChild.SetBool(num, true);
				}
			}
		}
	}
}
