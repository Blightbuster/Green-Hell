using System;
using System.Collections.Generic;
using CJTools;
using UnityEngine;

public class InventoryCellsGroup
{
	public bool IsFree()
	{
		foreach (InventoryCell inventoryCell in this.m_Cells)
		{
			if (inventoryCell.IsOccupied())
			{
				return false;
			}
		}
		return true;
	}

	public void Insert(Item item, GameObject grid)
	{
		foreach (InventoryCell inventoryCell in this.m_Cells)
		{
			inventoryCell.m_Items.Add(item);
		}
		if (item.m_Info.m_PrevInventoryCellsGroup == null)
		{
			item.m_Info.m_PrevInventoryCellsGroup = this;
		}
		item.m_PrevSlot = null;
		item.m_Info.m_InventoryCellsGroup = this;
		this.SetupTansform(item, grid);
	}

	public void SetupTansform(Item item, GameObject grid)
	{
		if (item && grid)
		{
			Collider component = grid.GetComponent<Collider>();
			Matrix4x4 identity = Matrix4x4.identity;
			Vector3 vector = -component.transform.forward;
			identity.SetColumn(1, vector);
			Vector3 v = Vector3.Cross(component.transform.up, vector);
			identity.SetColumn(0, v);
			identity.SetColumn(2, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(0)));
			identity.SetColumn(0, Vector3.Cross(identity.GetColumn(1), identity.GetColumn(2)));
			Quaternion rotation = CJTools.Math.QuaternionFromMatrix(identity);
			item.gameObject.transform.rotation = rotation;
			if (item.m_Info.m_InventoryRotated)
			{
				item.transform.RotateAround(item.m_BoxCollider.bounds.center, item.transform.up, 90f);
			}
			if (item.m_Pivot)
			{
				Vector3 b = item.transform.InverseTransformPoint(item.m_Pivot.transform.position);
				item.gameObject.transform.position = this.m_CenterWorld - b;
			}
			else
			{
				Vector3 b2 = item.m_BoxCollider.bounds.center - vector * item.m_BoxCollider.size.y * item.transform.localScale.y * 0.5f;
				item.transform.position = this.m_CenterWorld + (item.transform.position - b2);
			}
		}
	}

	public void Remove(Item item)
	{
		foreach (InventoryCell inventoryCell in this.m_Cells)
		{
			inventoryCell.m_Items.Remove(item);
		}
		item.m_Info.m_PrevInventoryCellsGroup = item.m_Info.m_InventoryCellsGroup;
		item.m_Info.m_InventoryCellsGroup = null;
	}

	public bool Contains(GameObject obj)
	{
		for (int i = 0; i < this.m_Cells.Count; i++)
		{
			if (this.m_Cells[i].m_Object == obj)
			{
				return true;
			}
		}
		return false;
	}

	public void Setup()
	{
		if (this.m_Cells.Count == 0)
		{
			return;
		}
		Vector3 position = this.m_Cells[0].m_Renderer.transform.position;
		Vector3 position2 = this.m_Cells[0].m_Renderer.transform.position;
		for (int i = 1; i < this.m_Cells.Count; i++)
		{
			Vector3 position3 = this.m_Cells[i].m_Renderer.transform.position;
			if (position3.x < position.x)
			{
				position.x = position3.x;
			}
			if (position3.x > position2.x)
			{
				position2.x = position3.x;
			}
			if (position3.y < position.y)
			{
				position.y = position3.y;
			}
			if (position3.y > position2.y)
			{
				position2.y = position3.y;
			}
			if (position3.z < position.z)
			{
				position.z = position3.z;
			}
			if (position3.z > position2.z)
			{
				position2.z = position3.z;
			}
		}
		this.m_CenterWorld = position + 0.5f * (position2 - position);
	}

	public List<InventoryCell> m_Cells = new List<InventoryCell>();

	public Vector3 m_CenterWorld;
}
