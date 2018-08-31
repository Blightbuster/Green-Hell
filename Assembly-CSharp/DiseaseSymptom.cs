using System;
using Enums;
using UnityEngine;

public class DiseaseSymptom
{
	public virtual void Initialize()
	{
	}

	public virtual void ParseKey(Key key)
	{
	}

	public void SetPlayerDiseasesModule(PlayerDiseasesModule module)
	{
		this.m_PlayerDiseasesModule = module;
	}

	public Enums.DiseaseSymptom GetSymptomType()
	{
		return this.m_Type;
	}

	public void Activate()
	{
		this.m_Active = true;
		this.OnActivate();
	}

	protected virtual void OnActivate()
	{
		this.m_StartTime = Time.time;
	}

	public void Deactivate()
	{
		this.m_Active = false;
		this.OnDeactivate();
	}

	protected virtual void OnDeactivate()
	{
	}

	public bool IsActive()
	{
		return this.m_Active;
	}

	public virtual void Update()
	{
		if (this.m_Duration > 0f && Time.time - this.m_StartTime >= this.m_Duration)
		{
			this.Deactivate();
		}
	}

	public virtual void OnEat(ConsumableInfo info)
	{
	}

	public virtual void OnDrink(LiquidType type, float hydration_amount)
	{
	}

	private bool m_Active;

	protected float m_StartTime;

	protected float m_Duration;

	public Enums.DiseaseSymptom m_Type = Enums.DiseaseSymptom.None;

	private PlayerDiseasesModule m_PlayerDiseasesModule;
}
