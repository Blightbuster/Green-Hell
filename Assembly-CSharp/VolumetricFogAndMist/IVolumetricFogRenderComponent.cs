using System;

namespace VolumetricFogAndMist
{
	internal interface IVolumetricFogRenderComponent
	{
		VolumetricFog fog { get; set; }

		void DestroySelf();
	}
}
