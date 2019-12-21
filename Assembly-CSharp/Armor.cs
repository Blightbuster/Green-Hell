using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class Armor : Item
{
	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
		if (HeavyObjectController.Get().IsActive())
		{
			return;
		}
		if (GreenHellGame.IsPadControllerActive() && Inventory3DManager.Get().IsActive() && Inventory3DManager.Get().CanSetCarriedItem(true))
		{
			actions.Add(TriggerAction.TYPE.Pick);
		}
		if (this.m_Info.m_CanBeAddedToInventory)
		{
			actions.Add(TriggerAction.TYPE.Take);
		}
		actions.Add(TriggerAction.TYPE.Expand);
	}

	public override bool CanTrigger()
	{
		return (!this.m_CantTriggerDuringDialog || !DialogsManager.Get().IsAnyDialogPlaying()) && (base.transform.parent == null || this.ReplIsOwner()) && (this.m_Limb == Limb.None || BodyInspectionController.Get().IsActive());
	}

	public override void Load(int index)
	{
		base.Load(index);
		this.m_Limb = (Limb)SaveGame.LoadIVal("ArmorLimb" + index);
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("ArmorLimb" + index, (int)this.m_Limb);
		SaveGame.SaveVal("ArmorLPos" + index, base.transform.localPosition);
		SaveGame.SaveVal("ArmorLRot" + index, base.transform.localRotation);
	}

	public override void SetupAfterLoad(int index)
	{
		base.SetupAfterLoad(index);
		if (this.m_Limb != Limb.None)
		{
			if (this.m_Info.m_ID == ItemID.broken_armor)
			{
				PlayerArmorModule.Get().InsertArmorDesstroyed(this.m_Limb);
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			PlayerArmorModule.Get().m_LimbMap[(int)this.m_Limb].m_Slot.InsertItem(this);
		}
	}

	public override bool CanShowExpandMenu()
	{
		return GreenHellGame.IsPadControllerActive() || (this.m_Info.m_ID != ItemID.broken_armor && this.m_Limb == Limb.None);
	}

	public override bool CanExecuteActions()
	{
		return base.CanExecuteActions() && this.CanShowExpandMenu();
	}

	private void OnTransformParentChanged()
	{
		bool enabled = base.transform.parent == null || !base.transform.parent.root.gameObject.IsRemotePlayer();
		Renderer[] componentsDeepChild = General.GetComponentsDeepChild<Renderer>(base.gameObject);
		for (int i = 0; i < componentsDeepChild.Length; i++)
		{
			componentsDeepChild[i].enabled = enabled;
		}
	}

	public Limb m_Limb = Limb.None;
}
