using System;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class Storage3D : MonoBehaviour
{
	public static Storage3D Get()
	{
		return Storage3D.s_Instance;
	}

	private void Awake()
	{
		Storage3D.s_Instance = this;
		for (int i = 0; i < 2; i++)
		{
			StorageData storageData = new StorageData();
			storageData.m_Type = (Storage3D.StorageType)i;
			storageData.m_Grid = new PocketGrid();
			storageData.m_Model = base.transform.Find(storageData.m_Type.ToString()).gameObject;
			storageData.m_Plane = storageData.m_Model.transform.GetChild(0).GetComponent<Collider>();
			storageData.m_Grid.m_Grid = storageData.m_Plane.gameObject;
			Storage3D.StorageType type = storageData.m_Type;
			if (type != Storage3D.StorageType.Box)
			{
				if (type == Storage3D.StorageType.Bag)
				{
					storageData.m_Grid.m_GridSize = new Vector2(5f, 7.5f);
				}
			}
			else
			{
				storageData.m_Grid.m_GridSize = new Vector2(10f, 15f);
			}
			storageData.m_Grid.Initialize(this.m_GridCellPrefab, BackpackPocket.Storage);
			this.m_Datas.Add(storageData);
		}
		base.gameObject.SetActive(false);
	}

	public bool IsActive()
	{
		return base.gameObject && base.gameObject.activeSelf;
	}

	public void SetupGrid()
	{
		if (!this.m_Storage)
		{
			return;
		}
		foreach (StorageData storageData in this.m_Datas)
		{
			if (storageData.m_Type == this.m_Storage.m_Type)
			{
				storageData.m_Grid.Setup();
				break;
			}
		}
	}

	public bool CanInsertItem(Item item)
	{
		return item && item.m_Info.m_CanBeRemovedFromInventory && !item.m_Info.IsHeavyObject() && (item.m_Info.m_BackpackPocket == BackpackPocket.Main || item.m_Info.m_BackpackPocket == BackpackPocket.Front) && item.m_Info.m_CanBePlacedInStorage;
	}

	public void Activate(Storage storage)
	{
		if (base.gameObject.activeSelf)
		{
			return;
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			Inventory3DManager.Get().Activate();
		}
		if (!Inventory3DManager.Get().gameObject.activeSelf)
		{
			return;
		}
		this.m_Storage = storage;
		this.m_Storage.Open();
		foreach (Item item in base.transform.GetComponentsInChildren<Item>(true))
		{
			item.gameObject.SetActive(this.m_Storage.Contains(item));
		}
		this.m_StartDist = this.m_Storage.transform.position.Distance(Player.Get().transform.position);
		if (CraftingManager.Get().IsActive())
		{
			CraftingManager.Get().Deactivate();
		}
		base.gameObject.SetActive(true);
		foreach (StorageData storageData in this.m_Datas)
		{
			if (storageData.m_Type == this.m_Storage.m_Type)
			{
				storageData.m_Model.SetActive(true);
				this.m_ActiveData = storageData;
			}
			else
			{
				storageData.m_Model.SetActive(false);
			}
		}
		this.SetupGrid();
	}

	public void Deactivate()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		this.m_ActiveData.m_Grid.OnSetSelectedGroup(null);
		if (this.m_Storage)
		{
			this.m_Storage.Close();
		}
		base.gameObject.SetActive(false);
	}

	public InsertResult InsertItem(Item item, ItemSlot slot, InventoryCellsGroup group, bool notify_if_cant = true, bool drop_if_cant = true)
	{
		if (!this.m_Storage)
		{
			return InsertResult.CantInsert;
		}
		return this.m_Storage.InsertItem(item, slot, group, notify_if_cant, drop_if_cant);
	}

	public void RemoveItem(Item item, bool from_destroy = false)
	{
		if (this.m_Storage)
		{
			this.m_Storage.RemoveItem(item, from_destroy);
		}
	}

	public InventoryCellsGroup FindBestGroup()
	{
		return this.m_ActiveData.m_Grid.FindBestGroup();
	}

	public void OnSetSelectedGroup(InventoryCellsGroup group)
	{
		this.m_ActiveData.m_Grid.OnSetSelectedGroup(group);
	}

	public PocketGrid GetGrid(Storage3D.StorageType type)
	{
		foreach (StorageData storageData in this.m_Datas)
		{
			if (storageData.m_Type == type)
			{
				return storageData.m_Grid;
			}
		}
		return null;
	}

	public void ResetGrids()
	{
		foreach (StorageData storageData in this.m_Datas)
		{
			storageData.m_Grid.Reset();
		}
	}

	private void Update()
	{
		if (!this.m_Storage)
		{
			return;
		}
		if (this.m_Storage.transform.position.Distance(Player.Get().transform.position) > this.m_StartDist + 0.5f)
		{
			this.Deactivate();
			return;
		}
		if (Vector3.Dot(Camera.main.transform.forward, (this.m_Storage.transform.position - Camera.main.transform.position).normalized) < 0f)
		{
			this.Deactivate();
			return;
		}
	}

	public GameObject m_GridCellPrefab;

	[HideInInspector]
	public StorageData m_ActiveData;

	private List<StorageData> m_Datas = new List<StorageData>();

	[HideInInspector]
	public Storage m_Storage;

	private float m_StartDist;

	private bool m_DebugRenderGrid;

	private static Storage3D s_Instance;

	public enum StorageType
	{
		None = -1,
		Box,
		Bag,
		Count
	}
}
