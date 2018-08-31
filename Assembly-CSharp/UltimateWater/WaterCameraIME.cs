using System;
using UnityEngine;

namespace UltimateWater
{
	[ExecuteInEditMode]
	public sealed class WaterCameraIME : MonoBehaviour
	{
		private void Awake()
		{
			this._WaterCamera = base.GetComponent<WaterCamera>();
		}

		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (this._WaterCamera == null)
			{
				Graphics.Blit(source, destination);
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(this);
				}
				return;
			}
			this._WaterCamera.OnRenderImageCallback(source, destination);
		}

		private WaterCamera _WaterCamera;
	}
}
