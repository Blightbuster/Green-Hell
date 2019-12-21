using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainNeighbours : MonoBehaviour
{
	private void Start()
	{
		this.CreateNeighbours();
	}

	public void CreateNeighbours()
	{
		List<Terrain> list = new List<Terrain>();
		list.AddRange(Terrain.activeTerrains);
		foreach (Terrain item in this.terrainsToOmit)
		{
			if (list.Contains(item))
			{
				list.Remove(item);
			}
		}
		foreach (Terrain item2 in this._terrains)
		{
			if (list.Contains(item2))
			{
				list.Remove(item2);
			}
		}
		if (this._terrainDict == null)
		{
			this._terrainDict = new Dictionary<int[], Terrain>(new IntArrayComparer());
		}
		Dictionary<int[], Terrain> dictionary = new Dictionary<int[], Terrain>(new IntArrayComparer());
		Dictionary<int[], Terrain> dictionary2 = new Dictionary<int[], Terrain>(new IntArrayComparer());
		if (list.Count > 0)
		{
			if (!this.firstPositonSet)
			{
				this.firstPositonSet = true;
				this.firstPosition = new Vector2(list[0].transform.position.x, list[0].transform.position.z);
				this.sizeX = (int)list[0].terrainData.size.x;
				this.sizeZ = (int)list[0].terrainData.size.z;
			}
			foreach (Terrain terrain in list)
			{
				this._terrains.Add(terrain);
				Vector3 vector = terrain.transform.position;
				if (this.worldMover != null)
				{
					vector -= this.worldMover.currentMove;
				}
				int[] key = new int[]
				{
					Mathf.RoundToInt((vector.x - this.firstPosition.x) / (float)this.sizeX),
					Mathf.RoundToInt((vector.z - this.firstPosition.y) / (float)this.sizeZ)
				};
				if (this._terrainDict.ContainsKey(key))
				{
					this._terrainDict[key] = terrain;
				}
				else
				{
					this._terrainDict.Add(key, terrain);
				}
				dictionary.Add(key, terrain);
			}
			foreach (KeyValuePair<int[], Terrain> keyValuePair in dictionary)
			{
				int[] key2 = keyValuePair.Key;
				Terrain terrain2 = null;
				Terrain terrain3 = null;
				Terrain terrain4 = null;
				Terrain terrain5 = null;
				int[] key3 = new int[]
				{
					key2[0],
					key2[1] + 1
				};
				this._terrainDict.TryGetValue(key3, out terrain2);
				int[] key4 = new int[]
				{
					key2[0] - 1,
					key2[1]
				};
				this._terrainDict.TryGetValue(key4, out terrain3);
				int[] key5 = new int[]
				{
					key2[0] + 1,
					key2[1]
				};
				this._terrainDict.TryGetValue(key5, out terrain4);
				int[] key6 = new int[]
				{
					key2[0],
					key2[1] - 1
				};
				this._terrainDict.TryGetValue(key6, out terrain5);
				keyValuePair.Value.SetNeighbors(terrain3, terrain2, terrain4, terrain5);
				keyValuePair.Value.Flush();
				if (terrain2 != null && !dictionary2.ContainsKey(key3))
				{
					dictionary2.Add(key3, terrain2);
				}
				if (terrain3 != null && !dictionary2.ContainsKey(key4))
				{
					dictionary2.Add(key4, terrain3);
				}
				if (terrain4 != null && !dictionary2.ContainsKey(key5))
				{
					dictionary2.Add(key5, terrain4);
				}
				if (terrain5 != null && !dictionary2.ContainsKey(key6))
				{
					dictionary2.Add(key6, terrain5);
				}
			}
			foreach (KeyValuePair<int[], Terrain> keyValuePair2 in dictionary2)
			{
				int[] key7 = keyValuePair2.Key;
				Terrain top = null;
				Terrain left = null;
				Terrain right = null;
				Terrain bottom = null;
				int[] key8 = new int[]
				{
					key7[0],
					key7[1] + 1
				};
				this._terrainDict.TryGetValue(key8, out top);
				int[] key9 = new int[]
				{
					key7[0] - 1,
					key7[1]
				};
				this._terrainDict.TryGetValue(key9, out left);
				int[] key10 = new int[]
				{
					key7[0] + 1,
					key7[1]
				};
				this._terrainDict.TryGetValue(key10, out right);
				int[] key11 = new int[]
				{
					key7[0],
					key7[1] - 1
				};
				this._terrainDict.TryGetValue(key11, out bottom);
				keyValuePair2.Value.SetNeighbors(left, top, right, bottom);
				keyValuePair2.Value.Flush();
			}
		}
	}

	public List<Terrain> terrainsToOmit;

	[Tooltip("If you use Floating Point fix system drag and drop world mover prefab from your scene hierarchy.")]
	public WorldMover worldMover;

	public List<Terrain> _terrains = new List<Terrain>();

	private Dictionary<int[], Terrain> _terrainDict;

	[Tooltip("Debug value, it gives info about starting position offset.")]
	private Vector2 firstPosition;

	private int sizeX;

	private int sizeZ;

	private bool firstPositonSet;
}
