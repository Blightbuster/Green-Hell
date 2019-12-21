using System;
using System.Collections.Generic;

namespace IESLights
{
	public class IESData
	{
		public List<float> VerticalAngles { get; set; }

		public List<float> HorizontalAngles { get; set; }

		public List<List<float>> CandelaValues { get; set; }

		public List<List<float>> NormalizedValues { get; set; }

		public PhotometricType PhotometricType { get; set; }

		public VerticalType VerticalType { get; set; }

		public HorizontalType HorizontalType { get; set; }

		public int PadBeforeAmount { get; set; }

		public int PadAfterAmount { get; set; }

		public float HalfSpotlightFov { get; set; }
	}
}
