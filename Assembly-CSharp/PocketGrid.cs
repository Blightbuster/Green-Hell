using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class PocketGrid
{
	public void Initialize(GameObject cell_prefab, BackpackPocket pocket)
	{
		this.m_CellLayer = LayerMask.NameToLayer("3DInventory");
		this.m_Pocked = pocket;
		Vector3 one = Vector3.one;
		one.x = 1f / (float)((int)this.m_GridSize.x);
		one.y = 1f / (float)((int)this.m_GridSize.y);
		this.m_CellSize.x = one.x * this.m_Grid.transform.localScale.x;
		this.m_CellSize.y = one.y * this.m_Grid.transform.localScale.y;
		int num = 0;
		this.m_Cells = new InventoryCell[(int)this.m_GridSize.x, (int)this.m_GridSize.y];
		for (int i = 0; i < (int)this.m_GridSize.y; i++)
		{
			for (int j = 0; j < (int)this.m_GridSize.x; j++)
			{
				InventoryCell inventoryCell = new InventoryCell();
				inventoryCell.m_IndexX = j;
				inventoryCell.m_IndexY = i;
				inventoryCell.pocket = this.m_Pocked;
				inventoryCell.m_Object = UnityEngine.Object.Instantiate<GameObject>(cell_prefab);
				inventoryCell.m_Object.name = this.m_Pocked.ToString() + num.ToString();
				inventoryCell.m_Object.layer = this.m_CellLayer;
				inventoryCell.m_Renderer = inventoryCell.m_Object.GetComponent<Renderer>();
				inventoryCell.m_Renderer.enabled = false;
				inventoryCell.m_Object.transform.parent = this.m_Grid.transform;
				inventoryCell.m_Object.transform.localRotation = Quaternion.identity;
				inventoryCell.m_Object.transform.localScale = one;
				Vector3 zero = Vector3.zero;
				zero.x = 0.5f - one.x * 0.5f - one.x * (float)j;
				zero.y = 0.5f - one.y * 0.5f - one.y * (float)i;
				inventoryCell.m_Object.transform.localPosition = zero;
				this.m_Cells[j, i] = inventoryCell;
				num++;
			}
		}
	}

	public void Reset()
	{
		for (int i = 0; i < (int)this.m_GridSize.y; i++)
		{
			for (int j = 0; j < (int)this.m_GridSize.x; j++)
			{
				this.m_Cells[j, i].m_Items.Clear();
			}
		}
	}

	public bool InsertItem(Item item, ItemSlot slot, InventoryCellsGroup group, bool can_stack, bool can_auto_select_group, Storage storage = null)
	{
		if (item.m_Info.m_FakeItem)
		{
			return true;
		}
		if (slot)
		{
			if (slot.CanInsertItem(item))
			{
				if (slot.IsStack() && slot.m_ItemParent && slot.m_ItemParent.m_Info.m_InventoryRotated != item.m_Info.m_InventoryRotated)
				{
					Inventory3DManager.Get().RotateItem(item, true);
				}
				slot.InsertItem(item);
				return true;
			}
			if (!slot.IsOccupied())
			{
				return false;
			}
		}
		if (group == null)
		{
			if (can_stack)
			{
				List<Item> list = storage ? storage.m_Items : InventoryBackpack.Get().m_Items;
				for (int i = 0; i < list.Count; i++)
				{
					Item item2 = list[i];
					if (item2.m_InventorySlot && !item2.m_CurrentSlot && item2.m_InventorySlot.CanInsertItem(item))
					{
						if (item2.m_Info.m_InventoryRotated != item.m_Info.m_InventoryRotated)
						{
							Inventory3DManager.Get().RotateItem(item, true);
						}
						item2.m_InventorySlot.InsertItem(item);
						return true;
					}
				}
			}
			if (can_auto_select_group)
			{
				group = this.FindFreeGroup(item);
				if (group != null && group.IsFree())
				{
					group.Insert(item, this.m_Grid);
					return true;
				}
			}
			return false;
		}
		if (group.IsFree())
		{
			group.Insert(item, this.m_Grid);
			return true;
		}
		return false;
	}

	public void RemoveItem(Item item)
	{
		if (item.m_Info.m_InventoryCellsGroup != null)
		{
			item.m_Info.m_InventoryCellsGroup.Remove(item);
		}
	}

	public void CalcRequiredCells(Item item, ref int x, ref int y)
	{
		Vector3 inventoryLocalScale = item.m_InventoryLocalScale;
		if (item.m_Info.m_InventoryRotated)
		{
			x = Mathf.CeilToInt(item.m_DefaultSize.z * inventoryLocalScale.z / this.m_CellSize.x);
			y = Mathf.CeilToInt(item.m_DefaultSize.x * inventoryLocalScale.x / this.m_CellSize.y);
			return;
		}
		x = Mathf.CeilToInt(item.m_DefaultSize.x * inventoryLocalScale.x / this.m_CellSize.x);
		y = Mathf.CeilToInt(item.m_DefaultSize.z * inventoryLocalScale.z / this.m_CellSize.y);
	}

	public void Setup()
	{
		this.m_MatchingGroups.Clear();
		Item carriedItem = Inventory3DManager.Get().m_CarriedItem;
		if (!carriedItem)
		{
			return;
		}
		if (this.m_Pocked == BackpackPocket.Storage && !Storage3D.Get().CanInsertItem(Inventory3DManager.Get().m_CarriedItem))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		this.CalcRequiredCells(carriedItem, ref num, ref num2);
		InventoryCell[,] cells = this.m_Cells;
		int upperBound = cells.GetUpperBound(0);
		int upperBound2 = cells.GetUpperBound(1);
		for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
			{
				InventoryCell inventoryCell = cells[i, j];
				if (inventoryCell.m_IndexX + num <= (int)this.m_GridSize.x && inventoryCell.m_IndexY + num2 <= (int)this.m_GridSize.y)
				{
					InventoryCellsGroup inventoryCellsGroup = new InventoryCellsGroup(this.m_Pocked);
					for (int k = inventoryCell.m_IndexX; k < inventoryCell.m_IndexX + num; k++)
					{
						for (int l = inventoryCell.m_IndexY; l < inventoryCell.m_IndexY + num2; l++)
						{
							inventoryCellsGroup.m_Cells.Add(this.m_Cells[k, l]);
						}
					}
					if (inventoryCellsGroup.m_Cells.Count != 0)
					{
						inventoryCellsGroup.Setup();
						this.m_MatchingGroups.Add(inventoryCellsGroup);
					}
				}
			}
		}
	}

	private InventoryCellsGroup FindFreeGroup(Item item)
	{
		if (!item.m_Info.m_CanBeAddedToInventory)
		{
			return null;
		}
		int req_x = 0;
		int req_y = 0;
		this.CalcRequiredCells(item, ref req_x, ref req_y);
		InventoryCellsGroup inventoryCellsGroup = this.FindFreeGroup(item, req_x, req_y);
		if (inventoryCellsGroup == null)
		{
			Inventory3DManager.Get().RotateItem(item, true);
			this.CalcRequiredCells(item, ref req_x, ref req_y);
			inventoryCellsGroup = this.FindFreeGroup(item, req_x, req_y);
		}
		return inventoryCellsGroup;
	}

	private InventoryCellsGroup FindFreeGroup(Item item, int req_x, int req_y)
	{
		InventoryCell[,] cells = this.m_Cells;
		int upperBound = cells.GetUpperBound(0);
		int upperBound2 = cells.GetUpperBound(1);
		for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
			{
				InventoryCell inventoryCell = cells[i, j];
				if (inventoryCell.m_IndexX + req_x <= (int)this.m_GridSize.x && inventoryCell.m_IndexY + req_y <= (int)this.m_GridSize.y)
				{
					InventoryCellsGroup inventoryCellsGroup = new InventoryCellsGroup(this.m_Pocked);
					for (int k = inventoryCell.m_IndexX; k < inventoryCell.m_IndexX + req_x; k++)
					{
						for (int l = inventoryCell.m_IndexY; l < inventoryCell.m_IndexY + req_y; l++)
						{
							inventoryCellsGroup.m_Cells.Add(this.m_Cells[k, l]);
						}
					}
					if (inventoryCellsGroup.m_Cells.Count != 0 && inventoryCellsGroup.IsFree())
					{
						inventoryCellsGroup.Setup();
						return inventoryCellsGroup;
					}
				}
			}
		}
		return null;
	}

	public InventoryCellsGroup FindBestGroup()
	{
		if (this.m_MatchingGroups.Count == 0)
		{
			return null;
		}
		Vector3 zero = Vector3.zero;
		RectTransform rectTransform = Inventory3DManager.Get().m_InventoryImage.rectTransform;
		Vector2 zero2 = Vector2.zero;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out zero2))
		{
			return null;
		}
		zero.x = CJTools.Math.GetProportionalClamp(0f, 1f, zero2.x, -rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.x * 0.5f);
		zero.y = CJTools.Math.GetProportionalClamp(0f, 1f, zero2.y, -rectTransform.sizeDelta.y * 0.5f, rectTransform.sizeDelta.y * 0.5f);
		GameObject gameObject = null;
		Vector3 vector = Vector3.zero;
		Inventory3DManager.Get().m_Camera.ViewportPointToRay(zero);
		RaycastHit[] backpackHits = Inventory3DManager.Get().m_BackpackHits;
		for (int i = 0; i < Inventory3DManager.Get().m_BackpackHitsCnt; i++)
		{
			if (backpackHits[i].collider.gameObject.transform.parent == this.m_Grid.transform)
			{
				gameObject = backpackHits[i].collider.gameObject;
				vector = backpackHits[i].point;
				break;
			}
		}
		if (!gameObject)
		{
			return null;
		}
		List<InventoryCellsGroup> list = new List<InventoryCellsGroup>();
		for (int j = 0; j < this.m_MatchingGroups.Count; j++)
		{
			if (this.m_MatchingGroups[j].Contains(gameObject))
			{
				list.Add(this.m_MatchingGroups[j]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		InventoryCellsGroup result = null;
		float num = float.MaxValue;
		foreach (InventoryCellsGroup inventoryCellsGroup in list)
		{
			float num2 = vector.Distance(inventoryCellsGroup.m_CenterWorld);
			if (num2 < num)
			{
				result = inventoryCellsGroup;
				num = num2;
			}
		}
		return result;
	}

	public void OnSetSelectedGroup(InventoryCellsGroup group)
	{
		InventoryCell[,] cells = this.m_Cells;
		int upperBound = cells.GetUpperBound(0);
		int upperBound2 = cells.GetUpperBound(1);
		for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
			{
				cells[i, j].m_Renderer.enabled = false;
			}
		}
		if (group == null)
		{
			return;
		}
		for (int k = 0; k < (int)this.m_GridSize.y; k++)
		{
			for (int l = 0; l < (int)this.m_GridSize.x; l++)
			{
				this.m_Cells[l, k].m_Renderer.material.color = Color.white;
			}
		}
		if (group.IsFree())
		{
			for (int m = 0; m < group.m_Cells.Count; m++)
			{
				group.m_Cells[m].m_Renderer.enabled = true;
				group.m_Cells[m].m_Renderer.material.color = InventoryBackpack.Get().m_FreeColor;
			}
			return;
		}
		for (int n = 0; n < group.m_Cells.Count; n++)
		{
			group.m_Cells[n].m_Renderer.enabled = true;
			group.m_Cells[n].m_Renderer.material.color = InventoryBackpack.Get().m_OccupiedColor;
			if (group.m_Cells[n].m_Items.Count > 0)
			{
				for (int num = 0; num < group.m_Cells[n].m_Items.Count; num++)
				{
					InventoryCellsGroup inventoryCellsGroup = group.m_Cells[n].m_Items[num].m_Info.m_InventoryCellsGroup;
					if (this.m_Pocked != BackpackPocket.Storage || !group.m_Cells[n].m_Items[num].m_Storage || !(group.m_Cells[n].m_Items[num].m_Storage != Storage3D.Get().m_Storage))
					{
						for (int num2 = 0; num2 < inventoryCellsGroup.m_Cells.Count; num2++)
						{
							inventoryCellsGroup.m_Cells[num2].m_Renderer.enabled = true;
							inventoryCellsGroup.m_Cells[num2].m_Renderer.material.color = InventoryBackpack.Get().m_OccupiedColor;
						}
					}
				}
			}
		}
	}

	public InventoryCell GetCellByName(string name)
	{
		InventoryCell[,] cells = this.m_Cells;
		int upperBound = cells.GetUpperBound(0);
		int upperBound2 = cells.GetUpperBound(1);
		for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
			{
				InventoryCell inventoryCell = cells[i, j];
				if (inventoryCell.m_Object.name == name)
				{
					return inventoryCell;
				}
			}
		}
		return null;
	}

	public void OnCloseBackpack()
	{
		InventoryCell[,] cells = this.m_Cells;
		int upperBound = cells.GetUpperBound(0);
		int upperBound2 = cells.GetUpperBound(1);
		for (int i = cells.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = cells.GetLowerBound(1); j <= upperBound2; j++)
			{
				cells[i, j].m_Renderer.enabled = false;
			}
		}
	}

	public GameObject m_Grid;

	public Vector2 m_GridSize = Vector2.one;

	private Vector2 m_CellSize = Vector2.zero;

	public InventoryCell[,] m_Cells;

	private List<InventoryCellsGroup> m_MatchingGroups = new List<InventoryCellsGroup>();

	private BackpackPocket m_Pocked = BackpackPocket.None;

	private int m_CellLayer = -1;
}
