using System;
using UnityEngine;

public class BeingModule : MonoBehaviour
{
	private void Start()
	{
	}

	public virtual void Initialize(Being being)
	{
		this.m_Being = being;
	}

	public virtual void PostInitialize()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnTakeDamage(DamageInfo info)
	{
	}

	public virtual void OnDie()
	{
	}

	public virtual void OnDestroy()
	{
		if (this.m_Being)
		{
			this.m_Being.OnDestroyModule(this);
		}
	}

	protected Being m_Being;
}
