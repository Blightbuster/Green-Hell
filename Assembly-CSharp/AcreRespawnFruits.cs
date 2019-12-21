using System;
using System.Collections.Generic;
using UnityEngine;

public class AcreRespawnFruits : MonoBehaviour
{
	private void Start()
	{
		Item component = base.GetComponent<Item>();
		if (component)
		{
			component.m_IsCut = true;
		}
	}

	public void SetAcre(Acre acre)
	{
		this.m_Acre = acre;
		this.OnSetAcre();
	}

	private void OnSetAcre()
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			Item component = this.m_ToCollect[i].GetComponent<Item>();
			if (component != null)
			{
				component.m_Acre = this.m_Acre;
			}
			else
			{
				PlantFruit component2 = this.m_ToCollect[i].GetComponent<PlantFruit>();
				if (component2 != null)
				{
					component2.m_Acre = this.m_Acre;
				}
				else
				{
					ItemReplacer component3 = this.m_ToCollect[i].GetComponent<ItemReplacer>();
					if (component3 != null)
					{
						component3.m_Acre = this.m_Acre;
					}
				}
			}
		}
	}

	public void UpdateInternal()
	{
		if (this.m_Acre.GetState() != AcreState.GrownNoFruits)
		{
			for (int i = 0; i < this.m_ToCollect.Count; i++)
			{
				if (this.m_ToCollect[i] != null)
				{
					return;
				}
			}
			this.m_Acre.SetState(AcreState.GrownNoFruits);
			this.m_TimeToRespawn = this.m_RespawnDuration;
			return;
		}
		if (this.m_Acre.GetState() == AcreState.GrownNoFruits && this.m_Acre.m_WaterAmount > 1f)
		{
			this.m_TimeToRespawn -= MainLevel.s_GameTimeDelta;
			if (this.m_TimeToRespawn <= 0f)
			{
				this.m_Acre.m_ForceNoRespawn = true;
				this.m_Acre.SetState(AcreState.Grown);
				this.m_TimeToRespawn = this.m_RespawnDuration;
			}
		}
	}

	public void OnTake(Item item)
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			if (this.m_ToCollect[i] != null && this.m_ToCollect[i] == item.gameObject)
			{
				this.m_ToCollect[i] = null;
			}
		}
	}

	public void OnEat(Item item)
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			if (this.m_ToCollect[i] != null && this.m_ToCollect[i] == item.gameObject)
			{
				this.m_ToCollect[i] = null;
			}
		}
	}

	public void OnTake(PlantFruit item)
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			if (this.m_ToCollect[i] != null && this.m_ToCollect[i] == item.gameObject)
			{
				this.m_ToCollect[i] = null;
			}
		}
	}

	public void OnEat(PlantFruit item)
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			if (this.m_ToCollect[i] != null && this.m_ToCollect[i] == item.gameObject)
			{
				this.m_ToCollect[i] = null;
			}
		}
	}

	public void OnTake(ItemReplacer item)
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			if (this.m_ToCollect[i] != null && this.m_ToCollect[i] == item.gameObject)
			{
				this.m_ToCollect[i] = null;
			}
		}
	}

	public float GetRespawnProgress()
	{
		return 1f - this.m_TimeToRespawn / this.m_RespawnDuration;
	}

	public void OnDestroyPlant()
	{
		for (int i = 0; i < this.m_ToCollect.Count; i++)
		{
			if (this.m_ToCollect[i] != null)
			{
				PlantFruit component = this.m_ToCollect[i].GetComponent<PlantFruit>();
				if (component && component.m_ItemInfo != null)
				{
					ItemsManager.Get().CreateItem(component.m_ItemInfo.m_ID, true, this.m_ToCollect[i].transform);
				}
				else
				{
					ItemReplacer component2 = this.m_ToCollect[i].GetComponent<ItemReplacer>();
					if (component2)
					{
						ItemsManager.Get().CreateItem(component2.m_ReplaceInfo.m_ID, true, this.m_ToCollect[i].transform);
					}
					else
					{
						Food component3 = this.m_ToCollect[i].GetComponent<Food>();
						if (component3)
						{
							component3.transform.SetParent(null);
						}
					}
				}
			}
		}
	}

	private Acre m_Acre;

	public List<GameObject> m_ToCollect = new List<GameObject>();

	[HideInInspector]
	public float m_TimeToRespawn;

	public float m_RespawnDuration = 10f;
}
