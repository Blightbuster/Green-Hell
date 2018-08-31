using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeBalanceSystem
{
	public QuadTreeBalanceSystem(float start_x, float start_y, float size_x, float size_y, int num_cells_x, int num_cells_y)
	{
		this.m_Cells = new QuadTreeCellBalanceSystem[num_cells_x, num_cells_y];
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
				this.m_Cells[i, j] = new QuadTreeCellBalanceSystem();
				this.m_Cells[i, j].m_X = i;
				this.m_Cells[i, j].m_Y = j;
				this.m_Cells[i, j].m_Pos.x = this.m_Start.x + (float)i * this.m_CellSize.x;
				this.m_Cells[i, j].m_Pos.y = this.m_Start.y + (float)j * this.m_CellSize.y;
				this.m_Cells[i, j].m_Size = this.m_CellSize;
			}
		}
	}

	public void InsertObject(BalanceSystemObject obj)
	{
		QuadTreeCellBalanceSystem cellAtPos = this.GetCellAtPos(obj.m_GameObject.transform.position);
		cellAtPos.m_Objects.Add(obj);
		this.m_ObjCellMap[obj] = cellAtPos;
	}

	public void RemoveObject(BalanceSystemObject go)
	{
		if (!this.m_ObjCellMap.ContainsKey(go))
		{
			DebugUtils.Assert("[QuadTree:RemoveObject] Tree does not contains object - " + go.m_GameObject.name, true, DebugUtils.AssertType.Info);
			return;
		}
		QuadTreeCellBalanceSystem quadTreeCellBalanceSystem = null;
		if (this.m_ObjCellMap.TryGetValue(go, out quadTreeCellBalanceSystem))
		{
			quadTreeCellBalanceSystem.RemoveObject(go);
			this.m_ObjCellMap.Remove(go);
		}
		else
		{
			DebugUtils.Assert(false, true);
		}
	}

	private QuadTreeCellBalanceSystem GetCellAtPos(Vector3 pos)
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

	public BalanceSystemObject GetObjectInPos(Vector3 pos)
	{
		QuadTreeCellBalanceSystem cellAtPos = this.GetCellAtPos(pos);
		foreach (BalanceSystemObject balanceSystemObject in cellAtPos.m_Objects)
		{
			if (balanceSystemObject.m_GameObject != null)
			{
				if (balanceSystemObject.m_GameObject.transform.position == pos)
				{
					return balanceSystemObject;
				}
			}
			else if (balanceSystemObject.m_Position == pos)
			{
				return balanceSystemObject;
			}
		}
		return null;
	}

	public List<BalanceSystemObject> GetObjectsInRadius(Vector3 pos, float radius)
	{
		this.m_ObjectsInRadius.Clear();
		Vector3 pos2 = pos;
		pos2.x -= radius;
		pos2.z -= radius;
		QuadTreeCellBalanceSystem cellAtPos = this.GetCellAtPos(pos2);
		Vector3 pos3 = pos;
		pos3.x += radius;
		pos3.z += radius;
		QuadTreeCellBalanceSystem cellAtPos2 = this.GetCellAtPos(pos3);
		for (int i = cellAtPos.m_X; i <= cellAtPos2.m_X; i++)
		{
			for (int j = cellAtPos.m_Y; j <= cellAtPos2.m_Y; j++)
			{
				QuadTreeCellBalanceSystem quadTreeCellBalanceSystem = this.m_Cells[i, j];
				for (int k = 0; k < quadTreeCellBalanceSystem.m_Objects.Count; k++)
				{
					BalanceSystemObject balanceSystemObject = quadTreeCellBalanceSystem.m_Objects[k];
					if (balanceSystemObject == null)
					{
						Debug.Log("Quad tree GetObjectsInRadius - obj is null : GameObject obj = cell.m_Objects[i];");
					}
					if (balanceSystemObject != null && (balanceSystemObject.m_GameObject.transform.position - pos).magnitude < radius)
					{
						this.m_ObjectsInRadius.Add(balanceSystemObject);
					}
				}
			}
		}
		return this.m_ObjectsInRadius;
	}

	private QuadTreeCellBalanceSystem FindObjectCell(GameObject go)
	{
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				QuadTreeCellBalanceSystem quadTreeCellBalanceSystem = this.m_Cells[i, j];
				for (int k = 0; k < quadTreeCellBalanceSystem.m_Objects.Count; k++)
				{
					BalanceSystemObject balanceSystemObject = quadTreeCellBalanceSystem.m_Objects[k];
					if (go == balanceSystemObject.m_GameObject)
					{
						return quadTreeCellBalanceSystem;
					}
				}
			}
		}
		return null;
	}

	public void DrawDebug()
	{
		bool flag = false;
		bool flag2 = true;
		bool flag3 = true;
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				QuadTreeCellBalanceSystem quadTreeCellBalanceSystem = this.m_Cells[i, j];
				if (flag || (flag3 && quadTreeCellBalanceSystem.m_Objects.Count > 0))
				{
					Vector3 zero = Vector3.zero;
					zero.x = quadTreeCellBalanceSystem.m_Pos.x;
					zero.z = quadTreeCellBalanceSystem.m_Pos.y;
					Vector3 end = zero;
					end.x += quadTreeCellBalanceSystem.m_Size.x;
					DebugRender.DrawLine(zero, end, Color.white, 0f);
					end = zero;
					end.z += quadTreeCellBalanceSystem.m_Size.y;
					DebugRender.DrawLine(zero, end, Color.white, 0f);
				}
				if (flag2)
				{
					for (int k = 0; k < quadTreeCellBalanceSystem.m_Objects.Count; k++)
					{
						BalanceSystemObject balanceSystemObject = quadTreeCellBalanceSystem.m_Objects[k];
						if (balanceSystemObject.m_GameObject != null)
						{
							DebugRender.DrawPoint(balanceSystemObject.m_GameObject.transform.position, (!balanceSystemObject.m_GameObject.activeSelf) ? Color.green : Color.red, 0.3f, 0f);
						}
					}
				}
			}
		}
	}

	private QuadTreeCellBalanceSystem[,] m_Cells;

	private Vector2 m_Start = default(Vector2);

	private Vector2 m_Size = default(Vector2);

	private Vector2 m_CellSize = default(Vector2);

	private int m_NumCellsX;

	private int m_NumCellsY;

	private List<BalanceSystemObject> m_ObjectsInRadius = new List<BalanceSystemObject>();

	private Dictionary<BalanceSystemObject, QuadTreeCellBalanceSystem> m_ObjCellMap = new Dictionary<BalanceSystemObject, QuadTreeCellBalanceSystem>();
}
