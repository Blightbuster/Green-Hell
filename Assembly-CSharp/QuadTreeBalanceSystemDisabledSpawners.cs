using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeBalanceSystemDisabledSpawners
{
	public QuadTreeBalanceSystemDisabledSpawners(float start_x, float start_y, float size_x, float size_y, int num_cells_x, int num_cells_y)
	{
		this.m_Cells = new QuadTreeBalanceSystemDisabledSpawnersCell[num_cells_x, num_cells_y];
		this.m_Start.x = start_x;
		this.m_Start.y = start_y;
		this.m_Size.x = size_x;
		this.m_Size.y = size_y;
		this.m_CellSize.x = size_x / (float)num_cells_x;
		this.m_CellSize.y = size_y / (float)num_cells_y;
		this.m_NumCellsX = num_cells_x;
		this.m_NumCellsY = num_cells_y;
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				this.m_Cells[i, j] = new QuadTreeBalanceSystemDisabledSpawnersCell();
				this.m_Cells[i, j].m_X = i;
				this.m_Cells[i, j].m_Y = j;
				this.m_Cells[i, j].m_Pos.x = this.m_Start.x + (float)i * this.m_CellSize.x;
				this.m_Cells[i, j].m_Pos.y = this.m_Start.y + (float)j * this.m_CellSize.y;
				this.m_Cells[i, j].m_Size = this.m_CellSize;
			}
		}
	}

	public void InsertObject(BalanceSpawner obj)
	{
		QuadTreeBalanceSystemDisabledSpawnersCell cellAtPos = this.GetCellAtPos(obj.transform.position);
		if (!cellAtPos.m_Objects.Contains(obj))
		{
			cellAtPos.m_Objects.Add(obj);
		}
		this.m_ObjCellMap[obj] = cellAtPos;
	}

	public void InsertObject(BalanceSpawner obj, Vector3 position)
	{
		QuadTreeBalanceSystemDisabledSpawnersCell cellAtPos = this.GetCellAtPos(position);
		if (!cellAtPos.m_Objects.Contains(obj))
		{
			cellAtPos.m_Objects.Add(obj);
		}
		this.m_ObjCellMap[obj] = cellAtPos;
	}

	public void RemoveObject(BalanceSpawner bs)
	{
		if (!this.m_ObjCellMap.ContainsKey(bs))
		{
			DebugUtils.Assert("[QuadTree:RemoveObject] Tree does not contains object - " + bs.name, true, DebugUtils.AssertType.Info);
			return;
		}
		QuadTreeBalanceSystemDisabledSpawnersCell quadTreeBalanceSystemDisabledSpawnersCell = null;
		if (this.m_ObjCellMap.TryGetValue(bs, out quadTreeBalanceSystemDisabledSpawnersCell))
		{
			quadTreeBalanceSystemDisabledSpawnersCell.RemoveObject(bs);
			this.m_ObjCellMap.Remove(bs);
			return;
		}
		DebugUtils.Assert(false, true);
	}

	private QuadTreeBalanceSystemDisabledSpawnersCell GetCellAtPos(Vector3 pos)
	{
		int num = Mathf.FloorToInt((pos.x - this.m_Start.x) / this.m_Size.x * (float)this.m_NumCellsX);
		int num2 = Mathf.FloorToInt((pos.z - this.m_Start.y) / this.m_Size.y * (float)this.m_NumCellsY);
		if (num < 0)
		{
			num = 0;
		}
		else if (num > this.m_NumCellsX - 1)
		{
			num = this.m_NumCellsX - 1;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		else if (num2 > this.m_NumCellsY - 1)
		{
			num2 = this.m_NumCellsY - 1;
		}
		return this.m_Cells[num, num2];
	}

	public BalanceSpawner GetObjectInPos(Vector3 pos)
	{
		foreach (BalanceSpawner balanceSpawner in this.GetCellAtPos(pos).m_Objects)
		{
			if (balanceSpawner != null && balanceSpawner.transform.position == pos)
			{
				return balanceSpawner;
			}
		}
		return null;
	}

	public List<BalanceSpawner> GetObjectsInRadius(Vector3 pos, float radius)
	{
		this.m_ObjectsInRadius.Clear();
		Vector3 pos2 = pos;
		pos2.x -= radius;
		pos2.z -= radius;
		QuadTreeBalanceSystemDisabledSpawnersCell cellAtPos = this.GetCellAtPos(pos2);
		Vector3 pos3 = pos;
		pos3.x += radius;
		pos3.z += radius;
		QuadTreeBalanceSystemDisabledSpawnersCell cellAtPos2 = this.GetCellAtPos(pos3);
		for (int i = cellAtPos.m_X; i <= cellAtPos2.m_X; i++)
		{
			for (int j = cellAtPos.m_Y; j <= cellAtPos2.m_Y; j++)
			{
				QuadTreeBalanceSystemDisabledSpawnersCell quadTreeBalanceSystemDisabledSpawnersCell = this.m_Cells[i, j];
				for (int k = 0; k < quadTreeBalanceSystemDisabledSpawnersCell.m_Objects.Count; k++)
				{
					BalanceSpawner balanceSpawner = quadTreeBalanceSystemDisabledSpawnersCell.m_Objects[k];
					if (balanceSpawner == null)
					{
						Debug.Log("Quad tree GetObjectsInRadius - obj is null : GameObject obj = cell.m_Objects[i];");
					}
					if (balanceSpawner != null && (balanceSpawner.transform.position - pos).magnitude < radius)
					{
						this.m_ObjectsInRadius.Add(balanceSpawner);
					}
				}
			}
		}
		return this.m_ObjectsInRadius;
	}

	public List<BalanceSpawner> GetObjectsInBounds(Bounds bounds)
	{
		this.m_ObjectsInRadius.Clear();
		Vector3 center = bounds.center;
		center.x -= bounds.extents.x;
		center.z -= bounds.extents.z;
		QuadTreeBalanceSystemDisabledSpawnersCell cellAtPos = this.GetCellAtPos(center);
		Vector3 center2 = bounds.center;
		center2.x += bounds.extents.x;
		center2.z += bounds.extents.z;
		QuadTreeBalanceSystemDisabledSpawnersCell cellAtPos2 = this.GetCellAtPos(center2);
		for (int i = cellAtPos.m_X; i <= cellAtPos2.m_X; i++)
		{
			for (int j = cellAtPos.m_Y; j <= cellAtPos2.m_Y; j++)
			{
				QuadTreeBalanceSystemDisabledSpawnersCell quadTreeBalanceSystemDisabledSpawnersCell = this.m_Cells[i, j];
				for (int k = 0; k < quadTreeBalanceSystemDisabledSpawnersCell.m_Objects.Count; k++)
				{
					BalanceSpawner balanceSpawner = quadTreeBalanceSystemDisabledSpawnersCell.m_Objects[k];
					if (balanceSpawner == null)
					{
						Debug.Log("Quad tree GetObjectsInRadius - obj is null : GameObject obj = cell.m_Objects[i];");
					}
					if (balanceSpawner != null && bounds.Contains(balanceSpawner.transform.position))
					{
						this.m_ObjectsInRadius.Add(balanceSpawner);
					}
				}
			}
		}
		return this.m_ObjectsInRadius;
	}

	private QuadTreeBalanceSystemDisabledSpawnersCell[,] m_Cells;

	private Vector2 m_Start;

	private Vector2 m_Size;

	private Vector2 m_CellSize;

	private int m_NumCellsX;

	private int m_NumCellsY;

	private List<BalanceSpawner> m_ObjectsInRadius = new List<BalanceSpawner>();

	private Dictionary<BalanceSpawner, QuadTreeBalanceSystemDisabledSpawnersCell> m_ObjCellMap = new Dictionary<BalanceSpawner, QuadTreeBalanceSystemDisabledSpawnersCell>();
}
