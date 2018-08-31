using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTerrainGrass
{
	[Serializable]
	public class GrassTerrainDefinitions : ScriptableObject
	{
		[SerializeField]
		[Header("Serialized Grass Data")]
		public List<DetailLayerMap> DensityMaps;

		public GrassCell[] Cells;

		public GrassCellContent[] CellContent;

		public int maxBucketDensity;
	}
}
