using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTerrainGrass
{
	[Serializable]
	public class GrassTerrainDefinitions : ScriptableObject
	{
		[Header("Serialized Grass Data")]
		[SerializeField]
		public List<DetailLayerMap> DensityMaps;

		public GrassCell[] Cells;

		public GrassCellContent[] CellContent;

		public int maxBucketDensity;
	}
}
