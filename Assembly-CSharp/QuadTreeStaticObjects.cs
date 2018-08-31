using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeStaticObjects
{
	public QuadTreeStaticObjects(float start_x, float start_y, float size_x, float size_y, int num_cells_x, int num_cells_y)
	{
		this.m_Cells = new QuadTreeStaticObjectsCell[num_cells_x, num_cells_y];
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
				this.m_Cells[i, j] = new QuadTreeStaticObjectsCell();
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
			QuadTreeStaticObjectsCell cellAtPos = this.GetCellAtPos(pos);
			QuadTreeStaticObjectsCell cellAtPos2 = this.GetCellAtPos(pos2);
			for (int i = cellAtPos.m_X; i <= cellAtPos2.m_X; i++)
			{
				for (int j = cellAtPos.m_Y; j <= cellAtPos2.m_Y; j++)
				{
					StaticObjectClass staticObjectClass = new StaticObjectClass();
					staticObjectClass.m_GameObject = obj;
					this.m_Cells[i, j].m_Objects.Add(staticObjectClass);
					this.m_ObjCellMap[obj] = this.m_Cells[i, j];
				}
			}
		}
		else
		{
			QuadTreeStaticObjectsCell cellAtPos3 = this.GetCellAtPos(obj.transform.position);
			StaticObjectClass staticObjectClass2 = new StaticObjectClass();
			staticObjectClass2.m_GameObject = obj;
			cellAtPos3.m_Objects.Add(staticObjectClass2);
			this.m_ObjCellMap[obj] = cellAtPos3;
		}
	}

	public void RemoveObject(GameObject go)
	{
		if (!this.m_ObjCellMap.ContainsKey(go))
		{
			return;
		}
		QuadTreeStaticObjectsCell quadTreeStaticObjectsCell = null;
		if (this.m_ObjCellMap.TryGetValue(go, out quadTreeStaticObjectsCell))
		{
			quadTreeStaticObjectsCell.RemoveObject(go);
			this.m_ObjCellMap.Remove(go);
		}
		else
		{
			DebugUtils.Assert(false, true);
		}
	}

	private QuadTreeStaticObjectsCell GetCellAtPos(Vector3 pos)
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

	public StaticObjectClass GetObjectsInPos(Vector3 pos)
	{
		QuadTreeStaticObjectsCell cellAtPos = this.GetCellAtPos(pos);
		foreach (StaticObjectClass staticObjectClass in cellAtPos.m_Objects)
		{
			if (staticObjectClass.m_GameObject.transform.position == pos)
			{
				return staticObjectClass;
			}
		}
		return null;
	}

	public List<StaticObjectClass> GetObjectsInRadius(Vector3 pos, float radius, bool use_bounds = false)
	{
		this.m_ObjectsInRadius.Clear();
		if (use_bounds)
		{
			QuadTreeStaticObjectsCell cellAtPos = this.GetCellAtPos(pos);
			for (int i = 0; i < cellAtPos.m_Objects.Count; i++)
			{
				Bounds bounds = default(Bounds);
				GameObject gameObject = cellAtPos.m_Objects[i].m_GameObject;
				bounds.Encapsulate(new Vector3(1f, 1f, 1f));
				bounds.center = new Vector3(0f, 0f, 0f);
				Vector3 point = gameObject.transform.InverseTransformPoint(pos);
				if (bounds.Contains(point))
				{
					this.m_ObjectsInRadius.Add(cellAtPos.m_Objects[i]);
				}
			}
		}
		else
		{
			Vector3 pos2 = pos;
			pos2.x -= radius;
			pos2.z -= radius;
			QuadTreeStaticObjectsCell cellAtPos2 = this.GetCellAtPos(pos2);
			Vector3 pos3 = pos;
			pos3.x += radius;
			pos3.z += radius;
			QuadTreeStaticObjectsCell cellAtPos3 = this.GetCellAtPos(pos3);
			for (int j = cellAtPos2.m_X; j <= cellAtPos3.m_X; j++)
			{
				for (int k = cellAtPos2.m_Y; k <= cellAtPos3.m_Y; k++)
				{
					QuadTreeStaticObjectsCell quadTreeStaticObjectsCell = this.m_Cells[j, k];
					for (int l = 0; l < quadTreeStaticObjectsCell.m_Objects.Count; l++)
					{
						GameObject gameObject2 = quadTreeStaticObjectsCell.m_Objects[l].m_GameObject;
						if (gameObject2 == null)
						{
							Debug.Log("Quad tree GetObjectsInRadius - obj is null : GameObject obj = cell.m_Objects[i];");
						}
						if (gameObject2 && (gameObject2.transform.position - pos).magnitude < radius)
						{
							this.m_ObjectsInRadius.Add(quadTreeStaticObjectsCell.m_Objects[l]);
						}
					}
				}
			}
		}
		return this.m_ObjectsInRadius;
	}

	public void OnObjectMoved(GameObject go)
	{
		QuadTreeStaticObjectsCell quadTreeStaticObjectsCell = null;
		if (!this.m_ObjCellMap.TryGetValue(go, out quadTreeStaticObjectsCell))
		{
			return;
		}
		QuadTreeStaticObjectsCell cellAtPos = this.GetCellAtPos(go.transform.position);
		if (quadTreeStaticObjectsCell != cellAtPos)
		{
			quadTreeStaticObjectsCell.RemoveObject(go);
			this.InsertObject(go, false);
		}
	}

	private QuadTreeStaticObjectsCell FindObjectCell(GameObject go)
	{
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				QuadTreeStaticObjectsCell quadTreeStaticObjectsCell = this.m_Cells[i, j];
				for (int k = 0; k < quadTreeStaticObjectsCell.m_Objects.Count; k++)
				{
					GameObject gameObject = quadTreeStaticObjectsCell.m_Objects[k].m_GameObject;
					if (go == gameObject)
					{
						return quadTreeStaticObjectsCell;
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
				QuadTreeStaticObjectsCell quadTreeStaticObjectsCell = this.m_Cells[i, j];
				if (flag || (flag3 && quadTreeStaticObjectsCell.m_Objects.Count > 0))
				{
					Vector3 zero = Vector3.zero;
					zero.x = quadTreeStaticObjectsCell.m_Pos.x;
					zero.z = quadTreeStaticObjectsCell.m_Pos.y;
					Vector3 end = zero;
					end.x += quadTreeStaticObjectsCell.m_Size.x;
					DebugRender.DrawLine(zero, end, Color.white, 0f);
					end = zero;
					end.z += quadTreeStaticObjectsCell.m_Size.y;
					DebugRender.DrawLine(zero, end, Color.white, 0f);
				}
				if (flag2)
				{
					for (int k = 0; k < quadTreeStaticObjectsCell.m_Objects.Count; k++)
					{
						GameObject gameObject = quadTreeStaticObjectsCell.m_Objects[k].m_GameObject;
						if (gameObject)
						{
							DebugRender.DrawPoint(gameObject.transform.position, (!gameObject.activeSelf) ? Color.green : Color.red, 0.3f, 0f);
						}
					}
				}
			}
		}
	}

	public QuadTreeStaticObjectsCell[,] GetCells()
	{
		return this.m_Cells;
	}

	public int GetNumCellsX()
	{
		return this.m_NumCellsX;
	}

	public int GetNumCellsY()
	{
		return this.m_NumCellsY;
	}

	private QuadTreeStaticObjectsCell[,] m_Cells;

	private Vector2 m_Start = default(Vector2);

	private Vector2 m_Size = default(Vector2);

	private Vector2 m_CellSize = default(Vector2);

	private int m_NumCellsX;

	private int m_NumCellsY;

	private List<StaticObjectClass> m_ObjectsInRadius = new List<StaticObjectClass>();

	private Dictionary<GameObject, QuadTreeStaticObjectsCell> m_ObjCellMap = new Dictionary<GameObject, QuadTreeStaticObjectsCell>();
}
