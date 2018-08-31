using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTerrainGrass
{
	[Serializable]
	public class GrassCell
	{
		public int state;

		public int index;

		public Vector3 Center;

		public List<int> CellContentIndexes = new List<int>();

		public int CellContentCount;
	}
}
