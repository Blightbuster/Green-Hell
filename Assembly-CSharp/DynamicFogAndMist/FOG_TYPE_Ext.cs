using System;

namespace DynamicFogAndMist
{
	internal static class FOG_TYPE_Ext
	{
		public static bool isPlus(this FOG_TYPE fogType)
		{
			return fogType == FOG_TYPE.DesktopFogPlusWithSkyHaze || fogType == FOG_TYPE.MobileFogSimple || fogType == FOG_TYPE.MobileFogBasic || fogType == FOG_TYPE.MobileFogOrthogonal || fogType == FOG_TYPE.DesktopFogPlusOrthogonal;
		}
	}
}
