using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
	public QuadTree(float start_x, float start_y, float size_x, float size_y, int num_cells_x, int num_cells_y)
	{
		this.m_Cells = new QuadTreeCell[num_cells_x, num_cells_y];
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
				this.m_Cells[i, j] = new QuadTreeCell();
				this.m_Cells[i, j].m_X = i;
				this.m_Cells[i, j].m_Y = j;
				this.m_Cells[i, j].m_Pos.x = this.m_Start.x + (float)i * this.m_CellSize.x;
				this.m_Cells[i, j].m_Pos.y = this.m_Start.y + (float)j * this.m_CellSize.y;
				this.m_Cells[i, j].m_Size = this.m_CellSize;
			}
		}
	}

	public void InsertObject(GameObject obj, bool use_bounds = false)
	{
		if (use_bounds)
		{
			Vector3 pos = obj.transform.position - obj.transform.localScale * 0.5f;
			Vector3 pos2 = obj.transform.position + obj.transform.localScale * 0.5f;
			QuadTreeCell cellAtPos = this.GetCellAtPos(pos);
			QuadTreeCell cellAtPos2 = this.GetCellAtPos(pos2);
			for (int i = cellAtPos.m_X; i <= cellAtPos2.m_X; i++)
			{
				for (int j = cellAtPos.m_Y; j <= cellAtPos2.m_Y; j++)
				{
					this.m_Cells[i, j].m_Objects.Add(obj);
					this.m_ObjCellMap[obj] = this.m_Cells[i, j];
				}
			}
			return;
		}
		QuadTreeCell cellAtPos3 = this.GetCellAtPos(obj.transform.position);
		cellAtPos3.m_Objects.Add(obj);
		this.m_ObjCellMap[obj] = cellAtPos3;
	}

	public void RemoveObject(GameObject go)
	{
		if (!this.m_ObjCellMap.ContainsKey(go))
		{
			DebugUtils.Assert("[QuadTree:RemoveObject] Tree does not contains object - " + go.name, true, DebugUtils.AssertType.Info);
			return;
		}
		QuadTreeCell quadTreeCell = null;
		if (this.m_ObjCellMap.TryGetValue(go, out quadTreeCell))
		{
			quadTreeCell.RemoveObject(go);
			this.m_ObjCellMap.Remove(go);
			return;
		}
		DebugUtils.Assert(false, true);
	}

	private QuadTreeCell GetCellAtPos(Vector3 pos)
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

	public GameObject GetObjectsInPos(Vector3 pos)
	{
		foreach (GameObject gameObject in this.GetCellAtPos(pos).m_Objects)
		{
			if (gameObject.transform.position == pos)
			{
				return gameObject;
			}
		}
		return null;
	}

	public List<GameObject> GetObjectsInRadius(Vector3 pos, float radius, bool use_bounds = false)
	{
		this.m_ObjectsInRadius.Clear();
		if (use_bounds)
		{
			QuadTreeCell cellAtPos = this.GetCellAtPos(pos);
			for (int i = 0; i < cellAtPos.m_Objects.Count; i++)
			{
				Bounds bounds = default(Bounds);
				GameObject gameObject = cellAtPos.m_Objects[i];
				bounds.Encapsulate(new Vector3(1f, 1f, 1f));
				bounds.center = new Vector3(0f, 0f, 0f);
				Vector3 point = gameObject.transform.InverseTransformPoint(pos);
				if (bounds.Contains(point))
				{
					this.m_ObjectsInRadius.Add(gameObject);
				}
			}
		}
		else
		{
			Vector3 pos2 = pos;
			pos2.x -= radius;
			pos2.z -= radius;
			QuadTreeCell cellAtPos2 = this.GetCellAtPos(pos2);
			Vector3 pos3 = pos;
			pos3.x += radius;
			pos3.z += radius;
			QuadTreeCell cellAtPos3 = this.GetCellAtPos(pos3);
			for (int j = cellAtPos2.m_X; j <= cellAtPos3.m_X; j++)
			{
				for (int k = cellAtPos2.m_Y; k <= cellAtPos3.m_Y; k++)
				{
					QuadTreeCell quadTreeCell = this.m_Cells[j, k];
					for (int l = 0; l < quadTreeCell.m_Objects.Count; l++)
					{
						GameObject gameObject2 = quadTreeCell.m_Objects[l];
						if (gameObject2 == null)
						{
							Debug.Log("Quad tree GetObjectsInRadius - obj is null : GameObject obj = cell.m_Objects[i];");
						}
						if (gameObject2 && (gameObject2.transform.position - pos).magnitude < radius)
						{
							this.m_ObjectsInRadius.Add(gameObject2);
						}
					}
				}
			}
		}
		return this.m_ObjectsInRadius;
	}

	public void OnObjectMoved(GameObject go)
	{
		QuadTreeCell quadTreeCell = null;
		if (!this.m_ObjCellMap.TryGetValue(go, out quadTreeCell))
		{
			return;
		}
		QuadTreeCell cellAtPos = this.GetCellAtPos(go.transform.position);
		if (quadTreeCell != cellAtPos)
		{
			quadTreeCell.RemoveObject(go);
			this.InsertObject(go, false);
		}
	}

	private QuadTreeCell FindObjectCell(GameObject go)
	{
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				QuadTreeCell quadTreeCell = this.m_Cells[i, j];
				for (int k = 0; k < quadTreeCell.m_Objects.Count; k++)
				{
					GameObject y = quadTreeCell.m_Objects[k];
					if (go == y)
					{
						return quadTreeCell;
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
				QuadTreeCell quadTreeCell = this.m_Cells[i, j];
				if (flag || (flag3 && quadTreeCell.m_Objects.Count > 0))
				{
					Vector3 zero = Vector3.zero;
					zero.x = quadTreeCell.m_Pos.x;
					zero.z = quadTreeCell.m_Pos.y;
					Vector3 end = zero;
					end.x += quadTreeCell.m_Size.x;
					DebugRender.DrawLine(zero, end, Color.white, 0f);
					end = zero;
					end.z += quadTreeCell.m_Size.y;
					DebugRender.DrawLine(zero, end, Color.white, 0f);
				}
				if (flag2)
				{
					for (int k = 0; k < quadTreeCell.m_Objects.Count; k++)
					{
						GameObject gameObject = quadTreeCell.m_Objects[k];
						if (gameObject)
						{
							DebugRender.DrawPoint(gameObject.transform.position, gameObject.activeSelf ? Color.red : Color.green, 0.3f, 0f);
						}
					}
				}
			}
		}
	}

	private QuadTreeCell[,] m_Cells;

	private Vector2 m_Start;

	private Vector2 m_Size;

	private Vector2 m_CellSize;

	private int m_NumCellsX;

	private int m_NumCellsY;

	private List<GameObject> m_ObjectsInRadius = new List<GameObject>();

	private Dictionary<GameObject, QuadTreeCell> m_ObjCellMap = new Dictionary<GameObject, QuadTreeCell>();
}
