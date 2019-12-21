using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class ShrimpTrap : Trap
{
	protected override void Awake()
	{
		base.Awake();
		this.m_Dummies = new List<TrapAIDummy>(base.GetComponentsInChildren<TrapAIDummy>());
		base.SetArmed(true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	protected override bool IsShrimpTrap()
	{
		return true;
	}

	public override void UpdateEffect()
	{
	}

	public override void ConstantUpdate()
	{
		foreach (TrapAIDummy trapAIDummy in this.m_Dummies)
		{
			if (trapAIDummy.m_Object && trapAIDummy.m_Object.transform.parent != base.transform)
			{
				trapAIDummy.m_Object = null;
			}
		}
		float num = Time.deltaTime;
		if (SleepController.Get().IsActive() && !SleepController.Get().IsWakingUp())
		{
			num = Player.GetSleepTimeFactor();
		}
		this.m_TimeFromLastCheck += num;
		if (this.m_TimeFromLastCheck >= this.m_CheckInterval)
		{
			this.Check();
			this.m_TimeFromLastCheck = 0f;
		}
	}

	public void Check()
	{
		if (UnityEngine.Random.Range(0f, 1f) < (this.m_Bait ? this.m_ChanceToCatchWithBait : this.m_ChanceToCatch))
		{
			this.Catch();
		}
	}

	public override void Catch()
	{
		if (this.m_Dummies.Count == 0)
		{
			return;
		}
		TrapAIDummy trapAIDummy = null;
		foreach (TrapAIDummy trapAIDummy2 in this.m_Dummies)
		{
			if (trapAIDummy2.m_Object == null)
			{
				trapAIDummy = trapAIDummy2;
				break;
			}
		}
		if (!trapAIDummy)
		{
			return;
		}
		Item item = ItemsManager.Get().CreateItem(this.m_ItemID, true, trapAIDummy.transform.position, trapAIDummy.transform.rotation);
		item.transform.parent = base.transform;
		item.StaticPhxRequestAdd();
		Physics.IgnoreCollision(item.m_BoxCollider, this.m_BoxCollider);
		trapAIDummy.m_Object = item.gameObject;
		if (this.m_Bait && this.m_Bait.m_Item)
		{
			this.m_Bait.DeleteItem();
		}
	}

	public override bool CanTrigger()
	{
		if (this.m_CantTriggerDuringDialog)
		{
			DialogsManager.Get().IsAnyDialogPlaying();
			return false;
		}
		return false;
	}

	public override void GetActions(List<TriggerAction.TYPE> actions)
	{
	}

	public override void OnExecute(TriggerAction.TYPE action)
	{
	}

	public override void Save(int index)
	{
		base.Save(index);
		SaveGame.SaveVal("ShrimpsCount" + index, this.m_Dummies.Count);
	}

	public override void Load(int index)
	{
		base.Load(index);
		int num = SaveGame.LoadIVal("ShrimpsCount" + index);
		for (int i = 0; i < num; i++)
		{
		}
	}

	public override bool TriggerThrough()
	{
		return true;
	}

	private List<TrapAIDummy> m_Dummies;

	public ItemID m_ItemID = ItemID.None;

	private float m_TimeFromLastCheck;
}
