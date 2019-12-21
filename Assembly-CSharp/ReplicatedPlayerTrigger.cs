using System;
using System.Collections.Generic;
using UnityEngine;

public class ReplicatedPlayerTrigger : ReplicatedBehaviour
{
	public override void OnReplicationPrepare()
	{
		base.OnReplicationPrepare();
		Trigger bestTrigger = TriggerController.Get().GetBestTrigger();
		GameObject gameObject = (bestTrigger != null) ? bestTrigger.gameObject : null;
		GameObject replObject = ReplicatedPlayerTrigger.GetReplObject(gameObject);
		if (this.m_BestTriggerReplObj != replObject)
		{
			this.m_BestTriggerReplObj = replObject;
			this.UpdateChildIdx(gameObject, this.m_BestTriggerChildIdx);
		}
		Trigger executedTrigger = this.GetExecutedTrigger();
		GameObject gameObject2 = (executedTrigger != null) ? executedTrigger.gameObject : null;
		GameObject replObject2 = ReplicatedPlayerTrigger.GetReplObject(gameObject2);
		if (this.m_ExecutedTriggerReplObj != replObject2)
		{
			this.m_ExecutedTriggerReplObj = replObject2;
			this.UpdateChildIdx(gameObject2, this.m_BestTriggerChildIdx);
		}
	}

	public override void OnReplicationResolve()
	{
		base.OnReplicationResolve();
		this.m_BestTrigger = this.GetTriggerObjectFromReplObject(this.m_BestTriggerReplObj_Repl, this.m_BestTriggerChildIdx_Repl);
		this.m_ExecutedTrigger = this.GetTriggerObjectFromReplObject(this.m_ExecutedTriggerReplObj_Repl, this.m_ExecutedTriggerChildIdx_Repl);
	}

	private Trigger GetExecutedTrigger()
	{
		Trigger trigger = HarvestingAnimalController.Get().GetBody();
		if (!trigger)
		{
			trigger = TriggerController.Get().m_TriggerToExecute;
		}
		return trigger;
	}

	public bool IsBestTrigger(Trigger trigger)
	{
		return trigger != null && this.m_BestTrigger == trigger.gameObject;
	}

	public bool IsExecutedTrigger(Trigger trigger)
	{
		return trigger != null && this.m_ExecutedTrigger == trigger.gameObject;
	}

	private void UpdateChildIdx(GameObject trigger_obj, List<int> index_list)
	{
		index_list.Clear();
		GameObject gameObject = trigger_obj;
		if (gameObject == null)
		{
			return;
		}
		if (gameObject.ReplIsReplicable())
		{
			return;
		}
		while (gameObject != null)
		{
			Transform parent = gameObject.transform.parent;
			GameObject gameObject2 = (parent != null) ? parent.gameObject : null;
			if (gameObject2)
			{
				index_list.Add(this.GetObjChildIndex(gameObject, gameObject2));
			}
			gameObject = gameObject2;
			if (gameObject && gameObject.ReplIsReplicable())
			{
				return;
			}
		}
		DebugUtils.Assert(false, "No replicated parent found for " + trigger_obj.name + "!", true, DebugUtils.AssertType.Info);
	}

	private GameObject GetTriggerObjectFromReplObject(GameObject obj, List<int> index_list)
	{
		if (obj != null)
		{
			for (int i = index_list.Count - 1; i >= 0; i--)
			{
				Transform child = obj.transform.GetChild(index_list[i]);
				obj = ((child != null) ? child.gameObject : null);
				DebugUtils.Assert(obj != null, true);
			}
		}
		return obj;
	}

	private int GetObjChildIndex(GameObject obj, GameObject parent)
	{
		for (int i = 0; i < parent.transform.childCount; i++)
		{
			if (parent.transform.GetChild(i).gameObject == obj)
			{
				return i;
			}
		}
		DebugUtils.Assert(false, true);
		return -1;
	}

	private static GameObject GetReplObject(GameObject obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj.ReplIsReplicable())
		{
			return obj;
		}
		while (obj != null)
		{
			Transform parent = obj.transform.parent;
			obj = ((parent != null) ? parent.gameObject : null);
			if (obj != null && obj.ReplIsReplicable())
			{
				return obj;
			}
		}
		return null;
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		if (this.m_BestTriggerReplObj_Repl != this.m_BestTriggerReplObj)
		{
			this.m_BestTriggerReplObj_Repl = this.m_BestTriggerReplObj;
			this.ReplSetDirty();
		}
		if (this.m_ExecutedTriggerReplObj_Repl != this.m_ExecutedTriggerReplObj)
		{
			this.m_ExecutedTriggerReplObj_Repl = this.m_ExecutedTriggerReplObj;
			this.ReplSetDirty();
		}
		int num = (this.m_BestTriggerChildIdx_Repl == null) ? 0 : this.m_BestTriggerChildIdx_Repl.Count;
		int num2 = (this.m_BestTriggerChildIdx == null) ? 0 : this.m_BestTriggerChildIdx.Count;
		if (num != num2)
		{
			if (num2 == 0)
			{
				this.m_BestTriggerChildIdx_Repl = null;
			}
			else
			{
				this.m_BestTriggerChildIdx_Repl = new List<int>(num2);
			}
			this.ReplSetDirty();
		}
		for (int i = 0; i < this.m_BestTriggerChildIdx.Count; i++)
		{
			if (this.m_BestTriggerChildIdx_Repl.Count != this.m_BestTriggerChildIdx.Count)
			{
				this.m_BestTriggerChildIdx_Repl.Add(this.m_BestTriggerChildIdx[i]);
			}
			else if (this.m_BestTriggerChildIdx_Repl[i] != this.m_BestTriggerChildIdx[i])
			{
				this.m_BestTriggerChildIdx_Repl[i] = this.m_BestTriggerChildIdx[i];
				this.ReplSetDirty();
			}
		}
		int num3 = (this.m_ExecutedTriggerChildIdx_Repl == null) ? 0 : this.m_ExecutedTriggerChildIdx_Repl.Count;
		int num4 = (this.m_ExecutedTriggerChildIdx == null) ? 0 : this.m_ExecutedTriggerChildIdx.Count;
		if (num3 != num4)
		{
			if (num4 == 0)
			{
				this.m_ExecutedTriggerChildIdx_Repl = null;
			}
			else
			{
				this.m_ExecutedTriggerChildIdx_Repl = new List<int>(num4);
			}
			this.ReplSetDirty();
		}
		for (int j = 0; j < this.m_ExecutedTriggerChildIdx.Count; j++)
		{
			if (this.m_ExecutedTriggerChildIdx_Repl.Count != this.m_ExecutedTriggerChildIdx.Count)
			{
				this.m_ExecutedTriggerChildIdx_Repl.Add(this.m_ExecutedTriggerChildIdx[j]);
			}
			else if (this.m_ExecutedTriggerChildIdx_Repl[j] != this.m_ExecutedTriggerChildIdx[j])
			{
				this.m_ExecutedTriggerChildIdx_Repl[j] = this.m_ExecutedTriggerChildIdx[j];
				this.ReplSetDirty();
			}
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		writer.Write(this.m_BestTriggerReplObj_Repl);
		writer.Write(this.m_ExecutedTriggerReplObj_Repl);
		if (this.m_BestTriggerChildIdx_Repl == null)
		{
			writer.WritePackedUInt32(0u);
		}
		else
		{
			writer.WritePackedUInt32((uint)this.m_BestTriggerChildIdx_Repl.Count);
			for (int i = 0; i < this.m_BestTriggerChildIdx_Repl.Count; i++)
			{
				writer.Write(this.m_BestTriggerChildIdx_Repl[i]);
			}
		}
		if (this.m_ExecutedTriggerChildIdx_Repl == null)
		{
			writer.WritePackedUInt32(0u);
			return;
		}
		writer.WritePackedUInt32((uint)this.m_ExecutedTriggerChildIdx_Repl.Count);
		for (int j = 0; j < this.m_ExecutedTriggerChildIdx_Repl.Count; j++)
		{
			writer.Write(this.m_ExecutedTriggerChildIdx_Repl[j]);
		}
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		this.m_BestTriggerReplObj_Repl = reader.ReadGameObject();
		this.m_ExecutedTriggerReplObj_Repl = reader.ReadGameObject();
		int num = (int)reader.ReadPackedUInt32();
		if (this.m_BestTriggerChildIdx_Repl == null || this.m_BestTriggerChildIdx_Repl.Count != num)
		{
			this.m_BestTriggerChildIdx_Repl = new List<int>(num);
		}
		for (int i = 0; i < num; i++)
		{
			if (this.m_BestTriggerChildIdx_Repl.Count != num)
			{
				int item = reader.ReadInt32();
				this.m_BestTriggerChildIdx_Repl.Add(item);
			}
			else
			{
				this.m_BestTriggerChildIdx_Repl[i] = reader.ReadInt32();
			}
		}
		int num2 = (int)reader.ReadPackedUInt32();
		if (this.m_ExecutedTriggerChildIdx_Repl == null || this.m_ExecutedTriggerChildIdx_Repl.Count != num2)
		{
			this.m_ExecutedTriggerChildIdx_Repl = new List<int>(num2);
		}
		for (int j = 0; j < num2; j++)
		{
			if (this.m_ExecutedTriggerChildIdx_Repl.Count != num2)
			{
				int item2 = reader.ReadInt32();
				this.m_ExecutedTriggerChildIdx_Repl.Add(item2);
			}
			else
			{
				this.m_ExecutedTriggerChildIdx_Repl[j] = reader.ReadInt32();
			}
		}
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		this.m_BestTriggerReplObj = this.m_BestTriggerReplObj_Repl;
		this.m_ExecutedTriggerReplObj = this.m_ExecutedTriggerReplObj_Repl;
		if (this.m_BestTriggerChildIdx_Repl != null && (this.m_BestTriggerChildIdx == null || this.m_BestTriggerChildIdx.Count != this.m_BestTriggerChildIdx_Repl.Count))
		{
			this.m_BestTriggerChildIdx = new List<int>(this.m_BestTriggerChildIdx_Repl.Count);
		}
		for (int i = 0; i < this.m_BestTriggerChildIdx.Count; i++)
		{
			this.m_BestTriggerChildIdx[i] = this.m_BestTriggerChildIdx_Repl[i];
		}
		if (this.m_ExecutedTriggerChildIdx_Repl != null && (this.m_ExecutedTriggerChildIdx == null || this.m_ExecutedTriggerChildIdx.Count != this.m_ExecutedTriggerChildIdx_Repl.Count))
		{
			this.m_ExecutedTriggerChildIdx = new List<int>(this.m_ExecutedTriggerChildIdx_Repl.Count);
		}
		for (int j = 0; j < this.m_ExecutedTriggerChildIdx.Count; j++)
		{
			this.m_ExecutedTriggerChildIdx[j] = this.m_ExecutedTriggerChildIdx_Repl[j];
		}
	}

	[Replicate(new string[]
	{

	})]
	private GameObject m_BestTriggerReplObj;

	[Replicate(new string[]
	{

	})]
	private GameObject m_ExecutedTriggerReplObj;

	[Replicate(new string[]
	{

	})]
	private List<int> m_BestTriggerChildIdx = new List<int>();

	[Replicate(new string[]
	{

	})]
	private List<int> m_ExecutedTriggerChildIdx = new List<int>();

	private GameObject m_BestTrigger;

	private GameObject m_ExecutedTrigger;

	private GameObject m_BestTriggerReplObj_Repl;

	private GameObject m_ExecutedTriggerReplObj_Repl;

	private List<int> m_BestTriggerChildIdx_Repl;

	private List<int> m_ExecutedTriggerChildIdx_Repl;
}
