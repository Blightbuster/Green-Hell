using System;
using UnityEngine;

public struct CachedComponent<T> where T : Component
{
	public CachedComponent(GameObject owner)
	{
		this.m_Checked = false;
		this.m_Owner = owner;
		this.m_Component = default(T);
	}

	public CachedComponent(T component)
	{
		this.m_Checked = true;
		T t = component;
		this.m_Owner = ((t != null) ? t.gameObject : null);
		this.m_Component = component;
	}

	public T Get()
	{
		if (!this.m_Checked)
		{
			DebugUtils.Assert(this.m_Owner != null, "CachedComponent was not initialized!", true, DebugUtils.AssertType.Info);
			this.m_Component = this.m_Owner.GetComponent<T>();
			this.m_Checked = true;
		}
		return this.m_Component;
	}

	public T Get(Component owner)
	{
		if (!this.m_Checked)
		{
			DebugUtils.Assert(owner != null, "CachedComponent owner not set!", true, DebugUtils.AssertType.Info);
			this.m_Component = owner.GetComponent<T>();
			this.m_Checked = true;
		}
		return this.m_Component;
	}

	public static implicit operator T(CachedComponent<T> c)
	{
		return c.Get();
	}

	public static implicit operator CachedComponent<T>(T c)
	{
		return new CachedComponent<T>(c);
	}

	private bool m_Checked;

	private T m_Component;

	private readonly GameObject m_Owner;
}
