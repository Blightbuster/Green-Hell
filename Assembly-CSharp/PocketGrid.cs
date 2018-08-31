using System;
using System.Collections.Generic;
using CJTools;
using Enums;
using UnityEngine;

public class PocketGrid
{
	public void Initialize()
	{
		this.m_CellLayer = LayerMask.NameToLayer("3DInventory");
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
				inventoryCell.pocket = BackpackPocket.Main;
				inventoryCell.m_Object = UnityEngine.Object.Instantiate<GameObject>(InventoryBackpack.Get().m_Backpack.m_GridCellPrefab);
				GameObject @object = inventoryCell.m_Object;
				@object.name += num.ToString();
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

	public bool InsertItem(Item item, ItemSlot slot, InventoryCellsGroup group, bool can_stack, bool can_auto_select_group)
	{
		if (slot)
		{
			if (slot.CanInsertItem(item))
			{
				slot.InsertItem(item);
				return true;
			}
			return false;
		}
		else
		{
			if (group == null)
			{
				if (can_stack)
				{
					for (int i = 0; i < InventoryBackpack.Get().m_Items.Count; i++)
					{
						Item item2 = InventoryBackpack.Get().m_Items[i];
						if (item2.m_InventorySlot && !item2.m_CurrentSlot && item2.m_InventorySlot.CanInsertItem(item))
						{
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
	}

	public void RemoveItem(Item item)
	{
		if (item.m_Info.m_InventoryCellsGroup != null)
		{
			item.m_Info.m_InventoryCellsGroup.Remove(item);
		}
	}

	private void CalcRequiredCells(Item item, ref int x, ref int y)
	{
		Vector3 inventoryLocalScale = item.m_InventoryLocalScale;
		if (item.m_Info.m_InventoryRotated)
		{
			x = Mathf.CeilToInt(item.m_DefaultSize.z * inventoryLocalScale.x / this.m_CellSize.x);
			y = Mathf.CeilToInt(item.m_DefaultSize.x * inventoryLocalScale.z / this.m_CellSize.y);
		}
		else
		{
			x = Mathf.CeilToInt(item.m_DefaultSize.x * inventoryLocalScale.x / this.m_CellSize.x);
			y = Mathf.CeilToInt(item.m_DefaultSize.z * inventoryLocalScale.z / this.m_CellSize.y);
		}
	}

	public void Setup()
	{
		this.m_MatchingGroups.Clear();
		Item carriedItem = Inventory3DManager.Get().m_CarriedItem;
		if (!carriedItem)
		{
			return;
		}
		if (carriedItem.m_Info.m_InventoryCellsGroup != null)
		{
			carriedItem.m_Info.m_InventoryCellsGroup.Remove(carriedItem);
		}
		int num = 0;
		int num2 = 0;
		this.CalcRequiredCells(carriedItem, ref num, ref num2);
		InventoryCell[,] cells = this.m_Cells;
		int length = cells.GetLength(0);
		int length2 = cells.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				InventoryCell inventoryCell = cells[i, j];
				if (inventoryCell.m_IndexX + num <= (int)this.m_GridSize.x)
				{
					if (inventoryCell.m_IndexY + num2 <= (int)this.m_GridSize.y)
					{
						InventoryCellsGroup inventoryCellsGroup = new InventoryCellsGroup();
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
	}

	private InventoryCellsGroup FindFreeGroup(Item item)
	{
		if (!item.m_Info.m_CanBeAddedToInventory)
		{
			return null;
		}
		int num = 0;
		int num2 = 0;
		item.m_Info.m_InventoryRotated = false;
		item.transform.rotation = Quaternion.identity;
		this.CalcRequiredCells(item, ref num, ref num2);
		InventoryCellsGroup inventoryCellsGroup = this.FindFreeGroup(item, num, num2);
		if (inventoryCellsGroup == null)
		{
			Inventory3DManager.Get().RotateItem(item);
			inventoryCellsGroup = this.FindFreeGroup(item, num2, num);
		}
		return inventoryCellsGroup;
	}

	private InventoryCellsGroup FindFreeGroup(Item item, int req_x, int req_y)
	{
		InventoryCell[,] cells = this.m_Cells;
		int length = cells.GetLength(0);
		int length2 = cells.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				InventoryCell inventoryCell = cells[i, j];
				if (inventoryCell.m_IndexX + req_x <= (int)this.m_GridSize.x)
				{
					if (inventoryCell.m_IndexY + req_y <= (int)this.m_GridSize.y)
					{
						InventoryCellsGroup inventoryCellsGroup = new InventoryCellsGroup();
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
		Ray ray = Inventory3DManager.Get().m_Camera.ViewportPointToRay(zero);
		RaycastHit[] array = Physics.RaycastAll(ray, 5f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject.transform.parent == this.m_Grid.transform)
			{
				gameObject = array[i].collider.gameObject;
				vector = array[i].point;
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
		int length = cells.GetLength(0);
		int length2 = cells.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				InventoryCell inventoryCell = cells[i, j];
				inventoryCell.m_Renderer.enabled = false;
			}
		}
		if (group == null)
		{
			return;
		}
		bool flag = group.IsFree();
		if (flag)
		{
			for (int k = 0; k < group.m_Cells.Count; k++)
			{
				group.m_Cells[k].m_Renderer.enabled = true;
				group.m_Cells[k].m_Renderer.material.color = InventoryBackpack.Get().m_FreeColor;
			}
		}
		else
		{
			for (int l = 0; l < group.m_Cells.Count; l++)
			{
				group.m_Cells[l].m_Renderer.enabled = true;
				group.m_Cells[l].m_Renderer.material.color = InventoryBackpack.Get().m_OccupiedColor;
				if (group.m_Cells[l].m_Items.Count > 0)
				{
					for (int m = 0; m < group.m_Cells[l].m_Items.Count; m++)
					{
						InventoryCellsGroup inventoryCellsGroup = group.m_Cells[l].m_Items[m].m_Info.m_InventoryCellsGroup;
						for (int n = 0; n < inventoryCellsGroup.m_Cells.Count; n++)
						{
							inventoryCellsGroup.m_Cells[n].m_Renderer.enabled = true;
							inventoryCellsGroup.m_Cells[n].m_Renderer.material.color = InventoryBackpack.Get().m_OccupiedColor;
						}
					}
				}
			}
		}
	}

	public InventoryCell GetCellByName(string name)
	{
		InventoryCell[,] cells = this.m_Cells;
		int length = cells.GetLength(0);
		int length2 = cells.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
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

	public GameObject m_Grid;

	public Vector2 m_GridSize = Vector2.one;

	private Vector2 m_CellSize = Vector2.zero;

	public InventoryCell[,] m_Cells;

	private List<InventoryCellsGroup> m_MatchingGroups = new List<InventoryCellsGroup>();

	private int m_CellLayer = -1;
}
