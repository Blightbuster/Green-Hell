using System;
using UnityEngine;

namespace UltimateWater
{
	[Serializable]
	public class WaterProfile : ScriptableObject
	{
		public WaterProfileData Data
		{
			get
			{
				return this._Data;
			}
		}

		[SerializeField]
		private WaterProfileData _Data = new WaterProfileData();
	}
}
