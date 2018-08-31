using System;
using UnityEngine;

namespace UltimateWater
{
	public interface IRenderModule
	{
		void OnEnable(WaterCamera camera);

		void OnDisable(WaterCamera camera);

		void OnValidate(WaterCamera camera);

		void Process(WaterCamera waterCamera);

		void Render(WaterCamera waterCamera, RenderTexture source, RenderTexture destination);
	}
}
