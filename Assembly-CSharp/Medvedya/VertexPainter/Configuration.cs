using System;
using UnityEngine;

namespace Medvedya.VertexPainter
{
	public class Configuration : ScriptableObject
	{
		public static Configuration configuration
		{
			get
			{
				if (Configuration._configuration == null)
				{
					Configuration._configuration = Resources.Load<Configuration>("VertexPainter/configuration");
				}
				return Configuration._configuration;
			}
		}

		private static Configuration _configuration;

		public Material standartMaterial;

		public bool addStandartMaterialAutomaticly;

		public ColorSet colorSet;
	}
}
