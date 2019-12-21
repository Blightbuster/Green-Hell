using System;
using CJTools;
using UnityEngine;

public class AnimalBody : Item
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.m_InInventory)
		{
			Animator componentDeepChild = General.GetComponentDeepChild<Animator>(base.gameObject);
			if (componentDeepChild)
			{
				componentDeepChild.SetBool(this.m_BackpackHash, true);
			}
		}
	}

	public override bool Take()
	{
		if (this.m_AttachedToSpear && !Inventory3DManager.Get().IsActive() && Player.Get().m_ControllerToStart != PlayerControllerType.HarvestingSmallAnimal)
		{
			Item item = ItemsManager.Get().CreateItem(this.m_Info.m_ID, false, Vector3.zero, Quaternion.identity);
			item.Take();
			Animator componentDeepChild = General.GetComponentDeepChild<Animator>(item.gameObject);
			if (componentDeepChild)
			{
				componentDeepChild.SetBool(this.m_BackpackHash, true);
			}
			UnityEngine.Object.Destroy(base.gameObject);
			return true;
		}
		return base.Take();
	}

	private int m_BackpackHash = Animator.StringToHash("Backpack");
}
