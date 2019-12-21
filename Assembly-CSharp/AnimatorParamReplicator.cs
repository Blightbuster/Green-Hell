using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimatorParamReplicator<T> : IAnimatorParamReplicator, IComparer<T> where T : IComparable<T>
{
	protected Animator LocalAnimator
	{
		get
		{
			return this.m_Owner.LocalSourceAnimator;
		}
	}

	protected Animator NetAnimator
	{
		get
		{
			return this.m_Owner.NetAnimator;
		}
	}

	public virtual int Compare(T x, T y)
	{
		return x.CompareTo(y);
	}

	public AnimatorParamReplicator(string param_name, ReplicatedAnimator owner)
	{
		this.m_ParamName = param_name;
		this.m_Owner = owner;
	}

	protected abstract T GetLocalValue();

	protected abstract void SetNetValue(T val);

	public abstract void Serialize(P2PNetworkWriter writer);

	public abstract void Deserialize(P2PNetworkReader reader);

	public virtual void Update(float dt)
	{
	}

	public virtual void TestUpdate()
	{
		this.SetNetValue(this.GetLocalValue());
	}

	public void UpdateCurrent()
	{
		if (this.m_Owner.ReplIsOwner())
		{
			T localValue = this.GetLocalValue();
			if (this.Compare(localValue, this.m_PrevValue) != 0)
			{
				this.m_PrevValue = localValue;
				this.m_Owner.OnParamChanged();
			}
		}
	}

	protected readonly string m_ParamName;

	protected T m_PrevValue;

	protected T m_ReplicatedValue;

	protected ReplicatedAnimator m_Owner;
}
