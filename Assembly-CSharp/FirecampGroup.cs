using System;
using System.Collections.Generic;
using AIs;
using UnityEngine;

public class FirecampGroup
{
	public int GetAllCount()
	{
		return this.m_Firecamps.Count + this.m_SurroundingConstructions.Count;
	}

	public int GetFirecampsCount()
	{
		return this.m_Firecamps.Count;
	}

	public bool Contains(IFireObject firecamp)
	{
		return this.m_Firecamps.Contains(firecamp);
	}

	public bool CanAdd(IFireObject new_firecamp)
	{
		using (List<IFireObject>.Enumerator enumerator = this.m_Firecamps.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetConstruction().transform.position.Distance(new_firecamp.GetConstruction().transform.position) <= 15f)
				{
					return true;
				}
			}
		}
		foreach (Construction construction in this.m_SurroundingConstructions)
		{
			if (construction && construction.transform.position.Distance(new_firecamp.GetConstruction().transform.position) <= 15f)
			{
				return true;
			}
		}
		return false;
	}

	public void Add(FirecampGroup group)
	{
		foreach (IFireObject fireObject in group.m_Firecamps)
		{
			if (!this.Contains(fireObject))
			{
				this.Add(fireObject);
			}
		}
	}

	public void Remove(IFireObject firecamp)
	{
		this.m_Firecamps.Remove(firecamp);
		this.UpdateBounds();
	}

	public void Add(IFireObject new_firecamp)
	{
		this.m_Firecamps.Add(new_firecamp);
		foreach (Construction construction in Construction.s_EnabledConstructions)
		{
			if ((construction.gameObject.GetComponent<IFireObject>() == null || !this.m_Firecamps.Contains((IFireObject)construction)) && !this.m_SurroundingConstructions.Contains(construction) && new_firecamp.GetConstruction().transform.position.Distance(construction.transform.position) <= 15f)
			{
				this.m_SurroundingConstructions.Add(construction);
			}
		}
		this.UpdateBounds();
	}

	private void UpdateBounds()
	{
		if (this.m_Firecamps.Count == 0 && this.m_SurroundingConstructions.Count == 0)
		{
			this.m_Bounds.center = Vector3.zero;
			this.m_Bounds.size = Vector3.zero;
			return;
		}
		this.m_Bounds.center = Vector3.zero;
		if (this.m_Firecamps.Count > 0 && this.m_Firecamps[0] != null)
		{
			this.m_Bounds.center = this.m_Firecamps[0].GetConstruction().transform.position;
		}
		else
		{
			if (!(this.m_SurroundingConstructions[0] != null))
			{
				this.m_Bounds.center = Vector3.zero;
				this.m_Bounds.size = Vector3.zero;
				return;
			}
			this.m_Bounds.center = this.m_SurroundingConstructions[0].transform.position;
		}
		foreach (IFireObject fireObject in this.m_Firecamps)
		{
			if (fireObject != null)
			{
				this.m_Bounds.Encapsulate(fireObject.GetConstruction().transform.position);
			}
		}
		foreach (Construction construction in this.m_SurroundingConstructions)
		{
			if (construction != null)
			{
				this.m_Bounds.Encapsulate(construction.transform.position);
			}
		}
	}

	public void TryAddConstruction(Construction construction)
	{
		if (this.m_SurroundingConstructions.Contains(construction))
		{
			return;
		}
		using (List<IFireObject>.Enumerator enumerator = this.m_Firecamps.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetConstruction().transform.position.Distance(construction.transform.position) <= 15f)
				{
					this.m_SurroundingConstructions.Add(construction);
					return;
				}
			}
		}
		this.UpdateBounds();
	}

	public void TryRemoveConstruction(Construction construction)
	{
		this.m_SurroundingConstructions.Remove(construction);
		this.UpdateBounds();
	}

	public void Update()
	{
		if (EnemyAISpawnManager.Get().m_TimeToNextSpawnWave > 0f)
		{
			this.m_BurningFactor = 0f;
			return;
		}
		bool flag = false;
		using (List<IFireObject>.Enumerator enumerator = this.m_Firecamps.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsBurning())
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			this.m_BurningFactor += Time.deltaTime * FirecampGroupsManager.Get().m_BurningFactorIncreaseSpeed[(int)DifficultySettings.ActivePreset.m_BaseDifficulty];
		}
		else
		{
			this.m_BurningFactor -= Time.deltaTime * FirecampGroupsManager.Get().m_BurningFactorDecreaseSpeed[(int)DifficultySettings.ActivePreset.m_BaseDifficulty];
		}
		this.m_BurningFactor = Mathf.Clamp01(this.m_BurningFactor);
	}

	public void GetAllConstructions(ref List<Construction> constructions)
	{
		constructions.Clear();
		foreach (IFireObject fireObject in this.m_Firecamps)
		{
			constructions.Add(fireObject.GetConstruction());
		}
		foreach (Construction item in this.m_SurroundingConstructions)
		{
			constructions.Add(item);
		}
	}

	public Vector3 GetCenterPos()
	{
		return this.m_Bounds.center;
	}

	private List<IFireObject> m_Firecamps = new List<IFireObject>();

	private List<Construction> m_SurroundingConstructions = new List<Construction>();

	public float m_BurningFactor;

	public Bounds m_Bounds;

	private const float MAX_RANGE = 15f;
}
