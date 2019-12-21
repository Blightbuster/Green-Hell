using System;
using CJTools;
using UnityEngine;

public class ReplicatedPlayerSpineRotator : ReplicatedBehaviour
{
	private void Awake()
	{
		DebugUtils.Assert(ReplicatedPlayerSpineRotator.BONES_FPP.Length <= ReplicatedPlayerSpineRotator.BONES_TPP.Length, true);
		if (base.ReplIsOwner())
		{
			Transform transform = Player.Get().gameObject.transform;
			this.m_BonesFPP = new Transform[ReplicatedPlayerSpineRotator.BONES_FPP.Length];
			for (int i = 0; i < ReplicatedPlayerSpineRotator.BONES_FPP.Length; i++)
			{
				this.m_BonesFPP[i] = transform.FindDeepChild(ReplicatedPlayerSpineRotator.BONES_FPP[i]);
			}
			return;
		}
		Transform transform2 = base.gameObject.transform;
		this.m_BonesTPP = new Transform[ReplicatedPlayerSpineRotator.BONES_TPP.Length];
		this.m_SmoothRotTPP = new SpringFloat[ReplicatedPlayerSpineRotator.BONES_TPP.Length];
		for (int j = 0; j < ReplicatedPlayerSpineRotator.BONES_TPP.Length; j++)
		{
			this.m_SmoothRotTPP[j].Init(0f, 1f);
			this.m_BonesTPP[j] = transform2.FindDeepChild(ReplicatedPlayerSpineRotator.BONES_TPP[j]);
		}
	}

	public virtual void OnReplicationPrepare_CJGenerated()
	{
		int num = (this.m_Rotation_Repl == null) ? 0 : this.m_Rotation_Repl.Length;
		int num2 = (this.m_Rotation == null) ? 0 : this.m_Rotation.Length;
		if (num != num2)
		{
			if (num2 == 0)
			{
				this.m_Rotation_Repl = null;
			}
			else
			{
				this.m_Rotation_Repl = new float[num2];
			}
			this.ReplSetDirty();
		}
		for (int i = 0; i < this.m_Rotation.Length; i++)
		{
			if (System.Math.Abs(this.m_Rotation_Repl[i] - this.m_Rotation[i]) >= 0.1f)
			{
				this.m_Rotation_Repl[i] = this.m_Rotation[i];
				this.ReplSetDirty();
			}
		}
	}

	public virtual void OnReplicationSerialize_CJGenerated(P2PNetworkWriter writer, bool initial_state)
	{
		if (this.m_Rotation_Repl == null)
		{
			writer.WritePackedUInt32(0u);
			return;
		}
		writer.WritePackedUInt32((uint)this.m_Rotation_Repl.Length);
		for (int i = 0; i < this.m_Rotation_Repl.Length; i++)
		{
			writer.Write(this.m_Rotation_Repl[i]);
		}
	}

	public virtual void OnReplicationDeserialize_CJGenerated(P2PNetworkReader reader, bool initial_state)
	{
		int num = (int)reader.ReadPackedUInt32();
		if (this.m_Rotation_Repl == null || this.m_Rotation_Repl.Length != num)
		{
			this.m_Rotation_Repl = new float[num];
		}
		for (int i = 0; i < num; i++)
		{
			this.m_Rotation_Repl[i] = reader.ReadFloat();
		}
	}

	public virtual void OnReplicationResolve_CJGenerated()
	{
		if (this.m_Rotation_Repl != null && (this.m_Rotation == null || this.m_Rotation.Length != this.m_Rotation_Repl.Length))
		{
			this.m_Rotation = new float[this.m_Rotation_Repl.Length];
		}
		for (int i = 0; i < this.m_Rotation.Length; i++)
		{
			this.m_Rotation[i] = this.m_Rotation_Repl[i];
		}
	}

	private static readonly string[] BONES_FPP = new string[]
	{
		"mixamorig:Spine",
		"mixamorig:Spine1",
		"mixamorig:Spine2"
	};

	private static readonly string[] BONES_TPP = new string[]
	{
		"spine1",
		"spine2",
		"spine3",
		"neck"
	};

	[Replicate(new string[]
	{

	})]
	private float[] m_Rotation = new float[ReplicatedPlayerSpineRotator.BONES_FPP.Length];

	private Transform[] m_BonesFPP;

	private Transform[] m_BonesTPP;

	private SpringFloat[] m_SmoothRotTPP;

	private float[] m_Rotation_Repl;
}
