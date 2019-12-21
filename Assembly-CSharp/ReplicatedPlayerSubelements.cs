using System;
using UnityEngine;

public class ReplicatedPlayerSubelements : ReplicatedBehaviour
{
	private void Awake()
	{
		this.m_LocalPlayerModel = Player.Get().transform.GetChild(0);
		this.m_NetworkPlayerModel = base.transform.GetChild(0);
		this.m_ReplActiveElements = new bool[this.m_LocalPlayerModel.childCount];
		if (base.ReplIsOwner())
		{
			this.m_ReplActiveElementsHash = new int[this.m_LocalPlayerModel.childCount];
			for (int i = 0; i < this.m_LocalPlayerModel.childCount; i++)
			{
				this.m_ReplActiveElementsHash[i] = this.m_LocalPlayerModel.GetChild(i).name.GetHashCode();
			}
			this.UpdateActiveElements();
		}
	}

	private void UpdateActiveElements()
	{
		for (int i = 0; i < this.m_LocalPlayerModel.childCount; i++)
		{
			bool activeSelf = this.m_LocalPlayerModel.GetChild(i).gameObject.activeSelf;
			if (activeSelf != this.m_ReplActiveElements[i])
			{
				this.m_ReplActiveElements[i] = activeSelf;
				this.ReplSetDirty();
			}
		}
	}

	public override void OnReplicationPrepare()
	{
		this.UpdateActiveElements();
	}

	public override void OnReplicationResolve()
	{
		for (int i = 0; i < this.m_ReplActiveElements.Length; i++)
		{
			if (this.m_ReplActiveElements_Repl[i] != this.m_ReplActiveElements[i])
			{
				int j = 0;
				while (j < this.m_NetworkPlayerModel.childCount)
				{
					if (this.m_NetworkPlayerModel.GetChild(j).name.GetHashCode() == this.m_ReplActiveElementsHash[i])
					{
						if (this.m_NetworkPlayerModel.GetChild(j).gameObject.activeSelf != this.m_ReplActiveElements_Repl[i])
						{
							this.m_NetworkPlayerModel.GetChild(j).gameObject.SetActive(this.m_ReplActiveElements_Repl[i]);
							break;
						}
						break;
					}
					else
					{
						j++;
					}
				}
			}
		}
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		int num = (this.m_ReplActiveElements_Repl == null) ? 0 : this.m_ReplActiveElements_Repl.Length;
		int num2 = (this.m_ReplActiveElements == null) ? 0 : this.m_ReplActiveElements.Length;
		if (num != num2)
		{
			if (num2 == 0)
			{
				this.m_ReplActiveElements_Repl = null;
			}
			else
			{
				this.m_ReplActiveElements_Repl = new bool[num2];
			}
			this.ReplSetDirty();
		}
		for (int i = 0; i < this.m_ReplActiveElements.Length; i++)
		{
			if (this.m_ReplActiveElements_Repl[i] != this.m_ReplActiveElements[i])
			{
				this.m_ReplActiveElements_Repl[i] = this.m_ReplActiveElements[i];
				this.ReplSetDirty();
			}
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		if (this.m_ReplActiveElements_Repl == null)
		{
			writer.WritePackedUInt32(0u);
		}
		else
		{
			writer.WritePackedUInt32((uint)this.m_ReplActiveElements_Repl.Length);
			for (int i = 0; i < this.m_ReplActiveElements_Repl.Length; i++)
			{
				writer.Write(this.m_ReplActiveElements_Repl[i]);
			}
		}
		if (initial_state)
		{
			if (this.m_ReplActiveElementsHash == null)
			{
				writer.WritePackedUInt32(0u);
				return;
			}
			writer.WritePackedUInt32((uint)this.m_ReplActiveElementsHash.Length);
			for (int j = 0; j < this.m_ReplActiveElementsHash.Length; j++)
			{
				writer.Write(this.m_ReplActiveElementsHash[j]);
			}
		}
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		int num = (int)reader.ReadPackedUInt32();
		if (this.m_ReplActiveElements_Repl == null || this.m_ReplActiveElements_Repl.Length != num)
		{
			this.m_ReplActiveElements_Repl = new bool[num];
		}
		for (int i = 0; i < num; i++)
		{
			this.m_ReplActiveElements_Repl[i] = reader.ReadBoolean();
		}
		if (initial_state)
		{
			int num2 = (int)reader.ReadPackedUInt32();
			if (this.m_ReplActiveElementsHash == null || this.m_ReplActiveElementsHash.Length != num2)
			{
				this.m_ReplActiveElementsHash = new int[num2];
			}
			for (int j = 0; j < num2; j++)
			{
				this.m_ReplActiveElementsHash[j] = reader.ReadInt32();
			}
		}
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		if (this.m_ReplActiveElements_Repl != null && (this.m_ReplActiveElements == null || this.m_ReplActiveElements.Length != this.m_ReplActiveElements_Repl.Length))
		{
			this.m_ReplActiveElements = new bool[this.m_ReplActiveElements_Repl.Length];
		}
		for (int i = 0; i < this.m_ReplActiveElements.Length; i++)
		{
			this.m_ReplActiveElements[i] = this.m_ReplActiveElements_Repl[i];
		}
	}

	private Transform m_LocalPlayerModel;

	private Transform m_NetworkPlayerModel;

	[Replicate(new string[]
	{

	})]
	private bool[] m_ReplActiveElements;

	[Replicate(new string[]
	{
		"initial_state_only"
	})]
	private int[] m_ReplActiveElementsHash;

	private const int CHILD_MODEL_INDEX = 0;

	private bool[] m_ReplActiveElements_Repl;

	private int[] m_ReplActiveElementsHash_Repl;
}
