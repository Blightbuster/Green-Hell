using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGrid
{
	public SimpleGrid(Vector2 start, int width, int height)
	{
		int num = (int)((float)width / this.m_CellSize);
		int num2 = (int)((float)height / this.m_CellSize);
		this.m_Cells = new SimpleGridCell[num, num2];
		this.m_Start.x = start.x;
		this.m_Start.y = start.y;
		this.m_Size.x = (float)width;
		this.m_Size.y = (float)height;
		this.m_NumCellsX = num;
		this.m_NumCellsY = num2;
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				this.m_Cells[i, j] = new SimpleGridCell();
				this.m_Cells[i, j].m_X = i;
				this.m_Cells[i, j].m_Y = j;
				this.m_Cells[i, j].m_Pos.x = this.m_Start.x + (float)i * this.m_CellSize;
				this.m_Cells[i, j].m_Pos.y = this.m_Start.y + (float)j * this.m_CellSize;
				this.m_Cells[i, j].m_Size = this.m_CellSize;
			}
		}
	}

	public void InsertPoint(Vector3 point, bool use_bounds = false)
	{
		this.GetCellAtPos(point).m_Points.Add(point);
	}

	private SimpleGridCell GetCellAtPos(Vector3 pos)
	{
		int num = Mathf.FloorToInt((pos.x - this.m_Start.x) / this.m_Size.x * (float)this.m_NumCellsX);
		int num2 = Mathf.FloorToInt((pos.z - this.m_Start.y) / this.m_Size.y * (float)this.m_NumCellsY);
		if (num < 0)
		{
			num = 0;
		}
		if (num > this.m_NumCellsX - 1)
		{
			num = this.m_NumCellsX - 1;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > this.m_NumCellsY - 1)
		{
			num2 = this.m_NumCellsY - 1;
		}
		return this.m_Cells[num, num2];
	}

	public List<Vector3> GetPointsInGivenRadius(Vector3 pos, float rad)
	{
		this.m_PointsInRadius.Clear();
		Vector3 pos2 = pos;
		pos2.x -= rad;
		pos2.z -= rad;
		SimpleGridCell cellAtPos = this.GetCellAtPos(pos2);
		Vector3 pos3 = pos;
		pos3.x += rad;
		pos3.z += rad;
		SimpleGridCell cellAtPos2 = this.GetCellAtPos(pos3);
		for (int i = cellAtPos.m_X; i <= cellAtPos2.m_X; i++)
		{
			for (int j = cellAtPos.m_Y; j <= cellAtPos2.m_Y; j++)
			{
				SimpleGridCell simpleGridCell = this.m_Cells[i, j];
				for (int k = 0; k < simpleGridCell.m_Points.Count; k++)
				{
					Vector3 vector = simpleGridCell.m_Points[k];
					if ((vector - pos).magnitude < rad)
					{
						this.m_PointsInRadius.Add(vector);
					}
				}
			}
		}
		return this.m_PointsInRadius;
	}

	public bool IsPointInPos(Vector3 pos)
	{
		SimpleGridCell cellAtPos = this.GetCellAtPos(pos);
		for (int i = 0; i < cellAtPos.m_Points.Count; i++)
		{
			if (cellAtPos.m_Points[i] == pos)
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				this.m_Cells[i, j].m_Points.Clear();
			}
		}
	}

	public List<Vector3> GetAllPoints()
	{
		this.m_AllPoints.Clear();
		for (int i = 0; i < this.m_NumCellsX; i++)
		{
			for (int j = 0; j < this.m_NumCellsY; j++)
			{
				this.m_AllPoints.AddRange(this.m_Cells[i, j].m_Points);
			}
		}
		return this.m_AllPoints;
	}

	private float m_CellSize = 10f;

	private SimpleGridCell[,] m_Cells;

	private Vector2 m_Start;

	private Vector2 m_Size;

	private int m_NumCellsX;

	private int m_NumCellsY;

	private List<Vector3> m_PointsInRadius = new List<Vector3>();

	private List<Vector3> m_AllPoints = new List<Vector3>();
}
